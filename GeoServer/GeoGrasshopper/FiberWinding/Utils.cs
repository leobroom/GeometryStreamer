using Rhino.Geometry;
using System;

namespace GeoGrasshopper.FiberWinding
{
    public static class Utils
    {
        public enum CircleTangent
        {
            Center = 0,
            Left = 1,
            Right = 2
        }
        //http://csharphelper.com/blog/2014/12/find-the-tangent-lines-between-two-circles-in-c/
        public static bool FindCircleTangent(CircleTangent startTan1, CircleTangent startTan2, Circle c1, Circle c2, out Point3d p1, out Point3d p2)
        {
            p1 = Point3d.Origin;
            p2 = Point3d.Origin;

            if (startTan1 == CircleTangent.Left && startTan2 == CircleTangent.Left || startTan1 == CircleTangent.Right && startTan2 == CircleTangent.Right)
            {
                return FindOuterCircleTangents(startTan1, c1.Center, c1.Radius, c2.Center, c2.Radius, out p1, out p2);
            }
            else if (startTan1 == CircleTangent.Left && startTan2 == CircleTangent.Right || startTan1 == CircleTangent.Right && startTan2 == CircleTangent.Left)
            {
                return FindInnerCircleTangents(startTan1, c1.Center, c1.Radius, c2.Center, c2.Radius, out p1, out p2);
            }
            else if (startTan1 == CircleTangent.Center && startTan2 == CircleTangent.Left || startTan1 == CircleTangent.Center && startTan2 == CircleTangent.Right)
            {
                return FindTangent(startTan1, startTan2, false, c1.Center, c1.Radius, c2.Center, c2.Radius, out p1, out p2);
            }
            else if (startTan1 == CircleTangent.Left && startTan2 == CircleTangent.Center || startTan1 == CircleTangent.Right && startTan2 == CircleTangent.Center)
            {
                return FindTangent(startTan1, startTan2, true, c1.Center, c1.Radius, c2.Center, c2.Radius, out p1, out p2);
            }
            else
            {
                p1 = c1.Center;
                p2 = c2.Center;

                return true;
            }
        }

        private static bool FindOuterCircleTangents(CircleTangent ctanType, Point3d c1,
          double radius1, Point3d c2, double radius2, out Point3d p1, out Point3d p2)
        {
            if (ctanType == CircleTangent.Center)
                throw new Exception();

            //Radius cant be the same size
            if (radius1 == radius2)
                radius1 += 0.00001;

            // Make sure radius1 <= radius2.
            if (radius1 > radius2)
            {
                ctanType = (ctanType == CircleTangent.Left) ? CircleTangent.Right : CircleTangent.Left;

                // Call this method switching the circles.
                return FindOuterCircleTangents(ctanType, c2, radius2, c1, radius1, out p1, out p2);
            }

            // Initialize the return values in case
            // some tangents are missing.
            p1 = Point3d.Origin;
            p2 = Point3d.Origin;

            Point3d outer1_p2 = Point3d.Origin;
            Point3d outer2_p2 = Point3d.Origin;

            // ***************************
            // * Find the outer tangents *
            // ***************************
            {
                double radius2a = radius2 - radius1;
                if (!FindTangents(c2, radius2a, c1,
                  out outer1_p2, out outer2_p2))
                {
                    return false;    // There are no tangents.
                }

                double vx, vy;

                // Get the vector perpendicular to the
                // second tangent with length radius1.
                if (ctanType == CircleTangent.Right)
                {
                    p2 = outer1_p2;
                    vx = -(p2.Y - c1.Y);
                    vy = p2.X - c1.X;
                }
                else
                {
                    p2 = outer2_p2;
                    vx = p2.Y - c1.Y;
                    vy = -(p2.X - c1.X);
                }

                double vLength = (double)Math.Sqrt(vx * vx + vy * vy);
                double rL = radius1 / vLength;
                vx *= rL;
                vy *= rL;

                // Offset the tangent vector's points.
                p1 = new Point3d(c1.X + vx, c1.Y + vy, 0);
                p2 = new Point3d(p2.X + vx, p2.Y + vy, 0);
            }

            return true;
        }

        // Find the tangent points for these two circles.
        // Return the number of tangents: 4, 2, or 0.
        private static bool FindInnerCircleTangents(CircleTangent ctanType,
          Point3d c1, double radius1, Point3d c2, double radius2, out Point3d p1, out Point3d p2)
        {
            if (ctanType == CircleTangent.Center)
                throw new Exception();

            if (radius1 == radius2)
            {
                radius1 += 0.0001;
            }

            // Make sure radius1 <= radius2.
            if (radius1 > radius2)
            {
                // Call this method switching the circles.
                return FindInnerCircleTangents(ctanType, c2, radius2, c1, radius1, out p2, out p1);
            }

            // Initialize the return values in case
            // some tangents are missing.

            p1 = Point3d.Origin;
            p2 = Point3d.Origin;

            Point3d inner1_p2 = Point3d.Origin;
            Point3d inner2_p2 = Point3d.Origin;

            // If the circles intersect, then there are no inner tangents.
            double dx = c2.X - c1.X;
            double dy = c2.Y - c1.Y;

            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist <= radius1 + radius2)
                return false;

            // ***************************
            // * Find the inner tangents *
            // ***************************
            double radius1a = radius1 + radius2;
            FindTangents(c1, radius1a, c2,
              out inner1_p2, out inner2_p2);

            double vx, vy;

            if (ctanType == CircleTangent.Left)
            {
                p2 = inner1_p2;

                // Get the vector perpendicular to the
                // first tangent with length radius2.
                vx = p2.Y - c2.Y;
                vy = -(p2.X - c2.X);
            }
            else
            {
                p2 = inner2_p2;

                // Get the vector perpendicular to the
                // second tangent with length radius2.
                vx = -(inner2_p2.Y - c2.Y);
                vy = inner2_p2.X - c2.X;
            }

            double vLength = (double)Math.Sqrt(vx * vx + vy * vy);
            double rL = radius2 / vLength;
            vx *= rL;
            vy *= rL;
            // Offset the tangent vector's points.
            p1 = new Point3d(c2.X + vx, c2.Y + vy, 0);
            p2 = new Point3d(p2.X + vx, p2.Y + vy, 0);


            return true;
        }

        //http://csharphelper.com/blog/2014/11/find-the-tangent-lines-between-a-point-and-a-circle-in-c/

        // Find the tangent points for this circle and external point.
        // Return true if we find the tangents, false if the point is
        // inside the circle.
        private static bool FindTangents(Point3d center, double radius,
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
              external_point.X, external_point.Y, (float)L,
              out pt1, out pt2);

            return true;
        }

        // Find the tangent points for this circle and external point.
        // Return true if we find the tangents, false if the point is
        // inside the circle.
        private static bool FindTangent(CircleTangent ctanType1, CircleTangent ctanType2, bool reverse, Point3d center1, double radius1, Point3d center2, double radius2, out Point3d pt1, out Point3d pt2)
        {
            bool success;

            CircleTangent ctanType = (reverse) ? ctanType1 : ctanType2;

            if (!reverse)
            {
                success = FindTangents(center2, radius2,
                  center1, out pt1, out pt2);
            }

            else
            {
                success = FindTangents(center1, radius1,
                  center2, out pt2, out pt1);
            }

            Point3d external_point = (reverse) ? center2 : center1;

            if (ctanType == CircleTangent.Right)
            {

                pt1 = external_point;

            }
            else
            {
                pt2 = pt1;
                pt1 = external_point;
            }

            return success;
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
                  (cx2 + h * (cy1 - cy0) / dist),
                  (cy2 - h * (cx1 - cx0) / dist), 0);
                intersection2 = new Point3d(
                  (cx2 - h * (cy1 - cy0) / dist),
                  (cy2 + h * (cx1 - cx0) / dist), 0);

                // See if we have 1 or 2 solutions.
                if (dist == radius0 + radius1) return 1;
                return 2;
            }
        }
    }
}