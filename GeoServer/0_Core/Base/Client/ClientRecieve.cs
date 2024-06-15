using System;
using System.Net.Sockets;

namespace GeoStreamer
{
    public abstract partial class Client<T> : BaseClient where T : IClient, new()
    {
        private void Receive(Socket client)
        {
                HeaderState state = new ()
                {
                    workSocket = client,
                    buffer = new byte[Serializer.HEADERSIZE]
                };

                //Header
                client.Receive(state.buffer, 0, Serializer.HEADERSIZE, 0);

                Utils.WriteHeaderState(state);

                //Data
                client.Receive(state.buffer, 0, state.buffer.Length, 0);

                Deserialize(client, state.headerType, state.buffer);
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