using KeePass.Plugins;
using System;
using System.Windows.Forms;

namespace KeePassCommander
{
    public sealed class KeePassCommanderExt : Plugin
    {
        public override string UpdateUrl { get { return "https://github.com/MircoBabin/KeePassCommander/releases/latest/download/keepass.plugin.version.txt"; } }

        private IPluginHost KeePassHost = null;

        private readonly DebugLog Debug = new DebugLog();
        private NamedPipeServer.NamedPipeServer NamedPipeServer = null;
        private ViaFileSystemServer.ViaFileSystemServer ViaFileSystemServer = null;

        public override bool Initialize(IPluginHost host)
        {
            Terminate();

            KeePassHost = host;

            //KeePass.exe --debug --KeePassCommanderDebug=c:\incoming\KeePassCommander.log
            string debugFileName = host.CommandLineArgs["KeePassCommanderDebug"];
            try
            {
                Debug.Initialize(debugFileName);
                Debug.OutputLine("--- Initialize() of KeePassCommanderExt ---");
            }
            catch (Exception ex)
            {
                MessageBox.Show("KeePassCommander debug logger failed to initialise. No logging will be performed until KeePass is restarted with a valid debug log file location. Reason: " + ex.ToString());
            }

            var CommandRunner = new Command.Runner(Debug, KeePassHost);

            NamedPipeServer = new NamedPipeServer.NamedPipeServer(Debug, CommandRunner);
            NamedPipeServer.StartServer();

            ViaFileSystemServer = new ViaFileSystemServer.ViaFileSystemServer(Debug, CommandRunner);
            KeePassHost.MainWindow.FileOpened += OnFileOpened;
            KeePassHost.MainWindow.FileClosed += OnFileClosed;
            ViaFileSystemServer.StartServer();
            ViaFileSystemServer.RescanEntries(KeePassHost);

            return true;
        }

        private void OnFileOpened(object sender, KeePass.Forms.FileOpenedEventArgs e)
        {
            if (ViaFileSystemServer != null) ViaFileSystemServer.RescanEntries(KeePassHost);
        }

        private void OnFileClosed(object sender, KeePass.Forms.FileClosedEventArgs e)
        {
            if (ViaFileSystemServer != null) ViaFileSystemServer.RescanEntries(KeePassHost);
        }

        public override void Terminate()
        {
            Debug.OutputLine("--- Terminate() of KeePassCommanderExt ---");
            if (KeePassHost == null) return;

            KeePassHost.MainWindow.FileOpened -= OnFileOpened;
            KeePassHost.MainWindow.FileClosed -= OnFileClosed;

            if (NamedPipeServer != null)
            {
                NamedPipeServer.StopServer();
                NamedPipeServer = null;
            }

            if (ViaFileSystemServer != null)
            {
                ViaFileSystemServer.StopServer();
                ViaFileSystemServer = null;
            }

            KeePassHost = null;
        }
    }
}
