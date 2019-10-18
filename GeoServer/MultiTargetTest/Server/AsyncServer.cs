using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GeoStreamer
{
    public partial class Server
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static Guid serverId;

        public Server() { }

        //Client dataBase
        static ConcurrentDictionary<Socket, ClientObject> socketToClientTable = new ConcurrentDictionary<Socket, ClientObject>();
        static ConcurrentDictionary<Socket, Queue<Tuple<byte[], byte[]>>> sendingDataQueueTable = new ConcurrentDictionary<Socket, Queue<Tuple<byte[], byte[]>>>();

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


        public static void Stop()
        {
            List<Socket> clientsToRemove = new List<Socket>(socketToClientTable.Keys);

            foreach (Socket toRemove in clientsToRemove)
            {
                RemoveClient(toRemove);
            }
        }

        /// <summary>
        /// Removes the Client from the Tables
        /// </summary>
        /// <param name="clientSocket"></param>
        private static void RemoveClient(Socket clientSocket)
        {
            if (socketToClientTable.ContainsKey(clientSocket))
            {
                ClientObject client = socketToClientTable[clientSocket];
                client.StopThread = true;
                socketToClientTable.TryRemove(clientSocket, out client);
            }

            sendingDataQueueTable.TryRemove(clientSocket, out var value);

            clientSocket.Close();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket clientSocket = listener.EndAccept(ar);
            ClientObject clientObject = new ClientObject();

            socketToClientTable.TryAdd(clientSocket, clientObject);
            sendingDataQueueTable.TryAdd(clientSocket, new Queue<Tuple<byte[], byte[]>>());

            // Create the state object.
            HeaderState state = new HeaderState
            {
                workSocket = clientSocket,
                buffer = new byte[Serialisation.HEADERSIZE]
            };

            clientSocket.BeginReceive(state.buffer, 0, Serialisation.HEADERSIZE, 0, new AsyncCallback(ReadCallback), state);

            StartSending(clientSocket, clientObject);
        }

        private static void StartSending(Socket socket, ClientObject clientObject)
        {
            Thread sendingThread = new Thread(() => SendData(clientObject));
            sendingThread.Start();

            void SendData(ClientObject client)
            {
                while (!client.StopThread)
                {
                    try
                    {
                        var bytes = sendingDataQueueTable[socket];

                        if (bytes.Count != 0)
                        {
                            Console.WriteLine(socketToClientTable[socket].Name + "send dataaaa" + bytes.Count);
                            Tuple<byte[], byte[]> headerData = bytes.Dequeue();
                            byte[] header = headerData.Item1;
                            byte[] data = headerData.Item2;
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
            Socket socket = state.workSocket;

            // Read data from the client socket. 

            int bytesRead = 0;
            try
            {
                 bytesRead = socket.EndReceive(ar);      
            }
            catch (SocketException e)
            {
                var client = socketToClientTable[socket];
                Message($"{client.Name}: {e.Message}");
                RemoveClient(socket);
            }

            if (bytesRead == 0)
                return;

            if (state.headerType == -1)
            {
                Utils.WriteHeaderState(state);

                Message($"{socketToClientTable[socket].Name} | NEW { state.ToString()}");
            }

            if (bytesRead == state.dataSize)
            {
                Deserialize(socket, (MessageType)state.headerType, state.buffer);

                state = new HeaderState
                {
                    workSocket = socket,
                    buffer = new byte[Serialisation.HEADERSIZE]
                };
            }

            try
            {
                socket.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (SocketException e)
            {

                var client = socketToClientTable[socket];
                Message($"{client.Name}: {e.Message}");
                RemoveClient(socket);
            }
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

                case MessageType.BroadCastMesh:
                    SendToOthers(data, (int)typeFromHeader, client);
                    break;
                case MessageType.BroadCastCurves:
                    SendToOthers(data, (int)typeFromHeader, client);
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

            sendingDataQueueTable[client].Enqueue(new Tuple<byte[], byte[]>(headerData, serializedData));
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

                sendingDataQueueTable[c].Enqueue(new Tuple<byte[], byte[]>(headerData, serializedData));
            }
        }

        public static void SendToOthers(byte[] data, int type, Socket client)
        {
            Guid clientId = socketToClientTable[client].Id;

            Console.WriteLine("...Send to others");
            byte[] headerData = Serialisation.GetHeader(data, type, data.Length, clientId);

            foreach (Socket c in sendingDataQueueTable.Keys)
            {
                if (c == client)
                    continue;

                Console.WriteLine("...Send to: " + socketToClientTable[c].Name);

                sendingDataQueueTable[c].Enqueue(new Tuple<byte[], byte[]>(headerData, data));
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