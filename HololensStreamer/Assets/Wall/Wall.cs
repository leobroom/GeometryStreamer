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
    }

    public void OnSelect() => Set();

    public void OnClear(){}

    int actualIndex = 0;

    enum Modus
    {
        None,
        SetIdx
    }

    Modus actualModus = Modus.None;

    private void Set()
    {
        switch (actualModus)
        {
            default:
            case Modus.None:
                Vector3 hit = new Vector3(provider.GazeDirection.x, 0, provider.GazeDirection.z);
                wall.transform.position = hitposition;
                wall.transform.localRotation = Quaternion.LookRotation(hit, Vector3.up);
                actualIndex = 0;
                actualModus = Modus.SetIdx;
                UnityClient.Instance.SendIndex(actualIndex);
                actualIndex++;
                break;
            case Modus.SetIdx:
                UnityClient.Instance.SendIndex(actualIndex);
                actualIndex++;
                break;
        }
    }

    void Update()
    {
        hitposition = provider.HitPosition;

        if (Input.GetKeyUp(KeyCode.KeypadEnter))
            Set();
    }
}