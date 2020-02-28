using UnityEngine;
using GeoStreamer;
using SocketStreamer;
using TMPro;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    private UnityClient client;

    public Material surfaceMat;
    public Material curveMat;
    public GameObject parent;
    public TextMeshPro ipText;
    public GameObject txtObjPrefab;

    static TextMeshPro _ipText;

    private static Test instance;

    public static Test Instance => instance;

    static Queue<string> debugTest = new Queue<string>();

    private void Awake()
    {
        instance = this;
        _ipText = ipText;
    }

    void Start()
    {
        Factory.Instance.TxtPrefab = txtObjPrefab;
        Factory.Instance.CreateParent(parent);

        int port = Utils.GetTestPort();

        client = UnityClient.Initialize(Utils.GetTestIpAdress(), port, "Hololens", ThreadingType.Task);
        client.Message += OnMessage;
        client.Connect();

        ipText.text = Utils.GetTestIpAdress();

        BroadCastMsg bc = new BroadCastMsg() { broadcastMsg = " Hey hier ist unity Whats up????" };

        client.Send(bc);
    }

    private void Update()
    {
        client.ProcessMessages();

        lock (debugTest)
        {
            if (debugTest.Count > 0)
            {
                var e = debugTest.Dequeue();
                Debug.Log(e);
                _ipText.text = e;
            }
        }

    }

    private void OnDisable()
    {
        client?.Disconnect();
    }

    private static void OnMessage(object sender, MessageArgs e)
    {
        lock (debugTest)
        {
            debugTest.Enqueue(e.Message);
        }
    }
}