using System;
using System.Threading;
using GeoStreamer;

namespace GeoCoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = Utils.GetTestIpAdress();

            Console.WriteLine("############");
            Console.WriteLine("Client 1 : "+ip);
            Console.WriteLine("############");

            Thread.Sleep(1000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            var client =  EventClient.Initialize(ip, 12345, "Client 1", ThreadingType.Thread);
            client.Message += OnMessage;
            client.Connect();

            client.SendingRandomData(10);

            for (int i = 0; i < 10; i++)
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