using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PipeCore
{
    public abstract class PipeConnection_SenderReciever
    {
        protected Queue<string> readQueue = new Queue<string>();
        protected Queue<string> writeQueue = new Queue<string>();

        protected object readLock;
        protected object writeLock;

        protected Thread readThread;
        protected Thread writeThread;

        public event EventHandler<PipeCommand> OnPipeCommandReceived;

        protected bool destroyed = true;

        protected PipeConnection_SenderReciever()
        {
            destroyed = false;

            readQueue = new Queue<string>();
            writeQueue = new Queue<string>();

            readLock = new object();
            writeLock = new object();

            readThread = new Thread(Thread_Read);
            readThread.Start();
            writeThread = new Thread(Thread_Write);
            writeThread.Start();
        }

        protected abstract void Thread_Read();
        protected abstract void Thread_Write();

        protected void ReadMessage()
        {
            //Hook onto the Event to Read Message
            lock (readLock)
            {
                if (readQueue.Count > 0)
                {
                    string message = readQueue.Dequeue();

                    PipeCommand pipeCommand = JsonConvert.DeserializeObject<PipeCommand>(message);

                    OnPipeCommandReceived?.Invoke(this, pipeCommand);
                }
            }
        }

        public void SendMessage(string message)
        {
            //Call from anywhere to Send a Message
            SendMessage(new PipeCommand { command = message });
        }

        public void SendMessage(PipeCommand pipeCommand)
        {
            //Call from anywhere to Send a Message
            lock (writeLock)
            {
                string message = JsonConvert.SerializeObject(pipeCommand, Formatting.Indented);
                writeQueue.Enqueue(message);
            }
        }

        public virtual void DestroySelf()
        {
            Thread.Sleep(100);

            // Run on a OnDestroy, stop the threads
            readThread?.Abort();
            writeThread?.Abort();
        }
    }
}