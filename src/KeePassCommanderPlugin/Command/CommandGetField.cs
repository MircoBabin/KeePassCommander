using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommander.Command
{
    public class CommandGetField : ICommand
    {
        public void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output)
        {
            Debug.OutputLine("Starting command getfield");

            output.AppendLine(Runner.BeginOfResponse + "[getfield][default-2-column]");

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                string name = (parms.Length >= 2 ? parms[1].Trim() : String.Empty);
                if (!string.IsNullOrEmpty(name))
                {
                    titles.Add(name, new List<PwEntry>());
                }
                EntriesHelper.FindTitles(Debug, KeePassHost, titles);
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
                    output.Append("title");
                    output.Append("\t");
                    output.Append(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField));
                    output.Append("\t");

                    foreach (string fieldname in fieldnames)
                    {
                        try
                        {
                            string value = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, fieldname);

                            output.Append(fieldname);
                            output.Append("\t");
                            output.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
                            output.Append("\t");
                        }
                        catch { }
                    }

                    output.AppendLine();
                }
            }

            Debug.OutputLine("Ended command getfield");
        }
    }
}
