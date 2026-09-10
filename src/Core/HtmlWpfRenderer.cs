using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MDPlus.Core
{
    public class HtmlInlineTag
    {
        public string RawHtml { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
    }

    public class HtmlBlockTag
    {
        public string RawHtml { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
    }

    /// <summary>
    /// High-performance native HTML renderer for WPF FlowDocuments.
    /// Renders raw HTML elements and blocks into crisp native WPF FlowDocument components
    /// without any WebBrowser or WebView2 dependency.
    /// Supports kbd, sub, sup, u, mark, del, b, i, span (with inline CSS styles),
    /// details/summary disclosure widgets, aligned paragraphs, divs, and tables.
    /// </summary>
    public static class HtmlWpfRenderer
    {
        public static Inline RenderHtmlInline(
            HtmlInline html,
            ThemePalette palette,
            Action<string>? onNavigate = null,
            Func<MarkdownInline, Inline?>? convertChild = null)
        {
            if (html == null) return new Run();

            string tag = (html.Tag ?? string.Empty).ToLowerInvariant();
            var fgBrush = palette?.EditorFg ?? Brushes.Black;
            var borderBrush = palette?.Border ?? Brushes.Gray;
            bool isDark = palette?.IsDark ?? false;

            switch (tag)
            {
                case "kbd":
                {
                    var keyBorder = new Border
                    {
                        Background = isDark ? new SolidColorBrush(Color.FromRgb(33, 38, 45)) : new SolidColorBrush(Color.FromRgb(243, 244, 246)),
                        BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(68, 76, 86)) : new SolidColorBrush(Color.FromRgb(209, 213, 218)),
                        BorderThickness = new Thickness(1, 1, 1, 2), // 3D keycap effect
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(6, 1, 6, 2),
                        Margin = new Thickness(2, 0, 2, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var textBlock = new TextBlock
                    {
                        Text = html.Content,
                        FontFamily = new FontFamily("Cascadia Code, Consolas, monospace"),
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = isDark ? new SolidColorBrush(Color.FromRgb(201, 209, 217)) : new SolidColorBrush(Color.FromRgb(36, 41, 47)),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    keyBorder.Child = textBlock;

                    return new InlineUIContainer(keyBorder)
                    {
                        BaselineAlignment = BaselineAlignment.Center,
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "kbd" }
                    };
                }

                case "sub":
                {
                    var subSpan = new Span
                    {
                        FontSize = 11,
                        BaselineAlignment = BaselineAlignment.Subscript,
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "sub" }
                    };
                    PopulateChildren(subSpan, html, convertChild, fgBrush);
                    return subSpan;
                }

                case "sup":
                {
                    var supSpan = new Span
                    {
                        FontSize = 11,
                        BaselineAlignment = BaselineAlignment.Superscript,
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "sup" }
                    };
                    PopulateChildren(supSpan, html, convertChild, fgBrush);
                    return supSpan;
                }

                case "u":
                {
                    var uSpan = new Span
                    {
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "u" }
                    };
                    uSpan.TextDecorations.Add(TextDecorations.Underline);
                    PopulateChildren(uSpan, html, convertChild, fgBrush);
                    return uSpan;
                }

                case "mark":
                {
                    var markSpan = new Span
                    {
                        Background = isDark
                            ? new SolidColorBrush(Color.FromRgb(102, 85, 0))
                            : new SolidColorBrush(Color.FromRgb(255, 243, 198)),
                        Foreground = isDark
                            ? new SolidColorBrush(Color.FromRgb(255, 230, 100))
                            : fgBrush,
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "mark" }
                    };
                    PopulateChildren(markSpan, html, convertChild, markSpan.Foreground);
                    return markSpan;
                }

                case "del":
                case "s":
                case "strike":
                {
                    var strikeSpan = new Span
                    {
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = tag }
                    };
                    strikeSpan.TextDecorations.Add(TextDecorations.Strikethrough);
                    PopulateChildren(strikeSpan, html, convertChild, fgBrush);
                    return strikeSpan;
                }

                case "b":
                case "strong":
                {
                    var boldSpan = new Bold
                    {
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = tag }
                    };
                    PopulateChildren(boldSpan, html, convertChild, fgBrush);
                    return boldSpan;
                }

                case "i":
                case "em":
                {
                    var italicSpan = new Italic
                    {
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = tag }
                    };
                    PopulateChildren(italicSpan, html, convertChild, fgBrush);
                    return italicSpan;
                }

                case "code":
                {
                    var codeBorder = new Border
                    {
                        Background = palette?.CodeBg ?? Brushes.Transparent,
                        BorderBrush = borderBrush,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(5, 1, 5, 1),
                        Margin = new Thickness(2, 0, 2, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var codeText = new TextBlock
                    {
                        Text = html.Content,
                        FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New, monospace"),
                        FontSize = 13,
                        Foreground = fgBrush,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    codeBorder.Child = codeText;
                    return new InlineUIContainer(codeBorder)
                    {
                        BaselineAlignment = BaselineAlignment.Center,
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "code" }
                    };
                }

                case "span":
                case "font":
                {
                    var span = new Span
                    {
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = tag }
                    };

                    ApplyInlineStyles(span, html.Attributes, isDark, fgBrush);
                    PopulateChildren(span, html, convertChild, span.Foreground ?? fgBrush);
                    return span;
                }

                case "br":
                {
                    return new LineBreak();
                }

                case "a":
                {
                    var hyperlink = new Hyperlink
                    {
                        Foreground = palette?.Accent ?? Brushes.Blue,
                        TextDecorations = null,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "a" }
                    };

                    string href = html.Attributes.TryGetValue("href", out string? hVal) ? hVal : string.Empty;
                    if (!string.IsNullOrEmpty(href))
                    {
                        hyperlink.ToolTip = href;
                        if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out Uri? uri))
                        {
                            hyperlink.NavigateUri = uri;
                        }
                    }

                    hyperlink.Click += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(href))
                        {
                            onNavigate?.Invoke(href);
                        }
                        e.Handled = true;
                    };

                    PopulateChildren(hyperlink, html, convertChild, hyperlink.Foreground);
                    return hyperlink;
                }

                case "img":
                {
                    string src = html.Attributes.TryGetValue("src", out string? sVal) ? sVal : string.Empty;
                    string alt = html.Attributes.TryGetValue("alt", out string? aVal) ? aVal : string.Empty;
                    string title = html.Attributes.TryGetValue("title", out string? tVal) ? tVal : string.Empty;

                    double width = double.NaN;
                    double height = double.NaN;
                    if (html.Attributes.TryGetValue("width", out string? wVal) && double.TryParse(wVal.TrimEnd('p', 'x'), out double wParsed))
                        width = wParsed;
                    if (html.Attributes.TryGetValue("height", out string? hVal2) && double.TryParse(hVal2.TrimEnd('p', 'x'), out double hParsed))
                        height = hParsed;

                    try
                    {
                        if (Uri.TryCreate(src, UriKind.Absolute, out Uri? imgUri) &&
                            (imgUri.Scheme == Uri.UriSchemeHttp || imgUri.Scheme == Uri.UriSchemeHttps || imgUri.Scheme == Uri.UriSchemeFile))
                        {
                            var bi = new BitmapImage();
                            bi.BeginInit();
                            bi.UriSource = imgUri;
                            bi.EndInit();

                            var image = new Image
                            {
                                Source = bi,
                                Stretch = Stretch.Uniform,
                                ToolTip = !string.IsNullOrEmpty(title) ? title : alt
                            };
                            if (!double.IsNaN(width)) image.Width = width;
                            if (!double.IsNaN(height)) image.Height = height;

                            return new InlineUIContainer(image)
                            {
                                BaselineAlignment = BaselineAlignment.Center,
                                Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = "img" }
                            };
                        }
                    }
                    catch
                    {
                        // Fallback
                    }

                    return new Run($"[{alt}]") { Foreground = palette?.MutedFg ?? Brushes.Gray };
                }

                default:
                {
                    // Generic inline tag wrapper
                    var fallbackSpan = new Span
                    {
                        Tag = new HtmlInlineTag { RawHtml = html.RawHtml, Tag = tag }
                    };
                    PopulateChildren(fallbackSpan, html, convertChild, fgBrush);
                    return fallbackSpan;
                }
            }
        }

        private static void PopulateChildren(
            Span container,
            HtmlInline html,
            Func<MarkdownInline, Inline?>? convertChild,
            Brush defaultFg)
        {
            if (html.Children.Count > 0 && convertChild != null)
            {
                foreach (var child in html.Children)
                {
                    var converted = convertChild(child);
                    if (converted != null) container.Inlines.Add(converted);
                }
            }
            else if (!string.IsNullOrEmpty(html.Content))
            {
                container.Inlines.Add(new Run(html.Content) { Foreground = defaultFg });
            }
        }

        private static void ApplyInlineStyles(Span span, Dictionary<string, string> attributes, bool isDark, Brush defaultFg)
        {
            if (attributes.TryGetValue("color", out string? fontColor))
            {
                var brush = TryParseColorBrush(fontColor);
                if (brush != null) span.Foreground = brush;
            }

            if (attributes.TryGetValue("style", out string? styleStr) && !string.IsNullOrWhiteSpace(styleStr))
            {
                var declarations = styleStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var decl in declarations)
                {
                    int colon = decl.IndexOf(':');
                    if (colon <= 0) continue;
                    string property = decl.Substring(0, colon).Trim().ToLowerInvariant();
                    string value = decl.Substring(colon + 1).Trim();

                    switch (property)
                    {
                        case "color":
                        {
                            var b = TryParseColorBrush(value);
                            if (b != null) span.Foreground = b;
                            break;
                        }
                        case "background":
                        case "background-color":
                        {
                            var b = TryParseColorBrush(value);
                            if (b != null) span.Background = b;
                            break;
                        }
                        case "font-size":
                        {
                            if (TryParseFontSize(value, out double fs))
                            {
                                span.FontSize = fs;
                            }
                            break;
                        }
                        case "font-weight":
                        {
                            if (value.Equals("bold", StringComparison.OrdinalIgnoreCase) ||
                                value.Equals("700", StringComparison.OrdinalIgnoreCase) ||
                                value.Equals("800", StringComparison.OrdinalIgnoreCase) ||
                                value.Equals("900", StringComparison.OrdinalIgnoreCase))
                            {
                                span.FontWeight = FontWeights.Bold;
                            }
                            else if (value.Equals("semibold", StringComparison.OrdinalIgnoreCase) ||
                                     value.Equals("600", StringComparison.OrdinalIgnoreCase))
                            {
                                span.FontWeight = FontWeights.SemiBold;
                            }
                            break;
                        }
                        case "font-style":
                        {
                            if (value.Equals("italic", StringComparison.OrdinalIgnoreCase))
                            {
                                span.FontStyle = FontStyles.Italic;
                            }
                            break;
                        }
                        case "text-decoration":
                        {
                            if (value.Contains("underline", StringComparison.OrdinalIgnoreCase))
                            {
                                span.TextDecorations.Add(TextDecorations.Underline);
                            }
                            if (value.Contains("line-through", StringComparison.OrdinalIgnoreCase))
                            {
                                span.TextDecorations.Add(TextDecorations.Strikethrough);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private static bool TryParseFontSize(string val, out double size)
        {
            size = 15;
            string clean = val.Trim().ToLowerInvariant();
            if (clean == "small" || clean == "smaller") { size = 12; return true; }
            if (clean == "medium") { size = 15; return true; }
            if (clean == "large" || clean == "larger") { size = 18; return true; }
            if (clean == "x-large") { size = 22; return true; }
            if (clean == "xx-large") { size = 26; return true; }

            if (clean.EndsWith("px") && double.TryParse(clean.Substring(0, clean.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out double px))
            {
                size = Math.Clamp(px, 8, 72);
                return true;
            }
            if (clean.EndsWith("pt") && double.TryParse(clean.Substring(0, clean.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out double pt))
            {
                size = Math.Clamp(pt * 1.333, 8, 72);
                return true;
            }
            if (clean.EndsWith("em") && double.TryParse(clean.Substring(0, clean.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out double em))
            {
                size = Math.Clamp(em * 15.0, 8, 72);
                return true;
            }

            return false;
        }

        public static Brush? TryParseColorBrush(string colorStr)
        {
            if (string.IsNullOrWhiteSpace(colorStr)) return null;
            string clean = colorStr.Trim();

            try
            {
                // Hex format #RGB, #RRGGBB, #AARRGGBB
                if (clean.StartsWith("#"))
                {
                    var color = (Color)ColorConverter.ConvertFromString(clean);
                    return new SolidColorBrush(color);
                }

                // Named colors
                var namedColor = (Color?)typeof(Colors).GetProperty(clean, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase)?.GetValue(null);
                if (namedColor.HasValue)
                {
                    return new SolidColorBrush(namedColor.Value);
                }

                // rgb(...) or rgba(...)
                if (clean.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                {
                    int openParen = clean.IndexOf('(');
                    int closeParen = clean.IndexOf(')');
                    if (openParen > 0 && closeParen > openParen)
                    {
                        string[] parts = clean.Substring(openParen + 1, closeParen - openParen - 1).Split(',');
                        if (parts.Length >= 3 &&
                            byte.TryParse(parts[0].Trim(), out byte r) &&
                            byte.TryParse(parts[1].Trim(), out byte g) &&
                            byte.TryParse(parts[2].Trim(), out byte b))
                        {
                            byte a = 255;
                            if (parts.Length >= 4 && double.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double aDbl))
                            {
                                a = (byte)Math.Clamp(aDbl * 255.0, 0, 255);
                            }
                            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
                        }
                    }
                }
            }
            catch
            {
                // Fail gracefully
            }

            return null;
        }

        public static Block RenderHtmlBlock(
            HtmlBlock html,
            ThemePalette palette,
            Func<MarkdownBlock, Block?> convertBlock)
        {
            string tag = (html.Tag ?? string.Empty).ToLowerInvariant();
            var borderBrush = palette?.Border ?? Brushes.Gray;
            bool isDark = palette?.IsDark ?? false;

            switch (tag)
            {
                case "details":
                {
                    // Collapsible Disclosure Widget
                    string summary = "Details";
                    if (html.Attributes.TryGetValue("summary", out string? sVal) && !string.IsNullOrWhiteSpace(sVal))
                    {
                        summary = sVal.Trim();
                    }

                    var expander = new Expander
                    {
                        IsExpanded = html.Attributes.ContainsKey("open"),
                        Background = isDark ? new SolidColorBrush(Color.FromArgb(25, 255, 255, 255)) : new SolidColorBrush(Color.FromArgb(15, 0, 0, 0)),
                        BorderBrush = borderBrush,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(0, 8, 0, 16)
                    };

                    var headerText = new TextBlock
                    {
                        Text = summary,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 14,
                        Foreground = palette?.EditorFg ?? Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    expander.Header = headerText;

                    var contentPanel = new StackPanel
                    {
                        Margin = new Thickness(0, 8, 0, 4)
                    };

                    if (html.Blocks.Count > 0)
                    {
                        foreach (var childBlock in html.Blocks)
                        {
                            var wpfBlock = convertBlock(childBlock);
                            if (wpfBlock != null)
                            {
                                // Block elements wrapped in RichTextBox / FlowDocument inside expander
                                var childDoc = new FlowDocument(wpfBlock)
                                {
                                    Background = Brushes.Transparent,
                                    Foreground = palette?.EditorFg ?? Brushes.White,
                                    PagePadding = new Thickness(0)
                                };
                                var childViewer = new FlowDocumentScrollViewer
                                {
                                    Document = childDoc,
                                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                                };
                                contentPanel.Children.Add(childViewer);
                            }
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(html.Content))
                    {
                        var contentText = new TextBlock
                        {
                            Text = html.Content.Trim(),
                            Foreground = palette?.EditorFg ?? Brushes.White,
                            FontSize = 14,
                            TextWrapping = TextWrapping.Wrap
                        };
                        contentPanel.Children.Add(contentText);
                    }

                    expander.Content = contentPanel;

                    return new BlockUIContainer(expander)
                    {
                        Tag = new HtmlBlockTag { RawHtml = html.RawHtml, Tag = "details" }
                    };
                }

                case "p":
                {
                    var p = new Paragraph
                    {
                        Margin = new Thickness(0, 0, 0, 14),
                        LineHeight = 24,
                        Tag = new HtmlBlockTag { RawHtml = html.RawHtml, Tag = "p" }
                    };

                    if (html.Attributes.TryGetValue("align", out string? align))
                    {
                        p.TextAlignment = align.ToLowerInvariant() switch
                        {
                            "center" => TextAlignment.Center,
                            "right" => TextAlignment.Right,
                            "justify" => TextAlignment.Justify,
                            _ => TextAlignment.Left
                        };
                    }

                    if (!string.IsNullOrWhiteSpace(html.Content))
                    {
                        p.Inlines.Add(new Run(html.Content) { Foreground = palette?.EditorFg ?? Brushes.White });
                    }

                    return p;
                }

                case "div":
                case "section":
                {
                    var section = new Section
                    {
                        Margin = new Thickness(0, 4, 0, 12),
                        Tag = new HtmlBlockTag { RawHtml = html.RawHtml, Tag = tag }
                    };

                    if (html.Blocks.Count > 0)
                    {
                        foreach (var b in html.Blocks)
                        {
                            var cb = convertBlock(b);
                            if (cb != null) section.Blocks.Add(cb);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(html.Content))
                    {
                        var p = new Paragraph(new Run(html.Content) { Foreground = palette?.EditorFg ?? Brushes.White })
                        {
                            Margin = new Thickness(0, 0, 0, 8)
                        };
                        section.Blocks.Add(p);
                    }

                    return section;
                }

                case "hr":
                {
                    var line = new Border
                    {
                        Height = 1,
                        Background = borderBrush,
                        Margin = new Thickness(0, 16, 0, 16),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Tag = "hr"
                    };
                    return new BlockUIContainer(line)
                    {
                        Tag = new HtmlBlockTag { RawHtml = html.RawHtml, Tag = "hr" }
                    };
                }

                default:
                {
                    // Generic block
                    var section = new Section
                    {
                        Tag = new HtmlBlockTag { RawHtml = html.RawHtml, Tag = tag }
                    };
                    if (html.Blocks.Count > 0)
                    {
                        foreach (var b in html.Blocks)
                        {
                            var cb = convertBlock(b);
                            if (cb != null) section.Blocks.Add(cb);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(html.Content))
                    {
                        section.Blocks.Add(new Paragraph(new Run(html.Content) { Foreground = palette?.EditorFg ?? Brushes.White }));
                    }
                    else
                    {
                        section.Blocks.Add(new Paragraph(new Run(html.RawHtml) { Foreground = palette?.MutedFg ?? Brushes.Gray }));
                    }
                    return section;
                }
            }
        }
    }
}
