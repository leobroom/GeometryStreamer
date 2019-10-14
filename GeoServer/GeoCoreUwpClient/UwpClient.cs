using GeoStreamer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UwpClient : Client<UwpClient>
{
    private Task debugTask;

    private Queue<string> debugQueue = new Queue<string>();

    public event EventHandler<MessageArgs> Message;
    protected override void SendLog(string message)
    {
        Message?.Invoke(this, new MessageArgs(message));

        Message?.Invoke(this, new MessageArgs("dsjflkdsjf"));

        //lock (debugQueue)
        //{
        //    debugQueue.Enqueue(message);
        //}
    }

    //public void StartDebugging()
    //{
    //    debugTask = new Task(() => Debug());
    //    debugTask.Start();

    //    void Debug()
    //    {
    //        string message;

    //        while (true)
    //        {
    //            try
    //            {
    //                if (debugQueue.Count != 0)
    //                {
    //                    message = debugQueue.Dequeue();
    //                    Message?.Invoke(this, new MessageArgs(message));
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                SendLog(e.Message);
    //            }
    //        }
    //    }
    //}
}