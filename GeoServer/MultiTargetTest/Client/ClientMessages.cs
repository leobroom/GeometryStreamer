using System;

namespace GeoStreamer
{
    public partial class Client<T> : BaseClient where T : IClient, new()
    {
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
    }
}
