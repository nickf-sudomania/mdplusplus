using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MDPlus.Core;

namespace MDPlus.Tests
{
    public class TestRunner
    {
        private static int _passCount = 0;
        private static int _failCount = 0;

        public static int Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("          MDPlus Comprehensive Test Suite         ");
            Console.WriteLine("==================================================\n");

            var sw = Stopwatch.StartNew();

            // 1. Heading Tests
            RunTest("ATX Headings 1 through 6", TestAtxHeadings);
            RunTest("Setext Headings", TestSetextHeadings);
            RunTest("Heading Anchors", TestHeadingAnchors);

            // 2. Formatting Tests
            RunTest("Bold and Italic Inline Formatting", TestBoldAndItalic);
            RunTest("Strikethrough, Highlight and Code Inline", TestStrikethroughHighlightAndCode);
            RunTest("Escaped Characters in Inlines", TestEscapedCharacters);

            // 3. Frontmatter Tests
            RunTest("YAML Frontmatter and Title Extraction", TestFrontmatter);

            // 4. Code Block Tests
            RunTest("Fenced Code Blocks with Language", TestCodeBlocks);
            RunTest("Unterminated Code Block Handling", TestUnterminatedCodeBlock);

            // 5. Blockquote and Callout Tests
            RunTest("Blockquotes and GitHub Callouts", TestBlockquotesAndCallouts);

            // 6. GFM Table Tests
            RunTest("GFM Tables with Alignments", TestTables);

            // 7. List and Checklist Tests
            RunTest("Unordered, Ordered, and Task Lists", TestListsAndChecklists);

            // 8. Links and Images Tests
            RunTest("Links and Images Parsing", TestLinksAndImages);

            // 9. Document Stats Tests
            RunTest("Word Count and Character Stats", TestDocumentStats);

            // 10. Syntax Highlighter Tests
            RunTest("Syntax Highlighting for C#, Python, JSON", TestSyntaxHighlighter);

            // 11. HTML Exporter Tests
            RunTest("HTML Exporter Full and Snippet Output", TestHtmlExporter);

            // 12. Edge Case & Stress Tests
            RunTest("Empty and Null Input Edge Cases", TestEmptyAndEdgeCases);
            RunTest("Performance Benchmark (5,000 lines)", TestPerformanceBenchmark);

            // 13. Deep Precision & Robustness Tests
            RunTest("Parentheses in Link & Image URLs", TestUrlsWithParentheses);
            RunTest("Soft vs Hard Line Breaks", TestLineBreaksSoftAndHard);
            RunTest("Spaced Thematic Breaks", TestThematicBreaksSpaced);
            RunTest("Ordered List Non-1 Start Index", TestOrderedListStartNumber);
            RunTest("Email Autolinks & HTML Comments", TestAutolinkEmailAndComment);
            RunTest("Escaped Delimiters in Inline Formatting", TestEscapedDelimitersInFormatting);

            sw.Stop();

            Console.WriteLine("\n==================================================");
            Console.WriteLine($"Test Results: {_passCount} PASSED, {_failCount} FAILED in {sw.ElapsedMilliseconds} ms");
            Console.WriteLine("==================================================");

            return _failCount > 0 ? 1 : 0;
        }

        private static void RunTest(string testName, Action testAction)
        {
            try
            {
                testAction();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  [PASS] ");
                Console.ResetColor();
                Console.WriteLine(testName);
                _passCount++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  [FAIL] ");
                Console.ResetColor();
                Console.WriteLine($"{testName}: {ex.Message}");
                _failCount++;
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void AssertEqual<T>(T expected, T actual, string name)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception($"{name} mismatch. Expected: '{expected}', Actual: '{actual}'");
            }
        }

        // --- Tests ---

        private static void TestAtxHeadings()
        {
            var parser = new MarkdownParser();
            string md = @"# Heading 1
## Heading 2
### Heading 3
#### Heading 4
##### Heading 5
###### Heading 6";

            var doc = parser.Parse(md);
            AssertEqual(6, doc.Blocks.Count, "Block count");
            for (int i = 0; i < 6; i++)
            {
                var h = doc.Blocks[i] as HeadingBlock;
                Assert(h != null, $"Block {i} should be HeadingBlock");
                AssertEqual(i + 1, h!.Level, $"Heading level for block {i}");
                AssertEqual($"Heading {i + 1}", h.Text, $"Heading text for block {i}");
            }
        }

        private static void TestSetextHeadings()
        {
            var parser = new MarkdownParser();
            string md = @"Primary Title
=============

Secondary Title
---------------";

            var doc = parser.Parse(md);
            AssertEqual(2, doc.Blocks.Count, "Block count");

            var h1 = doc.Blocks[0] as HeadingBlock;
            Assert(h1 != null, "First block should be HeadingBlock");
            AssertEqual(1, h1!.Level, "H1 level");
            AssertEqual("Primary Title", h1.Text, "H1 text");

            var h2 = doc.Blocks[1] as HeadingBlock;
            Assert(h2 != null, "Second block should be HeadingBlock");
            AssertEqual(2, h2!.Level, "H2 level");
            AssertEqual("Secondary Title", h2.Text, "H2 text");
        }

        private static void TestHeadingAnchors()
        {
            var parser = new MarkdownParser();
            var doc = parser.Parse("# Quick-Start Guide (v2.0)!");
            var h = doc.Blocks[0] as HeadingBlock;
            Assert(h != null, "Block should be HeadingBlock");
            AssertEqual("quick-start-guide-v20", h!.Anchor, "Anchor generation");
        }

        private static void TestBoldAndItalic()
        {
            var parser = new MarkdownParser();
            var inlines = parser.ParseInlines("Normal **Bold** and *Italic* and ***Both***");

            Assert(inlines.Any(i => i is BoldInline), "Should contain BoldInline");
            Assert(inlines.Any(i => i is ItalicInline), "Should contain ItalicInline");
            Assert(inlines.Any(i => i is BoldItalicInline), "Should contain BoldItalicInline");
        }

        private static void TestStrikethroughHighlightAndCode()
        {
            var parser = new MarkdownParser();
            var inlines = parser.ParseInlines("~~Deleted~~ and ==Highlighted== and `Console.WriteLine()`");

            Assert(inlines.Any(i => i is StrikethroughInline), "Should contain StrikethroughInline");
            Assert(inlines.Any(i => i is HighlightInline), "Should contain HighlightInline");
            var code = inlines.OfType<CodeInline>().FirstOrDefault();
            Assert(code != null, "Should contain CodeInline");
            AssertEqual("Console.WriteLine()", code!.Code, "Inline code content");
        }

        private static void TestEscapedCharacters()
        {
            var parser = new MarkdownParser();
            var inlines = parser.ParseInlines(@"\*not italic\* and \[not a link\]");
            Assert(inlines.All(i => i is TextInline), "All should be treated as literal TextInline when escaped");
            string combined = string.Join("", inlines.OfType<TextInline>().Select(t => t.Text));
            Assert(combined.Contains("*not italic*"), "Text should contain literal asterisk");
        }

        private static void TestFrontmatter()
        {
            var parser = new MarkdownParser();
            string md = @"---
title: ""My Project Docs""
author: Nick
date: 2026-09-09
tags: markdown, windows
---

# Content starts here";

            var doc = parser.Parse(md);
            AssertEqual("My Project Docs", doc.Title, "Document title from frontmatter");
            Assert(doc.Frontmatter.ContainsKey("author"), "Frontmatter contains author");
            AssertEqual("Nick", doc.Frontmatter["author"], "Author value");
            Assert(doc.Blocks.Any(b => b is FrontmatterBlock), "Frontmatter block created");
        }

        private static void TestCodeBlocks()
        {
            var parser = new MarkdownParser();
            string md = @"```csharp
public class Worker
{
    public void Work() => Console.WriteLine(""Done"");
}
```";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count, "Block count");
            var cb = doc.Blocks[0] as CodeBlock;
            Assert(cb != null, "Should be CodeBlock");
            AssertEqual("csharp", cb!.Language, "Code language");
            Assert(cb.Code.Contains("public class Worker"), "Code content");
        }

        private static void TestUnterminatedCodeBlock()
        {
            var parser = new MarkdownParser();
            string md = @"```python
def foo():
    return 42";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count, "Block count");
            var cb = doc.Blocks[0] as CodeBlock;
            Assert(cb != null, "Should recover gracefully and create CodeBlock");
            AssertEqual("python", cb!.Language, "Code language");
            Assert(cb.Code.Contains("def foo()"), "Code content");
        }

        private static void TestBlockquotesAndCallouts()
        {
            var parser = new MarkdownParser();
            string md = @"> [!NOTE]
> This is an important system note.

> [!WARNING] Be careful
> Proceed with caution.

> Standard quotation line.";

            var doc = parser.Parse(md);
            AssertEqual(3, doc.Blocks.Count, "Block count");

            var b1 = doc.Blocks[0] as BlockquoteBlock;
            Assert(b1 != null, "First blockquote");
            AssertEqual(CalloutType.Note, b1!.Callout, "Note callout type");

            var b2 = doc.Blocks[1] as BlockquoteBlock;
            Assert(b2 != null, "Second blockquote");
            AssertEqual(CalloutType.Warning, b2!.Callout, "Warning callout type");
            AssertEqual("Be careful", b2.CalloutTitle, "Warning custom title");

            var b3 = doc.Blocks[2] as BlockquoteBlock;
            Assert(b3 != null, "Third blockquote");
            AssertEqual(CalloutType.None, b3!.Callout, "Standard quote callout type");

            // Additional edge cases:
            // 1. Multi-paragraph blockquote with empty '>' line
            var docMulti = parser.Parse("> Para 1\n>\n> Para 2");
            AssertEqual(1, docMulti.Blocks.Count, "Multi-paragraph blockquote count");
            var bMulti = (BlockquoteBlock)docMulti.Blocks[0];
            AssertEqual(2, bMulti.Blocks.Count, "Multi-paragraph inner block count");

            // 2. Nested blockquote
            var docNested = parser.Parse("> > Inner quotation");
            AssertEqual(1, docNested.Blocks.Count, "Nested quote outer count");
            var bOuter = (BlockquoteBlock)docNested.Blocks[0];
            Assert(bOuter.Blocks.Count > 0 && bOuter.Blocks[0] is BlockquoteBlock, "Nested quote inner block is BlockquoteBlock");

            // 3. Multiple consecutive blank lines between quotes
            var docMultiBlank = parser.Parse("> First\n\n\n\n> Second");
            AssertEqual(2, docMultiBlank.Blocks.Count, "Multiple blank lines block count");

            // 4. Callout with only header
            var docCalloutEmpty = parser.Parse("> [!TIP]");
            AssertEqual(1, docCalloutEmpty.Blocks.Count, "Single-line callout block count");
            AssertEqual(CalloutType.Tip, ((BlockquoteBlock)docCalloutEmpty.Blocks[0]).Callout, "Single-line callout type");
        }

        private static void TestTables()
        {
            var parser = new MarkdownParser();
            string md = @"| Header 1 | Header 2 | Header 3 |
| :--- | :---: | ---: |
| Left | Center | Right |
| Val1 | Val2 | Val3 |";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count, "Block count");
            var table = doc.Blocks[0] as TableBlock;
            Assert(table != null, "Should be TableBlock");
            AssertEqual(3, table!.Header.Cells.Count, "Header column count");
            AssertEqual(ColumnAlignment.Left, table.Alignments[0], "Col 0 Left");
            AssertEqual(ColumnAlignment.Center, table.Alignments[1], "Col 1 Center");
            AssertEqual(ColumnAlignment.Right, table.Alignments[2], "Col 2 Right");
            AssertEqual(2, table.Rows.Count, "Data row count");
        }

        private static void TestListsAndChecklists()
        {
            var parser = new MarkdownParser();
            string md = @"- [x] Completed task item
- [ ] Incomplete task item
- Normal unordered item

1. First ordered
2. Second ordered";

            var doc = parser.Parse(md);
            AssertEqual(2, doc.Blocks.Count, "Two lists");

            var list1 = doc.Blocks[0] as ListBlock;
            Assert(list1 != null, "First should be ListBlock");
            Assert(!list1!.IsOrdered, "Should be unordered");
            AssertEqual(3, list1.Items.Count, "Items count");
            Assert(list1.Items[0].IsTask && list1.Items[0].IsChecked, "Task 1 checked");
            Assert(list1.Items[1].IsTask && !list1.Items[1].IsChecked, "Task 2 unchecked");
            Assert(!list1.Items[2].IsTask, "Item 3 normal");

            var list2 = doc.Blocks[1] as ListBlock;
            Assert(list2 != null && list2.IsOrdered, "Second should be ordered ListBlock");
            AssertEqual(2, list2!.Items.Count, "Ordered item count");
        }

        private static void TestLinksAndImages()
        {
            var parser = new MarkdownParser();
            var inlines = parser.ParseInlines("[GitHub](https://github.com \"Official Site\") and ![Logo](logo.png \"Logo Image\")");

            var link = inlines.OfType<LinkInline>().FirstOrDefault();
            Assert(link != null, "Should contain LinkInline");
            AssertEqual("https://github.com", link!.Url, "Link URL");
            AssertEqual("Official Site", link.Title, "Link title");

            var img = inlines.OfType<ImageInline>().FirstOrDefault();
            Assert(img != null, "Should contain ImageInline");
            AssertEqual("logo.png", img!.Url, "Image URL");
            AssertEqual("Logo", img.AltText, "Image alt text");
        }

        private static void TestDocumentStats()
        {
            var parser = new MarkdownParser();
            string md = "One two three four five six seven eight nine ten.";
            var doc = parser.Parse(md);

            AssertEqual(10, doc.WordCount, "Word count");
            Assert(doc.CharacterCount > 40, "Character count");
            AssertEqual(1, doc.ReadingTimeMinutes, "Reading time");
        }

        private static void TestSyntaxHighlighter()
        {
            var csharpTokens = SyntaxHighlighter.Highlight("public class App { private int count = 42; }", "csharp");
            Assert(csharpTokens.Any(t => t.Type == TokenType.Keyword && t.Text == "class"), "C# keyword 'class'");
            Assert(csharpTokens.Any(t => t.Type == TokenType.Number && t.Text == "42"), "C# number '42'");

            var jsonTokens = SyntaxHighlighter.Highlight("{\"name\": \"MDPlus\", \"fast\": true}", "json");
            Assert(jsonTokens.Any(t => t.Type == TokenType.Property), "JSON property");
            Assert(jsonTokens.Any(t => t.Type == TokenType.Keyword && t.Text == "true"), "JSON boolean 'true'");
        }

        private static void TestHtmlExporter()
        {
            var parser = new MarkdownParser();
            string md = @"# Export Test
> [!TIP]
> Tip callout

| Col1 | Col2 |
| :--- | :--- |
| A    | B    |";

            var doc = parser.Parse(md);
            string fullHtml = HtmlExporter.ExportToFullHtml(doc, "Test Document", false);

            Assert(fullHtml.Contains("<!DOCTYPE html>"), "Contains DOCTYPE");
            Assert(fullHtml.Contains("<title>Test Document</title>"), "Contains Title");
            Assert(fullHtml.Contains("<h1 id=\"export-test\">"), "Contains H1 with anchor");
            Assert(fullHtml.Contains("callout-tip"), "Contains callout class");
            Assert(fullHtml.Contains("<table>"), "Contains Table");
        }

        private static void TestEmptyAndEdgeCases()
        {
            var parser = new MarkdownParser();

            var docEmpty = parser.Parse("");
            AssertEqual(0, docEmpty.Blocks.Count, "Empty string produces 0 blocks");

            var docNull = parser.Parse(null!);
            AssertEqual(0, docNull.Blocks.Count, "Null string produces 0 blocks");

            var inlinesEmpty = parser.ParseInlines("");
            AssertEqual(0, inlinesEmpty.Count, "Empty inline produces 0 inlines");

            var docSpaces = parser.Parse("    \n\n\t\n   ");
            AssertEqual(0, docSpaces.Blocks.Count, "Whitespace produces 0 blocks");
        }

        private static void TestPerformanceBenchmark()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 500; i++)
            {
                sb.AppendLine($"# Heading Level {i % 6 + 1}");
                sb.AppendLine("This is a paragraph with **bold text**, *italic*, `inline code` and [links](https://example.com).");
                sb.AppendLine("> [!NOTE]");
                sb.AppendLine("> Important alert in repetitive stress test.");
                sb.AppendLine("```csharp");
                sb.AppendLine("var x = 123; // comment");
                sb.AppendLine("```");
                sb.AppendLine("| Header A | Header B |");
                sb.AppendLine("| :--- | :---: |");
                sb.AppendLine("| Value 1 | Value 2 |");
                sb.AppendLine("- [x] Completed task");
                sb.AppendLine("- [ ] Pending task");
                sb.AppendLine("---");
            }

            string largeMd = sb.ToString();
            var parser = new MarkdownParser();

            var sw = Stopwatch.StartNew();
            var doc = parser.Parse(largeMd);
            sw.Stop();

            Console.Write($" [{doc.Blocks.Count} blocks parsed in {sw.ElapsedMilliseconds}ms] ");
            // Expect 500 iterations * ~6 blocks = ~3000 blocks in under 150ms!
            Assert(sw.ElapsedMilliseconds < 500, $"Parsing should take under 500ms, took {sw.ElapsedMilliseconds}ms");
            Assert(doc.Blocks.Count > 2500, $"Expected >2500 blocks, got {doc.Blocks.Count}");
        }

        private static void TestUrlsWithParentheses()
        {
            var parser = new MarkdownParser();
            string md = "[Wikipedia C++](https://en.wikipedia.org/wiki/C++_(programming_language)) and ![Image](https://example.com/image_(1).png \"Title\")";
            var inlines = parser.ParseInlines(md);

            var link = inlines.OfType<LinkInline>().FirstOrDefault();
            Assert(link != null, "Link with parens should be found");
            AssertEqual("https://en.wikipedia.org/wiki/C++_(programming_language)", link!.Url, "Full URL with parens preserved");

            var img = inlines.OfType<ImageInline>().FirstOrDefault();
            Assert(img != null, "Image with parens should be found");
            AssertEqual("https://example.com/image_(1).png", img!.Url, "Image URL with parens preserved");
            AssertEqual("Title", img.Title, "Image title preserved");
        }

        private static void TestLineBreaksSoftAndHard()
        {
            var parser = new MarkdownParser();
            string md = "First line\nSecond line  \nThird line\\\nFourth line";
            var inlines = parser.ParseInlines(md);

            var breaks = inlines.OfType<LineBreakInline>().ToList();
            AssertEqual(3, breaks.Count, "Should find 3 breaks");
            Assert(!breaks[0].IsHard, "First break should be soft break");
            Assert(breaks[1].IsHard, "Second break (two trailing spaces) should be hard break");
            Assert(breaks[2].IsHard, "Third break (trailing backslash) should be hard break");
        }

        private static void TestThematicBreaksSpaced()
        {
            var parser = new MarkdownParser();
            string md = "- - -\n\n* * *\n\n_ _ _";
            var doc = parser.Parse(md);

            AssertEqual(3, doc.Blocks.Count, "Should produce 3 blocks");
            Assert(doc.Blocks.All(b => b is ThematicBreakBlock), "All 3 should be ThematicBreakBlock");
        }

        private static void TestOrderedListStartNumber()
        {
            var parser = new MarkdownParser();
            string md = "5. Fifth item\n6. Sixth item";
            var doc = parser.Parse(md);

            AssertEqual(1, doc.Blocks.Count, "Should produce 1 list block");
            var list = doc.Blocks[0] as ListBlock;
            Assert(list != null, "Should be ListBlock");
            Assert(list!.IsOrdered, "Should be ordered list");
            AssertEqual(5, list.StartNumber, "StartNumber should be 5");
            AssertEqual(2, list.Items.Count, "Should have 2 items");
        }

        private static void TestAutolinkEmailAndComment()
        {
            var parser = new MarkdownParser();
            string md = "Contact <support@mdplus.dev> or visit <https://mdplus.dev> <!-- hidden text --> hello";
            var inlines = parser.ParseInlines(md);

            var links = inlines.OfType<LinkInline>().ToList();
            AssertEqual(2, links.Count, "Should parse 2 autolinks");
            AssertEqual("mailto:support@mdplus.dev", links[0].Url, "Email mailto autolink");
            AssertEqual("https://mdplus.dev", links[1].Url, "HTTPS autolink");

            string fullText = string.Join("", inlines.OfType<TextInline>().Select(t => t.Text));
            Assert(!fullText.Contains("hidden text"), "HTML comment should be stripped from visible text");
        }

        private static void TestEscapedDelimitersInFormatting()
        {
            var parser = new MarkdownParser();
            string md = "*italic with literal \\* inside* and **bold with literal \\** inside**";
            var inlines = parser.ParseInlines(md);

            var italic = inlines.OfType<ItalicInline>().FirstOrDefault();
            Assert(italic != null, "Should parse ItalicInline");

            var bold = inlines.OfType<BoldInline>().FirstOrDefault();
            Assert(bold != null, "Should parse BoldInline");
        }
    }
}
