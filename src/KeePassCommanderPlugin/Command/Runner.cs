using KeePass.Plugins;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommander.Command
{
    public class Runner
    {
        public const string BeginOfResponse = "\t\t\t[--- begin of response 4.0 ---]\t\t\t";
        private const string EndOfResponse = "\t\t\t[--- end of response 4.0 ---]\t\t\t";

        private DebugLog Debug;
        private IPluginHost KeePassHost;

        public Runner(DebugLog Debug, IPluginHost KeePassHost)
        {
            this.Debug = Debug;
            this.KeePassHost = KeePassHost;
        }

        public StringBuilder Run(string[] parms, Dictionary<string, bool> allowedTitles)
        {
            StringBuilder output = new StringBuilder();
            ICommand command = null;

            if (parms.Length > 0)
            {
                if (parms[0] == "get")
                    command = new CommandGet();
                else if (parms[0] == "getfield")
                    command = new CommandGetField();
                else if (parms[0] == "getattachment")
                    command = new CommandGetAttachment();
                else if (parms[0] == "getnote")
                    command = new CommandGetNote();
                else if (parms[0] == "listgroup")
                    command = new CommandListGroup();
            }

            if (command != null) command.Run(Debug, KeePassHost, parms, output, allowedTitles);

            output.AppendLine();
            output.AppendLine(EndOfResponse);

            return output;
        }
    }
}
