using System;
using System.Linq;
using System.Net.Sockets;

namespace GeoServer
{
    public partial class Client
    {
        public void Send(ISerializableData data)
        {
            Serialisation.GetSerializedData(data, id, out byte[] headerData, out byte[] serializedData);
            sendingDataQueue.Enqueue((headerData, serializedData));
            SendMessage($"{data.GetType()} sent");
        }

        private void SendBytes(Socket client, byte[] header, byte[] data)
        {
            byte[] resultByte = header.Concat(data).ToArray();
            client.BeginSend(resultByte, 0, resultByte.Length, 0, new AsyncCallback(SendCallback), client);        
        }

        private void SendCallback(IAsyncResult ar)
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
                SendMessage(e.ToString());
            }
        }
    }
}