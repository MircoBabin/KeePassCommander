using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommander.Command
{
    public class CommandGetNote : ICommand
    {
        public void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output)
        {
            Debug.OutputLine("Starting command getnote");

            output.AppendLine(Runner.BeginOfResponse + "[getnote][default-1-column]");

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

                EntriesHelper.FindTitles(Debug, KeePassHost, titles);
            }

            foreach (var keypair in titles)
            {
                foreach (PwEntry entry in keypair.Value)
                {
                    output.Append(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField));
                    output.Append("\t");
                    try
                    {
                        output.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.NotesField))));
                        output.Append("\t");
                    }
                    catch { }
                    output.AppendLine();
                }
            }

            Debug.OutputLine("Ended command getnote");
        }
    }
}
