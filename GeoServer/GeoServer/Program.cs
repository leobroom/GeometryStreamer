using System;
using GeoStreamer;
using SocketStreamer;

class Program
{
    static GeoServer server;

    static void Main()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
        string ip = Utils.GetTestIpAdress();
        int port = Utils.GetTestPort();

        Console.WriteLine("############");
        Console.WriteLine("Server : " + ip);
        Console.WriteLine("############");

        Console.ForegroundColor = ConsoleColor.Red;

        server = GeoServer.Initialize(ip, port);
        server.Start();
    }

    private static void ProcessExit(object sender, EventArgs e)
    {
        server?.Stop();
    }
}