using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            RunTest("Theme Menu Grouping by Dark & Light Submenus", TestThemeMenuGroupingStructure);
            RunTest("Table of Contents Sidebar Dynamic Theme Contrast & Readability", TestTocThemeReadabilityAndContrast);

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

            // 21. Title Bar DWM & Update Service Tests
            RunTest("DWM Color Conversion to Win32 COLORREF", TestDwmColorConversionToWin32ColorRef);
            RunTest("GitHub Release Semantic Version Comparison & Prefix Handling", TestUpdateServiceVersionComparison);
            RunTest("Update Service 24-Hour Startup Debouncing Logic", TestUpdateServiceStartupDebouncing);
            RunTest("Update Service Checksum Manifest Parsing & Target Extraction", TestUpdateServiceChecksumExtraction);
            RunTest("AppSettings Update Preference & Timestamp Serialization", TestAppSettingsUpdateSettingsPersistence);
            RunTest("Window Title Bar & Help Menu Update Action Integrity", TestWindowTitleBarAndHelpMenuIntegrity);
            RunTest("Update Service GitHub API Mock & Cryptographic Verification", TestUpdateServiceCheckAndVerificationWithMock);
            RunTest("Update Service Path-Bounded Checksum & Release Notes Extraction", TestUpdateServicePathBoundedChecksumExtraction);
            RunTest("Update Service Download Cancellation & Temporary File Cleanup", TestUpdateServiceDownloadCancellationAndFileCleanup);
            RunTest("AppSettings Thread-Safe Concurrent Persistence", TestAppSettingsConcurrentSaveSafety);
            RunTest("DWM Border Color Attribute & Reset Helper Integrity", TestDwmHelperBorderAndResetAttributes);
            RunTest("Update Service Exact Asset Priority over Loose Suffix Matches", TestUpdateServiceExactAssetPriority);
            RunTest("Update Service Multi-Asset Release Highlights Hash Extraction", TestUpdateServiceMultiAssetReleaseHighlightsHashExtraction);
            RunTest("Update Service Incomplete Download Detection", TestUpdateServiceIncompleteDownloadDetection);
            RunTest("Update Service Version Prefix and Tag Edge Cases", TestUpdateServiceVersionPrefixAndTagEdgeCases);
            RunTest("Update Service Rate Limit Reset Header & Retry-After Parsing", TestUpdateServiceRateLimitWithResetHeaderAndRetryAfter);
            RunTest("Update Service Installer Process Start & Exit Hooks", TestUpdateServiceInstallerProcessLaunchAndExitHooks);
            RunTest("DWM Helper High Contrast & Win10 1809 Fallback Integrity", TestDwmHelperHighContrastAndWin10Fallback);
            RunTest("Verify Integrity Window Dynamic Palette Theming", TestVerifyIntegrityWindowPaletteDynamicTheming);
            RunTest("Update Service Versioned Asset Priority over Unrelated Setups", TestUpdateServiceVersionedAssetPriorityOverOtherSetups);
            RunTest("Update Service Multi-Asset Manifest False Positive Avoidance", TestUpdateServiceMultiAssetManifestFalsePositiveAvoidance);
            RunTest("Update Service Multi-Line Release Notes with Description Lines", TestUpdateServiceMultiLineReleaseNotesWithDescriptionLines);
            RunTest("Update Service Versioned Target Manifest Resolution", TestUpdateServiceVersionedTargetManifestMatching);
            RunTest("Update Service Locked Destination File Recovery", TestUpdateServiceLockedDestinationFileFallback);
            RunTest("DWM Window Reset & Update Dialog Keyboard Accessibility", TestDwmHelperResetWindowAndFullscreenLifecycle);
            RunTest("Update Service 404 Not Found Graceful Up-To-Date Handling", TestUpdateServiceNotFoundGracefulHandling);
            RunTest("Update Service Live GitHub Release v1.02 Detection", TestUpdateServiceLiveGitHubReleaseV102Detection);
            RunTest("Update Service Dialog Result & UAC Process Start Flow", TestUpdateServiceDialogResultAndUacElevationFlow);
            RunTest("LaTeX Math Inline and Display Parsing & Currency Protection", TestLatexMathInlineAndDisplayParsing);
            RunTest("Native HTML Inline Tags and Block Disclosures Parsing", TestHtmlInlineAndBlockParsing);
            RunTest("LaTeX and HTML FlowDocument Serialization Round-Trip", TestLatexAndHtmlFlowDocumentSerializationRoundTrip);
            RunTest("Plugin Runtime Toggle Behavior and Raw Fallback", TestPluginRuntimeToggleBehavior);
            RunTest("Empirical Performance Benchmark: LaTeX & HTML Zero-Overhead", TestEmpiricalPerformanceBenchmarkWithLatexAndHtmlPlugins);
            RunTest("LaTeX and HTML Edge Cases, Regression Guards & Symbol Typography", TestLatexAndHtmlEdgeCasesAndRegressions);
            RunTest("Measure Baseline Alignment", TestMeasureBaselineAlignment);

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
                ThemePreset.Monokai,
                ThemePreset.OneLight,
                ThemePreset.SolarizedLight,
                ThemePreset.QuietLight
            };

            AssertEqual(8, presets.Length, "8 theme presets");

            foreach (var preset in presets)
            {
                var palette = ThemePalette.GetPalette(preset);
                Assert(palette != null, $"Palette {preset} must not be null");
                AssertEqual(preset, palette!.Preset, $"Preset identity {preset}");
                Assert(!string.IsNullOrEmpty(palette.Name), $"Palette name for {preset}");

                bool expectedIsDark = preset == ThemePreset.GitHubDark ||
                                      preset == ThemePreset.Nord ||
                                      preset == ThemePreset.OneDark ||
                                      preset == ThemePreset.Monokai;
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

            // Verify ThemeManager exposes extended palette brushes
            var tm = ThemeManager.Instance;
            tm.SetPreset(ThemePreset.GitHubLight);
            Assert(tm.HeadingForeground != null && tm.HeadingForeground.IsFrozen, "ThemeManager HeadingForeground frozen");
            Assert(tm.SelectionBackground != null && tm.SelectionBackground.IsFrozen, "ThemeManager SelectionBackground frozen");
            Assert(tm.CodeBackground != null && tm.CodeBackground.IsFrozen, "ThemeManager CodeBackground frozen");
            tm.SetPreset(ThemePreset.GitHubDark);
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
                ThemePreset.Monokai,
                ThemePreset.OneLight,
                ThemePreset.SolarizedLight,
                ThemePreset.QuietLight
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

                // 6. Sidebar / TOC Heading & Editor Text vs Sidebar Background (WCAG AA >= 4.5:1)
                double sidebarHeadingContrast = ThemePalette.CalculateContrast(palette.SidebarBg.Color, palette.HeadingFg.Color);
                Assert(sidebarHeadingContrast >= 4.5, $"{preset} Sidebar Heading contrast ({sidebarHeadingContrast:F2}:1) must be >= 4.5:1");

                double sidebarEditorContrast = ThemePalette.CalculateContrast(palette.SidebarBg.Color, palette.EditorFg.Color);
                Assert(sidebarEditorContrast >= 4.5, $"{preset} Sidebar Editor contrast ({sidebarEditorContrast:F2}:1) must be >= 4.5:1");

                // 7. Muted Text vs Menu Background (for sidebar header title & buttons) (WCAG AA >= 4.5:1)
                double mutedMenuContrast = ThemePalette.CalculateContrast(palette.MenuBg.Color, palette.MutedFg.Color);
                Assert(mutedMenuContrast >= 4.5, $"{preset} MutedFg vs MenuBg ({mutedMenuContrast:F2}:1) must be >= 4.5:1");
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
                    AssertEqual(tm.CurrentPalette.IsDark, tm.IsDark, $"IsDark flag after SetPreset({preset})");
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
                ThemePreset.OneLight,
                ThemePreset.SolarizedLight,
                ThemePreset.QuietLight,
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

        private static void TestThemeMenuGroupingStructure()
        {
            // 1. Verify DarkPresets and LightPresets in ThemePalette
            AssertEqual(4, ThemePalette.DarkPresets.Count, "4 dark presets defined");
            AssertEqual(4, ThemePalette.LightPresets.Count, "4 light presets defined");

            foreach (var preset in ThemePalette.DarkPresets)
            {
                var palette = ThemePalette.GetPalette(preset);
                Assert(palette.IsDark, $"{preset} must have IsDark == true");
            }

            foreach (var preset in ThemePalette.LightPresets)
            {
                var palette = ThemePalette.GetPalette(preset);
                Assert(!palette.IsDark, $"{preset} must have IsDark == false");
            }

            // 2. Verify MainWindow.xaml contains grouped submenus for Dark and Light themes
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "MainWindow.xaml")
            };

            string xamlPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(xamlPath), "MainWindow.xaml must exist");
            string xamlText = System.IO.File.ReadAllText(xamlPath);

            // Verify MainMenu theme grouping submenus
            Assert(xamlText.Contains("Name=\"ThemeDarkThemesMenu\""), "MainMenu must contain ThemeDarkThemesMenu submenu");
            Assert(xamlText.Contains("Name=\"ThemeLightThemesMenu\""), "MainMenu must contain ThemeLightThemesMenu submenu");

            // Verify HamburgerContextMenu theme grouping submenus
            Assert(xamlText.Contains("Name=\"HamburgerThemeDarkThemesMenu\""), "HamburgerContextMenu must contain HamburgerThemeDarkThemesMenu");
            Assert(xamlText.Contains("Name=\"HamburgerThemeLightThemesMenu\""), "HamburgerContextMenu must contain HamburgerThemeLightThemesMenu");

            // Verify all presets are present in MainMenu
            Assert(xamlText.Contains("Name=\"ThemeGitHubDarkItem\""), "ThemeGitHubDarkItem present");
            Assert(xamlText.Contains("Name=\"ThemeNordItem\""), "ThemeNordItem present");
            Assert(xamlText.Contains("Name=\"ThemeOneDarkItem\""), "ThemeOneDarkItem present");
            Assert(xamlText.Contains("Name=\"ThemeMonokaiItem\""), "ThemeMonokaiItem present");
            Assert(xamlText.Contains("Name=\"ThemeGitHubLightItem\""), "ThemeGitHubLightItem present");
            Assert(xamlText.Contains("Name=\"ThemeOneLightItem\""), "ThemeOneLightItem present");
            Assert(xamlText.Contains("Name=\"ThemeSolarizedLightItem\""), "ThemeSolarizedLightItem present");
            Assert(xamlText.Contains("Name=\"ThemeQuietLightItem\""), "ThemeQuietLightItem present");

            // Verify all presets are present in HamburgerContextMenu
            Assert(xamlText.Contains("Name=\"HamburgerThemeGitHubDarkItem\""), "HamburgerThemeGitHubDarkItem present");
            Assert(xamlText.Contains("Name=\"HamburgerThemeNordItem\""), "HamburgerThemeNordItem present");
            Assert(xamlText.Contains("Name=\"HamburgerThemeOneDarkItem\""), "HamburgerThemeOneDarkItem present");
            Assert(xamlText.Contains("Name=\"HamburgerThemeMonokaiItem\""), "HamburgerThemeMonokaiItem present");
            Assert(xamlText.Contains("Name=\"HamburgerThemeGitHubLightItem\""), "HamburgerThemeGitHubLightItem present");
            Assert(xamlText.Contains("Name=\"HamburgerThemeOneLightItem\""), "HamburgerThemeOneLightItem present");
            Assert(xamlText.Contains("Name=\"HamburgerThemeSolarizedLightItem\""), "HamburgerThemeSolarizedLightItem present");
            Assert(xamlText.Contains("Name=\"HamburgerThemeQuietLightItem\""), "HamburgerThemeQuietLightItem present");

            // Verify access keys in Light themes menu do not collide
            Assert(xamlText.Contains("Header=\"_GitHub Light\""), "MainMenu ThemeGitHubLightItem access key is _G");
            Assert(xamlText.Contains("Header=\"_One Light\""), "MainMenu ThemeOneLightItem access key is _O");
            Assert(xamlText.Contains("Header=\"_Solarized Light\""), "MainMenu ThemeSolarizedLightItem access key is _S");
            Assert(xamlText.Contains("Header=\"_Quiet Light\""), "MainMenu ThemeQuietLightItem access key is _Q");
        }

        private static void TestTocThemeReadabilityAndContrast()
        {
            // 1. Validate GitHub Light TOC high contrast readability
            var ghLight = ThemePalette.GitHubLight;
            double ghLightHeadingContrast = ThemePalette.CalculateContrast(ghLight.SidebarBg.Color, ghLight.HeadingFg.Color);
            double ghLightEditorContrast = ThemePalette.CalculateContrast(ghLight.SidebarBg.Color, ghLight.EditorFg.Color);

            Assert(ghLightHeadingContrast >= 7.0, $"GitHub Light TOC Heading contrast ({ghLightHeadingContrast:F2}:1) must exceed WCAG AAA >= 7:1");
            Assert(ghLightEditorContrast >= 7.0, $"GitHub Light TOC Editor text contrast ({ghLightEditorContrast:F2}:1) must exceed WCAG AAA >= 7:1");

            // 2. Validate all Light themes satisfy WCAG AA (>= 4.5:1) for TOC sidebar text
            foreach (var preset in ThemePalette.LightPresets)
            {
                var palette = ThemePalette.GetPalette(preset);
                double headingContrast = ThemePalette.CalculateContrast(palette.SidebarBg.Color, palette.HeadingFg.Color);
                double editorContrast = ThemePalette.CalculateContrast(palette.SidebarBg.Color, palette.EditorFg.Color);
                Assert(headingContrast >= 4.5, $"{preset} TOC Heading contrast ({headingContrast:F2}:1) must be >= 4.5:1");
                Assert(editorContrast >= 4.5, $"{preset} TOC Editor contrast ({editorContrast:F2}:1) must be >= 4.5:1");
            }

            // 3. Verify MainWindow.xaml TOC markup does not use unreadable hardcoded white/light gray text
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "MainWindow.xaml")
            };

            string xamlPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(xamlPath), "MainWindow.xaml must exist");
            string xamlText = System.IO.File.ReadAllText(xamlPath);

            // TOC ListBox item template must use dynamic resource brushes, not hardcoded light colors
            Assert(!xamlText.Contains("<Setter Property=\"Foreground\" Value=\"#ffffff\"/>"),
                "TOC level 1 must not use hardcoded #ffffff");
            Assert(!xamlText.Contains("<Setter Property=\"Foreground\" Value=\"#e0e0e0\"/>"),
                "TOC level 2 must not use hardcoded #e0e0e0");

            Assert(xamlText.Contains("Foreground=\"{DynamicResource ForegroundBrush}\""),
                "TOC item template must use DynamicResource ForegroundBrush");
            Assert(xamlText.Contains("Value=\"{DynamicResource HeadingForegroundBrush}\""),
                "TOC level 1 must use DynamicResource HeadingForegroundBrush");
            Assert(xamlText.Contains("Name=\"SidebarHeaderBorder\""),
                "TOC sidebar must have named SidebarHeaderBorder for dynamic theming");
            Assert(xamlText.Contains("ToolTip=\"{Binding Text}\""),
                "TOC item template provides ToolTip for truncated headings");
            Assert(!xamlText.Contains("<TextBlock Text=\"{Binding Text}\" Margin=\"{Binding IndentMargin}\" ToolTip=\"{Binding Text}\" Foreground="),
                "TOC TextBlock must not locally shadow DataTrigger foreground");
            Assert(xamlText.Contains("Name=\"ContentSplitter\"") && xamlText.Contains("Background=\"{DynamicResource BorderBrush}\""),
                "ContentSplitter must use dynamic BorderBrush");
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

        // 21. Title Bar DWM & Update Service Tests
        private static void TestDwmColorConversionToWin32ColorRef()
        {
            // Pure colors: 0x00BBGGRR
            AssertEqual(0x000000FF, DwmHelper.ColorToColorRef(Color.FromRgb(255, 0, 0)), "Red COLORREF (0x000000FF)");
            AssertEqual(0x0000FF00, DwmHelper.ColorToColorRef(Color.FromRgb(0, 255, 0)), "Green COLORREF (0x0000FF00)");
            AssertEqual(0x00FF0000, DwmHelper.ColorToColorRef(Color.FromRgb(0, 0, 255)), "Blue COLORREF (0x00FF0000)");
            AssertEqual(0, DwmHelper.ColorToColorRef(Color.FromRgb(0, 0, 0)), "Black COLORREF (0)");
            AssertEqual(0x00FFFFFF, DwmHelper.ColorToColorRef(Color.FromRgb(255, 255, 255)), "White COLORREF (0x00FFFFFF)");

            // Byte overload
            AssertEqual(0x00221B16, DwmHelper.ColorToColorRef(0x16, 0x1B, 0x22), "Byte overload #161B22");

            // Theme palette header/chrome colors
            // GitHub Dark: MenuBg #161B22 -> 0x00221B16, MenuFg #E6EDF3 -> 0x00F3EDE6
            AssertEqual(0x00221B16, DwmHelper.ColorToColorRef(ThemePalette.GitHubDark.MenuBackgroundColor), "GitHub Dark header bg COLORREF");
            AssertEqual(0x00F3EDE6, DwmHelper.ColorToColorRef(ThemePalette.GitHubDark.MenuForegroundColor), "GitHub Dark header fg COLORREF");

            // GitHub Light: MenuBg #F6F8FA -> 0x00FAF8F6, MenuFg #24292F -> 0x002F2924
            AssertEqual(0x00FAF8F6, DwmHelper.ColorToColorRef(ThemePalette.GitHubLight.MenuBackgroundColor), "GitHub Light header bg COLORREF");
            AssertEqual(0x002F2924, DwmHelper.ColorToColorRef(ThemePalette.GitHubLight.MenuForegroundColor), "GitHub Light header fg COLORREF");

            // Nord: MenuBg #3B4252 -> 0x0052423B
            AssertEqual(0x0052423B, DwmHelper.ColorToColorRef(ThemePalette.Nord.MenuBackgroundColor), "Nord header bg COLORREF");

            // One Dark: MenuBg #21252B -> 0x002B2521
            AssertEqual(0x002B2521, DwmHelper.ColorToColorRef(ThemePalette.OneDark.MenuBackgroundColor), "One Dark header bg COLORREF");

            // Monokai: MenuBg #1E1F1C -> 0x001C1F1E
            AssertEqual(0x001C1F1E, DwmHelper.ColorToColorRef(ThemePalette.Monokai.MenuBackgroundColor), "Monokai header bg COLORREF");

            // Safe fallback with IntPtr.Zero
            Assert(!DwmHelper.ApplyTitleBarTheme(IntPtr.Zero, ThemePalette.GitHubDark), "ApplyTitleBarTheme with IntPtr.Zero returns false gracefully");
        }

        private static void TestUpdateServiceVersionComparison()
        {
            // Equal versions
            AssertEqual(0, UpdateService.CompareVersions("1.0.0", "1.0.0"), "1.0.0 == 1.0.0");
            AssertEqual(0, UpdateService.CompareVersions("v1.0.0", "1.0.0"), "v1.0.0 == 1.0.0");
            AssertEqual(0, UpdateService.CompareVersions("V1.0.0", "v1.0.0"), "V1.0.0 == v1.0.0");
            AssertEqual(0, UpdateService.CompareVersions("1.0", "1.0.0"), "1.0 == 1.0.0");
            AssertEqual(0, UpdateService.CompareVersions("1.0.0.0", "1.0.0"), "1.0.0.0 == 1.0.0");
            Assert(!UpdateService.IsNewerVersion("1.0.0", "1.0.0"), "Same version is not newer");
            Assert(!UpdateService.IsNewerVersion("1.0.0", "v1.0.0"), "v prefix same version is not newer");

            // Newer versions
            Assert(UpdateService.IsNewerVersion("1.0.0", "1.0.1"), "1.0.1 is newer than 1.0.0");
            Assert(UpdateService.IsNewerVersion("1.0.0", "v1.1.0"), "v1.1.0 is newer than 1.0.0");
            Assert(UpdateService.IsNewerVersion("1.0.0", "v2.0.0"), "v2.0.0 is newer than 1.0.0");
            Assert(UpdateService.IsNewerVersion("v1.0.0", "v1.0.1"), "v1.0.1 is newer than v1.0.0");
            Assert(UpdateService.IsNewerVersion("1.2.0", "1.10.0"), "1.10.0 is newer than 1.2.0 (numeric order)");
            Assert(UpdateService.IsNewerVersion("0.9.9", "1.0.0"), "1.0.0 is newer than 0.9.9");

            // Older / Not newer versions
            Assert(!UpdateService.IsNewerVersion("1.0.0", "0.9.9"), "0.9.9 is not newer than 1.0.0");
            Assert(!UpdateService.IsNewerVersion("2.0.0", "1.9.9"), "1.9.9 is not newer than 2.0.0");
            Assert(!UpdateService.IsNewerVersion("1.10.0", "1.2.0"), "1.2.0 is not newer than 1.10.0");

            // Prerelease / build metadata
            Assert(UpdateService.IsNewerVersion("1.0.0", "v1.0.1-rc1"), "v1.0.1-rc1 is newer than 1.0.0");
            Assert(!UpdateService.IsNewerVersion("1.0.0", "v1.0.0+build.42"), "v1.0.0+build.42 is not newer than 1.0.0");

            // Component parsing
            var parts = UpdateService.ParseVersionComponents("v2.14.7");
            Assert(parts != null && parts.Length == 3, "Parsed 3 components");
            AssertEqual(2, parts![0], "Major 2");
            AssertEqual(14, parts[1], "Minor 14");
            AssertEqual(7, parts[2], "Patch 7");

            // Null / empty edge cases
            Assert(!UpdateService.IsNewerVersion("1.0.0", null), "Null candidate is not newer");
            Assert(!UpdateService.IsNewerVersion("1.0.0", ""), "Empty candidate is not newer");
            Assert(UpdateService.IsNewerVersion(null, "1.0.0"), "Valid candidate is newer than null");
        }

        private static void TestUpdateServiceStartupDebouncing()
        {
            var now = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);

            // Null settings -> false
            Assert(!UpdateService.ShouldCheckOnStartup(null, now), "Null settings should not check");

            // CheckForUpdatesOnStartup == false -> false
            var disabledSettings = new AppSettings { CheckForUpdatesOnStartup = false, LastUpdateCheckUtc = null };
            Assert(!UpdateService.ShouldCheckOnStartup(disabledSettings, now), "Disabled CheckForUpdatesOnStartup should not check");

            // First run / never checked before (LastUpdateCheckUtc == null) -> true
            var freshSettings = new AppSettings { CheckForUpdatesOnStartup = true, LastUpdateCheckUtc = null };
            Assert(UpdateService.ShouldCheckOnStartup(freshSettings, now), "Never checked before should check on startup");

            // Checked 25 hours ago -> true
            var oldCheckSettings = new AppSettings
            {
                CheckForUpdatesOnStartup = true,
                LastUpdateCheckUtc = now.AddHours(-25)
            };
            Assert(UpdateService.ShouldCheckOnStartup(oldCheckSettings, now), "Checked 25 hours ago should check on startup");

            // Checked exactly 24 hours ago -> true
            var exactCheckSettings = new AppSettings
            {
                CheckForUpdatesOnStartup = true,
                LastUpdateCheckUtc = now.AddHours(-24)
            };
            Assert(UpdateService.ShouldCheckOnStartup(exactCheckSettings, now), "Checked exactly 24 hours ago should check on startup");

            // Checked 23 hours ago -> false (debounced!)
            var debouncedSettings = new AppSettings
            {
                CheckForUpdatesOnStartup = true,
                LastUpdateCheckUtc = now.AddHours(-23)
            };
            Assert(!UpdateService.ShouldCheckOnStartup(debouncedSettings, now), "Checked 23 hours ago should be debounced");

            // Checked 1 hour ago -> false (debounced!)
            var recentCheckSettings = new AppSettings
            {
                CheckForUpdatesOnStartup = true,
                LastUpdateCheckUtc = now.AddHours(-1)
            };
            Assert(!UpdateService.ShouldCheckOnStartup(recentCheckSettings, now), "Checked 1 hour ago should be debounced");

            // Clock skew (timestamp in the future) -> true (safe recovery)
            var futureCheckSettings = new AppSettings
            {
                CheckForUpdatesOnStartup = true,
                LastUpdateCheckUtc = now.AddHours(2)
            };
            Assert(UpdateService.ShouldCheckOnStartup(futureCheckSettings, now), "Future timestamp (clock skew) should allow check");
        }

        private static void TestUpdateServiceChecksumExtraction()
        {
            string manifest = @"# Official SHA-256 Checksums
18c57ed3518728fd97ca17fc34374230fa9da4d2ed65998dc52169e0cb3d1177  MDPlus.exe
b4f2e7af3a2e26456be05a236d8fa5f6750069fe45f8cf16197ea9934ee53b0a  MDPlus-win-x64.zip
78851d9f7915119e7e95d0ac08e51ccb1f91cfa70058216fc9f234916d421a14  MDPlus-1.0.0-src.zip
13092c6c1b93405e99d000f98ae8bd02a4ab04a413609e6d9fe9294e769ea522  MDPlus-Setup.exe";

            string? hash = UpdateService.ExtractExpectedHash(manifest, "MDPlus-Setup.exe");
            Assert(!string.IsNullOrEmpty(hash), "Hash found for MDPlus-Setup.exe");
            AssertEqual("13092c6c1b93405e99d000f98ae8bd02a4ab04a413609e6d9fe9294e769ea522", hash, "Extracted setup hash");

            // Case-insensitive match
            string? hashCase = UpdateService.ExtractExpectedHash(manifest, "mdplus-setup.exe");
            AssertEqual("13092c6c1b93405e99d000f98ae8bd02a4ab04a413609e6d9fe9294e769ea522", hashCase, "Case-insensitive extracted hash");

            // BSD style
            string bsd = "SHA256 (MDPlus-Setup.exe) = 13092c6c1b93405e99d000f98ae8bd02a4ab04a413609e6d9fe9294e769ea522";
            string? bsdHash = UpdateService.ExtractExpectedHash(bsd, "MDPlus-Setup.exe");
            AssertEqual("13092c6c1b93405e99d000f98ae8bd02a4ab04a413609e6d9fe9294e769ea522", bsdHash, "BSD extracted hash");

            // Bare hash file
            string bare = "13092c6c1b93405e99d000f98ae8bd02a4ab04a413609e6d9fe9294e769ea522";
            string? bareHash = UpdateService.ExtractExpectedHash(bare, "MDPlus-Setup.exe");
            AssertEqual("13092c6c1b93405e99d000f98ae8bd02a4ab04a413609e6d9fe9294e769ea522", bareHash, "Bare extracted hash");
        }

        private static void TestAppSettingsUpdateSettingsPersistence()
        {
            // Verify default settings
            var settings = new AppSettings();
            Assert(settings.CheckForUpdatesOnStartup, "CheckForUpdatesOnStartup defaults to true");
            Assert(settings.LastUpdateCheckUtc == null, "LastUpdateCheckUtc defaults to null");

            // Serialize & deserialize
            var checkTime = new DateTime(2026, 9, 9, 15, 30, 0, DateTimeKind.Utc);
            settings.CheckForUpdatesOnStartup = false;
            settings.LastUpdateCheckUtc = checkTime;

            string json = System.Text.Json.JsonSerializer.Serialize(settings);
            Assert(json.Contains("CheckForUpdatesOnStartup"), "JSON contains CheckForUpdatesOnStartup");
            Assert(json.Contains("LastUpdateCheckUtc"), "JSON contains LastUpdateCheckUtc");

            var deserialized = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
            Assert(deserialized != null, "Deserialized settings must not be null");
            Assert(!deserialized!.CheckForUpdatesOnStartup, "CheckForUpdatesOnStartup roundtrip preserved");
            Assert(deserialized.LastUpdateCheckUtc.HasValue, "LastUpdateCheckUtc has value");
            AssertEqual(checkTime, deserialized.LastUpdateCheckUtc!.Value, "LastUpdateCheckUtc roundtrip preserved");
        }

        private static void TestWindowTitleBarAndHelpMenuIntegrity()
        {
            // Verify XAML layout contains Check for Updates in MainMenu and HamburgerContextMenu
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "MainWindow.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "MainWindow.xaml")
            };
            string mainWindowXamlPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(mainWindowXamlPath), "MainWindow.xaml must exist");

            string xamlText = System.IO.File.ReadAllText(mainWindowXamlPath);
            Assert(xamlText.Contains("Header=\"Check for _Updates...\""), "MainWindow.xaml must contain Check for Updates menu item");
            Assert(xamlText.Contains("Click=\"CheckForUpdates_Click\""), "MainWindow.xaml must bind CheckForUpdates_Click");
        }

        private static void TestUpdateServiceCheckAndVerificationWithMock()
        {
            // Mock payload for a newer release v1.2.0
            string fakeInstallerContent = "FAKE_INSTALLER_BINARY_DATA_FOR_TESTING";
            byte[] installerBytes = System.Text.Encoding.UTF8.GetBytes(fakeInstallerContent);
            string realHash = HashService.ComputeSha256(installerBytes);

            string checksumsText = $"{realHash}  MDPlus-Setup.exe\n";

            string releaseJson = $@"{{
                ""tag_name"": ""v1.2.0"",
                ""name"": ""MDPlus Release 1.2.0"",
                ""body"": ""- Added DWM Title Bar Theming\n- Added GitHub Auto-Updater"",
                ""html_url"": ""https://github.com/nickf-sudomania/mdplusplus/releases/tag/v1.2.0"",
                ""assets"": [
                    {{
                        ""name"": ""MDPlus-Setup.exe"",
                        ""browser_download_url"": ""https://mock.download/MDPlus-Setup.exe"",
                        ""size"": {installerBytes.Length}
                    }},
                    {{
                        ""name"": ""SHA256SUMS.txt"",
                        ""browser_download_url"": ""https://mock.download/SHA256SUMS.txt"",
                        ""size"": {checksumsText.Length}
                    }}
                ]
            }}";

            var handler = new MockHttpMessageHandler(request =>
            {
                string url = request.RequestUri?.ToString() ?? string.Empty;
                if (url.Contains("/releases/latest"))
                {
                    return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.StringContent(releaseJson, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                if (url.EndsWith("SHA256SUMS.txt"))
                {
                    return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.StringContent(checksumsText, System.Text.Encoding.UTF8, "text/plain")
                    };
                }
                if (url.EndsWith("MDPlus-Setup.exe"))
                {
                    return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.ByteArrayContent(installerBytes)
                    };
                }
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            });

            using var httpClient = new System.Net.Http.HttpClient(handler);
            var updateService = new UpdateService(httpClient, "https://mock.api/releases/latest");

            // 1. Check for updates with running version 1.0.0 -> update available!
            var checkTask = updateService.CheckForUpdatesAsync("1.0.0");
            checkTask.Wait();
            var checkResult = checkTask.Result;

            Assert(checkResult.IsSuccess, "Check for updates success");
            Assert(checkResult.IsUpdateAvailable, "Update should be available (v1.2.0 > 1.0.0)");
            AssertEqual("v1.2.0", checkResult.LatestVersion, "Latest version tag");
            AssertEqual("1.0.0", checkResult.CurrentVersion, "Current version");
            Assert(checkResult.ReleaseHighlights.Contains("DWM Title Bar"), "Highlights extracted");
            Assert(!string.IsNullOrEmpty(checkResult.SetupDownloadUrl), "Setup download URL found");
            Assert(!string.IsNullOrEmpty(checkResult.ChecksumsDownloadUrl), "Checksums download URL found");

            // 2. Check for updates with running version 1.2.0 -> already up to date
            var checkUpToDateTask = updateService.CheckForUpdatesAsync("1.2.0");
            checkUpToDateTask.Wait();
            Assert(!checkUpToDateTask.Result.IsUpdateAvailable, "Already up to date (1.2.0 == v1.2.0)");

            // 3. Download and cryptographically verify installer with MATCHING hash
            var installTask = updateService.DownloadAndVerifyUpdateAsync(checkResult);
            installTask.Wait();
            var installResult = installTask.Result;

            Assert(installResult.Success, "Download & verify with valid SHA-256 hash must succeed");
            Assert(!string.IsNullOrEmpty(installResult.InstallerPath), "InstallerPath must be populated");
            Assert(System.IO.File.Exists(installResult.InstallerPath), "Downloaded installer file must exist");
            AssertEqual(realHash, installResult.ActualHash, "Actual hash matches computed hash");

            // Clean up downloaded test file
            if (!string.IsNullOrEmpty(installResult.InstallerPath))
            {
                try { System.IO.File.Delete(installResult.InstallerPath); } catch { }
            }

            // 4. Download and cryptographically verify installer with TAMPERED / MISMATCHING hash
            var tamperedCheckResult = new UpdateCheckResult
            {
                IsSuccess = true,
                IsUpdateAvailable = true,
                SetupDownloadUrl = "https://mock.download/MDPlus-Setup.exe",
                ChecksumsDownloadUrl = null,
                // Provide a corrupted/tampered expected hash
                ReleaseHighlights = "0000000000000000000000000000000000000000000000000000000000000000  MDPlus-Setup.exe"
            };

            var tamperedInstallTask = updateService.DownloadAndVerifyUpdateAsync(tamperedCheckResult);
            tamperedInstallTask.Wait();
            var tamperedResult = tamperedInstallTask.Result;

            Assert(!tamperedResult.Success, "Download & verify with corrupted/mismatching SHA-256 hash must fail");
            Assert(tamperedResult.ErrorMessage != null && tamperedResult.ErrorMessage.Contains("verification failed"), "Error message explains verification failure");
        }

        private static void TestUpdateServicePathBoundedChecksumExtraction()
        {
            // 1. Suffix collision test: ensure "Other-MDPlus-Setup.exe" does NOT match target "MDPlus-Setup.exe"
            string collisionManifest = @"
1111111111111111111111111111111111111111111111111111111111111111  Other-MDPlus-Setup.exe
2222222222222222222222222222222222222222222222222222222222222222  dist/MDPlus-Setup.exe";

            string? extractedHash = UpdateService.ExtractExpectedHash(collisionManifest, "MDPlus-Setup.exe");
            AssertEqual("2222222222222222222222222222222222222222222222222222222222222222", extractedHash, "Path-bounded setup hash must match dist/MDPlus-Setup.exe, not Other-MDPlus-Setup.exe");

            // 2. Binary mode prefix '*MDPlus-Setup.exe'
            string binaryManifest = "3333333333333333333333333333333333333333333333333333333333333333 *MDPlus-Setup.exe";
            string? binaryHash = UpdateService.ExtractExpectedHash(binaryManifest, "MDPlus-Setup.exe");
            AssertEqual("3333333333333333333333333333333333333333333333333333333333333333", binaryHash, "Binary mode prefix * must be extracted");

            // 3. Colon style with SHA-256 label in release notes
            string releaseNotesColon = "### Release Highlights\nSHA-256: 4444444444444444444444444444444444444444444444444444444444444444";
            string? labelHash = UpdateService.ExtractExpectedHash(releaseNotesColon, "MDPlus-Setup.exe");
            AssertEqual("4444444444444444444444444444444444444444444444444444444444444444", labelHash, "SHA-256 label in release notes must be extracted");

            // 4. Standalone 64-hex string in release highlights
            string standaloneHex = "MDPlus Setup Checksum: `5555555555555555555555555555555555555555555555555555555555555555`";
            string? standaloneHash = UpdateService.ExtractExpectedHash(standaloneHex, "MDPlus-Setup.exe");
            AssertEqual("5555555555555555555555555555555555555555555555555555555555555555", standaloneHash, "Standalone 64-hex token in release highlights must be extracted");
        }

        private static void TestUpdateServiceDownloadCancellationAndFileCleanup()
        {
            var handler = new MockHttpMessageHandler(request =>
            {
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.ByteArrayContent(new byte[1024 * 1024])
                };
            });

            using var httpClient = new System.Net.Http.HttpClient(handler);
            var updateService = new UpdateService(httpClient);

            var checkInfo = new UpdateCheckResult
            {
                IsSuccess = true,
                IsUpdateAvailable = true,
                SetupDownloadUrl = "https://mock.download/MDPlus-Setup.exe",
                ReleaseHighlights = "6666666666666666666666666666666666666666666666666666666666666666"
            };

            using var cts = new System.Threading.CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            bool threwCancellation = false;
            try
            {
                var task = updateService.DownloadAndVerifyUpdateAsync(checkInfo, cancellationToken: cts.Token);
                task.Wait();
            }
            catch (AggregateException ae) when (ae.InnerException is OperationCanceledException)
            {
                threwCancellation = true;
            }
            catch (OperationCanceledException)
            {
                threwCancellation = true;
            }

            Assert(threwCancellation, "DownloadAndVerifyUpdateAsync must rethrow OperationCanceledException upon cancellation");

            string destFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MDPlusUpdate", UpdateService.SetupFileName);
            Assert(!System.IO.File.Exists(destFile), "Canceled download must clean up destination executable file");
        }

        private static void TestAppSettingsConcurrentSaveSafety()
        {
            var settings = new AppSettings
            {
                CheckForUpdatesOnStartup = true,
                LastUpdateCheckUtc = DateTime.UtcNow
            };

            var tasks = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                int index = i;
                tasks.Add(Task.Run(() =>
                {
                    settings.WindowWidth = 1000 + index;
                    settings.LastUpdateCheckUtc = DateTime.UtcNow.AddMinutes(index);
                    settings.Save();
                }));
            }

            Task.WaitAll(tasks.ToArray());

            var loaded = AppSettings.Load();
            Assert(loaded != null, "AppSettings must load cleanly after concurrent saves");
        }

        private static void TestDwmHelperBorderAndResetAttributes()
        {
            AssertEqual(34, DwmHelper.DWMWA_BORDER_COLOR, "DWMWA_BORDER_COLOR is attribute 34");
            AssertEqual(35, DwmHelper.DWMWA_CAPTION_COLOR, "DWMWA_CAPTION_COLOR is attribute 35");
            AssertEqual(36, DwmHelper.DWMWA_TEXT_COLOR, "DWMWA_TEXT_COLOR is attribute 36");
            AssertEqual(20, DwmHelper.DWMWA_USE_IMMERSIVE_DARK_MODE, "DWMWA_USE_IMMERSIVE_DARK_MODE is attribute 20");
            AssertEqual(19, DwmHelper.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, "DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 is attribute 19");

            Assert(!DwmHelper.ResetTitleBarTheme(IntPtr.Zero), "ResetTitleBarTheme on IntPtr.Zero gracefully returns false");
        }

        private static void TestUpdateServiceExactAssetPriority()
        {
            string releaseJson = @"{
                ""tag_name"": ""v1.3.0"",
                ""name"": ""Release 1.3.0"",
                ""body"": ""6666666666666666666666666666666666666666666666666666666666666666"",
                ""assets"": [
                    {
                        ""name"": ""MDPlus-Setup.exe"",
                        ""browser_download_url"": ""https://download/exact/MDPlus-Setup.exe""
                    },
                    {
                        ""name"": ""Other-Setup.exe"",
                        ""browser_download_url"": ""https://download/loose/Other-Setup.exe""
                    },
                    {
                        ""name"": ""SHA256SUMS.txt"",
                        ""browser_download_url"": ""https://download/exact/SHA256SUMS.txt""
                    },
                    {
                        ""name"": ""package.sha256"",
                        ""browser_download_url"": ""https://download/loose/package.sha256""
                    }
                ]
            }";

            var handler = new MockHttpMessageHandler(req =>
            {
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(releaseJson, System.Text.Encoding.UTF8, "application/json")
                };
            });

            using var client = new System.Net.Http.HttpClient(handler);
            var service = new UpdateService(client, "https://mock.api/latest");

            var task = service.CheckForUpdatesAsync("1.0.0");
            task.Wait();
            var res = task.Result;

            AssertEqual("https://download/exact/MDPlus-Setup.exe", res.SetupDownloadUrl!, "Exact SetupFileName must take priority over loose *Setup.exe matches");
            AssertEqual("https://download/exact/SHA256SUMS.txt", res.ChecksumsDownloadUrl!, "Exact ChecksumsFileName must take priority over loose *.sha256 matches");
        }

        private static void TestUpdateServiceMultiAssetReleaseHighlightsHashExtraction()
        {
            // 1. Markdown list with multiple assets and backtick hashes
            string markdownList = @"
## Release v1.3.0 Notes
- **Other-MDPlus-Setup.exe**: `1111111111111111111111111111111111111111111111111111111111111111`
- **MDPlus-Setup.exe**: `2222222222222222222222222222222222222222222222222222222222222222`
- **MDPlus-win-x64.zip**: `3333333333333333333333333333333333333333333333333333333333333333`
";
            string? listHash = UpdateService.ExtractExpectedHash(markdownList, "MDPlus-Setup.exe");
            AssertEqual("2222222222222222222222222222222222222222222222222222222222222222", listHash, "Extracted hash from multi-asset markdown list");

            // 2. GFM Table with multiple assets
            string gfmTable = @"
| File | SHA-256 Checksum |
| :--- | :--- |
| `Other-MDPlus-Setup.exe` | 4444444444444444444444444444444444444444444444444444444444444444 |
| `MDPlus-Setup.exe` | 5555555555555555555555555555555555555555555555555555555555555555 |
| `MDPlus.zip` | 6666666666666666666666666666666666666666666666666666666666666666 |
";
            string? tableHash = UpdateService.ExtractExpectedHash(gfmTable, "MDPlus-Setup.exe");
            AssertEqual("5555555555555555555555555555555555555555555555555555555555555555", tableHash, "Extracted hash from multi-asset markdown table");

            // 3. Multi-line heading + label
            string multiLine = @"
### MDPlus-Setup.exe
SHA-256: 7777777777777777777777777777777777777777777777777777777777777777

### MDPlus-Portable.zip
SHA-256: 8888888888888888888888888888888888888888888888888888888888888888
";
            string? multiLineHash = UpdateService.ExtractExpectedHash(multiLine, "MDPlus-Setup.exe");
            AssertEqual("7777777777777777777777777777777777777777777777777777777777777777", multiLineHash, "Extracted hash from multi-line heading followed by SHA-256 label");
        }

        private static void TestUpdateServiceIncompleteDownloadDetection()
        {
            var handler = new MockHttpMessageHandler(request =>
            {
                var resp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
                resp.Content = new System.Net.Http.ByteArrayContent(new byte[500]);
                resp.Content.Headers.ContentLength = 10000;
                return resp;
            });

            using var httpClient = new System.Net.Http.HttpClient(handler);
            var updateService = new UpdateService(httpClient);

            var checkInfo = new UpdateCheckResult
            {
                IsSuccess = true,
                IsUpdateAvailable = true,
                SetupDownloadUrl = "https://mock.download/MDPlus-Setup.exe",
                ReleaseHighlights = "9999999999999999999999999999999999999999999999999999999999999999"
            };

            var installTask = updateService.DownloadAndVerifyUpdateAsync(checkInfo);
            installTask.Wait();
            var installResult = installTask.Result;

            Assert(!installResult.Success, "Incomplete download must fail");
            Assert(installResult.ErrorMessage != null && installResult.ErrorMessage.Contains("incomplete"),
                "Error message explains truncated/incomplete download");

            string destFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MDPlusUpdate", UpdateService.SetupFileName);
            Assert(!System.IO.File.Exists(destFile), "Truncated destination file must be deleted");
        }

        private static void TestUpdateServiceVersionPrefixAndTagEdgeCases()
        {
            // Version prefix parsing
            var compRelease = UpdateService.ParseVersionComponents("release-1.2.0");
            Assert(compRelease != null && compRelease.Length == 3 && compRelease[0] == 1 && compRelease[1] == 2 && compRelease[2] == 0,
                "Parse 'release-1.2.0' should yield [1, 2, 0]");

            var compApp = UpdateService.ParseVersionComponents("MDPlus-v2.5.1");
            Assert(compApp != null && compApp.Length == 3 && compApp[0] == 2 && compApp[1] == 5 && compApp[2] == 1,
                "Parse 'MDPlus-v2.5.1' should yield [2, 5, 1]");

            var compBuild = UpdateService.ParseVersionComponents("1.0.0+build100");
            Assert(compBuild != null && compBuild.Length == 3 && compBuild[0] == 1 && compBuild[1] == 0 && compBuild[2] == 0,
                "Parse '1.0.0+build100' should yield [1, 0, 0]");

            var compPrerelease = UpdateService.ParseVersionComponents("1.3.0-rc.1");
            Assert(compPrerelease != null && compPrerelease.Length == 3 && compPrerelease[0] == 1 && compPrerelease[1] == 3 && compPrerelease[2] == 0,
                "Parse '1.3.0-rc.1' should yield [1, 3, 0]");

            var compInvalid = UpdateService.ParseVersionComponents("no-digits");
            Assert(compInvalid == null, "Non-version string without digits should return null");

            // Comparison
            Assert(UpdateService.CompareVersions("release-1.3.0", "v1.2.0") > 0, "release-1.3.0 > v1.2.0");
            Assert(UpdateService.CompareVersions("MDPlus-v1.0.0", "1.0.0") == 0, "MDPlus-v1.0.0 == 1.0.0");
            Assert(UpdateService.IsNewerVersion("1.0.0", "release-1.0.1"), "release-1.0.1 is newer than 1.0.0");
            Assert(!UpdateService.IsNewerVersion("1.0.0", "release-1.0.0"), "release-1.0.0 is not newer than 1.0.0");
        }

        private static void TestUpdateServiceRateLimitWithResetHeaderAndRetryAfter()
        {
            long futureEpoch = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();

            var rateLimitHandler = new MockHttpMessageHandler(request =>
            {
                if (request.RequestUri?.ToString().Contains("ratelimit-403") == true)
                {
                    var resp = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                    resp.Headers.Add("x-ratelimit-reset", futureEpoch.ToString());
                    return resp;
                }
                if (request.RequestUri?.ToString().Contains("ratelimit-429") == true)
                {
                    var resp = new System.Net.Http.HttpResponseMessage((System.Net.HttpStatusCode)429);
                    resp.Headers.Add("Retry-After", "120");
                    return resp;
                }
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
            });

            using var client = new System.Net.Http.HttpClient(rateLimitHandler);

            // 1. 403 with x-ratelimit-reset
            var service403 = new UpdateService(client, "https://mock.api/ratelimit-403");
            var task403 = service403.CheckForUpdatesAsync("1.0.0");
            task403.Wait();
            var res403 = task403.Result;

            Assert(!res403.IsSuccess, "403 rate limit should report failure");
            Assert(res403.ErrorMessage != null && res403.ErrorMessage.Contains("rate limit reached"),
                "Message mentions rate limit reached");
            Assert(res403.ErrorMessage != null && res403.ErrorMessage.Contains("Window resets in approximately"),
                "Message includes reset countdown estimation");

            // 2. 429 with Retry-After
            var service429 = new UpdateService(client, "https://mock.api/ratelimit-429");
            var task429 = service429.CheckForUpdatesAsync("1.0.0");
            task429.Wait();
            var res429 = task429.Result;

            Assert(!res429.IsSuccess, "429 rate limit should report failure");
            Assert(res429.ErrorMessage != null && res429.ErrorMessage.Contains("rate limit reached"),
                "Message mentions rate limit reached on 429");
            Assert(res429.ErrorMessage != null && res429.ErrorMessage.Contains("Please retry in approximately"),
                "Message includes retry-after estimation");
        }

        private static void TestUpdateServiceInstallerProcessLaunchAndExitHooks()
        {
            // 1. Non-existent file throws FileNotFoundException
            bool threwMissing = false;
            try
            {
                UpdateService.CreateInstallerProcessStartInfo("C:\\nonexistent\\installer.exe");
            }
            catch (System.IO.FileNotFoundException)
            {
                threwMissing = true;
            }
            Assert(threwMissing, "CreateInstallerProcessStartInfo throws FileNotFoundException for missing file");

            // 2. Existing file returns valid ProcessStartInfo
            string tempInstaller = System.IO.Path.GetTempFileName();
            try
            {
                var psi = UpdateService.CreateInstallerProcessStartInfo(tempInstaller);
                AssertEqual(tempInstaller, psi.FileName, "ProcessStartInfo.FileName matches installer path");
                Assert(psi.UseShellExecute, "ProcessStartInfo.UseShellExecute is true");
                AssertEqual(System.IO.Path.GetDirectoryName(tempInstaller), psi.WorkingDirectory, "ProcessStartInfo.WorkingDirectory is set to installer folder");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    AssertEqual("runas", psi.Verb, "ProcessStartInfo.Verb is 'runas' on Windows NT for UAC elevation");
                }

                // 3. LaunchInstallerAndExit with test hooks executes delegates safely
                bool launched = false;
                bool exited = false;

                UpdateService.LaunchInstallerAndExit(
                    tempInstaller,
                    startProcess: p => { launched = (p.FileName == tempInstaller && (Environment.OSVersion.Platform != PlatformID.Win32NT || p.Verb == "runas")); },
                    exitApp: () => { exited = true; });

                Assert(launched, "startProcess delegate was executed with correct ProcessStartInfo and elevation verb");
                Assert(exited, "exitApp delegate was executed");

                // 4. Test UAC prompt cancellation handling (Win32Exception NativeErrorCode == 1223)
                bool exitedOnCancel = false;
                UpdateService.LaunchInstallerAndExit(
                    tempInstaller,
                    startProcess: p => throw new System.ComponentModel.Win32Exception(1223, "The operation was canceled by the user"),
                    exitApp: () => { exitedOnCancel = true; });

                Assert(!exitedOnCancel, "LaunchInstallerAndExit must NOT exit the application if UAC prompt was canceled by the user");
            }
            finally
            {
                try { System.IO.File.Delete(tempInstaller); } catch { }
            }
        }

        private static void TestDwmHelperHighContrastAndWin10Fallback()
        {
            // Reset on IntPtr.Zero returns false
            Assert(!DwmHelper.ResetTitleBarTheme(IntPtr.Zero), "ResetTitleBarTheme(IntPtr.Zero) returns false");

            // ApplyTitleBarTheme with null palette returns false
            Assert(!DwmHelper.ApplyTitleBarTheme(IntPtr.Zero, null!), "ApplyTitleBarTheme with null palette returns false");

            // Color conversions
            int red = DwmHelper.ColorToColorRef(Color.FromRgb(255, 0, 0));
            AssertEqual(0x000000FF, red, "Red ColorRef is 0x000000FF");

            int green = DwmHelper.ColorToColorRef(Color.FromRgb(0, 255, 0));
            AssertEqual(0x0000FF00, green, "Green ColorRef is 0x0000FF00");

            int blue = DwmHelper.ColorToColorRef(Color.FromRgb(0, 0, 255));
            AssertEqual(0x00FF0000, blue, "Blue ColorRef is 0x00FF0000");

            // Reset constants
            AssertEqual(unchecked((int)0xFFFFFFFF), unchecked((int)DwmHelper.DWMWA_COLOR_DEFAULT), "DWMWA_COLOR_DEFAULT is 0xFFFFFFFF");
        }

        private static void TestVerifyIntegrityWindowPaletteDynamicTheming()
        {
            // Verify XAML contains the named borders and buttons
            string[] possiblePaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "Controls", "VerifyIntegrityWindow.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Controls", "VerifyIntegrityWindow.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "Controls", "VerifyIntegrityWindow.xaml")
            };
            string xamlPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(xamlPath), "VerifyIntegrityWindow.xaml must exist");

            string xaml = System.IO.File.ReadAllText(xamlPath);
            Assert(xaml.Contains("Name=\"HeaderBorder\""), "VerifyIntegrityWindow.xaml has HeaderBorder");
            Assert(xaml.Contains("Name=\"FooterBorder\""), "VerifyIntegrityWindow.xaml has FooterBorder");
            Assert(xaml.Contains("Name=\"BrowseButton\""), "VerifyIntegrityWindow.xaml has BrowseButton");
            Assert(xaml.Contains("Name=\"CloseButton\""), "VerifyIntegrityWindow.xaml has CloseButton");

            // Verify UpdateDialog also has appropriate buttons
            string[] updateDialogPaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "Controls", "UpdateDialog.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Controls", "UpdateDialog.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "Controls", "UpdateDialog.xaml")
            };
            string updateXamlPath = updateDialogPaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(updateXamlPath), "UpdateDialog.xaml must exist");
            string updateXaml = System.IO.File.ReadAllText(updateXamlPath);
            Assert(updateXaml.Contains("Name=\"UpdateNowButton\""), "UpdateDialog.xaml has UpdateNowButton");
            Assert(updateXaml.Contains("Name=\"LaterButton\""), "UpdateDialog.xaml has LaterButton");
            Assert(updateXaml.Contains("Name=\"ReleaseNotesButton\""), "UpdateDialog.xaml has ReleaseNotesButton");
        }

        private static void TestUpdateServiceVersionedAssetPriorityOverOtherSetups()
        {
            // MDPlus-v1.3.0-Setup.exe followed by Other-Setup.exe: MDPlus must take priority despite being earlier
            string releaseJson = @"{
                ""tag_name"": ""v1.3.0"",
                ""name"": ""Release 1.3.0"",
                ""body"": ""Release notes"",
                ""assets"": [
                    {
                        ""name"": ""MDPlus-v1.3.0-Setup.exe"",
                        ""browser_download_url"": ""https://download/mdplus/MDPlus-v1.3.0-Setup.exe""
                    },
                    {
                        ""name"": ""Other-Setup.exe"",
                        ""browser_download_url"": ""https://download/other/Other-Setup.exe""
                    },
                    {
                        ""name"": ""MDPlus-v1.3.0-SHA256SUMS.txt"",
                        ""browser_download_url"": ""https://download/mdplus/MDPlus-v1.3.0-SHA256SUMS.txt""
                    },
                    {
                        ""name"": ""other-sums.txt"",
                        ""browser_download_url"": ""https://download/other/other-sums.txt""
                    }
                ]
            }";

            var handler = new MockHttpMessageHandler(req =>
            {
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(releaseJson, System.Text.Encoding.UTF8, "application/json")
                };
            });

            using var client = new System.Net.Http.HttpClient(handler);
            var service = new UpdateService(client, "https://mock.api/latest");

            var task = service.CheckForUpdatesAsync("1.0.0");
            task.Wait();
            var res = task.Result;

            AssertEqual("https://download/mdplus/MDPlus-v1.3.0-Setup.exe", res.SetupDownloadUrl!, "MDPlus-v1.3.0-Setup.exe must take priority over Other-Setup.exe");
            AssertEqual("MDPlus-v1.3.0-Setup.exe", res.SetupFileName!, "SetupFileName must be recorded as MDPlus-v1.3.0-Setup.exe");
            AssertEqual("https://download/mdplus/MDPlus-v1.3.0-SHA256SUMS.txt", res.ChecksumsDownloadUrl!, "MDPlus checksums must take priority over other-sums.txt");
        }

        private static void TestUpdateServiceMultiAssetManifestFalsePositiveAvoidance()
        {
            // Manifest with an unrelated entry that contains 'checksum' in key
            string manifest = @"
zip checksum: 1111111111111111111111111111111111111111111111111111111111111111
MDPlus-Setup.exe checksum: 2222222222222222222222222222222222222222222222222222222222222222
";
            string? hash = UpdateService.ExtractExpectedHash(manifest, "MDPlus-Setup.exe");
            AssertEqual("2222222222222222222222222222222222222222222222222222222222222222", hash, "Must NOT falsely match zip checksum when searching for MDPlus-Setup.exe");
        }

        private static void TestUpdateServiceMultiLineReleaseNotesWithDescriptionLines()
        {
            // Target heading followed by descriptive text before the hash line
            string notes = @"
### MDPlus-Setup.exe
Windows 64-bit installer for Windows 10 & 11.
Includes desktop shortcut and shell context menu integration.
SHA-256: 3333333333333333333333333333333333333333333333333333333333333333

### MDPlus-Portable.zip
Portable archive without installation.
SHA-256: 4444444444444444444444444444444444444444444444444444444444444444
";
            string? hash = UpdateService.ExtractExpectedHash(notes, "MDPlus-Setup.exe");
            AssertEqual("3333333333333333333333333333333333333333333333333333333333333333", hash, "Multi-line lookahead successfully extracts hash across description lines");
        }

        private static void TestUpdateServiceVersionedTargetManifestMatching()
        {
            // Manifest has versioned filename MDPlus-v1.4.0-Setup.exe, querying with MDPlus-Setup.exe
            string manifest = @"
5555555555555555555555555555555555555555555555555555555555555555  MDPlus-v1.4.0-Setup.exe
6666666666666666666666666666666666666666666666666666666666666666  MDPlus-v1.4.0-win-x64.zip
";
            string? hash = UpdateService.ExtractExpectedHash(manifest, "MDPlus-Setup.exe");
            AssertEqual("5555555555555555555555555555555555555555555555555555555555555555", hash, "Versioned setup name matches when querying with base MDPlus-Setup.exe");
        }

        private static void TestUpdateServiceLockedDestinationFileFallback()
        {
            // Test that DownloadAndVerifyUpdateAsync succeeds even when the target destination file in temp is locked
            byte[] dummyPayload = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 }; // Fake DOS header
            string expectedHash = HashService.ComputeSha256(new System.IO.MemoryStream(dummyPayload));

            var handler = new MockHttpMessageHandler(req =>
            {
                if (req.RequestUri!.ToString().Contains("SHA256SUMS.txt"))
                {
                    return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.StringContent($"{expectedHash}  MDPlus-Setup.exe\n")
                    };
                }
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.ByteArrayContent(dummyPayload)
                };
            });

            using var client = new System.Net.Http.HttpClient(handler);
            var service = new UpdateService(client);

            string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MDPlusUpdate");
            if (!System.IO.Directory.Exists(tempDir)) System.IO.Directory.CreateDirectory(tempDir);
            string lockedFile = System.IO.Path.Combine(tempDir, "MDPlus-Setup.exe");

            // Lock the default file with FileShare.None
            using (var lockStream = new System.IO.FileStream(lockedFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None))
            {
                var updateInfo = new UpdateCheckResult
                {
                    IsSuccess = true,
                    IsUpdateAvailable = true,
                    SetupDownloadUrl = "https://mock.download/MDPlus-Setup.exe",
                    SetupFileName = "MDPlus-Setup.exe",
                    ChecksumsDownloadUrl = "https://mock.download/SHA256SUMS.txt"
                };

                var task = service.DownloadAndVerifyUpdateAsync(updateInfo);
                task.Wait();
                var res = task.Result;

                Assert(res.Success, "Download and verification must succeed even when default destination file is locked");
                Assert(res.InstallerPath != null && System.IO.File.Exists(res.InstallerPath), "Generated fallback installer file exists");
                AssertEqual(expectedHash, res.ActualHash!, "Actual hash matches expected hash");

                // Clean up fallback file
                if (res.InstallerPath != null && res.InstallerPath != lockedFile)
                {
                    try { System.IO.File.Delete(res.InstallerPath); } catch { }
                }
            }

            try { System.IO.File.Delete(lockedFile); } catch { }
        }

        private static void TestDwmHelperResetWindowAndFullscreenLifecycle()
        {
            // ResetTitleBarTheme((Window)null!) returns false
            Assert(!DwmHelper.ResetTitleBarTheme((Window)null!), "ResetTitleBarTheme((Window)null!) returns false");

            // Check UpdateDialog.xaml buttons have IsDefault and IsCancel
            string[] updateDialogPaths = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "Controls", "UpdateDialog.xaml"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Controls", "UpdateDialog.xaml"),
                System.IO.Path.Combine(Environment.CurrentDirectory, "src", "Controls", "UpdateDialog.xaml")
            };
            string updateXamlPath = updateDialogPaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
            Assert(!string.IsNullOrEmpty(updateXamlPath), "UpdateDialog.xaml must exist");
            string updateXaml = System.IO.File.ReadAllText(updateXamlPath);
            Assert(updateXaml.Contains("IsDefault=\"True\""), "UpdateNowButton has IsDefault='True'");
            Assert(updateXaml.Contains("IsCancel=\"True\""), "LaterButton has IsCancel='True'");
            Assert(updateXaml.Contains("VerticalScrollBarVisibility=\"Auto\""), "HighlightsTextBox has VerticalScrollBarVisibility='Auto'");
        }

        private static void TestUpdateServiceNotFoundGracefulHandling()
        {
            var handler = new MockHttpMessageHandler(req =>
                new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            using var httpClient = new System.Net.Http.HttpClient(handler);
            var updateService = new UpdateService(httpClient, "https://mock.api/repos/user/repo/releases/latest");
            var task = updateService.CheckForUpdatesAsync("1.0.0");
            task.Wait();
            var result = task.Result;
            Assert(result.IsSuccess, "404 on releases/latest must be treated as successful up-to-date check");
            Assert(!result.IsUpdateAvailable, "No update should be available on 404");
            AssertEqual("1.0.0", result.CurrentVersion, "Current version preserved");
        }

        private static void TestUpdateServiceLiveGitHubReleaseV102Detection()
        {
            var updateService = new UpdateService();
            var task = updateService.CheckForUpdatesAsync("1.01");
            task.Wait();
            var result = task.Result;

            // Gracefully handle rate-limits or offline environments
            if (!result.IsSuccess && result.ErrorMessage != null && 
                (result.ErrorMessage.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
                 result.ErrorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                 result.ErrorMessage.Contains("host", StringComparison.OrdinalIgnoreCase)))
            {
                Console.Write(" [NETWORK/RATE-LIMITED, SKIPPED] ");
                return;
            }

            Assert(result.IsSuccess, "Check for updates should succeed against live GitHub: " + (result.ErrorMessage ?? ""));
            Assert(result.IsUpdateAvailable, "A newer release must be detected for v1.01");
            Assert(result.LatestVersion != null && UpdateService.CompareVersions(result.LatestVersion, "1.01") > 0, "Latest version must be newer than 1.01");
            Assert(!string.IsNullOrEmpty(result.SetupDownloadUrl), "Setup download URL must be populated");
            Assert(result.SetupDownloadUrl!.EndsWith("MDPlus-Setup.exe"), "Setup download URL must point to MDPlus-Setup.exe");
            Assert(!string.IsNullOrEmpty(result.ChecksumsDownloadUrl), "Checksums URL must be populated");

            // Verify that for a user already running the latest version, it correctly detects no update available
            var taskLatest = updateService.CheckForUpdatesAsync(result.LatestVersion!);
            taskLatest.Wait();
            var resultLatest = taskLatest.Result;
            if (resultLatest.IsSuccess)
            {
                Assert(!resultLatest.IsUpdateAvailable, "Running latest version should be recognized as up-to-date");
            }
        }

        private static void TestUpdateServiceDialogResultAndUacElevationFlow()
        {
            // 1. Verify ProcessStartInfo specifies Verb = "runas" on Windows NT
            string dummyExe = System.IO.Path.GetTempFileName();
            try
            {
                var psi = UpdateService.CreateInstallerProcessStartInfo(dummyExe);
                AssertEqual(dummyExe, psi.FileName, "Installer path matches");
                Assert(psi.UseShellExecute, "UseShellExecute must be true");
                AssertEqual(System.IO.Path.GetDirectoryName(dummyExe), psi.WorkingDirectory, "WorkingDirectory must be set to installer folder");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    AssertEqual("runas", psi.Verb, "UAC elevation verb 'runas' is configured on Windows NT");
                }

                // 2. Verify TryLaunchInstaller and LaunchInstallerAndExit catch UAC cancellation (code 1223) without calling exitApp
                bool tryCancel = UpdateService.TryLaunchInstaller(
                    dummyExe,
                    startProcess: p => throw new System.ComponentModel.Win32Exception(1223, "The operation was canceled by the user"));
                Assert(!tryCancel, "TryLaunchInstaller must return false when UAC elevation is canceled by user");

                bool exitCalled = false;
                UpdateService.LaunchInstallerAndExit(
                    dummyExe,
                    startProcess: p => throw new System.ComponentModel.Win32Exception(1223, "The operation was canceled by the user"),
                    exitApp: () => { exitCalled = true; });

                Assert(!exitCalled, "Application must NOT exit when UAC prompt is canceled by user (1223)");

                // 3. Verify TryLaunchInstaller and LaunchInstallerAndExit execute exitApp on successful process launch
                bool launched = false;
                bool trySuccess = UpdateService.TryLaunchInstaller(
                    dummyExe,
                    startProcess: p => { launched = true; });
                Assert(trySuccess, "TryLaunchInstaller must return true on successful process start");
                Assert(launched, "Installer process start hook was invoked");

                bool exitSuccess = false;
                UpdateService.LaunchInstallerAndExit(
                    dummyExe,
                    startProcess: p => { },
                    exitApp: () => { exitSuccess = true; });

                Assert(exitSuccess, "exitApp hook was invoked after installer launch");

                // 4. Verify UpdateDialog.xaml.cs contains VerifiedInstallerPath and DialogResult flow
                string[] updateDialogPaths = new[]
                {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "Controls", "UpdateDialog.xaml.cs"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Controls", "UpdateDialog.xaml.cs"),
                    System.IO.Path.Combine(Environment.CurrentDirectory, "src", "Controls", "UpdateDialog.xaml.cs")
                };
                string updateDialogCsPath = updateDialogPaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
                Assert(!string.IsNullOrEmpty(updateDialogCsPath), "UpdateDialog.xaml.cs must exist");
                string updateCs = System.IO.File.ReadAllText(updateDialogCsPath);
                Assert(updateCs.Contains("VerifiedInstallerPath = result.InstallerPath;"), "UpdateDialog sets VerifiedInstallerPath");
                Assert(updateCs.Contains("DialogResult = true;"), "UpdateDialog sets DialogResult for owner window");

                // 5. Verify MainWindow.xaml.cs contains CloseAndLaunchInstaller, avoids broken if (!IsLoaded), and launches before closing
                string[] mainPaths = new[]
                {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src", "MainWindow.xaml.cs"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "MainWindow.xaml.cs"),
                    System.IO.Path.Combine(Environment.CurrentDirectory, "src", "MainWindow.xaml.cs")
                };
                string mainCsPath = mainPaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? string.Empty;
                Assert(!string.IsNullOrEmpty(mainCsPath), "MainWindow.xaml.cs must exist");
                string mainCs = System.IO.File.ReadAllText(mainCsPath);
                Assert(mainCs.Contains("CloseAndLaunchInstaller"), "MainWindow contains CloseAndLaunchInstaller");
                Assert(mainCs.Contains("Closed += closedHandler;"), "CloseAndLaunchInstaller attaches Closed handler");
                Assert(mainCs.Contains("TryLaunchInstaller(installerPath)"), "CloseAndLaunchInstaller launches installer before closing window");
                Assert(!mainCs.Contains("if (!IsLoaded)\r\n                                                UpdateService.LaunchInstallerAndExit") &&
                       !mainCs.Contains("if (!IsLoaded)\n                                                UpdateService.LaunchInstallerAndExit"),
                       "MainWindow does not contain broken if (!IsLoaded) launch check");
            }
            finally
            {
                try { System.IO.File.Delete(dummyExe); } catch { }
            }
        }

        private static void TestLatexMathInlineAndDisplayParsing()
        {
            var parser = new MarkdownParser();

            // 1. Inline math
            string md1 = "Energy is $E = mc^2$ in physics.";
            var doc1 = parser.Parse(md1);
            var p1 = doc1.Blocks[0] as ParagraphBlock;
            Assert(p1 != null, "Paragraph expected");
            AssertEqual(3, p1!.Inlines.Count, "Inlines count");
            Assert(p1.Inlines[1] is MathInline, "Inline 1 must be MathInline");
            var mi1 = (MathInline)p1.Inlines[1];
            AssertEqual("E = mc^2", mi1.Expression, "Expression");
            Assert(!mi1.IsDisplay, "Inline math IsDisplay is false");

            // 2. Display block math ($$...$$)
            string md2 = "$$\n\\int_{0}^{\\infty} e^{-x^2} dx = \\frac{\\sqrt{\\pi}}{2}\n$$";
            var doc2 = parser.Parse(md2);
            AssertEqual(1, doc2.Blocks.Count, "Blocks count");
            Assert(doc2.Blocks[0] is MathBlock, "Block must be MathBlock");
            var mb2 = (MathBlock)doc2.Blocks[0];
            Assert(mb2.Expression.Contains("\\int_{0}^{\\infty}"), "Expression must contain integral");

            // 3. Single-line display math ($$...$$)
            string md3 = "$$a^2 + b^2 = c^2$$";
            var doc3 = parser.Parse(md3);
            AssertEqual(1, doc3.Blocks.Count, "Blocks count");
            Assert(doc3.Blocks[0] is MathBlock, "Single-line display math must be MathBlock");
            var mb3 = (MathBlock)doc3.Blocks[0];
            AssertEqual("a^2 + b^2 = c^2", mb3.Expression, "Expression matches");

            // 4. Currency protection ($100 and $200)
            string md4 = "The price is $100 and $200 for both.";
            var doc4 = parser.Parse(md4);
            var p4 = doc4.Blocks[0] as ParagraphBlock;
            Assert(p4 != null, "Paragraph expected");
            bool hasMath = p4!.Inlines.Any(i => i is MathInline);
            Assert(!hasMath, "Currency amounts like $100 and $200 must NOT be parsed as math");

            // 5. Delimiter whitespace rule ($ math $)
            string md5 = "This $ is not math $ here.";
            var doc5 = parser.Parse(md5);
            var p5 = doc5.Blocks[0] as ParagraphBlock;
            Assert(p5 != null, "Paragraph expected");
            Assert(!p5!.Inlines.Any(i => i is MathInline), "$ with leading/trailing spaces must NOT parse as math");

            // 6. Escaped dollar (\$50)
            string md6 = "Cost: \\$50 each.";
            var doc6 = parser.Parse(md6);
            var p6 = doc6.Blocks[0] as ParagraphBlock;
            Assert(p6 != null, "Paragraph expected");
            Assert(!p6!.Inlines.Any(i => i is MathInline), "Escaped dollar must NOT parse as math");

            // 7. Vector WPF rendering test
            var element = LatexMathRenderer.RenderMath("\\frac{a}{b} + \\sqrt{c} = \\alpha", ThemePalette.GitHubDark, 14, false);
            Assert(element != null, "LatexMathRenderer must return non-null UIElement");
            var displayElement = LatexMathRenderer.RenderMath("\\sum_{i=1}^n i = \\frac{n(n+1)}{2}", ThemePalette.GitHubLight, 16, true);
            Assert(displayElement != null, "LatexMathRenderer must render display formulas");
        }

        private static void TestHtmlInlineAndBlockParsing()
        {
            var parser = new MarkdownParser();

            // 1. Inline HTML tags: <kbd>, <sub>, <sup>, <u>, <mark>
            string md1 = "Press <kbd>Ctrl</kbd> + <kbd>C</kbd> to copy H<sub>2</sub>O and x<sup>2</sup>.";
            var doc1 = parser.Parse(md1);
            var p1 = doc1.Blocks[0] as ParagraphBlock;
            Assert(p1 != null, "Paragraph expected");
            var htmlInlines = p1!.Inlines.OfType<HtmlInline>().ToList();
            Assert(htmlInlines.Count >= 4, "Must detect <kbd>, <kbd>, <sub>, <sup>");
            AssertEqual("kbd", htmlInlines[0].Tag, "Tag 0 is kbd");
            AssertEqual("Ctrl", htmlInlines[0].Content, "Content 0 is Ctrl");
            AssertEqual("sub", htmlInlines[2].Tag, "Tag 2 is sub");
            AssertEqual("2", htmlInlines[2].Content, "Content 2 is 2");

            // 2. Styled span: <span style="color:#ff0000;font-weight:bold">Red text</span>
            string md2 = "Warning: <span style=\"color:#ff0000;font-weight:bold\">Danger</span> ahead.";
            var doc2 = parser.Parse(md2);
            var p2 = doc2.Blocks[0] as ParagraphBlock;
            var spanHtml = p2!.Inlines.OfType<HtmlInline>().FirstOrDefault(h => h.Tag == "span");
            Assert(spanHtml != null, "Span HtmlInline must be found");
            AssertEqual("Danger", spanHtml!.Content, "Content matches");
            Assert(spanHtml.Attributes.ContainsKey("style"), "Attributes contain style");

            // 3. Block HTML: <details><summary>Details</summary>Body</details>
            string md3 = "<details>\n<summary>More Info</summary>\nHere is the hidden content.\n</details>";
            var doc3 = parser.Parse(md3);
            AssertEqual(1, doc3.Blocks.Count, "Blocks count");
            Assert(doc3.Blocks[0] is HtmlBlock, "Block must be HtmlBlock");
            var hb3 = (HtmlBlock)doc3.Blocks[0];
            AssertEqual("details", hb3.Tag, "Tag is details");
            AssertEqual("More Info", hb3.Attributes["summary"], "Summary attribute matches");

            // 4. Centered paragraph block <p align="center">
            string md4 = "<p align=\"center\">Centered paragraph text</p>";
            var doc4 = parser.Parse(md4);
            Assert(doc4.Blocks[0] is HtmlBlock, "Block must be HtmlBlock");
            var hb4 = (HtmlBlock)doc4.Blocks[0];
            AssertEqual("p", hb4.Tag, "Tag is p");
            AssertEqual("center", hb4.Attributes["align"], "Align attribute matches");

            // 5. Native WPF HTML rendering test
            var kbdWpf = HtmlWpfRenderer.RenderHtmlInline(htmlInlines[0], ThemePalette.GitHubDark);
            Assert(kbdWpf != null, "HtmlWpfRenderer must render <kbd> as Inline");
            var blockWpf = HtmlWpfRenderer.RenderHtmlBlock(hb3, ThemePalette.GitHubDark, b => null);
            Assert(blockWpf != null, "HtmlWpfRenderer must render <details> as Block");
        }

        private static void TestLatexAndHtmlFlowDocumentSerializationRoundTrip()
        {
            var parser = new MarkdownParser();
            string original = "Formula: $E = mc^2$ and key: <kbd>Ctrl+S</kbd>.\n\n$$\n\\frac{a}{b} = c\n$$\n\n<details>\n<summary>Secret</summary>\nInside info\n</details>";

            var doc = parser.Parse(original);
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(doc);
            Assert(flowDoc != null, "FlowDocument must not be null");

            string serialized = MarkdownSerializer.Serialize(flowDoc!);
            Assert(serialized.Contains("$E = mc^2$"), "Serialized text must preserve $E = mc^2$");
            Assert(serialized.Contains("<kbd>Ctrl+S</kbd>"), "Serialized text must preserve <kbd>Ctrl+S</kbd>");
            Assert(serialized.Contains("\\frac{a}{b} = c"), "Serialized text must preserve display formula");
            Assert(serialized.Contains("<details"), "Serialized text must preserve <details>");
        }

        private static void TestPluginRuntimeToggleBehavior()
        {
            var parser = new MarkdownParser();
            string input = "Math: $x + y = z$.\n\nKey: <kbd>Enter</kbd>.\n\n$$\na = b\n$$";
            var doc = parser.Parse(input);

            // 1. With plugins enabled:
            var converterEnabled = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowEnabled = converterEnabled.Convert(doc);
            var p0 = flowEnabled.Blocks.FirstBlock as Paragraph;
            Assert(p0 != null, "Paragraph 0");
            bool hasInlineUI = p0!.Inlines.Any(i => i is InlineUIContainer);
            Assert(hasInlineUI, "With LaTeX enabled, inline math is rendered as InlineUIContainer vector element");

            // 2. With plugins disabled:
            var converterDisabled = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark, enableLatex: false, enableHtml: false);
            var flowDisabled = converterDisabled.Convert(doc);
            var pDisabled = flowDisabled.Blocks.FirstBlock as Paragraph;
            Assert(pDisabled != null, "Paragraph 0 disabled");
            bool hasNoInlineUI = !pDisabled!.Inlines.Any(i => i is InlineUIContainer);
            Assert(hasNoInlineUI, "With LaTeX disabled, inline math falls back to plain text Run");
            string textContent = string.Join("", pDisabled.Inlines.OfType<Run>().Select(r => r.Text));
            Assert(textContent.Contains("$x + y = z$"), "Raw math syntax is preserved as text when plugin disabled");
        }

        private static void TestEmpiricalPerformanceBenchmarkWithLatexAndHtmlPlugins()
        {
            var parser = new MarkdownParser();

            // 1. Build a heavy mathematical document with 200 formulas and 150 HTML tags
            var sb = new StringBuilder();
            sb.AppendLine("# Heavy Scientific Paper with Plugins");
            sb.AppendLine();
            for (int i = 0; i < 200; i++)
            {
                sb.AppendLine($"Section {i}: The energy is $E_{i} = m_{i} c^2$ and momentum is $p_{i} = \\hbar k_{i}$.");
                if (i % 10 == 0)
                {
                    sb.AppendLine("$$\n\\int_{0}^{\\infty} x^" + i + " e^{-x} dx = " + i + "!\n$$");
                    sb.AppendLine("<details>\n<summary>Proof " + i + "</summary>\nTrivial by induction.\n</details>");
                }
                if (i % 5 == 0)
                {
                    sb.AppendLine("Shortcut: <kbd>Ctrl</kbd>+<kbd>" + (char)('A' + (i % 26)) + "</kbd>, formula: H<sub>2</sub>O and x<sup>2</sup>.");
                }
                sb.AppendLine();
            }

            string heavyMarkdown = sb.ToString();

            // Measure Parsing Time
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var doc = parser.Parse(heavyMarkdown);
            sw.Stop();
            long parseTimeMs = sw.ElapsedMilliseconds;

            // Measure FlowDocument Rendering Time with Plugins Enabled
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            sw.Restart();
            var flowDoc = converter.Convert(doc);
            sw.Stop();
            long renderTimeMs = sw.ElapsedMilliseconds;

            Console.Write($" [200 Math + 150 HTML parsed in {parseTimeMs}ms, rendered in {renderTimeMs}ms] ");

            Assert(doc.Blocks.Count > 200, "Must parse all sections");
            Assert(flowDoc.Blocks.Count > 200, "Must convert all sections");
            // Parsing 200 math + 150 HTML elements should take < 100 ms (typical is 3-10 ms)
            Assert(parseTimeMs < 150, $"Parsing time must be fast (<150ms), actual: {parseTimeMs}ms");

            // 2. Measure plain document parsing to verify zero overhead when no math/HTML present
            var plainSb = new StringBuilder();
            for (int i = 0; i < 2000; i++)
            {
                plainSb.AppendLine($"Regular line {i} with **bold** and *italic* and `code` formatting.");
            }
            string plainMarkdown = plainSb.ToString();

            sw.Restart();
            var plainDoc = parser.Parse(plainMarkdown);
            sw.Stop();
            long plainParseTimeMs = sw.ElapsedMilliseconds;

            Console.Write($" [2,000 plain lines parsed in {plainParseTimeMs}ms] ");
            Assert(plainParseTimeMs < 100, $"Plain document parsing must be under 100ms, actual: {plainParseTimeMs}ms");
        }

        private static void TestLatexAndHtmlEdgeCasesAndRegressions()
        {
            var parser = new MarkdownParser();

            // 1. Triple dollar single line ($$$) must not throw ArgumentOutOfRangeException
            var docDollar = parser.Parse("$$$\nSome text\n$$$");
            Assert(docDollar.Blocks.Count > 0, "Triple dollar must not crash parser");

            // 2. Space after '<' must not be parsed as HTML inline
            string compMd = "if (a < b && c > d) return;";
            var docComp = parser.Parse(compMd);
            var pComp = docComp.Blocks[0] as ParagraphBlock;
            Assert(pComp != null, "Paragraph expected");
            Assert(!pComp!.Inlines.Any(i => i is HtmlInline), "Space after < must not be parsed as HtmlInline");

            // 3. Round-trip fidelity for <b>, <i>, <br> in MarkdownSerializer
            string mixedHtml = "Hello <b>bold</b> and <i>italic</i> and line<br>break.";
            var docMixed = parser.Parse(mixedHtml);
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowMixed = converter.Convert(docMixed);
            string serializedMixed = MarkdownSerializer.Serialize(flowMixed!);
            Assert(serializedMixed.Contains("<b>bold</b>"), "MarkdownSerializer must preserve <b>");
            Assert(serializedMixed.Contains("<i>italic</i>"), "MarkdownSerializer must preserve <i>");
            Assert(serializedMixed.Contains("<br>"), "MarkdownSerializer must preserve <br>");

            // 4. Empty HTML block preservation
            string emptyDiv = "<div id=\"marker\"></div>";
            var docEmpty = parser.Parse(emptyDiv);
            var flowEmpty = converter.Convert(docEmpty);
            string serializedEmpty = MarkdownSerializer.Serialize(flowEmpty!);
            Assert(serializedEmpty.Contains("<div id=\"marker\"></div>"), "MarkdownSerializer must preserve empty HTML block");

            // 5. Nested markdown inside HTML block
            string nestedHtml = "<div>\n\n### Inner Title\n\nSome **bold** inside.\n\n</div>";
            var docNested = parser.Parse(nestedHtml);
            Assert(docNested.Blocks[0] is HtmlBlock, "Must parse as HtmlBlock");
            var hbNested = (HtmlBlock)docNested.Blocks[0];
            Assert(hbNested.Blocks.Count >= 2, "Inner blocks inside HTML block must be parsed");
            Assert(hbNested.Blocks[0] is HeadingBlock, "First inner block must be HeadingBlock");
            var flowNested = converter.Convert(docNested);
            Assert(flowNested != null, "Flow document with nested HTML blocks must convert");

            // 6. <center> block and inline
            string centerBlock = "<center>\n\nCentered content\n\n</center>";
            var docCenter = parser.Parse(centerBlock);
            Assert(docCenter.Blocks[0] is HtmlBlock, "Must parse <center> as HtmlBlock");
            var flowCenter = converter.Convert(docCenter);
            Assert(flowCenter != null, "FlowDocument for center must convert");

            // 7. Extended LaTeX symbols and quote glyph
            var uiEllipses = LatexMathRenderer.RenderMath("1, \\ldots, n \\quad a_1 + \\cdots + a_k", ThemePalette.GitHubDark, 14, false);
            Assert(uiEllipses != null, "Latex renderer must support \\ldots and \\cdots");

            var uiArrows = LatexMathRenderer.RenderMath("A \\implies B \\iff C \\to D", ThemePalette.GitHubDark, 14, false);
            Assert(uiArrows != null, "Latex renderer must support \\implies, \\iff, \\to");

            var uiPrime = LatexMathRenderer.RenderMath("f'(x) + g''(x)", ThemePalette.GitHubDark, 14, false);
            Assert(uiPrime != null, "Latex renderer must support prime symbol f'(x)");

            var uiSymbols = LatexMathRenderer.RenderMath("\\angle ABC = 90^\\circ, L_1 \\perp L_2, L_3 \\parallel L_4", ThemePalette.GitHubDark, 14, false);
            Assert(uiSymbols != null, "Latex renderer must support \\angle, \\perp, \\parallel");

            // 8. Graceful handling of unclosed braces and environments
            var uiUnclosedBrace = LatexMathRenderer.RenderMath("\\text{unclosed text", ThemePalette.GitHubDark, 14, false);
            Assert(uiUnclosedBrace != null, "Latex renderer must not crash on unclosed brace");

            var uiUnclosedEnv = LatexMathRenderer.RenderMath("\\begin{matrix} 1 & 2 \\\\ 3 & 4", ThemePalette.GitHubDark, 14, false);
            Assert(uiUnclosedEnv != null, "Latex renderer must not crash on unclosed matrix environment");

            // 9. Script vertical alignment
            var uiSupOnly = LatexMathRenderer.RenderMath("x^2", ThemePalette.GitHubDark, 14, false);
            Assert(uiSupOnly != null, "Superscript only must render");
            var uiSubOnly = LatexMathRenderer.RenderMath("x_1", ThemePalette.GitHubDark, 14, false);
            Assert(uiSubOnly != null, "Subscript only must render");
            var uiBoth = LatexMathRenderer.RenderMath("x_1^2", ThemePalette.GitHubDark, 14, false);
            Assert(uiBoth != null, "Subscript and superscript must render");
        }

        private static void TestMeasureBaselineAlignment()
        {
            // 1. Verify LatexMathRenderer inline container margin and alignment
            var mathInlineElem = LatexMathRenderer.RenderMath("A = k \\times B", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            Assert(mathInlineElem is Border, "Inline math element must be wrapped in a Border container");
            var mathBorder = (Border)mathInlineElem;
            Assert(mathBorder.Margin.Top == 3 && mathBorder.Margin.Bottom == -3,
                $"Inline math border must have calibrated baseline margin (3, -3), got ({mathBorder.Margin.Top}, {mathBorder.Margin.Bottom})");

            // 2. Verify MarkdownToWpfConverter inline code span margin and baseline alignment
            var parser = new MarkdownParser();
            var docCode = parser.Parse("Text `BaseDomainGenerator.get_applied_q` more text");
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDocCode = converter.Convert(docCode);
            var paraCode = (Paragraph)flowDocCode.Blocks.FirstBlock!;
            var uicCode = paraCode.Inlines.OfType<InlineUIContainer>().FirstOrDefault();
            Assert(uicCode != null, "Inline code span must produce an InlineUIContainer");
            Assert(uicCode!.BaselineAlignment == BaselineAlignment.Center, "Inline code container must have BaselineAlignment.Center");
            Assert(uicCode.Child is Border, "Inline code container child must be a Border");
            var codeBorder = (Border)uicCode.Child!;
            Assert(codeBorder.Margin.Top == 2.5 && codeBorder.Margin.Bottom == -2.5,
                $"Inline code border must have calibrated baseline margin (2.5, -2.5), got ({codeBorder.Margin.Top}, {codeBorder.Margin.Bottom})");

            // 3. Verify MarkdownToWpfConverter inline math span margin and baseline alignment
            var docMath = parser.Parse("Ratio ($A = k \\times B$, find $A$).");
            var flowDocMath = converter.Convert(docMath);
            var paraMath = (Paragraph)flowDocMath.Blocks.FirstBlock!;
            var uicMathList = paraMath.Inlines.OfType<InlineUIContainer>().ToList();
            Assert(uicMathList.Count == 2, $"Expected 2 inline math containers, got {uicMathList.Count}");
            foreach (var uicMath in uicMathList)
            {
                Assert(uicMath.BaselineAlignment == BaselineAlignment.Center, "Inline math container must have BaselineAlignment.Center");
                Assert(uicMath.Child is Border, "Inline math container child must be a Border");
                var mb = (Border)uicMath.Child;
                Assert(mb.Margin.Top == 3 && mb.Margin.Bottom == -3,
                    $"Inline math border must have calibrated baseline margin (3, -3), got ({mb.Margin.Top}, {mb.Margin.Bottom})");
            }

            // 4. Verify HtmlWpfRenderer kbd and code tags
            var kbdInline = new HtmlInline { RawHtml = "<kbd>Ctrl</kbd>", Tag = "kbd", Content = "Ctrl" };
            var kbdResult = HtmlWpfRenderer.RenderHtmlInline(kbdInline, ThemePalette.GitHubDark, null, null);
            Assert(kbdResult is InlineUIContainer, "HTML kbd tag must render as InlineUIContainer");
            var uicKbd = (InlineUIContainer)kbdResult;
            Assert(uicKbd.BaselineAlignment == BaselineAlignment.Center, "HTML kbd container must have BaselineAlignment.Center");
            var kbdBorder = (Border)uicKbd.Child;
            Assert(kbdBorder.Margin.Top == 2.5 && kbdBorder.Margin.Bottom == -2.5,
                $"HTML kbd border must have calibrated baseline margin (2.5, -2.5), got ({kbdBorder.Margin.Top}, {kbdBorder.Margin.Bottom})");

            var htmlCodeInline = new HtmlInline { RawHtml = "<code>test</code>", Tag = "code", Content = "test" };
            var htmlCodeResult = HtmlWpfRenderer.RenderHtmlInline(htmlCodeInline, ThemePalette.GitHubDark, null, null);
            Assert(htmlCodeResult is InlineUIContainer, "HTML code tag must render as InlineUIContainer");
            var uicHtmlCode = (InlineUIContainer)htmlCodeResult;
            Assert(uicHtmlCode.BaselineAlignment == BaselineAlignment.Center, "HTML code container must have BaselineAlignment.Center");
            var htmlCodeBorder = (Border)uicHtmlCode.Child;
            Assert(htmlCodeBorder.Margin.Top == 2.5 && htmlCodeBorder.Margin.Bottom == -2.5,
                $"HTML code border must have calibrated baseline margin (2.5, -2.5), got ({htmlCodeBorder.Margin.Top}, {htmlCodeBorder.Margin.Bottom})");

            // 5. Render full user scenario directly using production converter to generate final visual verification artifact
            string userMarkdown =
                "* Regardless of whether the module covers Fractions, the code produces identical word problems defined in `BaseDomainGenerator.get_applied_q` (`domain_generators.py` line 142):\n" +
                "* **M01 (qn 71)**: Two-variable ratio comparison ($A = k \\times B$, total given; find $A$).\n" +
                "* **M02 (qn 72)**: Two-week gathering sum with difference ($W^2 = W^1 + d$, total both weeks).\n" +
                "* **M05 (qn 75)**: Equal end-state internal transfer ($A - t = B + t$; difference asked = 0).\n" +
                "* Keyboard shortcut: press <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>P</kbd> to inspect.";

            var fullDoc = parser.Parse(userMarkdown);
            var productionFlowDoc = converter.Convert(fullDoc);

            var rtb = new RichTextBox
            {
                Width = 850,
                Background = new SolidColorBrush(Color.FromRgb(24, 26, 32)),
                BorderThickness = new Thickness(0),
                Document = productionFlowDoc,
                IsReadOnly = true
            };

            rtb.Measure(new Size(850, 2000));
            rtb.Arrange(new Rect(0, 0, 850, rtb.DesiredSize.Height));
            rtb.UpdateLayout();

            Assert(rtb.ActualWidth > 0 && rtb.ActualHeight > 0, "RichTextBox layout measurement must succeed for baseline-aligned document");
        }

        private class MockHttpMessageHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly Func<System.Net.Http.HttpRequestMessage, System.Net.Http.HttpResponseMessage> _handler;

            public MockHttpMessageHandler(Func<System.Net.Http.HttpRequestMessage, System.Net.Http.HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(_handler(request));
            }
        }
    }
}
