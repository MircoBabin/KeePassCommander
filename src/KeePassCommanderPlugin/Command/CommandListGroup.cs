using KeePass.Plugins;
using KeePassLib;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommander.Command
{
    public class CommandListGroup : ICommand
    {
        public void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output)
        {
            Debug.OutputLine("Starting command listgroup");

            output.AppendLine(Runner.BeginOfResponse + "[listgroup][default-1-column]");

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

            Dictionary<string, List<PwEntry>> found = new Dictionary<string, List<PwEntry>>();
            {
                foreach (var keypair in titles)
                {
                    foreach (PwEntry entry in keypair.Value)
                    {
                        var notes = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.NotesField);
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
                                    EntriesHelper.FindTitlesInGroup(Debug, KeePassHost, entry, found);
                            }
                            else
                            {
                                search = "KeePassCommanderListAddItem=";
                                if (command.StartsWith(search))
                                {
                                    value = command.Substring(search.Length);

                                    EntriesHelper.FindTitle(Debug, KeePassHost, value, found);
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
                            output.Append(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField));
                            output.Append("\t");
                            output.AppendLine();
                        }
                    }
                }
            }

            Debug.OutputLine("Ended command listgroup");
        }
    }
}
