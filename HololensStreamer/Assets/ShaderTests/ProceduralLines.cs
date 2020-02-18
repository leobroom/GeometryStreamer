using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby;

public class ProceduralLines : MonoBehaviour
{
    public Material material;

    // Start is called before the first frame update
    void Start()
    {

      //  DigitalRuby.FastLineRenderer.SingleLineDrawer line = new DigitalRuby.FastLineRenderer.SingleLineDrawer()

        //MeshFilter filter = GetComponent<MeshFilter>();

        //var mesh = new Mesh();

        //mesh.name = "test";

        //List<Vector3> linesPos = new List<Vector3>();
        //linesPos.Add(new Vector3(0, 0, 0));
        //linesPos.Add(new Vector3(1, 5, 1));


        //List<int> trisPos = new List<int>();
        //trisPos.Add(0);
        //trisPos.Add(1);


        //mesh.SetVertices(linesPos);
        //mesh.SetIndices(trisPos.ToArray(), MeshTopology.Lines,0);
        //filter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        //GL.Begin(GL.LINES);
        //material.SetPass(0);
        //GL.Color(Color.red);
        //GL.Vertex3(0, 0, 0);
        //GL.Vertex3(2, 2, 2);
        //GL.
        //GL.End();
    }
}
