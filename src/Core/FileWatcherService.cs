using System;
using System.Collections.Concurrent;
using System.IO;
using System.Timers;

namespace MDPlus.Core
{
    public class FileWatcherService : IDisposable
    {
        public static FileWatcherService? Instance { get; private set; }

        private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, DateTime> _suppressedFiles = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler<string>? FileChanged;

        public FileWatcherService()
        {
            Instance = this;
        }

        public void SuppressNextChange(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            string fullPath = Path.GetFullPath(filePath);
            _suppressedFiles[fullPath] = DateTime.UtcNow.AddMilliseconds(2000);
        }

        public void IgnoreNextChange(string filePath) => SuppressNextChange(filePath);

        public void WatchFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            string fullPath = Path.GetFullPath(filePath);
            if (_watchers.ContainsKey(fullPath)) return;

            try
            {
                string? dir = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileName(fullPath);

                if (string.IsNullOrEmpty(dir)) return;

                var watcher = new FileSystemWatcher(dir, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };

                watcher.Changed += (s, e) => OnFileEvent(e.FullPath);
                watcher.Created += (s, e) => OnFileEvent(e.FullPath);
                watcher.Renamed += (s, e) => OnFileEvent(e.FullPath);

                _watchers.TryAdd(fullPath, watcher);
            }
            catch
            {
                // FileSystemWatcher can fail on some network shares or permissions, ignore gracefully
            }
        }

        public void UnwatchFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            string fullPath = Path.GetFullPath(filePath);

            if (_watchers.TryRemove(fullPath, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _lastEventTimes.TryRemove(fullPath, out _);
        }

        private void OnFileEvent(string fullPath)
        {
            if (_suppressedFiles.TryGetValue(fullPath, out var suppressUntil))
            {
                if (DateTime.UtcNow < suppressUntil)
                {
                    return;
                }
                _suppressedFiles.TryRemove(fullPath, out _);
            }

            DateTime now = DateTime.UtcNow;

            // Debounce within 300ms
            if (_lastEventTimes.TryGetValue(fullPath, out var lastTime))
            {
                if ((now - lastTime).TotalMilliseconds < 300)
                {
                    return;
                }
            }
            _lastEventTimes[fullPath] = now;

            FileChanged?.Invoke(this, fullPath);
        }

        public void Dispose()
        {
            foreach (var kvp in _watchers)
            {
                kvp.Value.EnableRaisingEvents = false;
                kvp.Value.Dispose();
            }
            _watchers.Clear();
            _lastEventTimes.Clear();
            _suppressedFiles.Clear();
            if (Instance == this) Instance = null;
        }
    }
}
