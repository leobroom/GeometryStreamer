using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketStreamer
{
    public abstract partial class BaseServer : IServer
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        //Client dataBase
        protected ConcurrentDictionary<Socket, ClientObject> socketToClientTable = new ConcurrentDictionary<Socket, ClientObject>();
        protected ConcurrentDictionary<Socket, Queue<Tuple<byte[], byte[]>>> sendingDataQueueTable = new ConcurrentDictionary<Socket, Queue<Tuple<byte[], byte[]>>>();

        protected Guid serverId;

        private string ip;
        private int port;

        protected Serializer serializer = new Serializer();

        public BaseServer() { }

        public void Set(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void Start()
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
                Message("Start: " + e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }


        public void Stop()
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
        protected void RemoveClient(Socket clientSocket)
        {
            try
            {
                Message("Remove Client...");

                if (socketToClientTable.ContainsKey(clientSocket))
                {
                    ClientObject client = socketToClientTable[clientSocket];
                    client.StopThread = true;
                    socketToClientTable.TryRemove(clientSocket, out _);
                }

                sendingDataQueueTable.TryRemove(clientSocket, out _);

                clientSocket?.Dispose();

                Message("sendingDataQueueTable:" + sendingDataQueueTable.Count);
            }
            catch (Exception)
            { }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket clientSocket = listener.EndAccept(ar);
            ClientObject clientObject = new ClientObject();

            bool addClientToTable = socketToClientTable.TryAdd(clientSocket, clientObject);
            bool addToSendingTable = sendingDataQueueTable.TryAdd(clientSocket, new Queue<Tuple<byte[], byte[]>>());

            Message("addClientToTable: " + addClientToTable);
            Message("addToSendingTable: " + addToSendingTable);


            // Create the state object.
            HeaderState state = new HeaderState
            {
                workSocket = clientSocket,
                buffer = new byte[Serializer.HEADERSIZE]
            };

            StartSending(clientSocket, clientObject);
            StartListening(clientSocket, clientObject);
        }

        public void StartListening(Socket socket, ClientObject clientObject)
        {
            Thread listingThread = new Thread(() => ListenData(clientObject));
            listingThread.Start();

            void ListenData(ClientObject client)
            {
                while (!client.StopThread)
                {
                    try
                    {
                        // Receive the response from the remote device.  
                        Receive(socket);
                        //receiveDone.WaitOne();
                    }
                    catch (Exception e)
                    {
                        Message($"SendData ERROR: {client.Name}: {e.Message}");
                        RemoveClient(socket);
                    }
                }

                Message("Read Data: client.Name: " + client.Name + ",StopThread: " + client.StopThread);

            }
        }

        private void Receive(Socket socket)
        {
            try
            {
                HeaderState state = new HeaderState
                {
                    workSocket = socket,
                    buffer = new byte[Serializer.HEADERSIZE]
                };

                //Header
                socket.Receive(state.buffer, 0, Serializer.HEADERSIZE, 0);

                Utils.WriteHeaderState(state);

                //Data
                socket.Receive(state.buffer, 0, state.buffer.Length, 0);

                Deserialize(socket, state.headerType, state.buffer);
                //receiveDone.Set();
            }
            catch (Exception e)
            {
                var client = socketToClientTable[socket];
                Message($"{client.Name}: {e.Message}");
                RemoveClient(socket);
            }
        }

        private void StartSending(Socket socket, ClientObject clientObject)
        {
            Thread sendingThread = new Thread(() => SendData(clientObject));
            sendingThread.Start();

            void SendData(ClientObject client)
            {
                Message("Send Data: client.Name: " + client.Name + ",StopThread: " + client.StopThread);      

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

                        Message($"SendData ERROR: {client.Name}: {e.Message}");
                        RemoveClient(socket);
                    }
                }

                Message("Send Data: client.Name: " + client.Name + ",StopThread: " + client.StopThread);
            }
        }

        private void SendBytes(Socket client, byte[] header, byte[] data)
        {
            byte[] resultByte = header.Concat(data).ToArray();

            try
            {
                client.Send(resultByte, 0, resultByte.Length, 0);
                sendDone.Set();
            }
            catch (Exception e)
            {
                var clientObj = socketToClientTable[client];
                Message($"{clientObj.Name}: {e.Message}");
                RemoveClient(client);
            }
        }

        private void ReadCallback(IAsyncResult ar)
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
                Deserialize(socket, state.headerType, state.buffer);

                state = new HeaderState
                {
                    workSocket = socket,
                    buffer = new byte[Serializer.HEADERSIZE]
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

        protected abstract void Deserialize(Socket client, int typeFromHeader, byte[] data);

        /// <summary>
        /// Just for Debug purpose - shows all the clients in the database
        /// </summary>
        private void ShowAllClients()
        {
            Console.WriteLine(Environment.NewLine + "############CLIENT LIST##############");
            foreach (var clientObject in socketToClientTable.Values)
                Console.WriteLine(clientObject);
            Console.WriteLine("#####################################" + System.Environment.NewLine);
        }
        public void Send(ISerializableData data, Socket client, Guid clientId)
        {
            serializer.GetSerializedData(data, clientId, out byte[] headerData, out byte[] serializedData);

            sendingDataQueueTable[client].Enqueue(new Tuple<byte[], byte[]>(headerData, serializedData));
        }

        public void SendToOthers(ISerializableData data, Socket client)
        {
            Guid clientId = socketToClientTable[client].Id;

            Console.WriteLine("...Send to others");
            serializer.GetSerializedData(data, clientId, out byte[] headerData, out byte[] serializedData);

            foreach (Socket c in sendingDataQueueTable.Keys)
            {
                if (c == client)
                    continue;

                Console.WriteLine("...Send to: " + socketToClientTable[c].Name);

                sendingDataQueueTable[c].Enqueue(new Tuple<byte[], byte[]>(headerData, serializedData));
            }
        }

        public void SendToOthers(byte[] data, int type, Socket client)
        {
            Guid clientId = socketToClientTable[client].Id;

            Console.WriteLine("...Send to others");
            byte[] headerData = serializer.GetHeader(data, type, data.Length, clientId);

            foreach (Socket c in sendingDataQueueTable.Keys)
            {
                if (c == client)
                    continue;

                Console.WriteLine("...Send to: " + socketToClientTable[c].Name);

                if (sendingDataQueueTable.ContainsKey(c))
                    sendingDataQueueTable[c].Enqueue(new Tuple<byte[], byte[]>(headerData, data));
            }
        }

        private void SendCallback(IAsyncResult ar)
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
                if (ar.AsyncState == null)
                {
                    Message($"r.AsyncState == null");
                    return;
                }

                HeaderState state = (HeaderState)ar.AsyncState;
                Socket socket = state.workSocket;
                var client = socketToClientTable[socket];
                Message($"{client.Name}: {e.Message}");
                RemoveClient(socket);
            }
        }
    }
}