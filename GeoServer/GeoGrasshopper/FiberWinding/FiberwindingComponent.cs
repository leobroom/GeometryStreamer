using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;

namespace GeoGrasshopper.FiberWinding
{
    public class FiberwindingComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FiberwindingComponent class.
        /// </summary>
        public FiberwindingComponent()
          : base("FiberwindingComponent", "FiberWind", "Tool to Help with Fiberwinding", "ITE", "Network")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("WeavingPoints", "Pts", "Points to weave", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Index", "Idx", "Index of Assembly Step", GH_ParamAccess.item);
            pManager.AddIntegerParameter("MarkerSize", "MSz", "Size of Marker", GH_ParamAccess.item, 40);
            pManager.AddIntegerParameter("ArrowSize", "ASz", "Size of Arrow", GH_ParamAccess.item, 10);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("StreamGeometry", "Geo", "Geometry which can be streamed", GH_ParamAccess.list);
            pManager.AddGenericParameter("StreamSettings", "Set", "StreamSettings", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //########INPUT#########

            List<Point3d> weavingPoints = new List<Point3d>();
            if (!DA.GetDataList<Point3d>(0, weavingPoints))
                return;

            int markerSize = 0, arrowSize = 0, idx = 0;

            DA.GetData(1, ref idx);
            DA.GetData(2, ref markerSize);
            DA.GetData(3, ref arrowSize);

            //########INPUT#########


            geometry = new List<NurbsCurve>();
            matId = new List<int>();
            crvWidth = new List<double>();
            crvDiv = new List<double>();

            if (arrowSize == 0)
                arrowSize = 8;

            int Index = idx;

            previousColor = System.Drawing.Color.Gray;
            nextColor = System.Drawing.Color.DarkCyan;

            //ResetValues
            if (Index < 0)
            {
                nextMarker = null;
                previousMarker = null;
                previous = null;
                next = null;
                arrowLine = null;
                textPrevious = "";
                textNext = "";
                directionMarker = null;

                return;
            }

            int weavingCount = weavingPoints.Count;
            Index = (Index >= weavingCount) ? weavingCount - 1 : Index;

            //Get Point Range of WeavingPoints
            List<Point3d> ptList = weavingPoints.GetRange(0, Index + 1);

            int ptCount = ptList.Count;

            List<Point3d> previousPts;

            List<string> simPts = new List<string>();
            List<NurbsCurve> simCurves = new List<NurbsCurve>();

            //Previous Weave
            if (Index > 1)
            {
                previousPts = (weavingCount == Index + 1) ? ptList : ptList.GetRange(0, ptCount - 1);
                previous = new Polyline(previousPts);

                //----------CORNER COLLECT TEST!!!!!!!!!!!!!!!!
                CollectCorners(previousPts, markerSize / 2, out simPts, simCurves);
                //----------CORNER COLLECT TEST!!!!!!!!!!!!!!!!
            }
            else
                previous = null;

            //Marker
            Point3d actualPt = weavingPoints[Index];

            var radius = markerSize / 2.00;
            var thickness = radius * 0.15;

            Point3d nextPt;
            if (Index < weavingCount - 1)
                nextPt = weavingPoints[Index + 1];
            else
                nextPt = weavingPoints[Index] + new Point3d(1, 1, 0);

            Point3d prevPt;
            if (ptCount > 1 && idx < ptCount)
                prevPt = weavingPoints[idx - 1];
            else
                prevPt = weavingPoints[Index] + new Point3d(-1, -1, 0);

            Plane pCross = new Plane(actualPt, actualPt - prevPt, actualPt - nextPt);

            if (ptCount > 1 && idx < ptCount)
            {
                if (ptCount > 2)
                {
                    Point3d oldPrevPt = weavingPoints[idx - 2];
                    Plane pCrossPrev = new Plane(prevPt, prevPt - oldPrevPt, prevPt - actualPt);
                    previousMarker = new Circle(pCrossPrev, radius).ToNurbsCurve();
                }
                else
                    previousMarker = new Circle(prevPt, radius).ToNurbsCurve();
            }
            else
            {
                previousMarker = null;
            }

            if (idx < ptCount)
            {
                Circle nMCircle = new Circle(pCross, radius);
                nextMarker = nMCircle.ToNurbsCurve();

                var planeRev = nMCircle.Plane;
                var trans = Rhino.Geometry.Transform.PlaneToPlane(planeRev, Plane.WorldXY);

                nMCircle.Transform(trans);

                Point3d ptA = GetTangentialPoint(trans, prevPt, nMCircle, pCross, false);
                Point3d ptB = GetTangentialPoint(trans, nextPt, nMCircle, pCross, true);

                Transform transRev = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, planeRev);

                var arc = DrawArcCurve(ptA, ptB, nMCircle);
                if (arc != null)
                    arc.Transform(transRev);
            }
            else
            {
                nextMarker = null;
            }

            //Direction Marker
            if (idx > 0 && Index < weavingCount - 1)
            {
                directionMarker = new Rhino.Geometry.Arc(pCross, markerSize / 2 + 5, Math.PI / 2).ToNurbsCurve();
            }
            else
            {
                directionMarker = null;
            }

            //Next Weave
            if (weavingCount != Index + 1)
            {
                List<Point3d> nextPts = new List<Point3d>();

                if (Index > 0)
                    nextPts.Add(weavingPoints[Index - 1]);

                var ptB = weavingPoints[Index];
                nextPts.Add(ptB);

                if (weavingCount != Index + 1)
                {
                    var ptC = weavingPoints[Index + 1];
                    Vector3d dir = ptC - ptB;
                    ptC = ptB + (dir * 0.25);
                    nextPts.Add(ptC);
                }

                var nextPL = new Polyline(nextPts);
                next = nextPL.ToNurbsCurve();
                next.Domain = new Interval(0, 1);
            }
            else
                next = null;

            //ArrowLine
            if (weavingCount != Index + 1)
                arrowLine = DrawArrowTip(next.ToNurbsCurve(), 1, arrowSize);
            else
                arrowLine = null;

            //TextPositions
            Vector3d moveTxt = new Vector3d(23, 0, 6);

            if (previousMarker != null)
            {
                textPosPrevious = ptList[ptCount - 2] + moveTxt;
                textPrevious = (Index - 1).ToString();
            }
            else
                textPrevious = "";

            if (nextMarker != null)
            {
                textPosNext = ptList[ptCount - 1] + moveTxt;
                textNext = Index.ToString();
            }
            else
                textNext = "";

            GeoGrasshopper.StreamSettings streamSet = new StreamSettings(System.Drawing.Color.Gray);

            //############StreamSettings################

            //#####MAT#####
            DisplayMaterial matPrev = new DisplayMaterial();
            matPrev.Diffuse = previousColor;

            DisplayMaterial matNext = new DisplayMaterial();
            matNext.Diffuse = nextColor;

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
            SetCrvSetting(previousMarker, prevColorId, curveBig, crvD);
            SetCrvSetting(next, nextColorId, curveBig, crvD);
            SetCrvSetting(arrowLine, nextColorId, curveBig, crvD);
            SetCrvSetting(nextMarker, nextColorId, curveBig, crvD);
            SetCrvSetting(directionMarker, nextColorId, curveUltraBig, crvD);

            streamSet.CurveDivisions = crvDiv;
            streamSet.CurveWidths = crvWidth;
            streamSet.ObjMatIds = matId;

            //#########OUTPUT#########

            DA.SetDataList(0, geometry);
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

        private Polyline previous;          //0
        private NurbsCurve next;            //1
        private NurbsCurve nextMarker;      //2
        private NurbsCurve previousMarker;  //3
        private NurbsCurve directionMarker; //4
        private NurbsCurve arrowLine;       //5
        private Point3d textPosNext;        //6
        private Point3d textPosPrevious;    //7
        private string textPrevious;        //8
        private string textNext;            //9

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

        public void CollectCorners(List<Point3d> pts, double radius, out List<string> cutPts, List<NurbsCurve> simCurves)
        {
            int ptCount = pts.Count;
            cutPts = new List<string>();

            for (int i = 0; i < pts.Count; i++)
            {
                Point3d actualPt = pts[i];
                Point3d prevPt;
                if (i > 1 && i < ptCount)
                    prevPt = pts[i - 1];
                else
                    prevPt = pts[i] + new Point3d(-1, -1, 0);

                Point3d nextPt;
                if (i < ptCount - 1)
                    nextPt = pts[i + 1];
                else
                    nextPt = pts[i] + new Point3d(1, 1, 0);

                Plane pCross = new Plane(actualPt, actualPt - prevPt, actualPt - nextPt);

                if (i < ptCount)
                {
                    Circle nMCircle = new Circle(pCross, radius);

                    var planeRev = nMCircle.Plane;
                    var trans = Rhino.Geometry.Transform.PlaneToPlane(planeRev, Plane.WorldXY);

                    nMCircle.Transform(trans);

                    Point3d ptA = GetTangentialPoint(trans, prevPt, nMCircle, pCross, false);
                    Point3d ptB = GetTangentialPoint(trans, nextPt, nMCircle, pCross, true);

                    Transform transRev = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, planeRev);

                    var angle = Vector3d.CrossProduct(actualPt - prevPt, actualPt - nextPt);

                    NurbsCurve arc = DrawArcCurve(ptA, ptB, nMCircle);

                    cutPts.Add(angle.ToString());

                    if (arc != null)
                        arc.Transform(transRev);
                }
            }
        }

        public NurbsCurve DrawArcCurve(Point3d a, Point3d b, Circle circle)
        {
            double param;
            circle.ClosestParameter(b, out param);
            var tan = circle.TangentAt(param);

            return (new Arc(b, tan, a)).ToNurbsCurve();
        }

        public Point3d GetTangentialPoint(Transform trans, Point3d next, Circle c, Plane plane, bool reverse)
        {
            next.Transform(trans);

            Point3d pt1;
            Point3d pt2;

            bool success = FindTangents(c.Plane, c.Center, c.Radius, next, out pt1, out pt2);

            if (success)
                return (reverse) ? pt2 : pt1;

            return new Point3d();
        }

        //  public override void DrawViewportMeshes(IGH_PreviewArgs args)
        //  {
        //    if(previous != null)
        //      args.Display.DrawCurve(previous.ToPolylineCurve(), previousColor, 5);
        //    if(next != null)
        //      args.Display.DrawCurve(next, nextColor, 5);
        //
        //    //Marker
        //    if(nextMarker != null)
        //      args.Display.DrawCurve(nextMarker, nextColor, 5);
        //    if(previousMarker != null)
        //      args.Display.DrawCurve(previousMarker, previousColor, 5);
        //    if(directionMarker != null)
        //      args.Display.DrawCurve(directionMarker, nextColor, 7);
        //
        //    //Arrow
        //    if(arrowLine != null)
        //      args.Display.DrawCurve(arrowLine, nextColor, 5);
        //  }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (textNext != "")
                DrawText(textNext, textPosNext, nextColor, args);

            if (textPrevious != "")
                DrawText(textPrevious, textPosPrevious, previousColor, args);
        }

        public void DrawText(string txt, Point3d pos, System.Drawing.Color c, IGH_PreviewArgs args)
        {
            var drawText = new Rhino.Display.Text3d(txt, new Plane(pos, Vector3d.ZAxis), 20);
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

        // <Custom additional code>
        // Find the tangent points for this circle and external point.
        // Return true if we find the tangents, false if the point is
        // inside the circle.
        private bool FindTangents(Plane plane, Point3d center, double radius,
          Point3d external_point, out Point3d pt1, out Point3d pt2)
        {
            // Find the distance squared from the
            // external point to the circle's center.
            double dx = center.X - external_point.X;
            double dy = center.Y - external_point.Y;

            double D_squared = dx * dx + dy * dy;

            if (D_squared < radius * radius)
            {
                pt1 = new Point3d(-1, -1, 0);
                pt2 = new Point3d(-1, -1, 0);
                return false;
            }

            // Find the distance from the external point
            // to the tangent points.
            double L = Math.Sqrt(D_squared - radius * radius);

            // Find the points of intersection between
            // the original circle and the circle with
            // center external_point and radius dist.
            FindCircleCircleIntersections(
              center.X, center.Y, radius,
              external_point.X, external_point.Y, L,
              out pt1, out pt2);

            //    pt1.Z = center.Z;
            //       pt2.Z = center.Z;

            return true;
        }

        // Find the points where the two circles intersect.
        private int FindCircleCircleIntersections(
          double cx0, double cy0, double radius0,
          double cx1, double cy1, double radius1,
          out Point3d intersection1, out Point3d intersection2)
        {
            // Find the distance between the centers.
            double dx = cx0 - cx1;
            double dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                intersection1 = new Point3d(0, 0, 0);
                intersection2 = new Point3d(0, 0, 0);
                return 0;
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                intersection1 = new Point3d(0, 0, 0);
                intersection2 = new Point3d(0, 0, 0);
                return 0;
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                intersection1 = new Point3d(0, 0, 0);
                intersection2 = new Point3d(0, 0, 0);
                return 0;
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 -
                  radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                // Get the points P3.
                intersection1 = new Point3d(
                  (double)(cx2 + h * (cy1 - cy0) / dist),
                  (double)(cy2 - h * (cx1 - cx0) / dist), 0);
                intersection2 = new Point3d(
                  (double)(cx2 - h * (cy1 - cy0) / dist),
                  (double)(cy2 + h * (cx1 - cx0) / dist), 0);

                // See if we have 1 or 2 solutions.
                if (dist == radius0 + radius1) return 1;
                return 2;
            }
        }

    }
}