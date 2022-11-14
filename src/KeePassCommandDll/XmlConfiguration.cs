using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace KeePassCommandDll
{
    /*
    KeePassCommand.config.xml

    <?xml version="1.0" encoding="utf-8"?>
    <Configuration>
        <filesystem>s:\incoming\KeePass</filesystem>
    </Configuration>
    */

    [XmlRootAttribute("Configuration", Namespace = "", IsNullable = false)]
    public class XmlConfiguration
    {
        public string filesystem { get; set; }

        public static XmlConfiguration Load()
        {
            string xmlfilename = Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
            xmlfilename = Path.Combine(xmlfilename, "KeePassCommand.config.xml");

            using (var reader = new StreamReader(xmlfilename))
            {
                return (XmlConfiguration)new XmlSerializer(typeof(XmlConfiguration)).Deserialize(reader);
            }
        }
    }
}
