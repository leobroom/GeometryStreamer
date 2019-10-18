using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public GazeProvider provider;
    private GameObject pointer;

    public Material mat;
    public GameObject wall;

    Vector3 hitposition;

    MaterialPropertyBlock propBlock;
    readonly float pointerSize = 0.03f;

    public List<GameObject> rebarsVert = new List<GameObject>();
    public List<GameObject> concrZigZag = new List<GameObject>();
    public GameObject concrete;

    void Start()
    {
        propBlock = new MaterialPropertyBlock();
      //  CreatePointer();
    }

    private void CreatePointer()
    {
        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointer.AddComponent<SphereCollider>().enabled = false;
        pointer.transform.localScale = new Vector3(pointerSize, pointerSize, pointerSize);
        pointer.GetComponent<Renderer>().material.color = Color.cyan;
        pointer.GetComponent<Collider>().enabled = false;
    }

    public void OnSelect() => SetPos();

    public void OnClear()
    {

    }

    int actualIndex = 0;

    enum Modus
    {
        None,
        Concrete,
        VerticalRebars,
        Surface,
        Reset
    }

    Modus actualModus = Modus.None;

    private void SetPos()
    {
        switch (actualModus)
        {
            default:
            case Modus.None:
                Vector3 hit = new Vector3(provider.GazeDirection.x, 0, provider.GazeDirection.z);

                wall.transform.position = hitposition;
                wall.transform.localRotation = Quaternion.LookRotation(hit, Vector3.up);
                actualIndex = 0;
                actualModus = Modus.Concrete;
                break;
            case Modus.Concrete:
                if (concrZigZag.Count  > actualIndex)
                {
                    concrZigZag[actualIndex].SetActive(true);
                    actualIndex++;
                }
                else
                {
                    actualIndex = 0;
                    actualModus = Modus.VerticalRebars;
                }
                break;
            case Modus.VerticalRebars:
                if (rebarsVert.Count > actualIndex)
                {
                    rebarsVert[actualIndex].SetActive(true);
                    actualIndex++;
                }
                else
                {
                    actualIndex = 0;
                    actualModus = Modus.Surface;
                }
                break;
            case Modus.Surface:
                concrete.SetActive(true);
                actualModus = Modus.Reset;
                break;
            case Modus.Reset:
                concrete.SetActive(false);

                foreach (var item in rebarsVert)
                    item.SetActive(false);

                foreach (var item in concrZigZag)
                    item.SetActive(false);
                actualModus = Modus.None;
                break;
        }
    }

    void Update()
    {
        hitposition = provider.HitPosition;

        if (Input.GetKeyUp(KeyCode.KeypadEnter))
            SetPos();
    }
}