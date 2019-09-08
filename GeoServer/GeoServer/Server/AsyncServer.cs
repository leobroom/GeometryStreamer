using GeoServer;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously
public class StateObject
{
    // Client  socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];

    // header
    public int headerType = -1;
    public int dataSize = -1;
    public Guid id = Guid.Empty;

    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

class ClientObject
{
    public Guid id = Guid.Empty;
    public string name = "";
    public ClientType deviceType = ClientType.NotSet;

    public override string ToString()=> $"Id: {id}, Name: {name}, DeviceType: {deviceType}";
}

public class Server
{
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public Server() { }


    //Client dataBase
    static ConcurrentDictionary<Socket, ClientObject> socketToClientTable = new ConcurrentDictionary<Socket, ClientObject>();
    //static ConcurrentDictionary<Guid, ClientObject> idToClientTable = new ConcurrentDictionary<Guid, ClientObject>();

    public static void StartListening(string ip, int port)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Server started");


        // Establish the local endpoint for the socket.    
        IPAddress iPAdress = IPAddress.Parse(ip);
        IPEndPoint localEndPoint = new IPEndPoint(iPAdress, port);

        // Create a TCP/IP socket.
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.
        try
        {
            socket.Bind(localEndPoint);
            socket.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("Waiting for a connection...");
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.
        allDone.Set();

        Console.WriteLine("AcceptCallback..." + ar.AsyncState);

        // Get the socket that handles the client request.
        Socket listener = (Socket)ar.AsyncState;

        Socket clientSocket = listener.EndAccept(ar);
        Console.WriteLine(listener.LocalEndPoint.ToString());

        Console.WriteLine("socket hash" + clientSocket.GetHashCode());

        // Create the state object.
        StateObject state = new StateObject { workSocket = clientSocket };

        state.buffer = new byte[Serialisation.HEADERSIZE];

        socketToClientTable.TryAdd(clientSocket, new ClientObject());

        clientSocket.BeginReceive(state.buffer, 0, Serialisation.HEADERSIZE, 0, new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead == 0)
            return;

        if (state.headerType == -1)
        {
            Console.WriteLine(" state.buffer: " + state.buffer.Length);

            byte[] headerBytes = state.buffer.Take(Serialisation.HEADERSIZE).ToArray();

            byte[] indexBytes = state.buffer.Take(8).ToArray();

            byte[] headerType = new byte[4];
            Array.Copy(headerBytes, 0, headerType, 0, 4);

            byte[] dataSize = new byte[4];
            Array.Copy(headerBytes, 4, dataSize, 0, 4);

            byte[] id = new byte[16];
            Array.Copy(headerBytes, 8, id, 0, 16);

            state.headerType = BitConverter.ToInt32(headerType);
            state.dataSize = BitConverter.ToInt32(dataSize);
            state.id = new Guid(id);

            Console.WriteLine(" state.headerType: " + state.headerType);
            Console.WriteLine(" state.dataSize: " + state.dataSize);
            Console.WriteLine(" state.id: " + state.id);

            state.buffer = new byte[state.dataSize];
        }

        if (bytesRead == state.dataSize)
        {
            Deserialize(handler, state.headerType, state.buffer);

            state = new StateObject
            { workSocket = handler };
            state.buffer = new byte[Serialisation.HEADERSIZE];
        }

        handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
    }

    public static void Deserialize(Socket client, int typeFromHeader, byte[] data)
    {
        switch (typeFromHeader)
        {
            case 1:
                var connectToServer = Serialisation.DeserializeFromBytes<ConnectToServerMsg>(data);

                var clientObject = socketToClientTable[client];
                clientObject.name = connectToServer.clientName;
                clientObject.deviceType = connectToServer.deviceType;
                clientObject.id = connectToServer.id;



                Console.WriteLine("connectToServerMsg: " + clientObject);

                // HIER NACHRICHT ZURÜCK SENDEN!!!!!
                Console.WriteLine("HIER NACHRICHT ZURÜCK SENDEN!!!!!HIER NACHRICHT ZURÜCK SENDEN!!!!!HIER NACHRICHT ZURÜCK SENDEN!!!!! ");
                break;
            case 98:
                var testData = Serialisation.DeserializeFromBytes<TestDataMsg>(data);
                Console.WriteLine("Result1: " + testData.number);
                break;
            case 99:
                var altTestData = Serialisation.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                Console.WriteLine("Result2: " + altTestData.txt);
                Serialisation.LogArr(altTestData.arr);
                break;
            default:
                throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
        }
    }

 
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}