using KeePassCommand; //see https://github.com/MircoBabin/KeePassCommander/blob/master/src/ProgrammingLanguagesConnectors/KeePassEntry.cs
using System;
using System.Diagnostics;
using System.IO;

namespace CsharpExample
{
    class Program
    {
        static string FindKeePassCommandDll(string fixedFilename)
        {
            if (!string.IsNullOrWhiteSpace(fixedFilename))
                return fixedFilename;

            string basepath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            #if DEBUG
                var subpath = "Debug";
            #else
                var subpath = "Release";
            #endif

            string path;

            // current directory
            path = Path.Combine(basepath, "KeePassCommandDll.dll");
            if (File.Exists(path)) return path;

            // example\CsharpExample\bin\Debug\CsharpExample.exe
            path = Path.Combine(basepath, "..\\..\\..\\..\\bin\\", subpath, "KeePassCommandDll.dll");
            if (File.Exists(path)) return path;

            // example\CsharpExample.exe
            path = Path.Combine(basepath, "..\\bin\\", subpath, "KeePassCommandDll.dll");
            if (File.Exists(path)) return path;

            return null;
        }

        static void Main(string[] args)
        {
            /* args[0] = <...\KeePassCommandDll.dll> --> (optional) full path of KeePassCommandDll.dll
             * args[1] = "namedpipe"                 --> (optional) use namedpipe communication
             * args[1] = <path>                      --> (optional) use filesystem communication via <path>
             */
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string dll = FindKeePassCommandDll(args.Length > 0 ? args[0] : null);
            if (dll != null)
                dll = Path.GetFullPath(dll);
            if (dll == null || !File.Exists(dll))
            {
                Console.WriteLine(dll);
                Console.WriteLine("KeePassCommandDll.dll does not exist.");
                Environment.Exit(1);
                return;
            }

            if (args.Length > 1)
            {
                if (args[1] == "namedpipe")
                {
                    Console.WriteLine("Using namedpipe communication.");
                    KeePassEntry.Initialize(dll, KeePassCommand.CommunicationType.NamedPipe);
                }
                else
                {
                    Console.WriteLine("Using filesystem communication: " + args[1] + ".");
                    KeePassEntry.Initialize(dll, KeePassCommand.CommunicationType.FileSystem, args[1]);
                }
            }
            else
            {
                Console.WriteLine("Using automatically determined communication via KeePassCommand.config.xml.");
                KeePassEntry.Initialize(dll);
            }

            KeePassEntry entry = null;
            string title = "Sample Entry";
            try
            {
                entry = KeePassEntry.getfirst(title, 
                    new string[] { "extra field 1", "extra password 1" },
                    new string[] { "example_attachment.txt" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (entry == null)
            {
                Console.WriteLine("KeePass is not started");
                Console.WriteLine("Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?");
                Environment.Exit(2);
                return;
            }

            foreach (var via in entry.CommunicationVia)
            {
                Console.WriteLine();
                Console.WriteLine(via.Name + " communicated via " + via.SendVia.ToString() + ".");

                if (via.XmlConfigFilename != null)
                {
                    string config = Path.GetFullPath(via.XmlConfigFilename);
                    if (Path.GetDirectoryName(config) == Path.GetDirectoryName(dll))
                    {
                        config = Path.GetFileName(config) + " in same directory as KeePassCommandDll.dll";
                    }

                    Console.WriteLine("Used configuration: " + config);
                }

                if (via.SendVia == CommunicationType.FileSystem)
                {
                    Console.WriteLine("Used filesystem: " + via.FileSystemDirectory);
                }
            }
            Console.WriteLine();

            if (entry.Title != title)
            {
                Console.WriteLine("KeePass Entry not found: " + title);
                Environment.Exit(3);
            }

            Console.WriteLine("title     : " + entry.Title);
            Console.WriteLine("username  : " + entry.Username);
            Console.WriteLine("password  : " + entry.Password);
            Console.WriteLine("url       : " + entry.Url);
            Console.WriteLine("urlscheme : " + entry.UrlScheme);
            Console.WriteLine("urlhost   : " + entry.UrlHost);
            Console.WriteLine("urlport   : " + entry.UrlPort);
            Console.WriteLine("urlpath   : " + entry.UrlPath);
            Console.WriteLine("notes     : " + entry.Notes);

            foreach(var field in entry.Fields)
            {
                Console.WriteLine("field     : " + field.Name);
                Console.WriteLine("     value: " + field.Value);
            }

            foreach (var attachment in entry.Attachments)
            {
                Console.WriteLine("attachment: " + attachment.Name);
                Console.WriteLine("     value: " + BitConverter.ToString(attachment.Value).Replace('-', ' '));
            }

            Environment.Exit(0);
        }
    }
}
