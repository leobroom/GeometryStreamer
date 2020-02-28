using SocketStreamer;
using System;

namespace GeoStreamer
{
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
            AllowClientToSendData =1,
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

    [Serializable]
    public class BroadCastMesh : ISerializableData
    {
        public int id = -1;
        public int meshNr = -1;
        public float[] vertices;
        public float[] normals;
        public int[] triangles;
        public byte[] color;
    }

    [Serializable]
    public class BroadCastGeometryInfo : ISerializableData
    {
        public int curvesCount = -1;
        public int meshesCount = -1;
        public int textCount = -1;
    }

    [Serializable]
    public class BroadCastCurve : ISerializableData
    {
        public int id = -1;
        public int curveNr = -1;
        public float[] positions;
        /// <summary>
        /// R,G,B,A...
        /// </summary>
        public byte[] colors;
        public float width;
    }

    [Serializable]
    public class BroadCastText : ISerializableData
    {
        public int id = -1;
        public int textNr = -1;
        public float[] position;
        public float[] rotation;
        public byte[] color;
        public string text;
        public int textSize;
    }

    [Serializable]
    public class BroadCastIndex : ISerializableData
    {
        public int gateId = -1;
        public int index = -999;
    }
}