using UnityEngine;
using GeoStreamer;
using SocketStreamer;
using TMPro;
using System.Threading.Tasks;

public class Test : MonoBehaviour
{
    private UnityClient client;

    public Material surfaceMat;
    public Material curveMat;
    public GameObject parent;
    public TextMeshPro ipText;

    static TextMeshPro _ipText;

    private static Test instance;

    public static Test Instance => instance;

    private void Awake()
    {
        instance = this;
        _ipText = ipText;
    }

    void Start()
    {
        Factory.Instance.CreateParent(parent);

        client = UnityClient.Initialize(Utils.GetTestIpAdress(), 12345, "Hololens", ThreadingType.Task);
        client.Message += OnMessage;
        client.Connect();

        ipText.text = Utils.GetTestIpAdress();

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
        _ipText.text = e.Message;

    }
}