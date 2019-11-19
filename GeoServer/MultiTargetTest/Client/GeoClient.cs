using System;
using System.Net.Sockets;
using SocketStreamer;

namespace GeoStreamer
{
    public enum ClientType
    {
        NotSet,
        Default,
        UWP
    }

    public class MessageArgs : EventArgs
    {
        private readonly string message;

        public MessageArgs(string message)
        {
            this.message = message;
        }

        public string Message => message;
    }

    public class GeoClient<T> : Client<T> where T : IClient, new()
    {
        public event EventHandler<MessageArgs> Message;

        public GeoClient() : base()
        {
            serializer = new GeoSerializer();
        }

        public static T Initialize(string ip, int port, string name, ThreadingType tType, ClientType clientType = ClientType.Default)
            => Initialize(ip, port, name, tType,(int)clientType);

        protected override void SendLog(string message)
        {
            Message?.Invoke(this, new MessageArgs(message));
        }

        protected override void Deserialize(Socket client, int typeFromHeader, byte[] data)
        {
            var messageType = (MessageType)typeFromHeader;

            switch (messageType)
            {
                case MessageType.ConnectToServer:
                    var connectToServer = serializer.DeserializeFromBytes<ConnectToServerMsg>(data);

                    Console.WriteLine(connectToServer);
                    break;
                case MessageType.SimpleMsg:
                    var simpleMsg = serializer.DeserializeFromBytes<SimpleMsg>(data);

                    GetSimpleMsg(simpleMsg);
                    break;
                case MessageType.BroadCastTest:
                    var bc = serializer.DeserializeFromBytes<BroadCastMsg>(data);
                    SendLog("BroadCast: " + bc.broadcastMsg);
                    break;
                case MessageType.TestData:
                    var testData = serializer.DeserializeFromBytes<TestDataMsg>(data);
                    SendLog("Result1: " + testData.number);
                    break;
                case MessageType.AlternativeTestData:
                    var altTestData = serializer.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                    Console.WriteLine("Result2: " + altTestData.txt);
                    serializer.LogArr(altTestData.arr);
                    break;
                case MessageType.BroadCastMesh:
                    var mesh = serializer.DeserializeFromBytes<BroadCastMesh>(data);
                    UpdateMesh(mesh);
                    break;
                case MessageType.BroadCastCurves:
                    var curves = serializer.DeserializeFromBytes<BroadCastCurve>(data);
                    UpdateCurves(curves);
                    break;
                case MessageType.BroadCastGeometryInfo:
                    var geoinfo = serializer.DeserializeFromBytes<BroadCastGeometryInfo>(data);
                    UpdateGeometry(geoinfo);
                    break;
                case MessageType.BroadCastIndex:
                    var index = serializer.DeserializeFromBytes<BroadCastIndex>(data);
                    UpdateIndex(index);
                    break;
                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
            }
        }

        protected virtual void UpdateGeometry(BroadCastGeometryInfo geoinfo)
        {
            Console.WriteLine("Update Geometry");
        }

        protected virtual void UpdateIndex(BroadCastIndex updateIdex)
        {
            Console.WriteLine("Update Index");
        }

        protected override void StartSending(Socket socket)
        {
            ConnectToServer((ClientType)clientType, name, id);

            base.StartSending(socket);
        }

        private void ConnectToServer(ClientType deviceType, string clientName, Guid clientId)
        {
            ConnectToServerMsg data = new ConnectToServerMsg()
            {
                clientName = clientName,
                deviceType = deviceType,
                id = clientId
            };

            Send(data);
        }

        private void GetSimpleMsg(SimpleMsg msg)
        {
            SendLog(msg.ToString());

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

        protected virtual void UpdateMesh(BroadCastMesh mesh)
        {
            Console.WriteLine("Mesh recieved: " + mesh.id);
            //Serialisation.LogArr(mesh.normals);
            //Serialisation.LogArr(mesh.triangles);
            //Serialisation.LogArr(mesh.vertices);
        }

        protected virtual void UpdateCurves(BroadCastCurve curve)
        {
            Console.WriteLine("Curves recieved:");
            //Serialisation.LogArr(curves.length);
            //Serialisation.LogArr(curves.positions);
            //   Serialisation.LogArr(curves.colors);
        }

        public void SendingRandomData(int count)
        {
            Random rnd = new Random();

            // creates a number between 1 and 12
            for (int i = 0; i < count; i++)
            {
                int numb = rnd.Next(1, 12);

                //NEW STUFF
                AlternativeTestDataMsg testClass = new AlternativeTestDataMsg
                {
                    txt = name,
                    arr = serializer.FillArr(numb)
                };

                Send(testClass);

                numb = rnd.Next(1, 200000000);

                TestDataMsg testClass2 = new TestDataMsg
                { number = numb };

                Send(testClass2);
            }
        }

        public void DoesDllWork()
        {
            SendLog("Geometry Dll is correctly loaded");
        }

        public override void Disconnect()
        {
            Message = null;

            try
            {
                base.Disconnect();
            }
            catch (Exception e)
            {
                SendLog("Disconnect: "+ e.Message);
            }
    
        }
    }
}