using System;
using System.Diagnostics;
using System.IO;

namespace CsharpExample
{
    class Program
    {
        static string FindKeePassCommandDll()
        {
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
            string dll = FindKeePassCommandDll();
            if (dll == null)
            {
                Console.WriteLine("Error locating KeePassCommandDll.dll");
                Environment.Exit(1);
                return;
            }
            KeePassEntry.Initialize(dll);

            var entry = KeePassEntry.get("Sample Entry");
            if (entry == null)
            {
                Console.WriteLine("KeePass is not started");
                Console.WriteLine("Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?");
                Environment.Exit(2);
                return;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine();
            Console.WriteLine("title     : " + entry.Title);
            Console.WriteLine("username  : " + entry.Username);
            Console.WriteLine("password  : " + entry.Password);
            Console.WriteLine("url       : " + entry.Url);
            Console.WriteLine("urlscheme : " + entry.UrlScheme);
            Console.WriteLine("urlhost   : " + entry.UrlHost);
            Console.WriteLine("urlport   : " + entry.UrlPort);
            Console.WriteLine("urlpath   : " + entry.UrlPath);
            Console.WriteLine("notes     : " + entry.Notes);
            Environment.Exit(0);
        }
    }
}
