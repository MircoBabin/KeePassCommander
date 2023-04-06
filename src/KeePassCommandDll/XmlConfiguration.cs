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

        [XmlIgnoreAttribute]
        private string _xmlfilename = null;

        private void SetXmlFilename(string Filename)
        {
            _xmlfilename = Filename;
        }

        public string GetXmlFilename()
        {
            return _xmlfilename;
        }

        public static XmlConfiguration Load()
        {
            string myPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetAssembly(typeof(XmlConfiguration)).Location));
            string xmlfilename = Path.Combine(myPath, "KeePassCommand.config.xml");

            using (var reader = new StreamReader(xmlfilename))
            {
                var config = (XmlConfiguration)new XmlSerializer(typeof(XmlConfiguration)).Deserialize(reader);
                config.SetXmlFilename(xmlfilename);
                return config;
            }
        }
    }
}
