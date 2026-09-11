using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MDPlus.Models;

namespace MDPlus.Core
{
    /// <summary>
    /// Converts plain text and log documents into crisp WPF FlowDocuments.
    /// Uses Cascadia Code / Consolas for logs and config files, and Segoe UI for plain text,
    /// enabling smooth font scaling, wrapping, and instant in-page find navigation.
    /// </summary>
    public static class PlainTextToFlowDocumentConverter
    {
        public static FlowDocument Convert(string? text, DocumentFormat format, ThemePalette palette)
        {
            bool isMonospace = format is DocumentFormat.Log or DocumentFormat.Ini or DocumentFormat.Cfg or DocumentFormat.Yaml or DocumentFormat.Xml or DocumentFormat.Json;

            var fontFamily = isMonospace
                ? new FontFamily("Cascadia Code, Consolas, Courier New, monospace")
                : new FontFamily("Segoe UI Variable Text, Segoe UI, -apple-system, sans-serif");

            double fontSize = isMonospace ? 13 : 14;
            double lineHeight = isMonospace ? 20 : 22;

            var doc = new FlowDocument
            {
                Background = palette.EditorBg,
                Foreground = palette.EditorFg,
                FontFamily = fontFamily,
                FontSize = fontSize,
                PagePadding = new Thickness(32, 24, 32, 32),
                ColumnWidth = double.PositiveInfinity
            };

            if (string.IsNullOrEmpty(text))
            {
                var emptyPara = new Paragraph(new Run($"Empty {DocumentFormatHelper.GetFormatDisplayName(format)}"))
                {
                    FontStyle = FontStyles.Italic,
                    Foreground = palette.MutedFg,
                    Margin = new Thickness(0)
                };
                doc.Blocks.Add(emptyPara);
                return doc;
            }

            var p = new Paragraph(new Run(text))
            {
                FontFamily = fontFamily,
                FontSize = fontSize,
                LineHeight = lineHeight,
                Foreground = palette.EditorFg,
                Margin = new Thickness(0)
            };

            doc.Blocks.Add(p);
            return doc;
        }
    }
}
