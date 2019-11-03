using System;
using SocketStreamer;

namespace GeoStreamer
{
    public enum MessageType
    {
        NotSet = -1,
        None = 0,
        ConnectToServer = 1,
        SimpleMsg = 2,
        BroadCastTest = 3,
        BroadCastMesh = 4,
        BroadCastCurves = 5,
        BroadCastGeometryInfo = 6,
        TestData = 98,
        AlternativeTestData = 99
    }

    public class GeoSerializer : Serializer
    {
        public override int GetMessageType(object d)
        {
            int type = 0;

            if (d is ConnectToServerMsg)
                type = (int)MessageType.ConnectToServer;
            else if (d is TestDataMsg)
                type = (int)MessageType.TestData;
            else if (d is AlternativeTestDataMsg)
                type = (int)MessageType.AlternativeTestData;
            else if (d is SimpleMsg)
                type = (int)MessageType.SimpleMsg;
            else if (d is BroadCastMsg)
                type = (int)MessageType.BroadCastTest;
            else if (d is BroadCastMesh)
                type = (int)MessageType.BroadCastMesh;
            else if (d is BroadCastCurve)
                type = (int)MessageType.BroadCastCurves;
            else if (d is BroadCastGeometryInfo)
                type = (int)MessageType.BroadCastGeometryInfo;
            else
                throw new Exception("MessageType is not added inside the GetHeader Method");

            return type;
        }
    }
}