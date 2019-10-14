using System;
using System.Collections.Generic;
using System.Text;

namespace GeoStreamer
{
    public interface IClient
    {
        void Set(string ip, int port, string name, 
            ThreadingType taskType, ClientType clientType );
    }
    public class BaseClient : IClient 
    {
        protected string ip;
        protected int port;
        protected Guid id = Guid.Empty;
        protected string name;
        protected ClientType clientType;
        protected bool useThreads = false;

        public void Set(string ip, int port, string name, ThreadingType taskType, ClientType clientType = ClientType.Default)
        {
            this.ip = ip;
            this.port = port;
            this.name = name;
            this.clientType = clientType;

            id = Guid.NewGuid();
            useThreads = (taskType == ThreadingType.Thread) ? true : false;
        }

        protected virtual void SendLog(string message) { }
    }
}
