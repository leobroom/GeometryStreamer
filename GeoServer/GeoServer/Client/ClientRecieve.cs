using System;
using System.Net.Sockets;

namespace GeoServer
{
    public partial class Client
    {
        private static void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject
                {
                    workSocket = client,
                    buffer = new byte[Serialisation.HEADERSIZE]
                };

                client.BeginReceive(state.buffer, 0, Serialisation.HEADERSIZE, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead == 0)
                return;

            if (state.headerType == -1)
            {
                Utils.UpdateStateObject(state);

                Console.WriteLine(" state.buffer: " + state.buffer.Length);
                Console.WriteLine(" state.headerType: " + state.headerType);
                Console.WriteLine(" state.dataSize: " + state.dataSize);
                Console.WriteLine(" state.id: " + state.id);
            }

            Console.WriteLine("bytesRead: " + bytesRead);
            Console.WriteLine("state.dataSize: " + state.dataSize);

            if (bytesRead == state.dataSize)
            {
                Deserialize(handler, state.headerType, state.buffer);

                state = new StateObject
                {
                    workSocket = handler,
                    buffer = new byte[Serialisation.HEADERSIZE]
                };
            }

            handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void Deserialize(Socket client, int typeFromHeader, byte[] data)
        {
            Console.WriteLine("Deserialize");

            switch (typeFromHeader)
            {
                case 1:
                    var connectToServer = Serialisation.DeserializeFromBytes<ConnectToServerMsg>(data);

                    Console.WriteLine("NACHRICHT ZURÜCK - TESTEN OB ALLES GEHT! ");
                    break;
                case 98:
                    var testData = Serialisation.DeserializeFromBytes<TestDataMsg>(data);
                    Console.WriteLine("Result1: " + testData.number);
                    break;
                case 99:
                    var altTestData = Serialisation.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                    Console.WriteLine("Result2: " + altTestData.txt);
                    Serialisation.LogArr(altTestData.arr);
                    break;
                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
            }
        }
        //private static void ReceiveCallback(IAsyncResult ar)
        //{
        //    Console.WriteLine("ReceiveCallback");
        //    try
        //    {
        //        // Retrieve the state object and the client socket   
        //        // from the asynchronous state object.  
        //        StateObject state = (StateObject)ar.AsyncState;
        //        Socket client = state.workSocket;

        //        // Read data from the remote device.  
        //        int bytesRead = client.EndReceive(ar);

        //        // There  might be more data, so store the data received so far.
        //        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

        //        String content = String.Empty;
        //        content = state.sb.ToString();

        //        if (bytesRead > 0)
        //        {
        //            if (content.IndexOf("<EOF>") > -1)
        //            {
        //                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
        //                    content.Length, content);

        //                state.sb.Clear();
        //            }

        //            // Get the rest of the data.  
        //            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,new AsyncCallback(ReceiveCallback), state);        
        //        }
        //        else
        //        {
        //            receiveDone.Set();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}
    }
}
