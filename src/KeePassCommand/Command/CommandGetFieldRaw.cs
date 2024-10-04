using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommand.Command
{
    public class CommandGetFieldRaw : ICommand
    {
        public void Run(ProgramArguments options, ISendCommand send)
        {
            if (send.Response.ResponseType != Response.ResponseLayoutType.default_2_column)
                throw new Exception("getfieldraw response type should be default_2_column, but is: " + send.Response.ResponseType.ToString());

            if (send.Response.Entries.Count != 1)
                throw new Exception("getfieldraw must query exactly one entry");

            List<ResponseItem> entry = send.Response.Entries[0];
            if (entry.Count != 2 || entry[0].Parts[0] != "title")
                throw new Exception("getfieldraw must query exactly one entry");

            string fieldvalue = Encoding.UTF8.GetString(Convert.FromBase64String(entry[1].Parts[1]));

            OutputUtils.OutputString(options, fieldvalue);
        }
    }
}
