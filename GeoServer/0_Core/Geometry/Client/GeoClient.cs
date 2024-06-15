using System;
using System.Net.Sockets;

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

        public MessageArgs(string message) => this.message = message;

        public string Message => message;
    }

    public class GeoClient<T> : Client<T> where T : IClient, new()
    {
        public event EventHandler<MessageArgs> Message;

        public GeoClient() : base() => serializer = new GeoSerializer();

        public static T Initialize(string ip, int port, string name, ThreadingType tType, int waitInMiliseconds, ClientType clientType = ClientType.Default)
            => Initialize(ip, port, name, tType, waitInMiliseconds, (int)clientType);

        protected override void SendLog(string message) => Message?.Invoke(this, new MessageArgs(message));

        protected override void Deserialize(Socket client, int typeFromHeader, byte[] data)
        {
            var messageType = (MessageType)typeFromHeader;

            switch (messageType)
            {
                case MessageType.ConnectToServerMsg:
                    var connectToServer = serializer.DeserializeFromBytes<ConnectToServerMsg>(data);

                    Console.WriteLine(connectToServer);
                    break;
                case MessageType.SimpleMsg:
                    var simpleMsg = serializer.DeserializeFromBytes<SimpleMsg>(data);

                    GetSimpleMsg(simpleMsg);
                    break;
                case MessageType.BroadCastMsg:
                    var bc = serializer.DeserializeFromBytes<BroadCastMsg>(data);
                    SendLog("BroadCast: " + bc.broadcastMsg);
                    break;
                case MessageType.TestDataMsg:
                    var testData = serializer.DeserializeFromBytes<TestDataMsg>(data);
                    SendLog("Result1: " + testData.number);
                    break;
                case MessageType.AlternativeTestDataMsg:
                    var altTestData = serializer.DeserializeFromBytes<AlternativeTestDataMsg>(data);
                    Console.WriteLine("Result2: " + altTestData.txt);
                    serializer.LogArr(altTestData.arr);
                    break;
                case MessageType.BroadCastMesh:
                    SendLog("MessageType.BroadCastMesh: ");
                    var mesh = serializer.DeserializeFromBytes<BroadCastMesh>(data);
                    UpdateMesh(mesh);
                    break;
                case MessageType.BroadCastCurve:
                    var curves = serializer.DeserializeFromBytes<BroadCastCurve>(data);
                    UpdateCurves(curves);
                    break;
                case MessageType.BroadCastGeometryInfo:
                    SendLog("BroadCastGeometryInfo:" + data.Length);
                    var geoinfo = serializer.DeserializeFromBytes<BroadCastGeometryInfo>(data);
                    SendLog("Serializer läuft:" + geoinfo.meshesCount + geoinfo.curvesCount);
                    UpdateGeometry(geoinfo);
                    break;
                case MessageType.BroadCastIndex:
                    var index = serializer.DeserializeFromBytes<BroadCastIndex>(data);
                    UpdateIndex(index);
                    break;
                case MessageType.BroadCastText:
                    var txt = serializer.DeserializeFromBytes<BroadCastText>(data);
                    UpdateText(txt);
                    break;
                default:
                    throw new Exception($"Type: {typeFromHeader} ist nicht vorhanden!");
            }
        }

        protected virtual void UpdateText(BroadCastText text)
        {
            Console.WriteLine("UpdateText");
        }

        protected virtual void UpdateGeometry(BroadCastGeometryInfo geoinfo) => Console.WriteLine("Update Geometry");

        protected virtual void UpdateIndex(BroadCastIndex updateIdex) => Console.WriteLine("Update Index");

        protected override void StartSending(Socket socket)
        {
            ConnectToServer((ClientType)clientType, name, id);

            base.StartSending(socket);
        }

        private void ConnectToServer(ClientType deviceType, string clientName, Guid clientId)
        {
            ConnectToServerMsg data = new()
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
                case SimpleMsg.Msg.ServerKillMe:
                    throw new Exception("I am not Server can't kill anyone");
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
            //Serialisation.LogArr(curves.colors);
        }

        //protected override void SendHeaderStateToDebug(HeaderState state)
        //{
        //    SendLog($"STATEOBJ: ID: {id}, BufferLength: {state.buffer.Length}, HeaderType: {(MessageType)state.headerType}, DataSize: {state.dataSize}");
        //}

        public void SendingRandomData(int count)
        {
            Random rnd = new();

            // creates a number between 1 and 12
            for (int i = 0; i < count; i++)
            {
                int numb = rnd.Next(1, 12);

                //NEW STUFF
                AlternativeTestDataMsg testClass = new()
                {
                    txt = name,
                    arr = serializer.FillArr(numb)
                };

                Send(testClass);

                numb = rnd.Next(1, 200000000);

                TestDataMsg testClass2 = new() { number = numb };

                Send(testClass2);
            }
        }

        public void DoesDllWork() => SendLog("Geometry Dll is correctly loaded");

        public override void Disconnect()
        {
            Message = null;

            try
            {
                SimpleMsg killMe = new(){ message = SimpleMsg.Msg.ServerKillMe };
                Send(killMe);

                base.Disconnect();
            }
            catch (Exception e)
            {
                SendLog("Disconnect: " + e.Message);
            }
        }
    }
}