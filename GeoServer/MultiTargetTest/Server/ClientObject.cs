using System;

namespace GeoStreamer
{
    class ClientObject
    {
        private Guid id = Guid.Empty;
        private string name = "";
        private ClientType deviceType = ClientType.NotSet;

        bool isSet = false;


        public Guid Id => id;
        public string Name => name;
        public ClientType DeviceType => deviceType;

        public override string ToString() => $"{name}, Id: {id} , ClientType: {deviceType}";

        /// <summary>
        /// Sets the initial Values 
        /// </summary>
        public void Set(Guid id, string name, ClientType deviceType)
        {
            if (isSet)
                throw new Exception("ClientObject already set!");

            this.id =id;
            this.name = name;
            this.deviceType = deviceType;

            isSet = true;
        }
    }
}