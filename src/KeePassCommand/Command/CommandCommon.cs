using KeePassCommandDll.Communication;
using KeePassCommander;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommand.Command
{
    public class CommandCommon : ICommand
    {
        public void Run(ProgramArguments options, ISendCommand send)
        {
            StringBuilder output = new StringBuilder();
            output.Append("SUCCESS" + KeePassCommanderConsts.EOL);

            switch (send.Response.ResponseType)
            {
                case Response.ResponseLayoutType.default_1_column:
                case Response.ResponseLayoutType.default_2_column:
                    break;

                default:
                    throw new Exception("Unknown response layout: " + send.Response.ResponseType.ToString());
            }

            foreach (List<ResponseItem> entry in send.Response.Entries)
            {
                switch (send.Response.ResponseType)
                {
                    case Response.ResponseLayoutType.default_1_column:
                        output.Append("B\t" + KeePassCommanderConsts.EOL);
                        foreach (ResponseItem item in entry)
                        {
                            output.Append("I\t" + item.Parts[0] + KeePassCommanderConsts.EOL);
                        }
                        output.Append("E\t" + KeePassCommanderConsts.EOL);
                        break;

                    case Response.ResponseLayoutType.default_2_column:
                        output.Append("B\t" + KeePassCommanderConsts.EOL);
                        foreach (ResponseItem item in entry)
                        {
                            output.Append("I\t" + item.Parts[0] + "\t" + item.Parts[1] + KeePassCommanderConsts.EOL);
                        }
                        output.Append("E\t" + KeePassCommanderConsts.EOL);
                        break;

                    default:
                        throw new Exception("Unknown response layout: " + send.Response.ResponseType.ToString());
                }
            }

            OutputUtils.OutputString(options, output);
        }
    }
}
