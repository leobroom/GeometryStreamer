﻿using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace GeoServer
{
    public partial class Client
    {
        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private static void Send(Socket client, byte[]header, byte[]data)
        {
            // Convert the string data to byte data using ASCII encoding.  

            byte[] resultByte = header.Concat(data).ToArray();

            // Begin sending the data to the remote device.  

            client.BeginSend(resultByte, 0, resultByte.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}