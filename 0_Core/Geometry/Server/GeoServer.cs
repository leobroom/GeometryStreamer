using System;
using System.Net.Sockets;

namespace GeoStreamer
{
    public class GeoServer : Server<GeoServer>
    {
        public GeoServer() : base()
        {
            serializer = new GeoSerializer();
        }

        protected override void Deserialize(Socket client, int typeFromHeader, byte[] data)
        {
            var messageType = (MessageType)typeFromHeader;

            switch (messageType)
            {
                case MessageType.ConnectToServerMsg:
                    var connectToServer = Serializer.DeserializeFromBytes<ConnectToServerMsg>(data);

                    ClientObject clientObject = socketToClientTable[client];
                    clientObject.Set(connectToServer.id, connectToServer.clientName, (int)connectToServer.deviceType);

                    ShowAllClients();

                    SimpleMsg allowToSend = new SimpleMsg()
                    { message = SimpleMsg.Msg.AllowClientToSendData };

                    Send(allowToSend, client, serverId);
                    break;

                case MessageType.BroadCastMsg:
                    var bc = Serializer.DeserializeFromBytes<BroadCastMsg>(data);

                    SendToOthers(bc, client);
                    break;

                case MessageType.TestDataMsg:
                    var testData = Serializer.DeserializeFromBytes<TestDataMsg>(data);
                    Console.WriteLine(testData.number);
                    break;

                case MessageType.AlternativeTestDataMsg:
                    var altTestData = Serializer.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                    Serializer.LogArr(altTestData.arr);
                    break;

                case MessageType.BroadCastMesh:
                    SendToOthers(data, (int)typeFromHeader, client);
                    break;
                case MessageType.BroadCastCurve:
                    SendToOthers(data, (int)typeFromHeader, client);
                    break;
                case MessageType.BroadCastGeometryInfo:
                    SendToOthers(data, (int)typeFromHeader, client);
                    break;
                case MessageType.BroadCastIndex:
                    SendToOthers(data, (int)typeFromHeader, client);
                    break;
                case MessageType.BroadCastText:
                    SendToOthers(data, (int)typeFromHeader, client);
                    break;
                case MessageType.SimpleMsg:
                    var simMsgData = Serializer.DeserializeFromBytes<SimpleMsg>(data);

                    if (simMsgData.message == SimpleMsg.Msg.ServerKillMe) 
                    {
                        Console.WriteLine("SimpleMsg.Msg.ServerKillMe, try to kill socket...");
                        RemoveClient(client);
                    }
                    break;
                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
                    //Console.WriteLine($"Type: {typeFromHeader} ist nicht vorhanden!");
                    //break;
                    //throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
            }
        }

        /// <summary>
        /// Just for Debug purpose - shows all the clients in the database
        /// </summary>
        private void ShowAllClients()
        {
            Console.WriteLine(Environment.NewLine + "############CLIENT LIST##############");
            foreach (var clientObject in socketToClientTable.Values)
                Console.WriteLine(clientObject);
            Console.WriteLine("#####################################" + System.Environment.NewLine);
        }
    }
}