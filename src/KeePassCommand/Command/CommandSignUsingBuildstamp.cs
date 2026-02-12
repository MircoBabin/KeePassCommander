using KeePassCommandDll.Communication;
using KeePassCommander;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeePassCommand.Command
{
    public class CommandSignUsingBuildstamp : ICommand, ICommandHasExitCode
    {
        private string _filename;
        private string _filenameOnly;
        public int ExitCode { get; private set; }

        public CommandSignUsingBuildstamp(string filename, string filenameOnly)
        {
            _filename = filename;
            _filenameOnly = filenameOnly;
            ExitCode = 99;
        }

        public void Run(ProgramArguments options, ISendCommand send)
        {
            try
            {
                if (send.Response.ResponseType != Response.ResponseLayoutType.default_2_column)
                    throw new Exception("sign-using-buildstamp response type should be default_2_column, but is: " + send.Response.ResponseType.ToString());

                if (send.Response.Entries.Count != 1)
                    throw new Exception("sign-using-buildstamp must query exactly one entry");

                List<ResponseItem> entry = send.Response.Entries[0];
                if (entry.Count < 4)
                    throw new Exception("sign-using-buildstamp response failure, expected 4 rows.");

                if (entry[0].Parts[0] != "exitcode")
                    throw new Exception("sign-using-buildstamp response failure, expected row 0 \"exitcode\".");
                int buildstampExitCode = Convert.ToInt32(entry[0].Parts[1]);

                if (entry[1].Parts[0] != "stdout")
                    throw new Exception("sign-using-buildstamp response failure, expected row 1 \"stdout\".");
                string stdout = Encoding.UTF8.GetString(Convert.FromBase64String(entry[1].Parts[1]));

                if (entry[2].Parts[0] != "stderr")
                    throw new Exception("sign-using-buildstamp response failure, expected row 2 \"stderr\".");
                string stderr = Encoding.UTF8.GetString(Convert.FromBase64String(entry[2].Parts[1]));

                byte[] signedBytes = null;
                if (buildstampExitCode == 0)
                {
                    string responseFilenameOnly = Encoding.UTF8.GetString(Convert.FromBase64String(entry[3].Parts[0]));
                    if (responseFilenameOnly != _filenameOnly)
                        throw new Exception("sign-using-buildstamp response failure, expected row 3 \"" + _filenameOnly + "\".");
                    signedBytes = Convert.FromBase64String(entry[3].Parts[1]);

                    File.WriteAllBytes(_filename, signedBytes);
                }

                StringBuilder output = new StringBuilder();
                output.Append("SUCCESS" + KeePassCommanderConsts.EOL);
                output.Append(stdout.Replace("\r", string.Empty).Replace("\n", KeePassCommanderConsts.EOL));
                output.Append(KeePassCommanderConsts.EOL);
                output.Append(stderr.Replace("\r", string.Empty).Replace("\n", KeePassCommanderConsts.EOL));
                output.Append(KeePassCommanderConsts.EOL);
                OutputUtils.OutputString(options, output);
                ExitCode = buildstampExitCode;
            }
            catch(Exception ex)
            {
                StringBuilder output = new StringBuilder();
                output.Append("ERROR" + KeePassCommanderConsts.EOL);
                output.Append(ex.Message + KeePassCommanderConsts.EOL);

                OutputUtils.OutputString(options, output);
                ExitCode = 99;
            }
        }
    }
}
