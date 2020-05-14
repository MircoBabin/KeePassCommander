using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeePassCommand
{
    class Program
    {
        private static void ShowHelp()
        {
            StringBuilder sb = new StringBuilder();

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

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
            sb.AppendLine("It's purpose is to provide a communication channel for php-scripts, bat-files, powershell, ... to be able to query the KeePass password store from the commandline without configuration and without password.");

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
            sb.AppendLine("- \"Notes\" are outputted as UTF-8, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get string field");
            sb.AppendLine("KeePassCommand.exe getfield \"KeePass-entry-title\" \"fieldname\" \"fieldname\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe getfield \"Sample Entry\" \"extra field 1\" \"extra password 1\"");

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

            sb.AppendLine("* Advanced get notes");
            sb.AppendLine("KeePassCommand.exe getnote \"KeePass-entry-title\" \"KeePass-entry-title\" ...");
            sb.AppendLine("e.g. KeePassCommand.exe getnote \"Sample Entry\"");
            sb.AppendLine("- \"Notes\" are outputted as UTF-8, base64 encoded.");

            sb.AppendLine();

            sb.AppendLine("* Advanced get notes into file");
            sb.AppendLine("KeePassCommand.exe getnoteraw <-out-utf8:outputfile or -out:> \"KeePass-entry-title\"");
            sb.AppendLine("e.g. KeePassCommand.exe getnoteraw -out-utf8:mynote.txt \"Sample Entry\"");
            sb.AppendLine("- With -out-utf8, \"Notes\" are outputted as UTF-8.");
            sb.AppendLine("- With -out, \"Notes\" are outputted in ANSI codepage.");

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

            try
            {
                if (args.Length == 0)
                {
                    ShowHelp();
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
                    else
                    {
                        if (String.IsNullOrEmpty(outcommand))
                        {
                            outcommand = arg;
                            if (arg == "getattachmentraw")
                            {
                                command.Append("getattachment");
                            }
                            else if (arg == "getnoteraw")
                            {
                                command.Append("getnote");
                            }
                            else
                            {
                                command.Append(arg);
                            }
                        }
                        else
                        {
                            command.Append(arg);
                        }

                        command.Append('\t');
                    }
                }

                SendCommand send = new SendCommand(command.ToString());

                if (outcommand == "getattachmentraw")
                {
                    if (outfile.Length == 0)
                        throw new Exception("getattachmentraw must be used in combination with -out: or -out-utf8: (will always be saved binary)");

                    if (send.ResponseType != SendCommand.ResponseLayoutType.default_2_column)
                        throw new Exception("getattachmentraw response type should be default_2_column, but is: " + send.ResponseType.ToString());

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

                    if (send.ResponseType != SendCommand.ResponseLayoutType.default_1_column)
                        throw new Exception("getnoteraw response type should be default_1_column, but is: " + send.ResponseType.ToString());

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

                switch (send.ResponseType)
                {
                    case SendCommand.ResponseLayoutType.default_1_column:
                    case SendCommand.ResponseLayoutType.default_2_column:
                        output.AppendLine("SUCCESS");
                        break;

                    default:
                        throw new Exception("Unknown response layout: " + send.ResponseType.ToString());
                }

                foreach (List<ResponseItem> entry in send.Response.Entries)
                {
                    switch(send.ResponseType)
                    {
                        case SendCommand.ResponseLayoutType.default_1_column:
                            output.AppendLine("B\t");
                            foreach (ResponseItem item in entry)
                            {
                                output.AppendLine("I\t" + item.Parts[0]);
                            }
                            output.AppendLine("E\t");
                            break;

                        case SendCommand.ResponseLayoutType.default_2_column:
                            output.AppendLine("B\t");
                            foreach (ResponseItem item in entry)
                            {
                                output.AppendLine("I\t" + item.Parts[0] + "\t" + item.Parts[1]);
                            }
                            output.AppendLine("E\t");
                            break;

                        default:
                            throw new Exception("Unknown response layout: " + send.ResponseType.ToString());
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
