using System;

namespace SocketStreamer
{
    public interface IClient
    {
        void Set(string ip, int port, string name, 
            ThreadingType taskType, int waitInMiliseconds, int clientType );
    }
    public class BaseClient : IClient 
    {
        protected string ip;
        protected int port;
        protected Guid id = Guid.Empty;
        protected string name;
        protected int clientType;
        protected bool useThreads = false;
        protected int waitInMiliseconds = 1000;

        public void Set(string ip, int port, string name, ThreadingType taskType, int waitInMiliseconds, int clientType = 1)
        {
            this.ip = ip;
            this.port = port;
            this.name = name;
            this.clientType = clientType;
            this.waitInMiliseconds = waitInMiliseconds;

            id = Guid.NewGuid();
            useThreads = (taskType == ThreadingType.Thread) ? true : false;
        }

        protected virtual void SendLog(string message) { }
    }
}