using System.Collections.Generic;
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

    public GameObject GetGeometry(int id, GeoType type)
    {
        try
        {
            GameObject stored;

            switch (type)
            {
                default:
                case GeoType.Mesh:
                    stored = meshGOStorage[id];
                    break;
                case GeoType.Curve:
                    stored = curveGOStorage[id];
                    break;
                case GeoType.Txt:
                    stored = txtGOStorage[id];
                    break;
            }

            return stored;
        }
        catch (System.Exception e )
        {
            string error = $"ID: {id}, Typ: {type}, curveGOStorage {curveGOStorage.Count}, meshGOStorage {meshGOStorage.Count} " + e.Message;
            throw new System.Exception(error);
        }
       
    }

    private void UpdateGeo(int count, List<GameObject> goTable, GetGameObject getGo)
    {

        Debug.Log($"UpdateGeo----------: " +  count +  "    "+goTable.Count);

        int tableCount = goTable.Count;
        if (count > tableCount)
        {
            int toCreate = count - tableCount;

            Debug.Log($"Create: " + toCreate);

            for (int i = 0; i < toCreate; i++)
            {
               var geo =  getGo();
                goTable.Add(geo);
            }
        }
        else if (count < tableCount)
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


        Debug.Log($"Nothing ");

    }

    public void UpdateGeometry(int curveCount, int meshCount, int txtCount)
    {
        Debug.Log($"GeoUpdate CRV/MSH:  {curveCount},  {meshCount}");
        UpdateGeo(curveCount, curveGOStorage, Factory.Instance.CreateCurveObject);
        UpdateGeo(meshCount, meshGOStorage, Factory.Instance.CreateMeshObject);
        UpdateGeo(txtCount, txtGOStorage, Factory.Instance.CreateTextObject);
        Debug.Log($"---------------");
        Debug.Log($"GeoUpdate CRV STOR/MSH STOR:  {curveGOStorage.Count},  {meshGOStorage.Count}");
    }
}