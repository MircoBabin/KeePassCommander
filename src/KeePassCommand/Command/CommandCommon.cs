using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeePassCommand.Command
{
    public class CommandCommon : ICommand
    {
        public void Run(ProgramArguments options, ISendCommand send)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("SUCCESS");

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
                        output.AppendLine("B\t");
                        foreach (ResponseItem item in entry)
                        {
                            output.AppendLine("I\t" + item.Parts[0]);
                        }
                        output.AppendLine("E\t");
                        break;

                    case Response.ResponseLayoutType.default_2_column:
                        output.AppendLine("B\t");
                        foreach (ResponseItem item in entry)
                        {
                            output.AppendLine("I\t" + item.Parts[0] + "\t" + item.Parts[1]);
                        }
                        output.AppendLine("E\t");
                        break;

                    default:
                        throw new Exception("Unknown response layout: " + send.Response.ResponseType.ToString());
                }
            }

            OutputUtils.OutputString(options, output);
        }
    }
}
