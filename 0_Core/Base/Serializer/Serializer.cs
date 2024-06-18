
using KGySoft.Serialization.Binary;
using System;
using System.Collections.Generic;
using System.IO;



namespace GeoStreamer
{
    public interface ISerializableData { }

    public class Serializer
    {
        private readonly Dictionary<Type, int> types = new Dictionary<Type, int>();

        public const int HEADERSIZE = 24; //  8+16

        /// <summary>
        /// Add Message Type
        /// </summary>
        protected void AddMType(Type type, int typeIdx) => types.Add(type, typeIdx);

        public int GetMessageType(object d) => types[d.GetType()];
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


        public void GetSerializedData(ISerializableData data, Guid id, out byte[] header, out byte[] serializedData)
        {
            serializedData = SerializeToBytes(data);
            int messageType = GetMessageType(data);
            header = GetHeader(messageType, serializedData.Length, id);
        }

        public static byte[] GetHeader(int messageType, int length, Guid id)
        {
            byte[] data = new byte[HEADERSIZE];
            byte[] byteId = id.ToByteArray();

            Array.Copy(BitConverter.GetBytes(messageType), 0, data, 0, 4);
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
            using (MemoryStream stream = new MemoryStream())
            {
                BinarySerializer.SerializeToStream(stream, source);
                return stream.ToArray();
            }
        }

        public static T DeserializeFromBytes<T>(byte[] source)
        {
            using (MemoryStream stream = new MemoryStream(source))
            {
                stream.Seek(0, SeekOrigin.Begin);

                return BinarySerializer.DeserializeFromStream<T>(stream);
            }
        }
    }
}