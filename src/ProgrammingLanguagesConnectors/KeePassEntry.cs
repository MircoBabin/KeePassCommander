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



/*
This source file can be included to your personal project and then compiled.
This loads KeePassCommandDll.dll dynamically. 
- No need to reference KeePassCommandDll.dll. 
- No need to copy KeePassCommandDll.dll to your personal project.
- No version mismatch problems.


As an alternative your personal project can also directly reference KeePassCommandDll.dll.
And directly use KeePassCommand.Api
*/



using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace KeePassCommand
{
    public class KeePassEntryField
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class KeePassEntryAttachment
    {
        public string Name { get; set; }
        public byte[] Value { get; set; }
    }

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

        public List<KeePassEntryField> Fields { get; set; }

        public List<KeePassEntryAttachment> Attachments { get; set; }

        public KeePassEntry()
        {
            this.Fields = new List<KeePassEntryField>();
            this.Attachments = new List<KeePassEntryAttachment>();
        }

        public static KeePassEntry getfirst(string title, string[] fieldNames = null, string[] attachmentNames = null)
        {
            if (KeePassCommandDll_ApiGetfirst == null) throw new Exception("Call KeePassEntry.Initialize() first");

            var entry = new KeePassEntry();

            {
                var result = KeePassCommandDll_ApiGetfirst.Invoke(null, new object[] { title });
                if (result == null) return entry;

                Type ApiGetResponse = result.GetType();

                entry.Title = GetResultPropertyString(result, ApiGetResponse, "Title");
                entry.Username = GetResultPropertyString(result, ApiGetResponse, "Username");
                entry.Password = GetResultPropertyString(result, ApiGetResponse, "Password");
                entry.Url = GetResultPropertyString(result, ApiGetResponse, "Url");
                entry.UrlScheme = GetResultPropertyString(result, ApiGetResponse, "UrlScheme");
                entry.UrlHost = GetResultPropertyString(result, ApiGetResponse, "UrlHost");
                entry.UrlPort = GetResultPropertyString(result, ApiGetResponse, "UrlPort");
                entry.UrlPath = GetResultPropertyString(result, ApiGetResponse, "UrlPath");
                entry.Notes = GetResultPropertyString(result, ApiGetResponse, "Notes");
            }

            if (fieldNames != null && fieldNames.Length > 0)
            {
                var result = KeePassCommandDll_ApiGetfield.Invoke(null, new object[] { title, fieldNames });

                if (result != null)
                {
                    foreach (var item in (IEnumerable)result)
                    {
                        Type ApiGetfieldResponse = item.GetType();

                        var field = new KeePassEntryField();
                        field.Name = GetResultPropertyString(item, ApiGetfieldResponse, "Name");
                        field.Value = GetResultPropertyString(item, ApiGetfieldResponse, "Value");

                        entry.Fields.Add(field);
                    }
                }
            }

            if (attachmentNames != null && attachmentNames.Length > 0)
            {
                var result = KeePassCommandDll_ApiGetattachment.Invoke(null, new object[] { title, attachmentNames });

                if (result != null)
                {
                    foreach (var item in (IEnumerable)result)
                    {
                        Type ApiGetattachmentResponse = item.GetType();

                        var attachment = new KeePassEntryAttachment();
                        attachment.Name = GetResultPropertyString(item, ApiGetattachmentResponse, "Name");
                        attachment.Value = GetResultPropertyBytes(item, ApiGetattachmentResponse, "Value");

                        entry.Attachments.Add(attachment);
                    }
                }
            }

            return entry;
        }

        protected static string GetResultPropertyString(object obj, Type type, string propertyName)
        {
            PropertyInfo prop = type.GetProperty(propertyName);
            if (prop == null) return null;

            return (string)prop.GetValue(obj, null);
        }

        protected static byte[] GetResultPropertyBytes(object obj, Type type, string propertyName)
        {
            PropertyInfo prop = type.GetProperty(propertyName);
            if (prop == null) return null;

            return (byte[])prop.GetValue(obj, null);
        }


        #region KeePassCommandDll loading
        protected static Assembly KeePassCommandDll = null;
        protected static Type KeePassCommandDll_Api = null;
        protected static MethodInfo KeePassCommandDll_ApiGetfirst = null;
        protected static MethodInfo KeePassCommandDll_ApiGetfield = null;
        protected static MethodInfo KeePassCommandDll_ApiGetattachment = null;

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

            KeePassCommandDll_ApiGetfield = KeePassCommandDll_Api.GetMethod("getfield", new Type[] { typeof(string), typeof(string[]) });
            if (KeePassCommandDll_ApiGetfield == null) throw new Exception("Error loading KeePassCommandDll.dll [KeePassCommandDll.Api.getfield(string, string[]) method] from " + KeePassCommandDllPath);

            KeePassCommandDll_ApiGetattachment = KeePassCommandDll_Api.GetMethod("getattachment", new Type[] { typeof(string), typeof(string[]) });
            if (KeePassCommandDll_ApiGetattachment == null) throw new Exception("Error loading KeePassCommandDll.dll [KeePassCommandDll.Api.getattachment(string, string[]) method] from " + KeePassCommandDllPath);
        }
        #endregion
    }
}
