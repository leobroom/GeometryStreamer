using System;


namespace GeoStreamer
{
    public class ClientObject
    {
        private Guid id = Guid.Empty;
        private string name = "";
        private int clientType = 0;

        bool isSet = false;


        public Guid Id => id;
        public string Name => name;
        public int ClientType => clientType;



        private bool stopThread = false;

        public bool StopThread
        {
            get { return stopThread; }
            set { stopThread = value; }
        }

        public override string ToString() => $"{name}, Id: {id} , ClientType: {clientType}";

        /// <summary>
        /// Sets the initial Values 
        /// </summary>
        public void Set(Guid id, string name, int clientType)
        {
            //if (isSet)
            //throw new Exception("ClientObject already set!");

            this.id = id;
            this.name = name;
            this.clientType = clientType;

            isSet = true;
        }
    }
}