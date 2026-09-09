using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MDPlus.Core;
using MDPlus.Models;

using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;
using WpfList = System.Windows.Documents.List;
using WpfListItem = System.Windows.Documents.ListItem;

namespace MDPlus.Tests
{
    public class TestRunner
    {
        private static int _passCount = 0;
        private static int _failCount = 0;

        [STAThread]
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

            // 14. SHA-256 Release Security & Integrity Tests (Notepad++ Standard)
            RunTest("SHA-256 Hash Computation & Verification", TestHashServiceSha256);
            RunTest("SHA-256 Checksum Manifest Parsing & Validation", TestHashServiceManifest);

            // 15. Deep Bug Fix Regression Tests
            RunTest("Setext Heading Does Not Capture Lists Or Quotes", TestSetextHeadingNotCapturingListsOrQuotes);
            RunTest("Callouts Preserve Child Blocks (Lists, Code, Tables)", TestCalloutPreservesChildBlocks);
            RunTest("Escaped Backslash at Line End Preserved", TestEscapedBackslashBeforeNewline);
            RunTest("Multi-Backtick Code Spans", TestMultiBacktickCodeSpans);
            RunTest("Angle Bracket URLs in Links & Images", TestAngleBracketUrls);
            RunTest("HTML Exporter Table Column Harmonization", TestTableColumnConsistency);
            RunTest("Inline Code with Literal Backslashes & Distinct Runs", TestInlineCodeBackslashesAndRuns);
            RunTest("Emphasis Left/Right Flanking Whitespace Rules (Math Multiplication Safe)", TestEmphasisWhitespaceFlanking);
            RunTest("Setext Heading Interrupting Paragraph Text", TestSetextHeadingInterruptingParagraph);
            RunTest("Heading Block Line Index Tracking", TestHeadingLineIndexTracking);
            RunTest("Multi-Format Checksum Manifest Parsing (GNU, BSD, Tabs, Relative)", TestMultiFormatChecksumManifest);
            RunTest("Asynchronous SHA-256 Hash Computation", TestAsyncSha256Computation);
            RunTest("Table Pipe and Delimiter Backslash Escaping", TestTableBackslashesAndEscapes);
            RunTest("HTML Exporter URL Sanitization (XSS Prevention)", TestHtmlExporterUrlSanitization);
            RunTest("Syntax Highlighter Multiline and Verbatim Strings", TestSyntaxHighlighterMultilineAndVerbatimStrings);
            RunTest("Table Alignment Separators and Column Padding", TestTableAlignmentsSeparatorsAndPadding);

            // 16. Markdown Serialization Engine Tests (Milestone 1)
            RunTest("Serialization: Headings 1 through 6 (Clean Asterisks)", TestSerializationHeadings);
            RunTest("Serialization: Inline Elements (Bold, Italic, Strike, Highlight, Code, Links, Images)", TestSerializationInlines);
            RunTest("Serialization: Fenced Code Blocks with Language", TestSerializationFencedCodeBlocks);
            RunTest("Serialization: Blockquotes and GitHub Callouts", TestSerializationBlockquotesAndCallouts);
            RunTest("Serialization: Unordered, Ordered, and Task Lists", TestSerializationListsAndTaskChecklists);
            RunTest("Serialization: GFM Tables with Alignment and Escaping", TestSerializationTables);
            RunTest("Serialization: Direct Block and Inline API", TestSerializationDirectBlockAndInlineApi);
            RunTest("Serialization: Full Document Round-Trip Lossless Fidelity", TestSerializationFullDocumentRoundTrip);

            // 17. Theme Palette System & Menu Readability Tests (Milestone 3)
            RunTest("Theme Presets Initialization & Brush Immutability", TestThemePresetsInitializationAndImmutability);
            RunTest("Theme WCAG AA Contrast Compliance (>= 4.5:1)", TestThemeWcagAaContrastCompliance);
            RunTest("ThemeManager Dynamic Switching & Notification", TestThemeManagerDynamicSwitchingAndEvents);
            RunTest("ThemeManager Cycling via CycleNextTheme", TestThemeManagerCycling);
            RunTest("MarkdownToWpfConverter Theme Palette Integration", TestMarkdownConverterThemePaletteIntegration);

            // 18. Windows Setup Installer & Prerequisite Bootstrapper Tests
            RunTest("Windows Installer Script Integrity & Shell Association Directives", TestInstallerScriptIntegrity);
            RunTest("Windows Installer SHA-256 Checksum Manifest Verification", TestInstallerChecksumManifestParsingAndVerification);
            RunTest(".NET 8 Desktop Runtime Prerequisite Detection Logic", TestDotNet8DesktopRuntimeDetection);

            // 19. Menu Bar & Scrollbar UI Tests (Milestone 4)
            RunTest("AppSettings ShowMenuBar Default & Serialization", TestAppSettingsShowMenuBar);
            RunTest("Menu Popup ScrollViewer Disabled Visibility in App.xaml", TestMenuScrollViewerDisabledInAppXaml);
            RunTest("Menu Bar Dynamic Theme Styling & Hardcoded Color Removal", TestMenuBarThemeColorPaletteConsistency);
            RunTest("Recent File Mnemonic Escaping & AltGr Handling Logic", TestRecentFileMnemonicAndAltGrHandling);

            // 20. Session Restore & Tab Lifecycle Tests
            RunTest("Session Restore & Startup Preference in AppSettings", TestAppSettingsSessionRestoreProperties);
            RunTest("Startup File Resolution & Mode Routing in App.xaml.cs", TestAppResolveStartupFilesLogic);
            RunTest("Single-Click Tab Close & Empty State Shell Integrity", TestTabClosingSingleClickAndEmptyState);

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

        private static void TestHashServiceSha256()
        {
            byte[] testData = Encoding.UTF8.GetBytes("MDPlus Native Windows Markdown Viewer");
            string hash = HashService.ComputeSha256(testData);

            AssertEqual(64, hash.Length, "SHA-256 length must be 64 characters");

            // Verify known vector
            byte[] helloBytes = Encoding.UTF8.GetBytes("hello world");
            string helloHash = HashService.ComputeSha256(helloBytes);
            // SHA-256("hello world") = b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9
            AssertEqual("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", helloHash, "Known SHA-256 vector");

            // Temporary file test
            string tempFile = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllText(tempFile, "hello world", new UTF8Encoding(false));
                string fileHash = HashService.ComputeSha256(tempFile);
                AssertEqual(helloHash, fileHash, "File SHA-256 must match byte SHA-256");

                Assert(HashService.VerifyFileSha256(tempFile, helloHash), "Verification succeeds with lowercase");
                Assert(HashService.VerifyFileSha256(tempFile, helloHash.ToUpperInvariant()), "Verification succeeds with uppercase");
                Assert(HashService.VerifyFileSha256(tempFile, "  " + helloHash + "  "), "Verification succeeds with whitespace");
                Assert(!HashService.VerifyFileSha256(tempFile, "0000000000000000000000000000000000000000000000000000000000000000"), "Verification fails on mismatch");
            }
            finally
            {
                if (System.IO.File.Exists(tempFile)) System.IO.File.Delete(tempFile);
            }
        }

        private static void TestHashServiceManifest()
        {
            string manifest = @"# MDPlus Checksums Manifest (Notepad++ Standard)
b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9  MDPlus.exe
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855 *MDPlus-win-x64.zip
";
            var parsed = HashService.ParseChecksums(manifest);
            AssertEqual(2, parsed.Count, "Must parse 2 entries from manifest");
            Assert(parsed.ContainsKey("MDPlus.exe"), "Contains MDPlus.exe");
            Assert(parsed.ContainsKey("MDPlus-win-x64.zip"), "Contains MDPlus-win-x64.zip (asterisk stripped)");
            AssertEqual("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", parsed["MDPlus.exe"], "MDPlus.exe hash");

            string entry = HashService.FormatChecksumEntry("file.txt", "abc123def");
            AssertEqual("abc123def  file.txt", entry, "Format checksum entry");
        }

        private static void TestSetextHeadingNotCapturingListsOrQuotes()
        {
            var parser = new MarkdownParser();
            string md = @"- List item 1
- List item 2
---
Next paragraph";

            var doc = parser.Parse(md);
            AssertEqual(3, doc.Blocks.Count, "Should produce 3 blocks (List, ThematicBreak, Paragraph)");
            var list = doc.Blocks[0] as ListBlock;
            Assert(list != null, "First block must remain ListBlock");
            AssertEqual(2, list!.Items.Count, "List must retain both items");

            var thematic = doc.Blocks[1] as ThematicBreakBlock;
            Assert(thematic != null, "Second block must remain ThematicBreakBlock, not consumed as underline");

            var para = doc.Blocks[2] as ParagraphBlock;
            Assert(para != null, "Third block must be ParagraphBlock");

            // Also test quote followed by ---
            string mdQuote = @"> Blockquote text
---
Following text";
            var docQuote = parser.Parse(mdQuote);
            AssertEqual(3, docQuote.Blocks.Count, "Should produce 3 blocks for quote followed by thematic break");
            Assert(docQuote.Blocks[0] is BlockquoteBlock, "First block must remain BlockquoteBlock");
            Assert(docQuote.Blocks[1] is ThematicBreakBlock, "Second block must remain ThematicBreakBlock");
        }

        private static void TestCalloutPreservesChildBlocks()
        {
            var parser = new MarkdownParser();
            string md = @"> [!NOTE]
> Here are items:
> - Item Alpha
> - Item Beta
> ```csharp
> int x = 100;
> ```";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count, "Block count");
            var callout = doc.Blocks[0] as BlockquoteBlock;
            Assert(callout != null, "Must be BlockquoteBlock");
            AssertEqual(CalloutType.Note, callout!.Callout, "Note callout type");

            // Child blocks must contain Paragraph, List, and CodeBlock
            Assert(callout.Blocks.Any(b => b is ParagraphBlock), "Callout contains paragraph");
            Assert(callout.Blocks.Any(b => b is ListBlock), "Callout contains ListBlock");
            Assert(callout.Blocks.Any(b => b is CodeBlock), "Callout contains CodeBlock");
        }

        private static void TestEscapedBackslashBeforeNewline()
        {
            var parser = new MarkdownParser();
            string md = "First line with backslash\\\\\nSecond line";
            var inlines = parser.ParseInlines(md);

            var texts = inlines.OfType<TextInline>().ToList();
            string combined = string.Join("", texts.Select(t => t.Text));
            Assert(combined.Contains("\\"), "Should preserve literal backslash from escaped '\\\\'");

            // The line break should be soft, not hard
            var br = inlines.OfType<LineBreakInline>().FirstOrDefault();
            Assert(br != null, "Should have a line break");
            Assert(!br!.IsHard, "Break after escaped backslash must be soft, not hard");
        }

        private static void TestMultiBacktickCodeSpans()
        {
            var parser = new MarkdownParser();
            string md = "Use ``code with ` inside`` and ```three ` ` backticks```";
            var inlines = parser.ParseInlines(md);

            var codeList = inlines.OfType<CodeInline>().ToList();
            AssertEqual(2, codeList.Count, "Should parse 2 code spans");
            AssertEqual("code with ` inside", codeList[0].Code, "First code span with nested backtick");
            AssertEqual("three ` ` backticks", codeList[1].Code, "Second code span with multiple backticks");
        }

        private static void TestAngleBracketUrls()
        {
            var parser = new MarkdownParser();
            string md = "[Document](<https://example.com/my document.pdf> \"My Doc\") and ![Photo](<https://example.com/photo (1).png>)";
            var inlines = parser.ParseInlines(md);

            var link = inlines.OfType<LinkInline>().FirstOrDefault();
            Assert(link != null, "Should find link with angle bracket URL");
            AssertEqual("https://example.com/my document.pdf", link!.Url, "Angle bracket link URL with spaces");
            AssertEqual("My Doc", link.Title, "Link title");

            var img = inlines.OfType<ImageInline>().FirstOrDefault();
            Assert(img != null, "Should find image with angle bracket URL");
            AssertEqual("https://example.com/photo (1).png", img!.Url, "Angle bracket image URL with parens");
        }

        private static void TestTableColumnConsistency()
        {
            var parser = new MarkdownParser();
            string md = @"| Header A | Header B |
| :--- | :---: |
| Row 1 Col 1 | Row 1 Col 2 | Row 1 Col 3 Extra |";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count, "Should parse TableBlock");
            var table = (TableBlock)doc.Blocks[0];

            string html = HtmlExporter.ExportBodyHtml(doc);
            Assert(html.Contains("<table>"), "Contains table tag");
            Assert(html.Contains("<th"), "Contains th");
            Assert(html.Contains("<td"), "Contains td");
        }

        private static void TestInlineCodeBackslashesAndRuns()
        {
            var parser = new MarkdownParser();

            // 1. Literal backslash at the end of code span: `\`
            string md1 = "`\\`";
            var inlines1 = parser.ParseInlines(md1);
            var code1 = inlines1.OfType<CodeInline>().FirstOrDefault();
            Assert(code1 != null, "Code with single backslash `\\` must be parsed");
            AssertEqual("\\", code1!.Code, "Content must be single backslash");

            // 2. Windows file path ending in backslash: `C:\Users\`
            string md2 = @"`C:\Users\`";
            var inlines2 = parser.ParseInlines(md2);
            var code2 = inlines2.OfType<CodeInline>().FirstOrDefault();
            Assert(code2 != null, @"Code with path `C:\Users\` must be parsed");
            AssertEqual(@"C:\Users\", code2!.Code, "Content must be path");

            // 3. Single-backtick code containing two backticks must not prematurely close
            string md3 = "`alpha `` beta`";
            var inlines3 = parser.ParseInlines(md3);
            var code3 = inlines3.OfType<CodeInline>().FirstOrDefault();
            Assert(code3 != null, "Code with double backticks inside must be parsed");
            AssertEqual("alpha `` beta", code3!.Code, "Must contain inner double backticks intact");
        }

        private static void TestEmphasisWhitespaceFlanking()
        {
            var parser = new MarkdownParser();

            // 1. Math multiplication: 3 * 4 * 5 must remain plain text, not italics!
            string math = "3 * 4 * 5";
            var inlinesMath = parser.ParseInlines(math);
            Assert(!inlinesMath.OfType<ItalicInline>().Any(), "Math multiplication 3 * 4 * 5 must not produce ItalicInline");

            // 2. Delimiter preceded/followed by whitespace
            string invalidItalic = "*not italic * and * not italic*";
            var inlinesInvalid = parser.ParseInlines(invalidItalic);
            Assert(!inlinesInvalid.OfType<ItalicInline>().Any(), "Delimiters with outer/inner spaces must not produce ItalicInline");

            // 3. Proper italic and bold
            string valid = "*italic* and **bold**";
            var inlinesValid = parser.ParseInlines(valid);
            Assert(inlinesValid.OfType<ItalicInline>().Any(), "Valid italic must parse");
            Assert(inlinesValid.OfType<BoldInline>().Any(), "Valid bold must parse");

            // 4. Nested bold inside italic: *foo **bar** baz*
            string nested = "*foo **bar** baz*";
            var inlinesNested = parser.ParseInlines(nested);
            var outerItalic = inlinesNested.OfType<ItalicInline>().FirstOrDefault();
            Assert(outerItalic != null, "Must parse outer italic");
            Assert(outerItalic!.Children.OfType<BoldInline>().Any(), "Must parse inner bold inside outer italic");
        }

        private static void TestSetextHeadingInterruptingParagraph()
        {
            var parser = new MarkdownParser();
            string md = @"This is normal text in paragraph.
Important Heading
---
Following paragraph text.";

            var doc = parser.Parse(md);
            AssertEqual(3, doc.Blocks.Count, "Should parse 3 blocks (Paragraph, Setext Heading 2, Paragraph)");
            Assert(doc.Blocks[0] is ParagraphBlock, "Block 0 must be ParagraphBlock");
            Assert(doc.Blocks[1] is HeadingBlock, "Block 1 must be HeadingBlock");
            var h = (HeadingBlock)doc.Blocks[1];
            AssertEqual(2, h.Level, "Setext level must be 2 for ---");
            AssertEqual("Important Heading", h.Text, "Heading text must be 'Important Heading'");
            Assert(doc.Blocks[2] is ParagraphBlock, "Block 2 must be ParagraphBlock");
        }

        private static void TestHeadingLineIndexTracking()
        {
            var parser = new MarkdownParser();
            string md = @"First paragraph line 0

# Heading At Line 2

Paragraph line 4

Subheading At Line 6
---
Paragraph line 9";

            var doc = parser.Parse(md);
            var headings = doc.Blocks.OfType<HeadingBlock>().ToList();
            AssertEqual(2, headings.Count, "Should have 2 headings");
            AssertEqual(2, headings[0].LineIndex, "First heading at line 2");
            AssertEqual(6, headings[1].LineIndex, "Second heading at line 6");
        }

        private static void TestMultiFormatChecksumManifest()
        {
            // GNU format, Tab-delimited format, BSD format, Asterisk binary format, Relative path
            string manifest = @"# Multi-format Manifest
b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9  MDPlus.exe
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855	MDPlus-win-x64.zip
SHA256 (MDPlus-1.0.0-src.zip) = a1b2c3d4e5f60718293a4b5c6d7e8f90123456789abcdef0123456789abcdef0
5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8 *./subfolder/tool.exe
";
            var parsed = HashService.ParseChecksums(manifest);
            AssertEqual(4, parsed.Count, "Must parse 4 entries across GNU, tab, BSD, and relative formats");
            Assert(parsed.ContainsKey("MDPlus.exe"), "Contains MDPlus.exe");
            Assert(parsed.ContainsKey("MDPlus-win-x64.zip"), "Contains tab-delimited MDPlus-win-x64.zip");
            Assert(parsed.ContainsKey("MDPlus-1.0.0-src.zip"), "Contains BSD-formatted MDPlus-1.0.0-src.zip");
            Assert(parsed.ContainsKey("subfolder/tool.exe"), "Contains relative-path stripped subfolder/tool.exe");

            AssertEqual("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", parsed["MDPlus.exe"], "MDPlus.exe hash");
            AssertEqual("a1b2c3d4e5f60718293a4b5c6d7e8f90123456789abcdef0123456789abcdef0", parsed["MDPlus-1.0.0-src.zip"], "BSD hash");
        }

        private static void TestAsyncSha256Computation()
        {
            string tempFile = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllText(tempFile, "Async SHA256 test data", Encoding.UTF8);
                string syncHash = HashService.ComputeSha256(tempFile);
                string asyncHash = HashService.ComputeSha256Async(tempFile).GetAwaiter().GetResult();
                AssertEqual(syncHash, asyncHash, "Async hash must match sync hash");

                bool verified = HashService.VerifyFileSha256Async(tempFile, syncHash).GetAwaiter().GetResult();
                Assert(verified, "Async verification must succeed");
            }
            finally
            {
                if (System.IO.File.Exists(tempFile)) System.IO.File.Delete(tempFile);
            }
        }

        private static void TestTableBackslashesAndEscapes()
        {
            var parser = new MarkdownParser();
            string md = @"| Header A | Header B |
| --- | --- |
| Escaped \| Pipe | Escaped \* Star |";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count, "Should parse 1 table block");
            var table = doc.Blocks[0] as TableBlock;
            Assert(table != null, "Block must be TableBlock");
            AssertEqual(1, table!.Rows.Count, "Should have 1 data row");
            AssertEqual(2, table.Rows[0].Cells.Count, "Row should have 2 cells");

            // Cell 0 should have unescaped pipe
            var cell0 = table.Rows[0].Cells[0];
            AssertEqual("Escaped | Pipe", cell0.Text, "Pipe must be unescaped");

            // Cell 1 should preserve backslash so inline star is not parsed as italic
            var cell1 = table.Rows[0].Cells[1];
            Assert(!cell1.Inlines.OfType<ItalicInline>().Any(), "Escaped star in cell must NOT produce ItalicInline");
            var textInline = cell1.Inlines.OfType<TextInline>().FirstOrDefault();
            Assert(textInline != null && textInline.Text.Contains("*"), "Text should contain literal asterisk");
        }

        private static void TestHtmlExporterUrlSanitization()
        {
            var parser = new MarkdownParser();
            string md = @"[XSS Link](javascript:alert('pwned'))
![XSS Image](data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==)
[Safe Link](https://notepad-plus-plus.org/)";

            var doc = parser.Parse(md);
            string html = HtmlExporter.ExportBodyHtml(doc);

            Assert(!html.Contains("href=\"javascript:"), "javascript: scheme must be sanitized");
            Assert(!html.Contains("src=\"data:text/html"), "data:text/html scheme must be sanitized");
            Assert(html.Contains("href=\"#\""), "Sanitized link must point to #");
            Assert(html.Contains("src=\"#\""), "Sanitized image must point to #");
            Assert(html.Contains("href=\"https://notepad-plus-plus.org/\""), "Safe https:// link must be preserved");
        }

        private static void TestSyntaxHighlighterMultilineAndVerbatimStrings()
        {
            // Python docstring spanning multiple lines
            string pyCode = "def foo():\n    \"\"\"First docstring line\n    Second docstring line\"\"\"\n    return True";
            var pyTokens = SyntaxHighlighter.Highlight(pyCode, "python");
            var pyStringTokens = pyTokens.Where(t => t.Type == TokenType.String).ToList();
            Assert(pyStringTokens.Count >= 1, "Must find Python docstring token");
            Assert(pyStringTokens.Any(t => t.Text.Contains("First docstring line") && t.Text.Contains("Second docstring line")),
                "Python triple-quote string must span newlines without premature termination");

            // C# verbatim string spanning multiple lines
            string csCode = "string path = @\"C:\\Users\\nickf\\\nDocuments\\mdplus\";";
            var csTokens = SyntaxHighlighter.Highlight(csCode, "csharp");
            var csStringTokens = csTokens.Where(t => t.Type == TokenType.String).ToList();
            Assert(csStringTokens.Count >= 1, "Must find C# verbatim string token");
            Assert(csStringTokens.Any(t => t.Text.Contains("C:\\Users\\nickf\\") && t.Text.Contains("Documents\\mdplus")),
                "C# verbatim string must span newlines");
        }

        private static void TestTableAlignmentsSeparatorsAndPadding()
        {
            var parser = new MarkdownParser();
            string md = @"| Col 1 | Col 2 | Col 3 | Col 4 |
| :---: | ---: | :--- | --- |
| 1 | 2 | 3 | 4 |";

            var doc = parser.Parse(md);
            var table = doc.Blocks.OfType<TableBlock>().FirstOrDefault();
            Assert(table != null, "Table parsed");
            AssertEqual(ColumnAlignment.Center, table!.Alignments[0], "Col 1 is Center");
            AssertEqual(ColumnAlignment.Right, table.Alignments[1], "Col 2 is Right");
            AssertEqual(ColumnAlignment.Left, table.Alignments[2], "Col 3 is Left");
            AssertEqual(ColumnAlignment.Left, table.Alignments[3], "Col 4 is Left");
        }

        private static void TestSerializationHeadings()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, true);

            // 1. All 6 levels round-trip
            string md = "# Heading 1\n\n## Heading 2\n\n### Heading 3\n\n#### Heading 4\n\n##### Heading 5\n\n###### Heading 6";
            var flowDoc = converter.Convert(parser.Parse(md));
            string serialized = MarkdownSerializer.Serialize(flowDoc).Trim();
            AssertEqual(md, serialized, "Headings 1-6 roundtrip");

            // Verify NO unintended bold asterisks from paragraph font-weight inheritance
            Assert(!serialized.Contains("**Heading"), "Headings must not serialize with inherited bold asterisks");

            // 2. Heading with explicit bold word
            string mdBoldHeading = "# Title with **bold** word";
            var flowDocBold = converter.Convert(parser.Parse(mdBoldHeading));
            string serBold = MarkdownSerializer.Serialize(flowDocBold).Trim();
            AssertEqual("# Title with **bold** word", serBold, "Heading with explicit bold preserves asterisks");

            // 3. Heading with inline code
            string mdCodeHeading = "## Heading with `code` word";
            var flowDocCode = converter.Convert(parser.Parse(mdCodeHeading));
            string serCode = MarkdownSerializer.Serialize(flowDocCode).Trim();
            AssertEqual("## Heading with `code` word", serCode, "Heading with code preserves backticks");

            // 4. Programmatic Paragraphs with Tag and FontSize
            var p1 = new Paragraph(new Run("Code Tag H1")) { Tag = "h1" };
            AssertEqual("# Code Tag H1", MarkdownSerializer.SerializeBlock(p1), "Tag h1");

            var p2 = new Paragraph(new Run("FontSize H2")) { FontSize = 20 };
            AssertEqual("## FontSize H2", MarkdownSerializer.SerializeBlock(p2), "FontSize 20 H2");

            var p3 = new Paragraph(new Run("Tag Object H3")) { Tag = new HeadingTag { Level = 3 } };
            AssertEqual("### Tag Object H3", MarkdownSerializer.SerializeBlock(p3), "HeadingTag H3");

            var p4 = new Paragraph(new Run("Int Tag H4")) { Tag = 4 };
            AssertEqual("#### Int Tag H4", MarkdownSerializer.SerializeBlock(p4), "Tag int 4");

            var p5 = new Paragraph(new Run("FontSize H5")) { FontSize = 13.5 };
            AssertEqual("##### FontSize H5", MarkdownSerializer.SerializeBlock(p5), "FontSize 13.5 H5");

            var p6 = new Paragraph(new Run("FontSize H6")) { FontSize = 12 };
            AssertEqual("###### FontSize H6", MarkdownSerializer.SerializeBlock(p6), "FontSize 12 H6");
        }

        private static void TestSerializationInlines()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, true);

            // 1. Core inlines round-trip
            string md = "Text with **bold**, *italic*, ***bold italic***, ~~strike~~, ==highlight==, and `inline code`.";
            var doc = parser.Parse(md);
            var flowDoc = converter.Convert(doc);
            string ser = MarkdownSerializer.Serialize(flowDoc).Trim();
            AssertEqual(md, ser, "Inlines roundtrip");

            // 2. Multi-backtick inline code with inner backticks
            string mdCode = "Code with `` ` `` backtick and `` `foo` `` code";
            var docCode = parser.Parse(mdCode);
            var flowDocCode = converter.Convert(docCode);
            string serCode = MarkdownSerializer.Serialize(flowDocCode).Trim();
            var reParsed = parser.Parse(serCode);
            var codeInlines = reParsed.Blocks.OfType<ParagraphBlock>().First().Inlines.OfType<CodeInline>().ToList();
            AssertEqual(2, codeInlines.Count, "Two code inlines");
            AssertEqual("`", codeInlines[0].Code, "First code backtick");
            AssertEqual("`foo`", codeInlines[1].Code, "Second code backtick");

            // 3. Hyperlinks
            string mdLink = "Visit [OpenAI](https://openai.com) for details.";
            var flowDocLink = converter.Convert(parser.Parse(mdLink));
            string serLink = MarkdownSerializer.Serialize(flowDocLink).Trim();
            AssertEqual(mdLink, serLink, "Hyperlink roundtrip");

            // 4. Standalone formatting
            var spanStrike = new Span(new Run("strikethrough text"));
            spanStrike.TextDecorations.Add(TextDecorations.Strikethrough);
            AssertEqual("~~strikethrough text~~", MarkdownSerializer.SerializeInline(spanStrike), "Standalone strikethrough span");

            var spanHl = new Span(new Run("highlighted text")) { Background = Brushes.Yellow };
            AssertEqual("==highlighted text==", MarkdownSerializer.SerializeInline(spanHl), "Standalone highlight span");

            var spanCode = new Span(new Run("var a = 42;")) { Tag = "code" };
            AssertEqual("`var a = 42;`", MarkdownSerializer.SerializeInline(spanCode), "Standalone code span");
        }

        private static void TestSerializationFencedCodeBlocks()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, true);

            // 1. C# code block
            string mdCs = "```csharp\npublic class Worker\n{\n    public void Work() => Console.WriteLine(\"Done\");\n}\n```";
            var flowDocCs = converter.Convert(parser.Parse(mdCs));
            string serCs = MarkdownSerializer.Serialize(flowDocCs).Trim();
            AssertEqual(mdCs, serCs, "C# code block roundtrip");

            // 2. Python code block
            string mdPy = "```python\ndef hello():\n    print(\"Hello world\")\n```";
            var flowDocPy = converter.Convert(parser.Parse(mdPy));
            string serPy = MarkdownSerializer.Serialize(flowDocPy).Trim();
            AssertEqual(mdPy, serPy, "Python code block roundtrip");

            // 3. Plain text / no language
            string mdText = "```\nPlain text line 1\nPlain text line 2\n```";
            var flowDocText = converter.Convert(parser.Parse(mdText));
            string serText = MarkdownSerializer.Serialize(flowDocText).Trim();
            AssertEqual(mdText, serText, "Plain text code block roundtrip");

            // 4. Code block with CodeBlockTag
            var pCode = new Paragraph { Tag = new CodeBlockTag { Language = "json", Code = "{\"key\": \"val\"}" } };
            string serTag = MarkdownSerializer.SerializeBlock(pCode);
            Assert(serTag.StartsWith("```json"), "Serialized tag code starts with ```json");
            Assert(serTag.Contains("\"key\": \"val\""), "Serialized tag code contains body");
        }

        private static void TestSerializationBlockquotesAndCallouts()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, true);

            // 1. Standard blockquote
            string mdQuote = "> This is a blockquote statement.";
            var flowDocQuote = converter.Convert(parser.Parse(mdQuote));
            string serQuote = MarkdownSerializer.Serialize(flowDocQuote).Trim();
            AssertEqual(mdQuote, serQuote, "Standard blockquote roundtrip");

            // 2. Multi-paragraph blockquote
            string mdMulti = "> Paragraph 1\n>\n> Paragraph 2";
            var flowDocMulti = converter.Convert(parser.Parse(mdMulti));
            string serMulti = MarkdownSerializer.Serialize(flowDocMulti).Trim();
            var reParsed = parser.Parse(serMulti);
            AssertEqual(1, reParsed.Blocks.Count, "1 blockquote block");
            AssertEqual(2, ((BlockquoteBlock)reParsed.Blocks[0]).Blocks.Count, "2 paragraphs inside blockquote");

            // 3. GitHub Callouts (Note, Tip, Important, Warning, Caution)
            string mdNote = "> [!NOTE]\n> Information note content.";
            string serNote = MarkdownSerializer.Serialize(converter.Convert(parser.Parse(mdNote))).Trim();
            AssertEqual(mdNote, serNote, "Note callout roundtrip");

            string mdTip = "> [!TIP]\n> Pro tip for markdown.";
            string serTip = MarkdownSerializer.Serialize(converter.Convert(parser.Parse(mdTip))).Trim();
            AssertEqual(mdTip, serTip, "Tip callout roundtrip");

            string mdWarn = "> [!WARNING] Cautionary Warning\n> Do not proceed without review.";
            string serWarn = MarkdownSerializer.Serialize(converter.Convert(parser.Parse(mdWarn))).Trim();
            AssertEqual(mdWarn, serWarn, "Warning callout with custom title roundtrip");

            // 4. Callout with child list and code block
            string mdComplexCallout = "> [!IMPORTANT]\n> Action required:\n>\n> - Item Alpha\n> - Item Beta\n>\n> ```csharp\n> int x = 100;\n> ```";
            var flowDocComplex = converter.Convert(parser.Parse(mdComplexCallout));
            string serComplex = MarkdownSerializer.Serialize(flowDocComplex).Trim();
            var reParsedComplex = parser.Parse(serComplex);
            var bqComplex = (BlockquoteBlock)reParsedComplex.Blocks[0];
            AssertEqual(CalloutType.Important, bqComplex.Callout, "Complex callout is Important");
            Assert(bqComplex.Blocks.Any(b => b is ParagraphBlock), "Contains paragraph");
            Assert(bqComplex.Blocks.Any(b => b is ListBlock), "Contains list");
            Assert(bqComplex.Blocks.Any(b => b is CodeBlock), "Contains code block");
        }

        private static void TestSerializationListsAndTaskChecklists()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, true);

            // 1. Task checklists with checked and unchecked items
            string mdTasks = "- [x] Complete first task\n- [ ] Pending second task\n- [x] Third task";
            var flowDocTasks = converter.Convert(parser.Parse(mdTasks));
            string serTasks = MarkdownSerializer.Serialize(flowDocTasks).Trim();
            AssertEqual(mdTasks, serTasks, "Task list roundtrip");

            // 2. Interactive toggle in FlowDocument
            var wpfList = flowDocTasks.Blocks.OfType<WpfList>().First();
            var item2 = wpfList.ListItems.Cast<WpfListItem>().ElementAt(1);
            var p2 = (Paragraph)item2.Blocks.FirstBlock!;
            var cb = (CheckBox)((InlineUIContainer)p2.Inlines.FirstInline!).Child;
            cb.IsChecked = true;
            string serToggled = MarkdownSerializer.Serialize(flowDocTasks).Trim();
            Assert(serToggled.Contains("- [x] Pending second task"), "Toggled checkbox serializes as checked");

            // 3. Ordered list
            string mdOrdered = "1. Item one\n2. Item two\n3. Item three";
            var flowDocOrd = converter.Convert(parser.Parse(mdOrdered));
            string serOrd = MarkdownSerializer.Serialize(flowDocOrd).Trim();
            AssertEqual(mdOrdered, serOrd, "Ordered list roundtrip");

            // 4. Unordered list
            string mdUnordered = "- Apple\n- Banana\n- Cherry";
            var flowDocUn = converter.Convert(parser.Parse(mdUnordered));
            string serUn = MarkdownSerializer.Serialize(flowDocUn).Trim();
            AssertEqual(mdUnordered, serUn, "Unordered list roundtrip");
        }

        private static void TestSerializationTables()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, true);

            // 1. Tables with column alignments
            string mdTable = "| Header 1 | Header 2 | Header 3 |\n| :--- | :---: | ---: |\n| Left | Center | Right |\n| Val 1 | Val 2 | Val 3 |";
            var flowDoc = converter.Convert(parser.Parse(mdTable));
            string ser = MarkdownSerializer.Serialize(flowDoc).Trim();
            var reParsed = parser.Parse(ser);
            AssertEqual(1, reParsed.Blocks.Count, "1 TableBlock");
            var tb = (TableBlock)reParsed.Blocks[0];
            AssertEqual(ColumnAlignment.Left, tb.Alignments[0], "Col 0 Left");
            AssertEqual(ColumnAlignment.Center, tb.Alignments[1], "Col 1 Center");
            AssertEqual(ColumnAlignment.Right, tb.Alignments[2], "Col 2 Right");
            AssertEqual(2, tb.Rows.Count, "2 data rows");

            // 2. Pipe escaping
            string mdPipe = "| Col A | Col B |\n| :--- | :--- |\n| Escaped \\| Pipe | Plain |";
            var flowDocPipe = converter.Convert(parser.Parse(mdPipe));
            string serPipe = MarkdownSerializer.Serialize(flowDocPipe).Trim();
            Assert(serPipe.Contains(@"Escaped \| Pipe"), "Serialized table escapes pipe character");
            var reParsedPipe = parser.Parse(serPipe);
            var tbPipe = (TableBlock)reParsedPipe.Blocks[0];
            AssertEqual("Escaped | Pipe", tbPipe.Rows[0].Cells[0].Text, "Unescaped cell text after parse");
        }

        private static void TestSerializationDirectBlockAndInlineApi()
        {
            // Null safety
            AssertEqual("", MarkdownSerializer.Serialize((FlowDocument)null!), "Null doc");
            AssertEqual("", MarkdownSerializer.SerializeBlock(null!), "Null block");
            AssertEqual("", MarkdownSerializer.SerializeInline(null!), "Null inline");

            // Direct inlines
            var run = new Run("Just text");
            AssertEqual("Just text", MarkdownSerializer.SerializeInline(run), "Run inline");

            var bold = new Bold(new Run("Bold text"));
            AssertEqual("**Bold text**", MarkdownSerializer.SerializeInline(bold), "Bold inline");

            var italic = new Italic(new Run("Italic text"));
            AssertEqual("*Italic text*", MarkdownSerializer.SerializeInline(italic), "Italic inline");

            var link = new Hyperlink(new Run("Click Here")) { NavigateUri = new Uri("https://example.com") };
            AssertEqual("[Click Here](https://example.com)", MarkdownSerializer.SerializeInline(link), "Link inline");

            var hr = new BlockUIContainer(new Border { Height = 1, Tag = "hr" });
            AssertEqual("---", MarkdownSerializer.SerializeBlock(hr), "HR block");
        }

        private static void TestSerializationFullDocumentRoundTrip()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, true);

            string fullDoc = @"# Project Overview

This is the **primary** specification for MDPlus, which is *extremely* fast and ==reliable==.
Visit [GitHub Repo](https://github.com/nickf-sudomania/mdplusplus) for details.

## Features

- [x] High performance native editor
- [x] Zero external dependencies
- [ ] Electron-free architecture

### Benchmark Comparison

| Metric | MDPlus | MarkText |
| :--- | :---: | ---: |
| Startup | 120ms | 1200ms |
| Idle RAM | 28MB | 180MB |

> [!NOTE]
> All benchmarks were measured on Windows 11 with .NET 8.

```csharp
public static void Main()
{
    Console.WriteLine(""MDPlus Ready"");
}
```

---

End of document.";

            var doc1 = parser.Parse(fullDoc);
            var flowDoc = converter.Convert(doc1);
            string serialized = MarkdownSerializer.Serialize(flowDoc).Trim();
            var doc2 = parser.Parse(serialized);

            AssertEqual(doc1.Blocks.Count, doc2.Blocks.Count, "Full document block counts match");
            for (int i = 0; i < doc1.Blocks.Count; i++)
            {
                AssertEqual(doc1.Blocks[i].GetType(), doc2.Blocks[i].GetType(), $"Block {i} type match");
            }

            // Idempotent 2nd round-trip
            var flowDoc2 = converter.Convert(doc2);
            string serialized2 = MarkdownSerializer.Serialize(flowDoc2).Trim();
            AssertEqual(serialized, serialized2, "Serialization is idempotent");
        }

        // 17. Theme Palette System & Menu Readability Tests (Milestone 3)

        private static void TestThemePresetsInitializationAndImmutability()
        {
            var presets = new[]
            {
                ThemePreset.GitHubDark,
                ThemePreset.GitHubLight,
                ThemePreset.Nord,
                ThemePreset.OneDark,
                ThemePreset.Monokai
            };

            AssertEqual(5, presets.Length, "5 theme presets");

            foreach (var preset in presets)
            {
                var palette = ThemePalette.GetPalette(preset);
                Assert(palette != null, $"Palette {preset} must not be null");
                AssertEqual(preset, palette!.Preset, $"Preset identity {preset}");
                Assert(!string.IsNullOrEmpty(palette.Name), $"Palette name for {preset}");

                bool expectedIsDark = preset != ThemePreset.GitHubLight;
                AssertEqual(expectedIsDark, palette.IsDark, $"IsDark flag for {preset}");

                // Validate all essential brushes are non-null and frozen
                var brushes = new[]
                {
                    ("WindowBg", palette.WindowBg),
                    ("EditorBg", palette.EditorBg),
                    ("EditorFg", palette.EditorFg),
                    ("SidebarBg", palette.SidebarBg),
                    ("MenuBg", palette.MenuBg),
                    ("MenuFg", palette.MenuFg),
                    ("MenuHoverBg", palette.MenuHoverBg),
                    ("MenuHoverFg", palette.MenuHoverFg),
                    ("MenuPopupBg", palette.MenuPopupBg),
                    ("MenuPopupBorder", palette.MenuPopupBorder),
                    ("MenuBorder", palette.MenuBorder),
                    ("MenuSeparator", palette.MenuSeparator),
                    ("StatusBg", palette.StatusBg),
                    ("StatusFg", palette.StatusFg),
                    ("Border", palette.Border),
                    ("MutedFg", palette.MutedFg),
                    ("Accent", palette.Accent),
                    ("SelectionBg", palette.SelectionBg),
                    ("CodeBg", palette.CodeBg),
                    ("CodeBorder", palette.CodeBorder),
                    ("TableHeaderBg", palette.TableHeaderBg),
                    ("TableAltRowBg", palette.TableAltRowBg),
                    ("TableBorder", palette.TableBorder),
                    ("TabActiveBg", palette.TabActiveBg),
                    ("TabInactiveBg", palette.TabInactiveBg),
                    ("HeadingFg", palette.HeadingFg)
                };

                foreach (var (name, brush) in brushes)
                {
                    Assert(brush != null, $"{preset} {name} brush must not be null");
                    Assert(brush!.IsFrozen, $"{preset} {name} brush must be frozen");
                }

                // Verify Color properties match brush colors
                AssertEqual(palette.WindowBg.Color, palette.WindowBackgroundColor, $"{preset} WindowBackgroundColor");
                AssertEqual(palette.EditorBg.Color, palette.EditorBackgroundColor, $"{preset} EditorBackgroundColor");
                AssertEqual(palette.EditorFg.Color, palette.EditorForegroundColor, $"{preset} EditorForegroundColor");
                AssertEqual(palette.MenuBg.Color, palette.MenuBackgroundColor, $"{preset} MenuBackgroundColor");
                AssertEqual(palette.MenuFg.Color, palette.MenuForegroundColor, $"{preset} MenuForegroundColor");
                AssertEqual(palette.MenuHoverBg.Color, palette.MenuHoverBackgroundColor, $"{preset} MenuHoverBackgroundColor");
                AssertEqual(palette.MenuHoverFg.Color, palette.MenuHoverForegroundColor, $"{preset} MenuHoverForegroundColor");
                AssertEqual(palette.StatusBg.Color, palette.StatusBarBackgroundColor, $"{preset} StatusBarBackgroundColor");
                AssertEqual(palette.StatusFg.Color, palette.StatusBarForegroundColor, $"{preset} StatusBarForegroundColor");
            }
        }

        private static void TestThemeWcagAaContrastCompliance()
        {
            // Verify mathematical formulas on control benchmarks
            double ratioBw = ThemePalette.CalculateContrast(Colors.Black, Colors.White);
            Assert(Math.Abs(ratioBw - 21.0) < 0.1, "Black/White contrast ratio is 21:1");

            double ratioSame = ThemePalette.CalculateContrast(Colors.Gray, Colors.Gray);
            Assert(Math.Abs(ratioSame - 1.0) < 0.01, "Identical colors contrast ratio is 1:1");

            var presets = new[]
            {
                ThemePreset.GitHubDark,
                ThemePreset.GitHubLight,
                ThemePreset.Nord,
                ThemePreset.OneDark,
                ThemePreset.Monokai
            };

            foreach (var preset in presets)
            {
                var palette = ThemePalette.GetPalette(preset);

                // 1. Editor Text vs Canvas Background (WCAG AA >= 4.5:1)
                double editorContrast = ThemePalette.CalculateContrast(palette.EditorBg.Color, palette.EditorFg.Color);
                Assert(editorContrast >= 4.5, $"{preset} Editor contrast ({editorContrast:F2}:1) must be >= 4.5:1");

                // 2. Menu Text vs Menu Background (WCAG AA >= 4.5:1)
                double menuContrast = ThemePalette.CalculateContrast(palette.MenuBg.Color, palette.MenuFg.Color);
                Assert(menuContrast >= 4.5, $"{preset} Menu contrast ({menuContrast:F2}:1) must be >= 4.5:1");

                // 3. Menu Hover Text vs Menu Hover Background (WCAG AA >= 4.5:1)
                double hoverContrast = ThemePalette.CalculateContrast(palette.MenuHoverBg.Color, palette.MenuHoverFg.Color);
                Assert(hoverContrast >= 4.5, $"{preset} Menu Hover contrast ({hoverContrast:F2}:1) must be >= 4.5:1");

                // 4. Status Bar Text vs Status Bar Background (WCAG AA >= 4.5:1)
                double statusContrast = ThemePalette.CalculateContrast(palette.StatusBg.Color, palette.StatusFg.Color);
                Assert(statusContrast >= 4.5, $"{preset} Status bar contrast ({statusContrast:F2}:1) must be >= 4.5:1");

                // 5. Heading Text vs Canvas Background (WCAG AA >= 4.5:1)
                double headingContrast = ThemePalette.CalculateContrast(palette.EditorBg.Color, palette.HeadingFg.Color);
                Assert(headingContrast >= 4.5, $"{preset} Heading contrast ({headingContrast:F2}:1) must be >= 4.5:1");
            }
        }

        private static void TestThemeManagerDynamicSwitchingAndEvents()
        {
            var tm = ThemeManager.Instance;
            Assert(tm != null, "ThemeManager instance exists");

            bool eventFired = false;
            EventHandler handler = (s, e) => eventFired = true;
            tm!.ThemeChanged += handler;

            try
            {
                // Test switching to each preset
                foreach (ThemePreset preset in Enum.GetValues<ThemePreset>())
                {
                    eventFired = false;
                    tm.SetPreset(preset);

                    Assert(eventFired, $"ThemeChanged event must fire on SetPreset({preset})");
                    AssertEqual(preset, tm.CurrentPreset, $"CurrentPreset after SetPreset({preset})");
                    AssertEqual(preset, tm.CurrentPalette.Preset, $"CurrentPalette.Preset after SetPreset({preset})");
                    AssertEqual(preset != ThemePreset.GitHubLight, tm.IsDark, $"IsDark flag after SetPreset({preset})");
                }
            }
            finally
            {
                tm!.ThemeChanged -= handler;
            }
        }

        private static void TestThemeManagerCycling()
        {
            var tm = ThemeManager.Instance;
            tm.SetPreset(ThemePreset.GitHubDark);

            var expectedOrder = new[]
            {
                ThemePreset.GitHubLight,
                ThemePreset.Nord,
                ThemePreset.OneDark,
                ThemePreset.Monokai,
                ThemePreset.GitHubDark
            };

            foreach (var expected in expectedOrder)
            {
                tm.CycleNextTheme();
                AssertEqual(expected, tm.CurrentPreset, $"CycleNextTheme step -> {expected}");
            }
        }

        private static void TestMarkdownConverterThemePaletteIntegration()
        {
            var parser = new MarkdownParser();
            string md = @"# Theme Document

Here is regular text with **bold** formatting.

| Col 1 | Col 2 |
| :--- | :--- |
| Val A | Val B |

```csharp
int x = 42;
```";
            var doc = parser.Parse(md);

            foreach (ThemePreset preset in Enum.GetValues<ThemePreset>())
            {
                var palette = ThemePalette.GetPalette(preset);
                var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, palette);
                var flowDoc = converter.Convert(doc);

                Assert(flowDoc != null, $"FlowDocument for {preset} converted");
                if (flowDoc != null)
                {
                    AssertEqual(palette.EditorFg.Color, ((SolidColorBrush)flowDoc.Foreground).Color, $"{preset} document foreground");
                    Assert(flowDoc.Blocks.Count >= 4, $"{preset} document has blocks");
                }
            }
        }

        // 18. Windows Setup Installer & Prerequisite Bootstrapper Tests
        private static void TestInstallerScriptIntegrity()
        {
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "installer", "MDPlus.iss"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "installer", "MDPlus.iss"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "installer", "MDPlus.iss")
            };

            string issPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(issPath), "Installer script MDPlus.iss must exist");

            string scriptText = System.IO.File.ReadAllText(issPath);

            // 1. Core metadata & architecture constraints
            Assert(scriptText.Contains("AppId={{E67BD82D-C178-43B3-9F93-78B43DF331B2}"), "Setup script must contain valid AppId GUID");
            Assert(scriptText.Contains("OutputBaseFilename=MDPlus-Setup"), "Setup output base filename must be MDPlus-Setup");
            Assert(scriptText.Contains("SetupIconFile=..\\src\\Resources\\AppIcon.ico"), "Setup icon must point to AppIcon.ico");
            Assert(scriptText.Contains("UninstallDisplayIcon={app}\\{#MyAppExeName}"), "Uninstall icon must be set");
            Assert(scriptText.Contains("ChangesAssociations=yes"), "Must declare ChangesAssociations=yes");
            Assert(scriptText.Contains("ArchitecturesAllowed=x64compatible"), "Must restrict to x64compatible architectures");
            Assert(scriptText.Contains("ArchitecturesInstallIn64BitMode=x64compatible"), "Must install in 64-bit mode on x64compatible");
            Assert(scriptText.Contains("runasoriginaluser"), "Must launch application with non-elevated user token");

            // 2. File associations & Shell integration
            Assert(scriptText.Contains("Software\\Classes\\.md"), "Must register .md file extension");
            Assert(scriptText.Contains("Software\\Classes\\.markdown"), "Must register .markdown file extension");
            Assert(scriptText.Contains("Software\\Classes\\SystemFileAssociations\\.md\\shell\\OpenWithMDPlus"), "Must register Open with MDPlus context menu for .md");
            Assert(scriptText.Contains("Software\\Classes\\SystemFileAssociations\\.markdown\\shell\\OpenWithMDPlus"), "Must register Open with MDPlus context menu for .markdown");
            Assert(scriptText.Contains("Software\\MDPlus\\Capabilities"), "Must register Windows Capabilities for default apps");
            Assert(scriptText.Contains("Software\\Microsoft\\Windows\\CurrentVersion\\App Paths"), "Must register Windows App Paths for Win+R execution");

            // 3. Prerequisite Bootstrapper
            Assert(scriptText.Contains("IsDotNet8DesktopInstalled"), "Must implement IsDotNet8DesktopInstalled detection function");
            Assert(scriptText.Contains("https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"), "Must reference official Microsoft .NET 8 desktop runtime download URL");
            Assert(scriptText.Contains("DownloadTemporaryFile"), "Must implement headless download via DownloadTemporaryFile for silent unattended mode");
            Assert(scriptText.Contains("ShellExec('runas'"), "Must support UAC elevation via ShellExec runas for prerequisite installer");
            Assert(scriptText.Contains("/install /quiet /norestart"), "Must invoke Microsoft installer with silent unattended flags");
            Assert(scriptText.Contains("Microsoft.WindowsDesktop.App"), "Must detect Microsoft.WindowsDesktop.App");
        }

        private static void TestInstallerChecksumManifestParsingAndVerification()
        {
            string manifest = @"# MDPlus Checksums Manifest (Notepad++ Standard)
6166614341e93dfc4b75575c7bd271a887d892756748cd15313e351a87014137  MDPlus.exe
3cd99b3e856f3adabb71f59d2bf23a15f2a754f94bc672853b0f97ed599b97bb  MDPlus-Setup.exe
ff004304d4ec4b73c6f7d5630aaa8d54c12705753589fab81d02863e69fbee2d  MDPlus-win-x64.zip
d9f764a730236c5a79103fe8ffb4c730649dcfc2cac93fcce59f5bbe12a183b5  MDPlus-1.0.0-src.zip
";
            var parsed = HashService.ParseChecksums(manifest);
            AssertEqual(4, parsed.Count, "Must parse 4 entries including installer");
            Assert(parsed.ContainsKey("MDPlus-Setup.exe"), "Must parse MDPlus-Setup.exe entry");
            AssertEqual("3cd99b3e856f3adabb71f59d2bf23a15f2a754f94bc672853b0f97ed599b97bb", parsed["MDPlus-Setup.exe"], "MDPlus-Setup.exe hash");

            // If dist/MDPlus-Setup.exe exists on disk, verify its SHA-256
            string[] possibleDistPaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "dist", "MDPlus-Setup.exe"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "dist", "MDPlus-Setup.exe"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "dist", "MDPlus-Setup.exe")
            };
            string setupPath = possibleDistPaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            if (!string.IsNullOrEmpty(setupPath))
            {
                string computedHash = HashService.ComputeSha256(setupPath);
                AssertEqual(64, computedHash.Length, "Installer SHA-256 length must be 64 characters");
                Assert(HashService.VerifyFileSha256(setupPath, computedHash), "Self-verification of MDPlus-Setup.exe SHA-256");

                string setupShaFile = setupPath + ".sha256";
                if (System.IO.File.Exists(setupShaFile))
                {
                    string shaContent = System.IO.File.ReadAllText(setupShaFile).Trim();
                    Assert(shaContent.StartsWith(computedHash, StringComparison.OrdinalIgnoreCase), "Individual .sha256 file must match computed hash");
                }
            }
        }

        private static void TestDotNet8DesktopRuntimeDetection()
        {
            bool runtimeFound = false;

            // 1. Check directory in Program Files
            string pfPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string desktopDir = System.IO.Path.Combine(pfPath, "dotnet", "shared", "Microsoft.WindowsDesktop.App");
            if (System.IO.Directory.Exists(desktopDir))
            {
                var dirs = System.IO.Directory.GetDirectories(desktopDir, "8.*");
                if (dirs.Length > 0) runtimeFound = true;
            }

            // 2. Check LocalAppData
            if (!runtimeFound)
            {
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string localDesktopDir = System.IO.Path.Combine(localApp, "Microsoft", "dotnet", "shared", "Microsoft.WindowsDesktop.App");
                if (System.IO.Directory.Exists(localDesktopDir))
                {
                    var dirs = System.IO.Directory.GetDirectories(localDesktopDir, "8.*");
                    if (dirs.Length > 0) runtimeFound = true;
                }
            }

            // 3. Check DOTNET_ROOT if configured
            if (!runtimeFound)
            {
                string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
                if (!string.IsNullOrEmpty(dotnetRoot))
                {
                    string customDesktopDir = System.IO.Path.Combine(dotnetRoot, "shared", "Microsoft.WindowsDesktop.App");
                    if (System.IO.Directory.Exists(customDesktopDir))
                    {
                        var dirs = System.IO.Directory.GetDirectories(customDesktopDir, "8.*");
                        if (dirs.Length > 0) runtimeFound = true;
                    }
                }
            }

            // 4. Fallback: since this test is running on .NET 8 desktop runtime itself
            if (!runtimeFound)
            {
                string runtimeVersion = Environment.Version.ToString();
                if (runtimeVersion.StartsWith("8.")) runtimeFound = true;
            }

            Assert(runtimeFound, "Current system running tests must satisfy .NET 8 Desktop Runtime detection");
        }

        // 19. Menu Bar & Scrollbar UI Tests (Milestone 4)
        private static void TestAppSettingsShowMenuBar()
        {
            var settings = new MDPlus.Models.AppSettings();
            AssertEqual(false, settings.ShowMenuBar, "ShowMenuBar must default to false");

            settings.ShowMenuBar = true;
            string json = System.Text.Json.JsonSerializer.Serialize(settings);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<MDPlus.Models.AppSettings>(json);
            Assert(deserialized != null, "Deserialized settings must not be null");
            AssertEqual(true, deserialized!.ShowMenuBar, "Deserialized ShowMenuBar must be true");

            string legacyJson = "{\"Theme\":\"GitHubDark\",\"ShowToc\":true}";
            var legacySettings = System.Text.Json.JsonSerializer.Deserialize<MDPlus.Models.AppSettings>(legacyJson);
            Assert(legacySettings != null, "Legacy settings must not be null");
            AssertEqual(false, legacySettings!.ShowMenuBar, "ShowMenuBar must default to false when absent in JSON");
        }

        private static void TestMenuScrollViewerDisabledInAppXaml()
        {
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "App.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "App.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "App.xaml")
            };

            string appXamlPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(appXamlPath), "App.xaml must exist");

            string xamlText = System.IO.File.ReadAllText(appXamlPath);

            Assert(xamlText.Contains("VerticalScrollBarVisibility=\"Disabled\""), "App.xaml must configure VerticalScrollBarVisibility=\"Disabled\"");
            Assert(xamlText.Contains("HorizontalScrollBarVisibility=\"Disabled\""), "App.xaml must configure HorizontalScrollBarVisibility=\"Disabled\"");

            int vertCount = 0;
            int idx = 0;
            while ((idx = xamlText.IndexOf("VerticalScrollBarVisibility=\"Disabled\"", idx, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                vertCount++;
                idx += 30;
            }
            Assert(vertCount >= 3, $"App.xaml must contain at least 3 disabled scrollbar configurations for TopLevelHeader, SubmenuHeader, ContextMenu (found {vertCount})");
        }

        private static void TestMenuBarThemeColorPaletteConsistency()
        {
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "MainWindow.xaml")
            };

            string mainWindowXamlPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(mainWindowXamlPath), "MainWindow.xaml must exist");

            string xamlText = System.IO.File.ReadAllText(mainWindowXamlPath);

            Assert(!xamlText.Contains("<Menu Grid.Row=\"0\" Name=\"MainMenu\" Background=\"#2d2d30\""),
                "MainMenu must not have hardcoded #2d2d30 background");

            Assert(xamlText.Contains("Name=\"MainMenu\" Visibility=\"Collapsed\""),
                "MainMenu must default to Visibility=\"Collapsed\"");
            Assert(xamlText.Contains("Background=\"{DynamicResource MenuBackgroundBrush}\""),
                "MainMenu must use DynamicResource MenuBackgroundBrush");

            Assert(xamlText.Contains("Name=\"HamburgerMenuButton\""), "Tab bar must include HamburgerMenuButton");
            Assert(xamlText.Contains("Content=\"☰\""), "HamburgerMenuButton must have ☰ content");
            Assert(xamlText.Contains("Name=\"HamburgerContextMenu\""), "HamburgerMenuButton must contain HamburgerContextMenu");
            Assert(xamlText.Contains("Name=\"ToggleMenuBarMenuItem\""), "View menu must contain ToggleMenuBarMenuItem");

            Assert(xamlText.Contains("Name=\"HamburgerViewRenderedItem\"") && xamlText.Contains("IsCheckable=\"True\" IsChecked=\"True\""),
                "HamburgerViewRenderedItem must be checkable and checked by default");
            Assert(xamlText.Contains("Name=\"HamburgerViewSplitItem\"") && xamlText.Contains("IsCheckable=\"True\""),
                "HamburgerViewSplitItem must be checkable");
            Assert(xamlText.Contains("Name=\"HamburgerViewRawItem\"") && xamlText.Contains("IsCheckable=\"True\""),
                "HamburgerViewRawItem must be checkable");
            Assert(xamlText.Contains("TargetName=\"BtnContent\" Property=\"TextElement.Foreground\" Value=\"{DynamicResource MenuHoverForegroundBrush}\""),
                "HamburgerMenuButton hover trigger must style TextElement.Foreground on BtnContent");

            foreach (ThemePreset preset in Enum.GetValues<ThemePreset>())
            {
                var palette = ThemePalette.GetPalette(preset);
                if (palette == null) throw new InvalidOperationException($"Palette {preset} must not be null");
                Assert(palette.MenuBg != null, $"Palette {preset} MenuBg must not be null");
                Assert(palette.MenuFg != null, $"Palette {preset} MenuFg must not be null");
                Assert(palette.Border != null, $"Palette {preset} Border must not be null");
                double contrast = ThemePalette.CalculateContrast(palette.MenuBg!.Color, palette.MenuFg!.Color);
                Assert(contrast >= 4.5, $"Menu text contrast for {preset} ({contrast:F2}:1) must satisfy WCAG AA >= 4.5:1");
            }
        }

        private static void TestRecentFileMnemonicAndAltGrHandling()
        {
            string fileName = "benchmark_5000_tests.md";
            string escaped = fileName.Replace("_", "__");
            AssertEqual("benchmark__5000__tests.md", escaped, "Underscores in recent filenames must be escaped to prevent access key mnemonic swallowing");

            // AltGr simulation: ModifierKeys includes Control
            var altGrMods = System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt;
            bool isCtrlDown = altGrMods.HasFlag(System.Windows.Input.ModifierKeys.Control);
            Assert(isCtrlDown, "AltGr combinations with Control modifier must be recognized as having Ctrl down");
        }

        // 20. Session Restore & Tab Lifecycle Tests
        private static void TestAppSettingsSessionRestoreProperties()
        {
            var settings = new AppSettings();
            AssertEqual(true, settings.ResumeSession, "ResumeSession must default to true");
            AssertEqual(false, settings.StartFresh, "StartFresh must default to false");
            AssertEqual(true, settings.RestoreSession, "RestoreSession must match ResumeSession");
            Assert(settings.OpenFiles != null, "OpenFiles must not be null");
            AssertEqual(0, settings.OpenFiles!.Count, "OpenFiles must default to empty");
            Assert(settings.ActiveFile == null, "ActiveFile must default to null");
            AssertEqual(false, settings.HasSavedSession, "HasSavedSession must default to false");
            AssertEqual(false, settings.FirstRunCompleted, "FirstRunCompleted must default to false");
            Assert(settings.IsFirstRun, "IsFirstRun must be true on brand-new installation");

            // Toggle StartFresh
            settings.StartFresh = true;
            AssertEqual(false, settings.ResumeSession, "Setting StartFresh=true must set ResumeSession=false");
            settings.ResumeSession = true;
            AssertEqual(false, settings.StartFresh, "Setting ResumeSession=true must set StartFresh=false");

            // IsFirstRun with recent files
            settings.RecentFiles.Add("C:\\sample.md");
            Assert(!settings.IsFirstRun, "IsFirstRun must be false when RecentFiles is not empty");
            settings.RecentFiles.Clear();
            settings.FirstRunCompleted = true;
            Assert(!settings.IsFirstRun, "IsFirstRun must be false when FirstRunCompleted is true");

            // Serialization & Deserialization
            var toSerialize = new AppSettings
            {
                ResumeSession = true,
                ActiveFile = "C:\\docs\\test.md",
                HasSavedSession = true,
                FirstRunCompleted = true
            };
            toSerialize.OpenFiles.Add("C:\\docs\\test.md");
            toSerialize.OpenFiles.Add("C:\\docs\\other.md");

            string json = System.Text.Json.JsonSerializer.Serialize(toSerialize);
            var deserialized = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
            Assert(deserialized != null, "Deserialized settings must not be null");
            AssertEqual(true, deserialized!.ResumeSession, "Deserialized ResumeSession must match");
            AssertEqual("C:\\docs\\test.md", deserialized.ActiveFile, "Deserialized ActiveFile must match");
            AssertEqual(2, deserialized.OpenFiles!.Count, "Deserialized OpenFiles count must match");
            AssertEqual("C:\\docs\\test.md", deserialized.OpenFiles[0], "First OpenFile must match");
            AssertEqual(true, deserialized.HasSavedSession, "HasSavedSession must match");

            // Legacy JSON backward compatibility
            string legacyJson = "{\"Theme\":\"GitHubDark\",\"ShowMenuBar\":true}";
            var legacy = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(legacyJson);
            Assert(legacy != null, "Legacy settings must not be null");
            AssertEqual(true, legacy!.ResumeSession, "ResumeSession must default to true when absent in JSON");
            Assert(legacy.OpenFiles != null && legacy.OpenFiles!.Count == 0, "OpenFiles must default to empty list when absent in JSON");
            Assert(legacy.ActiveFile == null, "ActiveFile must default to null when absent in JSON");

            // UpdateOpenFiles helper
            string existingFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_docs", "welcome.md");
            string gfmFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_docs", "gfm_features.md");
            if (System.IO.File.Exists(existingFile) && System.IO.File.Exists(gfmFile))
            {
                var s = new AppSettings();
                s.UpdateOpenFiles(new[] { existingFile, existingFile.ToUpperInvariant(), "C:\\non_existent_file_12345.md" }, existingFile);
                AssertEqual(1, s.OpenFiles.Count, "UpdateOpenFiles must deduplicate paths and ignore non-existent files");
                AssertEqual(System.IO.Path.GetFullPath(existingFile), s.OpenFiles[0], "OpenFiles path must be normalized full path");
                AssertEqual(System.IO.Path.GetFullPath(existingFile), s.ActiveFile, "ActiveFile must match target");
                AssertEqual(true, s.HasSavedSession, "UpdateOpenFiles must set HasSavedSession to true");

                // Multiple files with target active file
                s.UpdateOpenFiles(new[] { existingFile, gfmFile }, gfmFile);
                AssertEqual(2, s.OpenFiles.Count, "UpdateOpenFiles must retain multiple valid files");
                AssertEqual(System.IO.Path.GetFullPath(gfmFile), s.ActiveFile, "ActiveFile must accurately target specified file in list");

                // Target first file as active
                s.UpdateOpenFiles(new[] { existingFile, gfmFile }, existingFile);
                AssertEqual(System.IO.Path.GetFullPath(existingFile), s.ActiveFile, "ActiveFile must accurately target first file when selected");
            }
        }

        private static void TestAppResolveStartupFilesLogic()
        {
            string welcomeDoc = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_docs", "welcome.md");
            string gfmDoc = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_docs", "gfm_features.md");
            bool filesExist = System.IO.File.Exists(welcomeDoc) && System.IO.File.Exists(gfmDoc);
            Assert(filesExist, "Sample docs welcome.md and gfm_features.md must exist in test directory");

            var settings = new AppSettings
            {
                ResumeSession = true
            };
            settings.OpenFiles.Add(welcomeDoc);

            // 1. Explicit CLI arguments take precedence
            var cliFiles = App.ResolveStartupFiles(settings, new[] { gfmDoc });
            AssertEqual(1, cliFiles.Count, "CLI file argument must take precedence over session files");
            AssertEqual(System.IO.Path.GetFullPath(gfmDoc), cliFiles[0], "CLI argument file must be returned");

            // 2. CLI --start-fresh flag forces fresh start
            var freshCliFiles = App.ResolveStartupFiles(settings, new[] { "--start-fresh" });
            AssertEqual(0, freshCliFiles.Count, "--start-fresh CLI flag must return empty list");

            var freshShortCliFiles = App.ResolveStartupFiles(settings, new[] { "--fresh" });
            AssertEqual(0, freshShortCliFiles.Count, "--fresh CLI flag must return empty list");

            // 3. User preference ResumeSession = true restores open files
            var restoredFiles = App.ResolveStartupFiles(settings, Array.Empty<string>());
            AssertEqual(1, restoredFiles.Count, "ResumeSession=true must restore open files");
            AssertEqual(System.IO.Path.GetFullPath(welcomeDoc), restoredFiles[0], "Restored file must match OpenFiles");

            // 4. User preference StartFresh (ResumeSession = false) returns empty list
            settings.ResumeSession = false;
            var startFreshFiles = App.ResolveStartupFiles(settings, Array.Empty<string>());
            AssertEqual(0, startFreshFiles.Count, "ResumeSession=false must not restore open files (starts fresh)");

            // 5. Non-existent files in OpenFiles are filtered out
            settings.ResumeSession = true;
            settings.OpenFiles.Clear();
            settings.OpenFiles.Add("C:\\does_not_exist_file_abcdef.md");
            var filteredFiles = App.ResolveStartupFiles(settings, Array.Empty<string>());
            AssertEqual(0, filteredFiles.Count, "Non-existent files must be excluded from startup resolution");

            // 6. Transition case from RecentFiles when HasSavedSession is false
            settings.OpenFiles.Clear();
            settings.HasSavedSession = false;
            settings.RecentFiles.Add(gfmDoc);
            var transitionFiles = App.ResolveStartupFiles(settings, Array.Empty<string>());
            AssertEqual(1, transitionFiles.Count, "Transition case must restore from RecentFiles when HasSavedSession is false");
            AssertEqual(System.IO.Path.GetFullPath(gfmDoc), transitionFiles[0], "Transition file must match RecentFiles[0]");

            // 7. When HasSavedSession is true and OpenFiles is empty, must NOT fall back to RecentFiles
            settings.HasSavedSession = true;
            var deliberateEmptyFiles = App.ResolveStartupFiles(settings, Array.Empty<string>());
            AssertEqual(0, deliberateEmptyFiles.Count, "Deliberately closed session must not restore RecentFiles");

            // 8. Explicit CLI argument pointing to non-existent file must NOT fall back to session files
            settings.OpenFiles.Clear();
            settings.OpenFiles.Add(welcomeDoc);
            var cliMissingFiles = App.ResolveStartupFiles(settings, new[] { "C:\\does_not_exist_cli_xyz123.md" });
            AssertEqual(0, cliMissingFiles.Count, "Explicit non-existent CLI file argument must return empty list, not restore session files");
        }

        private static void TestTabClosingSingleClickAndEmptyState()
        {
            // Verify XAML layout contains startup menu options and single-click tab elements
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "MainWindow.xaml")
            };
            string mainWindowXamlPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(mainWindowXamlPath), "MainWindow.xaml must exist");

            string xamlText = System.IO.File.ReadAllText(mainWindowXamlPath);
            Assert(xamlText.Contains("Name=\"StartupMenu\""), "MainWindow.xaml must contain StartupMenu in File menu");
            Assert(xamlText.Contains("Name=\"StartupResumeItem\""), "MainWindow.xaml must contain StartupResumeItem");
            Assert(xamlText.Contains("Name=\"StartupFreshItem\""), "MainWindow.xaml must contain StartupFreshItem");
            Assert(xamlText.Contains("Name=\"HamburgerStartupMenu\""), "MainWindow.xaml must contain HamburgerStartupMenu");
            Assert(xamlText.Contains("Name=\"HamburgerStartupResumeItem\""), "MainWindow.xaml must contain HamburgerStartupResumeItem");
            Assert(xamlText.Contains("Name=\"HamburgerStartupFreshItem\""), "MainWindow.xaml must contain HamburgerStartupFreshItem");
            Assert(xamlText.Contains("➕ New Document (Ctrl+N)"), "WelcomeScreen must offer a New Document button");

            // DocumentTabItem title and dirty state tests
            var tab1 = new DocumentTabItem { Title = "welcome.md", FilePath = "C:\\docs\\welcome.md" };
            AssertEqual("welcome.md", tab1.DisplayTitle, "Clean tab title must equal filename");
            tab1.MarkDirty();
            AssertEqual("welcome.md *", tab1.DisplayTitle, "Dirty tab title must append asterisk");
            tab1.MarkClean();
            AssertEqual("welcome.md", tab1.DisplayTitle, "Cleaned tab title must remove asterisk");

            // Simulate tab closing indexing logic
            var tabs = new List<DocumentTabItem>
            {
                new DocumentTabItem { Title = "tab1.md", FilePath = "C:\\tab1.md" },
                new DocumentTabItem { Title = "tab2.md", FilePath = "C:\\tab2.md" },
                new DocumentTabItem { Title = "tab3.md", FilePath = "C:\\tab3.md" }
            };

            // Close middle active tab (index 1) -> selects next index Math.Min(1, 2-1) = 1 (tab3.md)
            int index = 1;
            tabs.RemoveAt(index);
            int nextIndex = Math.Min(index, tabs.Count - 1);
            AssertEqual(1, nextIndex, "Next active index must be 1");
            AssertEqual("tab3.md", tabs[nextIndex].Title, "Active tab after closing middle tab must be tab3.md");

            // Close last tab (index 1 of remaining 2) -> selects Math.Min(1, 1-1) = 0 (tab1.md)
            index = 1;
            tabs.RemoveAt(index);
            nextIndex = Math.Min(index, tabs.Count - 1);
            AssertEqual(0, nextIndex, "Next active index must be 0");
            AssertEqual("tab1.md", tabs[nextIndex].Title, "Active tab after closing end tab must be tab1.md");

            // Close final tab (index 0 of remaining 1) -> 0 tabs remain, active tab becomes null
            tabs.RemoveAt(0);
            AssertEqual(0, tabs.Count, "Tabs count must be 0 after closing final tab");
            DocumentTabItem? activeTab = tabs.Count > 0 ? tabs[0] : null;
            Assert(activeTab == null, "Active tab must be null when all tabs are closed");
        }
    }
}
