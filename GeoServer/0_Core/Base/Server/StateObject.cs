using System;
using System.Net.Sockets;
using System.Text;

namespace GeoStreamer
{
    /// <summary>
    /// State object for reading client data asynchronously
    /// </summary>
    public class HeaderState
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];

        // header
        public int headerType = -1;
        public int dataSize = -1;
        public Guid id = Guid.Empty;

        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public override string ToString() => $"STATEOBJ: ID: {id}, BufferLength: {buffer.Length}, HeaderType: {headerType}, DataSize: {dataSize}";
    }
}