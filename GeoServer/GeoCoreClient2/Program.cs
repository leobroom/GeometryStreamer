using GeoServer;
using System;
using System.Threading;

namespace GeoCoreClient2
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.Green;
            var client = new Client("127.0.0.1", 12345, "Client 2", ThreadingType.Task);
            client.Connect();
            client.Message += OnMessage;
            client.SendingRandomData(10);

            Console.Read();
            client.Disconnect();
        }

        private static void OnMessage(object sender, MessageArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
