
namespace SocketStreamer
{
    public interface IServer
    {
        void Set(string ip, int port);
    }

    public abstract class Server<T> : BaseServer where T : IServer, new()
    {
        private static T instance;

        protected Server() { }

        public static T Instance => instance;

        public static T Initialize(string ip, int port)
        {
            if (instance == null)
                instance = new T();

            instance.Set(ip, port);

            return instance;
        }
    }
}