using System;
using System.Linq;
using System.Net.Sockets;

namespace SocketStreamer
{
    public partial class Client<T> : BaseClient where T : IClient, new()
    {
        public void Send(ISerializableData data)
        {
            if (abort)
            {
                SendLog($"No Connection");
                return;
            }

            serializer.GetSerializedData(data, id, out byte[] headerData, out byte[] serializedData);
            sendingDataQueue.Enqueue(new Tuple<byte[], byte[]>(headerData, serializedData));
            SendLog($"{data.GetType()} sent");
        }

        private void SendBytes(Socket client, byte[] header, byte[] data)
        {
            byte[] resultByte = header.Concat(data).ToArray();

            try
            {
                client.BeginSend(resultByte, 0, resultByte.Length, 0, new AsyncCallback(SendCallback), client);

            }
            catch (SocketException e)
            {
                SendLog(e.Message);
                Disconnect();
            }
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
                SendLog(e.ToString());
            }
        }
    }
}