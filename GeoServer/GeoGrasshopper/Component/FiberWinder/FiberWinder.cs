using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GeoGrasshopper.FiberWinding
{
    public partial class FiberWinder : GH_Component
    {
        private NurbsCurve previous, next, arrowLine;
        private Point3d textPosActual, startPoint, endPoint;
        private string textActual;
        /// <summary>
        /// How many Planes should be in a bending radius?
        /// </summary>
        private const int arcP = 6;
        private const int drawingThickness = 3;

        private List<NurbsCurve> geometry;
        private List<int> matId;
        private List<double> crvWidth, crvDiv;

        private System.Drawing.Color nextColor, previousColor;

        private List<Plane> weavingPlanes;
        private int markerSize, idx;
        private double pinSize, fiberMulti, bendingMulti, bendingDistance, toolRotation;
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
            pManager.AddIntegerParameter("MarkerSize", "Ms", "Size of Marker", GH_ParamAccess.item, 40);
            pManager.AddIntegerParameter("AlignAxis", "AX", "AlignAxis 0: None, 1:X, 2:Y", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("ToolRotation", "TR", "In Degree", GH_ParamAccess.item, 0.00);


            for (int i = 3; i <= 10; i++)
                pManager[i].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
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
            DA.GetData(8, ref markerSize);
            DA.GetData(9, ref _alignAxis);
            DA.GetData(10, ref toolRotation);

            //########INPUT#########

            int ptCount = weavingPlanes.Count;
            if (ptCount < 1)
                return;

            //Gets radius
            double radius = (pinSize <= 0) ? 0.01 : pinSize / 2.00;

            double[] arcT = GetArcParams();

            Point3d lastPoint = startPoint;

            List<Plane> previewPlanes = new List<Plane>();

            for (int i = 0; i < ptCount; i++)
            {
                Point3d prevPt = (i > 1 && i < ptCount) ? weavingPlanes[i - 1].Origin : startPoint;
                Point3d actualPt = weavingPlanes[i].Origin;
                Point3d nextPt = (i < ptCount - 1) ? weavingPlanes[i + 1].Origin : endPoint;

                Plane pCross = new Plane(actualPt, actualPt - prevPt, actualPt - nextPt);

                Vector3d norm = weavingPlanes[i].Normal;
                Vector3d bendingVec = pCross.ZAxis;

                GetAndMoveDoublePins(norm, i, ref actualPt);

                bool isFlipped = CheckIfFlipped(pCross, ref norm, ref bendingVec);

                if (i >= ptCount)
                    return;

                NurbsCurve arc = DrawArcCurve(nextPt, actualPt, prevPt, norm, radius);
                Plane[] arcFrames = arc.GetPerpendicularFrames(arcT);

                CheckIfFramesHasToBeFlipped(isFlipped, arcFrames);

                RotatePlane90Degree(arcFrames);

                // StartPlane
                if (i == 0)
                    SetStartPlane(previewPlanes, arcFrames);

                Point3d pt1 = lastPoint;
                lastPoint = arc.Points[arc.Points.Count - 1].Location;

                if (i > 0)
                    SetMiddlePlanes(previewPlanes, arc, arcFrames, pt1, bendingVec);

                SetArcPlanes(previewPlanes, arcFrames);

                if (i == ptCount - 1)
                    SetEndPlane(previewPlanes, arcFrames);
            }

            SetAlignAxis(_alignAxis);
            AlignPlanes(alignAxis, previewPlanes);

            int actualPlaneIdx = GetActualPlaneIdx(previewPlanes.Count);

            Polyline pLine = ContructPrevLine(previewPlanes, actualPlaneIdx, ptCount);
            Polyline nLine = ContructNextLine(previewPlanes, actualPlaneIdx, ptCount);

            ConstructArrow(nLine, ptCount);
            ConstructText(nLine);

            StreamSettings streamSet = GetStreamSettings();

            //#########OUTPUT#########

            List<NurbsCurve> lines = new List<NurbsCurve>()
            { pLine.ToNurbsCurve(), nLine.ToNurbsCurve(), arrowLine };

            DA.SetDataList(0, lines);
            DA.SetData(1, streamSet);
            DA.SetDataList(2, previewPlanes);
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