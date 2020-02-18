﻿using System;

namespace SocketStreamer
{
    public interface IClient
    {
        void Set(string ip, int port, string name, 
            ThreadingType taskType, int clientType );
    }
    public class BaseClient : IClient 
    {
        protected string ip;
        protected int port;
        protected Guid id = Guid.Empty;
        protected string name;
        protected int clientType;
        protected bool useThreads = false;

        public void Set(string ip, int port, string name, ThreadingType taskType, int clientType = 1)
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