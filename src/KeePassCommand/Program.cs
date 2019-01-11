using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace KeePassCommand
{
    class Program
    {
        static private string ServerPipeName;
        private const string EndOfResponse = "\t\t\t[--- end of response ---]\t\t\t";

        static void Main(string[] args)
        {
            StringBuilder output = new StringBuilder();
            string outfile = String.Empty;

            try
            {
                ServerPipeName = "KeePassCommander." + Process.GetCurrentProcess().SessionId;

                StringBuilder command = new StringBuilder();
                foreach (var arg in args)
                {
                    if (arg.Length > 5 && arg.Substring(0, 5).ToLower() == "-out:")
                    {
                        outfile = arg.Substring(5);
                    }
                    else
                    {
                        command.Append(arg);
                        command.Append('\t');
                    }
                }

                NamedPipeClientStream connection = new NamedPipeClientStream(".", ServerPipeName, PipeDirection.InOut, PipeOptions.None);
                connection.Connect(5000);

                StreamReader reader = new StreamReader(connection, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(connection, Encoding.UTF8);

                writer.WriteLine(command.ToString());
                writer.Flush();
                connection.Flush();

                output.Length = 0;
                output.AppendLine("SUCCESS");
                while (true)
                {
                    string response = reader.ReadLine();
                    if (response == EndOfResponse) break;

                    if (response.Length > 0)
                    {
                        string[] parts = response.Split('\t');
                        output.AppendLine("B\t");
                        foreach (var part in parts)
                        {
                            output.AppendLine("I\t" + part);
                        }
                        output.AppendLine("E\t");
                    }
                }

                connection.Dispose();
            }
            catch (Exception ex)
            {
                output.Length = 0;
                output.AppendLine("ERROR");
                output.AppendLine(ex.Message);
            }

            if (outfile.Length > 0)
            {
                using (StreamWriter file = new StreamWriter(outfile, false, Encoding.Default))
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
