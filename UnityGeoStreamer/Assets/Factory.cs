using GeoStreamer;
using System.Collections.Generic;
using UnityEngine;

public class Factory
{
    private static Factory instance;

    private int scale = 1000;

    private Factory() { }

    public static Factory Instance
    {
        get
        {
            if (instance == null)
            {

                instance = new Factory();
                instance.CreateParent();
            }


            return instance;
        }
    }

    GameObject parent;
    private void CreateParent()
    {
        parent = new GameObject();
        parent.transform.rotation = Quaternion.Euler(-90, 0, 0);

    }

    public GameObject CreateMeshObject()
    {
        GameObject meshObject = new GameObject("MeshObject");
        meshObject.transform.SetParent(parent.transform, false);

        MeshFilter filter = meshObject.AddComponent<MeshFilter>();
        filter.mesh = new Mesh();

        var renderer = meshObject.AddComponent<MeshRenderer>();

        renderer.material = Test.Instance.surfaceMat;

        return meshObject;
    }

    public GameObject CreateCurveObject()
    {
        GameObject go = new GameObject("CurveObject");
        go.transform.SetParent(parent.transform, false);

        LineRenderer linerenderer = go.AddComponent<LineRenderer>();
        linerenderer.useWorldSpace = false;

        linerenderer.material = Test.Instance.curveMat;
        Color c = Color.cyan;
        float width = 0.04f;
        linerenderer.startColor = c;
        linerenderer.endColor = c;
        linerenderer.startWidth = width;
        linerenderer.endWidth = width;

        return go;
    }

    public void CreateMesh(BroadCastMesh broadcast)
    {
        // Debug.Log(broadcast.ToString());
        Debug.Log("mesh updated");

        GameObject go = GeometryStorage.Instance.GetGeometry(broadcast.id, GeometryStorage.GeoType.Mesh);
        MeshFilter filter = go.GetComponent<MeshFilter>();

        Mesh mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(GetVector3Array(broadcast.vertices));
        mesh.SetTriangles(broadcast.triangles, 0);
        mesh.SetNormals(GetVector3Array(broadcast.normals));
    }

    private List<Vector3> GetVector3Array(float[] floats)
    {
        int length = floats.Length / 3;
        List<Vector3> vecs = new List<Vector3>(length);

        for (int i = 0; i < length; i++)
        {
            int a = i * 3;
            Vector3 pos = new Vector3(floats[a] / scale, floats[a + 1] / scale, floats[a + 2] / scale);

            vecs.Add(pos);
        }

        return vecs;
    }

    public void CreateCurves(BroadCastCurves broadcast)
    {
        int count = broadcast.ids.Length;
        int startPos = 0;

        var allPoints = GetVector3Array(broadcast.positions).ToArray();
        for (int i = 0; i < count; i++)
        {
            int id = broadcast.ids[i];
            int length = broadcast.curveLength[i];

            Vector3[] vecs = new Vector3[length];

            for (int l = 0; l < length; l++)
                vecs[l] = allPoints[startPos + l];

            GameObject go = GeometryStorage.Instance.GetGeometry(id, GeometryStorage.GeoType.Curve);
            LineRenderer renderer = go.GetComponent<LineRenderer>();
            renderer.positionCount = length;
            renderer.SetPositions(vecs);

            startPos += broadcast.curveLength[i];
        }
    }
}