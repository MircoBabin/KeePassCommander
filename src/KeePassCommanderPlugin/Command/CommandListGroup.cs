using KeePass.Plugins;
using KeePassLib;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommander.Command
{
    public class CommandListGroup : ICommand
    {
        public void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output, Dictionary<string, bool> allowedTitles)
        {
            Debug.OutputLine("Starting command listgroup");

            output.Append(Runner.BeginOfResponse + "[listgroup][default-1-column]" + KeePassCommanderConsts.EOL);

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

                EntriesHelper.FindTitles(Debug, KeePassHost, titles, allowedTitles);
            }

            Dictionary<string, List<PwEntry>> found = new Dictionary<string, List<PwEntry>>();
            {
                foreach (var keypair in titles)
                {
                    foreach (PwEntry entry in keypair.Value)
                    {
                        EntriesHelper.ParseListGroupEntry(Debug, KeePassHost, entry, found);
                    }
                }
            }

            foreach (var keypair in found)
            {
                foreach (PwEntry entry in keypair.Value)
                {
                    output.Append(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField));
                    output.Append("\t");
                    output.Append(KeePassCommanderConsts.EOL);
                }
            }

            Debug.OutputLine("Ended command listgroup");
        }
    }
}
