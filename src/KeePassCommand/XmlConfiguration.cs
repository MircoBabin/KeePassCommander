using System.IO;
using System.Xml.Serialization;

namespace KeePassCommand
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

        public static XmlConfiguration Load(string xmlfilename)
        {
            using (var reader = new StreamReader(xmlfilename))
            {
                return (XmlConfiguration)new XmlSerializer(typeof(XmlConfiguration)).Deserialize(reader);
            }
        }
    }
}
