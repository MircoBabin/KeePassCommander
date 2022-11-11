using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;

namespace KeePassCommander.Command
{
    public class EntriesHelper
    {
        public static string GetEntryField(DebugLog Debug, IPluginHost KeePassHost, PwEntry entry, string FieldName)
        {
            const string StrRefStart = @"{REF:";
            const string StrRefEnd = @"}";
            const StringComparison ScMethod = StringComparison.OrdinalIgnoreCase;

            string input = entry.Strings.ReadSafe(FieldName);

            //replace field references
            //See also KeePass source SprEngine.cs function FillRefPlaceholders
            if (input.IndexOf(StrRefStart, 0, ScMethod) < 0)
            {
                //nothing to do
                return input;
            }

            KeePass.Util.Spr.SprContext ctx = new KeePass.Util.Spr.SprContext(entry,
                KeePassHost.MainWindow.DocumentManager.SafeFindContainerOf(entry),
                KeePass.Util.Spr.SprCompileFlags.Deref);


            int nOffset = 0;
            int maxTries = 100;
            while (true)
            {
                int nStart = input.IndexOf(StrRefStart, nOffset, ScMethod);
                if (nStart < 0) break;
                int nEnd = input.IndexOf(StrRefEnd, nStart + 1, ScMethod);
                if (nEnd <= nStart) break;

                string strFullRef = input.Substring(nStart, nEnd - nStart + 1);
                char chScan, chWanted;
                PwEntry peFound = KeePass.Util.Spr.SprEngine.FindRefTarget(strFullRef, ctx, out chScan, out chWanted);

                if (peFound != null)
                {
                    string strInsData;
                    if (chWanted == 'T')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.TitleField);
                    else if (chWanted == 'U')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.UserNameField);
                    else if (chWanted == 'A')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.UrlField);
                    else if (chWanted == 'P')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.PasswordField);
                    else if (chWanted == 'N')
                        strInsData = peFound.Strings.ReadSafe(PwDefs.NotesField);
                    else if (chWanted == 'I')
                        strInsData = peFound.Uuid.ToHexString();
                    else { nOffset = nStart + 1; continue; }

                    input = input.Substring(0, nStart) + strInsData + input.Substring(nEnd + StrRefEnd.Length);
                    maxTries--;
                    if (maxTries <= 0) break;
                }
                else { nOffset = nStart + 1; continue; }
            }

            return input;
        }

        public static void FindTitles(DebugLog Debug, IPluginHost KeePassHost, Dictionary<string, List<PwEntry>> search)
        {
            Debug.OutputLine("Starting FindTitles");
            if (search.Count == 0)
            {
                Debug.OutputLine("Ended FindTitles, nothing to search");
                return;
            }

            foreach (var doc in KeePassHost.MainWindow.DocumentManager.Documents)
            {
                PwDatabase db = doc.Database;

                if (db.IsOpen)
                {
                    Debug.OutputLine("    Database: " + db.Name);

                    var items = db.RootGroup.GetObjects(true, true);
                    foreach (var item in items)
                    {
                        if (item is PwEntry)
                        {
                            PwEntry entry = item as PwEntry;

                            string title = GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField);
                            if (search.ContainsKey(title))
                            {
                                search[title].Add(entry);
                            }
                        }
                    }
                }
            }

            Debug.OutputLine("Ended FindTitles");
        }

        public static void FindTitle(DebugLog Debug, IPluginHost KeePassHost, string title, Dictionary<string, List<PwEntry>> found)
        {
            var titletofind = new Dictionary<string, List<PwEntry>>();
            titletofind.Add(title, new List<PwEntry>());
            FindTitles(Debug, KeePassHost, titletofind);

            List<PwEntry> foundEntries;
            if (found.ContainsKey(title))
            {
                foundEntries = found[title];
            }
            else
            {
                foundEntries = new List<PwEntry>();
                found.Add(title, foundEntries);
            }

            foundEntries.AddRange(titletofind[title]);
        }

        public static  void FindTitlesInGroup(DebugLog Debug, IPluginHost KeePassHost, PwEntry groupEntry, Dictionary<string, List<PwEntry>> found)
        {
            Debug.OutputLine("Starting FindTitlesInGroup");
            if (groupEntry == null)
            {
                Debug.OutputLine("Ended FindTitlesInGroup, nothing to search");
                return;
            }

            PwGroup group = groupEntry.ParentGroup;
            if (group.Entries != null)
            {
                foreach (var entry in group.Entries)
                {
                    if (entry != groupEntry)
                    {
                        string title = GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField);

                        List<PwEntry> foundEntries;
                        if (found.ContainsKey(title))
                        {
                            foundEntries = found[title];
                        }
                        else
                        {
                            foundEntries = new List<PwEntry>();
                            found.Add(title, foundEntries);
                        }

                        foundEntries.Add(entry);
                    }
                }
            }

            Debug.OutputLine("Ended FindTitlesInGroup");
        }
    }
}
