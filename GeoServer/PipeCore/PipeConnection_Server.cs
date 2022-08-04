using System;
using System.IO.Pipes;
using System.Threading;

namespace PipeCore
{

    //Source https://www.youtube.com/watch?v=nFnomZDaCC8

    public class PipeConnection_Server : PipeConnection_SenderReciever
    {
        public static PipeConnection_Server Instance { get; private set; }

        public PipeConnection_Server() : base()
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
            NamedPipeServerStream pipeReadServer = new NamedPipeServerStream("MessageFromClient", PipeDirection.In);

            //Wait for clients to connect
            pipeReadServer.WaitForConnection();
            Console.WriteLine("Client Read Connected");

            try
            {
                StreamString streamReadString = new StreamString(pipeReadServer);

                while (true)
                {
                    string message = streamReadString.ReadString();
                    Console.WriteLine("Client MSG:" + message);

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

            pipeReadServer.Close();
        }

        protected override void Thread_Write()
        {
            NamedPipeServerStream pipeWriteServer = new NamedPipeServerStream("MessageToClient", PipeDirection.Out);

            //Wait for clients to connect
            pipeWriteServer.WaitForConnection();
            Console.WriteLine("Client Write Connected");

            try
            {
                StreamString streamWriteString = new StreamString(pipeWriteServer);

                SendMessage("Hello from the Server!");

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

            pipeWriteServer.Close();
        }
    }
}