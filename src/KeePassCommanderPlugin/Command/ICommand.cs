using KeePass.Plugins;
using System.Text;

namespace KeePassCommander.Command
{
    public interface ICommand
    {
        void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output);
    }
}
