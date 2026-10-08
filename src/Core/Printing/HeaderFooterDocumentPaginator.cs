using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MDPlus.Core.Printing
{
    /// <summary>
    /// Decorating DocumentPaginator that injects professional running headers,
    /// footers, divider rules, and page numbers onto each paginated page.
    /// </summary>
    public class HeaderFooterDocumentPaginator : DocumentPaginator
    {
        private readonly DocumentPaginator _inner;
        private readonly PrintSettings _settings;
        private readonly Typeface _typeface;
        private readonly Brush _textBrush;
        private readonly Pen _rulePen;
        private int _computedPageCount = 0;

        public HeaderFooterDocumentPaginator(DocumentPaginator inner, PrintSettings settings)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _typeface = new Typeface(new FontFamily("Segoe UI, Segoe UI Variable Text, Arial, sans-serif"),
                                     FontStyles.Normal,
                                     FontWeights.Normal,
                                     FontStretches.Normal);

            bool isDark = _settings.Theme == PrintThemeKind.CurrentTheme && ThemeManager.Instance.CurrentPalette.IsDark;
            _textBrush = new SolidColorBrush(isDark ? Color.FromRgb(139, 148, 158) : Color.FromRgb(87, 96, 106));
            _textBrush.Freeze();

            var ruleBrush = new SolidColorBrush(isDark ? Color.FromRgb(48, 54, 61) : Color.FromRgb(208, 215, 222));
            ruleBrush.Freeze();
            _rulePen = new Pen(ruleBrush, 0.75);
            _rulePen.Freeze();
        }

        public override bool IsPageCountValid => _inner.IsPageCountValid;

        public override int PageCount
        {
            get
            {
                if (_computedPageCount <= 0)
                {
                    _inner.ComputePageCount();
                    _computedPageCount = _inner.PageCount;
                }
                return _computedPageCount;
            }
        }

        public override Size PageSize
        {
            get => _inner.PageSize;
            set
            {
                _inner.PageSize = value;
                _inner.ComputePageCount();
                _computedPageCount = _inner.PageCount;
            }
        }

        public override IDocumentPaginatorSource Source => _inner.Source;

        public override void ComputePageCount()
        {
            _inner.ComputePageCount();
            _computedPageCount = _inner.PageCount;
        }

        public override void ComputePageCountAsync(object? userState)
        {
            _inner.ComputePageCountAsync(userState);
        }

        public override void CancelAsync(object userState)
        {
            _inner.CancelAsync(userState);
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            var basePage = _inner.GetPage(pageNumber);
            if (!_settings.IncludeHeadersAndFooters)
            {
                return basePage;
            }

            var container = new ContainerVisual();
            container.Children.Add(basePage.Visual);

            var headerFooterVisual = new DrawingVisual();
            using (var dc = headerFooterVisual.RenderOpen())
            {
                var padding = _settings.GetContentPadding();
                double left = padding.Left;
                double right = PageSize.Width - padding.Right;
                double contentWidth = right - left;

                if (contentWidth > 100)
                {
                    const double fontSize = 9.5;
                    const double pixelsPerDip = 96.0;

                    // 1. Header (Document Title & Date)
                    string title = !string.IsNullOrWhiteSpace(_settings.DocumentTitle)
                        ? _settings.DocumentTitle
                        : (!string.IsNullOrWhiteSpace(_settings.FilePath) ? Path.GetFileName(_settings.FilePath) : "MDPlus Document");

                    string dateStr = DateTime.Now.ToString("yyyy-MM-dd");

                    var headerLeftText = new FormattedText(
                        title,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeface,
                        fontSize,
                        _textBrush,
                        pixelsPerDip)
                    {
                        MaxTextWidth = Math.Max(50, contentWidth - 120),
                        MaxLineCount = 1,
                        Trimming = TextTrimming.CharacterEllipsis
                    };

                    var headerRightText = new FormattedText(
                        dateStr,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeface,
                        fontSize,
                        _textBrush,
                        pixelsPerDip);

                    double headerTextY = Math.Max(12, padding.Top - 30);
                    double headerRuleY = Math.Max(26, padding.Top - 12);

                    dc.DrawText(headerLeftText, new Point(left, headerTextY));
                    dc.DrawText(headerRightText, new Point(right - headerRightText.Width, headerTextY));
                    dc.DrawLine(_rulePen, new Point(left, headerRuleY), new Point(right, headerRuleY));

                    // 2. Footer (Source / App Name & Page X of Y)
                    string footerLabel = !string.IsNullOrWhiteSpace(_settings.FilePath)
                        ? Path.GetFileName(_settings.FilePath)
                        : "MDPlus";

                    string pageNumStr = string.Format(CultureInfo.CurrentCulture, "Page {0} of {1}", pageNumber + 1, Math.Max(1, PageCount));

                    var footerLeftText = new FormattedText(
                        footerLabel,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeface,
                        fontSize,
                        _textBrush,
                        pixelsPerDip)
                    {
                        MaxTextWidth = Math.Max(50, contentWidth - 120),
                        MaxLineCount = 1,
                        Trimming = TextTrimming.CharacterEllipsis
                    };

                    var footerRightText = new FormattedText(
                        pageNumStr,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeface,
                        fontSize,
                        _textBrush,
                        pixelsPerDip);

                    double footerRuleY = Math.Min(PageSize.Height - 26, PageSize.Height - padding.Bottom + 12);
                    double footerTextY = Math.Min(PageSize.Height - 20, PageSize.Height - padding.Bottom + 16);

                    dc.DrawLine(_rulePen, new Point(left, footerRuleY), new Point(right, footerRuleY));
                    dc.DrawText(footerLeftText, new Point(left, footerTextY));
                    dc.DrawText(footerRightText, new Point(right - footerRightText.Width, footerTextY));
                }
            }

            container.Children.Add(headerFooterVisual);
            return new DocumentPage(container, basePage.Size, basePage.BleedBox, basePage.ContentBox);
        }
    }
}
