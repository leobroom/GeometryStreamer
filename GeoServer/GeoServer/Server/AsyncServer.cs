using GeoServer;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously
public class StateObject
{
    // Client  socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];

    // header
    public int headerType = -1;
    public int dataSize = -1;


    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousSocketListener
{
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener() { }

    public static void StartListening(string ip, int port)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Server started");


        // Establish the local endpoint for the socket.    
        IPAddress iPAdress = IPAddress.Parse(ip);
        IPEndPoint localEndPoint = new IPEndPoint(iPAdress, port);

        // Create a TCP/IP socket.
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.
        try
        {
            socket.Bind(localEndPoint);
            socket.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("Waiting for a connection...");
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.
        allDone.Set();

        // Get the socket that handles the client request.
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.
        StateObject state = new StateObject
        { workSocket = handler };

        state.buffer = new byte[Serialisation.HEADERSIZE];

        handler.BeginReceive(state.buffer, 0, Serialisation.HEADERSIZE, 0, new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead == 0)
            return;

        if (state.headerType == -1)
        {
            Console.WriteLine(" state.buffer: " + state.buffer.Length);

            byte[] headerBytes = state.buffer.Take(8).ToArray();
            int[] header = Serialisation.GetIntArrayFromByteArray(headerBytes);

            state.headerType = header[0];
            state.dataSize = header[1];

            Console.WriteLine(" state.headerType: " + state.headerType);
            Console.WriteLine(" state.dataSize: " + state.dataSize);

            state.buffer = new byte[header[1]];
        }


        if (bytesRead == state.dataSize)
        {
            Serialisation.Deserialize(state.headerType, state.buffer);

            state = new StateObject
            { workSocket = handler };
            state.buffer = new byte[Serialisation.HEADERSIZE];
        }
          





        //Console.WriteLine("bytesRead: " + bytesRead);

        //// There  might be more data, so store the data received so far.
        //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

        //// Check for end-of-file tag. If it is not there, read 
        //// more data.
        //content = state.sb.ToString();

        //if (content.IndexOf("<EOF>") > -1)
        //{
        //    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
        //        content.Length, content);
        //    // Echo the data back to the client.
        //    // Send(handler, content);

        //    state.sb.Clear();
        //}

        handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}