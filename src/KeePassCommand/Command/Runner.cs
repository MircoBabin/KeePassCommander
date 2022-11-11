using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeePassCommand.Command
{
    public class Runner
    {
        public void Run(ProgramArguments options)
        {
            ICommand command = null;
            try
            {
                StringBuilder sendCommand = new StringBuilder();
                {
                    if (options.outcommand == "get")
                    {
                        command = new CommandCommon();
                        sendCommand.Append("get");
                    }
                    else if (options.outcommand == "getfield")
                    {
                        command = new CommandCommon();
                        sendCommand.Append("getfield");
                    }
                    else if (options.outcommand == "getfieldraw")
                    {
                        command = new CommandGetFieldRaw();
                        sendCommand.Append("getfield");
                    }
                    else if (options.outcommand == "getattachment")
                    {
                        command = new CommandCommon();
                        sendCommand.Append("getattachment");
                    }
                    else if (options.outcommand == "getattachmentraw")
                    {
                        command = new CommandGetAttachmentRaw();
                        sendCommand.Append("getattachment");
                    }
                    else if (options.outcommand == "getnote")
                    {
                        command = new CommandCommon();
                        sendCommand.Append("getnote");
                    }
                    else if (options.outcommand == "getnoteraw")
                    {
                        command = new CommandGetNoteRaw();
                        sendCommand.Append("getnote");
                    }
                    else if (options.outcommand == "listgroup")
                    {
                        command = new CommandListGroup();
                        sendCommand.Append("listgroup");
                    }
                    else
                    {
                        command = new CommandCommon();
                        throw new Exception("unknown command: " + options.outcommand);
                    }
                    sendCommand.Append('\t');

                    foreach (var arg in options.outargs)
                    {
                        sendCommand.Append(arg);
                        sendCommand.Append('\t');
                    }
                }

                ISendCommand send;
                if (!String.IsNullOrWhiteSpace(options.filesystem))
                    send = new SendCommandViaFileSystem(options.filesystem, sendCommand.ToString());
                else if (options.namedpipe)
                    send = new SendCommandViaNamedPipe(sendCommand.ToString());
                else
                    throw new Exception("Unknown communication method.");

                command.Run(options, send);
            }
            catch (Exception ex)
            {
                if (command != null && command is CommandCommon)
                {
                    StringBuilder output = new StringBuilder();
                    output.AppendLine("ERROR");
                    output.AppendLine(ex.Message);

                    if (!String.IsNullOrWhiteSpace(options.outfile))
                    {
                        if (File.Exists(options.outfile))
                        {
                            try { File.Delete(options.outfile); } catch { }
                        }

                        using (StreamWriter file = new StreamWriter(options.outfile, false, options.outfile_encoding))
                        {
                            file.Write(output.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine(output.ToString());
                    }
                }
            }
        }
    }
}
