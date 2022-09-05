using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace KeePassCommandDll.Communication
{
    public class SendCommand
    {
        private string ServerPipeName;
        private string Command;

        private StreamReader reader;

        private const string BeginOfResponse = "\t\t\t[--- begin of response 3.0 ---]\t\t\t";
        private const string EndOfResponse = "\t\t\t[--- end of response 3.0 ---]\t\t\t";

        public Response Response = new Response();
        private string ResponseForCommand;
        public enum ResponseLayoutType { none, default_1_column, default_2_column };
        public ResponseLayoutType ResponseType;

        public SendCommand(string command)
        {
            ServerPipeName = "KeePassCommander." + Process.GetCurrentProcess().SessionId;
            Command = command;

            Execute();
        }

        private void Execute()
        {
            NamedPipeClientStream connection = new NamedPipeClientStream(".", ServerPipeName, PipeDirection.InOut, PipeOptions.None);
            try
            {
                connection.Connect(5000);
                reader = new StreamReader(connection, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(connection, Encoding.UTF8);

                writer.WriteLine(Command.ToString());
                writer.Flush();
                connection.Flush();

                ReadBeginOfResponse();
                while (true)
                {
                    string response = reader.ReadLine();
                    if (response == EndOfResponse) break;

                    if (response.Length > 0)
                    {
                        string[] parts = response.Split('\t');
                        if (parts.Length > 1)
                        {
                            //Each line ends with \t, when there are at least 2 parts, the last part is not valid. 
                            //If there is only one part, no \t was present

                            Array.Resize(ref parts, parts.Length - 1);
                        }

                        switch (ResponseType)
                        {
                            case ResponseLayoutType.default_1_column:
                                HandleParts_Default_1_Column(parts);
                                break;

                            case ResponseLayoutType.default_2_column:
                                HandleParts_Default_2_Column(parts);
                                break;

                            default:
                                throw new Exception("Unknown response type: " + ResponseType.ToString());
                        }
                    }
                }
            }
            finally
            {
                connection.Dispose();
            }
        }

        private void ReadBeginOfResponse()
        {
            //First line BEGIN OF RESPONSE
            string response;
            int bracketopen, bracketclose;

            response = reader.ReadLine();
            if (!response.StartsWith(BeginOfResponse))
            {
                throw new Exception("Returned response is not valid, first line should be [--- begin of response ---][command][layout], is: " + response);
            }
            response = response.Remove(0, BeginOfResponse.Length);

            bracketopen = response.IndexOf('[');
            bracketclose = response.IndexOf(']');
            if (bracketopen != 0 || bracketclose <= bracketopen || (bracketclose - bracketopen - 1) <= 0)
            {
                throw new Exception("Returned response is not valid, first line should be [--- begin of response ---][command][layout], command not found [" + bracketopen + "-" + bracketclose + "]: " + response);
            }
            ResponseForCommand = response.Substring(bracketopen + 1, bracketclose - bracketopen - 1);
            response = response.Remove(0, bracketclose + 1);

            bracketopen = response.IndexOf('[');
            bracketclose = response.IndexOf(']');
            if (bracketopen != 0 || bracketclose <= bracketopen || (bracketclose - bracketopen - 1) <= 0)
            {
                throw new Exception("Returned response is not valid, first line should be [--- begin of response ---][command][layout], layout not found [" + bracketopen + "-" + bracketclose + "]: " + response);
            }
            string responselayout = response.Substring(bracketopen + 1, bracketclose - bracketopen - 1);
            response = response.Remove(0, bracketclose + 1);

            if (responselayout == "default-1-column")
                ResponseType = ResponseLayoutType.default_1_column;
            else if (responselayout == "default-2-column")
                ResponseType = ResponseLayoutType.default_2_column;
            else
                throw new Exception("Returned response has an unknown layout type: " + responselayout);
        }

        private void HandleParts_Default_1_Column(string[] parts)
        {
            List<ResponseItem> columns = new List<ResponseItem>();

            for (int i = 0; i < parts.Length; i++)
            {
                columns.Add(new ResponseItem(parts[i]));
            }

            Response.Entries.Add(columns);
        }

        private void HandleParts_Default_2_Column(string[] parts)
        {
            List<ResponseItem> columns = new List<ResponseItem>();

            for (int i = 0; i+1 < parts.Length; i+=2)
            {
                columns.Add(new ResponseItem(parts[i], parts[i+1]));
            }

            Response.Entries.Add(columns);
        }
    }
}
