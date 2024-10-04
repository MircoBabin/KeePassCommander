using System;
using System.IO;
using System.Text;

namespace KeePassCommand
{
    public static class OutputUtils
    {
        public static void OutputBinary(ProgramArguments options, byte[] data)
        {
            if (!String.IsNullOrWhiteSpace(options.outfile))
            {
                File.WriteAllBytes(options.outfile, data);
            }
            else
            {
                using (var consoleStream = Console.OpenStandardOutput())
                {
                    consoleStream.Write(data, 0, data.Length);
                }
            }
        }

        public static void OutputString(ProgramArguments options, StringBuilder data)
        {
            OutputString(options, data.ToString());
        }

        public static void OutputString(ProgramArguments options, string data)
        {
            if (!String.IsNullOrWhiteSpace(options.outfile))
            {
                using (StreamWriter file = new StreamWriter(options.outfile, false, options.outfile_encoding))
                {
                    file.Write(data);
                }
            }
            else
            {
                switch(options.stdout_encoding)
                {
                    case ProgramArguments.StdoutEncodingType.Default:
                        Console.Write(data);
                        break;

                    case ProgramArguments.StdoutEncodingType.Utf8:
                    case ProgramArguments.StdoutEncodingType.Utf8WithoutBom:
                        {
                            Encoding utf8WithoutBom = new UTF8Encoding(false);
                            var utf8Bytes = utf8WithoutBom.GetBytes(data);

                            if (options.stdout_encoding == ProgramArguments.StdoutEncodingType.Utf8)
                            {
                                var bomBytes = Encoding.UTF8.GetPreamble();

                                byte[] combined = new byte[bomBytes.Length + utf8Bytes.Length];
                                Buffer.BlockCopy(bomBytes, 0, combined, 0, bomBytes.Length);
                                Buffer.BlockCopy(utf8Bytes, 0, combined, bomBytes.Length, utf8Bytes.Length);

                                utf8Bytes = combined;
                            }

                            using (var consoleStream = Console.OpenStandardOutput())
                            {
                                consoleStream.Write(utf8Bytes, 0, utf8Bytes.Length);
                            }
                        }
                        break;

                    default:
                        throw new Exception("Unknown stdout encoding: " + options.stdout_encoding);
                }
            }
        }
    }
}
