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
                StreamReader reader = new StreamReader(connection, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(connection, Encoding.UTF8);

                writer.WriteLine(Command);
                writer.Flush();
                connection.Flush();

                Response.ReadFromStream(reader);
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}
