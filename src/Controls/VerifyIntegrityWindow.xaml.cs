using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using MDPlus.Core;

namespace MDPlus.Controls
{
    public partial class VerifyIntegrityWindow : Window
    {
        private System.Threading.CancellationTokenSource? _hashCts;

        public VerifyIntegrityWindow()
        {
            InitializeComponent();
            ApplyTheme();
        }

        public VerifyIntegrityWindow(string initialFilePath) : this()
        {
            if (File.Exists(initialFilePath))
            {
                FilePathTextBox.Text = initialFilePath;
            }
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.Instance.IsDark;
            RootGrid.Background = isDark ? new SolidColorBrush(Color.FromRgb(30, 30, 30)) : new SolidColorBrush(Color.FromRgb(250, 250, 250));
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select File to Verify",
                Filter = "All Files (*.*)|*.*|Executables (*.exe)|*.exe|Archives (*.zip)|*.zip"
            };

            if (dlg.ShowDialog(this) == true)
            {
                FilePathTextBox.Text = dlg.FileName;
            }
        }

        private void CurrentApp_Click(object sender, RoutedEventArgs e)
        {
            string? currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(currentExe) && File.Exists(currentExe))
            {
                FilePathTextBox.Text = currentExe;
            }
        }

        private async void FilePathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _hashCts?.Cancel();
            _hashCts?.Dispose();
            _hashCts = new System.Threading.CancellationTokenSource();
            var token = _hashCts.Token;

            string path = FilePathTextBox.Text.Trim('"', ' ');
            if (File.Exists(path))
            {
                ComputedHashTextBox.Text = "Calculating SHA-256...";
                SetStatus(
                    "⏳ CALCULATING...",
                    "Computing cryptographic SHA-256 hash...",
                    Color.FromRgb(88, 166, 255),
                    Color.FromArgb(30, 56, 139, 253),
                    Color.FromRgb(88, 166, 255));

                try
                {
                    string hash = await HashService.ComputeSha256Async(path, token);
                    if (!token.IsCancellationRequested)
                    {
                        ComputedHashTextBox.Text = hash;
                        PerformComparison();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancelled operations
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        ComputedHashTextBox.Text = $"Error: {ex.Message}";
                        PerformComparison();
                    }
                }
            }
            else
            {
                ComputedHashTextBox.Text = string.IsNullOrEmpty(path) ? "Select a file to compute hash..." : "File not found.";
                PerformComparison();
            }
        }

        private void ExpectedHashTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PerformComparison();
        }

        private void PerformComparison()
        {
            string computed = ComputedHashTextBox.Text.Trim();
            string expected = ExpectedHashTextBox.Text.Trim();

            if (computed.Length != 64 || computed.StartsWith("Error") || computed.StartsWith("Select") || computed.StartsWith("File"))
            {
                SetStatus(
                    "⚪ AWAITING FILE",
                    "Choose an existing file to compute its cryptographic SHA-256 hash.",
                    Color.FromRgb(139, 148, 158),
                    Color.FromRgb(37, 37, 38),
                    Color.FromRgb(60, 60, 60));
                return;
            }

            if (string.IsNullOrWhiteSpace(expected))
            {
                SetStatus(
                    "🔵 HASH COMPUTED",
                    "SHA-256 hash successfully calculated. Enter or paste the expected official hash above to verify authenticity.",
                    Color.FromRgb(88, 166, 255),
                    Color.FromArgb(30, 56, 139, 253),
                    Color.FromRgb(88, 166, 255));
                return;
            }

            string normComputed = HashService.NormalizeHash(computed);
            string normExpected = HashService.NormalizeHash(expected);

            if (string.Equals(normComputed, normExpected, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus(
                    "✅ VERIFIED AUTHENTIC & INTACT",
                    "The cryptographic SHA-256 hash matches the expected value exactly.\nThe file is genuine, complete, and has not been altered, corrupted, or tampered with.",
                    Color.FromRgb(63, 185, 80),
                    Color.FromArgb(40, 46, 160, 67),
                    Color.FromRgb(63, 185, 80));
            }
            else
            {
                SetStatus(
                    "❌ HASH MISMATCH / POSSIBLE CORRUPTION",
                    "WARNING: The computed SHA-256 hash DOES NOT match the expected value!\nThe file may be corrupted, incomplete, modified, or from an untrusted source.",
                    Color.FromRgb(248, 81, 73),
                    Color.FromArgb(40, 248, 81, 73),
                    Color.FromRgb(248, 81, 73));
            }
        }

        private void SetStatus(string title, string details, Color textColor, Color bgColor, Color borderColor)
        {
            StatusTitleText.Text = title;
            StatusTitleText.Foreground = new SolidColorBrush(textColor);
            StatusDetailText.Text = details;
            StatusBanner.Background = new SolidColorBrush(bgColor);
            StatusBanner.BorderBrush = new SolidColorBrush(borderColor);
        }

        private void CopyHash_Click(object sender, RoutedEventArgs e)
        {
            string hash = ComputedHashTextBox.Text.Trim();
            if (hash.Length == 64)
            {
                ClipboardHelper.SetText(hash);
                CopyHashButton.Content = "Copied!";
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += (ts, te) =>
                {
                    CopyHashButton.Content = "Copy Hash";
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void PasteExpected_Click(object sender, RoutedEventArgs e)
        {
            string? text = ClipboardHelper.GetText();
            if (!string.IsNullOrWhiteSpace(text))
            {
                ExpectedHashTextBox.Text = text.Trim();
            }
        }

        private void LoadChecksumFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Load SHA-256 Checksum File",
                Filter = "Checksum Files (*.sha256;*.txt)|*.sha256;*.txt|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) == true)
            {
                LoadChecksumFile(dlg.FileName);
            }
        }

        private void LoadChecksumFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                var parsed = HashService.ParseChecksums(content);

                string targetFile = Path.GetFileName(FilePathTextBox.Text.Trim('"', ' '));

                // 1. Direct match or filename match
                if (!string.IsNullOrEmpty(targetFile))
                {
                    foreach (var kvp in parsed)
                    {
                        if (kvp.Key.Equals(targetFile, StringComparison.OrdinalIgnoreCase) ||
                            Path.GetFileName(kvp.Key).Equals(targetFile, StringComparison.OrdinalIgnoreCase))
                        {
                            ExpectedHashTextBox.Text = kvp.Value;
                            return;
                        }
                    }
                }

                // 2. If single entry in file
                if (parsed.Count == 1)
                {
                    foreach (var val in parsed.Values)
                    {
                        ExpectedHashTextBox.Text = val;
                        return;
                    }
                }

                // 3. Check for bare hash in parsed
                if (parsed.TryGetValue(string.Empty, out string? bareHash) && bareHash.Length == 64)
                {
                    ExpectedHashTextBox.Text = bareHash;
                    return;
                }

                string raw = content.Trim();
                if (raw.Length == 64)
                {
                    ExpectedHashTextBox.Text = raw;
                }
                else
                {
                    MessageBox.Show($"Loaded checksum file contains {parsed.Count} entries, but none matched '{targetFile}'.",
                        "Checksum File", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read checksum file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                {
                    string? targetFile = null;
                    string? checksumFile = null;

                    foreach (var f in files)
                    {
                        if (!File.Exists(f)) continue;
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        if (ext == ".sha256" || (ext == ".txt" && f.Contains("checksum", StringComparison.OrdinalIgnoreCase)))
                        {
                            checksumFile = f;
                        }
                        else
                        {
                            targetFile = f;
                        }
                    }

                    if (targetFile != null)
                    {
                        FilePathTextBox.Text = targetFile;
                    }
                    else if (files.Length == 1 && File.Exists(files[0]))
                    {
                        FilePathTextBox.Text = files[0];
                    }

                    if (checksumFile != null)
                    {
                        LoadChecksumFile(checksumFile);
                    }
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
