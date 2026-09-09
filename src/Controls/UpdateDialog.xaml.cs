using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MDPlus.Core;

namespace MDPlus.Controls
{
    public partial class UpdateDialog : Window
    {
        private readonly UpdateCheckResult _updateInfo;
        private readonly UpdateService _updateService;
        private CancellationTokenSource? _downloadCts;

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

            string highlights = _updateInfo.ReleaseHighlights;
            if (string.IsNullOrWhiteSpace(highlights))
            {
                highlights = "No release notes provided for this version.";
            }
            HighlightsTextBox.Text = highlights;

            if (string.IsNullOrEmpty(_updateInfo.ReleaseUrl))
            {
                ReleaseNotesButton.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(_updateInfo.SetupDownloadUrl))
            {
                UpdateNowButton.IsEnabled = false;
                UpdateNowButton.ToolTip = "Installer binary is not available for this release.";
            }
        }

        private void ApplyTheme()
        {
            var palette = ThemeManager.Instance.CurrentPalette;

            RootGrid.Background = palette.EditorBg;
            HeaderBorder.Background = palette.SidebarBg;
            HeaderBorder.BorderBrush = palette.Border;

            HighlightsBorder.Background = palette.CodeBg;
            HighlightsBorder.BorderBrush = palette.Border;
            HighlightsLabel.Foreground = palette.MutedFg;
            HighlightsTextBox.Foreground = palette.EditorFg;

            FooterBorder.Background = palette.SidebarBg;
            FooterBorder.BorderBrush = palette.Border;

            TitleTextBlock.Foreground = palette.HeadingFg;
            VersionSubtext.Foreground = palette.MutedFg;
            StatusTextBlock.Foreground = palette.MutedFg;

            LaterButton.Background = palette.MenuHoverBg;
            LaterButton.Foreground = palette.MenuFg;
            LaterButton.BorderBrush = palette.Border;

            ReleaseNotesButton.Background = palette.MenuHoverBg;
            ReleaseNotesButton.Foreground = palette.MenuFg;
            ReleaseNotesButton.BorderBrush = palette.Border;

            DownloadProgressBar.Foreground = palette.Accent;
            DownloadProgressBar.Background = palette.Border;

            DwmHelper.ApplyTitleBarTheme(this, palette);
        }

        private void ReleaseNotes_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_updateInfo.ReleaseUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(_updateInfo.ReleaseUrl) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    if (IsLoaded)
                    {
                        MessageBox.Show(this, $"Failed to open release URL: {ex.Message}", "Open Browser Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void Later_Click(object sender, RoutedEventArgs e)
        {
            _downloadCts?.Cancel();
            Close();
        }

        private async void UpdateNow_Click(object sender, RoutedEventArgs e)
        {
            UpdateNowButton.IsEnabled = false;
            LaterButton.IsEnabled = false;
            ProgressPanel.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StatusTextBlock.Text = "Downloading installer (MDPlus-Setup.exe)...";

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

                    await Task.Delay(400);
                    UpdateService.LaunchInstallerAndExit(result.InstallerPath);
                }
                else
                {
                    StatusTextBlock.Text = "Update failed: " + result.ErrorMessage;
                    MessageBox.Show(this, result.ErrorMessage ?? "Installer verification failed.", "Update Verification Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateNowButton.IsEnabled = true;
                    LaterButton.IsEnabled = true;
                }
            }
            catch (OperationCanceledException)
            {
                if (!IsLoaded) return;
                StatusTextBlock.Text = "Download canceled.";
                UpdateNowButton.IsEnabled = true;
                LaterButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                if (!IsLoaded) return;
                StatusTextBlock.Text = "Error: " + ex.Message;
                MessageBox.Show(this, $"An error occurred during update: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateNowButton.IsEnabled = true;
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
