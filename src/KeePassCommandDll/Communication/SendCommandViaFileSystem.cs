using System;
using System.IO;
using System.Security.Cryptography;

namespace KeePassCommandDll.Communication
{
    public class SendCommandViaFileSystem : ISendCommand
    {
        private const string helloRequestSuffix = ".KeePassCommander.FileSystem.hello-request";
        private const string helloResponseSuffix = ".KeePassCommander.FileSystem.hello-response";
        private const string requestSuffix = ".KeePassCommander.FileSystem.request";
        private const string responseSuffix = ".KeePassCommander.FileSystem.response";

        private string FileSystemDirectory;
        private string Command;

        public Response Response { get; private set; }

        public SendCommandViaFileSystem(string FileSystemDirectory, string command)
        {
            Response = new Response();
            this.FileSystemDirectory = FileSystemDirectory;
            Command = command;

            Execute();
        }

        private void WriteRequest(string filename, byte[] data)
        {
            using (var stream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] bytes = BitConverter.GetBytes((UInt32)data.Length);
                if (bytes.Length != 4) throw new Exception("BitConverter.GetBytes(UInt32) must return 4 bytes.");
                stream.Write(bytes, 0, 4);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }


        private byte[] ReadResponse(string filename, int timeoutSeconds)
        {
            try
            {
                var StartTime = DateTime.Now;
                while (true)
                {
                    try
                    {
                        using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            byte[] bytes;

                            if (stream.Length < 4)
                                throw new Exception("File is not complete. Expected 4 bytes, length = " + stream.Length);

                            bytes = new byte[4];
                            stream.Read(bytes, 0, 4);
                            UInt32 length = BitConverter.ToUInt32(bytes, 0);

                            if (stream.Length != (4 + length))
                                throw new Exception("File is not complete. Expected (4 + " + length + ") " + (4 + length) + ", length = " + stream.Length);

                            bytes = new byte[length];
                            stream.Read(bytes, 0, (int)length);
                            return bytes;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (DateTime.Now.Subtract(StartTime).Seconds > timeoutSeconds)
                            throw new Exception("Waiting for completion failed on " + filename + " failed. " + ex.Message, ex);
                    }
                }
            }
            finally
            {
                KeePassCommander.Encryption.SecureDelete(filename);
            }
        }


        private void Execute()
        {
            string filename;
            string helloRequestFilename;
            while (true)
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] random = new byte[64];
                rng.GetBytes(random);
                filename = BitConverter.ToString(random).Replace("-", string.Empty);

                helloRequestFilename = Path.Combine(FileSystemDirectory, filename + helloRequestSuffix);
                if (!File.Exists(helloRequestFilename)) break;
            }
            string helloResponseFilename = Path.Combine(FileSystemDirectory, filename + helloResponseSuffix);
            string requestFilename = Path.Combine(FileSystemDirectory, filename + requestSuffix);
            string responseFilename = Path.Combine(FileSystemDirectory, filename + responseSuffix);

            try
            {
                KeePassCommander.Encryption encryption = new KeePassCommander.Encryption();
                {
                    // Hello - settle a shared key for encryption
                    WriteRequest(helloRequestFilename, encryption.PublicKeyForSettlement);

                    encryption.SettleSharedKey(ReadResponse(helloResponseFilename, 10 /* seconds */));
                }

                {
                    // Request - encrypted
                    WriteRequest(requestFilename, encryption.Encrypt(Command));

                    string response = encryption.Decrypt(ReadResponse(responseFilename, 300 /* 5 minutes */));
                    Response.ReadFromStream(response);
                }
            }
            finally
            {
                KeePassCommander.Encryption.SecureDelete(helloRequestFilename);
                KeePassCommander.Encryption.SecureDelete(helloResponseFilename);
                KeePassCommander.Encryption.SecureDelete(requestFilename);
                KeePassCommander.Encryption.SecureDelete(responseFilename);
            }
        }
    }
}
