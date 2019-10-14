using System;
using GeoStreamer;

namespace GeoServer
{
    class Program
    {
        static string ip = "192.168.1.221";
        static void Main()
        {
            Console.WriteLine("############");
            Console.WriteLine("Server : " + ip);
            Console.WriteLine("############");

            Console.ForegroundColor = ConsoleColor.Red;
            //Server.Start("127.0.0.1", 12345);
            Server.Start(ip, 12345); 
        }
    }
}