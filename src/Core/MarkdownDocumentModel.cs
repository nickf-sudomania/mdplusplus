using System;
using System.Collections.Generic;

namespace MDPlus.Core
{
    public class MarkdownDocument
    {
        public Dictionary<string, string> Frontmatter { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<MarkdownBlock> Blocks { get; set; } = new List<MarkdownBlock>();
        public string Title { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public int ReadingTimeMinutes => Math.Max(1, (int)Math.Ceiling(WordCount / 200.0));
    }

    public abstract class MarkdownBlock
    {
    }

    public class FrontmatterBlock : MarkdownBlock
    {
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string RawYaml { get; set; } = string.Empty;
    }

    public class HeadingBlock : MarkdownBlock
    {
        public int Level { get; set; } = 1; // 1 to 6
        public string Text { get; set; } = string.Empty;
        public string Anchor { get; set; } = string.Empty;
        public int LineIndex { get; set; }
        public List<MarkdownInline> Inlines { get; set; } = new List<MarkdownInline>();
    }

    public class ParagraphBlock : MarkdownBlock
    {
        public List<MarkdownInline> Inlines { get; set; } = new List<MarkdownInline>();
    }

    public enum CalloutType
    {
        None,
        Note,       // Blue
        Tip,        // Green
        Important,  // Purple
        Warning,    // Yellow/Amber
        Caution     // Red
    }

    public class BlockquoteBlock : MarkdownBlock
    {
        public List<MarkdownBlock> Blocks { get; set; } = new List<MarkdownBlock>();
        public CalloutType Callout { get; set; } = CalloutType.None;
        public string CalloutTitle { get; set; } = string.Empty;
    }

    public class CodeBlock : MarkdownBlock
    {
        public string Code { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }

    public enum ColumnAlignment
    {
        Left,
        Center,
        Right
    }

    public class TableCell
    {
        public string Text { get; set; } = string.Empty;
        public List<MarkdownInline> Inlines { get; set; } = new List<MarkdownInline>();
    }

    public class TableRow
    {
        public List<TableCell> Cells { get; set; } = new List<TableCell>();
    }

    public class TableBlock : MarkdownBlock
    {
        public List<ColumnAlignment> Alignments { get; set; } = new List<ColumnAlignment>();
        public TableRow Header { get; set; } = new TableRow();
        public List<TableRow> Rows { get; set; } = new List<TableRow>();
    }

    public class ListItemBlock
    {
        public List<MarkdownBlock> Blocks { get; set; } = new List<MarkdownBlock>();
        public bool IsTask { get; set; }
        public bool IsChecked { get; set; }
        public List<MarkdownInline> Inlines { get; set; } = new List<MarkdownInline>();
    }

    public class ListBlock : MarkdownBlock
    {
        public bool IsOrdered { get; set; }
        public int StartNumber { get; set; } = 1;
        public List<ListItemBlock> Items { get; set; } = new List<ListItemBlock>();
    }

    public class ThematicBreakBlock : MarkdownBlock
    {
    }

    // --- Inline Elements ---

    public abstract class MarkdownInline
    {
    }

    public class TextInline : MarkdownInline
    {
        public string Text { get; set; } = string.Empty;
        public TextInline() { }
        public TextInline(string text) { Text = text; }
    }

    public class BoldInline : MarkdownInline
    {
        public List<MarkdownInline> Children { get; set; } = new List<MarkdownInline>();
        public BoldInline() { }
        public BoldInline(params MarkdownInline[] inlines) { Children.AddRange(inlines); }
    }

    public class ItalicInline : MarkdownInline
    {
        public List<MarkdownInline> Children { get; set; } = new List<MarkdownInline>();
        public ItalicInline() { }
        public ItalicInline(params MarkdownInline[] inlines) { Children.AddRange(inlines); }
    }

    public class BoldItalicInline : MarkdownInline
    {
        public List<MarkdownInline> Children { get; set; } = new List<MarkdownInline>();
        public BoldItalicInline() { }
        public BoldItalicInline(params MarkdownInline[] inlines) { Children.AddRange(inlines); }
    }

    public class StrikethroughInline : MarkdownInline
    {
        public List<MarkdownInline> Children { get; set; } = new List<MarkdownInline>();
        public StrikethroughInline() { }
        public StrikethroughInline(params MarkdownInline[] inlines) { Children.AddRange(inlines); }
    }

    public class CodeInline : MarkdownInline
    {
        public string Code { get; set; } = string.Empty;
        public CodeInline() { }
        public CodeInline(string code) { Code = code; }
    }

    public class HighlightInline : MarkdownInline
    {
        public List<MarkdownInline> Children { get; set; } = new List<MarkdownInline>();
        public HighlightInline() { }
        public HighlightInline(params MarkdownInline[] inlines) { Children.AddRange(inlines); }
    }

    public class LinkInline : MarkdownInline
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<MarkdownInline> Children { get; set; } = new List<MarkdownInline>();
    }

    public class ImageInline : MarkdownInline
    {
        public string Url { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class LineBreakInline : MarkdownInline
    {
        public bool IsHard { get; set; }
        public LineBreakInline(bool isHard = false) { IsHard = isHard; }
    }
}
