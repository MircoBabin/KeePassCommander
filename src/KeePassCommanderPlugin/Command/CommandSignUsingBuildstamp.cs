using KeePass.Plugins;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeePassCommander.Command
{
    public class CommandSignUsingBuildstamp : ICommand
    {
        public void Run(DebugLog Debug, IPluginHost KeePassHost, string[] parms, StringBuilder output, Dictionary<string, bool> allowedTitles)
        {
            Debug.OutputLine("Starting command sign-using-buildstamp");

            output.Append(Runner.BeginOfResponse + "[getfield][default-2-column]" + KeePassCommanderConsts.EOL);

            Dictionary<string, List<PwEntry>> titles = new Dictionary<string, List<PwEntry>>();
            {
                string name = (parms.Length >= 2 ? parms[1].Trim() : String.Empty);
                if (!string.IsNullOrEmpty(name))
                {
                    titles.Add(name, new List<PwEntry>());
                }
                EntriesHelper.FindTitles(Debug, KeePassHost, titles, allowedTitles);
            }

            string filenameOnly = (parms.Length >= 3 ? Encoding.UTF8.GetString(Convert.FromBase64String(parms[2])) : String.Empty);
            byte[] fileBytes = (parms.Length >= 4 ? Convert.FromBase64String(parms[3]) : null);

            if (titles.Count == 1)
            {
                foreach(var keypair in titles)
                {
                    if (keypair.Value.Count == 1)
                    {
                        foreach (PwEntry entry in keypair.Value)
                        {
                            SignUsingEntry(Debug, KeePassHost, output, entry, filenameOnly, fileBytes);
                            break;
                        }
                    }
                    break;
                }
            }

            Debug.OutputLine("Ended command sign-using-buildstamp");
        }

        private void SignUsingEntry(DebugLog Debug, IPluginHost KeePassHost, StringBuilder output, PwEntry entry, string filenameOnly, byte[] fileBytes)
        {
            string buildstamp_exe = null;
            try
            {
                buildstamp_exe = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, "buildstamp-exe[" + Environment.MachineName.ToLowerInvariant() + "]");
                if (string.IsNullOrEmpty(buildstamp_exe)) buildstamp_exe = null;
            }
            catch { }

            if (buildstamp_exe == null)
            {
                try
                {
                    buildstamp_exe = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, "buildstamp-exe");
                    if (string.IsNullOrEmpty(buildstamp_exe)) buildstamp_exe = null;
                }
                catch { }
            }

            if (string.IsNullOrEmpty(buildstamp_exe))
            {
                Respond(output, 99, "", "Buildstamp.exe is not configured.", filenameOnly, null);
                return;
            }

            buildstamp_exe = Environment.ExpandEnvironmentVariables(buildstamp_exe);
            if (!File.Exists(buildstamp_exe))
            {
                Respond(output, 99, "", "Buildstamp.exe does not exist on KeePass host.", filenameOnly, null);
                return;
            }

            string pkcs11_driver = null;
            try
            {
                pkcs11_driver = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, "--pkcs11-driver[" + Environment.MachineName.ToLowerInvariant() + "]");
                if (string.IsNullOrEmpty(buildstamp_exe)) buildstamp_exe = null;
            }
            catch { }

            if (pkcs11_driver == null)
            {
                try
                {
                    pkcs11_driver = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, "--pkcs11-driver");
                    if (string.IsNullOrEmpty(buildstamp_exe)) buildstamp_exe = null;
                }
                catch { }
            }

            bool launchdebugger = false;
            try
            {
                var value = EntriesHelper.GetEntryField(Debug, KeePassHost, entry, "--launchdebugger");
                if (value.Trim().ToLowerInvariant() == "true") launchdebugger = true;
            }
            catch { }


            string KeePassCommanderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(this.GetType()).Location);
            string titleOption = (!string.IsNullOrEmpty(pkcs11_driver) ? "--keepass-pkcs11-title" : "--keepass-certificate-title");

            string tempFilename = Path.GetTempFileName();
            try
            {
                try
                {
                    File.WriteAllBytes(tempFilename, fileBytes);

                    byte[] writtenBytes = File.ReadAllBytes(tempFilename);
                    if (writtenBytes.Length != fileBytes.Length)
                        throw new Exception("Written file has incorrect length.");

                    for (var i = 0; i < fileBytes.Length; i++)
                    {
                        if (writtenBytes[i] != fileBytes[i])
                            throw new Exception("Written file is invalid.");
                    }
                }
                catch (Exception ex)
                {
                    Respond(output, 99, "", "Error writing fileBytes on KeePass Host. " + ex.Message, filenameOnly, null);
                    return;
                }

                List<string> buildstampArguments = new List<string>()
                {
                    "sign",
                    "--keepasscommander-path", KeePassCommanderPath,
                    titleOption, EntriesHelper.GetEntryField(Debug, KeePassHost, entry, PwDefs.TitleField),
                    "--filename", tempFilename,
                };

                if (launchdebugger)
                {
                    buildstampArguments.Add("--launchdebugger");
                }

                ConsoleExecutor.ConsoleExecutor executor = null;
                try
                {
                    try
                    {
                        executor = new ConsoleExecutor.ConsoleExecutor(
                            buildstamp_exe, buildstampArguments, null, null, null);

                        executor.WaitFor();
                        int exitcode = executor.ExitCode;
                        if (exitcode == 0)
                            Respond(output, 0, executor.Output, executor.Error, filenameOnly, File.ReadAllBytes(tempFilename));
                        else
                            Respond(output, exitcode, executor.Output, executor.Error, filenameOnly, null);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Respond(output, 99, "", "Buildstamp.exe failed on the KeePass host. " + ex.Message, filenameOnly, null);
                        return;
                    }
                }
                finally
                {
                    executor.Dispose();
                }
            }
            finally
            {
                try { File.Delete(tempFilename);  } catch { }
            }
        }

        private void Respond(StringBuilder output, int exitcode, string stdout, string stderr, string filenameOnly, byte[] signedBytes)
        {
            output.Append("exitcode");
            output.Append("\t");
            output.Append(exitcode);
            output.Append("\t");

            output.Append("stdout");
            output.Append("\t");
            output.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(stdout)));
            output.Append("\t");

            output.Append("stderr");
            output.Append("\t");
            output.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(stderr)));
            output.Append("\t");

            output.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(filenameOnly)));
            output.Append("\t");
            output.Append(signedBytes != null ? Convert.ToBase64String(signedBytes) : string.Empty);
            output.Append("\t");

            output.Append(KeePassCommanderConsts.EOL);
        }
    }
}
