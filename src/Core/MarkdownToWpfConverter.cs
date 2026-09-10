using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MDPlus.Core
{
    public class MarkdownToWpfConverter
    {
        private readonly string _baseDirectory;
        private readonly bool _isDark;
        private readonly ThemePalette _palette;

        // Theme colors
        private readonly SolidColorBrush _textBrush;
        private readonly SolidColorBrush _headingBrush;
        private readonly SolidColorBrush _mutedBrush;
        private readonly SolidColorBrush _borderBrush;
        private readonly SolidColorBrush _codeBgBrush;
        private readonly SolidColorBrush _tableHeaderBg;
        private readonly SolidColorBrush _tableAltRowBg;
        private readonly SolidColorBrush _accentBrush;
        private readonly SolidColorBrush _linkBrush;

        private readonly bool _enableLatex;
        private readonly bool _enableHtml;

        public event EventHandler<string>? AnchorNavigationRequested;
        public event EventHandler<FileNavigationEventArgs>? FileNavigationRequested;

        private DateTime _lastNavigationTime = DateTime.MinValue;
        private string _lastNavigationTarget = string.Empty;

        public MarkdownToWpfConverter(string baseDirectory, ThemePalette palette, bool enableLatex = true, bool enableHtml = true)
        {
            _baseDirectory = baseDirectory;
            _palette = palette ?? ThemePalette.GitHubDark;
            _isDark = _palette.IsDark;
            _enableLatex = enableLatex;
            _enableHtml = enableHtml;

            _textBrush = _palette.EditorFg;
            _headingBrush = _palette.HeadingFg;
            _mutedBrush = _palette.MutedFg;
            _borderBrush = _palette.Border;
            _codeBgBrush = _palette.CodeBg;
            _tableHeaderBg = _palette.TableHeaderBg;
            _tableAltRowBg = _palette.TableAltRowBg;
            _accentBrush = _palette.Accent;
            _linkBrush = _palette.Accent;
        }

        public MarkdownToWpfConverter(string baseDirectory, bool isDark, bool enableLatex = true, bool enableHtml = true)
            : this(baseDirectory, isDark ? ThemePalette.GitHubDark : ThemePalette.GitHubLight, enableLatex, enableHtml)
        {
        }

        public FlowDocument Convert(MarkdownDocument doc)
        {
            var flowDoc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI, -apple-system, sans-serif"),
                FontSize = 15,
                Foreground = _textBrush,
                Background = Brushes.Transparent,
                PagePadding = new Thickness(48, 32, 48, 48),
                LineHeight = 24,
                IsHyphenationEnabled = false,
                ColumnWidth = double.PositiveInfinity // Prevent multi-column layout on wide screens
            };

            foreach (var block in doc.Blocks)
            {
                var wpfBlock = ConvertBlock(block);
                if (wpfBlock != null)
                {
                    flowDoc.Blocks.Add(wpfBlock);
                }
            }

            return flowDoc;
        }

        private Block? ConvertBlock(MarkdownBlock block)
        {
            switch (block)
            {
                case FrontmatterBlock fm:
                    return ConvertFrontmatter(fm);

                case HeadingBlock heading:
                    return ConvertHeading(heading);

                case ParagraphBlock para:
                    return ConvertParagraph(para);

                case BlockquoteBlock quote:
                    return ConvertBlockquote(quote);

                case CodeBlock code:
                    return ConvertCodeBlock(code);

                case TableBlock table:
                    return ConvertTable(table);

                case ListBlock list:
                    return ConvertList(list);

                case ThematicBreakBlock _:
                    return ConvertThematicBreak();

                case MathBlock math:
                    return ConvertMathBlock(math);

                case HtmlBlock html:
                    return ConvertHtmlBlock(html);

                default:
                    return null;
            }
        }

        private Block ConvertMathBlock(MathBlock math)
        {
            if (_enableLatex)
            {
                var mathElement = LatexMathRenderer.RenderMath(math.Expression, _palette, 16, isDisplay: true);
                return new BlockUIContainer(mathElement)
                {
                    Tag = new MathTag { Expression = math.Expression, IsDisplay = true }
                };
            }
            else
            {
                return new Paragraph(new Run($"$$\n{math.Expression}\n$$") { Foreground = _mutedBrush })
                {
                    Margin = new Thickness(0, 8, 0, 12),
                    Tag = new MathTag { Expression = math.Expression, IsDisplay = true }
                };
            }
        }

        private Block ConvertHtmlBlock(HtmlBlock html)
        {
            if (_enableHtml)
            {
                return HtmlWpfRenderer.RenderHtmlBlock(html, _palette, ConvertBlock, ConvertInline);
            }
            else
            {
                return new Paragraph(new Run(html.RawHtml) { Foreground = _mutedBrush })
                {
                    Margin = new Thickness(0, 4, 0, 8),
                    Tag = new HtmlBlockTag { RawHtml = html.RawHtml, Tag = html.Tag }
                };
            }
        }

        private Block ConvertFrontmatter(FrontmatterBlock fm)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 24),
                Background = _codeBgBrush
            };

            var border = new Border
            {
                BorderBrush = _borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16, 12, 16, 12),
                Background = _codeBgBrush
            };

            var sp = new StackPanel();
            var titleText = new TextBlock
            {
                Text = "METADATA",
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = _mutedBrush,
                Margin = new Thickness(0, 0, 0, 8)
            };
            sp.Children.Add(titleText);

            foreach (var kvp in fm.Metadata)
            {
                var rowSp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                var keyText = new TextBlock
                {
                    Text = kvp.Key + ": ",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = _accentBrush,
                    FontSize = 13
                };
                var valText = new TextBlock
                {
                    Text = kvp.Value,
                    Foreground = _textBrush,
                    FontSize = 13
                };
                rowSp.Children.Add(keyText);
                rowSp.Children.Add(valText);
                sp.Children.Add(rowSp);
            }

            border.Child = sp;
            return new BlockUIContainer(border) { Margin = new Thickness(0, 0, 0, 16) };
        }

        private Block ConvertHeading(HeadingBlock heading)
        {
            var p = new Paragraph
            {
                Tag = heading.Anchor,
                FontWeight = FontWeights.SemiBold,
                Foreground = _headingBrush
            };

            switch (heading.Level)
            {
                case 1:
                    p.FontSize = 26;
                    p.FontWeight = FontWeights.Bold;
                    p.Margin = new Thickness(0, 28, 0, 14);
                    p.BorderBrush = _borderBrush;
                    p.BorderThickness = new Thickness(0, 0, 0, 1);
                    p.Padding = new Thickness(0, 0, 0, 8);
                    break;
                case 2:
                    p.FontSize = 20;
                    p.FontWeight = FontWeights.Bold;
                    p.Margin = new Thickness(0, 24, 0, 12);
                    p.BorderBrush = _borderBrush;
                    p.BorderThickness = new Thickness(0, 0, 0, 1);
                    p.Padding = new Thickness(0, 0, 0, 6);
                    break;
                case 3:
                    p.FontSize = 17;
                    p.Margin = new Thickness(0, 20, 0, 10);
                    break;
                case 4:
                    p.FontSize = 15;
                    p.Margin = new Thickness(0, 16, 0, 8);
                    break;
                case 5:
                    p.FontSize = 13.5;
                    p.Foreground = _mutedBrush;
                    p.Margin = new Thickness(0, 12, 0, 6);
                    break;
                case 6:
                    p.FontSize = 12.5;
                    p.FontStyle = FontStyles.Italic;
                    p.Foreground = _mutedBrush;
                    p.Margin = new Thickness(0, 10, 0, 4);
                    break;
            }

            foreach (var inline in heading.Inlines)
            {
                var wpfInline = ConvertInline(inline);
                if (wpfInline != null) p.Inlines.Add(wpfInline);
            }

            return p;
        }

        private Block ConvertParagraph(ParagraphBlock para)
        {
            var p = new Paragraph
            {
                Margin = new Thickness(0, 0, 0, 14),
                LineHeight = 24
            };

            foreach (var inline in para.Inlines)
            {
                var wpfInline = ConvertInline(inline);
                if (wpfInline != null) p.Inlines.Add(wpfInline);
            }

            return p;
        }

        private Block ConvertBlockquote(BlockquoteBlock quote)
        {
            if (quote.Callout != CalloutType.None)
            {
                return ConvertCallout(quote);
            }

            var section = new Section
            {
                BorderBrush = _borderBrush,
                BorderThickness = new Thickness(3.5, 0, 0, 0),
                Padding = new Thickness(16, 4, 0, 4),
                Margin = new Thickness(0, 8, 0, 16)
            };

            foreach (var childBlock in quote.Blocks)
            {
                var converted = ConvertBlock(childBlock);
                if (converted != null)
                {
                    if (converted is Paragraph p)
                    {
                        p.Foreground = _mutedBrush;
                    }
                    section.Blocks.Add(converted);
                }
            }

            return section;
        }

        private Block ConvertCallout(BlockquoteBlock quote)
        {
            Color calloutBorderColor;
            Color calloutBgColor;
            string icon;

            switch (quote.Callout)
            {
                case CalloutType.Tip:
                    calloutBorderColor = _isDark ? Color.FromRgb(63, 185, 80) : Color.FromRgb(26, 127, 55);
                    calloutBgColor = _isDark ? Color.FromArgb(30, 46, 160, 67) : Color.FromArgb(40, 218, 251, 225);
                    icon = "💡";
                    break;
                case CalloutType.Important:
                    calloutBorderColor = _isDark ? Color.FromRgb(163, 113, 247) : Color.FromRgb(130, 80, 223);
                    calloutBgColor = _isDark ? Color.FromArgb(30, 130, 80, 223) : Color.FromArgb(35, 237, 228, 255);
                    icon = "📌";
                    break;
                case CalloutType.Warning:
                    calloutBorderColor = _isDark ? Color.FromRgb(210, 153, 34) : Color.FromRgb(154, 103, 0);
                    calloutBgColor = _isDark ? Color.FromArgb(30, 210, 153, 34) : Color.FromArgb(40, 255, 248, 197);
                    icon = "⚠️";
                    break;
                case CalloutType.Caution:
                    calloutBorderColor = _isDark ? Color.FromRgb(248, 81, 73) : Color.FromRgb(207, 34, 46);
                    calloutBgColor = _isDark ? Color.FromArgb(30, 248, 81, 73) : Color.FromArgb(40, 255, 235, 233);
                    icon = "🚫";
                    break;
                case CalloutType.Note:
                default:
                    calloutBorderColor = _isDark ? Color.FromRgb(88, 166, 255) : Color.FromRgb(9, 105, 218);
                    calloutBgColor = _isDark ? Color.FromArgb(30, 56, 139, 253) : Color.FromArgb(40, 221, 244, 255);
                    icon = "ℹ️";
                    break;
            }

            var section = new Section
            {
                BorderBrush = new SolidColorBrush(calloutBorderColor),
                BorderThickness = new Thickness(4, 0, 0, 0),
                Background = new SolidColorBrush(calloutBgColor),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 8, 0, 16)
            };

            // Header paragraph with icon and title
            var headerPara = new Paragraph
            {
                Margin = new Thickness(0, 0, 0, quote.Blocks.Count > 0 ? 8 : 0),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(calloutBorderColor)
            };
            headerPara.Inlines.Add(new Run(icon + " " + quote.CalloutTitle));
            section.Blocks.Add(headerPara);

            // Render all child blocks (Paragraphs, Lists, Code blocks, Tables, etc.)
            foreach (var childBlock in quote.Blocks)
            {
                var converted = ConvertBlock(childBlock);
                if (converted != null)
                {
                    section.Blocks.Add(converted);
                }
            }

            return section;
        }

        private Block ConvertCodeBlock(CodeBlock code)
        {
            var outerBorder = new Border
            {
                Background = _codeBgBrush,
                BorderBrush = _palette != null ? _palette.CodeBorder : _borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 8, 0, 16)
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header row with language tag and copy button
            var headerGrid = new Grid
            {
                Background = _isDark ? new SolidColorBrush(Color.FromRgb(30, 35, 42)) : new SolidColorBrush(Color.FromRgb(235, 238, 242)),
                Height = 28
            };

            string displayLang = string.IsNullOrEmpty(code.Language) ? "TEXT" : code.Language.ToUpperInvariant();
            var langLabel = new TextBlock
            {
                Text = displayLang,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = _mutedBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            headerGrid.Children.Add(langLabel);

            var copyBtn = new Button
            {
                Content = "Copy",
                FontSize = 11,
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = _borderBrush,
                Foreground = _mutedBrush
            };

            string rawCode = code.Code;
            copyBtn.Click += (s, e) =>
            {
                if (ClipboardHelper.SetText(rawCode))
                {
                    copyBtn.Content = "Copied!";
                    var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (ts, te) =>
                    {
                        copyBtn.Content = "Copy";
                        timer.Stop();
                    };
                    timer.Start();
                }
            };
            headerGrid.Children.Add(copyBtn);
            Grid.SetRow(headerGrid, 0);
            mainGrid.Children.Add(headerGrid);

            // Code content with syntax highlighting
            var textBlock = new TextBlock
            {
                FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New, monospace"),
                FontSize = 13,
                LineHeight = 20,
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.Wrap
            };

            var tokens = SyntaxHighlighter.Highlight(code.Code, code.Language);
            foreach (var token in tokens)
            {
                var run = new Run(token.Text)
                {
                    Foreground = SyntaxHighlighter.GetTokenBrush(token.Type, _isDark)
                };
                textBlock.Inlines.Add(run);
            }

            Grid.SetRow(textBlock, 1);
            mainGrid.Children.Add(textBlock);

            outerBorder.Child = mainGrid;
            return new BlockUIContainer(outerBorder);
        }

        private Block ConvertTable(TableBlock table)
        {
            var wpfTable = new Table
            {
                CellSpacing = 0,
                Margin = new Thickness(0, 8, 0, 18),
                BorderBrush = _borderBrush,
                BorderThickness = new Thickness(1)
            };

            int colCount = Math.Max(table.Header.Cells.Count, table.Alignments.Count);
            if (colCount == 0) colCount = 1;

            for (int i = 0; i < colCount; i++)
            {
                wpfTable.Columns.Add(new TableColumn());
            }

            // Header Group
            var headerGroup = new TableRowGroup();
            var headerRow = new System.Windows.Documents.TableRow
            {
                Background = _tableHeaderBg
            };

            for (int i = 0; i < colCount; i++)
            {
                var cellText = i < table.Header.Cells.Count ? table.Header.Cells[i] : new TableCell();
                var align = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;

                var p = new Paragraph
                {
                    FontWeight = FontWeights.SemiBold,
                    Foreground = _headingBrush,
                    TextAlignment = ConvertAlignment(align),
                    Margin = new Thickness(0)
                };

                foreach (var inline in cellText.Inlines)
                {
                    var wpfInline = ConvertInline(inline);
                    if (wpfInline != null) p.Inlines.Add(wpfInline);
                }

                var cell = new System.Windows.Documents.TableCell(p)
                {
                    Padding = new Thickness(12, 10, 12, 10),
                    BorderBrush = _borderBrush,
                    BorderThickness = new Thickness(0, 0, i == colCount - 1 ? 0 : 1, 2)
                };
                headerRow.Cells.Add(cell);
            }
            headerGroup.Rows.Add(headerRow);
            wpfTable.RowGroups.Add(headerGroup);

            // Body Group
            if (table.Rows.Count > 0)
            {
                var bodyGroup = new TableRowGroup();
                for (int r = 0; r < table.Rows.Count; r++)
                {
                    var dataRow = table.Rows[r];
                    var wpfRow = new System.Windows.Documents.TableRow
                    {
                        Background = (r % 2 == 1) ? _tableAltRowBg : Brushes.Transparent
                    };

                    for (int i = 0; i < colCount; i++)
                    {
                        var cellText = i < dataRow.Cells.Count ? dataRow.Cells[i] : new TableCell();
                        var align = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;

                        var p = new Paragraph
                        {
                            TextAlignment = ConvertAlignment(align),
                            Margin = new Thickness(0)
                        };

                        foreach (var inline in cellText.Inlines)
                        {
                            var wpfInline = ConvertInline(inline);
                            if (wpfInline != null) p.Inlines.Add(wpfInline);
                        }

                        var cell = new System.Windows.Documents.TableCell(p)
                        {
                            Padding = new Thickness(12, 8, 12, 8),
                            BorderBrush = _borderBrush,
                            BorderThickness = new Thickness(0, 0, i == colCount - 1 ? 0 : 1, 1)
                        };
                        wpfRow.Cells.Add(cell);
                    }
                    bodyGroup.Rows.Add(wpfRow);
                }
                wpfTable.RowGroups.Add(bodyGroup);
            }

            return wpfTable;
        }

        private TextAlignment ConvertAlignment(ColumnAlignment align)
        {
            return align switch
            {
                ColumnAlignment.Center => TextAlignment.Center,
                ColumnAlignment.Right => TextAlignment.Right,
                _ => TextAlignment.Left
            };
        }

        private Block ConvertList(ListBlock list)
        {
            bool allTasks = list.Items.Count > 0 && list.Items.All(it => it.IsTask);
            var wpfList = new List
            {
                MarkerStyle = allTasks ? TextMarkerStyle.None : (list.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc),
                StartIndex = list.StartNumber,
                Margin = new Thickness(0, 4, 0, 14),
                Padding = new Thickness(24, 0, 0, 0)
            };

            foreach (var item in list.Items)
            {
                var wpfItem = new ListItem();

                if (item.IsTask)
                {
                    // Checkbox task item
                    var p = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };

                    var checkBox = new CheckBox
                    {
                        IsChecked = item.IsChecked,
                        IsEnabled = true,
                        Margin = new Thickness(allTasks ? -18 : 0, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    p.Inlines.Add(new InlineUIContainer(checkBox) { BaselineAlignment = BaselineAlignment.Center });

                    foreach (var inline in item.Inlines)
                    {
                        var wpfInline = ConvertInline(inline);
                        if (wpfInline != null)
                        {
                            if (item.IsChecked && wpfInline is Run run)
                            {
                                run.Foreground = _mutedBrush;
                            }
                            p.Inlines.Add(wpfInline);
                        }
                    }
                    wpfItem.Blocks.Add(p);
                }
                else
                {
                    var p = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                    foreach (var inline in item.Inlines)
                    {
                        var wpfInline = ConvertInline(inline);
                        if (wpfInline != null) p.Inlines.Add(wpfInline);
                    }
                    wpfItem.Blocks.Add(p);
                }

                foreach (var childBlock in item.Blocks)
                {
                    var converted = ConvertBlock(childBlock);
                    if (converted != null) wpfItem.Blocks.Add(converted);
                }

                wpfList.ListItems.Add(wpfItem);
            }

            return wpfList;
        }

        private Block ConvertThematicBreak()
        {
            var line = new Border
            {
                Height = 1,
                Background = _borderBrush,
                Margin = new Thickness(0, 16, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            return new BlockUIContainer(line);
        }

        private Inline? ConvertInline(MarkdownInline inline)
        {
            switch (inline)
            {
                case TextInline text:
                    return new Run(text.Text) { Foreground = _textBrush };

                case BoldInline bold:
                    var boldSpan = new Bold();
                    foreach (var c in bold.Children)
                    {
                        var ci = ConvertInline(c);
                        if (ci != null) boldSpan.Inlines.Add(ci);
                    }
                    return boldSpan;

                case ItalicInline italic:
                    var italicSpan = new Italic();
                    foreach (var c in italic.Children)
                    {
                        var ci = ConvertInline(c);
                        if (ci != null) italicSpan.Inlines.Add(ci);
                    }
                    return italicSpan;

                case BoldItalicInline bi:
                    var biSpan = new Bold(new Italic());
                    var innerItalic = (Italic)biSpan.Inlines.FirstInline;
                    foreach (var c in bi.Children)
                    {
                        var ci = ConvertInline(c);
                        if (ci != null) innerItalic.Inlines.Add(ci);
                    }
                    return biSpan;

                case StrikethroughInline strike:
                    var strikeSpan = new Span();
                    strikeSpan.TextDecorations.Add(TextDecorations.Strikethrough);
                    foreach (var c in strike.Children)
                    {
                        var ci = ConvertInline(c);
                        if (ci != null) strikeSpan.Inlines.Add(ci);
                    }
                    return strikeSpan;

                case HighlightInline hl:
                    var hlSpan = new Span
                    {
                        Background = _isDark ? new SolidColorBrush(Color.FromRgb(102, 85, 0)) : new SolidColorBrush(Color.FromRgb(255, 243, 198)),
                        Foreground = _isDark ? new SolidColorBrush(Color.FromRgb(255, 230, 100)) : _textBrush
                    };
                    foreach (var c in hl.Children)
                    {
                        var ci = ConvertInline(c);
                        if (ci != null) hlSpan.Inlines.Add(ci);
                    }
                    return hlSpan;

                case CodeInline code:
                    var codeBorder = new Border
                    {
                        Background = _codeBgBrush,
                        BorderBrush = _borderBrush,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(5, 1, 5, 1),
                        Margin = new Thickness(2, 2.5, 2, -2.5),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var codeText = new TextBlock
                    {
                        Text = code.Code,
                        FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New, monospace"),
                        FontSize = 13,
                        Foreground = _textBrush,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    codeBorder.Child = codeText;
                    return new InlineUIContainer(codeBorder) { BaselineAlignment = BaselineAlignment.Center };

                case LinkInline link:
                    var hyperlink = new Hyperlink
                    {
                        Foreground = _linkBrush,
                        TextDecorations = null,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        ToolTip = link.Url
                    };

                    if (!string.IsNullOrEmpty(link.Url))
                    {
                        if (Uri.TryCreate(link.Url, UriKind.RelativeOrAbsolute, out Uri? uri))
                        {
                            hyperlink.NavigateUri = uri;
                        }
                    }

                    hyperlink.RequestNavigate += (s, e) =>
                    {
                        HandleNavigation(e.Uri?.OriginalString ?? link.Url);
                        e.Handled = true;
                    };

                    hyperlink.Click += (s, e) =>
                    {
                        HandleNavigation(link.Url);
                        e.Handled = true;
                    };

                    foreach (var c in link.Children)
                    {
                        var ci = ConvertInline(c);
                        if (ci != null) hyperlink.Inlines.Add(ci);
                    }
                    return hyperlink;

                case ImageInline img:
                    return ConvertImageInline(img);

                case LineBreakInline br:
                    return br.IsHard ? (Inline)new LineBreak() : new Run(" ");

                case MathInline math:
                    if (_enableLatex)
                    {
                        var mathElement = LatexMathRenderer.RenderMath(math.Expression, _palette, 14.5, math.IsDisplay);
                        return new InlineUIContainer(mathElement)
                        {
                            BaselineAlignment = BaselineAlignment.Center,
                            Tag = new MathTag { Expression = math.Expression, IsDisplay = math.IsDisplay }
                        };
                    }
                    else
                    {
                        return new Run(math.IsDisplay ? $"$${math.Expression}$$" : $"${math.Expression}$")
                        {
                            Foreground = _textBrush,
                            Tag = new MathTag { Expression = math.Expression, IsDisplay = math.IsDisplay }
                        };
                    }

                case HtmlInline html:
                    if (_enableHtml)
                    {
                        return HtmlWpfRenderer.RenderHtmlInline(
                            html,
                            _palette,
                            onNavigate: HandleNavigation,
                            convertChild: ConvertInline);
                    }
                    else
                    {
                        return new Run(html.RawHtml)
                        {
                            Foreground = _textBrush,
                            Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = html.Tag }
                        };
                    }

                default:
                    return null;
            }
        }

        private Inline ConvertImageInline(ImageInline img)
        {
            string resolvedPath = Uri.UnescapeDataString(img.Url);

            try
            {
                if (!Uri.IsWellFormedUriString(resolvedPath, UriKind.Absolute) && !System.IO.Path.IsPathRooted(resolvedPath))
                {
                    if (!string.IsNullOrEmpty(_baseDirectory))
                    {
                        resolvedPath = System.IO.Path.Combine(_baseDirectory, resolvedPath);
                    }
                }

                if (File.Exists(resolvedPath))
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = new Uri(resolvedPath, UriKind.Absolute);
                    bi.EndInit();
                    bi.Freeze();

                    var imageControl = new Image
                    {
                        Source = bi,
                        Stretch = Stretch.Uniform,
                        MaxWidth = 800,
                        Margin = new Thickness(0, 8, 0, 8)
                    };

                    if (!string.IsNullOrEmpty(img.Title))
                    {
                        imageControl.ToolTip = img.Title;
                    }

                    return new InlineUIContainer(imageControl) { BaselineAlignment = BaselineAlignment.Center };
                }
                else if (Uri.TryCreate(img.Url, UriKind.Absolute, out Uri? webUri) && (webUri.Scheme == Uri.UriSchemeHttp || webUri.Scheme == Uri.UriSchemeHttps))
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = webUri;
                    bi.EndInit();

                    var imageControl = new Image
                    {
                        Source = bi,
                        Stretch = Stretch.Uniform,
                        MaxWidth = 800,
                        Margin = new Thickness(0, 8, 0, 8)
                    };
                    return new InlineUIContainer(imageControl) { BaselineAlignment = BaselineAlignment.Center };
                }
            }
            catch
            {
                // Fallback on image load failure
            }

            // Fallback placeholder
            var border = new Border
            {
                Background = _codeBgBrush,
                BorderBrush = _borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(2, 0, 2, 0)
            };
            var placeholderText = new TextBlock
            {
                Text = "🖼 " + (string.IsNullOrEmpty(img.AltText) ? img.Url : img.AltText),
                Foreground = _mutedBrush,
                FontSize = 12
            };
            border.Child = placeholderText;
            return new InlineUIContainer(border) { BaselineAlignment = BaselineAlignment.Center };
        }

        public void HandleNavigation(string? target)
        {
            if (string.IsNullOrWhiteSpace(target)) return;
            target = target.Trim();

            // Debounce rapid duplicate invocations (e.g. if both Click and RequestNavigate fire)
            if (target == _lastNavigationTarget && (DateTime.UtcNow - _lastNavigationTime).TotalMilliseconds < 400)
            {
                return;
            }
            _lastNavigationTime = DateTime.UtcNow;
            _lastNavigationTarget = target;

            // 1. Pure anchor navigation (#heading-anchor)
            if (target.StartsWith("#"))
            {
                string anchor = target.TrimStart('#');
                try { anchor = Uri.UnescapeDataString(anchor); } catch { }
                AnchorNavigationRequested?.Invoke(this, anchor);
                return;
            }

            // 2. Web schemes (http:, https:, mailto:, ftp:) always open in default browser
            if (Uri.TryCreate(target, UriKind.Absolute, out Uri? webUri) &&
                (webUri.Scheme == Uri.UriSchemeHttp ||
                 webUri.Scheme == Uri.UriSchemeHttps ||
                 webUri.Scheme == Uri.UriSchemeMailto ||
                 webUri.Scheme == Uri.UriSchemeFtp))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(webUri.AbsoluteUri) { UseShellExecute = true });
                }
                catch
                {
                    // Fail gracefully if browser launch fails
                }
                return;
            }

            // 3. Check for local Markdown file (relative, absolute, or file://, with optional anchor)
            string pathWithoutAnchor = target;
            string? targetAnchor = null;
            int hashIdx = target.IndexOf('#');
            if (hashIdx >= 0)
            {
                pathWithoutAnchor = target.Substring(0, hashIdx);
                targetAnchor = target.Substring(hashIdx + 1);
            }

            // Handle file:// URI scheme
            if (Uri.TryCreate(pathWithoutAnchor, UriKind.Absolute, out Uri? fileUri) && fileUri.IsFile)
            {
                pathWithoutAnchor = fileUri.LocalPath;
            }

            try
            {
                pathWithoutAnchor = Uri.UnescapeDataString(pathWithoutAnchor);
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(targetAnchor))
            {
                try
                {
                    targetAnchor = Uri.UnescapeDataString(targetAnchor);
                }
                catch
                {
                }
            }

            // Strip optional query string if present (e.g. "doc.md?version=1")
            int queryIdx = pathWithoutAnchor.IndexOf('?');
            if (queryIdx >= 0)
            {
                pathWithoutAnchor = pathWithoutAnchor.Substring(0, queryIdx);
            }

            if (pathWithoutAnchor.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                pathWithoutAnchor.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase) ||
                pathWithoutAnchor.EndsWith(".mdown", StringComparison.OrdinalIgnoreCase) ||
                pathWithoutAnchor.EndsWith(".mkd", StringComparison.OrdinalIgnoreCase))
            {
                string fullTargetPath = pathWithoutAnchor;
                if (!Path.IsPathRooted(fullTargetPath))
                {
                    if (!string.IsNullOrEmpty(_baseDirectory))
                    {
                        fullTargetPath = Path.Combine(_baseDirectory, pathWithoutAnchor);
                    }
                    else
                    {
                        fullTargetPath = Path.Combine(Directory.GetCurrentDirectory(), pathWithoutAnchor);
                    }
                }

                try
                {
                    fullTargetPath = Path.GetFullPath(fullTargetPath);
                }
                catch
                {
                }

                if (!File.Exists(fullTargetPath))
                {
                    // Fallback: check relative to AppDomain base or sample_docs
                    string appDomainPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathWithoutAnchor);
                    if (File.Exists(appDomainPath))
                    {
                        fullTargetPath = appDomainPath;
                    }
                    else
                    {
                        string samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_docs", Path.GetFileName(pathWithoutAnchor));
                        if (File.Exists(samplePath))
                        {
                            fullTargetPath = samplePath;
                        }
                    }
                }

                if (File.Exists(fullTargetPath))
                {
                    FileNavigationRequested?.Invoke(this, new FileNavigationEventArgs(fullTargetPath, targetAnchor));
                    return;
                }
            }
            // Non-web schemes (e.g. cmd:, powershell:, executables) or missing files are blocked for security.
        }
    }

    public class FileNavigationEventArgs : EventArgs
    {
        public string FilePath { get; }
        public string? Anchor { get; }

        public FileNavigationEventArgs(string filePath, string? anchor = null)
        {
            FilePath = filePath;
            Anchor = anchor;
        }
    }
}
