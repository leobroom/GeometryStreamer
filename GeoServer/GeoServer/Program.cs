using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace GeoServer
{
    class Program
    {
        static void Main()
        {
            int count;
            byte[] byteArray;
            char[] charArray;
            UnicodeEncoding uniEncoding = new UnicodeEncoding();

            // Create the data to write to the stream.
            byte[] firstString = uniEncoding.GetBytes(
                "Invalid file path characters are: ");
            byte[] secondString = uniEncoding.GetBytes(
                Path.GetInvalidPathChars());

            using (MemoryStream memStream = new MemoryStream(100))
            {
                // Write the first string to the stream.
                memStream.Write(firstString, 0, firstString.Length);

                // Write the second string to the stream, byte by byte.
                count = 0;
                while (count < secondString.Length)
                {
                    memStream.WriteByte(secondString[count++]);
                }

                // Write the stream properties to the console.
                Console.WriteLine(
                    "Capacity = {0}, Length = {1}, Position = {2}\n",
                    memStream.Capacity.ToString(),
                    memStream.Length.ToString(),
                    memStream.Position.ToString());

                // Set the position to the beginning of the stream.
                memStream.Seek(0, SeekOrigin.Begin);

                // Read the first 20 bytes from the stream.
                byteArray = new byte[memStream.Length];
                count = memStream.Read(byteArray, 0, 20);

                // Read the remaining bytes, byte by byte.
                while (count < memStream.Length)
                {
                    byteArray[count++] =
                        Convert.ToByte(memStream.ReadByte());
                }

                // Decode the byte array into a char array
                // and write it to the console.
                charArray = new char[uniEncoding.GetCharCount(
                    byteArray, 0, count)];
                uniEncoding.GetDecoder().GetChars(
                    byteArray, 0, count, charArray, 0);
                Console.WriteLine(charArray);
                Console.ReadKey();
            }
        }



        //static private TestData SerializeData()
        //{
        //    //-------write to database-------------------------
        //    //TestData test = new TestData();
        //    //test.number = 8;

        //    //MemoryStream memorystream = new MemoryStream();
        //    //BinaryFormatter bf = new BinaryFormatter();
        //    //bf.Serialize(memorystream, test);
        //    //byte[] yourBytesToDb = memorystream.ToArray();
        //    ////here you write yourBytesToDb to database


        //    ////----------read from database---------------------
        //    ////here you read from database binary data into yourBytesFromDb
        //    //MemoryStream memorystreamd = new MemoryStream(yourBytesToDb);

        //    //BinaryFormatter bfd = new BinaryFormatter();


        //    //TestData deserializedData = bfd.Deserialize(memorystreamd) as TestData;

        //    return deserializedData;
        //}

        private static byte[] GenerateHeader(int type, int length)
        {
            int[] header = new int[2];

            //int length = serializedObj.Length;
            header[0] = type;
            header[1] = length;

            byte[] result = new byte[2 * 4];
            Buffer.BlockCopy(header, 0, result, 0, result.Length);

            return result;
        }
    }

    [Serializable]
    class TestData
    {
        public int number = 6;
    }

    [Serializable]
    class Header
    {
        public int msgType = 1;
        public int bla;
    }
}