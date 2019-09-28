﻿using System;
using System.Threading;
using GeoServer;

namespace GeoCoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(500);
            Console.ForegroundColor = ConsoleColor.Yellow;
            var client = new Client("127.0.0.1", 12345, "Client 1", ThreadingType.Thread);
            client.Message += OnMessage;
            client.Connect();

            client.SendingRandomData(1);

            Console.Read();
            client.Disconnect();
        }

        private static void OnMessage(object sender, MessageArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}