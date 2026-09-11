using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;

namespace MDPlus.Core
{
    /// <summary>
    /// Serializes a WPF FlowDocument Table back into RFC 4180 compliant CSV or TSV text.
    /// Preserves quoted fields, escaped double quotes (""), and embedded newlines
    /// ensuring lossless round-trip fidelity between Rendered and Raw views.
    /// </summary>
    public static class CsvSerializer
    {
        public static string Serialize(FlowDocument? document, char delimiter = ',')
        {
            if (document == null || document.Blocks == null || document.Blocks.Count == 0)
                return string.Empty;

            WpfTable? table = FindTable(document);
            if (table == null)
            {
                // Fallback: extract plain text from all paragraphs
                return ExtractPlainText(document);
            }

            var sb = new StringBuilder();
            bool firstRow = true;

            foreach (var group in table.RowGroups)
            {
                foreach (var row in group.Rows)
                {
                    if (!firstRow)
                    {
                        sb.Append("\r\n");
                    }
                    firstRow = false;

                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(delimiter);
                        }

                        string cellText = GetCellText(row.Cells[i]);
                        sb.Append(EscapeField(cellText, delimiter));
                    }
                }
            }

            if (sb.Length > 0)
            {
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        public static string EscapeField(string text, char delimiter)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            bool mustQuote = text.IndexOf(delimiter) >= 0 ||
                             text.IndexOf('"') >= 0 ||
                             text.IndexOf('\r') >= 0 ||
                             text.IndexOf('\n') >= 0;

            if (mustQuote)
            {
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }

            return text;
        }

        private static WpfTable? FindTable(FlowDocument document)
        {
            foreach (var block in document.Blocks)
            {
                if (block is WpfTable t) return t;
                if (block is Section s)
                {
                    var inner = FindTableInSection(s);
                    if (inner != null) return inner;
                }
            }
            return null;
        }

        private static WpfTable? FindTableInSection(Section section)
        {
            foreach (var block in section.Blocks)
            {
                if (block is WpfTable t) return t;
                if (block is Section s)
                {
                    var inner = FindTableInSection(s);
                    if (inner != null) return inner;
                }
            }
            return null;
        }

        private static string GetCellText(WpfTableCell cell)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var block in cell.Blocks)
            {
                if (!first) sb.Append('\n');
                first = false;

                if (block is Paragraph p)
                {
                    ExtractInlinesText(p.Inlines, sb);
                }
            }
            return sb.ToString();
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

        private static string ExtractPlainText(FlowDocument document)
        {
            var sb = new StringBuilder();
            foreach (var block in document.Blocks)
            {
                if (block is Paragraph p)
                {
                    ExtractInlinesText(p.Inlines, sb);
                    sb.Append("\r\n");
                }
            }
            return sb.ToString();
        }
    }
}
