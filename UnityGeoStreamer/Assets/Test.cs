using UnityEngine;
using GeoStreamer;

public class Test : MonoBehaviour
{
    UnityClient client;

    public Material surfaceMat;
    public Material curveMat;

    private static Test instance;

    public static Test Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        client = UnityClient.Initialize(Utils.GetTestIpAdress(), 12345, "Client 1", ThreadingType.Thread);
        client.Message += OnMessage;
        client.Connect();


        BroadCastMsg bc = new BroadCastMsg() { broadcastMsg = " Hey hier ist unity Whats up????" };

        client.Send(bc);
    }

    private void Update()
    {
        client.ProcessMessages();
    }

    private void OnDisable()
    {
        client?.Disconnect();
    }

    private static void OnMessage(object sender, MessageArgs e)
    {
        Debug.Log(e.Message);
    }
}
