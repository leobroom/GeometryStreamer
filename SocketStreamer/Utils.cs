using System;
using System.Linq;

namespace SocketStreamer
{
    public class Utils
    { 
        /// <summary>
        /// Fills the StateObject with the actual Information
        /// </summary>
        /// <param name="state"></param>
        public static void WriteHeaderState(HeaderState state)
        {
            byte[] headerBytes = state.buffer.Take(Serializer.HEADERSIZE).ToArray();
            byte[] headerType = new byte[4];
            Array.Copy(headerBytes, 0, headerType, 0, 4);

            byte[] dataSize = new byte[4];
            Array.Copy(headerBytes, 4, dataSize, 0, 4);

            byte[] id = new byte[16];
            Array.Copy(headerBytes, 8, id, 0, 16);

            state.headerType = BitConverter.ToInt32(headerType,0);
            state.dataSize = BitConverter.ToInt32(dataSize,0);
            state.id = new Guid(id);

            try
            {
                state.buffer = new byte[state.dataSize];
            }
            catch (Exception e)
            {

                throw new Exception("state.dataSize: " + state.dataSize  +"     - "+ e.Message);
            }
            
        }

        /// <summary>
        /// Just for debugging - do not use this
        /// </summary>
        //public static string GetTestIpAdress() => "192.168.43.45";
        //public static string GetTestIpAdress() => "192.168.178.69";
        public static string GetTestIpAdress() => "127.0.0.1";
    }
}