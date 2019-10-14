using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GeoStreamer
{
    public enum ThreadingType
    {
        Task,
        Thread
    }

    public enum ClientType
    {
        NotSet,
        Default,
        UWP
    }



    public partial class Client<T> : BaseClient where T : IClient, new()
    {
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        private Socket socket;

#if (useThreads)
        private Thread listingThread;
        private Thread sendingThread;
#else
        private Task listingTask;
        private Task sendingTask;
#endif

        private Queue<Tuple<byte[], byte[]>> sendingDataQueue = new Queue<Tuple<byte[], byte[]>>();

        private bool allowSending = false;

        private static T instance;

        protected Client() { }

        public static T Instance => instance;

        public static T Initialize(string ip, int port, string name, ThreadingType taskType, ClientType clientType = ClientType.Default)
        {
            if (instance == null)
                instance = new T();

            instance.Set(ip, port, name, taskType, clientType);

            return instance;
        }

        /// <summary>
        /// Connects Client to Server
        /// </summary>
        public void Connect()
        {
            SendLog("Try to Connect...");

            ThreadingType t = useThreads ? ThreadingType.Thread : ThreadingType.Task;
            SendLog($"Id: {id}, IP: {ip}, Port: {port}, ThreadingType: {t}");

            // Connect to a remote device.  
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), socket);

#if (useThreads)
                connectDone.WaitOne();
#endif

                // Receive the response from the remote device.  
                StartListening(socket);
                StartSending(socket);
            }
            catch (Exception e)
            {
                SendLog(e.ToString());
            }
        }

        /// <summary>
        /// Disconnect Client from Server
        /// </summary>
        public void Disconnect()
        {
            Abort();

            socket?.Shutdown(SocketShutdown.Both);
            socket?.Close();
        }

        private void Abort()
        {
#if (useThreads)
                listingThread?.Abort();
                sendingThread?.Abort();
#else
            //listingTask.Dispose();
            //sendingTask.Dispose();
#endif
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                SendLog($"Socket connected to {client.RemoteEndPoint.ToString()}");

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void StartSending(Socket socket)
        {
            ConnectToServer(clientType, name, id);
#if (useThreads)
            sendingThread = new Thread(() => SendData());
            sendingThread.Start();
#else
            sendingTask = new Task(() => SendData());
            sendingTask.Start();
#endif

            void SendData()
            {
                //First connect To Server message
                Tuple<byte[], byte[]> headData = sendingDataQueue.Dequeue();
                byte[] header = headData.Item1;
                byte[] data = headData.Item2;
                SendBytes(socket, header, data);
                sendDone.WaitOne();

                while (true)
                {
                    try
                    {
                        if (allowSending && sendingDataQueue.Count != 0)
                        {
                            headData = sendingDataQueue.Dequeue();
                            header = headData.Item1;
                            data = headData.Item2;
                            SendBytes(socket, header, data);
                            sendDone.WaitOne();
                        }
                    }
                    catch (Exception e)
                    {
                        SendLog(e.Message);
                    }
                }
            }
        }

        public void StartListening(Socket socket)
        {
#if (useThreads)
            listingThread = new Thread(() =>ListenData());
            listingThread.Start();
#else
            listingTask = new Task(() => ListenData());
            listingTask.Start();
#endif

            void ListenData()
            {
                while (true)
                {
                    try
                    {
                        // Receive the response from the remote device.  
                        Receive(socket);
                        receiveDone.WaitOne();
                    }
                    catch (Exception e)
                    {
                        SendLog(e.Message);
                    }
                }
            }
        }
    }
}