using GeoStreamer;
using Rhino.Display;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GeoGrasshopper
{
    /// <summary>
    /// Helper Class to Rhino Send Geometry over Network
    /// </summary>
    static class Send
    {
        public static void GeometryInfo(int curveCount, int meshCount, int textCount, RhinoClient client)
        {
            BroadCastGeometryInfo netMesh = new BroadCastGeometryInfo
            {
                curvesCount = curveCount,
                meshesCount = meshCount,
                textCount = textCount
            };

            client.Send(netMesh);
        }

        public static void Mesh(int id, int meshNr, Mesh mesh, StreamSettings settings, RhinoClient client)
        {
            if (mesh == null || mesh.Vertices == null)
                return;

            BroadCastMesh netMesh = new BroadCastMesh
            {
                id = id,
                meshNr = meshNr,
                vertices = GetVertices(mesh),
                triangles = Triangulate(mesh),
                normals = GetNormals(mesh),
                color = GetColor(GetSettingsMaterial(id, settings).Diffuse)
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

        public static T GetSettingsValue<T>(int id, List<T> values, string error)
        {
            int count = values.Count;

            if (id < count)
                return values[id];
            else if (count > 0)
                return values[count - 1];
            else
                throw new System.Exception(error);
        }

        public static DisplayMaterial GetSettingsMaterial(int id, StreamSettings settings)
        {
            int matCount = settings.Materials.Count;
            int objMatIdsCount = settings.ObjMatIds.Count;

            if (id < objMatIdsCount)
            {
                int objMatId = settings.ObjMatIds[id];

                if (objMatId < matCount && objMatId >= 0)
                    return settings.Materials[objMatId];
                else
                    throw new System.Exception("No Material with the ID: " + objMatId);
            }
            else if (objMatIdsCount > 0)
                return settings.Materials[matCount - 1];
            else
                throw new System.Exception("No Material");
        }

        internal static void Text(int id, int textNr, StreamText geo, StreamSettings settings, RhinoClient client)
        {
            List<float> position = new List<float>();
            List<float> normal = new List<float>();

            AddPointValues(geo.Position, position);
            AddVector3dValues(geo.Normal, normal);

            BroadCastText netText = new BroadCastText
            {
                id = id,
                textNr = textNr,
                position = position.ToArray(),
                rotation = normal.ToArray(),
                text = geo.Text,
                textSize = geo.TextSize,
                color = GetColor(GetSettingsMaterial(id, settings).Diffuse),
            };

            client.Send(netText);
        }

        public static void Curve(int id, int curvNr, Curve curve, StreamSettings settings, RhinoClient client)
        {
            List<float> positions = new List<float>();

            double segmentLength = GetSettingsValue(id, settings.CurveDivisions, "segment not set");

            if (curve.IsPolyline())
            {
                var nc = curve.ToNurbsCurve();

                int ptCount = nc.Points.Count;

                for (int i = 0; i < ptCount; i++)
                    AddPointValues(nc.Points[i].Location, positions);
            }
            else
            {
                AddPointValues(curve.PointAt(curve.Domain.Min), positions);
                double maxLength = curve.GetLength();

                if (segmentLength != 0 && segmentLength < maxLength)
                {
                    curve.DivideByLength(segmentLength, false, out Point3d[] pts);

                    foreach (var pt in pts)
                        AddPointValues(pt, positions);
                }

                AddPointValues(curve.PointAt(curve.Domain.Max), positions);
            }

            BroadCastCurve netCurve = new BroadCastCurve
            {
                id = id,
                curveNr = curvNr,
                positions = positions.ToArray(),
                colors = GetColor(GetSettingsMaterial(id, settings).Diffuse),
                width = (float)GetSettingsValue(id, settings.CurveWidths, "cruve width not set")
            };

            client.Send(netCurve);
        }

        private static void AddPointValues(Point3d pt, List<float> positions)
        {
            positions.Add((float)pt.X);
            positions.Add((float)pt.Y);
            positions.Add((float)pt.Z);
        }

        private static void AddVector3dValues(Vector3d vec, List<float> vectors)
        {
            vectors.Add((float)vec.X);
            vectors.Add((float)vec.Y);
            vectors.Add((float)vec.Z);
        }

        private static byte[] GetColor(System.Drawing.Color c)
            => new byte[4] { c.R, c.G, c.B, c.A };
    }
}