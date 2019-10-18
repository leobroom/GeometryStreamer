using System.Collections;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    public Color Color1, Color2;
    public float Speed = 2, Offset;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    public float speed = 10000;
    private Cloth cloth;
    public int Id = -1;
    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    float countDown = 0;

    private void Start()
    {
        cloth = gameObject.GetComponent<Cloth>();
        Invoke("DestroyMeshUpdate", 3);//this will happen after 2 seconds
    }

    void DestroyMeshUpdate()
    {
        Creator.Instance.DestroyMesh(Id);
    }


    void Update()
    {
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_WireColor", Color.Lerp(Color1, Color2, (Mathf.Sin(Time.time * Speed + Offset) + 1) / 2f));
        _renderer.SetPropertyBlock(_propBlock);
    }
}