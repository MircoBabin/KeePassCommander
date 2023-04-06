using System;
using System.IO;

namespace KeePassCommandDll.Communication
{
    public class CommandSender
    {
        public enum CommunicationType { DetermineAutomatically, NamedPipe, FileSystem }

        private XmlConfiguration _xmlConfig;
        private CommunicationType _sendVia;
        private string _fileSystemDirectory;

        public class CommunicationSettings
        {
            public string XmlConfigFilename { get; private set; }
            public CommunicationType SendVia { get; private set; }
            public string FileSystemDirectory { get; private set; }

            public CommunicationSettings(string XmlConfigFilename, CommunicationType SendVia, string FileSystemDirectory)
            {
                this.XmlConfigFilename = XmlConfigFilename;
                this.SendVia = SendVia;
                this.FileSystemDirectory = FileSystemDirectory;
            }
        }

        public CommunicationSettings CommunicationVia { get; private set; }


        public CommandSender(CommunicationType SendVia = CommunicationType.DetermineAutomatically, string FileSystemDirectory = null)
        {
            _xmlConfig = null;
            _fileSystemDirectory = null;
            _sendVia = CommunicationType.NamedPipe;

            switch (SendVia)
            {
                case CommunicationType.NamedPipe:
                    break;

                case CommunicationType.FileSystem:
                    if (string.IsNullOrWhiteSpace(FileSystemDirectory) || !Directory.Exists(FileSystemDirectory))
                        throw new Exception("Communication via filesystem failed, directory does not exist: " + FileSystemDirectory);

                    _sendVia = CommunicationType.FileSystem;
                    _fileSystemDirectory = FileSystemDirectory;
                    break;

                case CommunicationType.DetermineAutomatically:
                    try
                    {
                       _xmlConfig = XmlConfiguration.Load();

                        if (!string.IsNullOrWhiteSpace(_xmlConfig.filesystem) && Directory.Exists(_xmlConfig.filesystem))
                        {
                            _sendVia = CommunicationType.FileSystem;
                            _fileSystemDirectory = _xmlConfig.filesystem;
                        }
                    }
                    catch { }
                    break;

                default:
                    throw new Exception("Unknown communication type: " + SendVia.ToString());
            }

            CommunicationVia = new CommunicationSettings(_xmlConfig != null ? _xmlConfig.GetXmlFilename() : null, _sendVia, _fileSystemDirectory);
        }

        public ISendCommand Send(string command)
        {
            switch(_sendVia)
            {
                case CommunicationType.NamedPipe:
                    return new SendCommandViaNamedPipe(command);

                case CommunicationType.FileSystem:
                    return new SendCommandViaFileSystem(_fileSystemDirectory, command);
            }

            throw new Exception("Unknown communication type: " + _sendVia.ToString());
        }
    }
}
