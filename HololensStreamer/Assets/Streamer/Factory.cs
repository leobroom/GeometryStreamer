using GeoStreamer;
using System;
using System.Collections.Generic;
using TMPro;
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
    private MaterialPropertyBlock propBlock = new MaterialPropertyBlock();


    private GameObject txtPrefab;

    public GameObject TxtPrefab
    {
        set { txtPrefab = value; }
    }

    private void CreateParent()
    {
        parent = new GameObject();
        parent.transform.rotation = Quaternion.Euler(-90, 0, 0);
        parent.transform.localScale = new Vector3(-1, 1, 1);
    }

    public void CreateParent(GameObject grandparent)
    {
        parent.transform.SetParent(grandparent.transform, false);
        parent.transform.rotation = Quaternion.Euler(-90, 0, 0);
        parent.transform.localScale = new Vector3(-1, 1, 1);
    }

    public GameObject CreateMeshObject()
    {
        GameObject meshObject = new GameObject("MeshObject");
        meshObject.transform.SetParent(parent.transform, false);

        MeshFilter filter = meshObject.AddComponent<MeshFilter>();
        filter.mesh = new Mesh();

        var renderer = meshObject.AddComponent<MeshRenderer>();

        renderer.sharedMaterial = Test.Instance.surfaceMat;

        return meshObject;
    }

    public GameObject CreateCurveObject()
    {
        GameObject go = new GameObject("CurveObject");
        go.transform.SetParent(parent.transform, false);

        LineRenderer linerenderer = go.AddComponent<LineRenderer>();
        linerenderer.useWorldSpace = false;

        linerenderer.sharedMaterial = Test.Instance.curveMat;
        float width = 0.004f;
        linerenderer.startWidth = width;
        linerenderer.endWidth = width;
        linerenderer.receiveShadows = false;
        linerenderer.allowOcclusionWhenDynamic = false;
        linerenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        linerenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;


        linerenderer.positionCount = 0;

        return go;
    }

    internal void UpdateText(BroadCastText broadcast)
    {
        Debug.Log("UpdateText");
        GameObject go = GeometryStorage.Instance.GetGeometry(broadcast.textNr, GeometryStorage.GeoType.Txt);
        TextMeshPro textMesh = go.GetComponent<TextMeshPro>();

        textMesh.color = GetUColor(broadcast.color);
        go.transform.localPosition = GetVector(broadcast.position);
        go.transform.localEulerAngles = GetVector(broadcast.rotation);
        textMesh.text = broadcast.text;
        textMesh.fontSize = broadcast.textSize/2;
    }

    public void UpdateMesh(BroadCastMesh broadcast)
    {
        GameObject go = GeometryStorage.Instance.GetGeometry(broadcast.meshNr, GeometryStorage.GeoType.Mesh);
        MeshFilter filter = go.GetComponent<MeshFilter>();

        Mesh mesh = filter.mesh;
        mesh.Clear();
        mesh.SetVertices(GetVector3List(broadcast.vertices));
        mesh.SetTriangles(broadcast.triangles, 0);
        mesh.SetNormals(GetVector3List(broadcast.normals));

        Color c = GetUColor(broadcast.color);
        var renderer = go.GetComponent<Renderer>();

        renderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", c);
        renderer.SetPropertyBlock(propBlock);
    }

    private Vector3 GetVector(float[] floats) => new Vector3(floats[0] / scale, floats[1] / scale, floats[2] / scale);

    private Vector3[] GetVector3Array(float[] floats, bool reverse = false)
    {
        int length = floats.Length / 3;
        Vector3[] vecs = new Vector3[length];

        for (int i = 0; i < length; i++)
        {
            int a = i * 3;
            Vector3 pos = new Vector3(floats[a] / scale, floats[a + 1] / scale, floats[a + 2] / scale);

            vecs[i] = pos;
        }

        return vecs;
    }

    private List<Vector3> GetVector3List(float[] floats, bool reverse = false)
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

    internal GameObject CreateTextObject()
    {
        Debug.Log("TextERSTELLT!");

        GameObject go = GameObject.Instantiate(txtPrefab);

        go.name = "TEXT";
        go.transform.SetParent(parent.transform, false);

     

        return go;
    }

    public void UpdateCurve(BroadCastCurve broadcast)
    {
        var allPoints = GetVector3Array(broadcast.positions);

        int length = allPoints.Length;

        GameObject go = GeometryStorage.Instance.GetGeometry(broadcast.curveNr, GeometryStorage.GeoType.Curve);
        LineRenderer renderer = go.GetComponent<LineRenderer>();
        renderer.positionCount = length;
        renderer.SetPositions(allPoints);

        renderer.GetPropertyBlock(propBlock);
        Color c = GetUColor(broadcast.colors);
        propBlock.SetColor("_Color", c);
        renderer.SetPropertyBlock(propBlock);

        float width = broadcast.width * 2;

        if (renderer.startWidth != width)
        {
            renderer.startWidth = width;
            renderer.endWidth = width;
        }
    }

    private Color GetUColor(byte[] colors)
    {
        float r = GetColorValue(colors[0]);
        float g = GetColorValue(colors[1]);
        float b = GetColorValue(colors[2]);
        float a = GetColorValue(colors[3]);

        return new Color(r, g, b, a);
    }

    private float GetColorValue(byte v)
        => ((int)v) / 255f;

    public void UpdateGeometry(BroadCastGeometryInfo broadcast)
    {
        GeometryStorage.Instance.DeleteToomuchGeometry(broadcast.curvesCount, broadcast.meshesCount, broadcast.textCount);
    }

    public void DestroyGeometry(GameObject go, float time)
    {
        GameObject.Destroy(go, time);
    }
}