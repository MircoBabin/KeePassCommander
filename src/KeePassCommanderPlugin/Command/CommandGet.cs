using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommander.Command
{
    public class CommandGet : ICommand
    {
        public void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output, Dictionary<string, bool> allowedTitles)
        {
            Debug.OutputLine("Starting command get");

            output.AppendLine(Runner.BeginOfResponse + "[get][default-1-column]");

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

            foreach (var keypair in titles)
            {
                Debug.OutputLine("Found entries for title: " + keypair.Key);
                foreach (PwEntry entry in keypair.Value)
                {
                    Debug.OutputLine("    Entry: " + entry.Strings.ReadSafe(PwDefs.TitleField));
                    try
                    {
                        string url = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.UrlField);
                        string urlscheme = String.Empty;
                        string urlhost = String.Empty;
                        string urlport = String.Empty;
                        string urlpath = String.Empty;

                        if (!string.IsNullOrEmpty(url))
                        {
                            try
                            {
                                Uri uri = new Uri(url);
                                urlscheme = uri.Scheme;
                                urlhost = uri.Host;
                                if (uri.Port != -1) urlport = uri.Port.ToString();
                                urlpath = uri.AbsolutePath;
                            }
                            catch { }
                        }

                        output.AppendLine(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField) + "\t" +
                                          EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.UserNameField) + "\t" +
                                          EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.PasswordField) + "\t" +
                                          url + "\t" +
                                          urlscheme + "\t" +
                                          urlhost + "\t" +
                                          urlport + "\t" +
                                          urlpath + "\t" +
                                          Convert.ToBase64String(Encoding.UTF8.GetBytes(EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.NotesField))) + "\t");
                    }
                    catch (Exception ex)
                    {
                        Debug.OutputLine("CommandGet Exception:" + Environment.NewLine + ex.ToString());
                    }
                }
            }

            Debug.OutputLine("Ended command get");
        }
    }
}
