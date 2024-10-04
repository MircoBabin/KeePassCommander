using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;

namespace KeePassCommand.Command
{
    public class CommandGetAttachmentRaw : ICommand
    {
        public void Run(ProgramArguments options, ISendCommand send)
        {
            if (send.Response.ResponseType != Response.ResponseLayoutType.default_2_column)
                throw new Exception("getattachmentraw response type should be default_2_column, but is: " + send.Response.ResponseType.ToString());

            if (send.Response.Entries.Count != 1)
                throw new Exception("getattachmentraw must query exactly one entry");

            List<ResponseItem> entry = send.Response.Entries[0];
            if (entry.Count != 2 || entry[0].Parts[0] != "title")
                throw new Exception("getattachmentraw must query exactly one entry");

            byte[] attachment = Convert.FromBase64String(entry[1].Parts[1]);

            OutputUtils.OutputBinary(options, attachment);
        }
    }
}
