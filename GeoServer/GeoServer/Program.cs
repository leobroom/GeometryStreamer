using System;
using GeoStreamer;

namespace GeoServer
{
    class Program
    {
       
        static void Main()
        {
             string ip = Utils.GetTestIpAdress();

            Console.WriteLine("############");
            Console.WriteLine("Server : " + ip);
            Console.WriteLine("############");

            Console.ForegroundColor = ConsoleColor.Red;
            //Server.Start("127.0.0.1", 12345);
            Server.Start(ip, 12345); 
        }
    }
}