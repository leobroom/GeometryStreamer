using System;
using GeoServer;

namespace GeoCoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            AsynchronousClient.StartClient("127.0.0.1", 12345);
        }
    }
}