using KeePassLib;
using System.Collections.Generic;
using System.IO;

namespace KeePassCommander.ViaFileSystemServer
{
    public class DirectoryWatcher
    {
        private FileSystemWatcher _watcher;
        private HandleFileCallback _handler;
        private Dictionary<string, bool> _allowedTitles = new Dictionary<string, bool>();

        public delegate void HandleFileCallback(string filename, Dictionary<string, bool> allowedTitles);

        public DirectoryWatcher(string directory, string filter, HandleFileCallback handler)
        {
            _handler = handler;

            // 
            _watcher = new FileSystemWatcher();
            _watcher.Path = directory;
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            _watcher.Filter = filter;
            _watcher.Created += new FileSystemEventHandler(Watcher_OnCreated);
            _watcher.Renamed += new RenamedEventHandler(Watcher_OnRenamed);
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~DirectoryWatcher()
        {
            Dispose(false);
        }

        private bool _isDisposed = false;
        public void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (disposing)
            {
                _watcher.Dispose();
            }

            _handler = null;
        }

        public void AddAllowedTitles(Dictionary<string, List<PwEntry>> found)
        {
            foreach (var keypair in found)
            {
                var title = keypair.Key;

                if (!_allowedTitles.ContainsKey(title))
                {
                    _allowedTitles.Add(title, true);
                }
            }
        }

        private void Watcher_OnCreated(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;

            try
            {
                _handler(e.FullPath, _allowedTitles);
            }
            catch { }
        }

        private void Watcher_OnRenamed(object source, RenamedEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;

            try
            {
                _handler(e.FullPath, _allowedTitles);
            }
            catch { }
        }

    }
}
