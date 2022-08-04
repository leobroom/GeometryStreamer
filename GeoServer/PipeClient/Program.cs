
using PipeCore;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;


Thread clientReadThread;
Thread clientWriteThread;

Console.WriteLine("THE CLIENT");


clientWriteThread = new Thread(ClientThread_Read);
clientWriteThread.Start();


Console.WriteLine("THE END");
//if (args.Length > 0)
//{
//    if (args[0] == "spawnclient")
//    {
//        var pipeClient =
//            new NamedPipeClientStream(".", "testpipe",
//                PipeDirection.InOut, PipeOptions.None,
//                TokenImpersonationLevel.Impersonation);

//        Console.WriteLine("Connecting to server...\n");
//        pipeClient.Connect();

//        var ss = new StreamString(pipeClient);

//        // Validate the server's signature string.
//        if (ss.ReadString() == "I am the one true server!")
//        {
//            // The client security token is sent with the first write.
//            // Send the name of the file whose contents are returned
//            // by the server.
//            ss.WriteString("c:\\PipeTest\\textfile.txt");

//            // Print the file to the screen.
//            Console.Write(ss.ReadString());
//        }
//        else
//        {
//            Console.WriteLine("Server could not be verified.");
//        }

//        pipeClient.Close();

//        // Give the client process some time to display results before exiting.
//        Thread.Sleep(4000);
//    }
//}
//else
//{
//    Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");
//    StartClients();
//}

public partial class Program
{
    static void ClientThread_Read()
    {
        NamedPipeClientStream namedPipeClientStream =
            new NamedPipeClientStream(".", "ServerRead_ClientWrite", PipeDirection.Out);

        namedPipeClientStream.Connect();
        Console.WriteLine("client connected ");

        StreamString streamString = new StreamString(namedPipeClientStream);
        streamString.WriteString("blubb");

        namedPipeClientStream.Close();
    }



    //private static int numClients = 4;

    // Helper function to create pipe client processes
    //static void StartClients()
    //{
    //    string currentProcessName = Environment.CommandLine;

    //    // Remove extra characters when launched from Visual Studio
    //    currentProcessName = currentProcessName.Trim('"', ' ');

    //    currentProcessName = Path.ChangeExtension(currentProcessName, ".exe");
    //    Process[] plist = new Process[numClients];

    //    Console.WriteLine("Spawning client processes...\n");

    //    if (currentProcessName.Contains(Environment.CurrentDirectory))
    //    {
    //        currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, String.Empty);
    //    }

    //    // Remove extra characters when launched from Visual Studio
    //    currentProcessName = currentProcessName.Replace("\\", String.Empty);
    //    currentProcessName = currentProcessName.Replace("\"", String.Empty);

    //    int i;
    //    for (i = 0; i < numClients; i++)
    //    {
    //        // Start 'this' program but spawn a named pipe client.
    //        plist[i] = Process.Start(currentProcessName, "spawnclient");
    //    }
    //    while (i > 0)
    //    {
    //        for (int j = 0; j < numClients; j++)
    //        {
    //            if (plist[j] != null)
    //            {
    //                if (plist[j].HasExited)
    //                {
    //                    Console.WriteLine($"Client process[{plist[j].Id}] has exited.");
    //                    plist[j] = null;
    //                    i--;    // decrement the process watch count
    //                }
    //                else
    //                {
    //                    Thread.Sleep(250);
    //                }
    //            }
    //        }
    //    }
    //    Console.WriteLine("\nClient processes finished, exiting.");
    //}
}