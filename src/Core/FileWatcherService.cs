using System;
using System.Collections.Concurrent;
using System.IO;
using System.Timers;

namespace MDPlus.Core
{
    public class FileWatcherService : IDisposable
    {
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, DateTime> _lastEventTimes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler<string>? FileChanged;

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
        }
    }
}
