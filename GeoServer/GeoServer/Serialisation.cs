using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GeoServer
{
    class Serialisation
    {
        const int HEADERSIZE = 8;

        public static void Test()
        {
            //TestData testClass = new TestData();
            //testClass.number = 2456798;

            AlternativeTestData testClass = new AlternativeTestData();
            testClass.txt = "Es hat funktioniert.";
            testClass.arr = FillArr(11);

            GetSerializedData(testClass, out byte[] headerData, out byte[] serializedData);

            using (MemoryStream memStream = new MemoryStream(100))
            {
                memStream.Write(headerData, 0, HEADERSIZE);
                memStream.Write(serializedData, 0, serializedData.Length);
                memStream.Seek(0, SeekOrigin.Begin);

                var headerBytes = new byte[HEADERSIZE];
                memStream.Read(headerBytes, 0, HEADERSIZE);

                int[] header = GetIntArrayFromByteArray(headerBytes);
                var data = ReadWholeArray(memStream, header[1]);

                Deserialize(header[0], data);
                Console.ReadKey();
            }
        }

        private static void LogArr(double[] arr)
        {
            string s = "Arr: ";

            for (int i = 0; i < arr.Length; i++)
                s += arr[i] + ", ";

            Console.WriteLine(s);
        }

        private static double[] FillArr(int count)
        {
            int min = -99;
            int max = 99;
            Random random = new Random();

            double[] arr = new double[count];

            for (int i = 0; i < count; i++)
                arr[i] = random.NextDouble() * (max - min) + min;

            return arr;
        }

        private static void GetSerializedData<T>(T data, out byte[] header, out byte[] serializedData)
        {
            serializedData = SerializeToBytes(data);
            header = GetHeader(data, serializedData.Length);
        }

        private static void Deserialize(int typeFromHeader, byte[] data)
        {
            Console.WriteLine("typeFromHeader: " + typeFromHeader);

            switch (typeFromHeader)
            {
                case 1:
                    var result1 = DeserializeFromBytes<TestData>(data);
                    Console.WriteLine("Result1: " + result1.number);
                    break;
                case 2:
                    var result2 = DeserializeFromBytes<AlternativeTestData>(data);
                    Console.WriteLine("Result2: " + result2.txt);
                    LogArr(result2.arr);
                    break;

                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
            }
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

        public static byte[] GetHeader(object d, int length)
        {
            int[] header = new int[2];

            int type = 0;

            if (d is TestData)
                type = 1;
            else if (d is AlternativeTestData)
                type = 2;

            header[0] = type;
            header[1] = length;

            byte[] data = new byte[HEADERSIZE];

            for (int i = 0; i < 2; i++)
                Array.Copy(BitConverter.GetBytes(header[i]), 0, data, i * 4, 4);

            return data;
        }

        public static int[] GetIntArrayFromByteArray(byte[] byteArray)
        {
            int[] intArray = new int[byteArray.Length / 4];

            for (int i = 0; i < byteArray.Length; i += 4)
                intArray[i / 4] = BitConverter.ToInt32(byteArray, i);

            return intArray;
        }

        public static byte[] SerializeToBytes<T>(T source)
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

        [Serializable]
        class TestData
        {
            public int number = 6;
        }

        [Serializable]
        class AlternativeTestData
        {
            public string txt = "not defined";
            public double[] arr;
        }
    }
}