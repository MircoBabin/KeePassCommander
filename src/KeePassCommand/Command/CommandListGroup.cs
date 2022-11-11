using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeePassCommand.Command
{
    public class CommandListGroup : ICommand
    {
        public void Run(ProgramArguments options, ISendCommand send)
        {
            // No success or failure indication, just one title per line, each line terminated with NEWLINE.
            // On FAILURE the output will be empty (0 bytes).
            // Unique sorted on title.

            StringBuilder output = new StringBuilder();

            SortedDictionary<string, string> unique = new SortedDictionary<string, string>();
            foreach (List<ResponseItem> entry in send.Response.Entries)
            {
                string title = entry[0].Parts[0];
                if (!unique.ContainsKey(title))
                    unique.Add(title, title);
            }

            foreach (string title in unique.Values)
            {
                output.AppendLine(title);
            }

            if (!String.IsNullOrWhiteSpace(options.outfile))
            {
                using (StreamWriter file = new StreamWriter(options.outfile, false, options.outfile_encoding))
                {
                    file.Write(output.ToString());
                }
            }
            else
            {
                Console.Write(output.ToString());
            }
        }
    }
}
