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

                KeePassCommander.Encryption encryption = new KeePassCommander.Encryption();
                {
                    // Hello - settle a shared key for encryption
                    string command = reader.ReadLine();
                    string[] parms = command.Split('\t');

                    if (parms.Length < 2) throw new Exception("hello request invalid, should be 2 parts.");
                    if (parms[0] != "hello") throw new Exception("hello request invalid, first part should be \"hello\".");

                    encryption.SettleSharedKey(Convert.FromBase64String(parms[1]));

                    writer.WriteLine("hello\t" + Convert.ToBase64String(encryption.PublicKeyForSettlement) + "\t");
                    writer.Flush();
                    Pipe.Flush();
                }

                {
                    // Request - encrypted
                    string command = encryption.Decrypt(Convert.FromBase64String(reader.ReadLine()));
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

                    writer.WriteLine(Convert.ToBase64String(encryption.Encrypt(runner.Run(parms, null).ToString())));
                    writer.Flush();
                    Pipe.Flush();
                }

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
