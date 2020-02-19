using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace GeoGrasshopper.FiberWinding
{
    public partial class FiberWinder : GH_Component
    {
        private void ConstructArrow(Polyline pline, int ptCount)
        {
            int plCpCount = pline.Count;

            //ArrowLine
            if (idx <= ptCount && plCpCount > 2)
            {
                Line arrowLn = new Line(pline[plCpCount - 2], pline[plCpCount - 1]);

                if (markerSize <= 0)
                    markerSize = 1;

                arrowLine = DrawArrowTip(arrowLn.ToNurbsCurve(), 1);
            }
            else
                arrowLine = null;
        }

        private void ConstructText(Polyline pline)
        {
            //TextPositions
            Vector3d moveTxt = new Vector3d(23, 0, 6);

            if (pline.Count > 0)
            {
                textPosActual = pline[pline.Count - 3] + moveTxt;
                textActual = idx.ToString();
            }
        }

        private Polyline ContructPrevLine( List<Plane> previewPlanes, int actualPlaneIdx, int ptCount)
        {
            int prevIdx = (idx < ptCount) ? actualPlaneIdx - arcP + 4 : actualPlaneIdx;

            Polyline pline = new Polyline(arcP + 2);

            for (int i = 0; i < prevIdx; i++)
            {
                pline.Add(previewPlanes[i].Origin);
            }
            previous = pline.ToNurbsCurve();

            return pline;
        }

        private Polyline ContructNextLine( List<Plane> previewPlanes, int actualPlaneIdx, int ptCount)
        {
            Polyline nLine = new Polyline();
            int nextIdx = actualPlaneIdx - arcP + 3;
            if (nextIdx < 0)
                nextIdx = 0;

            if (idx < ptCount)
            {
                for (int i = nextIdx; i < actualPlaneIdx + arcP; i++)
                    nLine.Add(previewPlanes[i].Origin);

                next = nLine.ToNurbsCurve();
            }

            return nLine;
        }

        public NurbsCurve DrawArcCurve
   ( Point3d nextPt, Point3d actualPt, Point3d prevPt, Vector3d norm, double radius)
        {
            Plane normalPlane = new Plane(actualPt, norm);
            Circle nMCircle = new Circle(normalPlane, radius);

            Plane planeRev = nMCircle.Plane;
            Transform trans = Transform.PlaneToPlane(planeRev, Plane.WorldXY);

            nMCircle.Transform(trans);

            Point3d a = GetTangentialPoint(trans, prevPt, nMCircle, false);
            Point3d b = GetTangentialPoint(trans, nextPt, nMCircle, true);

            Transform transRev = Transform.PlaneToPlane(Plane.WorldXY, planeRev);
            nMCircle.ClosestParameter(b, out double param);
            var tan = nMCircle.TangentAt(param);

            NurbsCurve arc = (new Arc(b, tan, a)).ToNurbsCurve();

            arc.Reverse();
            arc.Transform(transRev);
            arc.Domain = new Interval(0, 1);
            return arc;
        }

        public NurbsCurve DrawArrowTip(Curve crv, double param)
        {
            crv.PerpendicularFrameAt(param, out Plane p);

            Plane p2 = new Plane(p.Origin, p.XAxis, p.ZAxis);

            double dir = (param < 0.5) ? 0.5 : -0.5;
            p2.Rotate(Math.PI * dir, p.YAxis);

            Polyline arrow = Polyline.CreateInscribedPolygon(new Circle(p2, markerSize), 3);

            return arrow.ToNurbsCurve();
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (previous != null)
                args.Display.DrawCurve(previous, previousColor, drawingThickness);
            if (next != null)
                args.Display.DrawCurve(next, nextColor, drawingThickness);

            //Arrow
            if (arrowLine != null)
                args.Display.DrawCurve(arrowLine, nextColor, drawingThickness);
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
    }
}