using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SocketStreamer
{
    public interface ISerializableData { }

    public class Serializer
    {
        public const int HEADERSIZE = 24; //  8+16
        public void LogArr<T>(T[] arr)
        {
            string s = "Arr: ";

            for (int i = 0; i < arr.Length; i++)
                s += arr[i] + ", ";

            Console.WriteLine(s);
        }

        public double[] FillArr(int count)
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

        public virtual int GetMessageType(object d) => throw new Exception("Types not set");

        public byte[] GetHeader(int messageType, int length, Guid id)
        {
            byte[] data = new byte[HEADERSIZE];
            byte[] byteId = id.ToByteArray();

            Array.Copy(BitConverter.GetBytes(messageType), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(length), 0, data, 4, 4);

            for (int i = 0; i < 16; i++)
                data[i + 8] = byteId[i];

            return data;
        }

        public byte[] GetHeader(byte[] d, int type, int length, Guid id)
        {
            byte[] data = new byte[HEADERSIZE];
            byte[] byteId = id.ToByteArray();

            Array.Copy(BitConverter.GetBytes(type), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(length), 0, data, 4, 4);

            for (int i = 0; i < 16; i++)
                data[i + 8] = byteId[i];

            return data;
        }

        public byte[] SerializeToBytes(ISerializableData source)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }

        public T DeserializeFromBytes<T>(byte[] source)
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