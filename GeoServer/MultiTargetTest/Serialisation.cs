using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GeoStreamer
{
    public class Serialisation
    {
        public const int HEADERSIZE = 24; //  8+16
        public static void LogArr<T>(T[] arr)
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

        public static byte[] GetHeader(object d, int length, Guid id)
        {
            int type = 0;

            if (d is ConnectToServerMsg)
                type = (int)MessageType.ConnectToServer;
            else if (d is TestDataMsg)
                type = (int)MessageType.TestData;
            else if (d is AlternativeTestDataMsg)
                type = (int)MessageType.AlternativeTestData;
            else if (d is SimpleMsg)
                type = (int)MessageType.SimpleMsg;
            else if (d is BroadCastMsg)
                type = (int)MessageType.BroadCastTest;
            else if (d is BroadCastMesh)
                type = (int)MessageType.BroadCastMesh;
            else if (d is BroadCastCurves)
                type = (int)MessageType.BroadCastCurves;
            else
                throw new Exception("MessageType is not added inside the GetHeader Method");

            byte[] data = new byte[HEADERSIZE];
            byte[] byteId = id.ToByteArray();

            Array.Copy(BitConverter.GetBytes(type), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(length), 0, data, 4, 4);

            for (int i = 0; i < 16; i++)
                data[i + 8] = byteId[i];

            return data;
        }

        public static byte[] GetHeader(byte[] d, int type, int length, Guid id)
        {
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
}