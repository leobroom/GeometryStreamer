using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace GeoGrasshopper.FiberWinding
{
    public partial class FiberWinder : GH_Component
    {
        /// <summary>
        /// Set the MiddlePoints between Pins
        /// </summary>
        private void SetMiddlePlanes(List<Plane> previewPlanes, NurbsCurve arc, Plane[] frames, Point3d pt1, Vector3d bendingVec)
        {
            Plane pStart = previewPlanes[previewPlanes.Count - 1];
            Plane pEnd = frames[0];
            var pt2 = arc.Points[0].Location;
            Plane normPlane1 = GetBendingPlane(pStart, pEnd, pt1, pt2, bendingVec, bendingMulti, bendingDistance / 2);
            Plane normPlane2 = GetBendingPlane(pStart, pEnd, pt1, pt2, bendingVec, bendingMulti, 1 - bendingDistance / 2);
            previewPlanes.Add(normPlane1);
            previewPlanes.Add(normPlane2);
        }

        private void SetEndPlane(List<Plane> previewPlanes, Plane[] arcFrames)
        {
            Plane lastPlane = arcFrames[arcFrames.Length - 1];
            previewPlanes.Add(new Plane(endPoint, lastPlane.XAxis, lastPlane.YAxis));
        }

        private void SetStartPlane(List<Plane> previewPlanes, Plane[] arcFrames)
        {
            Plane firstPlane = arcFrames[0];
            previewPlanes.Add(new Plane(startPoint, firstPlane.XAxis, firstPlane.YAxis));
        }

        private void SetArcPlanes(List<Plane> previewPlanes, Plane[] arcFrames)
            => previewPlanes.AddRange(arcFrames);

        private void SetDefaultValues()
        {
            weavingPlanes = new List<Plane>();
            markerSize = 0;
            idx = 0;
            pinSize = 0.05;
            fiberMulti = 0;
            bendingMulti = 0;
            bendingDistance = 0;
            startPoint = Point3d.Origin;
            endPoint = Point3d.Origin;

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
            alignAxis = 0;
            toolRotation = 0;
        }
        public void GetAndMoveDoublePins(Vector3d norm, int actualIdx, ref Point3d actualPt)
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

        public Plane GetBendingPlane(Plane pStart, Plane pEnd, Point3d pt1, Point3d pt2, Vector3d bendingVec, double bendingMulti, double bendingDistance)
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

        public void CheckIfFramesHasToBeFlipped(bool isFlipped, Plane[] frames)
        {
            if (isFlipped)
                return;

            for (int u = 0; u < frames.Length; u++)
            {
                Plane f = frames[u];
                f.Rotate(Math.PI, f.ZAxis);

                frames[u] = f;
            }
        }

        public bool CheckIfFlipped(Plane pCross, ref Vector3d norm, ref Vector3d bendingVec)
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
        /// <returns></returns>
        public double[] GetArcParams()
        {
            double[] arcT = new double[arcP];
            for (int i = 0; i < arcP; i++)
                arcT[i] = 1.00 / (arcP - 1) * i;

            return arcT;
        }

        public Point3d GetTangentialPoint(Transform trans, Point3d next, Circle c, bool reverse)
        {
            next.Transform(trans);

            bool success = GetTangents(c.Center, c.Radius, next, out Point3d pt1, out Point3d pt2);
            if (success)
                return (reverse) ? pt2 : pt1;

            return new Point3d();
        }

        // <Custom additional code>
        // Find the tangent points for this circle and external point.
        // Return true if we find the tangents, false if the point is
        // inside the circle.
        private bool GetTangents(Point3d center, double radius,
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
            GetCircleCircleIntersections(
              center.X, center.Y, radius,
              external_point.X, external_point.Y, L,
              out pt1, out pt2);

            return true;
        }

        // Find the points where the two circles intersect.
        private int GetCircleCircleIntersections(
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


        private int GetActualPlaneIdx(int previewPlaneCount)
        {
            int actualPlaneIdx = idx * (arcP + 2) + 2;
            actualPlaneIdx = (actualPlaneIdx > previewPlaneCount) ? previewPlaneCount : actualPlaneIdx;

            return actualPlaneIdx;
        }


        private void RotatePlane90Degree(Plane[] arcFrames)
        {
            double d = Math.PI / 180;

            for (int q = 0; q < arcFrames.Length; q++)
            {
                Plane f = arcFrames[q];

                f.Rotate(90 * d, f.XAxis);

                if (toolRotation != 0)
                    f.Rotate(toolRotation * d, f.ZAxis);

                arcFrames[q] = f;
            }
        }



        enum AlignAxis
        {
            None = 0,
            X = 1,
            Y = 2
        }

        private void AlignPlanes(AlignAxis axis, List<Plane> previewPlanes)
        {
            if (axis == AlignAxis.None)
                return;

            for (int q = 0; q < previewPlanes.Count; q++)
            {
                Plane p = previewPlanes[q];

                double rotateAngle = 0;

                switch (axis)
                {
                    case AlignAxis.X:
                        rotateAngle = Vector3d.VectorAngle(p.XAxis, Vector3d.XAxis, p);
                        break;
                    case AlignAxis.Y:
                        rotateAngle = Vector3d.VectorAngle(p.YAxis, Vector3d.YAxis, p);
                        break;
                }

                p.Rotate(rotateAngle, p.ZAxis);

                if (toolRotation != 0)
                    p.Rotate(toolRotation * Math.PI / 180, p.ZAxis);

                previewPlanes[q] = p;
            }
        }

        private void SetAlignAxis(int _alignAxis)
        {
            if (_alignAxis > 2 || _alignAxis < 0)
                _alignAxis = 0;

            alignAxis = (AlignAxis)_alignAxis;
        }
    }
}