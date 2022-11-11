using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

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

            sb.AppendLine("Syntax: KeePassCommand.exe <command> {-out:outputfilename OR -out-utf8:outputfilename} ...");
            sb.AppendLine("- Unless -out or -out-utf8 is used, output will be at the console (STDOUT).");
            sb.AppendLine("- When -out-utf8:outputfile is used, output will be written in outputfile using UTF-8 codepage.");
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

            sb.AppendLine("* Advanced get string field raw into file");
            sb.AppendLine("KeePassCommand.exe getfieldraw <-out-utf8:outputfile or -out:> \"KeePass-entry-title\" \"fieldname\"");
            sb.AppendLine("e.g. KeePassCommand.exe getfieldraw -out-utf8:myfield.txt \"Sample Entry\" \"extra field 1\"");
            sb.AppendLine("- With -out-utf8, \"Value\" is outputted as UTF-8.");
            sb.AppendLine("- With -out, \"Value\" is outputted in ANSI codepage.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get file attachment");
            sb.AppendLine("KeePassCommand.exe getattachment \"KeePass-entry-title\" \"attachmentname\" \"attachmentname\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe getattachment \"Sample Entry\" \"example_attachment.txt\"");
            sb.AppendLine("- Attachment is outputted as binary, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get file attachment raw into file");
            sb.AppendLine("KeePassCommand.exe getattachmentraw -out:outputfilename \"KeePass-entry-title\" \"attachmentname\"");
            sb.AppendLine("e.g. KeePassCommand.exe getattachmentraw -out:myfile.txt \"Sample Entry\" \"example_attachment.txt\"");
            sb.AppendLine("- Attachment is saved in outputfilename, outputted as binary.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get note");
            sb.AppendLine("KeePassCommand.exe getnote \"KeePass-entry-title\" \"KeePass-entry-title\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe getnote \"Sample Entry\"");
            sb.AppendLine("- \"Note\" is outputted as UTF-8, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get note into file");
            sb.AppendLine("KeePassCommand.exe getnoteraw <-out-utf8:outputfile or -out:> \"KeePass-entry-title\"");
            sb.AppendLine("e.g. KeePassCommand.exe getnoteraw -out-utf8:mynote.txt \"Sample Entry\"");
            sb.AppendLine("- With -out-utf8, \"Note\" is outputted as UTF-8.");
            sb.AppendLine("- With -out, \"Note\" is outputted in ANSI codepage.");

            sb.AppendLine();
            sb.AppendLine("* Advanced list titles in group");
            sb.AppendLine("KeePassCommand.exe listgroup <-out-utf8:outputfile or -out:> \"KeePass-entry-title\"");
            sb.AppendLine("e.g. KeePassCommand.exe listgroup -out-utf8:titles.txt \"All Entries\"");
            sb.AppendLine("- With -out-utf8, \"titles\" are outputted as UTF-8.");
            sb.AppendLine("- With -out, \"titles\" are outputted in ANSI codepage.");
            sb.AppendLine("- The queried entry note may contain the line \"KeePassCommanderListGroup=true\".");
            sb.AppendLine("  This is not recursive, only titles in the current group are listed.");
            sb.AppendLine("- The queried entry note may contain lines \"KeePassCommanderListAddItem={title}\".");
            sb.AppendLine("- Output is one title per line, unique sorted on titlename.");
            sb.AppendLine("- There is no SUCCESS or ERROR indication in the output.");

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

        /*
        KeePassCommand.config.xml

        <?xml version="1.0" encoding="utf-8"?>
        <Configuration>
            <filesystem>s:\incoming\KeePass</filesystem>
        </Configuration>
        */
        private class Configuration
        {
            public string filesystem { get; set; }
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
                    result.outfile_encoding = Encoding.UTF8;
                }
                else if (arg.Length > 12 && arg.Substring(0, 12).ToLower() == "-filesystem:")
                {
                    var filesystem = arg.Substring(12);
                    if (Directory.Exists(filesystem))
                    {
                        result.filesystem = filesystem;
                    }
                }
                else if (arg.Length > 10 && arg.Substring(0, 10).ToLower() == "-namedpipe")
                {
                    result.namedpipe = true;
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

            if (!String.IsNullOrWhiteSpace(result.filesystem) && result.namedpipe)
                throw new Exception("Both commandline arguments -namedpipe and -filesystem: are specified. Only specify one of them.");

            if (String.IsNullOrWhiteSpace(result.filesystem) && !result.namedpipe)
            {
                try
                {
                    string xmlfilename;
                    {
                        Process me = Process.GetCurrentProcess();
                        string executableFileName = me.Modules[0].FileName;
                        string programPath = Path.GetFullPath(Path.GetDirectoryName(executableFileName));

                        xmlfilename = Path.Combine(programPath, "KeePassCommand.config.xml");
                    }

                    Configuration config;
                    using (var reader = new StreamReader(xmlfilename))
                    {
                        config = (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(reader);
                    }

                    if (!String.IsNullOrWhiteSpace(config.filesystem) && Directory.Exists(config.filesystem))
                        result.filesystem = config.filesystem;
                }
                catch { }

                if (String.IsNullOrWhiteSpace(result.filesystem) && !result.namedpipe)
                    result.namedpipe = true; // if both -namedpipe and -filesystem: are omitted then use the default setting of namedpipe
            }

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
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
