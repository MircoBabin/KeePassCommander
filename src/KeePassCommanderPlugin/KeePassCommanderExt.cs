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
        private NamedPipeServer.NamedPipeServer Server = null;

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
            Server = new NamedPipeServer.NamedPipeServer(Debug, CommandRunner);
            Server.StartServer();
            return true;
        }

        public override void Terminate()
        {
            Debug.OutputLine("--- Terminate() of KeePassCommanderExt ---");
            if (KeePassHost == null) return;

            if (Server != null)
            {
                Server.StopServer();
                Server = null;
            }

            KeePassHost = null;
        }
    }
}
