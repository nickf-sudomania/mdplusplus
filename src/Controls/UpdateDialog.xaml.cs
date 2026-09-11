using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MDPlus.Core;

namespace MDPlus.Controls
{
    public partial class UpdateDialog : Window
    {
        private readonly UpdateCheckResult _updateInfo;
        private readonly UpdateService _updateService;
        private CancellationTokenSource? _downloadCts;

        public string? VerifiedInstallerPath { get; private set; }
        public FlowDocument? RenderedNotesDocument => HighlightsViewer.Document;

        public UpdateDialog(UpdateCheckResult updateInfo, UpdateService? updateService = null)
        {
            InitializeComponent();
            _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));
            _updateService = updateService ?? new UpdateService();

            SourceInitialized += (s, e) =>
            {
                DwmHelper.ApplyTitleBarTheme(this, ThemeManager.Instance.CurrentPalette);
            };

            ThemeManager.Instance.ThemeChanged += OnThemeChanged;

            PopulateDialog();
            ApplyTheme();
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(ApplyTheme);
        }

        private void PopulateDialog()
        {
            string latest = !string.IsNullOrEmpty(_updateInfo.LatestVersion) ? _updateInfo.LatestVersion : "New version";
            string current = !string.IsNullOrEmpty(_updateInfo.CurrentVersion) ? _updateInfo.CurrentVersion : UpdateService.GetCurrentVersion();

            TitleTextBlock.Text = $"MDPlus {latest} is Available!";
            VersionSubtext.Text = $"Release tag: {latest} (currently running: v{current.TrimStart('v', 'V')}).";

            RenderReleaseNotes();

            if (string.IsNullOrEmpty(_updateInfo.SetupDownloadUrl))
            {
                UpdateNowButton.IsEnabled = false;
                UpdateNowButton.ToolTip = "Installer binary is not available for this release.";
            }
        }

        private void RenderReleaseNotes()
        {
            string rawNotes = FormatReleaseNotesForDisplay(_updateInfo.ReleaseHighlights, _updateInfo.LatestVersion);
            if (string.IsNullOrWhiteSpace(rawNotes))
            {
                rawNotes = "*No release notes provided for this version.*";
            }

            try
            {
                var parser = new MarkdownParser();
                var doc = parser.Parse(rawNotes);
                var palette = ThemeManager.Instance.CurrentPalette;
                var converter = new MarkdownToWpfConverter(
                    AppDomain.CurrentDomain.BaseDirectory,
                    palette,
                    enableLatex: true,
                    enableHtml: true);

                var flowDoc = converter.Convert(doc);
                flowDoc.PagePadding = new Thickness(16, 12, 16, 16);
                flowDoc.FontSize = 13;
                flowDoc.LineHeight = 22;
                flowDoc.Background = Brushes.Transparent;
                flowDoc.Foreground = palette.EditorFg;

                HighlightsViewer.Document = flowDoc;
            }
            catch
            {
                var palette = ThemeManager.Instance.CurrentPalette;
                var flowDoc = new FlowDocument(new Paragraph(new Run(rawNotes)))
                {
                    PagePadding = new Thickness(16, 12, 16, 16),
                    FontSize = 13,
                    Foreground = palette.EditorFg,
                    Background = Brushes.Transparent
                };
                HighlightsViewer.Document = flowDoc;
            }
        }

        /// <summary>
        /// Sanitizes and formats the raw release notes text to ensure that feature descriptions
        /// and what changed are prominently displayed, rather than only displaying generic installer
        /// text or raw cryptographic SHA-256 hashes.
        /// </summary>
        public static string FormatReleaseNotesForDisplay(string? rawNotes, string? version = null)
        {
            if (string.IsNullOrWhiteSpace(rawNotes) || UpdateService.IsGenericShaBoilerplate(rawNotes))
            {
                return GetDefaultReleaseNotesForVersion(version);
            }

            string text = rawNotes.Trim();

            // If the notes contain the redundant generic installer preamble followed by actual release notes,
            // clean out the generic installer boilerplate so what changed is shown right away
            if (text.StartsWith("## MDPlus Release") && text.Contains("### 📦 Official Windows Installer"))
            {
                int featuresIdx = text.IndexOf("## 🚀 Key Feature Highlights", StringComparison.OrdinalIgnoreCase);
                if (featuresIdx < 0) featuresIdx = text.IndexOf("### Highlights", StringComparison.OrdinalIgnoreCase);
                if (featuresIdx < 0) featuresIdx = text.IndexOf("## What's New", StringComparison.OrdinalIgnoreCase);
                if (featuresIdx < 0) featuresIdx = text.IndexOf("### What's New", StringComparison.OrdinalIgnoreCase);

                if (featuresIdx > 0)
                {
                    string cleanVer = (version ?? "New Version").TrimStart('v', 'V');
                    return $"# MDPlus v{cleanVer} Release Notes\n\n" + text.Substring(featuresIdx);
                }
            }

            return text;
        }

        public static string GetDefaultReleaseNotesForVersion(string? version)
        {
            string cleanVer = (version ?? "1.09").Trim().TrimStart('v', 'V');

            // 1. Try loading from local docs directory if available
            string[] candidatePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", $"RELEASE_NOTES_v{cleanVer}.md"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RELEASE_NOTES.md"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "docs", $"RELEASE_NOTES_v{cleanVer}.md"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "docs", $"RELEASE_NOTES_v{cleanVer}.md"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "docs", $"RELEASE_NOTES_v{cleanVer}.md"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "RELEASE_NOTES.md")
            };

            foreach (var p in candidatePaths)
            {
                try
                {
                    if (System.IO.File.Exists(p))
                    {
                        string content = System.IO.File.ReadAllText(p);
                        if (!string.IsNullOrWhiteSpace(content))
                            return content;
                    }
                }
                catch { }
            }

            // 2. Built-in curated release notes for known releases
            if (cleanVer.StartsWith("1.09"))
            {
                return @"# MDPlus v1.09 Release Notes

MDPlus v1.09 expands the hyper-fast native Windows reader into a universal text and structured data viewer, adding native support for **Plain Text, CSV/TSV Tabular Data, and JSON Documents** with clean, highly readable formatted layouts while preserving sub-150ms startup speed, low memory footprint (< 35 MB), and zero external runtime dependencies.

---

## 🚀 Key Feature Highlights

### 1. Multi-Format Text Document Loading & File Type Detection
- **Universal Text Support:** Open and edit `.txt`, `.log`, `.csv`, `.tsv`, `.json`, `.ini`, `.cfg`, `.yaml`, `.yml`, and `.xml` alongside Markdown with dynamic format detection and tab strip badges.
- **Multi-Format Dialog Filters:** File > Open defaults to 'All Supported Files' across all 10 formats.
- **Format Badges & Context Stats:** Clean format badges in tab headers with context-specific stats (rows/columns for CSV/TSV, keys for JSON).

### 2. High-Legibility Formatted CSV & TSV Table Layouts
- **RFC 4180 Parsing:** Single-pass tokenizer accurately parses comma- and tab-delimited files, handling quoted fields, escaped quotes (`""""`), and line breaks.
- **Styled FlowDocument Tables:** Displays tabular data with distinct semi-bold headers, 1px grid borders, subtle alternating row stripes, and numeric right-alignment.
- **Rendered & Raw Toggle:** Instant toggling between Rendered Table (`Ctrl+1`) and Raw Monospace (`Ctrl+3`) with lossless round-trip saving.

### 3. Structured JSON Pretty-Printing & Syntax Highlighting
- **Zero-Baggage Formatting:** Fast 2-space pretty printing powered directly by built-in `System.Text.Json`.
- **Theme-Aware Syntax Coloring:** Token highlights (keys, strings, numbers, booleans, null) dynamically adapt to all 8 light and dark themes.
- **Resilient Error Fallback:** Malformed JSON gracefully displays raw text with an inline syntax notification banner without crashing.

### 4. Plain Text & Log Typography
- **Optimized Font Stacks:** Cascadia Code / Consolas monospace for log and config files; Segoe UI for plain text documents.
- **Reader Controls:** Smooth font zoom scaling (`Ctrl + Plus/Minus/0`), line wrapping, and in-page find navigation (`Ctrl+F`).

### 5. Windows Shell & Installer Integration
- **Optional Association Tasks:** Installer includes configurable tasks for `.txt`, `.csv`, `.tsv`, and `.json` file associations.
- **Default Apps & Context Menus:** Windows Default Apps registration and right-click 'Open with MDPlus' shell menu verbs.

### 6. Performance & Memory Guardrails
- **Instant Cold Startup:** Launches in under 150 ms with zero background bloat.
- **Low Memory Footprint:** Consumes under 35 MB idle RAM (managed heap ~8–12 MB).
- **Sub-50ms 5,000-Line Parsing:** 5,000-line CSV and JSON documents parse in under 50 ms.";
            }

            if (cleanVer.StartsWith("1.08"))
            {
                return @"# MDPlus v1.08 Release Notes

### 🚀 Key Feature Highlights
- **Hardened In-Reader Hyperlink Navigation:** Explicit mouse capture and cached hyperlink routing on `PreviewMouseLeftButtonDown`, preventing WPF's text editor drag-selection engine from intercepting link clicks.
- **Bounded Hit-Testing & Empty Margin Protection:** Precise character rect bounding ensures clicking empty line margins never triggers hyperlinks.
- **Asynchronous Document Loading:** Safe tab replacement routing so mouse events complete cleanly before the visual tree changes.
- **Missing File Feedback:** Clear status bar reporting when referenced markdown files cannot be found.";
            }

            return $"# MDPlus v{cleanVer} Update\n\nMDPlus v{cleanVer} is available with performance improvements, bug fixes, and enhanced reader capabilities.";
        }

        private void ApplyTheme()
        {
            var palette = ThemeManager.Instance.CurrentPalette;

            RootGrid.Background = palette.EditorBg;
            HeaderBorder.Background = palette.SidebarBg;
            HeaderBorder.BorderBrush = palette.Border;

            HighlightsBorder.Background = palette.EditorBg;
            HighlightsBorder.BorderBrush = palette.Border;
            HighlightsLabel.Foreground = palette.MutedFg;

            RenderReleaseNotes();

            FooterBorder.Background = palette.SidebarBg;
            FooterBorder.BorderBrush = palette.Border;

            TitleTextBlock.Foreground = palette.HeadingFg;
            VersionSubtext.Foreground = palette.MutedFg;
            StatusTextBlock.Foreground = palette.MutedFg;

            LaterButton.Background = palette.MenuHoverBg;
            LaterButton.Foreground = palette.MenuFg;
            LaterButton.BorderBrush = palette.Border;

            DownloadProgressBar.Foreground = palette.Accent;
            DownloadProgressBar.Background = palette.Border;

            DwmHelper.ApplyTitleBarTheme(this, palette);
        }

        private void Later_Click(object sender, RoutedEventArgs e)
        {
            if (_downloadCts != null && !_downloadCts.IsCancellationRequested && UpdateNowButton.IsEnabled == false)
            {
                // Active download in progress: cancel the download
                _downloadCts.Cancel();
                StatusTextBlock.Text = "Canceling download...";
                LaterButton.IsEnabled = false;
                return;
            }

            _downloadCts?.Cancel();
            try
            {
                DialogResult = false;
            }
            catch (InvalidOperationException)
            {
                Close();
            }
        }

        private async void UpdateNow_Click(object sender, RoutedEventArgs e)
        {
            UpdateNowButton.IsEnabled = false;
            LaterButton.Content = "Cancel";
            LaterButton.IsEnabled = true;
            ProgressPanel.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StatusTextBlock.Text = "Downloading installer...";

            _downloadCts = new CancellationTokenSource();
            var progress = new Progress<double>(p =>
            {
                DownloadProgressBar.Value = p * 100;
                StatusTextBlock.Text = $"Downloading update: {(int)(p * 100)}%...";
            });

            try
            {
                var result = await _updateService.DownloadAndVerifyUpdateAsync(_updateInfo, progress, _downloadCts.Token);

                if (!IsLoaded) return;

                if (result.Success && !string.IsNullOrEmpty(result.InstallerPath))
                {
                    StatusTextBlock.Text = "Cryptographic SHA-256 verification passed! Launching installer...";
                    DownloadProgressBar.Value = 100;
                    LaterButton.IsEnabled = false;

                    await Task.Delay(300);

                    if (!IsLoaded) return;

                    VerifiedInstallerPath = result.InstallerPath;

                    if (Owner is MainWindow mainWindow && mainWindow.IsLoaded)
                    {
                        try
                        {
                            DialogResult = true;
                        }
                        catch (InvalidOperationException)
                        {
                            Close();
                            mainWindow.CloseAndLaunchInstaller(result.InstallerPath);
                        }
                    }
                    else if (Application.Current?.MainWindow is MainWindow appMain && appMain.IsLoaded)
                    {
                        Close();
                        appMain.CloseAndLaunchInstaller(result.InstallerPath);
                    }
                    else
                    {
                        Close();
                        UpdateService.LaunchInstallerAndExit(result.InstallerPath);
                    }
                }
                else
                {
                    StatusTextBlock.Text = "Update failed: " + result.ErrorMessage;
                    MessageBox.Show(this, result.ErrorMessage ?? "Installer verification failed.", "Update Verification Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateNowButton.IsEnabled = true;
                    LaterButton.Content = "Later";
                    LaterButton.IsEnabled = true;
                }
            }
            catch (OperationCanceledException)
            {
                if (!IsLoaded) return;
                StatusTextBlock.Text = "Download canceled.";
                ProgressPanel.Visibility = Visibility.Collapsed;
                UpdateNowButton.IsEnabled = true;
                LaterButton.Content = "Later";
                LaterButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                if (!IsLoaded) return;
                StatusTextBlock.Text = "Error: " + ex.Message;
                MessageBox.Show(this, $"An error occurred during update: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateNowButton.IsEnabled = true;
                LaterButton.Content = "Later";
                LaterButton.IsEnabled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            _downloadCts?.Cancel();
            base.OnClosed(e);
        }
    }
}
