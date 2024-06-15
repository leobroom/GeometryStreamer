using System;

namespace GeoStreamer
{
    public enum MessageType
    {
        NotSet = -1,
        None = 0,
        ConnectToServerMsg = 1,
        SimpleMsg = 2,
        BroadCastMsg = 3,
        BroadCastMesh = 4,
        BroadCastCurve = 5,
        BroadCastGeometryInfo = 6,
        BroadCastIndex = 7,
        BroadCastText = 8,
        TestDataMsg = 98,
        AlternativeTestDataMsg = 99
    }

    public class GeoSerializer : Serializer
    {
        public GeoSerializer()
        {
            AddMType(typeof(ConnectToServerMsg), MessageType.ConnectToServerMsg);
            AddMType(typeof(SimpleMsg), MessageType.SimpleMsg);
            AddMType(typeof(BroadCastMsg), MessageType.BroadCastMsg);
            AddMType(typeof(BroadCastMesh), MessageType.BroadCastMesh);
            AddMType(typeof(BroadCastCurve), MessageType.BroadCastCurve);
            AddMType(typeof(BroadCastGeometryInfo), MessageType.BroadCastGeometryInfo);
            AddMType(typeof(BroadCastIndex), MessageType.BroadCastIndex);
            AddMType(typeof(BroadCastText), MessageType.BroadCastText);
            AddMType(typeof(TestDataMsg), MessageType.TestDataMsg);
            AddMType(typeof(AlternativeTestDataMsg), MessageType.AlternativeTestDataMsg);
        }

        protected void AddMType(Type type, MessageType typeIdx) => AddMType(type, (int)typeIdx);
    }
}