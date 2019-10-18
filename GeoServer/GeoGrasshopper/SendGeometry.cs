using GeoStreamer;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace GeoGrasshopper
{
    /// <summary>
    /// Helper Class to Rhino Send Geometry over Network
    /// </summary>
    static class Send
    {
        public static void Mesh(int id, Mesh mesh, EventClient client)
        {
            if (mesh == null || mesh.Vertices == null)
                return;

            BroadCastMesh netMesh = new BroadCastMesh
            {
                id = id,
                vertices = GetVertices(mesh),
                triangles = Triangulate(mesh),
                normals = GetNormals(mesh)
            };

            client.Send(netMesh);
        }

        private static float[] GetVertices(Mesh m)
        {
            int count = m.Vertices.Count;

            float[] vertices = new float[count * 3];

            for (int i = 0; i < count; i++)
            {
                var vert = m.Vertices[i];
                int idx = i * 3;
                vertices[idx] = vert.X;
                vertices[idx + 1] = vert.Y;
                vertices[idx + 2] = vert.Z;
            }

            return vertices;
        }

        private static float[] GetNormals(Mesh m)
        {
            int count = m.Normals.Count;

            float[] normals = new float[count * 3];

            for (int i = 0; i < count; i++)
            {
                var norm = m.Normals[i];
                int idx = i * 3;
                normals[idx] = norm.X;
                normals[idx + 1] = norm.Y;
                normals[idx + 2] = norm.Z;
            }

            return normals;
        }

        private static int[] Triangulate(Mesh m)
        {
            int facecount = m.Faces.Count;
            List<int> lst = new List<int>(facecount * 3);

            for (int i = 0; i < facecount; i++)
            {
                var mf = m.Faces[i];

                if (mf.IsQuad)
                {
                    double dist1 = m.Vertices[mf.A].DistanceTo(m.Vertices[mf.C]);
                    double dist2 = m.Vertices[mf.B].DistanceTo(m.Vertices[mf.D]);
                    if (dist1 > dist2)
                    {
                        lst.Add(mf.A);
                        lst.Add(mf.B);
                        lst.Add(mf.D);
                        lst.Add(mf.B);
                        lst.Add(mf.C);
                        lst.Add(mf.D);
                    }
                    else
                    {
                        lst.Add(mf.A);
                        lst.Add(mf.B);
                        lst.Add(mf.C);
                        lst.Add(mf.A);
                        lst.Add(mf.C);
                        lst.Add(mf.D);
                    }
                }
                else
                {
                    lst.Add(mf.A);
                    lst.Add(mf.B);
                    lst.Add(mf.C);
                }
            }
            return lst.ToArray();
        }

        // CURVES

        public static void Curves(List<Curve> curves, StreamSettings settings, EventClient client)
        {
            int crvCount = curves.Count;

            List<float> positions = new List<float>();
            int[] curveLength = new int[crvCount];
            double segmentLength = settings.CurveDivision;

            //For every Curve
            for (int crvIdx = 0; crvIdx < crvCount; crvIdx++)
            {
                Curve curve = curves[crvIdx];

                if (curve.IsPolyline())
                {
                    var nc = curve.ToNurbsCurve();

                    int ptCount = nc.Points.Count;

                    for (int i = 0; i < ptCount; i++)
                        AddPointValues(nc.Points[i].Location, positions);

                    curveLength[crvIdx] = ptCount;
                }
                else
                {
                    AddPointValues(curve.PointAt(curve.Domain.Min), positions);
                    double maxLength = curve.GetLength();
                    if (segmentLength != 0 && segmentLength < maxLength)
                    {
                        Point3d[] pts;
                        curve.DivideByLength(segmentLength, false, out pts);

                        foreach (var pt in pts)
                            AddPointValues(pt, positions);

                        curveLength[crvIdx] = pts.Length + 2;
                    }
                    else
                        curveLength[crvIdx] = 2;
                    AddPointValues(curve.PointAt(curve.Domain.Max), positions);
                }
            }

            BroadCastCurves netCurves = new BroadCastCurves
            {
                length = curveLength,
                positions = positions.ToArray(),
                colors = GetCurveColors(crvCount,settings.CurveMaterial.DiffuseColor)
            };

            client.Send(netCurves);
        }

        private static void AddPointValues(Point3d pt, List<float> positions)
        {
            positions.Add((float)pt.X);
            positions.Add((float)pt.Y);
            positions.Add((float)pt.Z);
        }

        private static int[] GetCurveColors(int curveCount, System.Drawing.Color c)
        {
            int[] colors = new int[curveCount * 3];

            for (int i = 0; i < curveCount; i++)
            {
                int idx = i * 3;

                colors[idx] = 255;
                colors[idx + 1] = 100;
                colors[idx + 2] = 50;
            }

            return colors;
        }
    }
}
