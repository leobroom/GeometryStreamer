using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeOrient : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject a;
    public GameObject b;
    public GameObject c;

    public GameObject plane;

    void Start()
    {
        plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plane.transform.localScale = new Vector3(1, 0.001f, 1);

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = b.transform.position - a.transform.position;
        Vector3 right = c.transform.position - a.transform.position;

        Vector3 up = Vector3.Cross(forward, right);

        plane.transform.localPosition = a.transform.position;
        plane.transform.localRotation = Quaternion.LookRotation(forward, up);
        plane.transform.localPosition += (plane.transform.forward + plane.transform.right) * 0.5f;
    }
}
