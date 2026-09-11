using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;

namespace MDPlus.Core
{
    /// <summary>
    /// Converts parsed CSV/TSV tabular data into a high-legibility WPF FlowDocument Table.
    /// Features semi-bold headers, crisp grid borders, subtle alternating row stripes,
    /// automatic numeric right-alignment, and theme-adaptive styling.
    /// </summary>
    public static class CsvToFlowDocumentConverter
    {
        private const int MaxVisualRows = 3000;

        public static FlowDocument Convert(string text, ThemePalette palette, bool isTsv = false)
        {
            var data = CsvParser.Parse(text, isTsv ? '\t' : ',');
            return Convert(data, palette, isTsv);
        }

        public static FlowDocument Convert(List<List<string>>? data, ThemePalette palette, bool isTsv = false)
        {
            var doc = new FlowDocument
            {
                Background = palette.EditorBg,
                Foreground = palette.EditorFg,
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, -apple-system, sans-serif"),
                FontSize = 14,
                PagePadding = new Thickness(32, 24, 32, 32),
                ColumnWidth = double.PositiveInfinity
            };

            if (data == null || data.Count == 0)
            {
                var emptyParagraph = new Paragraph(new Run(isTsv ? "Empty TSV document" : "Empty CSV document"))
                {
                    FontStyle = FontStyles.Italic,
                    Foreground = palette.MutedFg,
                    Margin = new Thickness(0)
                };
                doc.Blocks.Add(emptyParagraph);
                return doc;
            }

            // Determine column count
            int colCount = 0;
            foreach (var row in data)
            {
                if (row.Count > colCount) colCount = row.Count;
            }

            if (colCount == 0)
            {
                return doc;
            }

            // Determine column alignments
            var alignments = DetermineColumnAlignments(data, colCount);

            var table = new WpfTable
            {
                CellSpacing = 0,
                Margin = new Thickness(0, 8, 0, 18),
                BorderBrush = palette.TableBorder,
                BorderThickness = new Thickness(1)
            };

            for (int i = 0; i < colCount; i++)
            {
                table.Columns.Add(new TableColumn());
            }

            // 1. Header Row Group
            var headerGroup = new WpfTableRowGroup();
            var headerRow = new WpfTableRow
            {
                Background = palette.TableHeaderBg
            };

            var firstRow = data[0];
            for (int i = 0; i < colCount; i++)
            {
                string headerText = i < firstRow.Count ? firstRow[i] : string.Empty;
                var p = new Paragraph(new Run(headerText))
                {
                    FontWeight = FontWeights.SemiBold,
                    Foreground = palette.HeadingFg,
                    TextAlignment = alignments[i],
                    Margin = new Thickness(0)
                };

                var cell = new WpfTableCell(p)
                {
                    Padding = new Thickness(12, 10, 12, 10),
                    BorderBrush = palette.TableBorder,
                    BorderThickness = new Thickness(0, 0, i == colCount - 1 ? 0 : 1, 2)
                };
                headerRow.Cells.Add(cell);
            }

            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            // 2. Data Rows Group
            int rowLimit = Math.Min(data.Count, MaxVisualRows + 1); // +1 for header
            if (data.Count > 1)
            {
                var bodyGroup = new WpfTableRowGroup();
                for (int r = 1; r < rowLimit; r++)
                {
                    var dataRow = data[r];
                    var wpfRow = new WpfTableRow
                    {
                        Background = ((r - 1) % 2 == 1) ? palette.TableAltRowBg : Brushes.Transparent
                    };

                    for (int i = 0; i < colCount; i++)
                    {
                        string cellText = i < dataRow.Count ? dataRow[i] : string.Empty;
                        var p = new Paragraph(new Run(cellText))
                        {
                            Foreground = palette.EditorFg,
                            TextAlignment = alignments[i],
                            Margin = new Thickness(0)
                        };

                        var cell = new WpfTableCell(p)
                        {
                            Padding = new Thickness(12, 8, 12, 8),
                            BorderBrush = palette.TableBorder,
                            BorderThickness = new Thickness(0, 0, i == colCount - 1 ? 0 : 1, 1)
                        };
                        wpfRow.Cells.Add(cell);
                    }

                    bodyGroup.Rows.Add(wpfRow);
                }
                table.RowGroups.Add(bodyGroup);
            }

            doc.Blocks.Add(table);

            if (data.Count > MaxVisualRows + 1)
            {
                var noticePara = new Paragraph(new Run($"Showing first {MaxVisualRows:N0} of {data.Count - 1:N0} records. Switch to Raw view (Ctrl+3) to view or edit full stream."))
                {
                    FontStyle = FontStyles.Italic,
                    Foreground = palette.MutedFg,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                doc.Blocks.Add(noticePara);
            }

            return doc;
        }

        private static TextAlignment[] DetermineColumnAlignments(List<List<string>> data, int colCount)
        {
            var alignments = new TextAlignment[colCount];

            for (int col = 0; col < colCount; col++)
            {
                int numericCount = 0;
                int totalNonEmpty = 0;

                // Examine data rows (skipping header at row 0)
                for (int r = 1; r < data.Count && r < 100; r++)
                {
                    var row = data[r];
                    if (col < row.Count)
                    {
                        string val = row[col].Trim();
                        if (!string.IsNullOrEmpty(val))
                        {
                            totalNonEmpty++;
                            if (IsNumeric(val))
                            {
                                numericCount++;
                            }
                        }
                    }
                }

                // If majority of non-empty values are numeric, align right
                if (totalNonEmpty > 0 && ((double)numericCount / totalNonEmpty) >= 0.75)
                {
                    alignments[col] = TextAlignment.Right;
                }
                else
                {
                    alignments[col] = TextAlignment.Left;
                }
            }

            return alignments;
        }

        private static bool IsNumeric(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            // Strip common currency symbols and percent signs
            string clean = value.Trim('$', '€', '£', '¥', '%', ' ', '\t');

            // Handle accounting parentheses: (123.45)
            if (clean.StartsWith("(") && clean.EndsWith(")"))
            {
                clean = "-" + clean.Substring(1, clean.Length - 2);
            }

            return double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                || double.TryParse(clean, NumberStyles.Any, CultureInfo.CurrentCulture, out _);
        }
    }
}
