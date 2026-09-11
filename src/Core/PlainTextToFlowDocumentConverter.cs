using System;
using System.Collections.Generic;
using System.Text;
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
    /// Also serializes edited FlowDocuments back into lossless plain text.
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
                    Margin = new Thickness(0),
                    Tag = "EmptyPlaceholder"
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

        /// <summary>
        /// Serializes a plain text, log, or config FlowDocument back into lossless plain text.
        /// Preserves edited content, paragraph splits, and line breaks with complete fidelity.
        /// </summary>
        public static string Serialize(FlowDocument? doc, DocumentFormat format = DocumentFormat.PlainText, string? lineEnding = null)
        {
            if (doc == null || doc.Blocks == null || doc.Blocks.Count == 0)
                return string.Empty;

            string eol = lineEnding == "LF" ? "\n" : (lineEnding == "CRLF" ? "\r\n" : Environment.NewLine);

            var paragraphs = new List<string>();
            foreach (var block in doc.Blocks)
            {
                if (block is Paragraph p)
                {
                    if (p.Tag is string tag && tag == "EmptyPlaceholder")
                        continue;

                    if (p.FontStyle == FontStyles.Italic && p.Inlines.FirstInline is Run firstRun &&
                        firstRun.Text.StartsWith("Empty ", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var sb = new StringBuilder();
                    ExtractInlinesText(p.Inlines, sb);
                    paragraphs.Add(sb.ToString());
                }
            }

            if (paragraphs.Count == 0)
                return string.Empty;

            if (paragraphs.Count == 1)
                return paragraphs[0];

            var result = new StringBuilder();
            for (int i = 0; i < paragraphs.Count; i++)
            {
                if (i > 0)
                {
                    string prev = paragraphs[i - 1];
                    if (!prev.EndsWith("\r\n") && !prev.EndsWith("\n") && !prev.EndsWith("\r"))
                    {
                        result.Append(eol);
                    }
                }
                result.Append(paragraphs[i]);
            }

            return result.ToString();
        }

        private static void ExtractInlinesText(InlineCollection inlines, StringBuilder sb)
        {
            for (Inline? cur = inlines.FirstInline; cur != null; cur = cur.NextInline)
            {
                switch (cur)
                {
                    case Run r:
                        sb.Append(r.Text);
                        break;
                    case LineBreak:
                        sb.Append('\n');
                        break;
                    case Span s:
                        ExtractInlinesText(s.Inlines, sb);
                        break;
                }
            }
        }
    }
}
