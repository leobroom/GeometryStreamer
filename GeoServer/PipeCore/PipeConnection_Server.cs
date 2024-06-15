using System;
using System.IO.Pipes;
using System.Threading;

namespace PipeCore
{

    //Source https://www.youtube.com/watch?v=nFnomZDaCC8

    public class PipeConnection_Server : PipeConnection_SenderReciever
    {
        private static PipeConnection_Server instance;
        public static PipeConnection_Server Instance => instance;

        public static event EventHandler<string> OnMessage;

        bool destroy = false;

        private PipeConnection_Server(): base()
        {
            OnMessage?.Invoke(this, "Initialize....");
        }

        public static void Initialize()
        {
            if (instance == null)
                instance = new PipeConnection_Server();
        }

   

        /// <summary>
        /// Thread for Reading Clientmessages
        /// </summary>
        protected override void Thread_Read()
        {
            NamedPipeServerStream pipeReadServer = new NamedPipeServerStream("MessageFromClient", PipeDirection.In);

            OnMessage?.Invoke(this, "Wait for Connection");

            //Wait for clients to connect
            pipeReadServer.WaitForConnection();

            OnMessage?.Invoke(this, "Client Read Connected");

            try
            {
                StreamString streamReadString = new StreamString(pipeReadServer);

                while (true)
                {
                    if (destroyed)
                    {
                        break;
                    }
                    else
                    {
                        OnMessage?.Invoke(this, "Thread_Read not destroyed");
                    }

                    string message = streamReadString.ReadString();

                    OnMessage?.Invoke(this, "Client MSG:" + message);

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

                OnMessage?.Invoke(this, "Thread_Read Error: " + e);
            }


            OnMessage?.Invoke(this, "Thread_Read Pipe closed!");

            pipeReadServer.Close();
        }

        protected override void Thread_Write()
        {
            NamedPipeServerStream pipeWriteServer = new NamedPipeServerStream("MessageToClient", PipeDirection.Out);

            //Wait for clients to connect
            pipeWriteServer.WaitForConnection();

            OnMessage?.Invoke(this, "Thread_Write Client Write Connected");

            try
            {
                StreamString streamWriteString = new StreamString(pipeWriteServer);

                SendMessage("Hello from the Server!");

                while (true)
                {
                    if (destroyed)
                    {
                        break;
                    }
                    else
                    {
                        OnMessage?.Invoke(this, "Thread_Write not destroyed");
                    }

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
                        OnMessage?.Invoke(this, "Send: " + messageQueue);
                        streamWriteString.WriteString(messageQueue);
                    }

                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }

            OnMessage?.Invoke(this, "Thread_Write Pipe closed!");

            pipeWriteServer.Close();
        }

        public override void DestroySelf()
        {
            OnMessage?.Invoke(this, "Pipe closing!");
            destroyed = true;
     
            //Creagte Fake client, so Server is not waiting in Loop

            NamedPipeClientStream pipeReadClient = new NamedPipeClientStream(".", "MessageFromClient", PipeDirection.Out, PipeOptions.Asynchronous);
            pipeReadClient.Connect();

            Thread.Sleep(100);

            SendMessage("blaaaaaaaaaa");

            //instance = null;

            //OnMessage?.Invoke(this, "instance = null!");

            //base.DestroySelf();

            //OnMessage = null;
        }
    }
}