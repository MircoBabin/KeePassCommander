using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KeePassCommander
{
    public sealed class KeePassCommanderExt : Plugin
    {
        public override string UpdateUrl { get { return "https://github.com/MircoBabin/KeePassCommander/releases/latest/download/keepass.plugin.version.txt"; } }

        private IPluginHost KeePassHost = null;
        private const string BeginOfResponse = "\t\t\t[--- begin of response 3.0 ---]\t\t\t";
        private const string EndOfResponse = "\t\t\t[--- end of response 3.0 ---]\t\t\t";

        private string ServerPipeName;

        private readonly DebugLog Debug = new DebugLog();

        public override bool Initialize(IPluginHost host)
        {
            Terminate();

            ServerPipeName = "KeePassCommander." + Process.GetCurrentProcess().SessionId;
            KeePassHost = host;

            //KeePass.exe --debug --KeePassCommanderDebug=c:\incoming\KeePassCommander.log
            string debugFileName = host.CommandLineArgs["KeePassCommanderDebug"];
            try
            { 
                Debug.Initialize(debugFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("KeePassCommander debug logger failed to initialise. No logging will be performed until KeePass is restarted with a valid debug log file location. Reason: " + ex.ToString());
            }


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
            if (Stop)
            {
                Debug.OutputLine("Not starting Named Pipe Server, Stop=true (1)");
                return;
            }

            lock (ServerLock)
            {
                if (Stop)
                {
                    Debug.OutputLine("Not starting Named Pipe Server, Stop=true (2)");
                    return;
                }

                try
                {
                    Debug.OutputLine("Starting Named Pipe Server on \"" + ServerPipeName + "\"");
                    ServerPipe = new NamedPipeServerStream(ServerPipeName,
                        PipeDirection.InOut,
                        10,
                        PipeTransmissionMode.Byte,
                        PipeOptions.None);
                    Debug.OutputLine("Named Pipe Server started");
                }
                catch (Exception ex)
                {
                    Debug.OutputLine("Starting Named Pipe Server failed" + Environment.NewLine + ex.ToString());
                    Stop = true;
                    return;
                }
            }

            try
            {
                Debug.OutputLine("Waiting for connection");
                ServerPipe.WaitForConnection();
                Debug.OutputLine("Connection received");

                if (Stop)
                {
                    Debug.OutputLine("Ending connection, Stop=true (3)");
                    ServerPipe.Dispose();
                    ServerPipe = null;
                    return;
                }

                ServerClient client;
                lock (ServerLock)
                {
                    if (Stop)
                    {
                        Debug.OutputLine("Ending connection, Stop=true (4)");
                        ServerPipe.Dispose();
                        ServerPipe = null;
                        return;
                    }

                    client = new ServerClient(ServerPipe);
                    ServerClients.Add(client);
                }

                Debug.OutputLine("Restart listening on named pipe");
                ServerPipe = null;
                StartServer();

                Debug.OutputLine("Starting run client");
                RunClient(client);
                Debug.OutputLine("Ended run client");
            }
            catch (Exception ex)
            {
                Debug.OutputLine("RunServer Exception:" + Environment.NewLine + ex.ToString());
            }
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

                if (parms.Length > 0)
                {
                    if (parms[0] == "get")
                        output = CommandGet(parms);
                    else if (parms[0] == "getfield")
                        output = CommandGetField(parms);
                    else if (parms[0] == "getattachment")
                        output = CommandGetAttachment(parms);
                    else if (parms[0] == "getnote")
                        output = CommandGetNote(parms);
                    else if (parms[0] == "listgroup")
                        output = CommandListGroup(parms);
                }

                writer.WriteLine(output);
                writer.WriteLine(EndOfResponse);
                writer.Flush();

                client.Pipe.Flush();
                client.Pipe.Close();
                client.Pipe.Dispose();
            }
            catch (Exception ex)
            {
                Debug.OutputLine("RunClient Exception:" + Environment.NewLine + ex.ToString());
            }

            if (Stop) return;
            lock (ServerLock)
            {
                if (Stop) return;

                ServerClients.Remove(client);
            }
        }

        private string GetEntryField(PwEntry entry, string FieldName)
        {
            const string StrRefStart = @"{REF:";
            const string StrRefEnd = @"}";
            const StringComparison ScMethod = StringComparison.OrdinalIgnoreCase;

            string input = entry.Strings.ReadSafe(FieldName);

            //replace field references
            //See also KeePass source SprEngine.cs function FillRefPlaceholders
            if (input.IndexOf(StrRefStart, 0, ScMethod) < 0)
            {
                //nothing to do
                return input;
            }

            KeePass.Util.Spr.SprContext ctx = new KeePass.Util.Spr.SprContext(entry,
                KeePassHost.MainWindow.DocumentManager.SafeFindContainerOf(entry),
                KeePass.Util.Spr.SprCompileFlags.Deref);


            int nOffset = 0;
            int maxTries = 100;
            while (true)
            {
                int nStart = input.IndexOf(StrRefStart, nOffset, ScMethod);
                if (nStart < 0) break;
                int nEnd = input.IndexOf(StrRefEnd, nStart + 1, ScMethod);
                if (nEnd <= nStart) break;

                string strFullRef = input.Substring(nStart, nEnd - nStart + 1);
                char chScan, chWanted;
                PwEntry peFound = KeePass.Util.Spr.SprEngine.FindRefTarget(strFullRef, ctx, out chScan, out chWanted);

                if (peFound != null)
                {
                    string strInsData;
                    if (chWanted == 'T')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.TitleField);
                    else if (chWanted == 'U')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.UserNameField);
                    else if (chWanted == 'A')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.UrlField);
                    else if (chWanted == 'P')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.PasswordField);
                    else if (chWanted == 'N')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.NotesField);
                    else if (chWanted == 'I')
                        strInsData = peFound.Uuid.ToHexString();
                    else { nOffset = nStart + 1; continue; }

                    input = input.Substring(0, nStart) + strInsData + input.Substring(nEnd + StrRefEnd.Length);
                    maxTries--;
                    if (maxTries <= 0) break;
                }
                else { nOffset = nStart + 1; continue; }
            }

            return input;
        }

        private void FindTitles(Dictionary<string, List<PwEntry>> search)
        {
            Debug.OutputLine("Starting FindTitles");
            if (search.Count == 0)
            {
                Debug.OutputLine("Ended FindTitles, nothing to search");
                return;
            }

            foreach (var doc in KeePassHost.MainWindow.DocumentManager.Documents)
            {
                PwDatabase db = doc.Database;

                if (db.IsOpen)
                {
                    Debug.OutputLine("    Database: " + db.Name);

                    var items = db.RootGroup.GetObjects(true, true);
                    foreach (var item in items)
                    {
                        if (item is PwEntry)
                        {
                            PwEntry entry = item as PwEntry;

                            string title = GetEntryField(entry, PwDefs.TitleField);
                            if (search.ContainsKey(title))
                            {
                                search[title].Add(entry);
                            }
                        }
                    }
                }
            }

            Debug.OutputLine("Ended FindTitles");
        }

        private void FindTitle(string title, Dictionary<string, List<PwEntry>> found)
        {
            var titletofind = new Dictionary<string, List<PwEntry>>();
            titletofind.Add(title, new List<PwEntry>());
            FindTitles(titletofind);

            List<PwEntry> foundEntries;
            if (found.ContainsKey(title))
            {
                foundEntries = found[title];
            }
            else
            {
                foundEntries = new List<PwEntry>();
                found.Add(title, foundEntries);
            }

            foundEntries.AddRange(titletofind[title]);
        }

        private void FindTitlesInGroup(PwEntry groupEntry, Dictionary<string, List<PwEntry>> found)
        {
            Debug.OutputLine("Starting FindTitlesInGroup");
            if (groupEntry == null)
            {
                Debug.OutputLine("Ended FindTitlesInGroup, nothing to search");
                return;
            }

            PwGroup group = groupEntry.ParentGroup;
            if (group.Entries != null)
            {
                foreach (var entry in group.Entries)
                {
                    if (entry != groupEntry)
                    {
                        string title = GetEntryField(entry, PwDefs.TitleField);

                        List<PwEntry> foundEntries;
                        if (found.ContainsKey(title))
                        {
                            foundEntries = found[title];
                        }
                        else
                        {
                            foundEntries = new List<PwEntry>();
                            found.Add(title, foundEntries);
                        }

                        foundEntries.Add(entry);
                    }
                }
            }

            Debug.OutputLine("Ended FindTitlesInGroup");
        }


        private string CommandGet(string[] parms)
        {
            Debug.OutputLine("Starting command get");

            StringBuilder result = new StringBuilder();
            result.AppendLine(BeginOfResponse + "[get][default-1-column]");

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                for (int i = 1; i < parms.Length; i++)
                {
                    string name = parms[i].Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        titles.Add(name, new List<PwEntry>());
                    }
                }
                FindTitles(titles);
            }

            foreach (var keypair in titles)
            {
                Debug.OutputLine("Found entries for title: " + keypair.Key);
                foreach (PwEntry entry in keypair.Value)
                {
                    Debug.OutputLine("    Entry: " + entry.Strings.ReadSafe(PwDefs.TitleField));
                    try
                    {
                        string url = GetEntryField(entry, PwDefs.UrlField);
                        string urlscheme = String.Empty;
                        string urlhost = String.Empty;
                        string urlport = String.Empty;
                        string urlpath = String.Empty;

                        if (!string.IsNullOrEmpty(url))
                        {
                            try
                            {
                                Uri uri = new Uri(url);
                                urlscheme = uri.Scheme;
                                urlhost = uri.Host;
                                if (uri.Port != -1) urlport = uri.Port.ToString();
                                urlpath = uri.AbsolutePath;
                            }
                            catch { }
                        }

                        result.AppendLine(GetEntryField(entry, PwDefs.TitleField) + "\t" +
                                          GetEntryField(entry, PwDefs.UserNameField) + "\t" +
                                          GetEntryField(entry, PwDefs.PasswordField) + "\t" +
                                          url + "\t" +
                                          urlscheme + "\t" +
                                          urlhost + "\t" +
                                          urlport + "\t" +
                                          urlpath + "\t" +
                                          Convert.ToBase64String(Encoding.UTF8.GetBytes(GetEntryField(entry, PwDefs.NotesField))) + "\t");
                    }
                    catch (Exception ex)
                    {
                        Debug.OutputLine("CommandGet Exception:" + Environment.NewLine + ex.ToString());
                    }
                }
            }

            Debug.OutputLine("Ended command get");
            return result.ToString();
        }

        private string CommandGetField(string[] parms)
        {
            Debug.OutputLine("Starting command getfield");

            StringBuilder result = new StringBuilder();
            result.AppendLine(BeginOfResponse + "[getfield][default-2-column]");

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                string name = (parms.Length >= 2 ? parms[1].Trim() : String.Empty);
                if (!string.IsNullOrEmpty(name))
                {
                    titles.Add(name, new List<PwEntry>());
                }
                FindTitles(titles);
            }

            List<string> fieldnames = new List<string>();
            {
                for (int i = 2; i < parms.Length; i++)
                {
                    string name = parms[i].Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        fieldnames.Add(name);
                    }
                }
            }

            foreach (var keypair in titles)
            {
                foreach (PwEntry entry in keypair.Value)
                {
                    result.Append("title");
                    result.Append("\t");
                    result.Append(GetEntryField(entry, PwDefs.TitleField));
                    result.Append("\t");

                    foreach (string fieldname in fieldnames)
                    {
                        try
                        {
                            string value = GetEntryField(entry, fieldname);

                            result.Append(fieldname);
                            result.Append("\t");
                            result.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
                            result.Append("\t");
                        }
                        catch { }
                    }

                    result.AppendLine();
                }
            }

            Debug.OutputLine("Ended command getfield");
            return result.ToString();
        }

        private string CommandGetAttachment(string[] parms)
        {
            Debug.OutputLine("Starting command getattachment");

            StringBuilder result = new StringBuilder();
            result.AppendLine(BeginOfResponse + "[getattachment][default-2-column]");

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                string name = (parms.Length >= 2 ? parms[1].Trim() : String.Empty);
                if (!string.IsNullOrEmpty(name))
                {
                    titles.Add(name, new List<PwEntry>());
                }
                FindTitles(titles);
            }

            List<string> attachmentnames = new List<string>();
            {
                for (int i = 2; i < parms.Length; i++)
                {
                    string name = parms[i].Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        attachmentnames.Add(name);
                    }
                }
            }

            foreach (var keypair in titles)
            {
                foreach (PwEntry entry in keypair.Value)
                {
                    result.Append("title");
                    result.Append("\t");
                    result.Append(GetEntryField(entry, PwDefs.TitleField));
                    result.Append("\t");

                    foreach (string attachmentname in attachmentnames)
                    {
                        try
                        {
                            byte[] value = entry.Binaries.Get(attachmentname).ReadData();

                            result.Append(attachmentname);
                            result.Append("\t");
                            result.Append(Convert.ToBase64String(value));
                            result.Append("\t");
                        }
                        catch { }
                    }

                    result.AppendLine();
                }
            }

            Debug.OutputLine("Ended command getattachment");
            return result.ToString();
        }

        private string CommandGetNote(string[] parms)
        {
            Debug.OutputLine("Starting command getnote");

            StringBuilder result = new StringBuilder();
            result.AppendLine(BeginOfResponse + "[getnote][default-1-column]");

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                for (int i = 1; i < parms.Length; i++)
                {
                    string name = parms[i].Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        titles.Add(name, new List<PwEntry>());
                    }
                }

                FindTitles(titles);
            }

            foreach (var keypair in titles)
            {
                foreach (PwEntry entry in keypair.Value)
                {
                    result.Append(GetEntryField(entry, PwDefs.TitleField));
                    result.Append("\t");
                    try
                    {
                        result.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(GetEntryField(entry, PwDefs.NotesField))));
                        result.Append("\t");
                    }
                    catch { }
                    result.AppendLine();
                }
            }

            Debug.OutputLine("Ended command getnote");
            return result.ToString();
        }

        private string CommandListGroup(string[] parms)
        {
            Debug.OutputLine("Starting command listgroup");

            StringBuilder result = new StringBuilder();
            result.AppendLine(BeginOfResponse + "[listgroup][default-1-column]");

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                for (int i = 1; i < parms.Length; i++)
                {
                    string name = parms[i].Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        titles.Add(name, new List<PwEntry>());
                    }
                }

                FindTitles(titles);
            }

            Dictionary<string, List<PwEntry>> found = new Dictionary<string, List<PwEntry>>();
            {
                foreach (var keypair in titles)
                {
                    foreach (PwEntry entry in keypair.Value)
                    {
                        var notes = GetEntryField(entry, PwDefs.NotesField);
                        //KeePassCommanderListGroup=true
                        //KeePassCommanderListAddItem={title}
                        foreach (var line in notes.Split('\n'))
                        {
                            var command = line.Trim();
                            string value;
                            string search;

                            search = "KeePassCommanderListGroup=";
                            if (command.StartsWith(search))
                            {
                                value = command.Substring(search.Length);
                                if (value.Trim() == "true")
                                    FindTitlesInGroup(entry, found);
                            }
                            else
                            {
                                search = "KeePassCommanderListAddItem=";
                                if (command.StartsWith(search))
                                {
                                    value = command.Substring(search.Length);

                                    FindTitle(value, found);
                                }
                            }
                        }
                    }
                }

                {
                    foreach (var keypair in found)
                    {
                        foreach (PwEntry entry in keypair.Value)
                        {
                            result.Append(GetEntryField(entry, PwDefs.TitleField));
                            result.Append("\t");
                            result.AppendLine();
                        }
                    }
                }
            }

            Debug.OutputLine("Ended command listgroup");
            return result.ToString();
        }
    }
}
