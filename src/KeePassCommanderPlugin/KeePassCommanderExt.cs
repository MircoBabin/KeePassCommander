using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace KeePassCommander
{
    public sealed class KeePassCommanderExt : Plugin
    {
        private IPluginHost KeePassHost = null;
        private const string EndOfResponse = "\t\t\t[--- end of response ---]\t\t\t";

        private string ServerPipeName;

        public override bool Initialize(IPluginHost host)
        {
            Terminate();

            ServerPipeName = "KeePassCommander." + Process.GetCurrentProcess().SessionId;
            KeePassHost = host;
            StartServer();
            return true;
        }

        public override void Terminate()
        {
            if (KeePassHost == null) return;

            StopServer();
            KeePassHost = null;
        }

        private void StartServer()
        {
            Thread ServerThread = new Thread(ThreadStartServer);
            ServerThread.Start(this);
        }

        private void StopServer()
        {
            Stop = true;

            lock (ServerLock)
            {
                try
                {
                    using (NamedPipeClientStream npcs = new NamedPipeClientStream(".", ServerPipeName, PipeDirection.InOut, PipeOptions.None))
                    {
                        npcs.Connect(100);
                    }
                }
                catch { }

                foreach (ServerClient parm in ServerClients)
                {
                    try
                    {
                        parm.Pipe.Close();
                    }
                    catch { }
                }
            }
        }

        private object ServerLock = new object();
        private Boolean Stop = false;
        private NamedPipeServerStream ServerPipe = null;
        private List<ServerClient> ServerClients = new List<ServerClient>();

        private static void ThreadStartServer(object data)
        {
            KeePassCommanderExt me = data as KeePassCommanderExt;
            if (me != null)
                try
                {
                    me.RunServer();
                }
                catch { }
        }

        private void RunServer()
        {
            if (Stop) return;
            lock (ServerLock)
            {
                if (Stop) return;

                ServerPipe = new NamedPipeServerStream(ServerPipeName,
                    PipeDirection.InOut,
                    10,
                    PipeTransmissionMode.Byte,
                    PipeOptions.None);
            }

            try
            {
                ServerPipe.WaitForConnection();

                if (Stop)
                {
                    ServerPipe.Dispose();
                    ServerPipe = null;
                    return;
                }
                lock (ServerLock)
                {
                    if (Stop)
                    {
                        ServerPipe.Dispose();
                        ServerPipe = null;
                        return;
                    }

                    ServerClient client = new ServerClient(ServerPipe);
                    ServerClients.Add(client);

                    ServerPipe = null;
                    StartServer();
                    RunClient(client);
                }
            }
            catch { }
        }

        private class ServerClient
        {
            public NamedPipeServerStream Pipe { get; private set; }

            public ServerClient(NamedPipeServerStream Pipe)
            {
                this.Pipe = Pipe;
            }
        }

        private void RunClient(ServerClient client)
        {
            try
            {
                StreamReader reader = new StreamReader(client.Pipe, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(client.Pipe, Encoding.UTF8);

                string output = String.Empty;
                string command = reader.ReadLine();
                string[] parms = command.Split('\t');
                if (parms.Length > 0)
                {
                    if (parms[0] == "get")
                        output = CommandGet(parms);
                }

                writer.WriteLine(output);
                writer.WriteLine(EndOfResponse);
                writer.Flush();

                client.Pipe.Flush();
                client.Pipe.Close();
                client.Pipe.Dispose();
            }
            catch { }

            if (Stop) return;
            lock (ServerLock)
            {
                if (Stop) return;

                ServerClients.Remove(client);
            }
        }

        private string CommandGet(string[] parms)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();

            for (int i = 1; i < parms.Length; i++)
            {
                string name = parms[i].Trim();
                if (name.Length > 0)
                {
                    names.Add(name, String.Empty);
                }
            }

            if (names.Count > 0)
            {
                foreach (var doc in KeePassHost.MainWindow.DocumentManager.Documents)
                {
                    PwDatabase db = doc.Database;

                    if (db.IsOpen)
                    {
                        var items = db.RootGroup.GetObjects(true, true);
                        foreach (var item in items)
                        {
                            if (item is PwEntry)
                            {
                                PwEntry entry = item as PwEntry;

                                string title = entry.Strings.ReadSafe("Title");
                                if (names.ContainsKey(title))
                                {
                                    string url = entry.Strings.ReadSafe("URL");
                                    string urlscheme = String.Empty;
                                    string urlhost = String.Empty;
                                    string urlport = String.Empty;
                                    string urlpath = String.Empty;
                                    try
                                    {
                                        Uri uri = new Uri(url);
                                        urlscheme = uri.Scheme;
                                        urlhost = uri.Host;
                                        if (uri.Port != -1) urlport = uri.Port.ToString();
                                        urlpath = uri.AbsolutePath;
                                    }
                                    catch { }

                                    names[title] = title + "\t" +
                                                   entry.Strings.ReadSafe("UserName") + "\t" +
                                                   entry.Strings.ReadSafe("Password") + "\t" +
                                                   url + "\t" +
                                                   urlscheme + "\t" +
                                                   urlhost + "\t" +
                                                   urlport + "\t" +
                                                   urlpath + "\t" +
                                                   Convert.ToBase64String(Encoding.UTF8.GetBytes(entry.Strings.ReadSafe("Notes"))) + "\t";
                                }
                            }
                        }
                    }
                }
            }

            StringBuilder result = new StringBuilder();
            foreach (var keypair in names)
            {
                string value = keypair.Value;
                if (value != String.Empty)
                {
                    result.AppendLine(value);
                }
            }

            return result.ToString();
        }
    }
}
