
using System;
using System.Collections.Generic;
using UnityEngine;

class GeometryStorage
{
    Dictionary<int, GameObject> storage = new Dictionary<int, GameObject>();

    public enum GeoType
    {
        Mesh,
        Curve
    }


    Dictionary<int, Mesh> meshStorage = new Dictionary<int, Mesh>();

    private static GeometryStorage instance;

    private GeometryStorage() { }

    public static GeometryStorage Instance
    {
        get
        {
            if (instance == null)
                instance = new GeometryStorage();

            return instance;
        }
    }

    public GameObject GetGeometry(int id, GeoType type)
    {

        GameObject stored;
        Debug.Log("GetGeometry");

        if (!storage.ContainsKey(id))
        {

            GameObject go;
            if (type == GeoType.Mesh)
            {
                go = Factory.Instance.CreateMeshObject();
            }
            else
            {
                go = Factory.Instance.CreateCurveObject();
            }

            storage.Add(id, go);
        }

        stored = storage[id];

        Debug.Log("GotGeometry");
        return stored;
    }

    public Mesh GetMesh(int id)
    {
        Debug.Log("GetMesh");

        Mesh stored;

        Mesh bla = new Mesh();

        Debug.Log("blaaa");

        lock (meshStorage)
        {
            if (!meshStorage.ContainsKey(id))
                meshStorage.Add(id, new Mesh());

            stored = meshStorage[id];
        }


        return stored;
    }

    internal void SetMesh(int id, Mesh mesh)
    {
        if (!storage.ContainsKey(id))
            meshStorage.Add(id, mesh);
        else
            meshStorage[id] = mesh;
    }
}