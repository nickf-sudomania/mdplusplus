using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MDPlus.Models;

namespace MDPlus
{
    public partial class App : Application
    {
        public static string[] StartupArgs { get; internal set; } = Array.Empty<string>();

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
