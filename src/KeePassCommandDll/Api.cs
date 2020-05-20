using KeePassCommandDll.Communication;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeePassCommandDll
{
    public static class Api
    {
        public static List<ApiGetResponse> get(string KeePassEntryTitle)
        {
            return get(new List<string>() { KeePassEntryTitle });
        }

        public static ApiGetResponse getfirst(string KeePassEntryTitle)
        {
            var results = get(new List<string>() { KeePassEntryTitle });
            if (results.Count == 0) return null;

            return results[0];
        }

        public static List<ApiGetResponse> get(List<string> KeePassEntryTitles)
        {
            StringBuilder command = new StringBuilder("get\t");
            foreach (var name in KeePassEntryTitles)
            {
                command.Append(name);
                command.Append('\t');
            }

            SendCommand send = new SendCommand(command.ToString());

            List<ApiGetResponse> results = new List<ApiGetResponse>();
            foreach (var entry in send.Response.Entries)
            {
                ApiGetResponse result = null;
                switch (send.ResponseType)
                {
                    case SendCommand.ResponseLayoutType.default_1_column:
                        result = new ApiGetResponse();
                        int no = 0;
                        foreach (ResponseItem item in entry)
                        {
                            no++;
                            switch (no)
                            {
                                case 1: //title
                                    result.Title = item.Parts[0];
                                    break;
                                case 2: //username
                                    result.Username = item.Parts[0];
                                    break;
                                case 3: //password
                                    result.Password = item.Parts[0];
                                    break;
                                case 4: //url full
                                    result.Url = item.Parts[0];
                                    break;
                                case 5: //url protocol
                                    result.UrlScheme = item.Parts[0];
                                    break;
                                case 6: //url host
                                    result.UrlHost = item.Parts[0];
                                    break;
                                case 7: //url port
                                    result.UrlPort = item.Parts[0];
                                    break;
                                case 8: //url path
                                    result.UrlPath = item.Parts[0];
                                    break;
                                case 9: //notes base64 encoded utf8-string
                                    result.Notes = Encoding.UTF8.GetString(Convert.FromBase64String(item.Parts[0]));
                                    break;
                            }
                        }
                        break;
                }

                if (result != null) results.Add(result);
            }

            return results;
        }
    }
}
