using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace GeoServer
{
    public partial class Client
    {
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  


        // Threads...
        private static Thread listingThread;

        private static Thread sendingThread;
        private static Queue<string> sendingStringQueue = new Queue<string>();
        private static Queue<(byte[], byte[])> sendingDataQueue = new Queue<(byte[], byte[])>();

        public static void StartClient(string ip, int port, string test)
        {
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

                for (int i = 0; i < 1; i++)
                {
                    int numb = rnd.Next(1, 6);
                    //NEW STUFF
                    AlternativeTestData testClass = new AlternativeTestData
                    {
                        txt = test,
                        arr = Serialisation.FillArr(numb)
                    };

                    Serialisation.GetSerializedData(testClass, out byte[] headerData, out byte[] serializedData);

                    sendingDataQueue.Enqueue((headerData, serializedData));

                     numb = rnd.Next(1, 200000000);
                    TestData testClass2 = new TestData();
                    testClass2.number = numb;

                    Serialisation.GetSerializedData(testClass2, out headerData, out serializedData);
                    sendingDataQueue.Enqueue((headerData, serializedData));
                    //TEST END
                }





                // Send test data to the remote device.  
                //  sendingStringQueue.Enqueue("This is a test");
                //sendingStringQueue.Enqueue("kjdfkljdklgjdfklgjdfl");
                //sendingStringQueue.Enqueue("blubb");


                // Receive the response from the remote device.  


                StartListening(socket);
                StartSending(socket);

                //// Release the socket.  
                Console.Read();
                listingThread?.Abort();
                sendingThread?.Abort();
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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

        public static void StartSending(Socket socket)
        {
            sendingThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (sendingStringQueue.Count != 0)
                        {
                            string s = sendingStringQueue.Dequeue();
                            Send(socket, $"{s}<EOF>");
                            sendDone.WaitOne();
                        }

                        // Test Send Classes

                        if (sendingDataQueue.Count != 0)
                        {
                            (byte[] header, byte[] data) = sendingDataQueue.Dequeue();
                            Send(socket, header, data);
                            sendDone.WaitOne();
                        }


                        //

                      //  Thread.Sleep(100);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }
            });

            sendingThread.Start();
        }

        public static void StartListening(Socket socket)
        {
            listingThread = new Thread(() =>
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
           });

            listingThread.Start();
        }
    }
}