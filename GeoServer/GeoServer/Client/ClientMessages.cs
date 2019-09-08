using System;

namespace GeoServer
{
    public partial class Client
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
