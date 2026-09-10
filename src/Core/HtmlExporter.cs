using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MDPlus.Core
{
    public class HtmlExporter
    {
        public static string ExportToFullHtml(MarkdownDocument doc, string title = "", bool isDark = false)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = string.IsNullOrEmpty(doc.Title) ? "Markdown Document" : doc.Title;
            }

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"UTF-8\">");
            sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"  <title>{WebUtility.HtmlEncode(title)}</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine(GetDefaultCss(isDark));
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <div class=\"markdown-body\">");

            sb.Append(ExportBodyHtml(doc));

            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        public static string ExportBodyHtml(MarkdownDocument doc)
        {
            var sb = new StringBuilder();

            foreach (var block in doc.Blocks)
            {
                ConvertBlockToHtml(block, sb);
            }

            return sb.ToString();
        }

        private static void ConvertBlockToHtml(MarkdownBlock block, StringBuilder sb)
        {
            switch (block)
            {
                case FrontmatterBlock fm:
                    sb.AppendLine("<div class=\"metadata-box\">");
                    sb.AppendLine("  <strong>Metadata</strong><br>");
                    foreach (var kvp in fm.Metadata)
                    {
                        sb.AppendLine($"  <span class=\"meta-key\">{WebUtility.HtmlEncode(kvp.Key)}:</span> {WebUtility.HtmlEncode(kvp.Value)}<br>");
                    }
                    sb.AppendLine("</div>");
                    break;

                case HeadingBlock heading:
                    sb.AppendLine($"<h{heading.Level} id=\"{WebUtility.HtmlEncode(heading.Anchor)}\">");
                    foreach (var inline in heading.Inlines)
                    {
                        ConvertInlineToHtml(inline, sb);
                    }
                    sb.AppendLine($"</h{heading.Level}>");
                    break;

                case ParagraphBlock para:
                    sb.Append("<p>");
                    foreach (var inline in para.Inlines)
                    {
                        ConvertInlineToHtml(inline, sb);
                    }
                    sb.AppendLine("</p>");
                    break;

                case BlockquoteBlock quote:
                    if (quote.Callout != CalloutType.None)
                    {
                        string calloutClass = quote.Callout.ToString().ToLowerInvariant();
                        sb.AppendLine($"<div class=\"callout callout-{calloutClass}\">");
                        sb.AppendLine($"  <div class=\"callout-title\">{WebUtility.HtmlEncode(quote.CalloutTitle)}</div>");
                        foreach (var child in quote.Blocks)
                        {
                            ConvertBlockToHtml(child, sb);
                        }
                        sb.AppendLine("</div>");
                    }
                    else
                    {
                        sb.AppendLine("<blockquote>");
                        foreach (var child in quote.Blocks)
                        {
                            ConvertBlockToHtml(child, sb);
                        }
                        sb.AppendLine("</blockquote>");
                    }
                    break;

                case CodeBlock code:
                    string lang = string.IsNullOrEmpty(code.Language) ? "text" : code.Language;
                    sb.AppendLine($"<pre><code class=\"language-{WebUtility.HtmlEncode(lang)}\">{WebUtility.HtmlEncode(code.Code)}</code></pre>");
                    break;

                case TableBlock table:
                    int colCount = Math.Max(table.Header.Cells.Count, table.Alignments.Count);
                    if (colCount == 0) colCount = 1;

                    sb.AppendLine("<table>");
                    sb.AppendLine("  <thead><tr>");
                    for (int i = 0; i < colCount; i++)
                    {
                        string align = GetAlignAttr(table.Alignments, i);
                        sb.Append($"    <th{align}>");
                        if (i < table.Header.Cells.Count)
                        {
                            foreach (var inline in table.Header.Cells[i].Inlines)
                            {
                                ConvertInlineToHtml(inline, sb);
                            }
                        }
                        sb.AppendLine("</th>");
                    }
                    sb.AppendLine("  </tr></thead>");
                    if (table.Rows.Count > 0)
                    {
                        sb.AppendLine("  <tbody>");
                        foreach (var row in table.Rows)
                        {
                            sb.AppendLine("    <tr>");
                            for (int i = 0; i < colCount; i++)
                            {
                                string align = GetAlignAttr(table.Alignments, i);
                                sb.Append($"      <td{align}>");
                                if (i < row.Cells.Count)
                                {
                                    foreach (var inline in row.Cells[i].Inlines)
                                    {
                                        ConvertInlineToHtml(inline, sb);
                                    }
                                }
                                sb.AppendLine("</td>");
                            }
                            sb.AppendLine("    </tr>");
                        }
                        sb.AppendLine("  </tbody>");
                    }
                    sb.AppendLine("</table>");
                    break;

                case ListBlock list:
                    string tag = list.IsOrdered ? "ol" : "ul";
                    string startAttr = list.IsOrdered && list.StartNumber > 1 ? $" start=\"{list.StartNumber}\"" : "";
                    sb.AppendLine($"<{tag}{startAttr}>");
                    foreach (var item in list.Items)
                    {
                        if (item.IsTask)
                        {
                            string checkAttr = item.IsChecked ? " checked" : "";
                            sb.Append($"  <li class=\"task-item\"><input type=\"checkbox\" disabled{checkAttr}> ");
                        }
                        else
                        {
                            sb.Append("  <li>");
                        }
                        foreach (var inline in item.Inlines)
                        {
                            ConvertInlineToHtml(inline, sb);
                        }
                        foreach (var child in item.Blocks)
                        {
                            ConvertBlockToHtml(child, sb);
                        }
                        sb.AppendLine("</li>");
                    }
                    sb.AppendLine($"</{tag}>");
                    break;

                case ThematicBreakBlock _:
                    sb.AppendLine("<hr>");
                    break;

                case MathBlock math:
                    sb.AppendLine($"<div class=\"math-display\">\\[{WebUtility.HtmlEncode(math.Expression)}\\]</div>");
                    break;

                case HtmlBlock html:
                    sb.AppendLine(html.RawHtml);
                    break;
            }
        }

        private static string GetAlignAttr(List<ColumnAlignment> aligns, int index)
        {
            if (index >= aligns.Count) return "";
            return aligns[index] switch
            {
                ColumnAlignment.Center => " style=\"text-align: center;\"",
                ColumnAlignment.Right => " style=\"text-align: right;\"",
                _ => " style=\"text-align: left;\""
            };
        }

        private static void ConvertInlineToHtml(MarkdownInline inline, StringBuilder sb)
        {
            switch (inline)
            {
                case TextInline text:
                    sb.Append(WebUtility.HtmlEncode(text.Text));
                    break;

                case BoldInline bold:
                    sb.Append("<strong>");
                    foreach (var c in bold.Children) ConvertInlineToHtml(c, sb);
                    sb.Append("</strong>");
                    break;

                case ItalicInline italic:
                    sb.Append("<em>");
                    foreach (var c in italic.Children) ConvertInlineToHtml(c, sb);
                    sb.Append("</em>");
                    break;

                case BoldItalicInline bi:
                    sb.Append("<strong><em>");
                    foreach (var c in bi.Children) ConvertInlineToHtml(c, sb);
                    sb.Append("</em></strong>");
                    break;

                case StrikethroughInline strike:
                    sb.Append("<del>");
                    foreach (var c in strike.Children) ConvertInlineToHtml(c, sb);
                    sb.Append("</del>");
                    break;

                case HighlightInline hl:
                    sb.Append("<mark>");
                    foreach (var c in hl.Children) ConvertInlineToHtml(c, sb);
                    sb.Append("</mark>");
                    break;

                case CodeInline code:
                    sb.Append($"<code>{WebUtility.HtmlEncode(code.Code)}</code>");
                    break;

                case LinkInline link:
                    string safeHref = SanitizeUrl(link.Url);
                    sb.Append($"<a href=\"{WebUtility.HtmlEncode(safeHref)}\"");
                    if (!string.IsNullOrEmpty(link.Title)) sb.Append($" title=\"{WebUtility.HtmlEncode(link.Title)}\"");
                    sb.Append(">");
                    foreach (var c in link.Children) ConvertInlineToHtml(c, sb);
                    sb.Append("</a>");
                    break;

                case ImageInline img:
                    string safeSrc = SanitizeUrl(img.Url);
                    sb.Append($"<img src=\"{WebUtility.HtmlEncode(safeSrc)}\" alt=\"{WebUtility.HtmlEncode(img.AltText)}\"");
                    if (!string.IsNullOrEmpty(img.Title)) sb.Append($" title=\"{WebUtility.HtmlEncode(img.Title)}\"");
                    sb.Append(">");
                    break;

                case LineBreakInline br:
                    sb.Append(br.IsHard ? "<br>\n" : " ");
                    break;

                case MathInline math:
                    if (math.IsDisplay)
                    {
                        sb.Append($"<span class=\"math-display\">\\[{WebUtility.HtmlEncode(math.Expression)}\\]</span>");
                    }
                    else
                    {
                        sb.Append($"<span class=\"math-inline\">\\({WebUtility.HtmlEncode(math.Expression)}\\)</span>");
                    }
                    break;

                case HtmlInline html:
                    sb.Append(html.RawHtml);
                    break;
            }
        }

        private static string SanitizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            string trimmed = url.Trim();
            if (trimmed.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("data:text/html", StringComparison.OrdinalIgnoreCase))
            {
                return "#";
            }
            return trimmed;
        }

        private static string GetDefaultCss(bool isDark)
        {
            if (isDark)
            {
                return @"
body {
    background-color: #0d1117;
    color: #c9d1d9;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
    font-size: 16px;
    line-height: 1.6;
    margin: 0;
    padding: 30px;
}
.markdown-body {
    max-width: 900px;
    margin: 0 auto;
}
h1, h2, h3, h4, h5, h6 {
    color: #f0f6fc;
    margin-top: 24px;
    margin-bottom: 16px;
    font-weight: 600;
    line-height: 1.25;
}
h1 { font-size: 2em; border-bottom: 1px solid #21262d; padding-bottom: .3em; }
h2 { font-size: 1.5em; border-bottom: 1px solid #21262d; padding-bottom: .3em; }
h3 { font-size: 1.25em; }
a { color: #58a6ff; text-decoration: none; }
a:hover { text-decoration: underline; }
pre {
    background-color: #161b22;
    border: 1px solid #30363d;
    border-radius: 6px;
    padding: 16px;
    overflow: auto;
    font-family: 'Cascadia Code', Consolas, monospace;
    font-size: 14px;
}
code {
    background-color: rgba(110, 118, 129, 0.4);
    padding: .2em .4em;
    border-radius: 4px;
    font-family: 'Cascadia Code', Consolas, monospace;
    font-size: 85%;
}
pre code { background-color: transparent; padding: 0; }
blockquote {
    border-left: .25em solid #3b434b;
    color: #8b949e;
    padding: 0 1em;
    margin: 0 0 16px 0;
}
table {
    border-collapse: collapse;
    width: 100%;
    margin-bottom: 16px;
}
th, td {
    border: 1px solid #30363d;
    padding: 6px 13px;
}
th { background-color: #161b22; font-weight: 600; }
tr:nth-child(2n) { background-color: #161b22; }
hr {
    height: .25em;
    background-color: #30363d;
    border: 0;
    margin: 24px 0;
}
.task-item { list-style: none; }
.callout {
    border-left: 4px solid #58a6ff;
    background-color: rgba(56, 139, 253, 0.15);
    padding: 12px 16px;
    margin: 16px 0;
    border-radius: 0 6px 6px 0;
}
.callout-title { font-weight: bold; margin-bottom: 6px; }
.callout-tip { border-color: #3fb950; background-color: rgba(46, 160, 67, 0.15); }
.callout-warning { border-color: #d29922; background-color: rgba(187, 128, 9, 0.15); }
.callout-caution { border-color: #f85149; background-color: rgba(248, 81, 73, 0.15); }
.callout-important { border-color: #a371f7; background-color: rgba(163, 113, 247, 0.15); }
.metadata-box {
    border: 1px solid #30363d;
    background-color: #161b22;
    padding: 12px;
    border-radius: 6px;
    margin-bottom: 20px;
    font-size: 13px;
}
.meta-key { color: #58a6ff; font-weight: 600; }
.math-display { text-align: center; margin: 16px 0; overflow-x: auto; font-family: 'Cambria Math', 'Times New Roman', serif; }
.math-inline { font-family: 'Cambria Math', 'Times New Roman', serif; }
kbd { background-color: #21262d; border: 1px solid #444c56; border-bottom-width: 2px; border-radius: 4px; padding: 2px 6px; font-family: 'Cascadia Code', Consolas, monospace; font-size: 85%; }
mark { background-color: rgba(255, 230, 0, 0.3); color: #f0f6fc; padding: 1px 4px; border-radius: 2px; }
details { border: 1px solid #30363d; border-radius: 6px; padding: 12px; margin: 16px 0; background-color: rgba(255, 255, 255, 0.04); }
details summary { font-weight: 600; cursor: pointer; }
";
            }
            else
            {
                return @"
body {
    background-color: #ffffff;
    color: #24292f;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
    font-size: 16px;
    line-height: 1.6;
    margin: 0;
    padding: 30px;
}
.markdown-body {
    max-width: 900px;
    margin: 0 auto;
}
h1, h2, h3, h4, h5, h6 {
    color: #1f2328;
    margin-top: 24px;
    margin-bottom: 16px;
    font-weight: 600;
    line-height: 1.25;
}
h1 { font-size: 2em; border-bottom: 1px solid #d8dee4; padding-bottom: .3em; }
h2 { font-size: 1.5em; border-bottom: 1px solid #d8dee4; padding-bottom: .3em; }
h3 { font-size: 1.25em; }
a { color: #0969da; text-decoration: none; }
a:hover { text-decoration: underline; }
pre {
    background-color: #f6f8fa;
    border: 1px solid #d0d7de;
    border-radius: 6px;
    padding: 16px;
    overflow: auto;
    font-family: 'Cascadia Code', Consolas, monospace;
    font-size: 14px;
}
code {
    background-color: rgba(175, 184, 193, 0.2);
    padding: .2em .4em;
    border-radius: 4px;
    font-family: 'Cascadia Code', Consolas, monospace;
    font-size: 85%;
}
pre code { background-color: transparent; padding: 0; }
blockquote {
    border-left: .25em solid #d0d7de;
    color: #656d76;
    padding: 0 1em;
    margin: 0 0 16px 0;
}
table {
    border-collapse: collapse;
    width: 100%;
    margin-bottom: 16px;
}
th, td {
    border: 1px solid #d0d7de;
    padding: 6px 13px;
}
th { background-color: #f6f8fa; font-weight: 600; }
tr:nth-child(2n) { background-color: #f6f8fa; }
hr {
    height: .25em;
    background-color: #d0d7de;
    border: 0;
    margin: 24px 0;
}
.task-item { list-style: none; }
.callout {
    border-left: 4px solid #0969da;
    background-color: #ddf4ff;
    padding: 12px 16px;
    margin: 16px 0;
    border-radius: 0 6px 6px 0;
}
.callout-title { font-weight: bold; margin-bottom: 6px; color: #0969da; }
.callout-tip { border-color: #1a7f37; background-color: #dafbe1; }
.callout-tip .callout-title { color: #1a7f37; }
.callout-warning { border-color: #9a6700; background-color: #fff8c5; }
.callout-warning .callout-title { color: #9a6700; }
.callout-caution { border-color: #cf222e; background-color: #ffebe9; }
.callout-caution .callout-title { color: #cf222e; }
.callout-important { border-color: #8250df; background-color: #fbefff; }
.callout-important .callout-title { color: #8250df; }
.metadata-box {
    border: 1px solid #d0d7de;
    background-color: #f6f8fa;
    padding: 12px;
    border-radius: 6px;
    margin-bottom: 20px;
    font-size: 13px;
}
.meta-key { color: #0969da; font-weight: 600; }
.math-display { text-align: center; margin: 16px 0; overflow-x: auto; font-family: 'Cambria Math', 'Times New Roman', serif; }
.math-inline { font-family: 'Cambria Math', 'Times New Roman', serif; }
kbd { background-color: #f3f4f6; border: 1px solid #d1d5db; border-bottom-width: 2px; border-radius: 4px; padding: 2px 6px; font-family: 'Cascadia Code', Consolas, monospace; font-size: 85%; }
mark { background-color: #fff8c5; color: #24292f; padding: 1px 4px; border-radius: 2px; }
details { border: 1px solid #d0d7de; border-radius: 6px; padding: 12px; margin: 16px 0; background-color: rgba(0, 0, 0, 0.02); }
details summary { font-weight: 600; cursor: pointer; }
";
            }
        }
    }
}
