using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GeoServer
{
    public partial class Server
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static Guid serverId;
        private static Thread sendingThread;

        public Server() { }

        //Client dataBase
        static ConcurrentDictionary<Socket, ClientObject> socketToClientTable = new ConcurrentDictionary<Socket, ClientObject>();
        static ConcurrentDictionary<Socket, Queue<(byte[], byte[])>> sendingDataQueueTable = new ConcurrentDictionary<Socket, Queue<(byte[], byte[])>>();

        public static void Start(string ip, int port)
        {
            serverId = Guid.NewGuid();

            Message(LogType.Serverstart);

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
                    Message(LogType.WaitForConnection);
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Message(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket clientSocket = listener.EndAccept(ar);

            socketToClientTable.TryAdd(clientSocket, new ClientObject());
            sendingDataQueueTable.TryAdd(clientSocket, new Queue<(byte[], byte[])>());

            // Create the state object.
            HeaderState state = new HeaderState
            {
                workSocket = clientSocket,
                buffer = new byte[Serialisation.HEADERSIZE]
            };

            clientSocket.BeginReceive(state.buffer, 0, Serialisation.HEADERSIZE, 0, new AsyncCallback(ReadCallback), state);

            StartSending(clientSocket);
        }

        public static void StartSending(Socket socket)
        {
            sendingThread = new Thread(() => SendData());
            sendingThread.Start();

            void SendData()
            {
                while (true)
                {
                    try
                    {
                        var bytes = sendingDataQueueTable[socket];

                        if (bytes.Count != 0)
                        {
                            Console.WriteLine(socketToClientTable[socket].Name + "send dataaaa" + bytes.Count);
                            (byte[] header, byte[] data) = bytes.Dequeue();
                            SendBytes(socket, header, data);
                            sendDone.WaitOne();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private static void SendBytes(Socket client, byte[] header, byte[] data)
        {
            byte[] resultByte = header.Concat(data).ToArray();
            client.BeginSend(resultByte, 0, resultByte.Length, 0, new AsyncCallback(SendCallback), client);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object.
            HeaderState state = (HeaderState)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead == 0)
                return;

            if (state.headerType == -1)
            {
                Utils.WriteHeaderState(state);

                Message($"{socketToClientTable[handler].Name} | NEW { state.ToString()}");
            }

            if (bytesRead == state.dataSize)
            {
                Deserialize(handler, (MessageType)state.headerType, state.buffer);

                state = new HeaderState
                {
                    workSocket = handler,
                    buffer = new byte[Serialisation.HEADERSIZE]
                };
            }

            handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void Deserialize(Socket client, MessageType typeFromHeader, byte[] data)
        {
            switch (typeFromHeader)
            {
                case MessageType.ConnectToServer:
                    var connectToServer = Serialisation.DeserializeFromBytes<ConnectToServerMsg>(data);

                    ClientObject clientObject = socketToClientTable[client];
                    clientObject.Set(connectToServer.id, connectToServer.clientName, connectToServer.deviceType);

                    ShowAllClients();

                    SimpleMsg allowToSend = new SimpleMsg()
                    { message = SimpleMsg.Msg.AllowClientToSendData };

                    Send(allowToSend, client, serverId);
                    break;

                case MessageType.BroadCastTest:
                    var bc = Serialisation.DeserializeFromBytes<BroadCastMsg>(data);

                    SendToOthers(bc, client);
                    break;

                case MessageType.TestData:
                    var testData = Serialisation.DeserializeFromBytes<TestDataMsg>(data);
                    Console.WriteLine(testData.number);
                    break;

                case MessageType.AlternativeTestData:
                    var altTestData = Serialisation.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                    Serialisation.LogArr(altTestData.arr);
                    break;

                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
            }
        }

        /// <summary>
        /// Just for Debug purpose - shows all the clients in the database
        /// </summary>
        private static void ShowAllClients()
        {
            Console.WriteLine(Environment.NewLine + "############CLIENT LIST##############");
            foreach (var clientObject in socketToClientTable.Values)
                Console.WriteLine(clientObject);
            Console.WriteLine("#####################################" + System.Environment.NewLine);
        }

        public static void Send(ISerializableData data, Socket client, Guid clientId)
        {
            Serialisation.GetSerializedData(data, clientId, out byte[] headerData, out byte[] serializedData);

            sendingDataQueueTable[client].Enqueue((headerData, serializedData));
        }

        public static void SendToOthers(ISerializableData data, Socket client)
        {
            Guid clientId = socketToClientTable[client].Id;

            Console.WriteLine("...Send to others");
            Serialisation.GetSerializedData(data, clientId, out byte[] headerData, out byte[] serializedData);

            foreach (Socket c in sendingDataQueueTable.Keys)
            {
                if (c == client)
                    continue;

                Console.WriteLine("...Send to: " + socketToClientTable[c].Name);

                sendingDataQueueTable[c].Enqueue((headerData, serializedData));
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

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}