using UnityEngine;

public static class MeshMaker
{
    public static Mesh Generate(int size, Vector3[] pts)
    {
        Vector3 a = pts[0];
        Vector3 b = pts[1];
        Vector3 c = pts[2];
        Vector3 d = pts[3];

        Vector3 ab = b - a;
        Vector3 ad = d - a;
        Vector3 cd = d - c;
        Vector3 cb = b - c;

        float sFactor = 1.00f / (size - 1);
        int sizesquare = size * size;

        Vector3[] vertices = new Vector3[sizesquare];
        Vector2[] uvs = new Vector2[sizesquare];

        for (int h = 0; h < size; h++)
        {
            for (int w = 0; w < size; w++)
            {
                Vector3 pt;

                if (w < size - h)
                {
                    pt = a + (ab * w + ad * h) * sFactor;
                }
                else
                {
                    var s = size - 1;
                    pt = c + (cd * (s - w) + cb * (s - h)) * sFactor;
                }

                int idx = h * size + w;

                vertices[idx] = pt;
                uvs[idx] = new Vector2(sFactor * w, sFactor * h);
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Cloth";
        mesh.MarkDynamic();

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = CalcTris(size - 1, size - 1);
        mesh.RecalculateNormals();

        return mesh;
    }

    private static int[] CalcTris(int xSize, int ySize)
    {
        int[] tris = new int[xSize * ySize * 6];

        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                tris[ti] = vi;
                tris[ti + 3] = tris[ti + 2] = vi + 1;
                tris[ti + 4] = tris[ti + 1] = vi + xSize + 1;
                tris[ti + 5] = vi + xSize + 2;
            }
        }

        return tris;
    }
}