using System;
using System.Net.Sockets;

namespace SocketStreamer
{
    public abstract partial class Client<T> : BaseClient where T : IClient, new()
    {
        private void Receive(Socket client)
        {
            try
            {
                HeaderState state = new HeaderState
                {
                    workSocket = client,
                    buffer = new byte[Serializer.HEADERSIZE]
                };

                client.BeginReceive(state.buffer, 0, Serializer.HEADERSIZE, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public void ReadCallback(IAsyncResult ar)
        {
            SendLog("ReadCallback");

            // Retrieve the state object and the handler socket from the asynchronous state object.
            HeaderState state = (HeaderState)ar.AsyncState;
            Socket server = state.workSocket;

            // Read data from the client socket. 
            int bytesRead =0;

            try
            {
                 bytesRead = server.EndReceive(ar);
            }
            catch (Exception e)
            {
                Disconnect();
            }

            SendLog("bytesRead= " + 0);

            if (bytesRead == 0)
                return;

            SendLog("(state.headerType= " + (state.headerType);

            if (state.headerType == -1)
            {
                Utils.WriteHeaderState(state);

                SendLog(state.ToString());
            }

            SendLog(bytesRead + " == " + state.dataSize);

            if (bytesRead == state.dataSize)
            {
                Deserialize(server, state.headerType, state.buffer);

                state = new HeaderState
                {
                    workSocket = server,
                    buffer = new byte[Serializer.HEADERSIZE]
                };
            }

            server.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
        }

        /// <summary>
        /// abstract function for deserialize Data - here is where the magic happens
        /// </summary>
        /// <param name="client"></param>
        /// <param name="typeFromHeader"></param>
        /// <param name="data"></param>
        protected abstract void Deserialize(Socket client, int typeFromHeader, byte[] data);     
    }
}