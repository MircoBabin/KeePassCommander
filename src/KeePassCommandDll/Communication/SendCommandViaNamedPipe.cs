using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace KeePassCommandDll.Communication
{
    public class SendCommandViaNamedPipe : ISendCommand
    {
        private string ServerPipeName;
        private string Command;

        public Response Response { get; private set; }

        public SendCommandViaNamedPipe(string command)
        {
            Response = new Response();
            ServerPipeName = "KeePassCommander." + Process.GetCurrentProcess().SessionId;
            Command = command;

            Execute();
        }

        private void Execute()
        {
            NamedPipeClientStream connection = new NamedPipeClientStream(".", ServerPipeName, PipeDirection.InOut, PipeOptions.None);
            try
            {
                connection.Connect(5000);

                StreamReader pipeReader = new StreamReader(connection, Encoding.UTF8);
                StreamWriter pipeWriter = new StreamWriter(connection, Encoding.UTF8);

                KeePassCommander.Encryption encryption = new KeePassCommander.Encryption();
                {
                    // Hello - settle a shared key for encryption
                    pipeWriter.WriteLine("hello\t" + Convert.ToBase64String(encryption.PublicKeyForSettlement)+"\t");
                    pipeWriter.Flush();
                    connection.Flush();

                    string helloResponse = pipeReader.ReadLine();
                    var parms = helloResponse.Split('\t');
                    if (parms.Length < 2) throw new Exception("hello response invalid, should be 2 parts.");
                    if (parms[0] != "hello") throw new Exception("hello response invalid, first part should be \"hello\".");

                    encryption.SettleSharedKey(Convert.FromBase64String(parms[1]));
                }

                {
                    // Request - encrypted
                    pipeWriter.WriteLine(Convert.ToBase64String(encryption.Encrypt(Command)));
                    pipeWriter.Flush();
                    connection.Flush();

                    string response = encryption.Decrypt(Convert.FromBase64String(pipeReader.ReadLine().Trim()));
                    Response.ReadFromStream(response);
                }
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}
