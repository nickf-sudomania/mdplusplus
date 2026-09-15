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
        public static FlowDocument Convert(string text, ThemePalette palette, bool isTsv = false, int totalRecords = -1)
        {
            var data = CsvParser.Parse(text, isTsv ? '\t' : ',', 0);
            int totalLines = totalRecords >= 0 ? totalRecords : CountLines(text);
            return Convert(data, palette, isTsv, totalLines);
        }

        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int count = 1;
            for (int i = 0; i < text.Length; i++)
                if (text[i] == '\n') count++;
            return count;
        }

        public static FlowDocument Convert(List<List<string>>? data, ThemePalette palette, bool isTsv = false, int totalRecords = -1)
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
                    Margin = new Thickness(0),
                    Tag = "EmptyPlaceholder"
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

            if (data.Count <= 100)
            {
                var table = new WpfTable
                {
                    CellSpacing = 0,
                    Margin = new Thickness(0, 8, 0, 18),
                    BorderBrush = palette.TableBorder,
                    BorderThickness = new Thickness(1)
                };
                for (int i = 0; i < colCount; i++) table.Columns.Add(new TableColumn());

                var headerGroup = new WpfTableRowGroup();
                var headerRow = new WpfTableRow { Background = palette.TableHeaderBg };
                var firstRow = data[0];
                for (int i = 0; i < colCount; i++)
                {
                    string headerText = i < firstRow.Count ? firstRow[i] : string.Empty;
                    var p = new Paragraph(new Run(headerText)) { FontWeight = FontWeights.SemiBold, Foreground = palette.HeadingFg, TextAlignment = alignments[i], Margin = new Thickness(0) };
                    headerRow.Cells.Add(new WpfTableCell(p) { Padding = new Thickness(12, 10, 12, 10), BorderBrush = palette.TableBorder, BorderThickness = new Thickness(0, 0, i == colCount - 1 ? 0 : 1, 2) });
                }
                headerGroup.Rows.Add(headerRow);
                table.RowGroups.Add(headerGroup);

                if (data.Count > 1)
                {
                    var bodyGroup = new WpfTableRowGroup();
                    for (int r = 1; r < data.Count; r++)
                    {
                        var dataRow = data[r];
                        var wpfRow = new WpfTableRow { Background = ((r - 1) % 2 == 1) ? palette.TableAltRowBg : Brushes.Transparent };
                        for (int i = 0; i < colCount; i++)
                        {
                            string cellText = i < dataRow.Count ? dataRow[i] : string.Empty;
                            var p = new Paragraph(new Run(cellText)) { Foreground = palette.EditorFg, TextAlignment = alignments[i], Margin = new Thickness(0) };
                            wpfRow.Cells.Add(new WpfTableCell(p) { Padding = new Thickness(12, 8, 12, 8), BorderBrush = palette.TableBorder, BorderThickness = new Thickness(0, 0, i == colCount - 1 ? 0 : 1, 1) });
                        }
                        bodyGroup.Rows.Add(wpfRow);
                    }
                    table.RowGroups.Add(bodyGroup);
                }
                doc.Blocks.Add(table);
            }
            else
            {
                var gridView = new System.Windows.Controls.GridView();
                var firstRow = data[0];
                for (int i = 0; i < colCount; i++)
                {
                    string headerText = i < firstRow.Count ? firstRow[i] : string.Empty;
                    gridView.Columns.Add(new System.Windows.Controls.GridViewColumn { Header = headerText, DisplayMemberBinding = new System.Windows.Data.Binding($"[{i}]") });
                }

                var items = new List<string[]>();
                for (int r = 1; r < data.Count; r++)
                {
                    var row = data[r];
                    var arr = new string[colCount];
                    for (int i = 0; i < colCount; i++) arr[i] = i < row.Count ? row[i] : string.Empty;
                    items.Add(arr);
                }

                var listView = new System.Windows.Controls.ListView
                {
                    View = gridView,
                    ItemsSource = items,
                    Background = palette.EditorBg,
                    Foreground = palette.EditorFg,
                    BorderThickness = new Thickness(1),
                    BorderBrush = palette.TableBorder,
                    MaxHeight = 600, // Important so it virtualizes within the FlowDocument
                };
                System.Windows.Controls.VirtualizingPanel.SetIsVirtualizing(listView, true);
                System.Windows.Controls.VirtualizingPanel.SetVirtualizationMode(listView, System.Windows.Controls.VirtualizationMode.Recycling);
                
                doc.Blocks.Add(new BlockUIContainer(listView));
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
