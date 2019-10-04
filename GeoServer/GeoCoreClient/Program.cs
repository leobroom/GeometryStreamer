using System;
using System.Threading;
using GeoServer;

namespace GeoCoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("############");
            Console.WriteLine("Client 1");
            Console.WriteLine("############");

            Thread.Sleep(2000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            var client = new Client("127.0.0.1", 12345, "Client 1", ThreadingType.Thread);
            client.Message += OnMessage;
            client.Connect();

            client.SendingRandomData(1000);

            for (int i = 0; i < 1000; i++)
            {
                BroadCastMsg bc = new BroadCastMsg() { broadcastMsg = " Hey hier ist client 1 Whats up????" };

                client.Send(bc);
            }
       
            Console.Read();
            client.Disconnect();
        }

        private static void OnMessage(object sender, MessageArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}