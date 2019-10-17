using System;
using System.Linq;

namespace GeoStreamer
{
    public class Utils
    { 
        /// <summary>
        /// Fills the StateObject with the actual INformation
        /// </summary>
        /// <param name="state"></param>
        public static void WriteHeaderState(HeaderState state)
        {
            byte[] headerBytes = state.buffer.Take(Serialisation.HEADERSIZE).ToArray();
            byte[] indexBytes = state.buffer.Take(8).ToArray();

            byte[] headerType = new byte[4];
            Array.Copy(headerBytes, 0, headerType, 0, 4);

            byte[] dataSize = new byte[4];
            Array.Copy(headerBytes, 4, dataSize, 0, 4);

            byte[] id = new byte[16];
            Array.Copy(headerBytes, 8, id, 0, 16);

            state.headerType = BitConverter.ToInt32(headerType,0);
            state.dataSize = BitConverter.ToInt32(dataSize,0);
            state.id = new Guid(id);
            state.buffer = new byte[state.dataSize];
        }

        /// <summary>
        /// Just for debugging - do not use this
        /// </summary>
        public static string GetTestIpAdress() => "172.18.26.161";
    }
}