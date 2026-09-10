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
            string rawNotes = _updateInfo.ReleaseHighlights;
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
