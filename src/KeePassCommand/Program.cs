using System;
using System.IO;
using System.Text;

namespace KeePassCommand
{
    class Program
    {
        private static Version GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static void ShowHelp()
        {
            StringBuilder sb = new StringBuilder();

            var version = GetVersion();

            sb.Append("KeePassCommand ");
            sb.Append(version.Major + "." + version.Minor);
            if (version.Build > 0)
            {
                sb.Append(" [patch " + version.Build + "]");
            }
            sb.AppendLine();
            sb.AppendLine("https://github.com/MircoBabin/KeePassCommander - MIT license");

            sb.AppendLine();

            sb.AppendLine("KeePass Commander is a plugin for the KeePass password store (https://keepass.info/).");
            sb.AppendLine("It is a command-line tool that provides a communication channel for PHP scripts, Windows CMD/BAT/PowerShell scripts, Python, C#, git, etc. to query the KeePass password store without requiring configuration or passwords.");

            sb.AppendLine();

            sb.AppendLine("Syntax: KeePassCommand.exe <command> {-filesystem:folderpath OR -namedpipe} {-out:outputfilename OR -out-utf8:outputfilename} ...");

            sb.AppendLine("- If neither -filesystem nor -namedpipe is specified, the default will be Named Pipe. Unless the configuration file KeePassCommand.config.xml specifies other.");
            sb.AppendLine("- When -namedpipe is specified, communication will be encrypted via a Named Pipe.");
            sb.AppendLine("- When -filesystem:folderpath is specified, communication will be via encrypted files inside shared folder folderpath. A special KeePass entry with title starting with \"KeePassCommander.FileSystem\", with folderpath as url, and notes like listgroup must be present for this to work. The purpose is querying from inside a Virtual Machine. See https://github.com/MircoBabin/KeePassCommander/docs/VirtualMachine.md for more information.");

            sb.AppendLine("- Unless -out or -out-utf8 is used, output will be at the console (STDOUT) (without BOM).");
            sb.AppendLine("- If -stdout-utf8 is used, output at the console (STDOUT) will always use UTF-8 codepage (with BOM).");
            sb.AppendLine("- If -stdout-utf8nobom is used, output at the console (STDOUT) will always use UTF-8 codepage (without BOM).");
            sb.AppendLine("- When -out-utf8:outputfile is used, output will be written in outputfile using UTF-8 codepage (with BOM).");
            sb.AppendLine("- When -out:outputfile is used, output will be written in outputfile using ANSI codepage.");

            sb.AppendLine("- \"KeePass-entry-title\" must exactly match (case sensitive), there is no fuzzy logic. All open databases in KeePass are searched.");
            sb.AppendLine("- When the expected \"KeePass-entry-title\" is not found (you know it must be there), you can assume KeePass is not started or the required database is not opened.");

            sb.AppendLine();

            sb.AppendLine("* Basic get");
            sb.AppendLine("KeePassCommand.exe get \"KeePass-entry-title\" \"KeePass-entry-title\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe get \"Sample Entry\"");
            sb.AppendLine("- \"Note\" is outputted as UTF-8, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get string field");
            sb.AppendLine("KeePassCommand.exe getfield \"KeePass-entry-title\" \"fieldname\" \"fieldname\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe getfield \"Sample Entry\" \"extra field 1\" \"extra password 1\"");
            sb.AppendLine("- \"Value\" is outputted as UTF-8, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get string field RAW");
            sb.AppendLine("KeePassCommand.exe getfieldraw \"KeePass-entry-title\" \"fieldname\"");
            sb.AppendLine("e.g. KeePassCommand.exe getfieldraw -out-utf8:myfield.txt \"Sample Entry\" \"extra field 1\"");
            sb.AppendLine("- With -out-utf8, \"Value\" is outputted as UTF-8.");
            sb.AppendLine("- With -out, \"Value\" is outputted in ANSI codepage.");
            sb.AppendLine("- Without -out*, \"Value\" is outputted at the console (STDOUT).");


            sb.AppendLine();

            sb.AppendLine("* Advanced get file attachment");
            sb.AppendLine("KeePassCommand.exe getattachment \"KeePass-entry-title\" \"attachmentname\" \"attachmentname\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe getattachment \"Sample Entry\" \"example_attachment.txt\"");
            sb.AppendLine("- Attachment is outputted as binary, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get file attachment RAW");
            sb.AppendLine("KeePassCommand.exe getattachmentraw \"KeePass-entry-title\" \"attachmentname\"");
            sb.AppendLine("e.g. KeePassCommand.exe getattachmentraw \"Sample Entry\" \"example_attachment.txt\"");
            sb.AppendLine("- With -out, attachment is saved in outputfilename, outputted as binary.");
            sb.AppendLine("- Without -out*, attachment is outputted binary at the console (STDOUT).");

            sb.AppendLine();

            sb.AppendLine("* Advanced get note");
            sb.AppendLine("KeePassCommand.exe getnote \"KeePass-entry-title\" \"KeePass-entry-title\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe getnote \"Sample Entry\"");
            sb.AppendLine("- \"Note\" is outputted as UTF-8, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get note RAW");
            sb.AppendLine("KeePassCommand.exe getnoteraw \"KeePass-entry-title\"");
            sb.AppendLine("e.g. KeePassCommand.exe getnoteraw \"Sample Entry\"");
            sb.AppendLine("- With -out-utf8, \"Note\" is outputted as UTF-8.");
            sb.AppendLine("- With -out, \"Note\" is outputted in ANSI codepage.");
            sb.AppendLine("- Without -out*, \"Note\" is outputted at the console (STDOUT).");

            sb.AppendLine();
            sb.AppendLine("* Advanced list titles in group");
            sb.AppendLine("KeePassCommand.exe listgroup \"KeePass-entry-title\"");
            sb.AppendLine("e.g. KeePassCommand.exe listgroup \"All Entries\"");
            sb.AppendLine("- The queried entry note may contain the line \"KeePassCommanderListGroup=true\".");
            sb.AppendLine("  This is not recursive, only titles in the current group are listed.");
            sb.AppendLine("- The queried entry note may contain lines \"KeePassCommanderListAddItem={title}\".");
            sb.AppendLine("- Output is one title per line, unique sorted on titlename.");
            sb.AppendLine("- There is no SUCCESS or ERROR indication in the output.");

            sb.AppendLine();
            sb.AppendLine("* sign using buildstamp ( https://github.com/MircoBabin/BuildStamp )");
            sb.AppendLine("KeePassCommand.exe sign-using-buildstamp \"KeePass-entry-title\" \"filename\"");
            sb.AppendLine("e.g. KeePassCommand.exe sign-using-buildstamp \"SafeNet Token\" \"c:\\my-project\\bin\\release\\my-executable.exe\"");
            sb.AppendLine("- The queried entry must contain the advanced field \"buildstamp-exe\" pointing to buildstamp.exe on the host running KeePass.");
            sb.AppendLine("- The advanced field \"buildstamp-exe[...lowercase computername of KeePass host...]\" is preferred.");
            sb.AppendLine("- The advanced field \"--signtool-exe[...lowercase computername of KeePass host...]\" is preferred.");
            sb.AppendLine("- The advanced field \"--pkcs11-driver[...lowercase computername of KeePass host...]\" is preferred.");
            sb.AppendLine("- The exitcode is the exitcode of buildstamp.exe. Exitcode will be 99 if buildstamp.exe is not startable.");
            sb.AppendLine("- Output will be the stdout output followed by the stderr output of buildstamp.exe.");

            sb.AppendLine();
            sb.AppendLine("--- LICENSE ---");
            sb.AppendLine("KeePass Commander");
            sb.AppendLine("MIT license");
            sb.AppendLine();
            sb.AppendLine("Copyright (c) 2018 Mirco Babin");
            sb.AppendLine();
            sb.AppendLine("Permission is hereby granted, free of charge, to any person");
            sb.AppendLine("obtaining a copy of this software and associated documentation");
            sb.AppendLine("files (the \"Software\"), to deal in the Software without");
            sb.AppendLine("restriction, including without limitation the rights to use,");
            sb.AppendLine("copy, modify, merge, publish, distribute, sublicense, and/or sell");
            sb.AppendLine("copies of the Software, and to permit persons to whom the");
            sb.AppendLine("Software is furnished to do so, subject to the following");
            sb.AppendLine("conditions:");
            sb.AppendLine();
            sb.AppendLine("The above copyright notice and this permission notice shall be");
            sb.AppendLine("included in all copies or substantial portions of the Software.");
            sb.AppendLine();
            sb.AppendLine("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND,");
            sb.AppendLine("EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES");
            sb.AppendLine("OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND");
            sb.AppendLine("NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT");
            sb.AppendLine("HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,");
            sb.AppendLine("WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING");
            sb.AppendLine("FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR");
            sb.AppendLine("OTHER DEALINGS IN THE SOFTWARE.");

            Console.WriteLine();
            Console.Write(sb.ToString());
        }

        static ProgramArguments ParseArguments(string[] args)
        {
            var result = new ProgramArguments();

            foreach (var arg in args)
            {
                if (arg.Length > 5 && arg.Substring(0, 5).ToLower() == "-out:")
                {
                    result.outfile = arg.Substring(5);
                    result.outfile_encoding = Encoding.Default; //ANSI encoding on the computer executing this executable
                }
                else if (arg.Length > 10 && arg.Substring(0, 10).ToLower() == "-out-utf8:")
                {
                    result.outfile = arg.Substring(10);
                    result.outfile_encoding = Encoding.UTF8; // with BOM
                }
                else if (arg.Length > 12 && arg.Substring(0, 12).ToLower() == "-filesystem:")
                {
                    var filesystem = arg.Substring(12);
                    if (Directory.Exists(filesystem))
                    {
                        result.filesystem = filesystem;
                    }
                }
                else if (arg.Length == 10 && arg.Substring(0, 10).ToLower() == "-namedpipe")
                {
                    result.namedpipe = true;
                }
                else if (arg.Length == 12 && arg.Substring(0, 12).ToLower() == "-stdout-utf8")
                {
                    result.stdout_encoding = ProgramArguments.StdoutEncodingType.Utf8;
                }
                else if (arg.Length == 17 && arg.Substring(0, 17).ToLower() == "-stdout-utf8nobom")
                {
                    result.stdout_encoding = ProgramArguments.StdoutEncodingType.Utf8WithoutBom;
                }
                else
                {
                    if (String.IsNullOrEmpty(result.outcommand))
                    {
                        result.outcommand = arg;
                    }
                    else
                    {
                        result.outargs.Add(arg);
                    }
                }
            }

            if (!String.IsNullOrWhiteSpace(result.filesystem) && result.namedpipe == true)
                throw new Exception("Both commandline arguments -namedpipe and -filesystem: are specified. Only specify one of them.");

            return result;
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    ShowHelp();
                    return;
                }

                if (args.Length == 1 && args[0].ToLower() == "--version")
                {
                    var version = GetVersion();
                    Console.Write(version.Major + "." + version.Minor);
                    return;
                }

                ProgramArguments options = ParseArguments(args);
                if (String.IsNullOrWhiteSpace(options.outcommand))
                    throw new Exception("no command specified");


                if (!String.IsNullOrWhiteSpace(options.outfile) && File.Exists(options.outfile))
                {
                    File.Delete(options.outfile);
                }

                var runner = new Command.Runner();
                runner.Run(options);
                Environment.Exit(runner.ExitCode);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(99);
            }
        }
    }
}
