using System;
using System.Linq;

namespace GeoServer
{
    public class Utils
    { 
        public static void UpdateStateObject(StateObject state)
        {
            byte[] headerBytes = state.buffer.Take(Serialisation.HEADERSIZE).ToArray();
            byte[] indexBytes = state.buffer.Take(8).ToArray();

            byte[] headerType = new byte[4];
            Array.Copy(headerBytes, 0, headerType, 0, 4);

            byte[] dataSize = new byte[4];
            Array.Copy(headerBytes, 4, dataSize, 0, 4);

            byte[] id = new byte[16];
            Array.Copy(headerBytes, 8, id, 0, 16);

            state.headerType = BitConverter.ToInt32(headerType);
            state.dataSize = BitConverter.ToInt32(dataSize);
            state.id = new Guid(id);
            state.buffer = new byte[state.dataSize];
        }
    }
}