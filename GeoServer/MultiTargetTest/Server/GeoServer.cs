using System;
using System.Net.Sockets;
using SocketStreamer;

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
                    var connectToServer = serializer.DeserializeFromBytes<ConnectToServerMsg>(data);

                    ClientObject clientObject = socketToClientTable[client];
                    clientObject.Set(connectToServer.id, connectToServer.clientName, (int)connectToServer.deviceType);

                    ShowAllClients();

                    SimpleMsg allowToSend = new SimpleMsg()
                    { message = SimpleMsg.Msg.AllowClientToSendData };

                    Send(allowToSend, client, serverId);
                    break;

                case MessageType.BroadCastMsg:
                    var bc = serializer.DeserializeFromBytes<BroadCastMsg>(data);

                    SendToOthers(bc, client);
                    break;

                case MessageType.TestDataMsg:
                    var testData = serializer.DeserializeFromBytes<TestDataMsg>(data);
                    Console.WriteLine(testData.number);
                    break;

                case MessageType.AlternativeTestDataMsg:
                    var altTestData = serializer.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                    serializer.LogArr(altTestData.arr);
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
                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
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