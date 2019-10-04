using System;
using System.Net.Sockets;

namespace GeoServer
{
    public partial class Client
    {
        private void Receive(Socket client)
        {
            try
            {
                HeaderState state = new HeaderState
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

        public void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket from the asynchronous state object.
            HeaderState state = (HeaderState)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead == 0)
                return;

            if (state.headerType == -1)
            {
                Utils.WriteHeaderState(state);

                SendLog(state.ToString());
            }

            if (bytesRead == state.dataSize)
            {
                Deserialize(handler, (MessageType)state.headerType, state.buffer);

                state = new HeaderState
                {
                    workSocket = handler,
                    buffer = new byte[Serialisation.HEADERSIZE]
                };
            }

            handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
        }

        public void Deserialize(Socket client, MessageType typeFromHeader, byte[] data)
        {
            Console.WriteLine("Deserialize");

            switch (typeFromHeader)
            {
                case MessageType.ConnectToServer:
                    var connectToServer = Serialisation.DeserializeFromBytes<ConnectToServerMsg>(data);

                    Console.WriteLine(connectToServer);
                    break;
                case MessageType.SimpleMsg:
                    var simpleMsg = Serialisation.DeserializeFromBytes<SimpleMsg>(data);

                    GetSimpleMsg(simpleMsg);
                    break;
                case MessageType.BroadCastTest:
                    var bc = Serialisation.DeserializeFromBytes<BroadCastMsg>(data);
                    SendLog("BroadCast: " + bc.broadcastMsg);
                    break;
                case MessageType.TestData:
                    var testData = Serialisation.DeserializeFromBytes<TestDataMsg>(data);
                    SendLog("Result1: " + testData.number);
                    break;
                case MessageType.AlternativeTestData:
                    var altTestData = Serialisation.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                    Console.WriteLine("Result2: " + altTestData.txt);
                    Serialisation.LogArr(altTestData.arr);
                    break;
                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
            }
        }

        private void GetSimpleMsg(SimpleMsg msg)
        {
            SendLog(msg);

            switch (msg.message)
            {
                case SimpleMsg.Msg.AllowClientToSendData:
                    allowSending = true;
                    SendLog("  allowSending = true");
                    break;
                default:
                    break;
            }
        }
    }

}