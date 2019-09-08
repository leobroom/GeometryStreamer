using System;
using System.Net.Sockets;
using System.Text;

namespace GeoServer
{
    public partial class Client
    {
        private static String response = String.Empty;

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject
                { workSocket = client };

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("ReceiveCallback");
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                String content = String.Empty;
                content = state.sb.ToString();

                if (bytesRead > 0)
                {
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content);
       
                        state.sb.Clear();
                    }

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,new AsyncCallback(ReceiveCallback), state);
             
                }
                else
                {
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
