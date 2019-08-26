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
            Client.StartClient("127.0.0.1", 12345, "Client 2");
            Console.Read();
        }
    }
}
