using System;

namespace GeoServer
{
    public enum MessageType
    {
        NotSet = -1,
        None = 0,
        ConnectToServer = 1,
        SimpleMsg = 2,
        BroadCastTest = 3,
        TestData = 98,
        AlternativeTestData = 99
    }

    public interface ISerializableData { }

    [Serializable]
    public class TestDataMsg : ISerializableData
    {
        public int number = 6;
    }

    [Serializable]
    public class AlternativeTestDataMsg : ISerializableData
    {
        public string txt = "not defined";
        public double[] arr;
    }

    [Serializable]
    public class ConnectToServerMsg : ISerializableData
    {
        //0 = PC
        //1 = Hololens
        public ClientType deviceType = ClientType.Default;
        public string clientName = "defaultClient";
        public Guid id = Guid.Empty;

        public override string ToString()
            => $"ConnectToServerMsg: DeviceType: {deviceType}, ClientName: {clientName}, Id: {id}";
    }

    [Serializable]
    public class SimpleMsg : ISerializableData
    {
        public Msg message = Msg.None;
        public enum Msg
        {
            None = 0,
            AllowClientToSendData,
        }

        public override string ToString() => $"SimpleMsg:  {message}";
    }

    /// <summary>
    /// A Random BroadCastMessage, which a client can send to other clients over the server
    /// </summary>
    [Serializable]
    public class BroadCastMsg : ISerializableData
    {
        public string broadcastMsg = "not set";
    }
}