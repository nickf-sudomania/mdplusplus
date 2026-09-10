using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MDPlus.Core
{
    public class MarkdownParser
    {
        private static readonly Regex FrontmatterFenceRegex = new Regex(@"^---\s*$", RegexOptions.Compiled);
        private static readonly Regex HeadingRegex = new Regex(@"^\s{0,3}(#{1,6})\s+(.*?)(?:\s+#+)?\s*$", RegexOptions.Compiled);
        private static readonly Regex ThematicBreakRegex = new Regex(@"^(?:(?:\*\s*){3,}|(?:-\s*){3,}|(?:_\s*){3,})$", RegexOptions.Compiled);
        private static readonly Regex CodeFenceRegex = new Regex(@"^(`{3,}|~{3,})\s*([^\s`~]*)", RegexOptions.Compiled);
        private static readonly Regex CalloutRegex = new Regex(@"^\[!(NOTE|TIP|IMPORTANT|WARNING|CAUTION)\]\s*(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex UnorderedListRegex = new Regex(@"^(\s*)([-*+])\s+(.*)$", RegexOptions.Compiled);
        private static readonly Regex OrderedListRegex = new Regex(@"^(\s*)(\d+)[.)]\s+(.*)$", RegexOptions.Compiled);
        private static readonly Regex TaskCheckboxRegex = new Regex(@"^\[([ xX])\]\s+(.*)$", RegexOptions.Compiled);
        private static readonly Regex TableSeparatorRegex = new Regex(@"^\s*\|?\s*:?-+:?\s*(\|\s*:?-+:?\s*)+\|?\s*$", RegexOptions.Compiled);

        public MarkdownDocument Parse(string markdown)
        {
            var doc = new MarkdownDocument();
            if (string.IsNullOrEmpty(markdown))
            {
                return doc;
            }

            // Normalize line endings to \n
            string normalized = markdown.Replace("\r\n", "\n").Replace('\r', '\n');
            string[] lines = normalized.Split('\n');

            // Count words and characters
            CalculateStats(lines, doc);

            int currentLine = 0;

            // 1. Check for YAML Frontmatter
            if (lines.Length > 0 && FrontmatterFenceRegex.IsMatch(lines[0]))
            {
                var fm = ParseFrontmatter(lines, ref currentLine);
                if (fm != null)
                {
                    doc.Frontmatter = fm.Metadata;
                    doc.Blocks.Add(fm);
                    if (fm.Metadata.TryGetValue("title", out string? val) && !string.IsNullOrWhiteSpace(val))
                    {
                        doc.Title = val.Trim('"', '\'');
                    }
                }
            }

            // 2. Parse blocks
            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    currentLine++;
                    continue;
                }

                // A0. Math Display Block ($$...$$)
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("$$"))
                {
                    if (trimmedLine.Length > 2 && trimmedLine.EndsWith("$$"))
                    {
                        string expr = trimmedLine.Substring(2, trimmedLine.Length - 4).Trim();
                        doc.Blocks.Add(new MathBlock(expr));
                        currentLine++;
                        continue;
                    }
                    else
                    {
                        currentLine++;
                        var mathSb = new StringBuilder();
                        while (currentLine < lines.Length)
                        {
                            string mLine = lines[currentLine];
                            if (mLine.Trim().StartsWith("$$"))
                            {
                                currentLine++;
                                break;
                            }
                            mathSb.AppendLine(mLine);
                            currentLine++;
                        }
                        doc.Blocks.Add(new MathBlock(mathSb.ToString().TrimEnd('\r', '\n')));
                        continue;
                    }
                }

                // A0.1 HTML Block (<details>, <div>, <p>, <hr>, etc.)
                if (IsHtmlBlockStarter(line))
                {
                    var htmlBlock = ParseHtmlBlock(lines, ref currentLine);
                    if (htmlBlock != null)
                    {
                        doc.Blocks.Add(htmlBlock);
                        continue;
                    }
                }

                // A. Fenced Code Block
                var codeMatch = CodeFenceRegex.Match(line.TrimStart());
                if (codeMatch.Success)
                {
                    string fence = codeMatch.Groups[1].Value;
                    string lang = codeMatch.Groups[2].Value.Trim();
                    currentLine++;

                    var sb = new StringBuilder();
                    while (currentLine < lines.Length)
                    {
                        string cLine = lines[currentLine];
                        if (cLine.TrimStart().StartsWith(fence))
                        {
                            currentLine++;
                            break;
                        }
                        sb.AppendLine(cLine);
                        currentLine++;
                    }

                    doc.Blocks.Add(new CodeBlock
                    {
                        Language = lang,
                        Code = sb.ToString().TrimEnd('\r', '\n')
                    });
                    continue;
                }

                // B. Heading (# ...)
                var headingMatch = HeadingRegex.Match(line);
                if (headingMatch.Success)
                {
                    int level = headingMatch.Groups[1].Value.Length;
                    string headingText = headingMatch.Groups[2].Value.Trim();
                    string anchor = GenerateAnchor(headingText);

                    var headingBlock = new HeadingBlock
                    {
                        Level = level,
                        Text = headingText,
                        Anchor = anchor,
                        LineIndex = currentLine,
                        Inlines = ParseInlines(headingText)
                    };
                    doc.Blocks.Add(headingBlock);

                    if (string.IsNullOrEmpty(doc.Title) && level == 1)
                    {
                        doc.Title = headingText;
                    }

                    currentLine++;
                    continue;
                }

                // C. Setext Heading (Title followed by === or ---)
                bool isNonParagraphLine = UnorderedListRegex.IsMatch(line) ||
                                          OrderedListRegex.IsMatch(line) ||
                                          line.TrimStart().StartsWith(">") ||
                                          line.TrimStart().StartsWith("$$") ||
                                          IsHtmlBlockStarter(line) ||
                                          CodeFenceRegex.IsMatch(line.TrimStart()) ||
                                          ThematicBreakRegex.IsMatch(line.Trim());

                if (!isNonParagraphLine && currentLine + 1 < lines.Length)
                {
                    string nextLine = lines[currentLine + 1];
                    if (Regex.IsMatch(nextLine, @"^\s{0,3}={3,}\s*$"))
                    {
                        string headingText = line.Trim();
                        doc.Blocks.Add(new HeadingBlock
                        {
                            Level = 1,
                            Text = headingText,
                            Anchor = GenerateAnchor(headingText),
                            LineIndex = currentLine,
                            Inlines = ParseInlines(headingText)
                        });
                        if (string.IsNullOrEmpty(doc.Title))
                        {
                            doc.Title = headingText;
                        }
                        currentLine += 2;
                        continue;
                    }
                    else if (Regex.IsMatch(nextLine, @"^\s{0,3}-{3,}\s*$") && !ThematicBreakRegex.IsMatch(line))
                    {
                        string headingText = line.Trim();
                        doc.Blocks.Add(new HeadingBlock
                        {
                            Level = 2,
                            Text = headingText,
                            Anchor = GenerateAnchor(headingText),
                            LineIndex = currentLine,
                            Inlines = ParseInlines(headingText)
                        });
                        currentLine += 2;
                        continue;
                    }
                }

                // D. Thematic Break (---, ***, ___)
                if (ThematicBreakRegex.IsMatch(line.Trim()))
                {
                    doc.Blocks.Add(new ThematicBreakBlock());
                    currentLine++;
                    continue;
                }

                // E. Table (GFM)
                if (line.Contains("|") && currentLine + 1 < lines.Length && TableSeparatorRegex.IsMatch(lines[currentLine + 1]))
                {
                    var tableBlock = ParseTable(lines, ref currentLine);
                    if (tableBlock != null)
                    {
                        doc.Blocks.Add(tableBlock);
                        continue;
                    }
                }

                // F. Blockquote (> ...)
                if (line.TrimStart().StartsWith(">"))
                {
                    var blockquote = ParseBlockquote(lines, ref currentLine);
                    if (blockquote != null)
                    {
                        doc.Blocks.Add(blockquote);
                        continue;
                    }
                }

                // G. List (Unordered or Ordered)
                if (UnorderedListRegex.IsMatch(line) || OrderedListRegex.IsMatch(line))
                {
                    var listBlock = ParseList(lines, ref currentLine);
                    if (listBlock != null)
                    {
                        doc.Blocks.Add(listBlock);
                        continue;
                    }
                }

                // H. Regular Paragraph
                var paraBlock = ParseParagraph(lines, ref currentLine);
                if (paraBlock != null)
                {
                    doc.Blocks.Add(paraBlock);
                }
            }

            return doc;
        }

        private FrontmatterBlock? ParseFrontmatter(string[] lines, ref int currentLine)
        {
            currentLine++; // skip opening ---
            var sb = new StringBuilder();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];
                if (FrontmatterFenceRegex.IsMatch(line))
                {
                    currentLine++;
                    return new FrontmatterBlock
                    {
                        Metadata = dict,
                        RawYaml = sb.ToString()
                    };
                }

                sb.AppendLine(line);
                int colonIdx = line.IndexOf(':');
                if (colonIdx > 0)
                {
                    string key = line.Substring(0, colonIdx).Trim();
                    string val = line.Substring(colonIdx + 1).Trim();
                    dict[key] = val;
                }
                currentLine++;
            }

            return null; // Unterminated frontmatter
        }

        private TableBlock? ParseTable(string[] lines, ref int currentLine)
        {
            string headerLine = lines[currentLine];
            string separatorLine = lines[currentLine + 1];

            var headerCells = SplitTableRow(headerLine);
            var alignments = ParseTableAlignments(separatorLine, headerCells.Count);

            var table = new TableBlock { Alignments = alignments };
            foreach (var cellText in headerCells)
            {
                table.Header.Cells.Add(new TableCell
                {
                    Text = cellText,
                    Inlines = ParseInlines(cellText)
                });
            }

            currentLine += 2;

            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("|"))
                {
                    break;
                }

                var rowCells = SplitTableRow(line);
                var row = new TableRow();
                for (int i = 0; i < alignments.Count; i++)
                {
                    string text = i < rowCells.Count ? rowCells[i] : string.Empty;
                    row.Cells.Add(new TableCell
                    {
                        Text = text,
                        Inlines = ParseInlines(text)
                    });
                }
                table.Rows.Add(row);
                currentLine++;
            }

            return table;
        }

        private List<string> SplitTableRow(string line)
        {
            var cells = new List<string>();
            string trimmed = line.Trim();
            if (trimmed.StartsWith("|")) trimmed = trimmed.Substring(1);
            if (trimmed.EndsWith("|"))
            {
                int slashCount = 0;
                int idx = trimmed.Length - 2;
                while (idx >= 0 && trimmed[idx] == '\\')
                {
                    slashCount++;
                    idx--;
                }
                if (slashCount % 2 == 0)
                {
                    trimmed = trimmed.Substring(0, trimmed.Length - 1);
                }
            }

            var sb = new StringBuilder();

            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];
                if (c == '\\' && i + 1 < trimmed.Length && trimmed[i + 1] == '|')
                {
                    sb.Append('|');
                    i++; // Skip the escaped pipe
                }
                else if (c == '|')
                {
                    cells.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            cells.Add(sb.ToString().Trim());
            return cells;
        }

        private List<ColumnAlignment> ParseTableAlignments(string separatorLine, int minColumns)
        {
            var aligns = new List<ColumnAlignment>();
            var parts = SplitTableRow(separatorLine);

            foreach (var p in parts)
            {
                string s = p.Trim();
                if (!s.Contains("-"))
                {
                    aligns.Add(ColumnAlignment.Left);
                    continue;
                }
                bool left = s.StartsWith(":");
                bool right = s.EndsWith(":");
                if (left && right) aligns.Add(ColumnAlignment.Center);
                else if (right) aligns.Add(ColumnAlignment.Right);
                else aligns.Add(ColumnAlignment.Left);
            }

            while (aligns.Count < minColumns)
            {
                aligns.Add(ColumnAlignment.Left);
            }

            return aligns;
        }

        private BlockquoteBlock ParseBlockquote(string[] lines, ref int currentLine)
        {
            var quote = new BlockquoteBlock();
            var quoteLines = new List<string>();

            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith(">"))
                {
                    string content = trimmed.Substring(1);
                    if (content.StartsWith(" ")) content = content.Substring(1);
                    quoteLines.Add(content);
                    currentLine++;
                }
                else
                {
                    break;
                }
            }

            // Check if first line is a callout / alert e.g. [!NOTE], [!TIP], etc.
            if (quoteLines.Count > 0)
            {
                var calloutMatch = CalloutRegex.Match(quoteLines[0].Trim());
                if (calloutMatch.Success)
                {
                    string alertType = calloutMatch.Groups[1].Value.ToUpperInvariant();
                    string customTitle = calloutMatch.Groups[2].Value.Trim();

                    quote.Callout = alertType switch
                    {
                        "NOTE" => CalloutType.Note,
                        "TIP" => CalloutType.Tip,
                        "IMPORTANT" => CalloutType.Important,
                        "WARNING" => CalloutType.Warning,
                        "CAUTION" => CalloutType.Caution,
                        _ => CalloutType.Note
                    };

                    quote.CalloutTitle = string.IsNullOrEmpty(customTitle) ? alertType : customTitle;
                    quoteLines.RemoveAt(0);
                }
            }

            // Parse inner blocks recursively
            string innerMarkdown = string.Join("\n", quoteLines);
            var innerParser = new MarkdownParser();
            var innerDoc = innerParser.Parse(innerMarkdown);
            quote.Blocks.AddRange(innerDoc.Blocks);

            return quote;
        }

        private ListBlock ParseList(string[] lines, ref int currentLine)
        {
            Match firstOrderedMatch = OrderedListRegex.Match(lines[currentLine]);
            bool isOrdered = firstOrderedMatch.Success;
            int startNum = 1;
            if (isOrdered && int.TryParse(firstOrderedMatch.Groups[2].Value, out int num))
            {
                startNum = num;
            }
            var list = new ListBlock { IsOrdered = isOrdered, StartNumber = startNum };

            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (currentLine + 1 < lines.Length &&
                        (UnorderedListRegex.IsMatch(lines[currentLine + 1]) || OrderedListRegex.IsMatch(lines[currentLine + 1])))
                    {
                        currentLine++;
                        continue;
                    }
                    break;
                }

                Match match = isOrdered ? OrderedListRegex.Match(line) : UnorderedListRegex.Match(line);
                if (!match.Success)
                {
                    // If not matching, maybe list is over or it's an alternate list type
                    if (UnorderedListRegex.IsMatch(line) || OrderedListRegex.IsMatch(line))
                    {
                        break; // switch list types
                    }
                    break;
                }

                string content = isOrdered ? match.Groups[3].Value : match.Groups[3].Value;
                currentLine++;

                // Multi-line list item continuation
                var itemLines = new List<string> { content };
                while (currentLine < lines.Length)
                {
                    string next = lines[currentLine];
                    if (string.IsNullOrWhiteSpace(next)) break;
                    if (UnorderedListRegex.IsMatch(next) || OrderedListRegex.IsMatch(next)) break;
                    if (next.StartsWith("    ") || next.StartsWith("\t") || next.StartsWith("  "))
                    {
                        itemLines.Add(next.Trim());
                        currentLine++;
                    }
                    else
                    {
                        break;
                    }
                }

                string fullItemText = string.Join(" ", itemLines);
                var listItem = new ListItemBlock();

                // Check for Task Checkbox: [ ] or [x]
                var taskMatch = TaskCheckboxRegex.Match(fullItemText);
                if (taskMatch.Success)
                {
                    listItem.IsTask = true;
                    listItem.IsChecked = taskMatch.Groups[1].Value.Equals("x", StringComparison.OrdinalIgnoreCase);
                    fullItemText = taskMatch.Groups[2].Value;
                }

                listItem.Inlines = ParseInlines(fullItemText);
                list.Items.Add(listItem);
            }

            return list;
        }

        private ParagraphBlock ParseParagraph(string[] lines, ref int currentLine)
        {
            var paraLines = new List<string>();

            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];
                if (string.IsNullOrWhiteSpace(line))
                {
                    currentLine++;
                    break;
                }

                // Check for block starters that interrupt a paragraph
                if (HeadingRegex.IsMatch(line) ||
                    ThematicBreakRegex.IsMatch(line.Trim()) ||
                    CodeFenceRegex.IsMatch(line.TrimStart()) ||
                    line.TrimStart().StartsWith(">") ||
                    line.TrimStart().StartsWith("$$") ||
                    IsHtmlBlockStarter(line) ||
                    UnorderedListRegex.IsMatch(line) ||
                    OrderedListRegex.IsMatch(line) ||
                    (line.Contains("|") && currentLine + 1 < lines.Length && TableSeparatorRegex.IsMatch(lines[currentLine + 1])))
                {
                    break;
                }

                // If this line is followed by a Setext heading underline, break so Setext heading can be parsed
                if (currentLine + 1 < lines.Length)
                {
                    string next = lines[currentLine + 1];
                    if (Regex.IsMatch(next, @"^\s{0,3}={3,}\s*$") ||
                        (Regex.IsMatch(next, @"^\s{0,3}-{3,}\s*$") && !ThematicBreakRegex.IsMatch(line.Trim())))
                    {
                        break;
                    }
                }

                paraLines.Add(line);
                currentLine++;
            }

            string combined = string.Join("\n", paraLines);
            return new ParagraphBlock
            {
                Inlines = ParseInlines(combined)
            };
        }

        public List<MarkdownInline> ParseInlines(string text)
        {
            var inlines = new List<MarkdownInline>();
            if (string.IsNullOrEmpty(text)) return inlines;

            int i = 0;
            int length = text.Length;
            var sb = new StringBuilder();

            void FlushText()
            {
                if (sb.Length > 0)
                {
                    inlines.Add(new TextInline(sb.ToString()));
                    sb.Clear();
                }
            }

            bool lastCharWasEscapedBackslash = false;

            while (i < length)
            {
                char c = text[i];

                // 1. Escaped characters: \* \_ \` etc.
                if (c == '\\' && i + 1 < length)
                {
                    char next = text[i + 1];
                    if (next == '\\')
                    {
                        sb.Append('\\');
                        lastCharWasEscapedBackslash = true;
                        i += 2;
                        continue;
                    }
                    else if ("`*_{}[]()#+-.!|~=$".IndexOf(next) >= 0)
                    {
                        sb.Append(next);
                        lastCharWasEscapedBackslash = false;
                        i += 2;
                        continue;
                    }
                }

                // 2. Line Breaks: \n or \r\n
                if (c == '\n')
                {
                    bool isHard = false;
                    if (sb.Length >= 1 && sb[sb.Length - 1] == '\\' && !lastCharWasEscapedBackslash)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        isHard = true;
                    }
                    else if (sb.Length >= 2 && sb[sb.Length - 1] == ' ' && sb[sb.Length - 2] == ' ')
                    {
                        while (sb.Length > 0 && sb[sb.Length - 1] == ' ')
                        {
                            sb.Remove(sb.Length - 1, 1);
                        }
                        isHard = true;
                    }
                    FlushText();
                    inlines.Add(new LineBreakInline(isHard));
                    lastCharWasEscapedBackslash = false;
                    i++;
                    continue;
                }

                lastCharWasEscapedBackslash = false;

                // 2.1 Math: $$display math$$ or $inline math$
                if (c == '$')
                {
                    if (i + 1 < length && text[i + 1] == '$')
                    {
                        int end = text.IndexOf("$$", i + 2, StringComparison.Ordinal);
                        if (end > i + 1)
                        {
                            FlushText();
                            string expr = text.Substring(i + 2, end - (i + 2));
                            inlines.Add(new MathInline(expr, isDisplay: true));
                            i = end + 2;
                            continue;
                        }
                    }
                    else if (i + 1 < length && !char.IsWhiteSpace(text[i + 1]) && text[i + 1] != '$')
                    {
                        int end = -1;
                        for (int k = i + 1; k < length; k++)
                        {
                            if (text[k] == '\\') { k++; continue; }
                            if (text[k] == '$')
                            {
                                if (!char.IsWhiteSpace(text[k - 1]))
                                {
                                    if (k + 1 >= length || !char.IsDigit(text[k + 1]))
                                    {
                                        if (k + 1 >= length || text[k + 1] != '$')
                                        {
                                            end = k;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (text[k] == '\n' && k + 1 < length && text[k + 1] == '\n')
                            {
                                break;
                            }
                        }

                        if (end > i)
                        {
                            FlushText();
                            string expr = text.Substring(i + 1, end - (i + 1));
                            inlines.Add(new MathInline(expr, isDisplay: false));
                            i = end + 1;
                            continue;
                        }
                    }
                }

                // 3. Inline Code: `code` or ``code``
                if (c == '`')
                {
                    int count = 0;
                    while (i + count < length && text[i + count] == '`') count++;

                    int end = -1;
                    int k = i + count;
                    while (k < length)
                    {
                        if (text[k] == '`')
                        {
                            int runLen = 0;
                            while (k + runLen < length && text[k + runLen] == '`') runLen++;
                            if (runLen == count)
                            {
                                end = k;
                                break;
                            }
                            k += runLen;
                        }
                        else
                        {
                            k++;
                        }
                    }

                    if (end > i)
                    {
                        FlushText();
                        string codeContent = text.Substring(i + count, end - (i + count));
                        if (codeContent.StartsWith(" ") && codeContent.EndsWith(" ") && codeContent.Trim().Length > 0)
                        {
                            codeContent = codeContent.Substring(1, codeContent.Length - 2);
                        }
                        inlines.Add(new CodeInline(codeContent));
                        i = end + count;
                        continue;
                    }
                }

                // 4. Image: ![alt](url "title")
                if (c == '!' && i + 1 < length && text[i + 1] == '[')
                {
                    int closingBracket = FindClosingBracket(text, i + 1);
                    if (closingBracket > i + 1 && closingBracket + 1 < length && text[closingBracket + 1] == '(')
                    {
                        int closingParen = FindMatchingClosingParen(text, closingBracket + 1);
                        if (closingParen > closingBracket + 1)
                        {
                            FlushText();
                            string alt = text.Substring(i + 2, closingBracket - (i + 2));
                            string linkPart = text.Substring(closingBracket + 2, closingParen - (closingBracket + 2)).Trim();
                            string url = linkPart;
                            string title = string.Empty;

                            if (url.StartsWith("<"))
                            {
                                int closeAngle = url.IndexOf('>');
                                if (closeAngle > 0)
                                {
                                    string innerUrl = url.Substring(1, closeAngle - 1);
                                    string remainder = url.Substring(closeAngle + 1).Trim();
                                    url = innerUrl;
                                    if (!string.IsNullOrEmpty(remainder))
                                    {
                                        title = remainder.Trim('"', '\'');
                                    }
                                }
                            }
                            else
                            {
                                int spaceIdx = linkPart.IndexOf(' ');
                                if (spaceIdx > 0)
                                {
                                    url = linkPart.Substring(0, spaceIdx).Trim();
                                    title = linkPart.Substring(spaceIdx + 1).Trim('"', '\'');
                                }
                            }

                            inlines.Add(new ImageInline
                            {
                                AltText = alt,
                                Url = url,
                                Title = title
                            });
                            i = closingParen + 1;
                            continue;
                        }
                    }
                }

                // 5. Link: [text](url "title")
                if (c == '[')
                {
                    int closingBracket = FindClosingBracket(text, i);
                    if (closingBracket > i && closingBracket + 1 < length && text[closingBracket + 1] == '(')
                    {
                        int closingParen = FindMatchingClosingParen(text, closingBracket + 1);
                        if (closingParen > closingBracket + 1)
                        {
                            FlushText();
                            string linkText = text.Substring(i + 1, closingBracket - (i + 1));
                            string linkPart = text.Substring(closingBracket + 2, closingParen - (closingBracket + 2)).Trim();
                            string url = linkPart;
                            string title = string.Empty;

                            if (url.StartsWith("<"))
                            {
                                int closeAngle = url.IndexOf('>');
                                if (closeAngle > 0)
                                {
                                    string innerUrl = url.Substring(1, closeAngle - 1);
                                    string remainder = url.Substring(closeAngle + 1).Trim();
                                    url = innerUrl;
                                    if (!string.IsNullOrEmpty(remainder))
                                    {
                                        title = remainder.Trim('"', '\'');
                                    }
                                }
                            }
                            else
                            {
                                int spaceIdx = linkPart.IndexOf(' ');
                                if (spaceIdx > 0)
                                {
                                    url = linkPart.Substring(0, spaceIdx).Trim();
                                    title = linkPart.Substring(spaceIdx + 1).Trim('"', '\'');
                                }
                            }

                            var linkInline = new LinkInline
                            {
                                Url = url,
                                Title = title,
                                Children = ParseInlines(linkText)
                            };
                            inlines.Add(linkInline);
                            i = closingParen + 1;
                            continue;
                        }
                    }
                }

                // 6. Autolink: <https://...> or <user@example.com> or HTML comments <!-- -->
                if (c == '<')
                {
                    if (i + 3 < length && text.Substring(i, 4) == "<!--")
                    {
                        int endComment = text.IndexOf("-->", i + 4, StringComparison.Ordinal);
                        if (endComment > i)
                        {
                            i = endComment + 3;
                            continue;
                        }
                    }

                    int closeTag = text.IndexOf('>', i + 1);
                    if (closeTag > i)
                    {
                        string inner = text.Substring(i + 1, closeTag - i - 1).Trim();
                        if (inner.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            inner.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                            inner.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                        {
                            FlushText();
                            var link = new LinkInline
                            {
                                Url = inner,
                                Children = new List<MarkdownInline> { new TextInline(inner) }
                            };
                            inlines.Add(link);
                            i = closeTag + 1;
                            continue;
                        }
                        else if (inner.Contains("@") && !inner.Contains(" ") && inner.Contains("."))
                        {
                            FlushText();
                            var link = new LinkInline
                            {
                                Url = "mailto:" + inner,
                                Children = new List<MarkdownInline> { new TextInline(inner) }
                            };
                            inlines.Add(link);
                            i = closeTag + 1;
                            continue;
                        }
                        else
                        {
                            var htmlInline = TryParseHtmlInline(text, i, closeTag, out int nextIdx);
                            if (htmlInline != null)
                            {
                                FlushText();
                                inlines.Add(htmlInline);
                                i = nextIdx;
                                continue;
                            }
                        }
                    }
                }

                // 7. Bold & Italic: *** or ___
                if ((c == '*' || c == '_') && i + 2 < length && text[i + 1] == c && text[i + 2] == c)
                {
                    // Opening delimiter must not be followed by whitespace
                    if (i + 3 < length && !char.IsWhiteSpace(text[i + 3]))
                    {
                        string delimiter = new string(c, 3);
                        int end = FindDelimiterEnd(text, delimiter, i + 3);
                        if (end > i)
                        {
                            FlushText();
                            string content = text.Substring(i + 3, end - i - 3);
                            inlines.Add(new BoldItalicInline(ParseInlines(content).ToArray()));
                            i = end + 3;
                            continue;
                        }
                    }
                }

                // 8. Bold: ** or __
                if ((c == '*' || c == '_') && i + 1 < length && text[i + 1] == c)
                {
                    // Opening delimiter must not be followed by whitespace
                    if (i + 2 < length && !char.IsWhiteSpace(text[i + 2]))
                    {
                        string delimiter = new string(c, 2);
                        int end = FindDelimiterEnd(text, delimiter, i + 2);
                        if (end > i)
                        {
                            FlushText();
                            string content = text.Substring(i + 2, end - i - 2);
                            inlines.Add(new BoldInline(ParseInlines(content).ToArray()));
                            i = end + 2;
                            continue;
                        }
                    }
                }

                // 9. Italic: * or _ (checking that _ is not within_word_identifier)
                if (c == '*' || c == '_')
                {
                    bool isWordCharBefore = i > 0 && char.IsLetterOrDigit(text[i - 1]);
                    // Opening delimiter must not be followed by whitespace
                    bool hasContentAfter = i + 1 < length && !char.IsWhiteSpace(text[i + 1]);
                    if (hasContentAfter && !(c == '_' && isWordCharBefore))
                    {
                        int end = FindDelimiterEnd(text, c.ToString(), i + 1);
                        if (end > i && end - i > 1)
                        {
                            bool isWordCharAfter = end + 1 < length && char.IsLetterOrDigit(text[end + 1]);
                            if (!(c == '_' && isWordCharAfter))
                            {
                                FlushText();
                                string content = text.Substring(i + 1, end - i - 1);
                                inlines.Add(new ItalicInline(ParseInlines(content).ToArray()));
                                i = end + 1;
                                continue;
                            }
                        }
                    }
                }

                // 10. Strikethrough: ~~text~~
                if (c == '~' && i + 1 < length && text[i + 1] == '~')
                {
                    if (i + 2 < length && !char.IsWhiteSpace(text[i + 2]))
                    {
                        int end = FindDelimiterEnd(text, "~~", i + 2);
                        if (end > i)
                        {
                            FlushText();
                            string content = text.Substring(i + 2, end - i - 2);
                            inlines.Add(new StrikethroughInline(ParseInlines(content).ToArray()));
                            i = end + 2;
                            continue;
                        }
                    }
                }

                // 11. Highlight: ==text==
                if (c == '=' && i + 1 < length && text[i + 1] == '=')
                {
                    if (i + 2 < length && !char.IsWhiteSpace(text[i + 2]))
                    {
                        int end = FindDelimiterEnd(text, "==", i + 2);
                        if (end > i)
                        {
                            FlushText();
                            string content = text.Substring(i + 2, end - i - 2);
                            inlines.Add(new HighlightInline(ParseInlines(content).ToArray()));
                            i = end + 2;
                            continue;
                        }
                    }
                }

                sb.Append(c);
                i++;
            }

            FlushText();
            return inlines;
        }

        private int FindDelimiterEnd(string text, string delimiter, int startIndex)
        {
            int dLen = delimiter.Length;
            char delimChar = delimiter[0];
            for (int k = startIndex; k <= text.Length - dLen; k++)
            {
                if (text[k] == '\\')
                {
                    k++; // Skip escaped character
                    continue;
                }
                if (string.CompareOrdinal(text, k, delimiter, 0, dLen) == 0)
                {
                    // Closing delimiter must not be preceded by whitespace
                    if (k > 0 && char.IsWhiteSpace(text[k - 1]))
                    {
                        // An unescaped delimiter of the exact same run length preceded by whitespace
                        // cannot close, and an emphasis span cannot leap across an invalid delimiter.
                        bool isExactRun = (k + dLen >= text.Length || text[k + dLen] != delimChar) &&
                                          (k <= startIndex || text[k - 1] != delimChar);
                        if (isExactRun)
                        {
                            return -1;
                        }
                        continue;
                    }
                    // For single/double delimiter, ensure not part of a longer run
                    if ((delimChar == '*' || delimChar == '_' || delimChar == '~' || delimChar == '=') &&
                        ((k + dLen < text.Length && text[k + dLen] == delimChar) ||
                         (k > startIndex && text[k - 1] == delimChar)))
                    {
                        continue;
                    }
                    return k;
                }
            }
            return -1;
        }

        private int FindMatchingClosingParen(string text, int openParenIndex)
        {
            int depth = 0;
            for (int k = openParenIndex; k < text.Length; k++)
            {
                if (text[k] == '\\')
                {
                    k++;
                    continue;
                }
                if (text[k] == '(') depth++;
                else if (text[k] == ')')
                {
                    depth--;
                    if (depth == 0) return k;
                }
            }
            return -1;
        }

        private int FindClosingBracket(string text, int startIndex)
        {
            int depth = 0;
            for (int k = startIndex; k < text.Length; k++)
            {
                if (text[k] == '\\')
                {
                    k++; // skip escaped
                    continue;
                }
                if (text[k] == '[') depth++;
                else if (text[k] == ']')
                {
                    depth--;
                    if (depth == 0) return k;
                }
            }
            return -1;
        }

        private string GenerateAnchor(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            string clean = Regex.Replace(text.ToLowerInvariant(), @"[^\w\s-]", "");
            return Regex.Replace(clean, @"\s+", "-");
        }

        private void CalculateStats(string[] lines, MarkdownDocument doc)
        {
            int totalChars = 0;
            int totalWords = 0;

            foreach (var line in lines)
            {
                totalChars += line.Length;
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var words = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    totalWords += words.Length;
                }
            }

            doc.CharacterCount = totalChars;
            doc.WordCount = totalWords;
        }

        private static readonly HashSet<string> KnownInlineHtmlTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "kbd", "sub", "sup", "b", "strong", "i", "em", "u", "mark", "del", "s", "strike",
            "code", "span", "font", "br", "hr", "a", "img", "abbr", "cite", "small", "var", "samp"
        };

        private static readonly HashSet<string> KnownBlockHtmlTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "details", "div", "section", "article", "header", "footer", "figure", "figcaption", "p", "table", "style", "hr"
        };

        private static readonly Regex HtmlAttrRegex = new Regex(
            @"([a-zA-Z0-9_-]+)\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>]+))",
            RegexOptions.Compiled);

        public static Dictionary<string, string> ParseHtmlAttributes(string tagContent)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(tagContent)) return dict;

            var matches = HtmlAttrRegex.Matches(tagContent);
            foreach (Match m in matches)
            {
                string key = m.Groups[1].Value;
                string val = m.Groups[2].Success ? m.Groups[2].Value :
                             m.Groups[3].Success ? m.Groups[3].Value :
                             m.Groups[4].Value;
                dict[key] = val;
            }
            return dict;
        }

        private HtmlInline? TryParseHtmlInline(string text, int startIndex, int openCloseTag, out int nextIndex)
        {
            nextIndex = startIndex;
            string tagHeader = text.Substring(startIndex + 1, openCloseTag - (startIndex + 1)).Trim();
            if (string.IsNullOrEmpty(tagHeader) || tagHeader.StartsWith("/"))
                return null;

            int sp = tagHeader.IndexOfAny(new[] { ' ', '\t', '/' });
            string tagName = (sp > 0 ? tagHeader.Substring(0, sp) : tagHeader).ToLowerInvariant();

            if (!KnownInlineHtmlTags.Contains(tagName))
                return null;

            var attrs = ParseHtmlAttributes(tagHeader);

            // Self-closing: <br/>, <hr/>, <img .../>, or void tags <br>, <hr>
            bool isSelfClosing = tagHeader.EndsWith("/") || tagName == "br" || tagName == "hr" || tagName == "img";
            if (isSelfClosing)
            {
                nextIndex = openCloseTag + 1;
                return new HtmlInline(text.Substring(startIndex, openCloseTag - startIndex + 1), tagName)
                {
                    IsSelfClosing = true,
                    Attributes = attrs
                };
            }

            // Paired tag: find </tagName>
            string closeTarget = "</" + tagName + ">";
            int closeIdx = text.IndexOf(closeTarget, openCloseTag + 1, StringComparison.OrdinalIgnoreCase);
            if (closeIdx > openCloseTag)
            {
                string inner = text.Substring(openCloseTag + 1, closeIdx - (openCloseTag + 1));
                nextIndex = closeIdx + closeTarget.Length;
                string fullRaw = text.Substring(startIndex, nextIndex - startIndex);
                return new HtmlInline(fullRaw, tagName)
                {
                    Content = inner,
                    Attributes = attrs,
                    Children = ParseInlines(inner)
                };
            }

            return null;
        }

        private static bool IsHtmlBlockStarter(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            string trimmed = line.TrimStart();
            if (!trimmed.StartsWith("<")) return false;
            if (trimmed.StartsWith("<!--") || trimmed.StartsWith("<?") || trimmed.StartsWith("<!")) return false;

            int endTag = trimmed.IndexOfAny(new[] { ' ', '>', '\t', '/' }, 1);
            if (endTag <= 1) return false;
            string tag = trimmed.Substring(1, endTag - 1);
            return KnownBlockHtmlTags.Contains(tag);
        }

        private HtmlBlock? ParseHtmlBlock(string[] lines, ref int currentLine)
        {
            string firstLine = lines[currentLine];
            string trimmed = firstLine.TrimStart();
            int endTag = trimmed.IndexOfAny(new[] { ' ', '>', '\t', '/' }, 1);
            if (endTag <= 1) return null;
            string tag = trimmed.Substring(1, endTag - 1).ToLowerInvariant();

            int closeBracket = trimmed.IndexOf('>');
            string header = closeBracket > 0 ? trimmed.Substring(1, closeBracket - 1) : tag;
            var attrs = ParseHtmlAttributes(header);

            if (tag == "hr")
            {
                currentLine++;
                return new HtmlBlock(firstLine.Trim(), "hr") { Attributes = attrs };
            }

            var sb = new StringBuilder();
            sb.AppendLine(firstLine);
            currentLine++;

            string closeTarget = "</" + tag + ">";

            // If the first line already contains the closing tag
            if (firstLine.IndexOf(closeTarget, StringComparison.OrdinalIgnoreCase) > 0)
            {
                string full = sb.ToString().TrimEnd('\r', '\n');
                return CreateHtmlBlockObject(tag, full, attrs);
            }

            while (currentLine < lines.Length)
            {
                string line = lines[currentLine];
                sb.AppendLine(line);
                currentLine++;

                if (line.IndexOf(closeTarget, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    break;
                }
            }

            string fullHtml = sb.ToString().TrimEnd('\r', '\n');
            return CreateHtmlBlockObject(tag, fullHtml, attrs);
        }

        private HtmlBlock CreateHtmlBlockObject(string tag, string fullHtml, Dictionary<string, string> attrs)
        {
            var block = new HtmlBlock(fullHtml, tag) { Attributes = attrs };

            if (tag == "details")
            {
                // Extract <summary> if present
                var summaryMatch = Regex.Match(fullHtml, @"<summary>(.*?)</summary>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (summaryMatch.Success)
                {
                    block.Attributes["summary"] = summaryMatch.Groups[1].Value.Trim();
                }

                // Inner content without <details> and </details> and <summary>
                string inner = fullHtml;
                int startTagEnd = inner.IndexOf('>');
                if (startTagEnd >= 0) inner = inner.Substring(startTagEnd + 1);
                int endTagStart = inner.LastIndexOf("</details>", StringComparison.OrdinalIgnoreCase);
                if (endTagStart >= 0) inner = inner.Substring(0, endTagStart);

                if (summaryMatch.Success)
                {
                    inner = inner.Replace(summaryMatch.Value, "");
                }

                block.Content = inner.Trim();
                if (!string.IsNullOrWhiteSpace(block.Content))
                {
                    var innerDoc = Parse(block.Content);
                    block.Blocks = innerDoc.Blocks;
                }
            }
            else
            {
                // Extract inner content
                int startTagEnd = fullHtml.IndexOf('>');
                int endTagStart = fullHtml.LastIndexOf("</" + tag + ">", StringComparison.OrdinalIgnoreCase);
                if (startTagEnd >= 0 && endTagStart > startTagEnd)
                {
                    block.Content = fullHtml.Substring(startTagEnd + 1, endTagStart - (startTagEnd + 1)).Trim();
                }
                else
                {
                    block.Content = fullHtml;
                }
            }

            return block;
        }
    }
}
