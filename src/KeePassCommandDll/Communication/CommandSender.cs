using System;
using System.IO;

namespace KeePassCommandDll.Communication
{
    public class CommandSender
    {
        public enum CommunicationType { DetermineAutomatically, NamedPipe, FileSystem }
        private CommunicationType _sendVia;
        private string _fileSystemDirectory;

        public CommandSender(CommunicationType SendVia = CommunicationType.DetermineAutomatically, string FileSystemDirectory = null)
        {
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
                        XmlConfiguration config = XmlConfiguration.Load();

                        if (!string.IsNullOrWhiteSpace(config.filesystem) && Directory.Exists(config.filesystem))
                        {
                            _sendVia = CommunicationType.FileSystem;
                            _fileSystemDirectory = config.filesystem;
                        }
                    }
                    catch { }
                    break;

                default:
                    throw new Exception("Unknown communication type: " + SendVia.ToString());
            }
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
