using System;
using GeoServer;

namespace GeoCoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client.StartClient("127.0.0.1", 12345, "Client 1");
            Console.Read();
        }
    }
}