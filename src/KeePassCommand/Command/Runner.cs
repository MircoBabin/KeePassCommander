using KeePassCommandDll.Communication;
using KeePassCommander;
using System;
using System.IO;
using System.Text;

namespace KeePassCommand.Command
{
    public class Runner
    {
        public int ExitCode { get; private set; }

        public Runner()
        {
            ExitCode = 99;
        }

        public void Run(ProgramArguments options)
        {
            ICommand command = null;
            try
            {
                StringBuilder sendCommand = new StringBuilder();
                bool appendRemainingArgs = true;
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
                    else if (options.outcommand == "sign-using-buildstamp")
                    {
                        appendRemainingArgs = false;

                        if (options.outargs.Count != 2)
                            throw new Exception("sign-using-buildstamp expects 2 parameters, the KeePass-entry-title and the filename to sign.");
                        string title = options.outargs[0];
                        string filename = options.outargs[1];
                        if (!File.Exists(filename))
                            throw new Exception("sign-using-buildstamp: file \"" + filename + "\" does not exist.");
                        string filenameOnly = Path.GetFileName(filename);

                        command = new CommandSignUsingBuildstamp(filename, filenameOnly);
                        sendCommand.Append("sign-using-buildstamp");
                        sendCommand.Append('\t');
                        sendCommand.Append(title);
                        sendCommand.Append('\t');

                        sendCommand.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(filenameOnly)));
                        sendCommand.Append('\t');
                        sendCommand.Append(Convert.ToBase64String(File.ReadAllBytes(filename)));
                        sendCommand.Append('\t');
                    }
                    else
                    {
                        command = new CommandCommon();
                        throw new Exception("unknown command: " + options.outcommand);
                    }

                    if (appendRemainingArgs)
                    {
                        sendCommand.Append('\t');

                        foreach (var arg in options.outargs)
                        {
                            sendCommand.Append(arg);
                            sendCommand.Append('\t');
                        }
                    }
                }

                CommandSender sender;
                if (options.namedpipe == true)
                    sender = new CommandSender(CommandSender.CommunicationType.NamedPipe);
                else if (!String.IsNullOrWhiteSpace(options.filesystem))
                    sender = new CommandSender(CommandSender.CommunicationType.FileSystem, options.filesystem);
                else
                    sender = new CommandSender(CommandSender.CommunicationType.DetermineAutomatically);

                ISendCommand send = sender.Send(sendCommand.ToString());

                command.Run(options, send);
                if (command is ICommandHasExitCode)
                {
                    var commandHasExitCode = (command as ICommandHasExitCode);
                    ExitCode = commandHasExitCode.ExitCode;
                }
                else
                {
                    ExitCode = 0;
                }
            }
            catch (Exception ex)
            {
                if (command != null && command is CommandCommon)
                {
                    StringBuilder output = new StringBuilder();
                    output.Append("ERROR" + KeePassCommanderConsts.EOL);
                    output.Append(ex.Message + KeePassCommanderConsts.EOL);

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
