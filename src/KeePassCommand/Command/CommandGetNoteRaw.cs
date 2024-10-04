using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommand.Command
{
    public class CommandGetNoteRaw : ICommand
    {
        public void Run(ProgramArguments options, ISendCommand send)
        {
            if (send.Response.ResponseType != Response.ResponseLayoutType.default_1_column)
                throw new Exception("getnoteraw response type should be default_1_column, but is: " + send.Response.ResponseType.ToString());

            if (send.Response.Entries.Count != 1)
                throw new Exception("getnoteraw must query exactly one entry");

            List<ResponseItem> entry = send.Response.Entries[0];
            if (entry.Count != 2)
                throw new Exception("getnoteraw must query exactly one entry");

            string notes = Encoding.UTF8.GetString(Convert.FromBase64String(entry[1].Parts[0]));

            OutputUtils.OutputString(options, notes);
        }
    }
}
