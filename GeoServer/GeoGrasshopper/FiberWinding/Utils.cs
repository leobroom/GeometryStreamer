using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace GeoGrasshopper.FiberWinding
{
    public static class Utils
    {
        public static void FindAndMoveDoublePins(List<Plane> weavingPlanes, Vector3d norm, int actualIdx, double fiberMulti, ref Point3d actualPt)
        {
            if (fiberMulti == 0)
                return;

            int samePoints = 0;

            if (actualIdx > 0)
            {
                for (int u = 0; u < actualIdx; u++)
                {
                    Point3d comparePt = weavingPlanes[u].Origin;
                    if (comparePt.Equals(actualPt))
                        samePoints++;
                }
            }

            Transform moveActualPoint = Transform.Translation(norm * samePoints * (fiberMulti * 2));
            actualPt.Transform(moveActualPoint);
        }

        public static Plane GetBendingPlane(Plane pStart, Plane pEnd, Point3d pt1, Point3d pt2, Vector3d bendingVec, double bendingMulti, double bendingDistance)
        {
            Vector3d xAxis = pEnd.XAxis + pStart.XAxis;
            Vector3d yAxis = pEnd.YAxis + pStart.YAxis;

            double distance = pt2.DistanceTo(pt1);

            Vector3d vecMiddle = (Vector3d)(pt2 - pt1) * bendingDistance + (bendingVec * bendingMulti * distance);
            Transform transMiddle = Transform.Translation(vecMiddle);
            var ptMiddle = pt1;
            ptMiddle.Transform(transMiddle);

            return new Plane(ptMiddle, xAxis, yAxis);
        }

        public static void CheckIfFramesHasToBeFlipped(bool isFlipped, Plane[] frames)
        {
            if (!isFlipped)
                return;

            for (int u = 0; u < frames.Length; u++)
            {
                Plane f = frames[u];
                f.Rotate(Math.PI, f.ZAxis);

                frames[u] = f;
            }
        }

        public static bool  CheckIfFlipped(Plane pCross, ref Vector3d norm, ref Vector3d bendingVec)
        {
            bendingVec = pCross.ZAxis;

            bool isFlipped = Vector3d.Multiply(norm, pCross.Normal) < 0;
            if (isFlipped)
            {
                norm = -norm;
                bendingVec = -bendingVec;
            }

            return isFlipped;
        }

        /// <summary>
        /// Get Arc params
        /// </summary>
        /// <param name="divArc">arcDivisions</param>
        /// <returns></returns>
        public static double[] GetArcParams(int divArc)
        {
            double[] arcT = new double[divArc];
            for (int i = 0; i < divArc; i++)
                arcT[i] = 1.00 / (divArc - 1) * i;

            return arcT;
        }

        public static NurbsCurve DrawArcCurve
          (Plane pCross, Point3d nextPt, Point3d actualPt, Point3d prevPt, Vector3d norm, double radius)
        {
            Plane normalPlane = new Plane(actualPt, norm);
            Circle nMCircle = new Circle(normalPlane, radius);

            Plane planeRev = nMCircle.Plane;
            Transform trans = Rhino.Geometry.Transform.PlaneToPlane(planeRev, Plane.WorldXY);

            nMCircle.Transform(trans);

            Point3d a = GetTangentialPoint(trans, prevPt, nMCircle, pCross, false);
            Point3d b = GetTangentialPoint(trans, nextPt, nMCircle, pCross, true);

            Transform transRev = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, planeRev);
            double param;
            nMCircle.ClosestParameter(b, out param);
            var tan = nMCircle.TangentAt(param);

            NurbsCurve arc = (new Arc(b, tan, a)).ToNurbsCurve();

            arc.Reverse();
            arc.Transform(transRev);
            arc.Domain = new Interval(0, 1);
            return arc;
        }

        public static Point3d GetTangentialPoint(Transform trans, Point3d next, Circle c, Plane plane, bool reverse)
        {
            next.Transform(trans);

            Point3d pt1;
            Point3d pt2;

            bool success = FindTangents(c.Plane, c.Center, c.Radius, next, out pt1, out pt2);

            if (success)
                return (reverse) ? pt2 : pt1;

            return new Point3d();
        }

        // <Custom additional code>
        // Find the tangent points for this circle and external point.
        // Return true if we find the tangents, false if the point is
        // inside the circle.
        private static bool FindTangents(Plane plane, Point3d center, double radius,
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
        private static int FindCircleCircleIntersections(
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