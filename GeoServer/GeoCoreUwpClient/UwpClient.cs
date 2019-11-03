using GeoStreamer;
using System.Collections.Generic;

public class UwpClient : GeoClient<UwpClient>
{
    private Queue<string> debugQueue = new Queue<string>();

    //protected override void SendLog(string message)
    //{
    //    Message?.Invoke(this, new MessageArgs(message));

    //    Message?.Invoke(this, new MessageArgs("dsjflkdsjf"));

    //    //lock (debugQueue)
    //    //{
    //    //    debugQueue.Enqueue(message);
    //    //}
    //}

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