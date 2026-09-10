using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using MDPlus.Controls;
using MDPlus.Core;
using MDPlus.Models;

namespace MDPlus
{
    public partial class MainWindow : Window
    {
        private readonly List<DocumentTabItem> _tabs = new List<DocumentTabItem>();
        private DocumentTabItem? _activeTab;
        private readonly MarkdownParser _parser = new MarkdownParser();
        private readonly FileWatcherService _fileWatcher = new FileWatcherService();
        private readonly AppSettings _settings;
        private bool _isFullScreen = false;
        private WindowState _previousWindowState = WindowState.Normal;
        private WindowStyle _previousWindowStyle = WindowStyle.SingleBorderWindow;
        private bool _isSyncingScroll = false;
        private bool _suppressDirtyTracking = false;
        private int _untitledIndex = 1;
        private bool _altKeyCandidate = false;
        private DateTime _lastHamburgerClosedTime = DateTime.MinValue;
        private readonly UpdateService _updateService = new UpdateService();

        public MainWindow()
        {
            InitializeComponent();
            Deactivated += (s, e) => _altKeyCandidate = false;
            SourceInitialized += (s, e) =>
            {
                DwmHelper.ApplyTitleBarTheme(this, ThemeManager.Instance.CurrentPalette);
            };

            _settings = AppSettings.Load();
            ThemeManager.Instance.SetPreset(_settings.Theme);
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;

            _fileWatcher.FileChanged += OnExternalFileChanged;

            DocumentFindBar.FindRequested += OnFindRequested;
            DocumentFindBar.Closed += (s, e) => MarkdownViewer.ClearHighlights();

            MarkdownViewer.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(MarkdownViewer_ScrollChanged));
            RawMarkdownTextBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(RawTextBox_ScrollChanged));

            MarkdownViewer.TextChanged += MarkdownViewer_TextChanged;
            RawMarkdownTextBox.TextChanged += RawMarkdownTextBox_TextChanged;
            MarkdownViewer.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(MarkdownViewer_ButtonClick));

            ApplyTheme();
            BuildRecentFilesMenu();
            UpdateStartupModeMenu();

            SetMenuBarVisibility(_settings.ShowMenuBar);
            if (HamburgerContextMenu != null)
            {
                HamburgerContextMenu.Closed += (s, e) => _lastHamburgerClosedTime = DateTime.UtcNow;
            }

            TocMenuItem.IsChecked = _settings.ShowToc;
            if (HamburgerTocMenuItem != null) HamburgerTocMenuItem.IsChecked = _settings.ShowToc;
            SetSidebarVisibility(_settings.ShowToc);

            WordWrapMenuItem.IsChecked = _settings.WordWrap;
            if (HamburgerWordWrapMenuItem != null) HamburgerWordWrapMenuItem.IsChecked = _settings.WordWrap;
            RawMarkdownTextBox.TextWrapping = _settings.WordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;

            UpdatePluginMenuChecks();
            UpdateOpenFilesInNewTabMenuChecks();

            Width = _settings.WindowWidth;
            Height = _settings.WindowHeight;
            if (_settings.WindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }

            StateChanged += (s, e) =>
            {
                if (WindowState != WindowState.Minimized && !_isFullScreen)
                {
                    DwmHelper.ApplyTitleBarTheme(this, ThemeManager.Instance.CurrentPalette);
                }
            };

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] rawArgs = Environment.GetCommandLineArgs();
            string[] args = rawArgs.Length > 1 ? rawArgs.Skip(1).ToArray() : Array.Empty<string>();
            bool loadedAny = false;

            if (args.Length > 0 && (args[0].Equals("--verify-integrity", StringComparison.OrdinalIgnoreCase) ||
                                    args[0].Equals("--verify", StringComparison.OrdinalIgnoreCase) ||
                                    args[0].Equals("--hash", StringComparison.OrdinalIgnoreCase)))
            {
                string target = args.Length > 1 ? args[1] : string.Empty;
                var dlg = new VerifyIntegrityWindow(target) { Owner = this };
                dlg.ShowDialog();
                return;
            }

            var startupFiles = App.ResolveStartupFiles(_settings, args);

            if (startupFiles.Count > 0)
            {
                string? desiredActiveFile = _settings.ActiveFile;

                for (int i = 0; i < startupFiles.Count; i++)
                {
                    OpenDocument(startupFiles[i], activate: false, saveSession: false, openInNewTab: true);
                    loadedAny = true;
                }

                BuildRecentFilesMenu();

                DocumentTabItem? targetTab = null;
                if (!string.IsNullOrEmpty(desiredActiveFile))
                {
                    targetTab = _tabs.FirstOrDefault(t => t.FilePath.Equals(desiredActiveFile, StringComparison.OrdinalIgnoreCase));
                }
                targetTab ??= _tabs.FirstOrDefault();

                if (targetTab != null)
                {
                    SetActiveTab(targetTab);
                }
                else
                {
                    SetActiveTab(null);
                }

                SaveSessionState();
            }

            if (!loadedAny)
            {
                if (_settings.IsFirstRun)
                {
                    _settings.FirstRunCompleted = true;
                    _settings.Save();

                    string samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_docs", "welcome.md");
                    if (File.Exists(samplePath))
                    {
                        OpenDocument(samplePath);
                    }
                    else
                    {
                        ShowWelcomeDocument();
                    }
                }
                else
                {
                    SetActiveTab(null);
                }
            }

            if (UpdateService.ShouldCheckOnStartup(_settings, DateTime.UtcNow))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(2000).ConfigureAwait(false);
                        var updateResult = await _updateService.CheckForUpdatesAsync().ConfigureAwait(false);
                        if (updateResult != null && updateResult.IsSuccess)
                        {
                            _settings.LastUpdateCheckUtc = DateTime.UtcNow;
                            _settings.Save();

                            if (updateResult.IsUpdateAvailable)
                            {
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    if (!IsLoaded || !IsVisible) return;
                                    try
                                    {
                                        var dlg = new UpdateDialog(updateResult, _updateService) { Owner = this };
                                        if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.VerifiedInstallerPath))
                                        {
                                            CloseAndLaunchInstaller(dlg.VerifiedInstallerPath);
                                        }
                                    }
                                    catch
                                    {
                                        // Window closing during startup check
                                    }
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Background check must never disturb startup or crash
                    }
                });
            }
        }

        public void OpenDocument(string filePath, string? anchor = null, bool activate = true, bool saveSession = true, bool? openInNewTab = null)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            string fullPath = Path.GetFullPath(filePath);

            // If already open in a tab, switch to it
            var existing = _tabs.FirstOrDefault(t => t.FilePath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (activate)
                {
                    SetActiveTab(existing);
                }
                if (!string.IsNullOrEmpty(anchor))
                {
                    MarkdownViewer.ScrollToAnchor(anchor);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MarkdownViewer.ScrollToAnchor(anchor);
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                return;
            }

            try
            {
                string text = File.ReadAllText(fullPath);
                var doc = _parser.Parse(text);

                string title = !string.IsNullOrEmpty(doc.Title) ? doc.Title : Path.GetFileName(fullPath);

                var tab = new DocumentTabItem
                {
                    FilePath = fullPath,
                    Title = title,
                    RawMarkdown = text,
                    Document = doc,
                    Headings = ExtractHeadings(doc),
                    LineEndingName = text.Contains("\r\n") ? "CRLF" : "LF"
                };

                RenderDocumentTab(tab);

                bool shouldOpenInNewTab = openInNewTab ?? _settings.OpenFilesInNewTab;

                // If user has a single clean untitled/welcome tab, reuse it rather than creating extra tab
                if (_tabs.Count == 1 && string.IsNullOrEmpty(_tabs[0].FilePath) && !_tabs[0].IsDirty)
                {
                    _tabs[0] = tab;
                }
                else if (shouldOpenInNewTab)
                {
                    _tabs.Add(tab);
                }
                else
                {
                    // Open in current tab: replace _activeTab if present
                    if (_activeTab != null)
                    {
                        if (_activeTab.IsDirty)
                        {
                            if (!CloseTab(_activeTab))
                            {
                                return;
                            }
                            _tabs.Add(tab);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_activeTab.FilePath))
                            {
                                _fileWatcher.UnwatchFile(_activeTab.FilePath);
                            }

                            int activeIdx = _tabs.IndexOf(_activeTab);
                            if (activeIdx >= 0)
                            {
                                _tabs[activeIdx] = tab;
                            }
                            else
                            {
                                _tabs.Add(tab);
                            }
                        }
                    }
                    else
                    {
                        _tabs.Add(tab);
                    }
                }

                _settings.AddRecentFile(fullPath);
                if (saveSession)
                {
                    _settings.Save();
                    BuildRecentFilesMenu();
                }

                if (_settings.AutoReload)
                {
                    _fileWatcher.WatchFile(fullPath);
                }

                RebuildTabStrip();
                if (activate)
                {
                    SetActiveTab(tab);
                }
                if (saveSession)
                {
                    SaveSessionState();
                }

                if (!string.IsNullOrEmpty(anchor))
                {
                    MarkdownViewer.ScrollToAnchor(anchor);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MarkdownViewer.ScrollToAnchor(anchor);
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open markdown file:\n{ex.Message}", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowWelcomeDocument()
        {
            string welcomeMarkdown = @"# Welcome to MDPlus

**MDPlus** is a high-speed, lightweight native Windows Markdown viewer inspired by the simplicity and performance of Notepad and Notepad++.

---

## 🚀 Key Highlights

- **⚡ Blistering Speed:** Instant launch and sub-millisecond document parsing with zero Electron bloat.
- **📄 Full GFM Support:** CommonMark + GitHub Flavored Markdown (tables, checklists, blockquotes, code blocks).
- **🎨 Dark & Light Modes:** Respects Windows system theme or toggle manually via `F8`.
- **📑 Tabbed Browsing:** Open and view multiple documents side by side in tabs.
- **🔄 Live Auto-Reload:** Automatically refreshes when you edit files in external editors.
- **🔍 Quick Find (`Ctrl+F`):** Rapid in-document search with match count and navigation.
- **🗂️ Outline Sidebar (`Ctrl+T`):** Jump to any section in one click.

---

## 📝 GitHub Flavored Markdown Demos

### Task Lists / Checklists
- [x] Lightweight native architecture
- [x] Dark mode and light mode
- [x] Syntax-highlighted code blocks
- [ ] Try opening your own `.md` file!

### GFM Table

| Feature | MDPlus | MarkText / Electron Apps |
| :--- | :---: | :---: |
| **Startup Time** | **< 150 ms** | 2,500+ ms |
| **Memory Usage** | **~25 MB** | ~350 MB |
| **Native Feel** | **Win32 / WPF DirectWrite** | Chromium WebView |
| **File Watcher** | Built-in live reload | Slow / Resource-heavy |

### Code Blocks with Syntax Highlighting

```csharp
// MDPlus Markdown Parser
var parser = new MarkdownParser();
var doc = parser.Parse(""# Hello World"");
Console.WriteLine($""Parsed {doc.Blocks.Count} blocks in 2ms!"");
```

### GitHub Callout Alerts

> [!NOTE]
> You can open any Markdown file by pressing **Ctrl+O** or simply dragging and dropping it into this window.

> [!TIP]
> Press **Ctrl+2** to toggle Split View and inspect the raw markdown syntax side by side with the formatted document!

### 🔬 Rendering Plugins (v1.07)

- **Vector LaTeX Math:** $E = mc^2$ and $\int_{-\infty}^{\infty} e^{-x^2} dx = \sqrt{\pi}$
- **Display Formulas:**
$$
f(x) = \frac{1}{\sigma \sqrt{2\pi}} e^{-\frac{1}{2}\left(\frac{x - \mu}{\sigma}\right)^2}
$$
- **Native HTML Elements:** <kbd>Ctrl</kbd> + <kbd>P</kbd>, <u>Underlined</u>, <mark>Highlight</mark>, and H<sub>2</sub>O.
- **Interactive Disclosure Widget:**
<details>
<summary>Click to expand plugin info</summary>
Plugins can be enabled or disabled instantly via the Plugins menu without restarting!
</details>

---

### 📚 Explore Sample Documentation
- 📖 [GitHub Flavored Markdown Features Guide](gfm_features.md)
- 💻 [Multi-Language Syntax-Highlighted Code Samples](code_samples.md)

---

*Enjoy distraction-free, lightning-fast Markdown viewing with MDPlus!*
";

            var doc = _parser.Parse(welcomeMarkdown);
            var tab = new DocumentTabItem
            {
                FilePath = string.Empty,
                Title = "Welcome",
                RawMarkdown = welcomeMarkdown,
                Document = doc,
                Headings = ExtractHeadings(doc)
            };

            RenderDocumentTab(tab);
            _tabs.Add(tab);
            RebuildTabStrip();
            SetActiveTab(tab);
        }

        private void RenderDocumentTab(DocumentTabItem tab)
        {
            var converter = new MarkdownToWpfConverter(tab.DirectoryName, ThemeManager.Instance.CurrentPalette, _settings.EnableLatexRendering, _settings.EnableHtmlRendering);
            converter.AnchorNavigationRequested += (s, anchor) =>
            {
                MarkdownViewer.ScrollToAnchor(anchor);
                Dispatcher.BeginInvoke(new Action(() => MarkdownViewer.ScrollToAnchor(anchor)), System.Windows.Threading.DispatcherPriority.Loaded);
            };
            converter.FileNavigationRequested += (s, e) => OpenDocument(e.FilePath, e.Anchor);
            tab.FlowDocument = converter.Convert(tab.Document);
        }

        private List<HeadingItem> ExtractHeadings(MarkdownDocument doc)
        {
            var list = new List<HeadingItem>();
            foreach (var block in doc.Blocks)
            {
                if (block is HeadingBlock h)
                {
                    list.Add(new HeadingItem
                    {
                        Level = h.Level,
                        Text = h.Text,
                        Anchor = h.Anchor,
                        LineIndex = h.LineIndex
                    });
                }
            }
            return list;
        }

        private void SetActiveTab(DocumentTabItem? tab)
        {
            _activeTab = tab;

            if (tab == null)
            {
                WelcomeScreen.Visibility = Visibility.Visible;
                MarkdownViewer.Document = null;
                RawMarkdownTextBox.Text = string.Empty;
                TocListBox.ItemsSource = null;
                Title = $"MDPlus v{UpdateService.GetCurrentVersion().TrimStart('v', 'V')}";
                UpdateStatusBar();
                RebuildTabStrip();
                return;
            }

            WelcomeScreen.Visibility = Visibility.Collapsed;

            // Render if not yet rendered
            if (tab.FlowDocument == null)
            {
                RenderDocumentTab(tab);
            }

            _suppressDirtyTracking = true;
            try
            {
                MarkdownViewer.Document = tab.FlowDocument;
                MarkdownViewer.Zoom = tab.Zoom;
                RawMarkdownTextBox.FontSize = 13.0 * (tab.Zoom / 100.0);
                RawMarkdownTextBox.Text = tab.RawMarkdown;
            }
            finally
            {
                _suppressDirtyTracking = false;
            }
            TocListBox.ItemsSource = tab.Headings;

            UpdateViewDisplayMode(tab.ViewMode);

            Title = $"{tab.DisplayTitle} - MDPlus";
            if (!string.IsNullOrEmpty(tab.FilePath))
            {
                _settings.ActiveFile = tab.FilePath;
            }
            UpdateStatusBar();

            if (DocumentFindBar.Visibility == Visibility.Visible)
            {
                OnFindRequested(this, new FindEventArgs
                {
                    SearchText = DocumentFindBar.FindTextBox.Text,
                    MatchCase = DocumentFindBar.MatchCaseCheckBox.IsChecked == true,
                    Forward = true
                });
            }

            RebuildTabStrip();
        }

        private void RebuildTabStrip()
        {
            TabStripPanel.Children.Clear();
            var palette = ThemeManager.Instance.CurrentPalette;

            foreach (var tab in _tabs)
            {
                bool isActive = tab == _activeTab;

                var tabBorder = new Border
                {
                    Background = isActive ? palette.TabActiveBg : palette.TabInactiveBg,
                    BorderBrush = palette.Border,
                    BorderThickness = new Thickness(1, 1, 1, isActive ? 0 : 1),
                    CornerRadius = new CornerRadius(4, 4, 0, 0),
                    Margin = new Thickness(2, 4, 2, 0),
                    Padding = new Thickness(10, 4, 6, 4),
                    Cursor = Cursors.Hand,
                    ToolTip = tab.FilePath
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                var titleBlock = new TextBlock
                {
                    Text = tab.DisplayTitle,
                    Foreground = isActive ? palette.EditorFg : palette.MutedFg,
                    FontWeight = isActive ? FontWeights.SemiBold : FontWeights.Normal,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                sp.Children.Add(titleBlock);

                var closeBtn = new Button
                {
                    Content = "✕",
                    Width = 18,
                    Height = 18,
                    Background = Brushes.Transparent,
                    Foreground = palette.MutedFg,
                    BorderThickness = new Thickness(0),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand,
                    ToolTip = "Close Tab (Ctrl+W)",
                    VerticalAlignment = VerticalAlignment.Center,
                    Focusable = false
                };

                var closeBtnTemplate = new ControlTemplate(typeof(Button));
                var borderFactory = new FrameworkElementFactory(typeof(Border), "border");
                borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
                borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
                var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
                borderFactory.AppendChild(contentFactory);
                closeBtnTemplate.VisualTree = borderFactory;

                var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
                hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, palette.MenuHoverBg, "border"));
                hoverTrigger.Setters.Add(new Setter(Button.ForegroundProperty, palette.MenuHoverFg));
                closeBtnTemplate.Triggers.Add(hoverTrigger);

                closeBtn.Template = closeBtnTemplate;

                var currentTab = tab;
                closeBtn.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    e.Handled = true;
                    CloseTab(currentTab);
                };
                closeBtn.Click += (s, e) =>
                {
                    e.Handled = true;
                    CloseTab(currentTab);
                };

                tabBorder.MouseLeftButtonDown += (s, e) => SetActiveTab(currentTab);
                tabBorder.MouseDown += (s, e) =>
                {
                    if (e.MiddleButton == MouseButtonState.Pressed)
                    {
                        CloseTab(currentTab);
                    }
                };

                sp.Children.Add(closeBtn);
                tabBorder.Child = sp;
                TabStripPanel.Children.Add(tabBorder);
            }

            NewTabButton.Foreground = palette.MutedFg;
            TabStripPanel.Children.Add(NewTabButton);
        }

        private bool CloseTab(DocumentTabItem tab)
        {
            if (tab == null) return true;

            if (tab.IsDirty)
            {
                string name = !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.FileName;
                var result = MessageBox.Show(
                    $"Do you want to save changes to '{name}'?",
                    "MDPlus",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (!SaveTab(tab))
                    {
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(tab.FilePath))
            {
                _fileWatcher.UnwatchFile(tab.FilePath);
            }

            int index = _tabs.IndexOf(tab);
            if (index < 0)
            {
                return false;
            }
            _tabs.RemoveAt(index);

            if (_activeTab == tab)
            {
                if (_tabs.Count > 0)
                {
                    int nextIndex = Math.Min(index, _tabs.Count - 1);
                    SetActiveTab(_tabs[nextIndex]);
                }
                else
                {
                    SetActiveTab(null);
                }
            }
            else
            {
                RebuildTabStrip();
            }

            SaveSessionState();
            return true;
        }

        private void OnExternalFileChanged(object? sender, string filePath)
        {
            Dispatcher.Invoke(async () =>
            {
                var tab = _tabs.FirstOrDefault(t => t.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                if (tab != null)
                {
                    string text = string.Empty;
                    bool success = false;

                    // Editors often lock or write atomically. Retry reading up to 5 times.
                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        try
                        {
                            if (File.Exists(filePath))
                            {
                                text = File.ReadAllText(filePath);
                                success = true;
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            await System.Threading.Tasks.Task.Delay(50);
                        }
                        catch
                        {
                            break;
                        }
                    }

                    if (!success) return;

                    try
                    {
                        double scrollOffset = MarkdownViewer.VerticalOffset;
                        double rawScroll = RawMarkdownTextBox.VerticalOffset;
                        int rawCaret = RawMarkdownTextBox.CaretIndex;

                        tab.RawMarkdown = text;
                        tab.LineEndingName = text.Contains("\r\n") ? "CRLF" : "LF";
                        tab.Document = _parser.Parse(text);
                        tab.Headings = ExtractHeadings(tab.Document);
                        tab.Title = !string.IsNullOrEmpty(tab.Document.Title) ? tab.Document.Title : Path.GetFileName(tab.FilePath);
                        RenderDocumentTab(tab);

                        if (tab == _activeTab)
                        {
                            MarkdownViewer.Document = tab.FlowDocument;
                            RawMarkdownTextBox.Text = tab.RawMarkdown;
                            TocListBox.ItemsSource = tab.Headings;
                            StatusStatsText.Text = tab.StatsText;
                            StatusEncodingText.Text = $"{tab.EncodingName} • {tab.LineEndingName}";
                            Title = $"{tab.FileName} - MDPlus";

                            MarkdownViewer.ScrollToVerticalOffset(scrollOffset);
                            RawMarkdownTextBox.ScrollToVerticalOffset(rawScroll);
                            if (rawCaret >= 0 && rawCaret <= tab.RawMarkdown.Length)
                            {
                                RawMarkdownTextBox.CaretIndex = rawCaret;
                            }
                        }

                        RebuildTabStrip();
                    }
                    catch
                    {
                        // Handle parse/render error gracefully
                    }
                }
            });
        }

        private void OnFindRequested(object? sender, FindEventArgs e)
        {
            if (_activeTab?.ViewMode == ViewDisplayMode.Raw)
            {
                var (current, total) = SearchRawTextBox(e.SearchText, e.MatchCase, e.Forward);
                DocumentFindBar.SetMatchCount(current, total);
            }
            else
            {
                var (current, total) = MarkdownViewer.SearchText(e.SearchText, e.MatchCase, e.Forward);
                if (_activeTab?.ViewMode == ViewDisplayMode.Split)
                {
                    SearchRawTextBox(e.SearchText, e.MatchCase, e.Forward);
                }
                DocumentFindBar.SetMatchCount(current, total);
            }
        }

        private (int current, int total) SearchRawTextBox(string searchText, bool matchCase, bool forward)
        {
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(RawMarkdownTextBox.Text))
            {
                return (0, 0);
            }

            string content = RawMarkdownTextBox.Text;
            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var indices = new List<int>();
            int idx = 0;
            while (idx < content.Length)
            {
                int found = content.IndexOf(searchText, idx, comparison);
                if (found == -1) break;
                indices.Add(found);
                idx = found + Math.Max(1, searchText.Length);
            }

            if (indices.Count == 0)
            {
                return (0, 0);
            }

            int currentCaret = RawMarkdownTextBox.SelectionStart;
            int chosenIndex = -1;
            int matchNum = 0;

            if (forward)
            {
                int startIndex = currentCaret + Math.Max(1, RawMarkdownTextBox.SelectionLength);
                for (int m = 0; m < indices.Count; m++)
                {
                    if (indices[m] >= startIndex)
                    {
                        chosenIndex = indices[m];
                        matchNum = m + 1;
                        break;
                    }
                }
                if (chosenIndex == -1)
                {
                    chosenIndex = indices[0];
                    matchNum = 1;
                }
            }
            else
            {
                int startIndex = currentCaret - 1;
                for (int m = indices.Count - 1; m >= 0; m--)
                {
                    if (indices[m] <= startIndex)
                    {
                        chosenIndex = indices[m];
                        matchNum = m + 1;
                        break;
                    }
                }
                if (chosenIndex == -1)
                {
                    chosenIndex = indices[indices.Count - 1];
                    matchNum = indices.Count;
                }
            }

            if (chosenIndex >= 0)
            {
                RawMarkdownTextBox.Select(chosenIndex, searchText.Length);
                int lineIndex = RawMarkdownTextBox.GetLineIndexFromCharacterIndex(chosenIndex);
                RawMarkdownTextBox.ScrollToLine(lineIndex);
                RawMarkdownTextBox.Focus();
            }

            return (matchNum, indices.Count);
        }

        private void UpdateViewDisplayMode(ViewDisplayMode mode)
        {
            if (_activeTab == null) return;
            var oldMode = _activeTab.ViewMode;
            _activeTab.ViewMode = mode;

            // View Synchronization
            if (mode == ViewDisplayMode.Raw)
            {
                if (oldMode != ViewDisplayMode.Raw && _activeTab.FlowDocument != null)
                {
                    string serialized = MarkdownSerializer.Serialize(_activeTab.FlowDocument);
                    _activeTab.RawMarkdown = serialized;
                    _suppressDirtyTracking = true;
                    try
                    {
                        RawMarkdownTextBox.Text = serialized;
                    }
                    finally
                    {
                        _suppressDirtyTracking = false;
                    }
                }
            }
            else if (mode == ViewDisplayMode.Rendered)
            {
                if (oldMode == ViewDisplayMode.Raw)
                {
                    string raw = RawMarkdownTextBox.Text;
                    _activeTab.RawMarkdown = raw;
                    _activeTab.Document = _parser.Parse(raw);
                    _activeTab.Headings = ExtractHeadings(_activeTab.Document);
                    RenderDocumentTab(_activeTab);
                    _suppressDirtyTracking = true;
                    try
                    {
                        MarkdownViewer.Document = _activeTab.FlowDocument;
                    }
                    finally
                    {
                        _suppressDirtyTracking = false;
                    }
                    TocListBox.ItemsSource = _activeTab.Headings;
                    UpdateStatusBar();
                }
            }
            else if (mode == ViewDisplayMode.Split)
            {
                if (oldMode == ViewDisplayMode.Raw)
                {
                    string raw = RawMarkdownTextBox.Text;
                    _activeTab.RawMarkdown = raw;
                    _activeTab.Document = _parser.Parse(raw);
                    _activeTab.Headings = ExtractHeadings(_activeTab.Document);
                    RenderDocumentTab(_activeTab);
                    _suppressDirtyTracking = true;
                    try
                    {
                        MarkdownViewer.Document = _activeTab.FlowDocument;
                    }
                    finally
                    {
                        _suppressDirtyTracking = false;
                    }
                    TocListBox.ItemsSource = _activeTab.Headings;
                    UpdateStatusBar();
                }
                else if (oldMode == ViewDisplayMode.Rendered && _activeTab.FlowDocument != null)
                {
                    string serialized = MarkdownSerializer.Serialize(_activeTab.FlowDocument);
                    _activeTab.RawMarkdown = serialized;
                    _suppressDirtyTracking = true;
                    try
                    {
                        RawMarkdownTextBox.Text = serialized;
                    }
                    finally
                    {
                        _suppressDirtyTracking = false;
                    }
                }
            }

            switch (mode)
            {
                case ViewDisplayMode.Rendered:
                    RenderedColumn.Width = new GridLength(1, GridUnitType.Star);
                    RawColumn.Width = new GridLength(0);
                    ContentSplitterColumn.Width = new GridLength(0);
                    ContentSplitter.Visibility = Visibility.Collapsed;
                    MarkdownViewer.Visibility = Visibility.Visible;
                    RawMarkdownTextBox.Visibility = Visibility.Collapsed;
                    ViewRenderedItem.IsChecked = true;
                    ViewSplitItem.IsChecked = false;
                    ViewRawItem.IsChecked = false;
                    if (HamburgerViewRenderedItem != null) HamburgerViewRenderedItem.IsChecked = true;
                    if (HamburgerViewSplitItem != null) HamburgerViewSplitItem.IsChecked = false;
                    if (HamburgerViewRawItem != null) HamburgerViewRawItem.IsChecked = false;
                    break;

                case ViewDisplayMode.Split:
                    RenderedColumn.Width = new GridLength(1, GridUnitType.Star);
                    RawColumn.Width = new GridLength(1, GridUnitType.Star);
                    ContentSplitterColumn.Width = new GridLength(5);
                    ContentSplitter.Visibility = Visibility.Visible;
                    MarkdownViewer.Visibility = Visibility.Visible;
                    RawMarkdownTextBox.Visibility = Visibility.Visible;
                    ViewRenderedItem.IsChecked = false;
                    ViewSplitItem.IsChecked = true;
                    ViewRawItem.IsChecked = false;
                    if (HamburgerViewRenderedItem != null) HamburgerViewRenderedItem.IsChecked = false;
                    if (HamburgerViewSplitItem != null) HamburgerViewSplitItem.IsChecked = true;
                    if (HamburgerViewRawItem != null) HamburgerViewRawItem.IsChecked = false;
                    break;

                case ViewDisplayMode.Raw:
                    RenderedColumn.Width = new GridLength(0);
                    RawColumn.Width = new GridLength(1, GridUnitType.Star);
                    ContentSplitterColumn.Width = new GridLength(0);
                    ContentSplitter.Visibility = Visibility.Collapsed;
                    MarkdownViewer.Visibility = Visibility.Collapsed;
                    RawMarkdownTextBox.Visibility = Visibility.Visible;
                    ViewRenderedItem.IsChecked = false;
                    ViewSplitItem.IsChecked = false;
                    ViewRawItem.IsChecked = true;
                    if (HamburgerViewRenderedItem != null) HamburgerViewRenderedItem.IsChecked = false;
                    if (HamburgerViewSplitItem != null) HamburgerViewSplitItem.IsChecked = false;
                    if (HamburgerViewRawItem != null) HamburgerViewRawItem.IsChecked = true;
                    break;
            }

            StatusViewModeText.Text = mode.ToString();
        }

        private void SetSidebarVisibility(bool visible)
        {
            if (visible)
            {
                SidebarColumn.Width = new GridLength(260);
                SidebarSplitterColumn.Width = new GridLength(5);
                SidebarBorder.Visibility = Visibility.Visible;
                SidebarSplitter.Visibility = Visibility.Visible;
                TocMenuItem.IsChecked = true;
                if (HamburgerTocMenuItem != null) HamburgerTocMenuItem.IsChecked = true;
            }
            else
            {
                SidebarColumn.Width = new GridLength(0);
                SidebarSplitterColumn.Width = new GridLength(0);
                SidebarBorder.Visibility = Visibility.Collapsed;
                SidebarSplitter.Visibility = Visibility.Collapsed;
                TocMenuItem.IsChecked = false;
                if (HamburgerTocMenuItem != null) HamburgerTocMenuItem.IsChecked = false;
            }
        }

        private void ApplyTheme()
        {
            var palette = ThemeManager.Instance.CurrentPalette;
            bool isDark = ThemeManager.Instance.IsDark;

            RootGrid.Background = palette.WindowBg;
            MainMenu.Background = palette.MenuBg;
            MainMenu.Foreground = palette.MenuFg;
            MainMenu.BorderBrush = palette.Border;

            TabBarBorder.Background = palette.SidebarBg;
            TabBarBorder.BorderBrush = palette.Border;
            if (NewTabButton != null) NewTabButton.Foreground = palette.MutedFg;

            SidebarBorder.Background = palette.SidebarBg;
            SidebarBorder.BorderBrush = palette.Border;
            if (SidebarHeaderBorder != null)
            {
                SidebarHeaderBorder.Background = palette.MenuBg;
                SidebarHeaderBorder.BorderBrush = palette.Border;
            }
            if (SidebarHeaderTitle != null)
            {
                SidebarHeaderTitle.Foreground = palette.MutedFg;
            }
            if (SidebarCloseButton != null)
            {
                SidebarCloseButton.Foreground = palette.MutedFg;
            }
            if (SidebarSplitter != null)
            {
                SidebarSplitter.Background = palette.Border;
            }
            if (ContentSplitter != null)
            {
                ContentSplitter.Background = palette.Border;
            }

            ContentGrid.Background = palette.EditorBg;
            WelcomeScreen.Background = palette.EditorBg;

            RawMarkdownTextBox.Background = palette.EditorBg;
            RawMarkdownTextBox.Foreground = palette.EditorFg;

            AppStatusBar.Background = palette.StatusBg;
            AppStatusBar.Foreground = palette.StatusFg;

            DocumentFindBar.ApplyTheme(isDark);
            UpdateThemeMenuChecks();

            ReRenderAllTabs();

            RebuildTabStrip();
            DwmHelper.ApplyTitleBarTheme(this, palette);
        }

        private void ReRenderAllTabs()
        {
            foreach (var tab in _tabs)
            {
                RenderDocumentTab(tab);
            }

            if (_activeTab != null)
            {
                MarkdownViewer.Document = _activeTab.FlowDocument;
            }
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void BuildRecentFilesMenu()
        {
            PopulateRecentMenu(RecentFilesMenu);
            if (HamburgerRecentFilesMenu != null)
            {
                PopulateRecentMenu(HamburgerRecentFilesMenu);
            }
        }

        private void PopulateRecentMenu(MenuItem menu)
        {
            if (menu == null) return;
            menu.Items.Clear();

            if (_settings.RecentFiles.Count == 0)
            {
                var emptyItem = new MenuItem { Header = "No recent files", IsEnabled = false };
                menu.Items.Add(emptyItem);
                return;
            }

            foreach (var file in _settings.RecentFiles)
            {
                string headerText = Path.GetFileName(file)?.Replace("_", "__") ?? file;
                var item = new MenuItem { Header = headerText, ToolTip = file };
                string target = file;
                item.Click += (s, e) =>
                {
                    if (File.Exists(target))
                    {
                        OpenDocument(target);
                    }
                    else
                    {
                        MessageBox.Show($"File no longer exists:\n{target}", "MDPlus", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _settings.RecentFiles.Remove(target);
                        _settings.Save();
                        BuildRecentFilesMenu();
                    }
                };
                menu.Items.Add(item);
            }

            menu.Items.Add(new Separator());
            var clearItem = new MenuItem { Header = "Clear Recent Files" };
            clearItem.Click += (s, e) =>
            {
                _settings.RecentFiles.Clear();
                _settings.Save();
                BuildRecentFilesMenu();
            };
            menu.Items.Add(clearItem);
        }

        private void SaveSessionState()
        {
            _settings.UpdateOpenFiles(
                _tabs.Where(t => !string.IsNullOrEmpty(t.FilePath)).Select(t => t.FilePath),
                _activeTab?.FilePath);
            _settings.Save();
        }

        private void UpdateStartupModeMenu()
        {
            bool resume = _settings.ResumeSession;
            if (StartupResumeItem != null) StartupResumeItem.IsChecked = resume;
            if (StartupFreshItem != null) StartupFreshItem.IsChecked = !resume;
            if (HamburgerStartupResumeItem != null) HamburgerStartupResumeItem.IsChecked = resume;
            if (HamburgerStartupFreshItem != null) HamburgerStartupFreshItem.IsChecked = !resume;
        }

        private void StartupResume_Click(object sender, RoutedEventArgs e)
        {
            _settings.ResumeSession = true;
            _settings.Save();
            UpdateStartupModeMenu();
        }

        private void StartupFresh_Click(object sender, RoutedEventArgs e)
        {
            _settings.ResumeSession = false;
            _settings.Save();
            UpdateStartupModeMenu();
        }

        // --- Event Handlers & Actions ---

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Open Markdown Document",
                Filter = "Markdown Files (*.md;*.markdown;*.mdown;*.mkd)|*.md;*.markdown;*.mdown;*.mkd|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (var file in dlg.FileNames)
                {
                    OpenDocument(file);
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null && !string.IsNullOrEmpty(_activeTab.FilePath))
            {
                string dir = _activeTab.DirectoryName;
                if (Directory.Exists(dir))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", dir) { UseShellExecute = true });
                }
            }
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            var doc = _parser.Parse(string.Empty);
            var tab = new DocumentTabItem
            {
                FilePath = string.Empty,
                Title = $"Untitled {_untitledIndex++}",
                RawMarkdown = string.Empty,
                Document = doc,
                Headings = new List<HeadingItem>()
            };
            RenderDocumentTab(tab);
            _tabs.Add(tab);
            RebuildTabStrip();
            SetActiveTab(tab);
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveActiveTab();
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null)
            {
                SaveTab(_activeTab, forceSaveAs: true);
            }
        }

        public bool SaveActiveTab()
        {
            if (_activeTab == null) return false;
            return SaveTab(_activeTab, forceSaveAs: false);
        }

        public bool SaveTab(DocumentTabItem tab, bool forceSaveAs = false)
        {
            if (tab == null) return false;

            // 1. Sync latest content into tab.RawMarkdown
            if (tab == _activeTab)
            {
                if (tab.ViewMode == ViewDisplayMode.Raw)
                {
                    tab.RawMarkdown = RawMarkdownTextBox.Text;
                }
                else
                {
                    if (tab.FlowDocument != null)
                    {
                        tab.RawMarkdown = MarkdownSerializer.Serialize(tab.FlowDocument);
                        if (tab.ViewMode == ViewDisplayMode.Split)
                        {
                            _suppressDirtyTracking = true;
                            try
                            {
                                RawMarkdownTextBox.Text = tab.RawMarkdown;
                            }
                            finally
                            {
                                _suppressDirtyTracking = false;
                            }
                        }
                    }
                }
            }
            else
            {
                if (tab.FlowDocument != null)
                {
                    tab.RawMarkdown = MarkdownSerializer.Serialize(tab.FlowDocument);
                }
            }

            // 2. Handle Untitled or Save As
            if (string.IsNullOrEmpty(tab.FilePath) || forceSaveAs)
            {
                var dlg = new SaveFileDialog
                {
                    Title = "Save Markdown File",
                    Filter = "Markdown Files (*.md)|*.md|All Files (*.*)|*.*",
                    FileName = !string.IsNullOrEmpty(tab.FilePath) ? Path.GetFileName(tab.FilePath) : (tab.FileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? tab.FileName : $"{tab.FileName}.md"),
                    DefaultExt = ".md"
                };

                if (dlg.ShowDialog(this) != true)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(tab.FilePath) && !tab.FilePath.Equals(dlg.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    _fileWatcher.UnwatchFile(tab.FilePath);
                }

                tab.FilePath = dlg.FileName;
                tab.Title = Path.GetFileName(tab.FilePath);
            }

            // 3. Suppress FileWatcher before writing
            _fileWatcher.SuppressNextChange(tab.FilePath);

            try
            {
                File.WriteAllText(tab.FilePath, tab.RawMarkdown, new UTF8Encoding(false));

                if (_settings.AutoReload)
                {
                    _fileWatcher.WatchFile(tab.FilePath);
                }

                tab.MarkClean();
                RebuildTabStrip();
                UpdateStatusBar();
                if (_activeTab == tab)
                {
                    Title = $"{tab.DisplayTitle} - MDPlus";
                    StatusFileText.Text = tab.FilePath;
                }

                _settings.AddRecentFile(tab.FilePath);
                _settings.Save();
                BuildRecentFilesMenu();
                SaveSessionState();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save file '{tab.FilePath}':\n{ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void MarkdownViewer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressDirtyTracking || _activeTab == null) return;
            if (!_activeTab.IsDirty)
            {
                _activeTab.MarkDirty();
                RebuildTabStrip();
                Title = $"{_activeTab.DisplayTitle} - MDPlus";
            }
        }

        private void RawMarkdownTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressDirtyTracking || _activeTab == null) return;
            if (!_activeTab.IsDirty)
            {
                _activeTab.MarkDirty();
                RebuildTabStrip();
                Title = $"{_activeTab.DisplayTitle} - MDPlus";
            }
        }

        private void MarkdownViewer_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (_suppressDirtyTracking || _activeTab == null) return;
            if (e.OriginalSource is CheckBox)
            {
                if (!_activeTab.IsDirty)
                {
                    _activeTab.MarkDirty();
                    RebuildTabStrip();
                    Title = $"{_activeTab.DisplayTitle} - MDPlus";
                }
            }
        }

        private void UpdateStatusBar()
        {
            if (_activeTab == null)
            {
                StatusFileText.Text = "Ready";
                StatusStatsText.Text = string.Empty;
                StatusZoomText.Text = "100%";
                StatusEncodingText.Text = string.Empty;
                StatusViewModeText.Text = string.Empty;
                return;
            }
            StatusFileText.Text = string.IsNullOrEmpty(_activeTab.FilePath) ? _activeTab.Title : _activeTab.FilePath;
            StatusStatsText.Text = _activeTab.StatsText;
            StatusZoomText.Text = _activeTab.ZoomText;
            StatusEncodingText.Text = $"{_activeTab.EncodingName} • {_activeTab.LineEndingName}";
            StatusViewModeText.Text = _activeTab.ViewMode.ToString();
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null)
            {
                CloseTab(_activeTab);
            }
        }

        private void CloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            while (_tabs.Count > 0)
            {
                if (!CloseTab(_tabs[0]))
                {
                    break;
                }
            }
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null && !string.IsNullOrEmpty(_activeTab.FilePath))
            {
                OpenDocument(_activeTab.FilePath);
            }
        }

        private void ExportHtml_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null) return;

            var dlg = new SaveFileDialog
            {
                Title = "Export as HTML",
                Filter = "HTML Document (*.html;*.htm)|*.html;*.htm",
                FileName = Path.ChangeExtension(_activeTab.FileName, ".html")
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    string html = HtmlExporter.ExportToFullHtml(_activeTab.Document, _activeTab.Title, ThemeManager.Instance.IsDark);
                    File.WriteAllText(dlg.FileName, html);
                    MessageBox.Show("HTML exported successfully!", "Export HTML", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export HTML:\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyHtml_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null) return;

            string html = HtmlExporter.ExportBodyHtml(_activeTab.Document);
            if (ClipboardHelper.SetText(html))
            {
                MessageBox.Show("HTML snippet copied to clipboard!", "MDPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Clipboard is busy or could not be accessed.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (MarkdownViewer.Document == null) return;

            var printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                IDocumentPaginatorSource idp = MarkdownViewer.Document;
                printDlg.PrintDocument(idp.DocumentPaginator, _activeTab?.Title ?? "Markdown Document");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.ViewMode == ViewDisplayMode.Raw)
            {
                RawMarkdownTextBox.Copy();
            }
            else
            {
                ApplicationCommands.Copy.Execute(null, MarkdownViewer);
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.ViewMode == ViewDisplayMode.Raw)
            {
                RawMarkdownTextBox.SelectAll();
            }
            else
            {
                ApplicationCommands.SelectAll.Execute(null, MarkdownViewer);
            }
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            DocumentFindBar.Visibility = Visibility.Visible;
            DocumentFindBar.FocusSearchBox();
        }

        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentFindBar.Visibility == Visibility.Visible)
            {
                OnFindRequested(this, new FindEventArgs { SearchText = DocumentFindBar.FindTextBox.Text, MatchCase = DocumentFindBar.MatchCaseCheckBox.IsChecked == true, Forward = true });
            }
            else
            {
                Find_Click(sender, e);
            }
        }

        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentFindBar.Visibility == Visibility.Visible)
            {
                OnFindRequested(this, new FindEventArgs { SearchText = DocumentFindBar.FindTextBox.Text, MatchCase = DocumentFindBar.MatchCaseCheckBox.IsChecked == true, Forward = false });
            }
            else
            {
                Find_Click(sender, e);
            }
        }

        private void UpdateZoom(double newZoom)
        {
            if (_activeTab != null)
            {
                _activeTab.Zoom = newZoom;
                MarkdownViewer.Zoom = _activeTab.Zoom;
                RawMarkdownTextBox.FontSize = 13.0 * (_activeTab.Zoom / 100.0);
                StatusZoomText.Text = _activeTab.ZoomText;
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null) UpdateZoom(_activeTab.Zoom + 10);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null) UpdateZoom(_activeTab.Zoom - 10);
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null) UpdateZoom(100);
        }

        private void ToggleToc_Click(object sender, RoutedEventArgs e)
        {
            bool isVisible = SidebarBorder.Visibility == Visibility.Visible;
            SetSidebarVisibility(!isVisible);
            _settings.ShowToc = !isVisible;
        }

        private void CloseSidebar_Click(object sender, RoutedEventArgs e)
        {
            SetSidebarVisibility(false);
            _settings.ShowToc = false;
        }

        private void ViewRendered_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewDisplayMode(ViewDisplayMode.Rendered);
        }

        private void ViewSplit_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewDisplayMode(ViewDisplayMode.Split);
        }

        private void ViewRaw_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewDisplayMode(ViewDisplayMode.Raw);
        }

        private void ToggleWordWrap_Click(object sender, RoutedEventArgs e)
        {
            _settings.WordWrap = !_settings.WordWrap;
            WordWrapMenuItem.IsChecked = _settings.WordWrap;
            if (HamburgerWordWrapMenuItem != null) HamburgerWordWrapMenuItem.IsChecked = _settings.WordWrap;
            RawMarkdownTextBox.TextWrapping = _settings.WordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            _settings.Save();
        }

        private void UpdateThemeMenuChecks()
        {
            var preset = ThemeManager.Instance.CurrentPreset;
            if (ThemeGitHubDarkItem != null) ThemeGitHubDarkItem.IsChecked = preset == ThemePreset.GitHubDark;
            if (ThemeNordItem != null) ThemeNordItem.IsChecked = preset == ThemePreset.Nord;
            if (ThemeOneDarkItem != null) ThemeOneDarkItem.IsChecked = preset == ThemePreset.OneDark;
            if (ThemeMonokaiItem != null) ThemeMonokaiItem.IsChecked = preset == ThemePreset.Monokai;
            if (ThemeGitHubLightItem != null) ThemeGitHubLightItem.IsChecked = preset == ThemePreset.GitHubLight;
            if (ThemeOneLightItem != null) ThemeOneLightItem.IsChecked = preset == ThemePreset.OneLight;
            if (ThemeSolarizedLightItem != null) ThemeSolarizedLightItem.IsChecked = preset == ThemePreset.SolarizedLight;
            if (ThemeQuietLightItem != null) ThemeQuietLightItem.IsChecked = preset == ThemePreset.QuietLight;

            if (HamburgerThemeGitHubDarkItem != null) HamburgerThemeGitHubDarkItem.IsChecked = preset == ThemePreset.GitHubDark;
            if (HamburgerThemeNordItem != null) HamburgerThemeNordItem.IsChecked = preset == ThemePreset.Nord;
            if (HamburgerThemeOneDarkItem != null) HamburgerThemeOneDarkItem.IsChecked = preset == ThemePreset.OneDark;
            if (HamburgerThemeMonokaiItem != null) HamburgerThemeMonokaiItem.IsChecked = preset == ThemePreset.Monokai;
            if (HamburgerThemeGitHubLightItem != null) HamburgerThemeGitHubLightItem.IsChecked = preset == ThemePreset.GitHubLight;
            if (HamburgerThemeOneLightItem != null) HamburgerThemeOneLightItem.IsChecked = preset == ThemePreset.OneLight;
            if (HamburgerThemeSolarizedLightItem != null) HamburgerThemeSolarizedLightItem.IsChecked = preset == ThemePreset.SolarizedLight;
            if (HamburgerThemeQuietLightItem != null) HamburgerThemeQuietLightItem.IsChecked = preset == ThemePreset.QuietLight;
        }

        public void OpenFile(string filePath)
        {
            OpenDocument(filePath, activate: true, saveSession: true);
        }

        public void RestoreAndActivateWindow()
        {
            DwmHelper.BringWindowToForeground(this);
        }

        private void UpdateOpenFilesInNewTabMenuChecks()
        {
            if (OpenFilesInNewTabMenuItem != null) OpenFilesInNewTabMenuItem.IsChecked = _settings.OpenFilesInNewTab;
            if (HamburgerOpenFilesInNewTabMenuItem != null) HamburgerOpenFilesInNewTabMenuItem.IsChecked = _settings.OpenFilesInNewTab;
        }

        private void OpenFilesInNewTab_Click(object sender, RoutedEventArgs e)
        {
            _settings.OpenFilesInNewTab = !_settings.OpenFilesInNewTab;
            _settings.Save();
            UpdateOpenFilesInNewTabMenuChecks();
        }

        private void UpdatePluginMenuChecks()
        {
            if (PluginLatexMenuItem != null) PluginLatexMenuItem.IsChecked = _settings.EnableLatexRendering;
            if (HamburgerPluginLatexMenuItem != null) HamburgerPluginLatexMenuItem.IsChecked = _settings.EnableLatexRendering;
            if (PluginHtmlMenuItem != null) PluginHtmlMenuItem.IsChecked = _settings.EnableHtmlRendering;
            if (HamburgerPluginHtmlMenuItem != null) HamburgerPluginHtmlMenuItem.IsChecked = _settings.EnableHtmlRendering;
        }

        private void PluginLatex_Click(object sender, RoutedEventArgs e)
        {
            _settings.EnableLatexRendering = !_settings.EnableLatexRendering;
            UpdatePluginMenuChecks();
            _settings.Save();
            ReRenderAllTabs();
        }

        private void PluginHtml_Click(object sender, RoutedEventArgs e)
        {
            _settings.EnableHtmlRendering = !_settings.EnableHtmlRendering;
            UpdatePluginMenuChecks();
            _settings.Save();
            ReRenderAllTabs();
        }

        private void ToggleMenuBar_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenuBar();
        }

        public void ToggleMenuBar()
        {
            bool isVisible = MainMenu.Visibility == Visibility.Visible;
            SetMenuBarVisibility(!isVisible);
            _settings.ShowMenuBar = !isVisible;
            _settings.Save();

            if (!isVisible)
            {
                if (MainMenu.Items.Count > 0 && MainMenu.Items[0] is MenuItem firstItem)
                {
                    firstItem.Focus();
                }
                else
                {
                    MainMenu.Focus();
                }
            }
        }

        public void SetMenuBarVisibility(bool visible)
        {
            MainMenu.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (ToggleMenuBarMenuItem != null) ToggleMenuBarMenuItem.IsChecked = visible;
            if (HamburgerToggleMenuBarItem != null) HamburgerToggleMenuBarItem.IsChecked = visible;
            if (HamburgerViewToggleMenuBarItem != null) HamburgerViewToggleMenuBarItem.IsChecked = visible;
        }

        private void HamburgerMenu_Click(object sender, RoutedEventArgs e)
        {
            if ((DateTime.UtcNow - _lastHamburgerClosedTime).TotalMilliseconds < 200)
            {
                return;
            }

            if (HamburgerContextMenu != null)
            {
                HamburgerContextMenu.PlacementTarget = HamburgerMenuButton;
                HamburgerContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                HamburgerContextMenu.IsOpen = true;
            }
        }

        private void ThemePreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string tagStr && Enum.TryParse<ThemePreset>(tagStr, out var preset))
            {
                ThemeManager.Instance.SetPreset(preset);
                _settings.Theme = preset;
                _settings.Save();
            }
        }

        private void CycleTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.CycleNextTheme();
            _settings.Theme = ThemeManager.Instance.CurrentPreset;
            _settings.Save();
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            CycleTheme_Click(sender, e);
        }

        private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullScreen)
            {
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                _isFullScreen = true;
            }
            else
            {
                WindowStyle = _previousWindowStyle;
                WindowState = _previousWindowState;
                _isFullScreen = false;
                DwmHelper.ApplyTitleBarTheme(this, ThemeManager.Instance.CurrentPalette);
            }
        }

        private void HelpMarkdownRef_Click(object sender, RoutedEventArgs e)
        {
            string guide = @"# CommonMark & GFM Reference Guide

## 1. Headings
```markdown
# Heading 1
## Heading 2
### Heading 3
#### Heading 4
##### Heading 5
###### Heading 6
```

## 2. Text Styling
```markdown
**Bold text** or __bold text__
*Italic text* or _italic text_
***Bold & italic***
~~Strikethrough~~
==Highlight==
`inline code`
```

## 3. Lists
```markdown
- Item A
- Item B
  - Nested item

1. First
2. Second
3. Third

- [x] Completed task
- [ ] Incomplete task
```

## 4. Tables
```markdown
| Left | Center | Right |
| :--- | :---: | ---: |
| 1    | 2     | 3    |
```

## 5. Callouts
```markdown
> [!NOTE]
> Information callout

> [!TIP]
> Helpful tip

> [!WARNING]
> Warning alert
```
";
            var doc = _parser.Parse(guide);
            var tab = new DocumentTabItem
            {
                FilePath = string.Empty,
                Title = "Markdown Reference",
                RawMarkdown = guide,
                Document = doc,
                Headings = ExtractHeadings(doc)
            };
            RenderDocumentTab(tab);
            _tabs.Add(tab);
            RebuildTabStrip();
            SetActiveTab(tab);
        }

        private void HelpShortcuts_Click(object sender, RoutedEventArgs e)
        {
            string shortcuts = @"# MDPlus Keyboard Shortcuts

| Shortcut | Action |
| :--- | :--- |
| **Ctrl + O** | Open Markdown File |
| **Ctrl + N** | New Document / Open File |
| **Ctrl + W** | Close Current Tab |
| **F5** or **Ctrl + R** | Reload Document |
| **Ctrl + F** | Find in Document |
| **F3** / **Shift + F3** | Find Next / Previous |
| **Ctrl + T** | Toggle Table of Contents Outline |
| **Alt** or **Ctrl + M** | Toggle Menu Bar |
| **Ctrl + 1** | Rendered View Only |
| **Ctrl + 2** | Split View (Rendered + Raw) |
| **Ctrl + 3** | Raw Markdown View Only |
| **Ctrl + +** / **Ctrl + -** | Zoom In / Out |
| **Ctrl + 0** | Reset Zoom to 100% |
| **F8** | Cycle Theme (Dark & Light) |
| **F11** | Toggle Full Screen |
| **Ctrl + P** | Print Document |
| **Ctrl + Shift + S** | Export to Standalone HTML |
";
            var doc = _parser.Parse(shortcuts);
            var tab = new DocumentTabItem
            {
                FilePath = string.Empty,
                Title = "Keyboard Shortcuts",
                RawMarkdown = shortcuts,
                Document = doc,
                Headings = ExtractHeadings(doc)
            };
            RenderDocumentTab(tab);
            _tabs.Add(tab);
            RebuildTabStrip();
            SetActiveTab(tab);
        }

        private void ToolsVerifyIntegrity_Click(object sender, RoutedEventArgs e)
        {
            string? currentFile = _activeTab?.FilePath;
            if (string.IsNullOrEmpty(currentFile))
            {
                currentFile = Environment.ProcessPath ?? string.Empty;
            }
            var dlg = new VerifyIntegrityWindow(currentFile) { Owner = this };
            dlg.ShowDialog();
        }

        internal void CloseAndLaunchInstaller(string installerPath)
        {
            if (string.IsNullOrEmpty(installerPath)) return;

            // 1. If any tabs are dirty, prompt user to save or cancel before launching installer
            foreach (var tab in _tabs.ToList())
            {
                if (tab.IsDirty)
                {
                    SetActiveTab(tab);
                    string name = !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.FileName;
                    var result = MessageBox.Show(
                        $"Do you want to save changes to '{name}' before updating?",
                        "MDPlus Update",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (!SaveTab(tab)) return;
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        tab.IsDirty = false;
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
            }

            // 2. Commit session state and settings
            SaveSessionState();
            _settings.Save();

            // 3. Launch installer FIRST to verify UAC elevation before destroying the window
            bool launched = UpdateService.TryLaunchInstaller(installerPath);
            if (!launched)
            {
                // User cancelled UAC prompt; keep the window open and inform the user
                StatusFileText.Text = "Update cancelled by user.";
                UpdateStatusBar();
                return;
            }

            // 4. Installer process has started; now close window and exit cleanly
            bool isClosed = false;
            EventHandler closedHandler = (s, e) => isClosed = true;
            Closed += closedHandler;
            try
            {
                Close();
            }
            finally
            {
                Closed -= closedHandler;
            }

            if (isClosed)
            {
                UpdateService.ExitApplication();
            }
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            string originalStatus = StatusFileText.Text;
            StatusFileText.Text = "Checking for updates...";
            var previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                var result = await _updateService.CheckForUpdatesAsync();
                if (!IsLoaded) return;

                if (result.IsSuccess)
                {
                    _settings.LastUpdateCheckUtc = DateTime.UtcNow;
                    _settings.Save();
                }

                if (!result.IsSuccess)
                {
                    MessageBox.Show(this,
                        $"Unable to check for updates:\n{result.ErrorMessage}\n\nPlease check your network connection and try again.",
                        "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (result.IsUpdateAvailable)
                {
                    var dlg = new UpdateDialog(result, _updateService) { Owner = this };
                    if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.VerifiedInstallerPath))
                    {
                        CloseAndLaunchInstaller(dlg.VerifiedInstallerPath);
                    }
                }
                else
                {
                    MessageBox.Show(this,
                        $"You are running the latest version of MDPlus (v{result.CurrentVersion.TrimStart('v', 'V')}).\nNo updates are currently available.",
                        "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (!IsLoaded) return;
                MessageBox.Show(this,
                    $"An error occurred while checking for updates:\n{ex.Message}",
                    "Check for Updates", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = previousCursor;
                if (IsLoaded)
                {
                    StatusFileText.Text = originalStatus;
                }
            }
        }

        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            string? currentExe = Environment.ProcessPath;
            string sha256 = !string.IsNullOrEmpty(currentExe) && File.Exists(currentExe)
                ? HashService.ComputeSha256(currentExe)
                : "Development Build";

            string ver = UpdateService.GetCurrentVersion().TrimStart('v', 'V');
            MessageBox.Show(
                $"MDPlus - Native Windows Markdown Viewer\nVersion v{ver}\n\n" +
                "A fast, lightweight desktop Markdown display application designed with the simplicity and performance of Notepad and Notepad++.\n\n" +
                $"Current Executable SHA-256 Digest:\n{sha256}\n\n" +
                "Project & Release Hashes:\nhttps://github.com/nickf-sudomania/mdplusplus\n\n" +
                "Zero Electron. Zero Chromium. Instant launch.",
                "About MDPlus",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void TocListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TocListBox.SelectedItem is HeadingItem heading)
            {
                if (_activeTab?.ViewMode == ViewDisplayMode.Raw)
                {
                    if (heading.LineIndex >= 0 && heading.LineIndex < RawMarkdownTextBox.LineCount)
                    {
                        int charIdx = RawMarkdownTextBox.GetCharacterIndexFromLineIndex(heading.LineIndex);
                        int lineLen = RawMarkdownTextBox.GetLineLength(heading.LineIndex);
                        RawMarkdownTextBox.Focus();
                        RawMarkdownTextBox.Select(charIdx, lineLen);
                        RawMarkdownTextBox.ScrollToLine(heading.LineIndex);
                    }
                    else
                    {
                        int lineIdx = RawMarkdownTextBox.Text.IndexOf(heading.Text, StringComparison.OrdinalIgnoreCase);
                        if (lineIdx >= 0)
                        {
                            RawMarkdownTextBox.Select(lineIdx, heading.Text.Length);
                            int line = RawMarkdownTextBox.GetLineIndexFromCharacterIndex(lineIdx);
                            RawMarkdownTextBox.ScrollToLine(line);
                        }
                    }
                }
                else
                {
                    MarkdownViewer.ScrollToAnchor(heading.Anchor);
                }
                Dispatcher.BeginInvoke(new Action(() => TocListBox.SelectedItem = null));
            }
        }

        private void MarkdownViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Delta > 0) ZoomIn_Click(sender, e);
                else if (e.Delta < 0) ZoomOut_Click(sender, e);
                e.Handled = true;
            }
        }

        private void StatusZoom_Click(object sender, MouseButtonEventArgs e)
        {
            ResetZoom_Click(sender, e);
        }

        private void StatusViewMode_Click(object sender, MouseButtonEventArgs e)
        {
            if (_activeTab == null) return;
            var nextMode = _activeTab.ViewMode switch
            {
                ViewDisplayMode.Rendered => ViewDisplayMode.Split,
                ViewDisplayMode.Split => ViewDisplayMode.Raw,
                _ => ViewDisplayMode.Rendered
            };
            UpdateViewDisplayMode(nextMode);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        OpenDocument(file);
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var mods = Keyboard.Modifiers;

            if (e.Key == Key.Escape && DocumentFindBar.Visibility == Visibility.Visible)
            {
                DocumentFindBar.Visibility = Visibility.Collapsed;
                MarkdownViewer.ClearHighlights();
                e.Handled = true;
                return;
            }

            if (mods == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.S:
                        SaveFile_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.O:
                        OpenFile_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.N:
                        NewTab_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.W:
                        CloseTab_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.F:
                        Find_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.R:
                        Reload_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.T:
                        ToggleToc_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.P:
                        Print_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.D1:
                        ViewRendered_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.D2:
                        ViewSplit_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.D3:
                        ViewRaw_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.OemPlus:
                    case Key.Add:
                        ZoomIn_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.OemMinus:
                    case Key.Subtract:
                        ZoomOut_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.M:
                        ToggleMenuBar();
                        e.Handled = true;
                        break;
                    case Key.D0:
                    case Key.NumPad0:
                        ResetZoom_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                }
            }
            else if (mods == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                switch (e.Key)
                {
                    case Key.S:
                        SaveAsFile_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.E:
                        ExportHtml_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.H:
                        CopyHtml_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                }
            }
            else if (mods == ModifierKeys.None)
            {
                switch (e.Key)
                {
                    case Key.F5:
                        Reload_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.F8:
                        ToggleTheme_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.F10:
                        ToggleMenuBar();
                        e.Handled = true;
                        break;
                    case Key.F11:
                        ToggleFullscreen_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    case Key.F3:
                        FindNext_Click(this, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                }
            }
            else if (mods == ModifierKeys.Shift && e.Key == Key.F3)
            {
                FindPrevious_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }

            bool isCtrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) ||
                              Keyboard.IsKeyDown(Key.LeftCtrl) ||
                              Keyboard.IsKeyDown(Key.RightCtrl);

            if (!isCtrlDown &&
                ((e.Key == Key.System && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)) ||
                 e.Key == Key.LeftAlt || e.Key == Key.RightAlt))
            {
                _altKeyCandidate = true;
            }
            else
            {
                _altKeyCandidate = false;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            bool isCtrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) ||
                              Keyboard.IsKeyDown(Key.LeftCtrl) ||
                              Keyboard.IsKeyDown(Key.RightCtrl);

            if (_altKeyCandidate && !isCtrlDown &&
                ((e.Key == Key.System && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)) ||
                 e.Key == Key.LeftAlt || e.Key == Key.RightAlt))
            {
                _altKeyCandidate = false;
                ToggleMenuBar();
                e.Handled = true;
                return;
            }
            _altKeyCandidate = false;
        }

        private void MarkdownViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncingScroll || _activeTab?.ViewMode != ViewDisplayMode.Split) return;
            if (e.ExtentHeight <= e.ViewportHeight) return;

            try
            {
                _isSyncingScroll = true;
                double ratio = e.VerticalOffset / (e.ExtentHeight - e.ViewportHeight);
                double targetOffset = ratio * (RawMarkdownTextBox.ExtentHeight - RawMarkdownTextBox.ViewportHeight);
                RawMarkdownTextBox.ScrollToVerticalOffset(targetOffset);
            }
            finally
            {
                _isSyncingScroll = false;
            }
        }

        private void RawTextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncingScroll || _activeTab?.ViewMode != ViewDisplayMode.Split) return;
            if (e.ExtentHeight <= e.ViewportHeight) return;

            try
            {
                _isSyncingScroll = true;
                double ratio = e.VerticalOffset / (e.ExtentHeight - e.ViewportHeight);
                if (MarkdownViewer.ExtentHeight > MarkdownViewer.ViewportHeight)
                {
                    double targetOffset = ratio * (MarkdownViewer.ExtentHeight - MarkdownViewer.ViewportHeight);
                    MarkdownViewer.ScrollToVerticalOffset(targetOffset);
                }
            }
            finally
            {
                _isSyncingScroll = false;
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;
                var found = FindVisualChild<T>(child);
                if (found != null) return found;
            }
            return null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var tab in _tabs.ToList())
            {
                if (tab.IsDirty)
                {
                    SetActiveTab(tab);
                    string name = !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.FileName;
                    var result = MessageBox.Show(
                        $"Do you want to save changes to '{name}'?",
                        "MDPlus",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (!SaveTab(tab))
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

            _fileWatcher.Dispose();

            if (WindowState == WindowState.Maximized)
            {
                _settings.WindowMaximized = true;
                _settings.WindowWidth = RestoreBounds.Width;
                _settings.WindowHeight = RestoreBounds.Height;
            }
            else
            {
                _settings.WindowMaximized = false;
                _settings.WindowWidth = Width;
                _settings.WindowHeight = Height;
            }
            SaveSessionState();
            _settings.Save();
        }
    }
}
