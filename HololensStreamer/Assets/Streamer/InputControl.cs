using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;


public class InputControl : MonoBehaviour
{
    public GameObject cursorDummy;

    public GameObject providerObj;
    private GazeProvider provider;
    private GameObject pointer;

    public GameObject WorldCoordinateObject;

    public Material mat;

    Vector3 hitposition;

    MaterialPropertyBlock propBlock;
    readonly float pointerSize = 0.03f;


    //Orient
    private List<GameObject> orientSpheres = new List<GameObject>();

    void Start()
    {
        provider = providerObj.GetComponentInChildren<GazeProvider>();
        propBlock = new MaterialPropertyBlock();

        SetMode((int)Modus.Reset);
    }

    public void OnSelect()
    {
        switch (actualModus)
        {
            default:
            case Modus.Reset:

                break;
            case Modus.Orient:
                SetOrient();
                break;
            case Modus.Next:
                NextStep();
                break;
            case Modus.Previous:
                PreviousStep();
                break;
        }
    }

    private void SetOrient()
    {
        Debug.Log("SetOrient");

        if (orientSpheres.Count == 2)
        {
            CreateSphere(hitposition);
            SetOrientPlane();
            SetMode((int)Modus.Reset);
            return;
        }

        CreateSphere(hitposition);
    }

    void CreateSphere(Vector3 pos)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        go.GetComponent<Collider>().enabled = false;

        float s = 0.02f;
        go.transform.localScale = new Vector3(s, s, s);
        go.transform.position = pos;

        orientSpheres.Add(go);
    }

    int actualIndex = 0;

    public enum Modus
    {
        Reset = 0,
        Orient = 1,
        Next = 2,
        Previous = 3
    }

    Modus actualModus = Modus.Reset;

    public void SetMode(int modus)
    {
        actualModus = (Modus)modus;

        Debug.Log(actualModus);

        switch (actualModus)
        {
            case Modus.Orient:
                SetCursor(CursorMode.Orient);
                DestroyAllSpheres();
                SetSpatialMapping(true);
                break;
            default:
            case Modus.Reset:
                SetCursor(CursorMode.None);
                DestroyAllSpheres();
                SetSpatialMapping(false);            
                break;
            case Modus.Next:
                SetCursor(CursorMode.Next);
                DestroyAllSpheres();
                SetSpatialMapping(false);
                break;
            case Modus.Previous:
                SetCursor(CursorMode.Previous);
                DestroyAllSpheres();
                SetSpatialMapping(false);
                break;


        }
    }

    void SetSpatialMapping(bool active)
    {
        ////GEht noch nicht

        //if (active)
        //{
        //    MixedRealityToolkit.SpatialAwarenessSystem.Enable();
        //    //var observers = MixedRealityToolkit.SpatialAwarenessSystem.GetObservers();

        //    //foreach (var o in observers)
        //    //{
        //    //    o.Reset();
        //    //}

        //}
        //else
        //{
        //    MixedRealityToolkit.SpatialAwarenessSystem.Disable();
        //}
    }

    void Update()
    {
        hitposition = provider.HitPosition;

        cursorDummy.gameObject.transform.position = hitposition;
        Vector3 hit = new Vector3(provider.GazeDirection.x, 0, provider.GazeDirection.z);
        cursorDummy.gameObject.transform.localRotation = Quaternion.LookRotation(provider.transform.forward, provider.transform.up);

        if (Input.GetKeyUp(KeyCode.KeypadEnter))
            OnSelect();
    }

    public void NextStep()
    {
        actualIndex++;
        UnityClient.Instance.SendIndex(actualIndex);
    }

    public void PreviousStep()
    {
        actualIndex--;
        UnityClient.Instance.SendIndex(actualIndex);
    }


    // Cursor

    enum CursorMode
    {
        None,
        Orient,
        Next,
        Previous
    }

    CursorMode actualCursor;
    void SetCursor(CursorMode actualCursor)
    {
        this.actualCursor = actualCursor;

        switch (actualCursor)
        {
            default:
            case CursorMode.None:
                SetCursorStatus(false);
                break;
            case CursorMode.Orient:
                SetCursorStatus(true);
                SetCursorColor(Color.grey);
                break;
            case CursorMode.Next:
                SetCursorStatus(true);
                SetCursorColor(Color.white);
                break;
            case CursorMode.Previous:
                SetCursorStatus(true);
                SetCursorColor(Color.red);
                break;
        }
    }


    void SetCursorStatus(bool isActive) => cursorDummy.SetActive(isActive);

    void SetCursorColor(Color c)
    {
        LineRenderer[] renderers = cursorDummy.GetComponentsInChildren<LineRenderer>();

        foreach (var r in renderers)
        {
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", c);
            r.SetPropertyBlock(propBlock);
        }
    }

    // ORIENT

    private void DestroyAllSpheres()
    {
        foreach (var go in orientSpheres)
        {
            GameObject.Destroy(go);
        }

        orientSpheres.Clear();
    }

    void SetOrientPlane()
    {
        WorldCoordinateObject.SetActive(true);

        Vector3 a = orientSpheres[0].transform.position;
        Vector3 b = orientSpheres[1].transform.position;
        Vector3 c = orientSpheres[2].transform.position;

        Vector3 forward = b - a;
        Vector3 right = c - a;

        Vector3 up = Vector3.Cross(forward, right);

        WorldCoordinateObject.transform.localPosition = a;
        WorldCoordinateObject.transform.localRotation = Quaternion.LookRotation(forward, up);
        Vector3 movingVec = (WorldCoordinateObject.transform.forward + WorldCoordinateObject.transform.right) * 0.5f;
        WorldCoordinateObject.transform.localPosition += movingVec;
    }
}
