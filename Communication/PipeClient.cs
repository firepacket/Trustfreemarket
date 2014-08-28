using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Security.Principal;
using System.Diagnostics;
using System.Threading;

namespace AnarkRE.Communication
{
    public class PipeClient
    {
        //private static int numClients = 4;
        private const string pipename = "BTCNOTIFY";
        private const string banner = "BTCNOTIFY";

        public void SendData(string message)
        {
            NamedPipeClientStream pipeClient =
                        new NamedPipeClientStream(".", pipename,
                            PipeDirection.InOut, PipeOptions.None,
                            TokenImpersonationLevel.Impersonation);

            //Console.WriteLine("Connecting to server...\n");
            pipeClient.Connect();

            StreamString ss = new StreamString(pipeClient);
            // Validate the server's signature string 
            if (ss.ReadString() == banner)
            {
                // The client security token is sent with the first write. 
                // Send the name of the file whose contents are returned 
                // by the server.
                ss.WriteString(message);

                // Print the file to the screen.
                //Console.Write(ss.ReadString());
            }
            else
            {
                //Console.WriteLine("Server could not be verified.");
            }
            pipeClient.Close();
            // Give the client process some time to display results before exiting.
            //Thread.Sleep(4000);
        }

    
    }

    // Defines the data protocol for reading and writing strings on our stream 
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}