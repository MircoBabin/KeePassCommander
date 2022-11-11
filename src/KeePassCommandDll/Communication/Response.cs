using System;
using System.Collections.Generic;
using System.IO;

namespace KeePassCommandDll.Communication
{
    public class Response
    {
        public List<List<ResponseItem>> Entries = new List<List<ResponseItem>>();

        private const string BeginOfResponse = "\t\t\t[--- begin of response 3.0 ---]\t\t\t";
        private const string EndOfResponse = "\t\t\t[--- end of response 3.0 ---]\t\t\t";

        private string ResponseForCommand;
        public enum ResponseLayoutType { none, default_1_column, default_2_column };
        public ResponseLayoutType ResponseType;

        public void ReadFromStream(StreamReader reader)
        {
            ReadFromStream(new LineReader(reader));
        }

        public void ReadFromStream(StringReader reader)
        {
            ReadFromStream(new LineReader(reader));
        }

        public void ReadFromStream(LineReader reader)
        {
            ReadBeginOfResponse(reader);
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

        private void ReadBeginOfResponse(LineReader reader)
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

            Entries.Add(columns);
        }

        private void HandleParts_Default_2_Column(string[] parts)
        {
            List<ResponseItem> columns = new List<ResponseItem>();

            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                columns.Add(new ResponseItem(parts[i], parts[i + 1]));
            }

            Entries.Add(columns);
        }
    }
}
