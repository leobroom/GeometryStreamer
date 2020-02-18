using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;

namespace GeoGrasshopper.FiberWinding
{
    public class FiberWinder : GH_Component
    {
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
            pManager.AddNumberParameter("pinSize", "Ps", "Size of Marker", GH_ParamAccess.item, 1.00);
            pManager.AddNumberParameter("FiberMulti", "Fm", "", GH_ParamAccess.item, 0);

            pManager.AddNumberParameter("BendingMulti", "Bm", "Between 0 and 1", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("BendingDistance", "Bd", "Between 0 and 1", GH_ParamAccess.item, 0);

            pManager.AddIntegerParameter("MarkerSize", "Ms", "Size of Marker", GH_ParamAccess.item, 40);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
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
            //########INPUT#########

            List<Plane> weavingPlanes = new List<Plane>();
            int markerSize = 0, idx = 0;
            double pinSize = 0.05, fiberMulti = 0, bendingMulti = 0, bendingDistance = 0;
            Point3d startPoint = Point3d.Origin, endPoint = Point3d.Origin;

            if (!(DA.GetDataList(0, weavingPlanes) &&
                DA.GetData(1, ref startPoint) &&
                DA.GetData(2, ref endPoint)))
                return;

            DA.GetData(3, ref idx);
            DA.GetData(4, ref pinSize);
            DA.GetData(5, ref fiberMulti);
            DA.GetData(6, ref bendingMulti);
            DA.GetData(7, ref bendingDistance);
            DA.GetData(8, ref markerSize);

            //########INPUT#########

            geometry = new List<NurbsCurve>();
            matId = new List<int>();
            crvWidth = new List<double>();
            crvDiv = new List<double>();

            previousColor = System.Drawing.Color.Gray;
            nextColor = System.Drawing.Color.DarkCyan;

            previous = null;
            next = null;
            arrowLine = null;
            textActual = "";

            int ptCount = weavingPlanes.Count;
            if (ptCount < 1)
                return;

            //Gets radius
            double radius = (pinSize <= 0) ? 0.01 : pinSize / 2.00;
            int arcP = 6;
            double[] arcT = Utils.GetArcParams(arcP);

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


                Utils.FindAndMoveDoublePins(weavingPlanes, norm, i, fiberMulti, ref actualPt);

                bool isFlipped = Utils.CheckIfFlipped(pCross, ref norm, ref bendingVec);

                if (i >= ptCount)
                    return;

                NurbsCurve arc = Utils.DrawArcCurve(pCross, nextPt, actualPt, prevPt, norm, radius);
                Plane[] frames = arc.GetPerpendicularFrames(arcT);

                Utils.CheckIfFramesHasToBeFlipped(isFlipped, frames);

                // StartPlane
                if (i == 0)
                {
                    Plane firstPlane = frames[0];
                    previewPlanes.Add(new Plane(startPoint, firstPlane.XAxis, firstPlane.YAxis));
                }

                Point3d pt1 = lastPoint;
                lastPoint = arc.Points[arc.Points.Count - 1].Location;

                // Hochgesetzer Mittelpunkt
                if (i > 0)
                {
                    Plane pStart = previewPlanes[previewPlanes.Count - 1];
                    Plane pEnd = frames[0];
                    var pt2 = arc.Points[0].Location;
                    Plane normPlane1 = Utils.GetBendingPlane(pStart, pEnd, pt1, pt2, bendingVec, bendingMulti, bendingDistance / 2);
                    Plane normPlane2 = Utils.GetBendingPlane(pStart, pEnd, pt1, pt2, bendingVec, bendingMulti, 1 - bendingDistance / 2);
                    previewPlanes.Add(normPlane1);
                    previewPlanes.Add(normPlane2);
                }

                previewPlanes.AddRange(frames); ;

                // Endpoint
                if (i == ptCount - 1)
                {
                    Plane lastPlane = frames[frames.Length - 1];
                    previewPlanes.Add(new Plane(endPoint, lastPlane.XAxis, lastPlane.YAxis));
                }
            }

            int actualPlaneIdx = idx * (arcP + 2) + 2;
            actualPlaneIdx = (actualPlaneIdx > previewPlanes.Count) ? previewPlanes.Count : actualPlaneIdx;

            int prevIdx = (idx < ptCount) ? actualPlaneIdx - arcP + 4 : actualPlaneIdx;

            Polyline pline;

            pline = new Polyline(arcP + 2);
            for (int i = 0; i < prevIdx; i++)
            {
                pline.Add(previewPlanes[i].Origin);
            }
            previous = pline.ToNurbsCurve();


            pline = new Polyline();
            int nextIdx = actualPlaneIdx - arcP + 3;
            if (nextIdx < 0)
                nextIdx = 0;

            if (idx < ptCount)
            {
                for (int i = nextIdx; i < actualPlaneIdx + arcP; i++)
                {
                    pline.Add(previewPlanes[i].Origin);
                }

                next = pline.ToNurbsCurve();
            }


            int plCpCount = pline.Count;

            //ArrowLine
            if (idx <= ptCount && plCpCount > 2)
            {
                Line arrowLn = new Line(pline[plCpCount - 2], pline[plCpCount - 1]);

                if (markerSize <= 0)
                    markerSize = 1;

                arrowLine = DrawArrowTip(arrowLn.ToNurbsCurve(), 1, markerSize);
            }
            else
                arrowLine = null;

            //TextPositions
            Vector3d moveTxt = new Vector3d(23, 0, 6);

            if (pline.Count > 0)
            {
                textPosActual = pline[pline.Count - 3] + moveTxt;
                textActual = idx.ToString();
            }

            GeoGrasshopper.StreamSettings streamSet = new StreamSettings(System.Drawing.Color.Gray);

            //############StreamSettings################

            //#####MAT#####
            DisplayMaterial matPrev = new DisplayMaterial { Diffuse = previousColor };
            DisplayMaterial matNext = new DisplayMaterial { Diffuse = nextColor };

            streamSet.Materials = new List<DisplayMaterial>() { matPrev, matNext };

            //#####Geometry####

            double curveSmall = 0.001;
            double curveBig = 0.002;
            double curveUltraBig = 0.003;

            int prevColorId = 0;
            int nextColorId = 1;

            int crvD = 8;

            //#####MATID#####

            if (previous != null)
                SetCrvSetting(previous.ToNurbsCurve(), prevColorId, curveSmall, crvD);
            SetCrvSetting(next, nextColorId, curveBig, crvD);
            SetCrvSetting(arrowLine, nextColorId, curveBig, crvD);

            streamSet.CurveDivisions = crvDiv;
            streamSet.CurveWidths = crvWidth;
            streamSet.ObjMatIds = matId;

            //#########OUTPUT#########

            DA.SetDataList(2, previewPlanes);
            DA.SetDataList(0, pline);
            DA.SetData(1, streamSet);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.gs_fiber;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("233e96be-42f2-4556-be53-c959be6cca96");

        private NurbsCurve previous;          //0
        private NurbsCurve next;            //1
        private NurbsCurve arrowLine;       //5
        private Point3d textPosActual;        //6
        private string textActual;            //9

        List<NurbsCurve> geometry = new List<NurbsCurve>();
        List<int> matId = new List<int>();
        List<double> crvWidth = new List<double>();
        List<double> crvDiv = new List<double>();

        System.Drawing.Color nextColor;
        System.Drawing.Color previousColor;

        public void SetCrvSetting(NurbsCurve crv, int mat, double crvWdth, int crvD)
        {
            if (crv == null)
                return;

            geometry.Add(crv);
            matId.Add(mat);
            crvWidth.Add(crvWdth);
            crvDiv.Add(crvD);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (previous != null)
                args.Display.DrawCurve(previous, previousColor, 5);
            if (next != null)
                args.Display.DrawCurve(next, nextColor, 5);

            //Arrow
            if (arrowLine != null)
                args.Display.DrawCurve(arrowLine, nextColor, 5);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (textActual != "")
                DrawText(textActual, textPosActual, nextColor, args);
        }

        public void DrawText(string txt, Point3d pos, System.Drawing.Color c, IGH_PreviewArgs args)
        {
            var drawText = new Rhino.Display.Text3d(txt, new Plane(pos, Vector3d.ZAxis), 40);
            args.Display.Draw3dText(drawText, c);
            drawText.Dispose();
        }

        public NurbsCurve DrawArrowTip(Curve crv, double param, double size)
        {
            Plane p;
            crv.PerpendicularFrameAt(param, out p);

            Plane p2 = new Plane(p.Origin, p.XAxis, p.ZAxis);

            double dir = (param < 0.5) ? 0.5 : -0.5;
            p2.Rotate(Math.PI * dir, p.YAxis);

            Polyline arrow = Polyline.CreateInscribedPolygon(new Circle(p2, size), 3);

            return arrow.ToNurbsCurve();
        }
    }
}