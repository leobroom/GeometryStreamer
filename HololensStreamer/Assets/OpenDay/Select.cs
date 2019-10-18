
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class Select : MonoBehaviour
{
    public GazeProvider provider;

    bool isSelected = false;

    GameObject cube;

    void Start()
    {
        CreateCube();
    }

    float s = 0.3f;

    void Update()
    {
        cube.transform.localPosition = provider.HitPosition + (provider.HitNormal * s/2);


    }

    public void OnSelect()
    {
        CreateCube();
    }

    void CreateCube()
    {
        if (cube != null) {
            cube.GetComponent<Renderer>().material.color = Color.white;
            cube.GetComponent<Collider>().enabled = true;
        }
         
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.transform.localScale = new Vector3(s, s, s);
        cube.GetComponent<Renderer>().material.color = Color.cyan;
        cube.GetComponent<Collider>().enabled = false;
    }
}