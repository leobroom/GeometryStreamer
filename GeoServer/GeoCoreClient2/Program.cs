using GeoServer;
using System;

namespace GeoCoreClient2
{
    class Program
    {
        static void Main(string[] args)
        {
            Client.StartClient("127.0.0.1", 12345, "Client 2");
            Console.Read();
        }
    }
}
