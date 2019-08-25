using System;

namespace GeoServer
{
    class Program
    {
        static void Main()
        {
            // Serialisation.Test();
    
            //Serialisation.Test();
            
            Console.WriteLine("#####################");
            AsynchronousSocketListener.StartListening("127.0.0.1",12345);


        }
    }
}