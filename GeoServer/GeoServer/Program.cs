using System;

namespace GeoServer
{
    class Program
    {
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Server.Start("127.0.0.1", 12345);
            //Server.Start("192.168.178.34", 12345);

    
        }
    }
}