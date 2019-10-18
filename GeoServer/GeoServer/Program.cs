using System;
using GeoStreamer;

namespace GeoServer
{
    class Program
    {
       
        static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
            string ip = Utils.GetTestIpAdress();

            Console.WriteLine("############");
            Console.WriteLine("Server : " + ip);
            Console.WriteLine("############");

            Console.ForegroundColor = ConsoleColor.Red;
            Server.Start(ip, 12345); 

        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            Server.Stop();
        }
    }
}