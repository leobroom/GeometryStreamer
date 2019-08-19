namespace GeoServer
{
    class Program
    {
        static void Main()
        {
            // Serialisation.Test();
            AsynchronousSocketListener.StartListening("127.0.0.1",12345);
        }
    }
}