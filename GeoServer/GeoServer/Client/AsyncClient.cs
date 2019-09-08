using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GeoServer
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

    public class MessageArgs : EventArgs
    {
        private string message;

        public MessageArgs(string message)
        {
            this.message = message;
        }

        public string Message => message;
    }

    public partial class Client
    {
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        Socket socket;

        readonly bool useThreads = false;

#if (useThreads)
        private Thread listingThread;
        private Thread sendingThread;
#else
        private Task listingTask;
        private Task sendingTask;
#endif

        private Queue<(byte[], byte[])> sendingDataQueue = new Queue<(byte[], byte[])>();

        private string ip;
        private int port;
        private Guid id = Guid.Empty;
        private string name;
        private ClientType clientType;

        public event EventHandler<MessageArgs> Message;

        public Client(string ip, int port, string name, ThreadingType taskType, ClientType clientType = ClientType.Default)
        {
            this.ip = ip;
            this.port = port;
            this.name = name;
            this.clientType = clientType;

            id = Guid.NewGuid();
            useThreads = (taskType == ThreadingType.Thread) ? true : false;
        }

        /// <summary>
        /// Connects Client to Server
        /// </summary>
        public void Connect()
        {
            SendMessage("Try to Connect...");

            ThreadingType t = useThreads ? ThreadingType.Thread : ThreadingType.Task;
            SendMessage($"Id: {id}, IP: {ip}, Port: {port}, ThreadingType: {t}");

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

                // Send first Message
                ConnectToServer(clientType, name, id);
            }
            catch (Exception e)
            {
                SendMessage(e.ToString());
            }
        }

        private void SendMessage(string message) => Message?.Invoke(this, new MessageArgs(message));

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

                SendMessage($"Socket connected to {client.RemoteEndPoint.ToString()}");

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void StartSending(Socket socket)
        {
#if (useThreads)
            sendingThread = new Thread(() => SendData());
            sendingThread.Start();
#else
            sendingTask = new Task(() => SendData());
            sendingTask.Start();
#endif

            void SendData()
            {
                while (true)
                {
                    try
                    {
                        if (sendingDataQueue.Count != 0)
                        {
                            (byte[] header, byte[] data) = sendingDataQueue.Dequeue();
                            SendBytes(socket, header, data);
                            sendDone.WaitOne();
                        }
                    }
                    catch (Exception e)
                    {
                        SendMessage(e.Message);
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
                        SendMessage(e.Message);
                    }
                }
            }
        }
    }
}