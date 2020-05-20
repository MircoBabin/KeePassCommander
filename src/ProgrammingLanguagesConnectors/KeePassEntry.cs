/*
KeePass Commander
https://github.com/MircoBabin/KeePassCommander - MIT license 

Copyright (c) 2018 Mirco Babin

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Reflection;

namespace CsharpExample
{
    public class KeePassEntry
    {
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string UrlScheme { get; set; }
        public string UrlHost { get; set; }
        public string UrlPort { get; set; }
        public string UrlPath { get; set; }
        public string Notes { get; set; }

        public static KeePassEntry get(string title)
        {
            if (KeePassCommandDll_ApiGetfirst == null) throw new Exception("Call KeePassEntry.Initialize() first");

            var result = KeePassCommandDll_ApiGetfirst.Invoke(null, new object[] { title });
            if (result == null) return new KeePassEntry();

            Type ApiGetResponse = result.GetType();

            var entry = new KeePassEntry();
            entry.Title = GetResultProperty(result, ApiGetResponse, "Title");
            entry.Username = GetResultProperty(result, ApiGetResponse, "Username");
            entry.Password = GetResultProperty(result, ApiGetResponse, "Password");
            entry.Url = GetResultProperty(result, ApiGetResponse, "Url");
            entry.UrlScheme = GetResultProperty(result, ApiGetResponse, "UrlScheme");
            entry.UrlHost = GetResultProperty(result, ApiGetResponse, "UrlHost");
            entry.UrlPort = GetResultProperty(result, ApiGetResponse, "UrlPort");
            entry.UrlPath = GetResultProperty(result, ApiGetResponse, "UrlPath");
            entry.Notes = GetResultProperty(result, ApiGetResponse, "Notes");

            return entry;
        }

        protected static string GetResultProperty(object obj, Type type, string propertyName)
        {
            PropertyInfo prop = type.GetProperty(propertyName);
            if (prop == null) return null;

            return (string)prop.GetValue(obj, null);
        }

        #region KeePassCommandDll loading
        protected static Assembly KeePassCommandDll = null;
        protected static Type KeePassCommandDll_Api = null;
        protected static MethodInfo KeePassCommandDll_ApiGetfirst = null;

        public static void Initialize(string KeePassCommandDllPath)
        {
            if (KeePassCommandDll_ApiGetfirst != null) throw new Exception("KeePassEntry.Initialize() has already been called");

            if (String.IsNullOrEmpty(KeePassCommandDllPath))
                throw new Exception("KeePassCommandDllPath is not provided");

            if (!File.Exists(KeePassCommandDllPath))
                throw new Exception("KeePassCommandDll.dll does not exist: " + KeePassCommandDllPath);

            KeePassCommandDll = Assembly.LoadFile(KeePassCommandDllPath);
            if (KeePassCommandDll == null) throw new Exception("Error loading KeePassCommandDll.dll [assembly] from " + KeePassCommandDllPath);

            KeePassCommandDll_Api = KeePassCommandDll.GetType("KeePassCommandDll.Api");
            if (KeePassCommandDll_Api == null) throw new Exception("Error loading KeePassCommandDll.dll [KeePassCommandDll.Api class] from " + KeePassCommandDllPath);

            KeePassCommandDll_ApiGetfirst = KeePassCommandDll_Api.GetMethod("getfirst", new Type[] { typeof(string) });
            if (KeePassCommandDll_ApiGetfirst == null) throw new Exception("Error loading KeePassCommandDll.dll [KeePassCommandDll.Api.getfirst(string) method] from " + KeePassCommandDllPath);
        }
        #endregion
    }
}
