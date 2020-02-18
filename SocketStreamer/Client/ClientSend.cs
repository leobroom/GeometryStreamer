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
                client.Send(resultByte, 0, resultByte.Length, 0);
            }
            catch (SocketException e)
            {
                SendLog(e.Message);
                Disconnect();
            }
        }
    }
}