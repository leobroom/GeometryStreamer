using System;

namespace SocketStreamer
{
    public abstract partial class BaseServer : IServer
    {
        private enum LogType
        {
            None,
            Serverstart,
            WaitForConnection
        }

        private void Message(LogType mType)
        {
            switch (mType)
            {
                case LogType.Serverstart:
                    Console.WriteLine("#####################");
                    Console.WriteLine("Server started, id: " + serverId);
                    Console.WriteLine("#####################");
                    Console.WriteLine("");
                    break;
                case LogType.WaitForConnection:
                    Console.WriteLine("Waiting for a connection...");
                    break;
                default:
                    throw new System.Exception("No given Message");
            }
        }

        private static void Message(string message)
        {
            Console.WriteLine(message);
        }
    }
}
