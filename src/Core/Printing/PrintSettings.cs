using System;
using System.Globalization;
using System.Windows;

namespace MDPlus.Core.Printing
{
    public enum PaperSizeKind
    {
        Letter,
        A4,
        Legal
    }

    public enum PageOrientationKind
    {
        Portrait,
        Landscape
    }

    public enum MarginKind
    {
        Normal,
        Narrow,
        Wide
    }

    public enum PrintThemeKind
    {
        LightPaper,
        CurrentTheme
    }

    public class PrintSettings
    {
        public PaperSizeKind PaperSize { get; set; } = GetDefaultPaperSize();
        public PageOrientationKind Orientation { get; set; } = PageOrientationKind.Portrait;
        public MarginKind Margin { get; set; } = MarginKind.Normal;
        public PrintThemeKind Theme { get; set; } = PrintThemeKind.LightPaper;
        public bool IncludeHeadersAndFooters { get; set; } = true;

        public string DocumentTitle { get; set; } = "Markdown Document";
        public string FilePath { get; set; } = string.Empty;

        public static PaperSizeKind GetDefaultPaperSize()
        {
            var region = RegionInfo.CurrentRegion;
            // Metric countries default to A4, US/CA/MX default to Letter
            if (region != null && !region.IsMetric)
            {
                return PaperSizeKind.Letter;
            }
            return PaperSizeKind.A4;
        }

        public Size GetPhysicalPageSize()
        {
            double width;
            double height;

            switch (PaperSize)
            {
                case PaperSizeKind.A4:
                    // 210mm x 297mm @ 96 DPI: ~793.7 x 1122.5
                    width = 794;
                    height = 1123;
                    break;
                case PaperSizeKind.Legal:
                    // 8.5in x 14in @ 96 DPI: 816 x 1344
                    width = 816;
                    height = 1344;
                    break;
                case PaperSizeKind.Letter:
                default:
                    // 8.5in x 11in @ 96 DPI: 816 x 1056
                    width = 816;
                    height = 1056;
                    break;
            }

            if (Orientation == PageOrientationKind.Landscape)
            {
                return new Size(height, width);
            }

            return new Size(width, height);
        }

        public Thickness GetContentPadding()
        {
            double sideMargin;
            double topMargin;
            double bottomMargin;

            switch (Margin)
            {
                case MarginKind.Narrow:
                    sideMargin = 48; // 0.5 in
                    topMargin = IncludeHeadersAndFooters ? 54 : 48;
                    bottomMargin = IncludeHeadersAndFooters ? 54 : 48;
                    break;
                case MarginKind.Wide:
                    sideMargin = 96; // 1.0 in
                    topMargin = IncludeHeadersAndFooters ? 72 : 96;
                    bottomMargin = IncludeHeadersAndFooters ? 72 : 96;
                    break;
                case MarginKind.Normal:
                default:
                    sideMargin = 72; // 0.75 in
                    topMargin = IncludeHeadersAndFooters ? 60 : 72;
                    bottomMargin = IncludeHeadersAndFooters ? 60 : 72;
                    break;
            }

            return new Thickness(sideMargin, topMargin, sideMargin, bottomMargin);
        }
    }
}
