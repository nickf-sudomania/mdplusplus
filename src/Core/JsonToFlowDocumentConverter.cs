using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MDPlus.Core
{
    /// <summary>
    /// Converts JSON documents into formatted, syntax-highlighted WPF FlowDocuments.
    /// Pretty-prints with 2-space indentation via System.Text.Json, applies token
    /// highlighting matching ThemePalette across all 8 presets, and provides resilient
    /// fallback for malformed JSON without throwing unhandled exceptions.
    /// </summary>
    public static class JsonToFlowDocumentConverter
    {
        public static FlowDocument Convert(string? jsonText, ThemePalette palette)
        {
            var doc = new FlowDocument
            {
                Background = palette.EditorBg,
                Foreground = palette.EditorFg,
                FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New, monospace"),
                FontSize = 13,
                PagePadding = new Thickness(32, 24, 32, 32),
                ColumnWidth = double.PositiveInfinity
            };

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                var emptyPara = new Paragraph(new Run("Empty JSON document"))
                {
                    FontStyle = FontStyles.Italic,
                    Foreground = palette.MutedFg,
                    Margin = new Thickness(0)
                };
                doc.Blocks.Add(emptyPara);
                return doc;
            }

            string formattedJson;
            bool isMalformed = false;
            string? errorMessage = null;
            long errorLine = 0;
            long errorCol = 0;

            try
            {
                using var jsonDoc = JsonDocument.Parse(jsonText, new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                });

                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    jsonDoc.WriteTo(writer);
                }
                formattedJson = Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (JsonException ex)
            {
                isMalformed = true;
                errorMessage = ex.Message;
                errorLine = ex.LineNumber ?? 1;
                errorCol = ex.BytePositionInLine ?? 1;
                formattedJson = jsonText;
            }

            if (isMalformed)
            {
                var errorPara = new Paragraph
                {
                    Background = palette.CodeBg,
                    BorderBrush = palette.Accent,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 0, 0, 16)
                };

                errorPara.Inlines.Add(new Run("⚠️ Malformed JSON Syntax: ")
                {
                    FontWeight = FontWeights.SemiBold,
                    Foreground = palette.Accent
                });

                errorPara.Inlines.Add(new Run($"Line {errorLine}, Column {errorCol}: {errorMessage}")
                {
                    Foreground = palette.EditorFg
                });

                doc.Blocks.Add(errorPara);
            }

            // Render lines with syntax highlighting
            RenderJsonLines(doc, formattedJson, palette);

            return doc;
        }

        private const int MaxVisualLines = 2500;

        private static void RenderJsonLines(FlowDocument doc, string text, ThemePalette palette)
        {
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int count = Math.Min(lines.Length, MaxVisualLines);

            for (int i = 0; i < count; i++)
            {
                var line = lines[i];
                var p = new Paragraph
                {
                    Margin = new Thickness(0),
                    LineHeight = 20
                };

                if (line.Length == 0)
                {
                    p.Inlines.Add(new Run(string.Empty));
                    doc.Blocks.Add(p);
                    continue;
                }

                TokenizeLine(p, line, palette);
                doc.Blocks.Add(p);
            }

            if (lines.Length > MaxVisualLines)
            {
                var noticePara = new Paragraph(new Run($"Showing first {MaxVisualLines:N0} of {lines.Length:N0} lines. Switch to Raw view (Ctrl+3) to view or edit full stream."))
                {
                    FontStyle = FontStyles.Italic,
                    Foreground = palette.MutedFg,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                doc.Blocks.Add(noticePara);
            }
        }

        private static void TokenizeLine(Paragraph p, string line, ThemePalette palette)
        {
            int pos = 0;
            int len = line.Length;

            while (pos < len)
            {
                // 1. Whitespace / indentation
                if (char.IsWhiteSpace(line[pos]))
                {
                    int start = pos;
                    while (pos < len && char.IsWhiteSpace(line[pos])) pos++;
                    p.Inlines.Add(new Run(line.Substring(start, pos - start)) { Foreground = palette.EditorFg });
                    continue;
                }

                char c = line[pos];

                // 2. String literal: "..."
                if (c == '"')
                {
                    int start = pos;
                    pos++; // skip opening quote
                    while (pos < len)
                    {
                        if (line[pos] == '\\')
                        {
                            pos += 2; // skip escape sequence
                            if (pos > len) pos = len;
                        }
                        else if (line[pos] == '"')
                        {
                            pos++; // include closing quote
                            break;
                        }
                        else
                        {
                            pos++;
                        }
                    }

                    string str = line.Substring(start, pos - start);

                    // Check if key (followed by ':')
                    int peek = pos;
                    while (peek < len && char.IsWhiteSpace(line[peek])) peek++;
                    bool isKey = peek < len && line[peek] == ':';

                    p.Inlines.Add(new Run(str)
                    {
                        Foreground = isKey ? palette.SyntaxProperty : palette.SyntaxString
                    });
                    continue;
                }

                // 3. Numbers
                if (char.IsDigit(c) || (c == '-' && pos + 1 < len && char.IsDigit(line[pos + 1])))
                {
                    int start = pos;
                    pos++;
                    while (pos < len && (char.IsDigit(line[pos]) || line[pos] == '.' || line[pos] == 'e' || line[pos] == 'E' || line[pos] == '+' || line[pos] == '-'))
                    {
                        pos++;
                    }
                    string num = line.Substring(start, pos - start);
                    p.Inlines.Add(new Run(num) { Foreground = palette.SyntaxNumber });
                    continue;
                }

                // 4. Keywords (true, false, null)
                if (char.IsLetter(c))
                {
                    int start = pos;
                    while (pos < len && char.IsLetter(line[pos])) pos++;
                    string word = line.Substring(start, pos - start);
                    if (word is "true" or "false" or "null")
                    {
                        p.Inlines.Add(new Run(word) { Foreground = palette.SyntaxKeyword });
                    }
                    else
                    {
                        p.Inlines.Add(new Run(word) { Foreground = palette.EditorFg });
                    }
                    continue;
                }

                // 5. Punctuation / structural symbols ({ } [ ] : ,)
                p.Inlines.Add(new Run(c.ToString()) { Foreground = palette.EditorFg });
                pos++;
            }
        }
    }
}
