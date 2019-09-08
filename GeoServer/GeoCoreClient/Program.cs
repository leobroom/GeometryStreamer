using System;
using GeoServer;

namespace GeoCoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client("127.0.0.1", 12345, "Client 1", ThreadingType.Thread);
            client.Start();

            Console.Read();
        }
    }
}