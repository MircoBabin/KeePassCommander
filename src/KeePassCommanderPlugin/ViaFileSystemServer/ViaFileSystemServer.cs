using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace KeePassCommander.ViaFileSystemServer
{
    public class ViaFileSystemServer
    {
        private const string helloRequestSuffix = ".KeePassCommander.FileSystem.hello-request";
        private const string helloResponseSuffix = ".KeePassCommander.FileSystem.hello-response";
        private const string requestSuffix = ".KeePassCommander.FileSystem.request";
        private const string responseSuffix = ".KeePassCommander.FileSystem.response";

        private readonly DebugLog Debug;
        private readonly Command.Runner Runner;
        private readonly Dictionary<string, DirectoryWatcher> Watchers = new Dictionary<string, DirectoryWatcher>();

        public ViaFileSystemServer(DebugLog Debug, Command.Runner Runner)
        {
            this.Debug = Debug;
            this.Runner = Runner;
        }

        public void StartServer()
        {
        }

        public void StopServer()
        {
            StopWatchers();
        }

        private void StopWatchers()
        {
            foreach (var keypair in Watchers)
            {
                var watcher = keypair.Value;
                try
                {
                    watcher.Dispose();
                }
                catch { }
            }

            Watchers.Clear();
        }

        public void RescanEntries(IPluginHost KeePassHost)
        {
            Debug.OutputLine("ViaFileSystemServer - starting rescan entries for KeePassCommander.FileSystem");

            StopWatchers();

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

                            string title = Command.EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField);
                            if (title.TrimStart().StartsWith("KeePassCommander.FileSystem"))
                            {
                                string directory = Path.GetFullPath(Command.EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.UrlField).Trim());
                                if (Directory.Exists(directory))
                                {
                                    Dictionary<string, List<PwEntry>> found = new Dictionary<string, List<PwEntry>>();
                                    Command.EntriesHelper.ParseListGroupEntry(Debug, KeePassHost, entry, found);

                                    if (Debug.Enabled)
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendLine("ViaFileSystemServer - rescan entries - found title: " + title);
                                        sb.AppendLine("    Directory: " + directory);
                                        foreach (var keypair in found)
                                        {
                                            sb.AppendLine("    Allowed Title: " + keypair.Key);
                                        }
                                        Debug.OutputLine(sb.ToString());
                                    }

                                    string key = directory.ToLowerInvariant();
                                    if (!Watchers.ContainsKey(key))
                                    {
                                        var watcher = new DirectoryWatcher(directory, "*" + helloRequestSuffix, HandleFile);
                                        watcher.AddAllowedTitles(found);

                                        Watchers.Add(key, watcher);
                                    }
                                    else
                                    {
                                        var watcher = Watchers[key];
                                        watcher.AddAllowedTitles(found);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Debug.OutputLine("ViaFileSystemServer - ended rescan entries for KeePassCommander.FileSystem");
        }

        private class FileSystemConnection
        {
            public string helloRequestFilename;
            public string helloResponseFilename;
            public string requestFilename;
            public string responseFilename;
            public DateTime started;

            public FileSystemConnection(string helloRequestFilename, string helloResponseFilename,
                string requestFilename, string responseFilename)
            {
                this.helloRequestFilename = helloRequestFilename;
                this.helloResponseFilename = helloResponseFilename;
                this.requestFilename = requestFilename;
                this.responseFilename = responseFilename;
                this.started = DateTime.Now;
            }
        }

        private Dictionary<string, FileSystemConnection> requests = new Dictionary<string, FileSystemConnection>();

        private byte[] ReadRequest(string filename)
        {
            try
            {
                var StartTime = DateTime.Now;
                int timeoutSeconds = 10;
                while (true)
                {
                    try
                    {
                        using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            byte[] bytes;

                            if (stream.Length < 4)
                                throw new Exception("File is not complete. Expected 4 bytes, length = " + stream.Length);

                            bytes = new byte[4];
                            stream.Read(bytes, 0, 4);
                            UInt32 length = BitConverter.ToUInt32(bytes, 0);

                            if (stream.Length != (4 + length))
                                throw new Exception("File is not complete. Expected (4 + " + length + ") " + (4 + length) + ", length = " + stream.Length);

                            bytes = new byte[length];
                            stream.Read(bytes, 0, (int)length);
                            return bytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (DateTime.Now.Subtract(StartTime).Seconds > timeoutSeconds)
                            throw new Exception("Waiting for completion failed on " + filename + " failed. " + ex.Message, ex);
                    }
                }
            }
            finally
            {
                KeePassCommander.Encryption.SecureDelete(filename);
            }
        }

        private void WriteResponse(string filename, byte[] data)
        {
            using (var stream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] bytes = BitConverter.GetBytes((UInt32)data.Length);
                if (bytes.Length != 4) throw new Exception("BitConverter.GetBytes(UInt32) must return 4 bytes.");
                stream.Write(bytes, 0, 4);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }

        private void HandleFile(string filename, Dictionary<string, bool> allowedTitles)
        {
            try
            {
                var helloRequestFilename = Path.GetFullPath(filename);
                if (!helloRequestFilename.EndsWith(helloRequestSuffix)) return;

                var helloResponseFilename = helloRequestFilename.Substring(0, helloRequestFilename.Length - helloRequestSuffix.Length) + helloResponseSuffix;
                var requestFilename = helloRequestFilename.Substring(0, helloRequestFilename.Length - helloRequestSuffix.Length) + requestSuffix;
                var responseFilename = helloRequestFilename.Substring(0, helloRequestFilename.Length - helloRequestSuffix.Length) + responseSuffix;

                var requestsKey = helloRequestFilename.ToLowerInvariant();
                lock (requests)
                {
                    if (requests.ContainsKey(requestsKey)) return;

                    requests.Add(requestsKey, new FileSystemConnection(helloRequestFilename, helloResponseFilename,
                        requestFilename, responseFilename));
                }

                try
                {
                    if (!File.Exists(helloRequestFilename)) return;

                    Debug.OutputLine("ViaFileSystemServer - HandleFile: " + requestFilename);

                    KeePassCommander.Encryption encryption = new KeePassCommander.Encryption();
                    {
                        // Hello - settle a shared key for encryption
                        encryption.SettleSharedKey(ReadRequest(helloRequestFilename));

                        WriteResponse(helloResponseFilename, encryption.PublicKeyForSettlement);
                    }

                    {
                        // Request - encrypted
                        string command = encryption.Decrypt(ReadRequest(requestFilename));
                        string[] parms = command.Split('\t');

                        if (Debug.Enabled)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("ViaFileSystemServer - Received parameters:");
                            foreach (var parm in parms)
                            {
                                sb.AppendLine(parm);
                            }
                            Debug.OutputLine(sb.ToString());
                        }

                        var output = Runner.Run(parms, allowedTitles);

                        WriteResponse(responseFilename, encryption.Encrypt(output.ToString()));
                    }
                }
                finally
                {
                    KeePassCommander.Encryption.SecureDelete(helloRequestFilename);
                    KeePassCommander.Encryption.SecureDelete(helloResponseFilename);
                    KeePassCommander.Encryption.SecureDelete(requestFilename);

                    lock (requests)
                    {
                        requests.Remove(requestsKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.OutputLine("ViaFileSystemServer - HandleFile Exception:" + Environment.NewLine + ex.ToString());
            }
        }
    }
}
