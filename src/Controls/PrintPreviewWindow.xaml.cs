using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using MDPlus.Core.Printing;
using MDPlus.Models;

namespace MDPlus.Controls
{
    public partial class PrintPreviewWindow : Window
    {
        private readonly DocumentTabItem _tab;
        private readonly string? _activeEditorText;
        private readonly PrintSettings _settings = new PrintSettings();
        private PrintPreviewDocumentHandle? _handle;
        private bool _isInitializing = true;

        public PrintPreviewWindow(DocumentTabItem tab, string? activeEditorText = null)
        {
            InitializeComponent();

            _tab = tab ?? throw new ArgumentNullException(nameof(tab));
            _activeEditorText = activeEditorText;

            _settings.DocumentTitle = !string.IsNullOrWhiteSpace(_tab.Title) ? _tab.Title : "Markdown Document";
            _settings.FilePath = _tab.FilePath;

            Title = $"Print Preview - {(!string.IsNullOrEmpty(_tab.FileName) ? _tab.FileName : "Untitled")} - MDPlus";

            Loaded += PrintPreviewWindow_Loaded;
        }

        private void PrintPreviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize combo selections to match detected defaults
            PaperSizeComboBox.SelectedIndex = (int)_settings.PaperSize;
            OrientationComboBox.SelectedIndex = (int)_settings.Orientation;
            MarginComboBox.SelectedIndex = (int)_settings.Margin;
            ThemeComboBox.SelectedIndex = (int)_settings.Theme;
            HeadersFootersCheckBox.IsChecked = _settings.IncludeHeadersAndFooters;

            _isInitializing = false;
            RefreshPreview();

            // Set initial pleasant zoom
            Dispatcher.BeginInvoke(new Action(() =>
            {
                PreviewDocumentViewer.FitToWidth();
                UpdateZoomText();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void RefreshPreview()
        {
            if (_isInitializing) return;

            // Sync settings from controls
            _settings.PaperSize = (PaperSizeKind)Math.Max(0, PaperSizeComboBox.SelectedIndex);
            _settings.Orientation = (PageOrientationKind)Math.Max(0, OrientationComboBox.SelectedIndex);
            _settings.Margin = (MarginKind)Math.Max(0, MarginComboBox.SelectedIndex);
            _settings.Theme = (PrintThemeKind)Math.Max(0, ThemeComboBox.SelectedIndex);
            _settings.IncludeHeadersAndFooters = HeadersFootersCheckBox.IsChecked == true;

            try
            {
                _handle?.Dispose();

                var flowDoc = PrintDocumentBuilder.BuildPrintFlowDocument(_tab, _settings, _activeEditorText);
                _handle = PrintDocumentBuilder.CreateInmemoryXpsDocument(flowDoc, _settings);

                PreviewDocumentViewer.Document = _handle.DocumentSequence;

                int pageCount = Math.Max(1, _handle.PageCount);
                DocumentInfoText.Text = $"{_settings.PaperSize} • {_settings.Orientation} • {pageCount} Page{(pageCount == 1 ? "" : "s")}";
                TotalPagesTextBlock.Text = $"of {pageCount}";
                CurrentPageTextBox.Text = PreviewDocumentViewer.MasterPageNumber.ToString();

                UpdateNavigationButtons();
                UpdateZoomText();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to render print preview: {ex.Message}", "Print Preview Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SettingsChanged_Handler(object sender, RoutedEventArgs e)
        {
            RefreshPreview();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_handle == null) return;

            var printDlg = new PrintDialog
            {
                UserPageRangeEnabled = true,
                MinPage = 1,
                MaxPage = (uint)Math.Max(1, _handle.PageCount)
            };

            if (printDlg.ShowDialog() == true)
            {
                DocumentPaginator paginatorToPrint;
                if (printDlg.PageRangeSelection == PageRangeSelection.UserPages && printDlg.PageRange.PageFrom <= printDlg.PageRange.PageTo)
                {
                    paginatorToPrint = new PageRangeDocumentPaginator(_handle.DocumentSequence.DocumentPaginator, printDlg.PageRange);
                }
                else
                {
                    paginatorToPrint = _handle.DocumentSequence.DocumentPaginator;
                }

                printDlg.PrintDocument(paginatorToPrint, _settings.DocumentTitle);
                Close();
            }
        }

        #region Navigation & Zoom Handlers

        private void FirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.FirstPage();
            UpdateNavigationButtons();
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.PreviousPage();
            UpdateNavigationButtons();
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.NextPage();
            UpdateNavigationButtons();
        }

        private void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.LastPage();
            UpdateNavigationButtons();
        }

        private void CurrentPageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyCurrentPageInput();
                e.Handled = true;
            }
        }

        private void CurrentPageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyCurrentPageInput();
        }

        private void ApplyCurrentPageInput()
        {
            if (int.TryParse(CurrentPageTextBox.Text, out int page) && _handle != null)
            {
                int target = Math.Clamp(page, 1, Math.Max(1, _handle.PageCount));
                PreviewDocumentViewer.GoToPage(target);
                CurrentPageTextBox.Text = target.ToString();
                UpdateNavigationButtons();
            }
            else
            {
                CurrentPageTextBox.Text = PreviewDocumentViewer.MasterPageNumber.ToString();
            }
        }

        private void UpdateNavigationButtons()
        {
            if (_handle == null) return;

            int current = PreviewDocumentViewer.MasterPageNumber;
            int total = Math.Max(1, _handle.PageCount);

            CurrentPageTextBox.Text = current.ToString();
            FirstPageButton.IsEnabled = current > 1;
            PreviousPageButton.IsEnabled = current > 1;
            NextPageButton.IsEnabled = current < total;
            LastPageButton.IsEnabled = current < total;
        }

        private void FitWidthButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.FitToWidth();
            UpdateZoomText();
        }

        private void FitPageButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.FitToHeight();
            UpdateZoomText();
        }

        private void TwoPagesButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.FitToMaxPagesAcross(2);
            UpdateZoomText();
        }

        private void Zoom100Button_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.Zoom = 100.0;
            UpdateZoomText();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.IncreaseZoom();
            UpdateZoomText();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewDocumentViewer.DecreaseZoom();
            UpdateZoomText();
        }

        private void UpdateZoomText()
        {
            ZoomLevelTextBlock.Text = $"{Math.Round(PreviewDocumentViewer.Zoom)}%";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Keyboard & Cleanup

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PrintButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp || (e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Alt))
            {
                PreviousPageButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown || (e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Alt))
            {
                NextPageButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Home && Keyboard.Modifiers == ModifierKeys.Control)
            {
                FirstPageButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.End && Keyboard.Modifiers == ModifierKeys.Control)
            {
                LastPageButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ZoomInButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.OemMinus && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ZoomOutButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.D0 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Zoom100Button_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _handle?.Dispose();
            _handle = null;
        }

        #endregion
    }
}
