using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MDPlus.Models;

using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MDPlus
{
    public partial class App : Application
    {
        public static string[] StartupArgs { get; internal set; } = Array.Empty<string>();
        private const string IpcPipeName = "MDPlus_SingleInstance_Pipe";
        private const string SingleInstanceMutexName = "Local\\MDPlus_SingleInstance_Mutex";
        private static Mutex? _singleInstanceMutex;
        private static CancellationTokenSource? _ipcCancelSource;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            StartupArgs = e.Args ?? Array.Empty<string>();

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                        "MDPlus Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            // Check single instance & multi-tab preference
            var settings = AppSettings.Load();
            bool forceNewWindow = StartupArgs.Any(a => a.Equals("--new-window", StringComparison.OrdinalIgnoreCase));

            if (settings.OpenFilesInNewTab && !forceNewWindow)
            {
                bool isPrimaryInstance = false;
                try
                {
                    _singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out isPrimaryInstance);
                }
                catch
                {
                    isPrimaryInstance = false;
                }

                if (!isPrimaryInstance)
                {
                    // Existing instance running - transmit arguments to it and exit cleanly
                    if (SendArgsToExistingInstance(StartupArgs))
                    {
                        Shutdown(0);
                        return;
                    }
                }
                else
                {
                    StartIpcServer();
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _ipcCancelSource?.Cancel();
                if (_singleInstanceMutex != null)
                {
                    _singleInstanceMutex.ReleaseMutex();
                    _singleInstanceMutex.Dispose();
                    _singleInstanceMutex = null;
                }
            }
            catch { }

            base.OnExit(e);
        }

        private static bool SendArgsToExistingInstance(string[]? args)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", IpcPipeName, PipeDirection.Out);
                client.Connect(1500);
                using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                string payload = JsonSerializer.Serialize(args ?? Array.Empty<string>());
                writer.WriteLine(payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void StartIpcServer()
        {
            _ipcCancelSource = new CancellationTokenSource();
            var token = _ipcCancelSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(
                            IpcPipeName,
                            PipeDirection.In,
                            NamedPipeServerStream.MaxAllowedServerInstances,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous);

                        await server.WaitForConnectionAsync(token).ConfigureAwait(false);

                        using var reader = new StreamReader(server, Encoding.UTF8);
                        string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[]? incomingArgs = null;
                            try
                            {
                                incomingArgs = JsonSerializer.Deserialize<string[]>(line);
                            }
                            catch
                            {
                                incomingArgs = new[] { line };
                            }

                            Application.Current?.Dispatcher?.InvokeAsync(() =>
                            {
                                HandleIncomingInstanceArgs(incomingArgs);
                            });
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        try { await Task.Delay(300, token).ConfigureAwait(false); } catch { break; }
                    }
                }
            }, token);
        }

        public static void HandleIncomingInstanceArgs(string[]? incomingArgs)
        {
            if (Application.Current?.MainWindow is MainWindow mw)
            {
                var settings = AppSettings.Load();
                var filesToOpen = ResolveStartupFiles(settings, incomingArgs);

                foreach (var file in filesToOpen)
                {
                    mw.OpenDocument(file, activate: true, saveSession: true);
                }

                mw.RestoreAndActivateWindow();
            }
        }

        /// <summary>
        /// Resolves the list of files to open on application startup based on CLI arguments,
        /// user session preferences (resume session vs start fresh), and session history.
        /// </summary>
        public static List<string> ResolveStartupFiles(AppSettings settings, string[]? args = null)
        {
            var filesToOpen = new List<string>();
            args ??= StartupArgs;

            // 1. Explicit CLI file arguments take highest precedence
            if (args != null && args.Length > 0)
            {
                bool hasExplicitFileArg = false;
                foreach (string arg in args)
                {
                    if (string.IsNullOrWhiteSpace(arg) || arg.StartsWith("-")) continue;
                    hasExplicitFileArg = true;
                    if (File.Exists(arg))
                    {
                        string full = Path.GetFullPath(arg);
                        if (!filesToOpen.Contains(full, StringComparer.OrdinalIgnoreCase))
                        {
                            filesToOpen.Add(full);
                        }
                    }
                }

                if (hasExplicitFileArg)
                {
                    return filesToOpen;
                }
            }

            // 2. Check for explicit CLI override flags
            bool forceStartFresh = args != null && args.Any(a => a.Equals("--fresh", StringComparison.OrdinalIgnoreCase) ||
                                                                  a.Equals("--start-fresh", StringComparison.OrdinalIgnoreCase));
            if (forceStartFresh)
            {
                return filesToOpen;
            }

            // 3. User session preference: Resume session if enabled (default: true)
            if (settings.ResumeSession)
            {
                if (settings.OpenFiles != null && settings.OpenFiles.Count > 0)
                {
                    foreach (string file in settings.OpenFiles)
                    {
                        if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                        {
                            string full = Path.GetFullPath(file);
                            if (!filesToOpen.Contains(full, StringComparer.OrdinalIgnoreCase))
                            {
                                filesToOpen.Add(full);
                            }
                        }
                    }
                }
                else if (!settings.HasSavedSession && settings.RecentFiles != null && settings.RecentFiles.Count > 0)
                {
                    // Transition fallback for upgraded installations before first session save
                    foreach (string recent in settings.RecentFiles)
                    {
                        if (!string.IsNullOrWhiteSpace(recent) && File.Exists(recent))
                        {
                            string full = Path.GetFullPath(recent);
                            filesToOpen.Add(full);
                            break;
                        }
                    }
                }
            }

            return filesToOpen;
        }
    }
}
