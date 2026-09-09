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

        private void FilePathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string path = FilePathTextBox.Text.Trim('"', ' ');
            if (File.Exists(path))
            {
                try
                {
                    string hash = HashService.ComputeSha256(path);
                    ComputedHashTextBox.Text = hash;
                }
                catch (Exception ex)
                {
                    ComputedHashTextBox.Text = $"Error: {ex.Message}";
                }
            }
            else
            {
                ComputedHashTextBox.Text = string.IsNullOrEmpty(path) ? "Select a file to compute hash..." : "File not found.";
            }

            PerformComparison();
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
            try
            {
                string text = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ExpectedHashTextBox.Text = text.Trim();
                }
            }
            catch { }
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
                try
                {
                    string content = File.ReadAllText(dlg.FileName);
                    var parsed = HashService.ParseChecksums(content);

                    string targetFile = Path.GetFileName(FilePathTextBox.Text.Trim('"', ' '));
                    if (!string.IsNullOrEmpty(targetFile) && parsed.TryGetValue(targetFile, out string? matchingHash))
                    {
                        ExpectedHashTextBox.Text = matchingHash;
                        return;
                    }

                    // Fallback: If 1 entry or single hash string in file
                    if (parsed.Count == 1)
                    {
                        foreach (var val in parsed.Values)
                        {
                            ExpectedHashTextBox.Text = val;
                            return;
                        }
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
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && File.Exists(files[0]))
                {
                    FilePathTextBox.Text = files[0];
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
