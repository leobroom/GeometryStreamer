using System;

namespace GeoServer
{
    class Program
    {
        static void Main()
        {        
            Console.WriteLine("#####################");
            Server.StartListening("127.0.0.1",12345);
       //     AsynchronousSocketListener.StartListening("192.168.178.34", 12345);
        }
    }
}