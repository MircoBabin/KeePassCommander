using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
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

        static void Main(string[] args)
        {
            StringBuilder output = new StringBuilder();
            string outfile = String.Empty;
            Encoding outfile_encoding = null;
            string outcommand = String.Empty;
            string filesystem = String.Empty;

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

                StringBuilder command = new StringBuilder();
                foreach (var arg in args)
                {
                    if (arg.Length > 5 && arg.Substring(0, 5).ToLower() == "-out:")
                    {
                        outfile = arg.Substring(5);
                        outfile_encoding = Encoding.Default; //ANSI encoding on the computer executing this executable
                    }
                    else if (arg.Length > 10 && arg.Substring(0, 10).ToLower() == "-out-utf8:")
                    {
                        outfile = arg.Substring(10);
                        outfile_encoding = Encoding.UTF8;
                    }
                    else if (arg.Length > 12 && arg.Substring(0, 12).ToLower() == "-filesystem:")
                    {
                        filesystem = arg.Substring(12);
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(outcommand))
                        {
                            outcommand = arg;
                            if (outcommand == "get")
                            {
                                command.Append("get");
                            }
                            else if (outcommand == "getfield" || outcommand == "getfieldraw")
                            {
                                command.Append("getfield");
                            }
                            else if (outcommand == "getattachment" || outcommand == "getattachmentraw")
                            {
                                command.Append("getattachment");
                            }
                            else if (outcommand == "getnote" || outcommand == "getnoteraw")
                            {
                                command.Append("getnote");
                            }
                            else if (outcommand == "listgroup")
                            {
                                command.Append("listgroup");
                            }
                            else
                            {
                                throw new Exception("unknown command: " + outcommand);
                            }
                        }
                        else
                        {
                            command.Append(arg);
                        }

                        command.Append('\t');
                    }
                }

                ISendCommand send;
                if (!String.IsNullOrWhiteSpace(filesystem) && Directory.Exists(filesystem))
                    send = new SendCommandViaFileSystem(filesystem, command.ToString());
                else
                    send = new SendCommandViaNamedPipe(command.ToString());

                if (outcommand == "getfieldraw")
                {
                    if (outfile.Length == 0)
                        throw new Exception("getfieldraw must be used in combination with -out: or -out-utf8:");

                    if (send.Response.ResponseType != Response.ResponseLayoutType.default_2_column)
                        throw new Exception("getfieldraw response type should be default_2_column, but is: " + send.Response.ResponseType.ToString());

                    if (send.Response.Entries.Count != 1)
                        throw new Exception("getfieldraw must query exactly one entry");

                    List<ResponseItem> entry = send.Response.Entries[0];
                    if (entry.Count != 2 || entry[0].Parts[0] != "title")
                        throw new Exception("getfieldraw must query exactly one entry");

                    string fieldvalue = Encoding.UTF8.GetString(Convert.FromBase64String(entry[1].Parts[1]));
                    using (StreamWriter file = new StreamWriter(outfile, false, outfile_encoding))
                    {
                        file.Write(fieldvalue.ToString());
                    }
                    return;
                }

                if (outcommand == "getattachmentraw")
                {
                    if (outfile.Length == 0)
                        throw new Exception("getattachmentraw must be used in combination with -out: or -out-utf8: (will always be saved binary)");

                    if (send.Response.ResponseType != Response.ResponseLayoutType.default_2_column)
                        throw new Exception("getattachmentraw response type should be default_2_column, but is: " + send.Response.ResponseType.ToString());

                    if (send.Response.Entries.Count != 1)
                        throw new Exception("getattachmentraw must query exactly one entry");

                    List<ResponseItem> entry = send.Response.Entries[0];
                    if (entry.Count != 2 || entry[0].Parts[0] != "title")
                        throw new Exception("getattachmentraw must query exactly one entry");

                    byte[] attachment = Convert.FromBase64String(entry[1].Parts[1]);
                    File.WriteAllBytes(outfile, attachment);
                    return;
                }

                if (outcommand == "getnoteraw")
                {
                    if (outfile.Length == 0)
                        throw new Exception("getnoteraw must be used in combination with -out: or -out-utf8:");

                    if (send.Response.ResponseType != Response.ResponseLayoutType.default_1_column)
                        throw new Exception("getnoteraw response type should be default_1_column, but is: " + send.Response.ResponseType.ToString());

                    if (send.Response.Entries.Count != 1)
                        throw new Exception("getnoteraw must query exactly one entry");

                    List<ResponseItem> entry = send.Response.Entries[0];
                    if (entry.Count != 2)
                        throw new Exception("getnoteraw must query exactly one entry");

                    string notes = Encoding.UTF8.GetString(Convert.FromBase64String(entry[1].Parts[0]));
                    using (StreamWriter file = new StreamWriter(outfile, false, outfile_encoding))
                    {
                        file.Write(notes.ToString());
                    }
                    return;
                }

                if (outcommand == "listgroup")
                {
                    // No success or failure indication, just one title per line, each line terminated with NEWLINE.
                    // On FAILURE the output will be empty (0 bytes).
                    // Unique sorted on title.

                    SortedDictionary<string, string> unique = new SortedDictionary<string, string>();
                    foreach (List<ResponseItem> entry in send.Response.Entries)
                    {
                        string title = entry[0].Parts[0];
                        if (!unique.ContainsKey(title))
                            unique.Add(title, title);
                    }

                    foreach(string title in unique.Values)
                    {
                        output.AppendLine(title);
                    }

                    if (outfile.Length > 0)
                    {
                        using (StreamWriter file = new StreamWriter(outfile, false, outfile_encoding))
                        {
                            file.Write(output.ToString());
                        }
                    }
                    else
                    {
                        Console.Write(output.ToString());
                    }

                    return;
                }

                switch (send.Response.ResponseType)
                {
                    case Response.ResponseLayoutType.default_1_column:
                    case Response.ResponseLayoutType.default_2_column:
                        output.AppendLine("SUCCESS");
                        break;

                    default:
                        throw new Exception("Unknown response layout: " + send.Response.ResponseType.ToString());
                }

                foreach (List<ResponseItem> entry in send.Response.Entries)
                {
                    switch(send.Response.ResponseType)
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
            }
            catch (Exception ex)
            {
                output.Length = 0;
                output.AppendLine("ERROR");
                output.AppendLine(ex.Message);
            }

            if (outfile.Length > 0)
            {
                using (StreamWriter file = new StreamWriter(outfile, false, outfile_encoding))
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
