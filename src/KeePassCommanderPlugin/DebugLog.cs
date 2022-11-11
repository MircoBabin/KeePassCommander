using System;
using System.IO;
using System.Threading;

namespace KeePassCommander
{
    public class DebugLog
    {
        private TextWriter Logger = null;

        public bool Enabled { get { return Logger != null; } }

        public void Initialize(string debugFilename)
        {
            if (Logger != null) return;
            if (String.IsNullOrEmpty(debugFilename)) return;

            Logger = new StreamWriter(debugFilename);
            ((StreamWriter)Logger).AutoFlush = true;

            OutputLine("Debug initialized");
        }

        public void OutputLine(string message)
        {
            if (Logger == null) return;

            lock (Logger)
            {
                Logger.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "][" + Thread.CurrentThread.ManagedThreadId + "] " + message);
            }
        }

    }
}
