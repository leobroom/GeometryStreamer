using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GeoServer
{
    public class Serialisation
    {
        public const int HEADERSIZE = 24; //  8+16
        public static void LogArr(double[] arr)
        {
            string s = "Arr: ";

            for (int i = 0; i < arr.Length; i++)
                s += arr[i] + ", ";

            Console.WriteLine(s);
        }

        public static double[] FillArr(int count)
        {
            int min = -99;
            int max = 99;
            Random random = new Random();

            double[] arr = new double[count];

            for (int i = 0; i < count; i++)
                arr[i] = random.NextDouble() * (max - min) + min;

            return arr;
        }

        public static void GetSerializedData(ISerializableData data, Guid id, out byte[] header, out byte[] serializedData)
        {
            serializedData = SerializeToBytes(data);
            header = GetHeader(data, serializedData.Length, id);
        }


        /// <summary>
        /// Reads data into a complete array, throwing an EndOfStreamException
        /// if the stream runs out of data first, or if an IOException
        /// naturally occurs.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="data">The array to read bytes into. The array
        /// will be completely filled from the stream, so an appropriate
        /// source: https://jonskeet.uk/csharp/readbinary.html
        /// size must be given.</param>
        public static byte[] ReadWholeArray(Stream stream, int lengthOfData)
        {
            int offset = 0;
            byte[] data = new byte[lengthOfData];

            while (lengthOfData > 0)
            {
                int read = stream.Read(data, offset, lengthOfData);
                if (read <= 0)
                    throw new EndOfStreamException
                        (String.Format("End of stream reached with {0} bytes left to read", lengthOfData));
                lengthOfData -= read;
                offset += read;
            }

            return data;
        }

        public static byte[] GetHeader(object d, int length, Guid id)
        {
            int type = 0;

            if (d is ConnectToServerMsg)
                type = (int)Server.MessageType.ConnectToServer;
            else if (d is TestDataMsg)
                type = (int)Server.MessageType.TestData;
            else if (d is AlternativeTestDataMsg)
                type = (int)Server.MessageType.AlternativeTestData;
            else if (d is SimpleMsg)
                type = (int)Server.MessageType.SimpleMsg;
            else if (d is BroadCastMsg)
                type = (int)Server.MessageType.BroadCastTest;

            byte[] data = new byte[HEADERSIZE];
            byte[] byteId = id.ToByteArray();

            Array.Copy(BitConverter.GetBytes(type), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(length), 0, data, 4, 4);

            for (int i = 0; i < 16; i++)
                data[i + 8] = byteId[i];

            return data;
        }

        public static byte[] SerializeToBytes(ISerializableData source)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }

        public static T DeserializeFromBytes<T>(byte[] source)
        {
            using (var stream = new MemoryStream(source))
            {
                var formatter = new BinaryFormatter();
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }

    public interface ISerializableData { }

    [Serializable]
    public class TestDataMsg : ISerializableData
    {
        public int number = 6;
    }

    [Serializable]
    public class AlternativeTestDataMsg : ISerializableData
    {
        public string txt = "not defined";
        public double[] arr;
    }

    [Serializable]
    public class ConnectToServerMsg : ISerializableData
    {
        //0 = PC
        //1 = Hololens
        public ClientType deviceType = ClientType.Default;
        public string clientName = "defaultClient";
        public Guid id = Guid.Empty;

        public override string ToString()
            => $"ConnectToServerMsg: DeviceType: {deviceType}, ClientName: {clientName}, Id: {id}";
    }

    [Serializable]
    public class SimpleMsg : ISerializableData
    {
        public Msg message = Msg.None;
        public enum Msg
        {
            None = 0,
            AllowClientToSendData,
        }

        public override string ToString() => $"SimpleMsg:  {message}";
    }

    /// <summary>
    /// A Random BroadCastMessage, which a client can send to other clients over the server
    /// </summary>
    [Serializable]
    public class BroadCastMsg : ISerializableData
    {
        public string broadcastMsg = "not set";
    }
}