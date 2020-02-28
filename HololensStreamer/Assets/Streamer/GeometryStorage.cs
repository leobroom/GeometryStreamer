﻿using System.Collections.Generic;
using UnityEngine;

class GeometryStorage
{
    private readonly List<GameObject> meshGOStorage = new List<GameObject>();
    private readonly List<GameObject> curveGOStorage = new List<GameObject>();
    private readonly List<GameObject> txtGOStorage = new List<GameObject>();

    public enum GeoType
    {
        Mesh,
        Curve,
        Txt
    }

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

    delegate GameObject GetGameObject();

    public GameObject GetGeometry(int objNr, GeoType type)
    {
        try
        {
            GameObject stored;

            switch (type)
            {
                default:
                case GeoType.Mesh:
                 
                    if (meshGOStorage.Count - 1 < objNr)
                    {
                        stored = Factory.Instance.CreateMeshObject();
                        meshGOStorage.Add(stored);
                    }
                    else
                    {
                        stored = meshGOStorage[objNr];
                    }
                    break;
                case GeoType.Curve:
               
                    if (curveGOStorage.Count-1 < objNr)
                    {
                        stored = Factory.Instance.CreateCurveObject();
                        curveGOStorage.Add(stored);
                    }
                    else
                    {
                        stored = curveGOStorage[objNr];
                    }
                    break;
                case GeoType.Txt:
             
                    if (txtGOStorage.Count - 1 < objNr)
                    {
                        stored = Factory.Instance.CreateTextObject();
                        txtGOStorage.Add(stored);
                    }
                    else
                    {
                        stored = txtGOStorage[objNr];
                    }
                    break;
            }

            return stored;
        }
        catch (System.Exception e)
        {
            string error = $"ID: {objNr}, Typ: {type}, curveGOStorage {curveGOStorage.Count}, meshGOStorage {meshGOStorage.Count}, textGOStorage {txtGOStorage.Count} " + e.Message;
            throw new System.Exception(error);
        }
    }

    private void UpdateGeo(int count, List<GameObject> goTable)
    {
        //Debug.Log($"UpdateGeo----------: " +  count +  "    "+goTable.Count);

        int tableCount = goTable.Count;
        if (count < tableCount)
        {
            int toDelete = tableCount - count;

            int toCreate = count - tableCount;

            Debug.Log($"Destroy: " + toDelete);

            List<GameObject> geos = new List<GameObject>();

            for (int i = 0; i < toDelete; i++)
            {
                geos.Add(goTable[i]);
            }

            for (int i = 0; i < geos.Count; i++)
            {
                goTable.RemoveAt(0);
                Factory.Instance.DestroyGeometry(geos[i], i * 0.01f);
            }
        }

        //Debug.Log($"Nothing ");
    }

    public void DeleteToomuchGeometry(int curveCount, int meshCount, int txtCount)
    {
        Debug.Log($"GeoUpdate CRV/MSH/TXT:  {curveCount},  {meshCount},  {txtCount}");
        UpdateGeo(curveCount, curveGOStorage);
        UpdateGeo(meshCount, meshGOStorage);
        UpdateGeo(txtCount, txtGOStorage);
        //Debug.Log($"---------------");
    }
}