using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommander.Command
{
    public class CommandGetAttachment : ICommand
    {
        public void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output)
        {
            Debug.OutputLine("Starting command getattachment");

            output.AppendLine(Runner.BeginOfResponse + "[getattachment][default-2-column]");

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                string name = (parms.Length >= 2 ? parms[1].Trim() : String.Empty);
                if (!string.IsNullOrEmpty(name))
                {
                    titles.Add(name, new List<PwEntry>());
                }
                EntriesHelper.FindTitles(Debug, KeePassHost, titles);
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
                    output.Append("title");
                    output.Append("\t");
                    output.Append(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField));
                    output.Append("\t");

                    foreach (string attachmentname in attachmentnames)
                    {
                        try
                        {
                            byte[] value = entry.Binaries.Get(attachmentname).ReadData();

                            output.Append(attachmentname);
                            output.Append("\t");
                            output.Append(Convert.ToBase64String(value));
                            output.Append("\t");
                        }
                        catch { }
                    }

                    output.AppendLine();
                }
            }

            Debug.OutputLine("Ended command getattachment");
        }
    }
}
