using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GeoGrasshopper
{
    public partial class FiberWinder : GH_Component
    {
        private NurbsCurve previous, next;
        private GH_StreamText streamText;
        private Mesh arrowLine;
        private Mesh pinMarkers;
        private Point3d textPosActual, startPoint, endPoint;
        private string textActual;
        /// <summary>
        /// How many Planes should be in a bending radius?
        /// </summary>
        private const int arcP = 6;
        private const int drawingThickness = 3;

        private List<object> geometry;
        private List<int> matId;
        private List<double> crvWidth, crvDiv;

        private System.Drawing.Color nextColor, previousColor, markerColor;

        private List<Plane> weavingPlanes;
        private int markerSize, idx;
        private double pinSize, fiberMulti, bendingMulti, bendingDistance, toolRotation;
        private List<Vector3d> bendingVectors;
        private AlignAxis alignAxis;

        /// <summary>
        /// Initializes a new instance of the FiberwindingComponent class.
        /// </summary>
        public FiberWinder()
          : base("Fiber Winder", "FWinder", "Tool to Help with Fiber winding", "ITE", "Tools") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("WeavingPlanes", "P", "Planes - normals are for Pindirection", GH_ParamAccess.list);
            pManager.AddPointParameter("StartPoint", "Sp", "StartPoint", GH_ParamAccess.item);
            pManager.AddPointParameter("EndPoint", "Ep", "EndPoint", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Index", "Idx", "Index of Assembly Step", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("PinSize", "Ps", "Size of Marker", GH_ParamAccess.item, 1.00);
            pManager.AddNumberParameter("FiberMulti", "Fm", "", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("BendingMulti", "Bm", "Between 0 and 1", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("BendingDistance", "Bd", "Between 0 and 1", GH_ParamAccess.item, 0);
            pManager.AddVectorParameter("BendingVectors", "Bv", "WeavingPlanes Count -1 * 2 | If nor set, some vector parameters gets created", GH_ParamAccess.list);
            pManager.AddIntegerParameter("MarkerSize", "Ms", "Size of Marker", GH_ParamAccess.item, 40);
            pManager.AddIntegerParameter("AlignAxis", "AX", "AlignAxis 0: None, 1:X, 2:Y", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("ToolRotation", "TR", "In Degree", GH_ParamAccess.item, 0.00);

            for (int i = 3; i <= 11; i++)
                pManager[i].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FiberWinding", "FW", "FiberWinding", GH_ParamAccess.list);
            pManager.AddGenericParameter("StreamGeometry", "Geo", "Geometry which can be streamed", GH_ParamAccess.list);
            pManager.AddGenericParameter("StreamSettings", "Set", "StreamSettings", GH_ParamAccess.item);
            pManager.AddGenericParameter("RobotPlanes", "P", "Planes for RobotControl", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            SetDefaultValues();
            //########INPUT#########

            if (!(DA.GetDataList(0, weavingPlanes) &&
                DA.GetData(1, ref startPoint) &&
                DA.GetData(2, ref endPoint)))
                return;

            int _alignAxis = 0;

            DA.GetData(3, ref idx);
            DA.GetData(4, ref pinSize);
            DA.GetData(5, ref fiberMulti);
            DA.GetData(6, ref bendingMulti);
            DA.GetData(7, ref bendingDistance);
            DA.GetDataList(8, bendingVectors);
            DA.GetData(9, ref markerSize);
            DA.GetData(10, ref _alignAxis);
            DA.GetData(11, ref toolRotation);

            //########INPUT#########

            int ptCount = weavingPlanes.Count;
            if (ptCount < 1)
                return;

            //Gets radius
            double radius = (pinSize <= 0) ? 0.01 : pinSize / 2.00;

            double[] arcT = GetArcParams();

            Point3d lastPoint = startPoint;

            List<Plane> previewPlanes = new List<Plane>();
            //  List<Plane> bendingPlanes = new List<Plane>();
            int bendingVCount = bendingVectors.Count;

            Plane start = Plane.WorldXY, end = Plane.WorldXY;

            GH_Structure<GH_Plane> bendingPlaneTree =
                new GH_Structure<GH_Plane>(), arcPlaneTree = new GH_Structure<GH_Plane>();
            int pathIdx = 0;
            for (int i = 0; i < ptCount; i++)
            {
                Point3d prevPt = (i > 1 && i < ptCount) ? weavingPlanes[i - 1].Origin : startPoint;
                Point3d actualPt = weavingPlanes[i].Origin;
                Point3d nextPt = (i < ptCount - 1) ? weavingPlanes[i + 1].Origin : endPoint;

                Plane pCross = new Plane(actualPt, actualPt - prevPt, actualPt - nextPt);

                Vector3d norm = weavingPlanes[i].Normal;

                GetAndMoveDoublePins(norm, i, ref actualPt);

                Vector3d[] bendingVecs = new Vector3d[] { pCross.ZAxis, pCross.ZAxis };

                int bendIdx = (i - 1) * 2;

                bool hasNewBendingVecs = false;

                if (i > 0 && bendingVCount > bendIdx)
                {
                    bendingVecs[0] = bendingVectors[bendIdx];
                    bendingVecs[1] = bendingVectors[bendIdx + 1];
                    hasNewBendingVecs = true;
                }

                bool isFlipped = CheckIfFlipped(pCross, ref norm, ref bendingVecs, hasNewBendingVecs);

                if (i >= ptCount)
                    return;

                NurbsCurve arc = DrawArcCurve(nextPt, actualPt, prevPt, norm, radius);

                Plane[] arcFrames = arc.GetPerpendicularFrames(arcT);

                CheckIfFramesHasToBeFlipped(isFlipped, arcFrames);

                RotatePlane90Degree(arcFrames);

                // StartPlane
                if (i == 0)
                    start = SetStartPlane(previewPlanes, arcFrames);

                Point3d pt1 = lastPoint;
                lastPoint = arc.Points[arc.Points.Count - 1].Location;

                if (i > 0)
                {
                    List<GH_Plane> bendingPlanes = new List<GH_Plane>();
                    SetMiddlePlanes(previewPlanes, bendingPlanes, arc, arcFrames, pt1, bendingVecs);

                    bendingPlaneTree.AppendRange(bendingPlanes, new GH_Path(pathIdx));
                    pathIdx++;       
                }

                for (int z = 0; z < arcFrames.Length; z++)
                    arcPlaneTree.Append(new GH_Plane(arcFrames[z]), new GH_Path(i));

                SetArcPlanes(previewPlanes, arcFrames);

                if (i == ptCount - 1)
                {
                    end = SetEndPlane(previewPlanes, arcFrames);
                }
            }

            SetAlignAxis(_alignAxis);
            AlignPlanes(alignAxis, previewPlanes);

            AlignPlane(alignAxis, ref start);

            //Dirty Hack LB - muss verbessert werden

            foreach (var item in bendingPlaneTree.Branches)
                AlignPlanes(alignAxis, item);

            foreach (var item in arcPlaneTree.Branches)
                AlignPlanes(alignAxis, item);

            AlignPlane(alignAxis, ref end);


            int actualPlaneIdx = GetActualPlaneIdx(previewPlanes.Count);

            Polyline pLine = ContructPrevLine(previewPlanes, actualPlaneIdx, ptCount);
            Polyline nLine = ContructNextLine(previewPlanes, actualPlaneIdx, ptCount);

            ConstructArrow(nLine, ptCount);
            ConstructMarkers();
            streamText = ConstructText(nLine);

            StreamSettings streamSet = GetStreamSettings();

            //#########OUTPUT#########

            List<object> streaminggeometry = new List<object>()
            { pLine.ToNurbsCurve(), nLine.ToNurbsCurve(), arrowLine, pinMarkers};

            if (streamText != null)
                streaminggeometry.Add(streamText);

            //FiberWinding
            FiberWinding winding =
                new FiberWinding(weavingPlanes, arcPlaneTree, bendingPlaneTree, start, end);

            DA.SetData(0, winding);
            DA.SetDataList(1, streaminggeometry);
            DA.SetData(2, streamSet);
            DA.SetDataList(3, previewPlanes);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.gs_fiber;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("233e96be-42f2-4556-be53-c959be6cca96");
    }
}