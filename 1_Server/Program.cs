﻿using System;
using System.Net;
using GeoStreamer;

class Program
{
    static GeoServer server;

    static void Main()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);
        //string ip = GetIP();
        string ip = "127.0.0.1";
        int port = Utils.GetTestPort();
  

        Console.WriteLine("############");
        Console.WriteLine("Server : " + ip);
        Console.WriteLine("############");

        Console.ForegroundColor = ConsoleColor.Red;

        server = GeoServer.Initialize(ip, port);
        server.Start();
    }

    private static string GetIP()
    {
        string strHostName = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
        IPAddress[] addr = ipEntry.AddressList;
        return addr[^1].ToString();
    }

    private static void ProcessExit(object sender, EventArgs e)
    {
        server?.Stop();
    }
}