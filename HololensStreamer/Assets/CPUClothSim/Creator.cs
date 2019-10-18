using System;
using System.Collections.Generic;
using UnityEngine;

public class Creator
{
    private static Creator instance;
    private Creator() { }

    public static Creator Instance
    {
        get
        {
            if (instance == null)
                instance = new Creator();

            return instance;
        }
    }

    private Vector3 force = new Vector3(0, 30, 0);

    private int idNumb = 0;

    private Dictionary<int, Tuple<GameObject, Mesh, PointMass[], Vector3[]>> storedMeshes =
        new Dictionary<int, Tuple<GameObject, Mesh, PointMass[], Vector3[]>>();

    private List<GameObject> deleted = new List<GameObject>();

    public void CreateHangingMesh(int size, Vector3[] pts, Material mat)
    {
        Mesh mesh = MeshMaker.Generate(size, pts);
        GameObject go = CreateObject(mesh, mat, Instance.idNumb);

        PointMass[] cloth = new PointMass[size * size];
        Link[] links = new Link[6 * size * size];
        Vector3[] newVerts = new Vector3[cloth.Length];

        Tuple<GameObject, Mesh, PointMass[], Vector3[]> hangingMesh =
            new Tuple<GameObject, Mesh, PointMass[], Vector3[]>(go, mesh, cloth, newVerts);

        int linkno = 0;

        for (int j = 0; j < size; j++)
        {
            for (int i = 0; i < size; i++)
            {
                int idx = GetIndex(i, j, size);

                PointMass pm = new PointMass(mesh.vertices[idx]);

                cloth[idx] = pm;
                // Horiontal links
                if (i > 0)
                {
                    AddLink(ref linkno, cloth, links, i, j, i - 1, j, size);
                }
                // Vertical links
                if (j > 0)
                {
                    AddLink(ref linkno, cloth, links, i, j, i, j - 1, size);
                }
            }
        }

        int idx0 = 0;
        int idx1 = size - 1;
        int idx2 = size * idx1;
        int idx3 = size * size - 1;

        cloth[idx0].pinned = true;
        cloth[idx1].pinned = true;
        cloth[idx2].pinned = true;
        cloth[idx3].pinned = true;

        storedMeshes.Add(idNumb, hangingMesh);
        idNumb++;

        // Diagonal (shear)
        for (int i = 0; i < size; i++)
        {
            for (int j = 1; j < size; j++)
            {
                if (i % 2 == 0)
                {
                    if (i > 0)
                    {
                        AddLink(ref linkno, cloth, links, i, j, i - 1, j - 1, size);
                    }
                    AddLink(ref linkno, cloth, links, i, j, i + 1, j - 1, size);
                }
                else
                {
                    if (i < size - 1)
                    {
                        AddLink(ref linkno, cloth, links, i, j, i + 1, j - 1, size);
                    }
                    AddLink(ref linkno, cloth, links, i, j, i - 1, j - 1, size);
                }
            }
        }

        //    bend links
        for (int i = 0; i < size - 2; i++)
        {
            for (int j = 0; j < size - 2; j++)
            {
                AddLink(ref linkno, cloth, links, i, j, i + 2, j, size);
                AddLink(ref linkno, cloth, links, i, j, i, j + 2, size);
            }
        }
    }

    public void ClearHangingMeshes()
    {
        foreach (var item in storedMeshes.Values)
        {
            GameObject toDelete = item.Item1;

            GameObject.Destroy(toDelete);
            item.Item2.Clear();
        }

        storedMeshes.Clear();

        foreach (var item in deleted)
        {
            GameObject toDelete = item;
            GameObject.Destroy(toDelete);
        }

        deleted.Clear();
    }

    internal void DestroyMesh(int id)
    {
        deleted.Add(storedMeshes[id].Item1);

        storedMeshes.Remove(id);
    }

    private GameObject CreateObject(Mesh mesh, Material mat, int idNumb)
    {
        GameObject go = new GameObject("ClothOBj");
        var filter = go.AddComponent<MeshFilter>();
        var renderer = go.AddComponent<MeshRenderer>();

        filter.mesh = mesh;
        renderer.material = mat;

        var colorChange = go.AddComponent<ColorChange>();
        colorChange.Color1 = Color.white;
        colorChange.Color2 = Color.cyan;
        colorChange.Id = idNumb;

        float speed = UnityEngine.Random.Range(0.1f, 4);
        colorChange.Speed = speed;

        return go;
    }

    private int GetIndex(int i, int j, int size)
         => i + (size) * j;

    public void Update()
    {
        foreach (var item in storedMeshes.Values)
        {
            Mesh mesh = item.Item2;
            PointMass[] cloth = item.Item3;
            Vector3[] newVerts = item.Item4;

            int i = 0;

            foreach (var pMass in cloth)
            {
                pMass.UpdateForce(force);
                newVerts[i++] = pMass.ActualPos;
            }

            mesh.vertices = newVerts;
            // mesh.RecalculateNormals();
        }
    }

    void AddLink(ref int linkno, PointMass[] cloth, Link[] links, int i1, int j1, int i2, int j2, int size)
    {
        PointMass pMass1 = cloth[GetIndex(i1, j1, size)];
        PointMass pMass2 = cloth[GetIndex(i2, j2, size)];

        Link l = new Link(pMass1, pMass2);
        links[linkno] = l;

        pMass1.links.Add(l);
        linkno++;
    }
}