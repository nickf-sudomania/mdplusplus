using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;
using WpfList = System.Windows.Documents.List;
using WpfListItem = System.Windows.Documents.ListItem;

namespace MDPlus.Core
{
    public class HeadingTag
    {
        public int Level { get; set; } = 1;
        public string Anchor { get; set; } = string.Empty;
    }

    public class CodeBlockTag
    {
        public string Language { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class CalloutTag
    {
        public CalloutType Type { get; set; } = CalloutType.Note;
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// Serializes a WPF FlowDocument back into standard CommonMark / GFM markdown.
    /// Supports headings (H1-H6), inlines (bold, italic, strikethrough, highlight, code, links, images),
    /// fenced code blocks, blockquotes, callouts, lists, task checklists, tables, and thematic breaks.
    /// </summary>
    public struct InlineContext
    {
        public bool InHeading;
        public bool InBold;
        public bool InItalic;
        public bool InStrike;
        public bool InHighlight;
        public bool InCode;
    }

    /// <summary>
    /// Serializes a WPF FlowDocument back into standard CommonMark / GFM markdown.
    /// Supports headings (H1-H6), inlines (bold, italic, strikethrough, highlight, code, links, images),
    /// fenced code blocks, blockquotes, callouts, lists, task checklists, tables, and thematic breaks.
    /// </summary>
    public static class MarkdownSerializer
    {
        private static readonly Regex HeadingTagRegex = new Regex(@"^(?:h|header|heading)-?([1-6])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [ThreadStatic]
        private static ScratchPool? t_scratch;

        private sealed class ScratchPool
        {
            public readonly StringBuilder[] Buffers = new StringBuilder[16]
            {
                new StringBuilder(2048),
                new StringBuilder(2048),
                new StringBuilder(2048),
                new StringBuilder(2048),
                new StringBuilder(1024),
                new StringBuilder(1024),
                new StringBuilder(1024),
                new StringBuilder(1024),
                new StringBuilder(512),
                new StringBuilder(512),
                new StringBuilder(512),
                new StringBuilder(512),
                new StringBuilder(512),
                new StringBuilder(512),
                new StringBuilder(512),
                new StringBuilder(512)
            };
            public int Top = 0;
        }

        private static StringBuilder RentScratch()
        {
            t_scratch ??= new ScratchPool();
            if (t_scratch.Top < t_scratch.Buffers.Length)
            {
                var sb = t_scratch.Buffers[t_scratch.Top++];
                sb.Clear();
                return sb;
            }
            return new StringBuilder(512);
        }

        private static void ReturnScratch(StringBuilder sb)
        {
            if (t_scratch != null && t_scratch.Top > 0)
            {
                if (t_scratch.Top - 1 < t_scratch.Buffers.Length && ReferenceEquals(t_scratch.Buffers[t_scratch.Top - 1], sb))
                {
                    sb.Clear();
                    t_scratch.Top--;
                    return;
                }
            }
            sb.Clear();
        }

        /// <summary>
        /// Serializes an entire FlowDocument into standard CommonMark / GFM markdown.
        /// </summary>
        public static string Serialize(FlowDocument doc)
        {
            if (doc == null || doc.Blocks == null || doc.Blocks.Count == 0)
                return string.Empty;

            int estimatedCapacity = Math.Clamp(doc.Blocks.Count * 128, 4096, 1024 * 1024);
            var sb = new StringBuilder(estimatedCapacity);
            SerializeBlocks(doc.Blocks, sb);
            if (sb.Length == 0)
                return string.Empty;

            sb.Append('\n');
            return sb.ToString();
        }

        /// <summary>
        /// Serializes a BlockCollection into markdown text separated by double newlines.
        /// </summary>
        public static string SerializeBlocks(BlockCollection blocks)
        {
            if (blocks == null || blocks.Count == 0) return string.Empty;
            var sb = new StringBuilder(blocks.Count * 128);
            SerializeBlocks(blocks, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Serializes an enumerable of Blocks into markdown text separated by double newlines.
        /// </summary>
        public static string SerializeBlocks(IEnumerable<Block> blocks)
        {
            if (blocks == null) return string.Empty;
            if (blocks is BlockCollection bc)
            {
                return SerializeBlocks(bc);
            }
            var sb = new StringBuilder(1024);
            SerializeBlocks(blocks, sb);
            return sb.ToString();
        }

        private static void SerializeBlocks(BlockCollection blocks, StringBuilder sb)
        {
            if (blocks == null || blocks.Count == 0) return;
            bool firstBlock = true;

            for (Block? cur = blocks.FirstBlock; cur != null; cur = cur.NextBlock)
            {
                int startLen = sb.Length;
                if (!firstBlock)
                {
                    sb.Append("\n\n");
                }
                int blockStart = sb.Length;
                SerializeBlock(cur, sb);

                // Check if block was empty or pure whitespace
                int blockEnd = sb.Length;
                while (blockEnd > blockStart && char.IsWhiteSpace(sb[blockEnd - 1]))
                {
                    blockEnd--;
                }

                if (blockEnd == blockStart)
                {
                    sb.Length = startLen;
                }
                else
                {
                    while (sb.Length > blockStart && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
                    {
                        sb.Length--;
                    }
                    firstBlock = false;
                }
            }
        }

        private static void SerializeBlocks(IEnumerable<Block> blocks, StringBuilder sb)
        {
            if (blocks == null) return;
            if (blocks is BlockCollection bc)
            {
                SerializeBlocks(bc, sb);
                return;
            }

            bool firstBlock = true;
            foreach (var block in blocks)
            {
                int startLen = sb.Length;
                if (!firstBlock)
                {
                    sb.Append("\n\n");
                }
                int blockStart = sb.Length;
                SerializeBlock(block, sb);

                // Check if block was empty or pure whitespace
                int blockEnd = sb.Length;
                while (blockEnd > blockStart && char.IsWhiteSpace(sb[blockEnd - 1]))
                {
                    blockEnd--;
                }

                if (blockEnd == blockStart)
                {
                    sb.Length = startLen;
                }
                else
                {
                    while (sb.Length > blockStart && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
                    {
                        sb.Length--;
                    }
                    firstBlock = false;
                }
            }
        }

        /// <summary>
        /// Serializes a single WPF Block into markdown.
        /// </summary>
        public static string SerializeBlock(Block block)
        {
            if (block == null) return string.Empty;
            var sb = new StringBuilder();
            SerializeBlock(block, sb);
            return sb.ToString();
        }

        private static void SerializeBlock(Block block, StringBuilder sb)
        {
            if (block == null) return;

            switch (block)
            {
                case Paragraph p:
                    SerializeParagraph(p, sb);
                    break;

                case Section s:
                    SerializeSection(s, sb);
                    break;

                case WpfList l:
                    SerializeList(l, sb);
                    break;

                case WpfTable t:
                    SerializeTable(t, sb);
                    break;

                case BlockUIContainer buic:
                    SerializeBlockUIContainer(buic, sb);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Serializes an InlineCollection into markdown string.
        /// </summary>
        public static string SerializeInlines(InlineCollection inlines)
        {
            if (inlines == null || inlines.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            SerializeInlines(inlines, sb, default);
            return sb.ToString();
        }

        /// <summary>
        /// Serializes an enumerable of Inlines into markdown string.
        /// </summary>
        public static string SerializeInlines(IEnumerable<Inline> inlines)
        {
            if (inlines == null) return string.Empty;
            var sb = new StringBuilder();
            SerializeInlines(inlines, sb, default);
            return sb.ToString();
        }

        private static void SerializeInlines(InlineCollection inlines, StringBuilder sb, InlineContext ctx)
        {
            if (inlines == null || inlines.Count == 0) return;
            for (Inline? cur = inlines.FirstInline; cur != null; cur = cur.NextInline)
            {
                SerializeInline(cur, sb, ctx);
            }
        }

        private static void SerializeInlines(IEnumerable<Inline> inlines, StringBuilder sb, InlineContext ctx)
        {
            if (inlines == null) return;
            foreach (var inline in inlines)
            {
                SerializeInline(inline, sb, ctx);
            }
        }

        /// <summary>
        /// Serializes a single WPF Inline into markdown.
        /// Note: Bold, Italic, and Hyperlink inherit from Span, so they must precede Span in the switch.
        /// </summary>
        public static string SerializeInline(Inline inline)
        {
            if (inline == null) return string.Empty;
            var sb = new StringBuilder();
            SerializeInline(inline, sb, default);
            return sb.ToString();
        }

        private static void SerializeInline(Inline inline, StringBuilder sb, InlineContext ctx)
        {
            if (inline == null) return;

            switch (inline)
            {
                case Bold bold:
                {
                    var childCtx = ctx;
                    childCtx.InBold = true;
                    var innerSb = RentScratch();
                    try
                    {
                        SerializeInlines(bold.Inlines, innerSb, childCtx);
                        WrapWithDelimiter(innerSb, "**", sb);
                    }
                    finally
                    {
                        ReturnScratch(innerSb);
                    }
                    break;
                }

                case Italic italic:
                {
                    var childCtx = ctx;
                    childCtx.InItalic = true;
                    var innerSb = RentScratch();
                    try
                    {
                        SerializeInlines(italic.Inlines, innerSb, childCtx);
                        WrapWithDelimiter(innerSb, "*", sb);
                    }
                    finally
                    {
                        ReturnScratch(innerSb);
                    }
                    break;
                }

                case Hyperlink hyperlink:
                    SerializeHyperlink(hyperlink, sb, ctx);
                    break;

                case Span span:
                    SerializeSpan(span, sb, ctx);
                    break;

                case Run run:
                    SerializeRun(run, sb, ctx);
                    break;

                case LineBreak _:
                    sb.Append("  \n");
                    break;

                case InlineUIContainer uic:
                    SerializeInlineUIContainer(uic, sb, ctx);
                    break;

                default:
                    break;
            }
        }

        // =========================================================================
        // Block Serializers
        // =========================================================================

        private static void SerializeParagraph(Paragraph p, StringBuilder sb)
        {
            if (p == null) return;

            // 1. Check if heading
            if (TryGetHeadingLevel(p, out int level))
            {
                sb.Append('#', level).Append(' ');
                var headingCtx = new InlineContext { InHeading = true };
                int start = sb.Length;
                SerializeInlines(p.Inlines, sb, headingCtx);
                // Trim leading spaces if any right after hashes
                int ws = 0;
                while (start + ws < sb.Length && sb[start + ws] == ' ') ws++;
                if (ws > 0) sb.Remove(start, ws);
                // Trim trailing whitespace
                while (sb.Length > start && char.IsWhiteSpace(sb[sb.Length - 1]))
                {
                    sb.Length--;
                }
                return;
            }

            // 2. Check if tagged as code block
            if (p.Tag is CodeBlockTag cbTag)
            {
                FormatCodeBlock(cbTag.Code, cbTag.Language, sb);
                return;
            }

            // 2.1 Check if tagged as Math block or HTML block
            if (p.Tag is MathTag mt)
            {
                sb.Append("$$\n").Append(mt.Expression).Append("\n$$");
                return;
            }
            if (p.Tag is HtmlBlockTag hbt)
            {
                sb.Append(hbt.RawHtml);
                return;
            }

            // 3. Normal paragraph
            int pStart = sb.Length;
            SerializeInlines(p.Inlines, sb, default);
            while (sb.Length > pStart && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
            {
                sb.Length--;
            }
        }

        private static bool TryGetHeadingLevel(Paragraph p, out int level)
        {
            level = 0;
            if (p == null) return false;

            object tag = p.Tag;
            if (tag != null)
            {
                // Explicit HeadingTag
                if (tag is HeadingTag ht && ht.Level >= 1 && ht.Level <= 6)
                {
                    level = ht.Level;
                    return true;
                }

                // Integer tag (1..6)
                if (tag is int tagInt && tagInt >= 1 && tagInt <= 6)
                {
                    level = tagInt;
                    return true;
                }

                if (tag is string tagStr && tagStr.Length > 0)
                {
                    if (tagStr.Length == 1 && tagStr[0] >= '1' && tagStr[0] <= '6')
                    {
                        level = tagStr[0] - '0';
                        return true;
                    }
                    if (tagStr.StartsWith("h", StringComparison.OrdinalIgnoreCase) ||
                        tagStr.StartsWith("header", StringComparison.OrdinalIgnoreCase) ||
                        tagStr.StartsWith("heading", StringComparison.OrdinalIgnoreCase))
                    {
                        var m = HeadingTagRegex.Match(tagStr.Trim());
                        if (m.Success && int.TryParse(m.Groups[1].Value, out int lvl))
                        {
                            level = lvl;
                            return true;
                        }
                    }
                }
            }

            bool hasLocalFontSize = p.ReadLocalValue(TextElement.FontSizeProperty) != DependencyProperty.UnsetValue;
            bool hasAnchorTag = tag is string anchorStr && !string.IsNullOrEmpty(anchorStr);

            if (hasLocalFontSize || hasAnchorTag)
            {
                double fs = p.FontSize;
                if (fs >= 24.0) level = 1;
                else if (fs >= 19.0) level = 2;
                else if (fs >= 16.0) level = 3;
                else if (fs >= 14.0) level = 4;
                else if (fs >= 13.0) level = 5;
                else level = 6;
                return true;
            }

            if (p.FontWeight >= FontWeights.SemiBold && p.FontSize >= 13.0)
            {
                double fs = p.FontSize;
                if (fs >= 24.0) level = 1;
                else if (fs >= 19.0) level = 2;
                else if (fs >= 16.0) level = 3;
                else if (fs >= 14.0) level = 4;
                else if (fs >= 13.0) level = 5;
                else level = 6;
                return true;
            }

            return false;
        }

        private static void SerializeSection(Section s, StringBuilder sb)
        {
            if (s == null || s.Blocks.Count == 0) return;

            // 0. Check if HTML block
            if (s.Tag is HtmlBlockTag hbt)
            {
                sb.Append(hbt.RawHtml);
                return;
            }

            // 1. Callout alert detection
            if (TryGetCallout(s, out CalloutType calloutType, out string calloutTitle, out bool skipHeaderPara))
            {
                string typeName = calloutType.ToString().ToUpperInvariant();
                if (string.IsNullOrEmpty(calloutTitle) || calloutTitle.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append("> [!").Append(typeName).Append("]\n");
                }
                else
                {
                    sb.Append("> [!").Append(typeName).Append("] ").Append(calloutTitle).Append('\n');
                }

                Block? startBlock = skipHeaderPara ? s.Blocks.FirstBlock?.NextBlock : s.Blocks.FirstBlock;
                if (startBlock != null)
                {
                    var childSb = RentScratch();
                    try
                    {
                        SerializeBlockChain(startBlock, childSb);
                        PrefixLinesWithQuote(childSb, sb);
                    }
                    finally
                    {
                        ReturnScratch(childSb);
                    }
                }

                while (sb.Length > 0 && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
                {
                    sb.Length--;
                }
                return;
            }

            // 2. Standard Blockquote
            var bqChildSb = RentScratch();
            try
            {
                SerializeBlockChain(s.Blocks.FirstBlock, bqChildSb);
                if (bqChildSb.Length == 0)
                {
                    sb.Append('>');
                    return;
                }

                PrefixLinesWithQuote(bqChildSb, sb);
                while (sb.Length > 0 && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
                {
                    sb.Length--;
                }
            }
            finally
            {
                ReturnScratch(bqChildSb);
            }
        }

        private static void SerializeBlockChain(Block? startBlock, StringBuilder sb)
        {
            bool firstBlock = true;
            for (Block? cur = startBlock; cur != null; cur = cur.NextBlock)
            {
                int startLen = sb.Length;
                if (!firstBlock)
                {
                    sb.Append("\n\n");
                }
                int blockStart = sb.Length;
                SerializeBlock(cur, sb);

                int blockEnd = sb.Length;
                while (blockEnd > blockStart && char.IsWhiteSpace(sb[blockEnd - 1]))
                {
                    blockEnd--;
                }

                if (blockEnd == blockStart)
                {
                    sb.Length = startLen;
                }
                else
                {
                    while (sb.Length > blockStart && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
                    {
                        sb.Length--;
                    }
                    firstBlock = false;
                }
            }
        }

        private static void PrefixLinesWithQuote(StringBuilder source, StringBuilder dest)
        {
            while (source.Length > 0 && (source[source.Length - 1] == '\r' || source[source.Length - 1] == '\n'))
            {
                source.Length--;
            }

            int lineStart = 0;
            while (lineStart < source.Length)
            {
                int lineEnd = lineStart;
                while (lineEnd < source.Length && source[lineEnd] != '\n' && source[lineEnd] != '\r')
                {
                    lineEnd++;
                }

                bool isWs = true;
                for (int k = lineStart; k < lineEnd; k++)
                {
                    if (!char.IsWhiteSpace(source[k]))
                    {
                        isWs = false;
                        break;
                    }
                }

                if (isWs)
                {
                    dest.Append(">\n");
                }
                else
                {
                    dest.Append("> ");
                    dest.Append(source, lineStart, lineEnd - lineStart);
                    dest.Append('\n');
                }

                if (lineEnd < source.Length && source[lineEnd] == '\r') lineEnd++;
                if (lineEnd < source.Length && source[lineEnd] == '\n') lineEnd++;
                lineStart = lineEnd;
            }
        }

        private static bool TryGetCallout(Section s, out CalloutType calloutType, out string calloutTitle, out bool skipHeaderPara)
        {
            calloutType = CalloutType.None;
            calloutTitle = string.Empty;
            skipHeaderPara = false;

            if (s.Tag is CalloutTag ct)
            {
                calloutType = ct.Type;
                calloutTitle = ct.Title;
                skipHeaderPara = false;
                return true;
            }

            if (s.Blocks.Count > 0 && s.Blocks.FirstBlock is Paragraph headerPara)
            {
                if (headerPara.Inlines.FirstInline is Run firstRun)
                {
                    string t = firstRun.Text;
                    if (!t.Contains("💡") && !t.Contains("📌") && !t.Contains("⚠️") && !t.Contains("🚫") && !t.Contains("ℹ"))
                    {
                        return false;
                    }

                    if (firstRun.NextInline == null)
                    {
                        string cleanText = t.Trim('*', ' ');
                        CalloutType parsedType = CalloutType.None;
                        string parsedTitle = "";

                        if (cleanText.StartsWith("💡"))
                        {
                            parsedType = CalloutType.Tip;
                            parsedTitle = cleanText.Substring("💡".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("📌"))
                        {
                            parsedType = CalloutType.Important;
                            parsedTitle = cleanText.Substring("📌".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("⚠️"))
                        {
                            parsedType = CalloutType.Warning;
                            parsedTitle = cleanText.Substring("⚠️".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("🚫"))
                        {
                            parsedType = CalloutType.Caution;
                            parsedTitle = cleanText.Substring("🚫".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("ℹ️") || cleanText.StartsWith("ℹ"))
                        {
                            parsedType = CalloutType.Note;
                            parsedTitle = cleanText.TrimStart('ℹ', '️', ' ').Trim().Trim('*', ' ');
                        }

                        if (parsedType != CalloutType.None)
                        {
                            calloutType = parsedType;
                            calloutTitle = parsedTitle;
                            skipHeaderPara = true;
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }

                var scratch = RentScratch();
                try
                {
                    var ctx = new InlineContext { InHeading = TryGetHeadingLevel(headerPara, out _) };
                    SerializeInlines(headerPara.Inlines, scratch, ctx);

                    int start = 0;
                    while (start < scratch.Length && (char.IsWhiteSpace(scratch[start]) || scratch[start] == '*')) start++;
                    int end = scratch.Length - 1;
                    while (end >= start && (char.IsWhiteSpace(scratch[end]) || scratch[end] == '*')) end--;

                    if (start <= end)
                    {
                        string cleanText = scratch.ToString(start, end - start + 1);
                        CalloutType parsedType = CalloutType.None;
                        string parsedTitle = "";

                        if (cleanText.StartsWith("💡"))
                        {
                            parsedType = CalloutType.Tip;
                            parsedTitle = cleanText.Substring("💡".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("📌"))
                        {
                            parsedType = CalloutType.Important;
                            parsedTitle = cleanText.Substring("📌".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("⚠️"))
                        {
                            parsedType = CalloutType.Warning;
                            parsedTitle = cleanText.Substring("⚠️".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("🚫"))
                        {
                            parsedType = CalloutType.Caution;
                            parsedTitle = cleanText.Substring("🚫".Length).Trim().Trim('*', ' ');
                        }
                        else if (cleanText.StartsWith("ℹ️") || cleanText.StartsWith("ℹ"))
                        {
                            parsedType = CalloutType.Note;
                            parsedTitle = cleanText.TrimStart('ℹ', '️', ' ').Trim().Trim('*', ' ');
                        }

                        if (parsedType != CalloutType.None)
                        {
                            calloutType = parsedType;
                            calloutTitle = parsedTitle;
                            skipHeaderPara = true;
                            return true;
                        }
                    }
                }
                finally
                {
                    ReturnScratch(scratch);
                }
            }

            return false;
        }

        private static void SerializeList(WpfList list, StringBuilder sb)
        {
            if (list == null || list.ListItems.Count == 0) return;

            bool isOrdered = list.MarkerStyle == TextMarkerStyle.Decimal;
            int currentIndex = list.StartIndex > 0 ? list.StartIndex : 1;

            for (ListItem? item = list.ListItems.FirstListItem; item != null; item = item.NextListItem)
            {
                string prefix = isOrdered ? $"{currentIndex}. " : "- ";
                currentIndex++;

                var firstBlock = item.Blocks.FirstBlock;

                if (firstBlock is Paragraph p)
                {
                    Inline? firstInline = p.Inlines.FirstInline;
                    if (firstInline is InlineUIContainer uic && uic.Child is CheckBox cb)
                    {
                        prefix = cb.IsChecked == true ? "- [x] " : "- [ ] ";
                        firstInline = firstInline.NextInline;
                    }

                    sb.Append(prefix);
                    int beforeInlines = sb.Length;
                    for (Inline? cur = firstInline; cur != null; cur = cur.NextInline)
                    {
                        SerializeInline(cur, sb, default);
                    }

                    while (sb.Length > beforeInlines && char.IsWhiteSpace(sb[sb.Length - 1]))
                    {
                        sb.Length--;
                    }
                    sb.Append('\n');
                }
                else if (firstBlock != null)
                {
                    sb.Append(prefix);
                    int beforeBlock = sb.Length;
                    SerializeBlock(firstBlock, sb);
                    while (sb.Length > beforeBlock && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
                    {
                        sb.Length--;
                    }
                    sb.Append('\n');
                }

                if (item.Blocks.Count > 1)
                {
                    var childSb = RentScratch();
                    try
                    {
                        bool firstRemaining = true;
                        for (Block? b = item.Blocks.FirstBlock?.NextBlock; b != null; b = b.NextBlock)
                        {
                            if (!firstRemaining) childSb.Append("\n\n");
                            int bStart = childSb.Length;
                            SerializeBlock(b, childSb);
                            while (childSb.Length > bStart && (childSb[childSb.Length - 1] == '\r' || childSb[childSb.Length - 1] == '\n'))
                            {
                                childSb.Length--;
                            }
                            firstRemaining = false;
                        }

                        while (childSb.Length > 0 && (childSb[childSb.Length - 1] == '\r' || childSb[childSb.Length - 1] == '\n'))
                        {
                            childSb.Length--;
                        }

                        string indent = new string(' ', Math.Max(2, prefix.Length));
                        int lineStart = 0;
                        while (lineStart < childSb.Length)
                        {
                            int lineEnd = lineStart;
                            while (lineEnd < childSb.Length && childSb[lineEnd] != '\n' && childSb[lineEnd] != '\r')
                            {
                                lineEnd++;
                            }

                            bool isWs = true;
                            for (int k = lineStart; k < lineEnd; k++)
                            {
                                if (!char.IsWhiteSpace(childSb[k]))
                                {
                                    isWs = false;
                                    break;
                                }
                            }

                            if (isWs)
                            {
                                sb.Append('\n');
                            }
                            else
                            {
                                sb.Append(indent);
                                sb.Append(childSb, lineStart, lineEnd - lineStart);
                                sb.Append('\n');
                            }

                            if (lineEnd < childSb.Length && childSb[lineEnd] == '\r') lineEnd++;
                            if (lineEnd < childSb.Length && childSb[lineEnd] == '\n') lineEnd++;
                            lineStart = lineEnd;
                        }
                    }
                    finally
                    {
                        ReturnScratch(childSb);
                    }
                }
            }

            while (sb.Length > 0 && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
            {
                sb.Length--;
            }
        }

        private static Paragraph? GetCellParagraph(WpfTableCell cell)
        {
            if (cell.Blocks.Count > 0 && cell.Blocks.FirstBlock is Paragraph firstP)
                return firstP;
            foreach (var b in cell.Blocks)
            {
                if (b is Paragraph p) return p;
            }
            return null;
        }

        private static void SerializeTableCellTo(WpfTableCell? cell, InlineContext ctx, StringBuilder dest)
        {
            if (cell == null) return;
            var p = GetCellParagraph(cell);
            if (p == null) return;

            var cellSb = RentScratch();
            try
            {
                SerializeInlines(p.Inlines, cellSb, ctx);
                int start = 0;
                while (start < cellSb.Length && char.IsWhiteSpace(cellSb[start])) start++;
                int end = cellSb.Length - 1;
                while (end >= start && char.IsWhiteSpace(cellSb[end])) end--;

                if (start > end) return;

                bool hasPipe = false;
                for (int i = start; i <= end; i++)
                {
                    if (cellSb[i] == '|')
                    {
                        hasPipe = true;
                        break;
                    }
                }

                if (!hasPipe)
                {
                    for (int i = start; i <= end; i++)
                    {
                        dest.Append(cellSb[i]);
                    }
                }
                else
                {
                    for (int i = start; i <= end; i++)
                    {
                        char c = cellSb[i];
                        if (c == '|')
                        {
                            int slashCount = 0;
                            for (int j = i - 1; j >= start && cellSb[j] == '\\'; j--)
                            {
                                slashCount++;
                            }
                            if (slashCount % 2 == 0)
                            {
                                dest.Append('\\');
                            }
                        }
                        dest.Append(c);
                    }
                }
            }
            finally
            {
                ReturnScratch(cellSb);
            }
        }

        private static void SerializeTable(WpfTable table, StringBuilder sb)
        {
            if (table == null || table.RowGroups.Count == 0) return;

            WpfTableRow? headerRow = null;
            if (table.RowGroups.Count > 0 && table.RowGroups[0].Rows.Count > 0)
            {
                headerRow = table.RowGroups[0].Rows[0];
            }

            int colCount = headerRow?.Cells.Count ?? 0;
            for (int rgIdx = 0; rgIdx < table.RowGroups.Count; rgIdx++)
            {
                var rg = table.RowGroups[rgIdx];
                int startRow = (rgIdx == 0 && headerRow != null) ? 1 : 0;
                for (int r = startRow; r < rg.Rows.Count; r++)
                {
                    colCount = Math.Max(colCount, rg.Rows[r].Cells.Count);
                }
            }

            if (colCount == 0) return;

            // Compute alignments once
            var colAlignments = new TextAlignment[colCount];
            if (headerRow != null)
            {
                for (int i = 0; i < Math.Min(colCount, headerRow.Cells.Count); i++)
                {
                    var p = GetCellParagraph(headerRow.Cells[i]);
                    if (p != null) colAlignments[i] = p.TextAlignment;
                }
            }
            for (int i = 0; i < colCount; i++)
            {
                if (colAlignments[i] == TextAlignment.Left)
                {
                    for (int rgIdx = 0; rgIdx < table.RowGroups.Count; rgIdx++)
                    {
                        var rg = table.RowGroups[rgIdx];
                        int startRow = (rgIdx == 0 && headerRow != null) ? 1 : 0;
                        for (int r = startRow; r < rg.Rows.Count; r++)
                        {
                            var row = rg.Rows[r];
                            if (i < row.Cells.Count)
                            {
                                var p = GetCellParagraph(row.Cells[i]);
                                if (p != null && p.TextAlignment != TextAlignment.Left)
                                {
                                    colAlignments[i] = p.TextAlignment;
                                    break;
                                }
                            }
                        }
                        if (colAlignments[i] != TextAlignment.Left) break;
                    }
                }
            }

            // 1. Header Cells (InHeading = true prevents false bold wrapping)
            var headerCtx = new InlineContext { InHeading = true };
            sb.Append("| ");
            for (int i = 0; i < colCount; i++)
            {
                if (i > 0) sb.Append(" | ");
                WpfTableCell? cell = (headerRow != null && i < headerRow.Cells.Count) ? headerRow.Cells[i] : null;
                SerializeTableCellTo(cell, headerCtx, sb);
            }
            sb.Append(" |\n");

            // 2. Alignment Separators
            sb.Append("| ");
            for (int i = 0; i < colCount; i++)
            {
                if (i > 0) sb.Append(" | ");
                string sep = colAlignments[i] switch
                {
                    TextAlignment.Center => ":---:",
                    TextAlignment.Right => "---:",
                    _ => ":---"
                };
                sb.Append(sep);
            }
            sb.Append(" |\n");

            // 3. Data Rows
            for (int rgIdx = 0; rgIdx < table.RowGroups.Count; rgIdx++)
            {
                var rg = table.RowGroups[rgIdx];
                int startRow = (rgIdx == 0 && headerRow != null) ? 1 : 0;
                for (int r = startRow; r < rg.Rows.Count; r++)
                {
                    var row = rg.Rows[r];
                    sb.Append("| ");
                    for (int i = 0; i < colCount; i++)
                    {
                        if (i > 0) sb.Append(" | ");
                        WpfTableCell? cell = i < row.Cells.Count ? row.Cells[i] : null;
                        SerializeTableCellTo(cell, default, sb);
                    }
                    sb.Append(" |\n");
                }
            }

            while (sb.Length > 0 && (sb[sb.Length - 1] == '\r' || sb[sb.Length - 1] == '\n'))
            {
                sb.Length--;
            }
        }

        private static void SerializeBlockUIContainer(BlockUIContainer buic, StringBuilder sb)
        {
            if (buic == null || buic.Child == null) return;

            if (buic.Tag is MathTag mt)
            {
                sb.Append("$$\n").Append(mt.Expression).Append("\n$$");
                return;
            }

            if (buic.Tag is HtmlBlockTag hbt)
            {
                sb.Append(hbt.RawHtml);
                return;
            }

            // 1. Thematic Break (horizontal rule)
            if (buic.Tag as string == "hr" ||
                (buic.Child is Border lineBorder && (lineBorder.Height <= 2 || lineBorder.Tag as string == "hr")))
            {
                sb.Append("---");
                return;
            }

            // 2. Fenced Code Block (Border -> Grid -> [Header, TextBlock])
            if (buic.Child is Border codeBorder && codeBorder.Child is Grid mainGrid)
            {
                string language = "";
                string code = "";

                foreach (var child in mainGrid.Children)
                {
                    if (child is Grid headerGrid)
                    {
                        foreach (var hChild in headerGrid.Children)
                        {
                            if (hChild is TextBlock tb)
                            {
                                string t = tb.Text.Trim();
                                if (!string.IsNullOrEmpty(t) && !t.Equals("TEXT", StringComparison.OrdinalIgnoreCase))
                                {
                                    language = t.ToLowerInvariant();
                                }
                            }
                        }
                    }
                    else if (child is TextBlock codeTb)
                    {
                        if (codeTb.Inlines.Count > 0)
                        {
                            var codeSb = RentScratch();
                            try
                            {
                                for (Inline? cur = codeTb.Inlines.FirstInline; cur != null; cur = cur.NextInline)
                                {
                                    if (cur is Run r) codeSb.Append(r.Text);
                                }
                                code = codeSb.ToString();
                            }
                            finally
                            {
                                ReturnScratch(codeSb);
                            }
                        }
                        else
                        {
                            code = codeTb.Text;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(language))
                {
                    FormatCodeBlock(code, language, sb);
                    return;
                }
            }

            // 3. Frontmatter (Border -> StackPanel containing METADATA)
            if (buic.Child is Border fmBorder && fmBorder.Child is StackPanel fmSp && fmSp.Children.Count > 0)
            {
                var spChildren = fmSp.Children.Cast<UIElement>().ToList();
                if (spChildren.Count > 0 && spChildren[0] is TextBlock titleTb && titleTb.Text == "METADATA")
                {
                    sb.Append("---\n");
                    foreach (var child in spChildren.Skip(1))
                    {
                        if (child is StackPanel rowSp)
                        {
                            var rowChildren = rowSp.Children.Cast<UIElement>().ToList();
                            if (rowChildren.Count == 2)
                            {
                                string key = ((TextBlock)rowChildren[0]).Text.TrimEnd(':', ' ');
                                string val = ((TextBlock)rowChildren[1]).Text.Trim();
                                sb.Append(key).Append(": ").Append(val).Append('\n');
                            }
                        }
                    }
                    sb.Append("---");
                    return;
                }
            }

            // Fallback: TextBlock
            if (buic.Child is TextBlock textBlock)
            {
                sb.Append(textBlock.Text);
            }
        }

        private static void FormatCodeBlock(string code, string language, StringBuilder sb)
        {
            int maxBackticks = 0;
            int currentRun = 0;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '`')
                {
                    currentRun++;
                    if (currentRun > maxBackticks) maxBackticks = currentRun;
                }
                else
                {
                    currentRun = 0;
                }
            }

            int fenceLen = Math.Max(3, maxBackticks + 1);
            sb.Append('`', fenceLen);
            sb.Append(language.Trim());
            sb.Append('\n');

            int end = code.Length;
            while (end > 0 && (code[end - 1] == '\r' || code[end - 1] == '\n'))
            {
                end--;
            }

            for (int i = 0; i < end; i++)
            {
                char c = code[i];
                if (c == '\r')
                {
                    if (i + 1 < end && code[i + 1] == '\n')
                    {
                        continue;
                    }
                    sb.Append('\n');
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (end > 0)
            {
                sb.Append('\n');
            }
            sb.Append('`', fenceLen);
        }

        private static string FormatCodeBlock(string code, string language)
        {
            var sb = RentScratch();
            try
            {
                FormatCodeBlock(code, language, sb);
                return sb.ToString();
            }
            finally
            {
                ReturnScratch(sb);
            }
        }

        // =========================================================================
        // Inline Serializers
        // =========================================================================

        private static void SerializeSpan(Span span, StringBuilder sb, InlineContext ctx)
        {
            if (span == null) return;

            if (span.Tag is HtmlInlineTag hit)
            {
                sb.Append(hit.RawHtml);
                return;
            }

            // 1. Inline Code
            bool isCode = (span.Tag as string == "code") ||
                          (span.FontFamily != null && span.FontFamily.Source.IndexOf("Cascadia Code", StringComparison.OrdinalIgnoreCase) >= 0);
            if (isCode)
            {
                if (span.Inlines.FirstInline is Run r && r.NextInline == null)
                {
                    FormatInlineCode(r.Text, sb);
                }
                else
                {
                    var codeSb = RentScratch();
                    try
                    {
                        ExtractRawText(span, codeSb);
                        FormatInlineCode(codeSb, sb);
                    }
                    finally
                    {
                        ReturnScratch(codeSb);
                    }
                }
                return;
            }

            // 2. Strikethrough
            bool isStrike = !ctx.InStrike &&
                            ((span.TextDecorations != null && span.TextDecorations.Any(d => d.Location == TextDecorationLocation.Strikethrough)) ||
                             span.Tag as string == "strikethrough");

            // 3. Highlight
            bool isHighlight = !ctx.InHighlight &&
                               ((span.Background != null && span.Background != Brushes.Transparent) ||
                                span.Tag as string == "highlight");

            var childCtx = ctx;
            if (isStrike) childCtx.InStrike = true;
            if (isHighlight) childCtx.InHighlight = true;

            if (!isStrike && !isHighlight)
            {
                SerializeInlines(span.Inlines, sb, childCtx);
                return;
            }

            var innerSb = RentScratch();
            try
            {
                SerializeInlines(span.Inlines, innerSb, childCtx);
                if (isStrike && isHighlight)
                {
                    var tempSb = RentScratch();
                    try
                    {
                        WrapWithDelimiter(innerSb, "~~", tempSb);
                        WrapWithDelimiter(tempSb, "==", sb);
                    }
                    finally
                    {
                        ReturnScratch(tempSb);
                    }
                }
                else if (isStrike)
                {
                    WrapWithDelimiter(innerSb, "~~", sb);
                }
                else if (isHighlight)
                {
                    WrapWithDelimiter(innerSb, "==", sb);
                }
            }
            finally
            {
                ReturnScratch(innerSb);
            }
        }

        private static void SerializeRun(Run run, StringBuilder sb, InlineContext ctx)
        {
            if (run == null || string.IsNullOrEmpty(run.Text))
                return;

            if (run.Tag is MathTag mt)
            {
                sb.Append(mt.IsDisplay ? $"$${mt.Expression}$$" : $"${mt.Expression}$");
                return;
            }

            if (run.Tag is HtmlInlineTag hit)
            {
                sb.Append(hit.RawHtml);
                return;
            }

            if (run.Tag is HtmlBlockTag hbt)
            {
                sb.Append(hbt.RawHtml);
                return;
            }

            string text = run.Text;

            if (run.Tag as string == "code" || ctx.InCode)
            {
                FormatInlineCode(text, sb);
                return;
            }

            bool isBold = false;
            if (!ctx.InBold)
            {
                if (ctx.InHeading)
                {
                    if (run.ReadLocalValue(TextElement.FontWeightProperty) != DependencyProperty.UnsetValue)
                    {
                        isBold = run.FontWeight >= FontWeights.SemiBold;
                    }
                }
                else
                {
                    isBold = run.FontWeight >= FontWeights.SemiBold;
                }
            }

            bool isItalic = false;
            if (!ctx.InItalic)
            {
                if (ctx.InHeading)
                {
                    if (run.ReadLocalValue(TextElement.FontStyleProperty) != DependencyProperty.UnsetValue)
                    {
                        isItalic = run.FontStyle == FontStyles.Italic;
                    }
                }
                else
                {
                    isItalic = run.FontStyle == FontStyles.Italic;
                }
            }

            bool isStrike = false;
            if (!ctx.InStrike)
            {
                if (run.Tag as string == "strikethrough")
                {
                    isStrike = true;
                }
                else if (run.ReadLocalValue(Inline.TextDecorationsProperty) != DependencyProperty.UnsetValue)
                {
                    var decs = run.TextDecorations;
                    if (decs != null && decs.Count > 0)
                    {
                        for (int d = 0; d < decs.Count; d++)
                        {
                            if (decs[d].Location == TextDecorationLocation.Strikethrough)
                            {
                                isStrike = true;
                                break;
                            }
                        }
                    }
                }
            }

            bool isHighlight = false;
            if (!ctx.InHighlight)
            {
                if (run.Tag as string == "highlight")
                {
                    isHighlight = true;
                }
                else if (run.ReadLocalValue(TextElement.BackgroundProperty) != DependencyProperty.UnsetValue)
                {
                    var bg = run.Background;
                    if (bg != null && bg != Brushes.Transparent)
                    {
                        isHighlight = true;
                    }
                }
            }

            if (!isStrike && !isHighlight)
            {
                if (isBold && isItalic) WrapWithDelimiter(text, "***", sb);
                else if (isBold) WrapWithDelimiter(text, "**", sb);
                else if (isItalic) WrapWithDelimiter(text, "*", sb);
                else sb.Append(text);
                return;
            }

            var temp = RentScratch();
            try
            {
                temp.Append(text);
                if (isStrike)
                {
                    var next = RentScratch();
                    WrapWithDelimiter(temp, "~~", next);
                    ReturnScratch(temp);
                    temp = next;
                }
                if (isHighlight)
                {
                    var next = RentScratch();
                    WrapWithDelimiter(temp, "==", next);
                    ReturnScratch(temp);
                    temp = next;
                }
                if (isBold && isItalic)
                {
                    WrapWithDelimiter(temp, "***", sb);
                }
                else if (isBold)
                {
                    WrapWithDelimiter(temp, "**", sb);
                }
                else if (isItalic)
                {
                    WrapWithDelimiter(temp, "*", sb);
                }
                else
                {
                    sb.Append(temp);
                }
            }
            finally
            {
                ReturnScratch(temp);
            }
        }

        private static void SerializeHyperlink(Hyperlink hyperlink, StringBuilder sb, InlineContext ctx)
        {
            if (hyperlink == null) return;

            if (hyperlink.Tag is HtmlInlineTag hit)
            {
                sb.Append(hit.RawHtml);
                return;
            }

            string url = hyperlink.NavigateUri?.OriginalString ?? hyperlink.ToolTip?.ToString() ?? (hyperlink.Tag as string) ?? "";
            string? tip = hyperlink.ToolTip?.ToString();

            sb.Append('[');
            SerializeInlines(hyperlink.Inlines, sb, ctx);
            sb.Append(']').Append('(').Append(url);
            if (!string.IsNullOrEmpty(tip) && tip != url && !string.IsNullOrEmpty(url))
            {
                sb.Append(" \"").Append(tip).Append('"');
            }
            sb.Append(')');
        }

        private static void SerializeInlineUIContainer(InlineUIContainer uic, StringBuilder sb, InlineContext ctx)
        {
            if (uic == null || uic.Child == null) return;

            if (uic.Tag is MathTag mt)
            {
                sb.Append(mt.IsDisplay ? $"$${mt.Expression}$$" : $"${mt.Expression}$");
                return;
            }

            if (uic.Tag is HtmlInlineTag hit)
            {
                sb.Append(hit.RawHtml);
                return;
            }

            if (uic.Child is CheckBox cb)
            {
                sb.Append(cb.IsChecked == true ? "- [x] " : "- [ ] ");
                return;
            }

            if (uic.Child is Image img)
            {
                string url = "";
                if (img.Tag is string tagStr && !string.IsNullOrEmpty(tagStr) && (tagStr.StartsWith("http") || tagStr.Contains("/") || tagStr.Contains("\\")))
                {
                    url = tagStr;
                }
                else if (img.Source is BitmapImage bi && bi.UriSource != null)
                {
                    url = bi.UriSource.IsAbsoluteUri && bi.UriSource.Scheme == "file" ? bi.UriSource.LocalPath : bi.UriSource.OriginalString;
                }
                else if (img.Source != null)
                {
                    url = img.Source.ToString();
                }

                string alt = img.Tag as string ?? (uic.Tag as string) ?? "";
                string title = img.ToolTip?.ToString() ?? "";

                if (alt == url) alt = "";

                if (!string.IsNullOrEmpty(title) && title != url)
                {
                    sb.Append("![").Append(alt).Append("](").Append(url).Append(" \"").Append(title).Append("\")");
                }
                else
                {
                    sb.Append("![").Append(alt).Append("](").Append(url).Append(')');
                }
                return;
            }

            if (uic.Child is Border imgBorder && imgBorder.Child is TextBlock imgTb && imgTb.Text.StartsWith("🖼 "))
            {
                string content = imgTb.Text.Substring(2).Trim();
                sb.Append("![").Append(content).Append("](").Append(content).Append(')');
                return;
            }

            if (uic.Child is Border codeBorder && codeBorder.Child is TextBlock codeTb)
            {
                FormatInlineCode(codeTb.Text, sb);
                return;
            }

            if (uic.Child is TextBlock tb)
            {
                sb.Append(tb.Text);
            }
        }

        private static void FormatInlineCode(StringBuilder code, StringBuilder sb)
        {
            if (code == null || code.Length == 0)
            {
                sb.Append("``");
                return;
            }

            int maxBackticks = 0;
            int currentRun = 0;
            bool hasBacktick = false;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '`')
                {
                    hasBacktick = true;
                    currentRun++;
                    if (currentRun > maxBackticks) maxBackticks = currentRun;
                }
                else
                {
                    currentRun = 0;
                }
            }

            if (maxBackticks == 0)
            {
                sb.Append('`').Append(code).Append('`');
                return;
            }

            int fenceLen = maxBackticks + 1;
            if (code[0] == '`' || code[code.Length - 1] == '`' || hasBacktick)
            {
                sb.Append('`', fenceLen).Append(' ').Append(code).Append(' ').Append('`', fenceLen);
            }
            else
            {
                sb.Append('`', fenceLen).Append(code).Append('`', fenceLen);
            }
        }

        private static void FormatInlineCode(string code, StringBuilder sb)
        {
            if (string.IsNullOrEmpty(code))
            {
                sb.Append("``");
                return;
            }

            if (code.IndexOf('`') < 0)
            {
                sb.Append('`').Append(code).Append('`');
                return;
            }

            int maxBackticks = 0;
            int currentRun = 0;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '`')
                {
                    currentRun++;
                    if (currentRun > maxBackticks) maxBackticks = currentRun;
                }
                else
                {
                    currentRun = 0;
                }
            }

            int fenceLen = maxBackticks + 1;
            if (code.StartsWith("`") || code.EndsWith("`") || code.Contains("`"))
            {
                sb.Append('`', fenceLen).Append(' ').Append(code).Append(' ').Append('`', fenceLen);
            }
            else
            {
                sb.Append('`', fenceLen).Append(code).Append('`', fenceLen);
            }
        }

        private static string FormatInlineCode(string code)
        {
            var sb = RentScratch();
            try
            {
                FormatInlineCode(code, sb);
                return sb.ToString();
            }
            finally
            {
                ReturnScratch(sb);
            }
        }

        private static void WrapWithDelimiter(StringBuilder textSb, string delimiter, StringBuilder dest)
        {
            if (textSb == null || textSb.Length == 0) return;

            if (!char.IsWhiteSpace(textSb[0]) && !char.IsWhiteSpace(textSb[textSb.Length - 1]))
            {
                dest.Append(delimiter);
                for (int i = 0; i < textSb.Length; i++) dest.Append(textSb[i]);
                dest.Append(delimiter);
                return;
            }

            int start = 0;
            while (start < textSb.Length && char.IsWhiteSpace(textSb[start])) start++;
            if (start == textSb.Length)
            {
                dest.Append(textSb);
                return;
            }

            int end = textSb.Length - 1;
            while (end >= 0 && char.IsWhiteSpace(textSb[end])) end--;

            if (start > 0)
            {
                for (int i = 0; i < start; i++) dest.Append(textSb[i]);
            }
            dest.Append(delimiter);
            for (int i = start; i <= end; i++) dest.Append(textSb[i]);
            dest.Append(delimiter);
            if (end < textSb.Length - 1)
            {
                for (int i = end + 1; i < textSb.Length; i++) dest.Append(textSb[i]);
            }
        }

        private static void WrapWithDelimiter(string text, string delimiter, StringBuilder sb)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (!char.IsWhiteSpace(text[0]) && !char.IsWhiteSpace(text[text.Length - 1]))
            {
                sb.Append(delimiter).Append(text).Append(delimiter);
                return;
            }

            int start = 0;
            while (start < text.Length && char.IsWhiteSpace(text[start])) start++;
            if (start == text.Length)
            {
                sb.Append(text);
                return;
            }

            int end = text.Length - 1;
            while (end >= 0 && char.IsWhiteSpace(text[end])) end--;

            if (start > 0)
            {
                sb.Append(text, 0, start);
            }
            sb.Append(delimiter);
            sb.Append(text, start, end - start + 1);
            sb.Append(delimiter);
            if (end < text.Length - 1)
            {
                sb.Append(text, end + 1, text.Length - end - 1);
            }
        }

        private static string WrapWithDelimiter(string text, string delimiter)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = RentScratch();
            try
            {
                WrapWithDelimiter(text, delimiter, sb);
                return sb.ToString();
            }
            finally
            {
                ReturnScratch(sb);
            }
        }

        private static string ExtractRawText(TextElement element)
        {
            var sb = RentScratch();
            try
            {
                ExtractRawText(element, sb);
                return sb.ToString();
            }
            finally
            {
                ReturnScratch(sb);
            }
        }

        private static void ExtractRawText(TextElement element, StringBuilder sb)
        {
            if (element is Run r)
            {
                sb.Append(r.Text);
            }
            else if (element is Span s)
            {
                for (Inline? cur = s.Inlines.FirstInline; cur != null; cur = cur.NextInline)
                {
                    ExtractRawText(cur, sb);
                }
            }
            else if (element is Paragraph p)
            {
                for (Inline? cur = p.Inlines.FirstInline; cur != null; cur = cur.NextInline)
                {
                    ExtractRawText(cur, sb);
                }
            }
        }
    }
}
