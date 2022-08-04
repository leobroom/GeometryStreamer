using System;
using System.IO.Pipes;
using System.Threading;

namespace PipeCore
{
    public class PipeConnection_Client : PipeConnection_SenderReciever
    {
        public static PipeConnection_Client Instance { get; private set; }

        public PipeConnection_Client() : base()
        {
            Instance = this;

            Console.WriteLine("Waiting for client connect....");

            //FunctionUpdater.Create(ReadMessage)// Read messages on the main thread
        }

        /// <summary>
        /// Thread for Reading Clientmessages
        /// </summary>
        protected override void Thread_Read()
        {
            NamedPipeClientStream pipeReadClient = new NamedPipeClientStream(".", "MessageFromClient", PipeDirection.In);

            //Try to connect
            while (!pipeReadClient.IsConnected)
            {
                Console.WriteLine("Connecting to server...");
                try
                {
                    pipeReadClient.Connect();
                }
                catch
                {
                    Console.WriteLine("Connection failed");
                }
                Thread.Sleep(1000);
            }

            {
                Console.WriteLine("Client Read Connected!");

                try
                {
                    StreamString streamReadString = new StreamString(pipeReadClient);

                    while (true)
                    {
                        string message = streamReadString.ReadString();
                        Console.WriteLine("Server MSG:" + message);

                        // Lock to make it ThreadSafe
                        lock (readLock)
                        {
                            readQueue.Enqueue(message);
                        }

                        Thread.Sleep(10);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e);
                }

                Console.WriteLine("Pipe closed!");

                pipeReadClient.Close();
            }
        }

        protected override void Thread_Write()
        {
            NamedPipeClientStream pipeWriteClient = new NamedPipeClientStream(".", "MessageToClient", PipeDirection.Out);

            //Try to connect
            while (!pipeWriteClient.IsConnected)
            {
                Console.WriteLine("Connecting to server...");
                try
                {
                    pipeWriteClient.Connect();
                }
                catch
                {
                    Console.WriteLine("Connection failed");
                }
                Thread.Sleep(1000);
            }

            Console.WriteLine("Client Write Connected!");

            try
            {
                StreamString streamWriteString = new StreamString(pipeWriteClient);

                SendMessage("Hello from the Client!");

                while (true)
                {
                    string messageQueue = null;

                    // Lock to make it ThreadSafe
                    lock (writeLock)
                    {
                        if (writeQueue.Count > 0)
                        {
                            messageQueue = writeQueue.Dequeue();
                        }
                    }

                    if (messageQueue != null)
                    {
                        Console.WriteLine("Send: " + messageQueue);
                        streamWriteString.WriteString(messageQueue);
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }

            Console.WriteLine("Pipe closed!");

            pipeWriteClient.Close();

        }
    }
}