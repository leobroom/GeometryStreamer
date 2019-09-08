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

    public partial class Client
    {
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

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

        public Client(string ip, int port, string name, ThreadingType taskType)
        {
            this.ip = ip;
            this.port = port;
            this.name = name;

            id = Guid.NewGuid();
            useThreads = (taskType == ThreadingType.Thread) ? true : false;
        }

        public void Start()
        {
            Console.WriteLine(" id: " + id);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Client started");

            // Connect to a remote device.  
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();

                Random rnd = new Random();

                // creates a number between 1 and 12
                for (int i = 0; i < 10; i++)
                {
                    int numb = rnd.Next(1, 12);

                    //NEW STUFF
                    AlternativeTestData testClass = new AlternativeTestData
                    {
                        txt = name,
                        arr = Serialisation.FillArr(numb)
                    };

                    Serialisation.GetSerializedData(testClass, id, out byte[] headerData, out byte[] serializedData);

                    sendingDataQueue.Enqueue((headerData, serializedData));

                    numb = rnd.Next(1, 200000000);
                    TestData testClass2 = new TestData
                    { number = numb };

                    Serialisation.GetSerializedData(testClass2, id, out headerData, out serializedData);
                    sendingDataQueue.Enqueue((headerData, serializedData));
                    //TEST END
                }

                // Receive the response from the remote device.  
                StartListening(socket);
                StartSending(socket);

                //// Release the socket.  
                Console.Read();

                Abort();

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Abort()
        {
#if (useThreads)
                listingThread?.Abort();
                sendingThread?.Abort();
#else
            listingTask.Dispose();
            sendingTask.Dispose();
#endif
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

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
                            Send(socket, header, data);
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
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}