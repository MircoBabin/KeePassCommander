using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace KeePassCommander.NamedPipeServer
{
    public class NamedPipeServerConnection
    {
        private DebugLog Debug;
        private NamedPipeServerStream Pipe;

        public delegate void OnFinishedCallback(NamedPipeServerConnection client);
        private OnFinishedCallback OnFinished;

        public NamedPipeServerConnection(DebugLog Debug, NamedPipeServerStream Pipe, OnFinishedCallback OnFinished)
        {
            this.Debug = Debug;
            this.Pipe = Pipe;
            this.OnFinished = OnFinished;
        }

        public void Close()
        {
            if (Pipe != null) Pipe.Close();
        }

        public void Run(Command.Runner runner)
        {
            try
            {
                StreamReader reader = new StreamReader(Pipe, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(Pipe, Encoding.UTF8);

                string command = reader.ReadLine();
                string[] parms = command.Split('\t');

                if (Debug.Enabled)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Received parameters:");
                    foreach (var parm in parms)
                    {
                        sb.AppendLine(parm);
                    }
                    Debug.OutputLine(sb.ToString());
                }

                writer.WriteLine(runner.Run(parms));
                writer.Flush();

                Pipe.Flush();
                Pipe.Close();
                Pipe.Dispose();
                Pipe = null;
            }
            catch (Exception ex)
            {
                Debug.OutputLine("RunClient Exception:" + Environment.NewLine + ex.ToString());
            }

            OnFinished(this);
        }
    }
}
