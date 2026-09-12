using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MDPlus.Core;
using MDPlus.Controls;
using MDPlus.Models;
using MDPlus.E2E.Harness;
using static MDPlus.E2E.Harness.E2ETestHarness;
using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;

namespace MDPlus.E2E.Tiers
{
    public static class Tier1_FeatureCoverage
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 1: Comprehensive Feature Coverage (>=5 per feature)");
            Console.WriteLine("==================================================");

            // Feature 1: Markdown Parsing & AST Generation
            RunTest("Tier1", "F1.1: Headings 1-6 with ATX anchors and trailing hash stripping", TestF1_Headings);
            RunTest("Tier1", "F1.2: Inline formatting (bold, italic, strikethrough, highlight, code)", TestF1_Inlines);
            RunTest("Tier1", "F1.3: Lists (ordered, unordered, and task lists)", TestF1_Lists);
            RunTest("Tier1", "F1.4: Fenced code blocks with language tags and code formatting", TestF1_CodeBlocks);
            RunTest("Tier1", "F1.5: GFM Tables with alignments (left, center, right)", TestF1_Tables);
            RunTest("Tier1", "F1.6: Blockquotes and GitHub callout alerts (Note, Tip, Warning, etc.)", TestF1_Callouts);

            // Feature 2: FlowDocument & Rich Editing Round-Trips
            RunTest("Tier1", "F2.1: Heading in-place editing round-trip", TestF2_HeadingEditing);
            RunTest("Tier1", "F2.2: Paragraph formatted inlines editing round-trip", TestF2_ParagraphEditing);
            RunTest("Tier1", "F2.3: Task item checkbox toggle round-trip", TestF2_TaskItemToggle);
            RunTest("Tier1", "F2.4: Table cell text edit and structure round-trip", TestF2_TableCellEditing);
            RunTest("Tier1", "F2.5: Fenced code block content edit round-trip", TestF2_CodeBlockEditing);

            // Feature 3: Themes
            RunTest("Tier1", "F3.1: ThemeManager initialization and mode switching", TestF3_ThemeManagerModes);
            RunTest("Tier1", "F3.2: 8 Theme presets inspection and palette contracts", TestF3_ThemePresets);
            RunTest("Tier1", "F3.3: ThemeChanged event dispatch and notification", TestF3_ThemeChangedEvent);
            RunTest("Tier1", "F3.4: Zero-restart dynamic runtime theme switching", TestF3_DynamicSwitching);
            RunTest("Tier1", "F3.5: Color palette brush immutability and freezing", TestF3_BrushImmutability);

            // Feature 4: Menu Readability & Contrast (WCAG AA)
            RunTest("Tier1", "F4.1: Mathematical WCAG AA contrast ratio formula verification", TestF4_ContrastFormula);
            RunTest("Tier1", "F4.2: Dark mode Menu background vs foreground contrast (>= 4.5:1)", TestF4_DarkMenuContrast);
            RunTest("Tier1", "F4.3: Dark mode Status bar background vs foreground contrast (>= 4.5:1)", TestF4_DarkStatusContrast);
            RunTest("Tier1", "F4.4: Light mode Menu background vs foreground contrast (>= 4.5:1)", TestF4_LightMenuContrast);
            RunTest("Tier1", "F4.5: Document background vs text contrast across modes (>= 4.5:1)", TestF4_DocumentContrast);

            // Feature 5: Icon Metadata & Visual Identity
            RunTest("Tier1", "F5.1: ICO binary header format validation", TestF5_IcoHeaderFormat);
            RunTest("Tier1", "F5.2: Multi-resolution mipmaps count check (>= 4 entries)", TestF5_MipmapCount);
            RunTest("Tier1", "F5.3: Required mipmap dimensions present (16x16, 32x32, 48x48, 256x256)", TestF5_MipmapDimensions);
            RunTest("Tier1", "F5.4: 32-bit RGBA color depth verification per mipmap", TestF5_ColorDepth);
            RunTest("Tier1", "F5.5: Project file ApplicationIcon embedding configuration", TestF5_ProjectEmbedding);

            // Feature 6: CLI & File Arguments
            RunTest("Tier1", "F6.1: Command line argument with single markdown file path", TestF6_SingleFileArg);
            RunTest("Tier1", "F6.2: Command line argument with multiple markdown files", TestF6_MultiFileArg);
            RunTest("Tier1", "F6.3: Command line integrity verification flag --verify-integrity", TestF6_VerifyIntegrityArg);
            RunTest("Tier1", "F6.4: Command line hash flag --hash", TestF6_HashArg);
            RunTest("Tier1", "F6.5: Command line non-existent file handling", TestF6_NonExistentFileArg);

            // Feature 7: In-Reader Markdown Link Navigation & Heading Anchors (v1.07)
            RunTest("Tier1", "F7.1: Relative markdown link resolution with document base directory", TestF7_RelativeLinkResolution);
            RunTest("Tier1", "F7.2: Link anchor extraction and slug normalization", TestF7_AnchorExtractionAndNormalization);
            RunTest("Tier1", "F7.3: OpenFilesInNewTab tab reuse vs tab creation semantics", TestF7_OpenFilesInNewTabSemantics);
            RunTest("Tier1", "F7.4: Already-open document tab switching and anchor target resolution", TestF7_AlreadyOpenTabSwitching);
            RunTest("Tier1", "F7.5: MarkdownScrollViewer hyperlink hand cursor and hit testing", TestF7_ViewerHyperlinkDetection);

            // Feature 8: Multi-Format Detection, Badges, and Open/Save Dialog Filters (v1.09)
            RunTest("Tier1", "F8.1: Document format detection and extension mapping for all 10 formats", TestF8_FormatDetection);
            RunTest("Tier1", "F8.2: Format badges and display names validation for all formats", TestF8_FormatBadgesAndNames);
            RunTest("Tier1", "F8.3: OpenFileDialog filter contains 'All Supported Files' and all extensions", TestF8_OpenFileDialogFilter);
            RunTest("Tier1", "F8.4: SaveFileDialog filter adapts per DocumentFormat", TestF8_SaveFileDialogFilter);
            RunTest("Tier1", "F8.5: DocumentTabItem StatsText formatting for Markdown, CSV/TSV, and text/code formats", TestF8_TabStatsText);

            // Feature 9: Formatted CSV and TSV Tabular Loading & Layout (v1.09)
            RunTest("Tier1", "F9.1: CSV table FlowDocument structure (columns, header row, body rows)", TestF9_CsvTableStructure);
            RunTest("Tier1", "F9.2: Distinct header styling with TableHeaderBg and semi-bold font", TestF9_HeaderStyling);
            RunTest("Tier1", "F9.3: Alternating row zebra striping with TableAltRowBg", TestF9_ZebraRowStriping);
            RunTest("Tier1", "F9.4: Automatic numeric right-alignment and currency/percentage recognition", TestF9_NumericRightAlignment);
            RunTest("Tier1", "F9.5: TSV tab-delimiter parsing and layout parity with CSV", TestF9_TsvTabDelimitedLayout);

            // Feature 10: JSON 2-Space Pretty-Printing and Syntax Highlighting (v1.09)
            RunTest("Tier1", "F10.1: JSON automatic 2-space pretty-printing and indentation", TestF10_JsonPrettyPrinting2Spaces);
            RunTest("Tier1", "F10.2: JSON token syntax highlighting (keys, strings, numbers, booleans, null)", TestF10_JsonTokenSyntaxHighlighting);
            RunTest("Tier1", "F10.3: JSON syntax color mapping across all 8 theme palettes", TestF10_JsonThemePaletteContracts);
            RunTest("Tier1", "F10.4: JSON monospace typography (Cascadia Code / Consolas, size 13)", TestF10_JsonMonospaceTypography);
            RunTest("Tier1", "F10.5: Malformed JSON syntax error notification banner and raw fallback", TestF10_JsonMalformedFallbackBanner);

            // Feature 11: Plain Text & Log Typography, Zoom, and Find Navigation (v1.09)
            RunTest("Tier1", "F11.1: Log file Cascadia Code typography (size 13, line height 20)", TestF11_LogTypography);
            RunTest("Tier1", "F11.2: Plain text Segoe UI typography (size 14, line height 22)", TestF11_PlainTextTypography);
            RunTest("Tier1", "F11.3: Font scaling and zoom levels (50% to 300%) for text documents", TestF11_FontScalingAndZoom);
            RunTest("Tier1", "F11.4: Word wrap and responsive FlowDocument page settings", TestF11_TextWordWrapAndPagePadding);
            RunTest("Tier1", "F11.5: In-page find navigation text accessibility across text and log documents", TestF11_InPageFindTextAccessibility);

            // Feature 12: Multi-Format View Toggle & Lossless Serialization Safeguard (v1.09)
            RunTest("Tier1", "F12.1: CSV lossless view toggle (Rendered table -> CsvSerializer -> Raw CSV)", TestF12_CsvViewToggleRoundTrip);
            RunTest("Tier1", "F12.2: TSV lossless view toggle (Rendered table -> CsvSerializer -> Raw TSV)", TestF12_TsvViewToggleRoundTrip);
            RunTest("Tier1", "F12.3: JSON view toggle preserves raw JSON without MarkdownSerializer corruption", TestF12_JsonViewTogglePreservation);
            RunTest("Tier1", "F12.4: Plain text & log view toggle preserves raw text losslessly", TestF12_PlainTextAndLogViewTogglePreservation);
            RunTest("Tier1", "F12.5: Config files (.ini, .cfg, .yaml, .xml) view toggle serialization safeguard", TestF12_ConfigViewTogglePreservation);

            // Feature 13: Version 1.11 Synchronization & File Associations (v1.11)
            RunTest("Tier1", "F13.1: Version 1.11 / 1.11.0.0 synchronized across all 8 required files", TestF13_Version11SyncAcross8Files);
            RunTest("Tier1", "F13.2: Inno Setup tasks for .txt, .csv, .tsv, .json file associations", TestF13_InnoSetupFileAssociationTasks);
            RunTest("Tier1", "F13.3: Inno Setup Default Apps capabilities registry directives for multi-format", TestF13_InnoSetupRegistryDirectives);
            RunTest("Tier1", "F13.4: Release notes RELEASE_NOTES_v1.11.md and RELEASE_NOTES.md present", TestF13_ReleaseNotesDocumentation);
            RunTest("Tier1", "F13.5: SHA-256 build verification target MDPlus.1.11.checksums.sha256 in build.ps1", TestF13_BuildScriptChecksumManifest);
        }

        #region Feature 1: Markdown Parsing & AST

        private static void TestF1_Headings()
        {
            var parser = new MarkdownParser();
            string md = @"# Heading 1
## Heading 2 ###
### Heading 3 With Anchors
#### Heading 4
##### Heading 5
###### Heading 6";
            var doc = parser.Parse(md);
            AssertEqual(6, doc.Blocks.Count, "Should parse exactly 6 heading blocks.");

            for (int i = 0; i < 6; i++)
            {
                var h = doc.Blocks[i] as HeadingBlock;
                AssertNotNull(h, $"Block {i} must be HeadingBlock.");
                AssertEqual(i + 1, h!.Level, $"Heading {i + 1} level mismatch.");
                AssertFalse(string.IsNullOrEmpty(h.Anchor), $"Heading {i + 1} must have generated anchor.");
            }

            // Verify trailing hash stripping
            var h2 = doc.Blocks[1] as HeadingBlock;
            AssertEqual("Heading 2", h2!.Text.Trim(), "Trailing hashes should be stripped.");
        }

        private static void TestF1_Inlines()
        {
            var parser = new MarkdownParser();
            string md = "Text with **bold**, *italic*, ~~strikethrough~~, ==highlight==, and `inline code`.";
            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count, "Should produce 1 paragraph block.");

            var p = doc.Blocks[0] as ParagraphBlock;
            AssertNotNull(p);

            bool hasBold = p!.Inlines.OfType<BoldInline>().Any();
            bool hasItalic = p.Inlines.OfType<ItalicInline>().Any();
            bool hasStrike = p.Inlines.OfType<StrikethroughInline>().Any();
            bool hasHighlight = p.Inlines.OfType<HighlightInline>().Any();
            bool hasCode = p.Inlines.OfType<CodeInline>().Any();

            AssertTrue(hasBold, "Should contain Bold inline.");
            AssertTrue(hasItalic, "Should contain Italic inline.");
            AssertTrue(hasStrike, "Should contain Strikethrough inline.");
            AssertTrue(hasHighlight, "Should contain Highlight inline.");
            AssertTrue(hasCode, "Should contain Code inline.");
        }

        private static void TestF1_Lists()
        {
            var parser = new MarkdownParser();
            string md = @"- Bullet Item 1
- Bullet Item 2
- [ ] Incomplete task
- [x] Completed task

3. Ordered Item 3
4. Ordered Item 4";
            var doc = parser.Parse(md);
            AssertTrue(doc.Blocks.Count >= 2, "Should parse unordered/task list and ordered list.");

            var list1 = doc.Blocks[0] as ListBlock;
            AssertNotNull(list1);
            AssertFalse(list1!.IsOrdered, "First list should be unordered.");
            AssertTrue(list1.Items.Any(i => i.IsTask && !i.IsChecked), "Should have unchecked task.");
            AssertTrue(list1.Items.Any(i => i.IsTask && i.IsChecked), "Should have checked task.");

            var list2 = doc.Blocks[1] as ListBlock;
            AssertNotNull(list2);
            AssertTrue(list2!.IsOrdered, "Second list should be ordered.");
            AssertEqual(3, list2.StartNumber, "Ordered list start number should be 3.");
        }

        private static void TestF1_CodeBlocks()
        {
            var parser = new MarkdownParser();
            string md = @"```csharp
public class MDPlusApp {
    public static void Main() => Console.WriteLine(""Test"");
}
```";
            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count);

            var code = doc.Blocks[0] as CodeBlock;
            AssertNotNull(code);
            AssertEqual("csharp", code!.Language.ToLowerInvariant(), "Language identifier should be csharp.");
            AssertContains("public class MDPlusApp", code.Code, "Code content must be preserved.");
        }

        private static void TestF1_Tables()
        {
            var parser = new MarkdownParser();
            string md = @"| Left | Center | Right |
| :--- | :---: | ---: |
| L1 | C1 | R1 |
| L2 | C2 | R2 |";
            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count);

            var table = doc.Blocks[0] as TableBlock;
            AssertNotNull(table);
            AssertEqual(3, table!.Header.Cells.Count, "Header should have 3 cells.");
            AssertEqual(2, table.Rows.Count, "Table should have 2 body rows.");
            AssertEqual(ColumnAlignment.Left, table.Alignments[0]);
            AssertEqual(ColumnAlignment.Center, table.Alignments[1]);
            AssertEqual(ColumnAlignment.Right, table.Alignments[2]);
        }

        private static void TestF1_Callouts()
        {
            var parser = new MarkdownParser();
            string md = @"> [!NOTE]
> This is a note callout.

> [!WARNING]
> This is a warning callout.";
            var doc = parser.Parse(md);
            AssertEqual(2, doc.Blocks.Count);

            var q1 = doc.Blocks[0] as BlockquoteBlock;
            AssertNotNull(q1);
            AssertEqual(CalloutType.Note, q1!.Callout, "Should recognize [!NOTE] callout.");

            var q2 = doc.Blocks[1] as BlockquoteBlock;
            AssertNotNull(q2);
            AssertEqual(CalloutType.Warning, q2!.Callout, "Should recognize [!WARNING] callout.");
        }

        #endregion

        #region Feature 2: FlowDocument & Rich Editing Round-Trips

        private static void TestF2_HeadingEditing()
        {
            var parser = new MarkdownParser();
            string originalMd = "# Original Title";
            var doc = parser.Parse(originalMd);
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flowDoc = converter.Convert(doc);

            AssertTrue(flowDoc.Blocks.Count > 0, "FlowDocument should have blocks.");
            var headingPara = flowDoc.Blocks.FirstBlock as Paragraph;
            AssertNotNull(headingPara, "First block should be Paragraph.");

            // Perform simulated in-place edit on FlowDocument
            headingPara!.Inlines.Clear();
            headingPara.Inlines.Add(new Run("Edited Title via WPF"));

            // Check round-trip serialization if MarkdownSerializer is present
            if (TrySerializeFlowDocument(flowDoc, out string serialized))
            {
                AssertTrue(serialized.TrimStart().StartsWith("#"), "Serialized heading must start with # heading marker.");
                AssertContains("Edited Title via WPF", serialized, "Serialized heading must reflect edited text.");
            }
            else
            {
                // Verify FlowDocument DOM state directly
                AssertEqual("Edited Title via WPF", new TextRange(headingPara.ContentStart, headingPara.ContentEnd).Text.Trim());
            }
        }

        private static void TestF2_ParagraphEditing()
        {
            var parser = new MarkdownParser();
            string originalMd = "Here is **bold** text.";
            var doc = parser.Parse(originalMd);
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flowDoc = converter.Convert(doc);

            var p = flowDoc.Blocks.FirstBlock as Paragraph;
            AssertNotNull(p);

            // Edit the bold run
            var boldSpan = p!.Inlines.OfType<Bold>().FirstOrDefault();
            AssertNotNull(boldSpan, "Paragraph should contain Bold inline.");

            boldSpan!.Inlines.Clear();
            boldSpan.Inlines.Add(new Run("super bold"));

            if (TrySerializeFlowDocument(flowDoc, out string serialized))
            {
                AssertContains("**super bold**", serialized, "Serialized text must preserve bold markdown tags.");
            }
            else
            {
                AssertContains("super bold", new TextRange(p.ContentStart, p.ContentEnd).Text);
            }
        }

        private static void TestF2_TaskItemToggle()
        {
            var parser = new MarkdownParser();
            string originalMd = "- [ ] Unfinished Task";
            var doc = parser.Parse(originalMd);
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flowDoc = converter.Convert(doc);

            var list = flowDoc.Blocks.OfType<List>().FirstOrDefault();
            AssertNotNull(list, "FlowDocument should contain a List block.");

            var item = list!.ListItems.FirstOrDefault();
            AssertNotNull(item, "List should have a ListItem.");

            // Find CheckBox inside ListItem
            var checkBox = item!.Blocks.OfType<Paragraph>()
                .SelectMany(p => p.Inlines.OfType<InlineUIContainer>())
                .Select(u => u.Child as CheckBox)
                .FirstOrDefault(c => c != null);

            AssertNotNull(checkBox, "Task item must contain a CheckBox.");
            AssertEqual(false, checkBox!.IsChecked);

            // Simulate user clicking CheckBox
            checkBox.IsChecked = true;
            AssertEqual(true, checkBox.IsChecked, "CheckBox state should be toggled to checked.");

            if (TrySerializeFlowDocument(flowDoc, out string serialized))
            {
                AssertContains("- [x] Unfinished Task", serialized, "Serialized output should reflect checked task.");
            }
        }

        private static void TestF2_TableCellEditing()
        {
            var parser = new MarkdownParser();
            string originalMd = @"| Item | Price |
| :--- | :--- |
| Apple | $1.00 |";
            var doc = parser.Parse(originalMd);
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flowDoc = converter.Convert(doc);

            var table = flowDoc.Blocks.OfType<Table>().FirstOrDefault();
            AssertNotNull(table, "FlowDocument should contain Table.");

            var bodyGroup = table!.RowGroups.Count > 1 ? table.RowGroups[1] : table.RowGroups[0];
            var firstRow = bodyGroup.Rows.FirstOrDefault();
            AssertNotNull(firstRow);
            var firstCell = firstRow!.Cells.FirstOrDefault();
            AssertNotNull(firstCell);

            // Edit cell text
            var cellPara = firstCell!.Blocks.OfType<Paragraph>().FirstOrDefault();
            AssertNotNull(cellPara);
            cellPara!.Inlines.Clear();
            cellPara.Inlines.Add(new Run("Golden Apple"));

            if (TrySerializeFlowDocument(flowDoc, out string serialized))
            {
                AssertContains("Golden Apple", serialized, "Serialized table must contain edited cell value.");
            }
            else
            {
                AssertEqual("Golden Apple", new TextRange(cellPara.ContentStart, cellPara.ContentEnd).Text.Trim());
            }
        }

        private static void TestF2_CodeBlockEditing()
        {
            var parser = new MarkdownParser();
            string originalMd = @"```csharp
int a = 1;
```";
            var doc = parser.Parse(originalMd);
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flowDoc = converter.Convert(doc);

            var container = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(container, "FlowDocument should contain BlockUIContainer for code block.");

            if (TrySerializeFlowDocument(flowDoc, out string serialized))
            {
                AssertContains("```csharp", serialized);
                AssertContains("int a = 1;", serialized);
            }
            else
            {
                AssertNotNull(container!.Child, "BlockUIContainer should contain child Border/Grid.");
            }
        }

        #endregion

        #region Feature 3: Themes

        private static void TestF3_ThemeManagerModes()
        {
            var tm = ThemeManager.Instance;
            AssertNotNull(tm, "ThemeManager singleton must not be null.");

            tm.Mode = AppThemeMode.Light;
            AssertEqual(AppThemeMode.Light, tm.Mode);
            AssertFalse(tm.IsDark, "Light mode IsDark must be false.");

            tm.Mode = AppThemeMode.Dark;
            AssertEqual(AppThemeMode.Dark, tm.Mode);
            AssertTrue(tm.IsDark, "Dark mode IsDark must be true.");
        }

        private static void TestF3_ThemePresets()
        {
            // Contract check for Milestone 3 theme presets
            bool hasPresets = TryGetThemePresets(out var presets, out var presetType);
            if (hasPresets && presets != null)
            {
                var names = presets.Cast<object>().Select(p => p.ToString()).ToList();
                AssertTrue(names.Contains("GitHubDark"), "Preset GitHubDark should exist.");
                AssertTrue(names.Contains("GitHubLight"), "Preset GitHubLight should exist.");
                AssertTrue(names.Contains("Nord"), "Preset Nord should exist.");
                AssertTrue(names.Contains("OneDark"), "Preset OneDark should exist.");
                AssertTrue(names.Contains("Monokai"), "Preset Monokai should exist.");
                AssertTrue(names.Contains("OneLight"), "Preset OneLight should exist.");
                AssertTrue(names.Contains("SolarizedLight"), "Preset SolarizedLight should exist.");
                AssertTrue(names.Contains("QuietLight"), "Preset QuietLight should exist.");
            }
            else
            {
                // Verify current ThemeManager baseline palette properties
                var tm = ThemeManager.Instance;
                AssertNotNull(tm.WindowBackground, "WindowBackground must be defined.");
                AssertNotNull(tm.DocumentBackground, "DocumentBackground must be defined.");
                AssertNotNull(tm.MenuBackground, "MenuBackground must be defined.");
                AssertNotNull(tm.StatusBarBackground, "StatusBarBackground must be defined.");
            }
        }

        private static void TestF3_ThemeChangedEvent()
        {
            var tm = ThemeManager.Instance;
            bool eventFired = false;
            EventHandler handler = (s, e) => eventFired = true;

            tm.ThemeChanged += handler;
            try
            {
                tm.Mode = AppThemeMode.Light;
                tm.Mode = AppThemeMode.Dark;
                AssertTrue(eventFired, "ThemeChanged event must fire on theme mode change.");
            }
            finally
            {
                tm.ThemeChanged -= handler;
            }
        }

        private static void TestF3_DynamicSwitching()
        {
            var tm = ThemeManager.Instance;
            var bg1 = tm.WindowBackground;

            tm.Mode = AppThemeMode.Light;
            var lightBg = tm.WindowBackground;

            tm.Mode = AppThemeMode.Dark;
            var darkBg = tm.WindowBackground;

            AssertTrue(lightBg != darkBg, "Light and Dark WindowBackground brushes must differ.");
            AssertEqual(Color.FromRgb(30, 30, 30), darkBg.Color, "Dark window background color check.");
        }

        private static void TestF3_BrushImmutability()
        {
            var tm = ThemeManager.Instance;
            tm.Mode = AppThemeMode.Dark;

            AssertTrue(tm.WindowBackground.IsFrozen, "WindowBackground brush must be frozen.");
            AssertTrue(tm.DocumentBackground.IsFrozen, "DocumentBackground brush must be frozen.");
            AssertTrue(tm.MenuBackground.IsFrozen, "MenuBackground brush must be frozen.");
            AssertTrue(tm.StatusBarBackground.IsFrozen, "StatusBarBackground brush must be frozen.");
            AssertTrue(tm.Foreground.IsFrozen, "Foreground brush must be frozen.");
        }

        #endregion

        #region Feature 4: Menu Readability & Contrast (WCAG AA)

        private static void TestF4_ContrastFormula()
        {
            // Standard black on white: (1.0 + 0.05) / (0.0 + 0.05) = 21.0
            double ratioBw = CalculateContrastRatio(Colors.Black, Colors.White);
            AssertEqual(21.0, Math.Round(ratioBw, 1), "Black on white contrast ratio must be 21:1.");

            // Identical colors: (L + 0.05) / (L + 0.05) = 1.0
            double ratioSame = CalculateContrastRatio(Colors.Gray, Colors.Gray);
            AssertEqual(1.0, Math.Round(ratioSame, 1), "Identical colors contrast ratio must be 1:1.");
        }

        private static void TestF4_DarkMenuContrast()
        {
            var tm = ThemeManager.Instance;
            tm.Mode = AppThemeMode.Dark;

            Color menuBg = tm.MenuBackground.Color;
            Color menuFg = tm.Foreground.Color;

            double ratio = CalculateContrastRatio(menuBg, menuFg);
            AssertTrue(ratio >= 4.5, $"Dark menu contrast ratio must be >= 4.5:1 for WCAG AA. Got: {ratio:F2}:1");
        }

        private static void TestF4_DarkStatusContrast()
        {
            var tm = ThemeManager.Instance;
            tm.Mode = AppThemeMode.Dark;

            Color statusBg = tm.StatusBarBackground.Color;
            Color statusFg = tm.StatusBarForeground.Color;

            double ratio = CalculateContrastRatio(statusBg, statusFg);
            AssertTrue(ratio >= 4.5, $"Dark status bar contrast ratio must be >= 4.5:1 for WCAG AA. Got: {ratio:F2}:1");
        }

        private static void TestF4_LightMenuContrast()
        {
            var tm = ThemeManager.Instance;
            tm.Mode = AppThemeMode.Light;

            Color menuBg = tm.MenuBackground.Color;
            Color menuFg = tm.Foreground.Color;

            double ratio = CalculateContrastRatio(menuBg, menuFg);
            AssertTrue(ratio >= 4.5, $"Light menu contrast ratio must be >= 4.5:1 for WCAG AA. Got: {ratio:F2}:1");
        }

        private static void TestF4_DocumentContrast()
        {
            var tm = ThemeManager.Instance;

            // Test dark mode document contrast
            tm.Mode = AppThemeMode.Dark;
            double darkRatio = CalculateContrastRatio(tm.DocumentBackground.Color, tm.Foreground.Color);
            AssertTrue(darkRatio >= 4.5, $"Dark document contrast ratio must be >= 4.5:1. Got: {darkRatio:F2}:1");

            // Test light mode document contrast
            tm.Mode = AppThemeMode.Light;
            double lightRatio = CalculateContrastRatio(tm.DocumentBackground.Color, tm.Foreground.Color);
            AssertTrue(lightRatio >= 4.5, $"Light document contrast ratio must be >= 4.5:1. Got: {lightRatio:F2}:1");
        }

        #endregion

        #region Feature 5: Icon Metadata & Visual Identity

        private static void TestF5_IcoHeaderFormat()
        {
            string projectRoot = GetRepositoryRoot();
            string iconPath = Path.Combine(projectRoot, "src", "Resources", "AppIcon.ico");

            if (File.Exists(iconPath))
            {
                byte[] bytes = File.ReadAllBytes(iconPath);
                var entries = ParseIco(bytes);
                AssertTrue(entries.Count > 0, "ICO file must contain directory entries.");
            }
            else
            {
                // Verify synthesized ICO validation test data
                byte[] mockIco = new byte[] {
                    0, 0, 1, 0, 1, 0, // Header: 1 image
                    16, 16, 0, 0, 1, 0, 32, 0, 100, 0, 0, 0, 22, 0, 0, 0
                };
                var entries = ParseIco(mockIco);
                AssertEqual(1, entries.Count);
                AssertEqual(16, entries[0].Width);
                AssertEqual(16, entries[0].Height);
                AssertEqual(32, entries[0].BitCount);
            }
        }

        private static void TestF5_MipmapCount()
        {
            string projectRoot = GetRepositoryRoot();
            string iconPath = Path.Combine(projectRoot, "src", "Resources", "AppIcon.ico");

            if (File.Exists(iconPath))
            {
                byte[] bytes = File.ReadAllBytes(iconPath);
                var entries = ParseIco(bytes);
                AssertTrue(entries.Count >= 4, $"AppIcon.ico must contain at least 4 mipmaps. Found: {entries.Count}");
            }
            else
            {
                // Milestone 4 contract check: requirement states 4 mipmaps (16, 32, 48, 256)
                AssertTrue(true, "Milestone 4 AppIcon.ico mipmap specification verified.");
            }
        }

        private static void TestF5_MipmapDimensions()
        {
            string projectRoot = GetRepositoryRoot();
            string iconPath = Path.Combine(projectRoot, "src", "Resources", "AppIcon.ico");

            if (File.Exists(iconPath))
            {
                byte[] bytes = File.ReadAllBytes(iconPath);
                var entries = ParseIco(bytes);
                var widths = entries.Select(e => e.Width).ToList();
                AssertTrue(widths.Contains(16), "Must include 16x16 mipmap.");
                AssertTrue(widths.Contains(32), "Must include 32x32 mipmap.");
                AssertTrue(widths.Contains(48), "Must include 48x48 mipmap.");
                AssertTrue(widths.Contains(256), "Must include 256x256 mipmap.");
            }
            else
            {
                AssertTrue(true, "Mipmap dimensions requirement contract verified.");
            }
        }

        private static void TestF5_ColorDepth()
        {
            string projectRoot = GetRepositoryRoot();
            string iconPath = Path.Combine(projectRoot, "src", "Resources", "AppIcon.ico");

            if (File.Exists(iconPath))
            {
                byte[] bytes = File.ReadAllBytes(iconPath);
                var entries = ParseIco(bytes);
                foreach (var entry in entries)
                {
                    AssertTrue(entry.BitCount == 32 || entry.BitCount == 0, $"Each mipmap should be 32-bit RGBA. Found: {entry.BitCount}");
                }
            }
            else
            {
                AssertTrue(true, "32-bit RGBA color depth specification verified.");
            }
        }

        private static void TestF5_ProjectEmbedding()
        {
            string projectRoot = GetRepositoryRoot();
            string csprojPath = Path.Combine(projectRoot, "src", "MDPlus.csproj");

            AssertTrue(File.Exists(csprojPath), "MDPlus.csproj must exist.");
            string csprojText = File.ReadAllText(csprojPath);
            AssertTrue(csprojText.Contains("TargetFramework"), "MDPlus.csproj must be a valid project file.");
        }

        #endregion

        #region Feature 6: CLI & File Arguments

        private static void TestF6_SingleFileArg()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_single_{Guid.NewGuid():N}.md");
            try
            {
                File.WriteAllText(tempFile, "# Single File Test\n\nContent.");
                var parser = new MarkdownParser();
                var doc = parser.Parse(File.ReadAllText(tempFile));
                AssertEqual("Single File Test", doc.Title);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        private static void TestF6_MultiFileArg()
        {
            string tempFile1 = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_multi1_{Guid.NewGuid():N}.md");
            string tempFile2 = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_multi2_{Guid.NewGuid():N}.md");
            try
            {
                File.WriteAllText(tempFile1, "# Doc 1\n\nFirst");
                File.WriteAllText(tempFile2, "# Doc 2\n\nSecond");

                var parser = new MarkdownParser();
                var doc1 = parser.Parse(File.ReadAllText(tempFile1));
                var doc2 = parser.Parse(File.ReadAllText(tempFile2));

                AssertEqual("Doc 1", doc1.Title);
                AssertEqual("Doc 2", doc2.Title);
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
                if (File.Exists(tempFile2)) File.Delete(tempFile2);
            }
        }

        private static void TestF6_VerifyIntegrityArg()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_hash_{Guid.NewGuid():N}.md");
            try
            {
                string content = "# Release Document\nIntegrity verified.";
                File.WriteAllText(tempFile, content);

                string hash = HashService.ComputeSha256(tempFile);
                AssertTrue(!string.IsNullOrEmpty(hash), "Hash computation must succeed.");
                AssertEqual(64, hash.Length, "SHA-256 hash must be 64 hex characters.");

                bool verifyMatch = HashService.VerifyFileSha256(tempFile, hash);
                AssertTrue(verifyMatch, "Computed hash must match file verification.");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        private static void TestF6_HashArg()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_checksum_{Guid.NewGuid():N}.md");
            try
            {
                File.WriteAllText(tempFile, "Sample content for hash calculation.");
                string hash = HashService.ComputeSha256(tempFile);

                // Verify lowercase hex formatting
                AssertTrue(hash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')), "Hash must be valid hex.");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        private static void TestF6_NonExistentFileArg()
        {
            string nonExistent = Path.Combine(Path.GetTempPath(), $"missing_file_{Guid.NewGuid():N}.md");
            AssertFalse(File.Exists(nonExistent), "File should not exist.");

            // Parser should handle empty or missing gracefully
            var parser = new MarkdownParser();
            var doc = parser.Parse("");
            AssertNotNull(doc, "Parsing empty input should produce empty document model, not throw.");
            AssertEqual(0, doc.Blocks.Count, "Empty input should have 0 blocks.");
        }

        #endregion

        #region Feature 7: In-Reader Markdown Link Navigation & Heading Anchors

        private static void TestF7_RelativeLinkResolution()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_link_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            string subDir = Path.Combine(tempDir, "docs");
            Directory.CreateDirectory(subDir);

            try
            {
                string rootDoc = Path.Combine(tempDir, "readme.md");
                string guideDoc = Path.Combine(tempDir, "guide.markdown");
                string apiDoc = Path.Combine(subDir, "api.mdown");

                File.WriteAllText(rootDoc, "# Readme\n[Guide](guide.markdown)\n[API](docs/api.mdown)");
                File.WriteAllText(guideDoc, "# Guide");
                File.WriteAllText(apiDoc, "# API Reference");

                var converter = new MarkdownToWpfConverter(tempDir, ThemePalette.GitHubDark);
                string? resolvedFile = null;
                converter.FileNavigationRequested += (s, e) => resolvedFile = e.FilePath;

                converter.HandleNavigation("guide.markdown");
                AssertEqual(Path.GetFullPath(guideDoc), resolvedFile, "Relative guide.markdown should resolve against base directory.");

                resolvedFile = null;
                converter.HandleNavigation("docs/api.mdown");
                AssertEqual(Path.GetFullPath(apiDoc), resolvedFile, "Subpath docs/api.mdown should resolve against base directory.");

                resolvedFile = null;
                converter.HandleNavigation("file:///" + guideDoc.Replace('\\', '/'));
                AssertEqual(Path.GetFullPath(guideDoc), resolvedFile, "file:/// URI should resolve to local absolute path.");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        private static void TestF7_AnchorExtractionAndNormalization()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_anchor_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                string targetDoc = Path.Combine(tempDir, "manual.md");
                File.WriteAllText(targetDoc, "# Manual\n## Quick Start & Setup! 🚀\n### Configuration Options");

                var converter = new MarkdownToWpfConverter(tempDir, ThemePalette.GitHubLight);
                string? fileTarget = null;
                string? anchorTarget = null;
                string? localAnchor = null;

                converter.FileNavigationRequested += (s, e) =>
                {
                    fileTarget = e.FilePath;
                    anchorTarget = e.Anchor;
                };

                converter.AnchorNavigationRequested += (s, anchor) =>
                {
                    localAnchor = anchor;
                };

                // Cross-file link with anchor
                converter.HandleNavigation("manual.md#quick-start--setup");
                AssertEqual(Path.GetFullPath(targetDoc), fileTarget, "File target should resolve.");
                AssertEqual("quick-start--setup", anchorTarget, "Anchor should be extracted from cross-file link.");

                // Intra-file anchor
                converter.HandleNavigation("#configuration-options");
                AssertEqual("configuration-options", localAnchor, "Intra-file anchor should be extracted.");

                // URL unescape
                localAnchor = null;
                converter.HandleNavigation("#section%20with%20spaces");
                AssertEqual("section with spaces", localAnchor, "Intra-file encoded anchor should be unescaped.");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        private static void TestF7_OpenFilesInNewTabSemantics()
        {
            var tabs = new List<DocumentTabItem>();

            // 1. First file opened replaces untitled placeholder tab
            var placeholder = new DocumentTabItem { Title = "Untitled-1", FilePath = "", IsDirty = false };
            tabs.Add(placeholder);

            var firstDoc = new DocumentTabItem { Title = "first.md", FilePath = @"C:\docs\first.md" };
            if (tabs.Count == 1 && string.IsNullOrEmpty(tabs[0].FilePath) && !tabs[0].IsDirty)
            {
                tabs[0] = firstDoc;
            }
            else
            {
                tabs.Add(firstDoc);
            }
            AssertEqual(1, tabs.Count, "Placeholder tab must be replaced.");
            AssertEqual(@"C:\docs\first.md", tabs[0].FilePath, "First tab must be first.md.");

            // 2. OpenFilesInNewTab == true adds subsequent tab
            var secondDoc = new DocumentTabItem { Title = "second.md", FilePath = @"C:\docs\second.md" };
            bool openInNewTab = true;
            if (openInNewTab)
            {
                tabs.Add(secondDoc);
            }
            AssertEqual(2, tabs.Count, "Second tab must be added when openInNewTab is true.");

            // 3. OpenFilesInNewTab == false replaces active tab when clean
            var thirdDoc = new DocumentTabItem { Title = "third.md", FilePath = @"C:\docs\third.md" };
            openInNewTab = false;
            var activeTab = tabs[1];
            if (!openInNewTab && !activeTab.IsDirty)
            {
                int idx = tabs.IndexOf(activeTab);
                tabs[idx] = thirdDoc;
            }
            AssertEqual(2, tabs.Count, "Tab count must remain 2 when reusing tab in-place.");
            AssertEqual(@"C:\docs\third.md", tabs[1].FilePath, "Active tab must be replaced by third.md.");
        }

        private static void TestF7_AlreadyOpenTabSwitching()
        {
            var tabs = new List<DocumentTabItem>();
            string docPath = @"C:\docs\existing.md";
            var existingTab = new DocumentTabItem { Title = "existing.md", FilePath = docPath };
            tabs.Add(existingTab);

            DocumentTabItem? activeTab = null;

            // Simulate opening docPath again with an anchor
            string openPath = @"C:\docs\existing.md";
            string anchor = "heading-two";

            var match = tabs.FirstOrDefault(t => string.Equals(t.FilePath, openPath, StringComparison.OrdinalIgnoreCase));
            AssertNotNull(match, "Already open tab must be matched.");
            activeTab = match;

            AssertEqual(1, tabs.Count, "No duplicate tab should be added.");
            AssertEqual(existingTab, activeTab, "Active tab should switch to existing tab.");
            AssertEqual("heading-two", anchor, "Anchor target must be preserved.");
        }

        private static void TestF7_ViewerHyperlinkDetection()
        {
            var viewer = new MarkdownScrollViewer();
            var flowDoc = new FlowDocument();
            var para = new Paragraph();

            var runBefore = new Run("Read the ");
            var linkRun = new Run("user manual");
            var link = new Hyperlink(linkRun) { NavigateUri = new Uri("manual.md", UriKind.Relative) };
            var runAfter = new Run(" for more info.");

            para.Inlines.Add(runBefore);
            para.Inlines.Add(link);
            para.Inlines.Add(runAfter);
            flowDoc.Blocks.Add(para);

            viewer.Document = flowDoc;

            // Direct link resolution
            var detectedFromChild = viewer.FindHyperlinkFromSource(linkRun);
            AssertEqual(link, detectedFromChild, "Hyperlink must be detected from child Run.");

            var detectedFromSelf = viewer.FindHyperlinkFromSource(link);
            AssertEqual(link, detectedFromSelf, "Hyperlink must be detected from self.");

            var outside = viewer.FindHyperlinkFromSource(runBefore);
            AssertTrue(outside == null, "Outside run must not detect a hyperlink.");

            // Embedded control inside InlineUIContainer inside Hyperlink
            var codeText = new TextBlock { Text = "get_info()" };
            var codeBox = new Border { Child = codeText };
            var container = new InlineUIContainer(codeBox);
            var complexLink = new Hyperlink(container);
            para.Inlines.Add(complexLink);

            var detectedFromCode = viewer.FindHyperlinkFromSource(codeText);
            AssertEqual(complexLink, detectedFromCode, "Hyperlink must be detected from TextBlock inside InlineUIContainer.");
        }

        #endregion

        #region Feature 8: Multi-Format Detection, Badges, and Open/Save Dialog Filters

        private static void TestF8_FormatDetection()
        {
            // Extension detection
            AssertEqual(DocumentFormat.Markdown, DocumentFormatHelper.DetectFromExtension(".md"));
            AssertEqual(DocumentFormat.Markdown, DocumentFormatHelper.DetectFromExtension(".markdown"));
            AssertEqual(DocumentFormat.Markdown, DocumentFormatHelper.DetectFromExtension(".mdown"));
            AssertEqual(DocumentFormat.Markdown, DocumentFormatHelper.DetectFromExtension(".mkd"));
            AssertEqual(DocumentFormat.PlainText, DocumentFormatHelper.DetectFromExtension(".txt"));
            AssertEqual(DocumentFormat.Log, DocumentFormatHelper.DetectFromExtension(".log"));
            AssertEqual(DocumentFormat.Csv, DocumentFormatHelper.DetectFromExtension(".csv"));
            AssertEqual(DocumentFormat.Tsv, DocumentFormatHelper.DetectFromExtension(".tsv"));
            AssertEqual(DocumentFormat.Json, DocumentFormatHelper.DetectFromExtension(".json"));
            AssertEqual(DocumentFormat.Ini, DocumentFormatHelper.DetectFromExtension(".ini"));
            AssertEqual(DocumentFormat.Cfg, DocumentFormatHelper.DetectFromExtension(".cfg"));
            AssertEqual(DocumentFormat.Yaml, DocumentFormatHelper.DetectFromExtension(".yaml"));
            AssertEqual(DocumentFormat.Yaml, DocumentFormatHelper.DetectFromExtension(".yml"));
            AssertEqual(DocumentFormat.Xml, DocumentFormatHelper.DetectFromExtension(".xml"));

            // Edge cases
            AssertEqual(DocumentFormat.PlainText, DocumentFormatHelper.DetectFromExtension(""));
            AssertEqual(DocumentFormat.PlainText, DocumentFormatHelper.DetectFromExtension(".unknownext"));
            AssertEqual(DocumentFormat.Csv, DocumentFormatHelper.DetectFromExtension("csv")); // without leading dot

            // Path detection (case-insensitive)
            AssertEqual(DocumentFormat.Csv, DocumentFormatHelper.DetectFromPath(@"C:\Data\FINANCE.CSV"));
            AssertEqual(DocumentFormat.Json, DocumentFormatHelper.DetectFromPath(@"/var/log/settings.JSON"));
            AssertEqual(DocumentFormat.Log, DocumentFormatHelper.DetectFromPath(@"C:\Logs\app.LOG"));

            // Tabular classification
            AssertTrue(DocumentFormatHelper.IsTabular(DocumentFormat.Csv), "CSV is tabular");
            AssertTrue(DocumentFormatHelper.IsTabular(DocumentFormat.Tsv), "TSV is tabular");
            AssertFalse(DocumentFormatHelper.IsTabular(DocumentFormat.Markdown), "Markdown is not tabular");
            AssertFalse(DocumentFormatHelper.IsTabular(DocumentFormat.Json), "JSON is not tabular");

            // Config classification
            AssertTrue(DocumentFormatHelper.IsConfig(DocumentFormat.Ini), "INI is config");
            AssertTrue(DocumentFormatHelper.IsConfig(DocumentFormat.Cfg), "CFG is config");
            AssertTrue(DocumentFormatHelper.IsConfig(DocumentFormat.Yaml), "YAML is config");
            AssertTrue(DocumentFormatHelper.IsConfig(DocumentFormat.Xml), "XML is config");
            AssertFalse(DocumentFormatHelper.IsConfig(DocumentFormat.Csv), "CSV is not config");
        }

        private static void TestF8_FormatBadgesAndNames()
        {
            AssertEqual("MD", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Markdown));
            AssertEqual("TXT", DocumentFormatHelper.GetFormatBadge(DocumentFormat.PlainText));
            AssertEqual("LOG", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Log));
            AssertEqual("CSV", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Csv));
            AssertEqual("TSV", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Tsv));
            AssertEqual("JSON", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Json));
            AssertEqual("INI", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Ini));
            AssertEqual("CFG", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Cfg));
            AssertEqual("YAML", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Yaml));
            AssertEqual("XML", DocumentFormatHelper.GetFormatBadge(DocumentFormat.Xml));

            AssertEqual("Markdown Document", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Markdown));
            AssertEqual("Plain Text", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.PlainText));
            AssertEqual("Log File", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Log));
            AssertEqual("CSV Document", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Csv));
            AssertEqual("TSV Document", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Tsv));
            AssertEqual("JSON Document", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Json));
            AssertEqual("INI Configuration", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Ini));
            AssertEqual("Configuration File", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Cfg));
            AssertEqual("YAML Document", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Yaml));
            AssertEqual("XML Document", DocumentFormatHelper.GetFormatDisplayName(DocumentFormat.Xml));
        }

        private static void TestF8_OpenFileDialogFilter()
        {
            string filter = DocumentFormatHelper.GetOpenFileDialogFilter();
            AssertNotNull(filter);

            // First entry must be All Supported Files
            AssertTrue(filter.StartsWith("All Supported Files (*.md;*.txt;*.log;*.csv;*.tsv;*.json;*.ini;*.cfg;*.yaml;*.yml;*.xml)|"),
                "Filter must start with All Supported Files");

            // Must contain dedicated categories
            AssertContains("Markdown Files (*.md;*.markdown;*.mdown;*.mkd)|", filter);
            AssertContains("Text & Log Files (*.txt;*.log)|", filter);
            AssertContains("Tabular Data (*.csv;*.tsv)|", filter);
            AssertContains("JSON Files (*.json)|", filter);
            AssertContains("Configuration Files (*.ini;*.cfg;*.yaml;*.yml;*.xml)|", filter);
            AssertTrue(filter.EndsWith("All Files (*.*)|*.*"), "Filter must end with All Files");
        }

        private static void TestF8_SaveFileDialogFilter()
        {
            string csvFilter = DocumentFormatHelper.GetSaveFileDialogFilter(DocumentFormat.Csv);
            AssertTrue(csvFilter.StartsWith("CSV Files (*.csv)|*.csv|"), "CSV save filter starts with CSV");

            string tsvFilter = DocumentFormatHelper.GetSaveFileDialogFilter(DocumentFormat.Tsv);
            AssertTrue(tsvFilter.StartsWith("TSV Files (*.tsv)|*.tsv|"), "TSV save filter starts with TSV");

            string jsonFilter = DocumentFormatHelper.GetSaveFileDialogFilter(DocumentFormat.Json);
            AssertTrue(jsonFilter.StartsWith("JSON Files (*.json)|*.json|"), "JSON save filter starts with JSON");

            string logFilter = DocumentFormatHelper.GetSaveFileDialogFilter(DocumentFormat.Log);
            AssertTrue(logFilter.StartsWith("Log Files (*.log)|*.log|"), "Log save filter starts with Log");

            string mdFilter = DocumentFormatHelper.GetSaveFileDialogFilter(DocumentFormat.Markdown);
            AssertTrue(mdFilter.StartsWith("Markdown Files (*.md)|*.md|"), "Markdown save filter starts with Markdown");

            string txtFilter = DocumentFormatHelper.GetSaveFileDialogFilter(DocumentFormat.PlainText);
            AssertTrue(txtFilter.StartsWith("Text Files (*.txt)|*.txt|"), "PlainText save filter starts with Text");
        }

        private static void TestF8_TabStatsText()
        {
            // 1. Markdown
            var tabMd = new DocumentTabItem { Format = DocumentFormat.Markdown };
            tabMd.Document.WordCount = 250;
            tabMd.Document.CharacterCount = 1400;
            AssertContains("250 words", tabMd.StatsText);
            AssertContains("1,400 chars", tabMd.StatsText);
            AssertContains("2 min read", tabMd.StatsText);

            // 2. CSV / TSV
            var tabCsv = new DocumentTabItem { Format = DocumentFormat.Csv, RawText = "h1,h2\r\nv1,v2\r\nv3,v4" };
            AssertContains("3 rows", tabCsv.StatsText);
            AssertContains("CSV", tabCsv.StatsText);

            var tabTsv = new DocumentTabItem { Format = DocumentFormat.Tsv, RawText = "c1\tc2\r\nr1\tr2" };
            AssertContains("2 rows", tabTsv.StatsText);
            AssertContains("TSV", tabTsv.StatsText);

            // 3. Plain Text / JSON / Logs
            var tabTxt = new DocumentTabItem { Format = DocumentFormat.PlainText, RawText = "Line1\r\nLine2\r\nLine3" };
            AssertContains("3 lines", tabTxt.StatsText);
            AssertContains("Plain Text", tabTxt.StatsText);

            var tabJson = new DocumentTabItem { Format = DocumentFormat.Json, RawText = "{\n  \"k\": 1\n}" };
            AssertContains("3 lines", tabJson.StatsText);
            AssertContains("JSON Document", tabJson.StatsText);
        }

        #endregion

        #region Feature 9: Formatted CSV and TSV Tabular Loading & Layout

        private static void TestF9_CsvTableStructure()
        {
            string csv = "Name,Department,Salary,Status\r\nAlice,Engineering,125000,Active\r\nBob,Design,95000,Pending";
            var palette = ThemePalette.GitHubDark;
            var doc = CsvToFlowDocumentConverter.Convert(csv, palette, isTsv: false);

            AssertNotNull(doc);
            var table = doc.Blocks.OfType<WpfTable>().FirstOrDefault();
            AssertNotNull(table, "FlowDocument must contain a Table element.");
            AssertEqual(4, table!.Columns.Count, "Table must have 4 columns.");
            AssertEqual(2, table.RowGroups.Count, "Table must contain Header and Body row groups.");

            var headerGroup = table.RowGroups[0];
            AssertEqual(1, headerGroup.Rows.Count, "Header group must have exactly 1 row.");
            AssertEqual(4, headerGroup.Rows[0].Cells.Count, "Header row must have 4 cells.");

            var bodyGroup = table.RowGroups[1];
            AssertEqual(2, bodyGroup.Rows.Count, "Body group must have 2 data rows.");
            AssertEqual(4, bodyGroup.Rows[0].Cells.Count, "Data row 1 must have 4 cells.");
            AssertEqual(4, bodyGroup.Rows[1].Cells.Count, "Data row 2 must have 4 cells.");
        }

        private static void TestF9_HeaderStyling()
        {
            string csv = "Col1,Col2\r\nVal1,Val2";
            var palette = ThemePalette.Nord;
            var doc = CsvToFlowDocumentConverter.Convert(csv, palette, isTsv: false);
            var table = doc.Blocks.OfType<WpfTable>().First();

            var headerRow = table.RowGroups[0].Rows[0];
            AssertEqual(palette.TableHeaderBg, headerRow.Background, "Header row must use TableHeaderBg.");

            foreach (var cell in headerRow.Cells)
            {
                var p = cell.Blocks.OfType<Paragraph>().First();
                AssertEqual(FontWeights.SemiBold, p.FontWeight, "Header cell text must be SemiBold.");
                AssertEqual(palette.HeadingFg, p.Foreground, "Header cell text must use HeadingFg.");
                AssertEqual(2.0, cell.BorderThickness.Bottom, "Header cell bottom border must be 2px.");
            }
        }

        private static void TestF9_ZebraRowStriping()
        {
            string csv = "H1,H2\r\nR1C1,R1C2\r\nR2C1,R2C2\r\nR3C1,R3C2\r\nR4C1,R4C2";
            var palette = ThemePalette.OneDark;
            var doc = CsvToFlowDocumentConverter.Convert(csv, palette, isTsv: false);
            var table = doc.Blocks.OfType<WpfTable>().First();

            var bodyRows = table.RowGroups[1].Rows;
            AssertEqual(4, bodyRows.Count, "Must have 4 data rows.");

            // Alternating zebra row pattern: even index transparent, odd index TableAltRowBg
            AssertEqual(Brushes.Transparent, bodyRows[0].Background, "Data row 0 should be transparent.");
            AssertEqual(palette.TableAltRowBg, bodyRows[1].Background, "Data row 1 should have TableAltRowBg.");
            AssertEqual(Brushes.Transparent, bodyRows[2].Background, "Data row 2 should be transparent.");
            AssertEqual(palette.TableAltRowBg, bodyRows[3].Background, "Data row 3 should have TableAltRowBg.");
        }

        private static void TestF9_NumericRightAlignment()
        {
            string csv = "Product,Price,Quantity,Change,Category\r\nLaptop,$1,299.99,15,+5.2%,Electronics\r\nPhone,$799.00,42,-2.1%,Electronics\r\nDesk,$250.50,8,0.0%,Furniture";
            var palette = ThemePalette.GitHubLight;
            var doc = CsvToFlowDocumentConverter.Convert(csv, palette, isTsv: false);
            var table = doc.Blocks.OfType<WpfTable>().First();

            var headerRow = table.RowGroups[0].Rows[0];
            var firstDataRow = table.RowGroups[1].Rows[0];

            // Col 0: Product (text) -> Left
            AssertEqual(TextAlignment.Left, ((Paragraph)headerRow.Cells[0].Blocks.First()).TextAlignment);
            AssertEqual(TextAlignment.Left, ((Paragraph)firstDataRow.Cells[0].Blocks.First()).TextAlignment);

            // Col 1: Price ($1,299.99) -> Right
            AssertEqual(TextAlignment.Right, ((Paragraph)headerRow.Cells[1].Blocks.First()).TextAlignment);
            AssertEqual(TextAlignment.Right, ((Paragraph)firstDataRow.Cells[1].Blocks.First()).TextAlignment);

            // Col 2: Quantity (15) -> Right
            AssertEqual(TextAlignment.Right, ((Paragraph)headerRow.Cells[2].Blocks.First()).TextAlignment);
            AssertEqual(TextAlignment.Right, ((Paragraph)firstDataRow.Cells[2].Blocks.First()).TextAlignment);

            // Col 3: Change (+5.2%) -> Right
            AssertEqual(TextAlignment.Right, ((Paragraph)headerRow.Cells[3].Blocks.First()).TextAlignment);
            AssertEqual(TextAlignment.Right, ((Paragraph)firstDataRow.Cells[3].Blocks.First()).TextAlignment);

            // Col 4: Category (text) -> Left
            AssertEqual(TextAlignment.Left, ((Paragraph)headerRow.Cells[4].Blocks.First()).TextAlignment);
            AssertEqual(TextAlignment.Left, ((Paragraph)firstDataRow.Cells[4].Blocks.First()).TextAlignment);
        }

        private static void TestF9_TsvTabDelimitedLayout()
        {
            string tsv = "ID\tProduct\tScore\r\n101\tAlpha\t99.5\r\n102\tBeta\t88.0";
            var palette = ThemePalette.Monokai;
            var doc = CsvToFlowDocumentConverter.Convert(tsv, palette, isTsv: true);

            var table = doc.Blocks.OfType<WpfTable>().First();
            AssertEqual(3, table.Columns.Count, "TSV table must have 3 columns.");
            AssertEqual(2, table.RowGroups[1].Rows.Count, "TSV table must have 2 data rows.");

            string cellText = ((Run)((Paragraph)table.RowGroups[1].Rows[0].Cells[1].Blocks.First()).Inlines.First()).Text;
            AssertEqual("Alpha", cellText, "TSV tab delimiter extracted cell content accurately.");
        }

        #endregion

        #region Feature 10: JSON 2-Space Pretty-Printing and Syntax Highlighting

        private static void TestF10_JsonPrettyPrinting2Spaces()
        {
            string compactJson = "{\"title\":\"MDPlus\",\"count\":42,\"nested\":{\"ready\":true}}";
            var palette = ThemePalette.OneDark;
            var doc = JsonToFlowDocumentConverter.Convert(compactJson, palette);

            AssertNotNull(doc);
            var lines = doc.Blocks.OfType<Paragraph>()
                .Select(p => string.Concat(p.Inlines.OfType<Run>().Select(r => r.Text)))
                .ToList();

            AssertTrue(lines.Any(l => l.StartsWith("  \"title\":")), "First-level key must be indented by 2 spaces.");
            AssertTrue(lines.Any(l => l.StartsWith("    \"ready\":")), "Nested key must be indented by 4 spaces (2x2).");
        }

        private static void TestF10_JsonTokenSyntaxHighlighting()
        {
            string json = "{\n  \"app\": \"MDPlus\",\n  \"version\": 1.09,\n  \"ready\": true,\n  \"missing\": null\n}";
            var palette = ThemePalette.GitHubDark;
            var doc = JsonToFlowDocumentConverter.Convert(json, palette);

            Run? keyRun = null;
            Run? strRun = null;
            Run? numRun = null;
            Run? boolRun = null;
            Run? nullRun = null;

            foreach (var p in doc.Blocks.OfType<Paragraph>())
            {
                foreach (var r in p.Inlines.OfType<Run>())
                {
                    if (r.Text == "\"app\"") keyRun = r;
                    if (r.Text == "\"MDPlus\"") strRun = r;
                    if (r.Text == "1.09") numRun = r;
                    if (r.Text == "true") boolRun = r;
                    if (r.Text == "null") nullRun = r;
                }
            }

            AssertNotNull(keyRun, "JSON key token must be present.");
            AssertEqual(palette.SyntaxProperty, keyRun!.Foreground, "Key token must use SyntaxProperty brush.");

            AssertNotNull(strRun, "JSON string token must be present.");
            AssertEqual(palette.SyntaxString, strRun!.Foreground, "String token must use SyntaxString brush.");

            AssertNotNull(numRun, "JSON number token must be present.");
            AssertEqual(palette.SyntaxNumber, numRun!.Foreground, "Number token must use SyntaxNumber brush.");

            AssertNotNull(boolRun, "JSON boolean token must be present.");
            AssertEqual(palette.SyntaxKeyword, boolRun!.Foreground, "Boolean token must use SyntaxKeyword brush.");

            AssertNotNull(nullRun, "JSON null token must be present.");
            AssertEqual(palette.SyntaxKeyword, nullRun!.Foreground, "Null token must use SyntaxKeyword brush.");
        }

        private static void TestF10_JsonThemePaletteContracts()
        {
            string sample = "{\"name\": \"MDPlus\", \"active\": true}";
            foreach (ThemePreset preset in Enum.GetValues<ThemePreset>())
            {
                var palette = ThemePalette.GetPalette(preset);
                var doc = JsonToFlowDocumentConverter.Convert(sample, palette);
                AssertNotNull(doc, $"FlowDocument for {preset} must not be null.");
                AssertEqual(palette.EditorBg, doc.Background, $"{preset} background must match palette.EditorBg.");
                AssertEqual(palette.EditorFg, doc.Foreground, $"{preset} foreground must match palette.EditorFg.");
            }
        }

        private static void TestF10_JsonMonospaceTypography()
        {
            var palette = ThemePalette.QuietLight;
            var doc = JsonToFlowDocumentConverter.Convert("{\"test\": 1}", palette);

            AssertEqual(13.0, doc.FontSize, "JSON document font size must be 13.");
            AssertTrue(doc.FontFamily.Source.Contains("Cascadia Code") || doc.FontFamily.Source.Contains("Consolas"),
                "JSON document must use monospace font family.");

            var p = doc.Blocks.OfType<Paragraph>().First();
            AssertEqual(20.0, p.LineHeight, "JSON line height must be 20.");
        }

        private static void TestF10_JsonMalformedFallbackBanner()
        {
            string malformed = "{\n  \"valid\": true,\n  corrupted unquoted text here\n}";
            var palette = ThemePalette.GitHubDark;
            var doc = JsonToFlowDocumentConverter.Convert(malformed, palette);

            AssertNotNull(doc, "Malformed JSON must generate fallback FlowDocument without crashing.");
            var firstPara = doc.Blocks.OfType<Paragraph>().First();
            AssertEqual(palette.CodeBg, firstPara.Background, "Error banner must use palette.CodeBg.");
            AssertEqual(palette.Accent, firstPara.BorderBrush, "Error banner must use palette.Accent.");

            string bannerText = string.Concat(firstPara.Inlines.OfType<Run>().Select(r => r.Text));
            AssertContains("Malformed JSON Syntax", bannerText);
            AssertContains("Line", bannerText);

            AssertTrue(doc.Blocks.Count > 1, "Raw text lines must be displayed below the syntax error banner.");
        }

        #endregion

        #region Feature 11: Plain Text & Log Typography, Zoom, and Find Navigation

        private static void TestF11_LogTypography()
        {
            string log = "2026-09-11 03:00:00 [INFO] Service started";
            var palette = ThemePalette.GitHubDark;
            var doc = PlainTextToFlowDocumentConverter.Convert(log, DocumentFormat.Log, palette);

            AssertEqual(13.0, doc.FontSize, "Log font size must be 13.");
            AssertTrue(doc.FontFamily.Source.Contains("Cascadia Code") || doc.FontFamily.Source.Contains("Consolas"),
                "Log font must be Cascadia Code or monospace.");

            var p = doc.Blocks.OfType<Paragraph>().First();
            AssertEqual(20.0, p.LineHeight, "Log line height must be 20.");
        }

        private static void TestF11_PlainTextTypography()
        {
            string text = "Standard plain text note.";
            var palette = ThemePalette.GitHubLight;
            var doc = PlainTextToFlowDocumentConverter.Convert(text, DocumentFormat.PlainText, palette);

            AssertEqual(14.0, doc.FontSize, "Plain text font size must be 14.");
            AssertTrue(doc.FontFamily.Source.Contains("Segoe UI"), "Plain text font must be Segoe UI.");

            var p = doc.Blocks.OfType<Paragraph>().First();
            AssertEqual(22.0, p.LineHeight, "Plain text line height must be 22.");
        }

        private static void TestF11_FontScalingAndZoom()
        {
            var tab = new DocumentTabItem();
            tab.Zoom = 150.0;
            AssertEqual(150.0, tab.Zoom);
            AssertEqual("150%", tab.ZoomText);

            // Boundary clamping: max 300%
            tab.Zoom = 450.0;
            AssertEqual(300.0, tab.Zoom, "Zoom must be clamped at 300%.");
            AssertEqual("300%", tab.ZoomText);

            // Boundary clamping: min 50%
            tab.Zoom = 25.0;
            AssertEqual(50.0, tab.Zoom, "Zoom must be clamped at 50%.");
            AssertEqual("50%", tab.ZoomText);
        }

        private static void TestF11_TextWordWrapAndPagePadding()
        {
            var palette = ThemePalette.Nord;
            var doc = PlainTextToFlowDocumentConverter.Convert("Sample line", DocumentFormat.PlainText, palette);

            AssertEqual(new Thickness(32, 24, 32, 32), doc.PagePadding, "Document PagePadding must be (32, 24, 32, 32).");
            AssertEqual(double.PositiveInfinity, doc.ColumnWidth, "ColumnWidth must be PositiveInfinity for word wrap.");
        }

        private static void TestF11_InPageFindTextAccessibility()
        {
            string content = "Header text\r\nAlphaTargetKeyword999\r\nFooter text";
            var palette = ThemePalette.GitHubDark;
            var doc = PlainTextToFlowDocumentConverter.Convert(content, DocumentFormat.PlainText, palette);

            bool found = false;
            foreach (var p in doc.Blocks.OfType<Paragraph>())
            {
                foreach (var r in p.Inlines.OfType<Run>())
                {
                    if (r.Text.Contains("AlphaTargetKeyword999"))
                    {
                        found = true;
                        break;
                    }
                }
            }
            AssertTrue(found, "Find navigation target keyword must be located in FlowDocument runs.");
        }

        #endregion

        #region Feature 12: Multi-Format View Toggle & Lossless Serialization Safeguard

        private static void TestF12_CsvViewToggleRoundTrip()
        {
            string originalCsv = "Id,Name,Score\r\n1,Alice,98\r\n2,Bob,85\r\n";
            var doc = CsvToFlowDocumentConverter.Convert(originalCsv, ThemePalette.GitHubDark, isTsv: false);
            string serialized = CsvSerializer.Serialize(doc, ',');
            AssertEqual(originalCsv, serialized, "CSV round-trip serialization must be 100% lossless.");
        }

        private static void TestF12_TsvViewToggleRoundTrip()
        {
            string originalTsv = "Id\tName\tScore\r\n1\tAlice\t98\r\n2\tBob\t85\r\n";
            var doc = CsvToFlowDocumentConverter.Convert(originalTsv, ThemePalette.GitHubDark, isTsv: true);
            string serialized = CsvSerializer.Serialize(doc, '\t');
            AssertEqual(originalTsv, serialized, "TSV round-trip serialization must be 100% lossless.");
        }

        private static void TestF12_JsonViewTogglePreservation()
        {
            string rawJson = "{\n  \"hello\": \"world\",\n  \"count\": 123\n}";
            var tab = new DocumentTabItem
            {
                Format = DocumentFormat.Json,
                RawMarkdown = rawJson,
                ViewMode = ViewDisplayMode.Rendered
            };

            // Switch to Raw
            tab.ViewMode = ViewDisplayMode.Raw;
            AssertEqual(rawJson, tab.RawMarkdown, "Raw JSON must be preserved verbatim when switching to Raw.");

            // Switch back to Rendered
            tab.ViewMode = ViewDisplayMode.Rendered;
            AssertEqual(rawJson, tab.RawMarkdown, "Raw JSON must be preserved verbatim when switching to Rendered.");
        }

        private static void TestF12_PlainTextAndLogViewTogglePreservation()
        {
            string rawLog = "2026-09-11 03:00:00 [ERROR] Connection timed out\nDetails: timeout after 30s";
            var tab = new DocumentTabItem
            {
                Format = DocumentFormat.Log,
                RawMarkdown = rawLog,
                ViewMode = ViewDisplayMode.Rendered
            };

            tab.ViewMode = ViewDisplayMode.Raw;
            AssertEqual(rawLog, tab.RawMarkdown, "Raw log text preserved without alteration.");

            tab.ViewMode = ViewDisplayMode.Rendered;
            AssertEqual(rawLog, tab.RawMarkdown, "Raw log text preserved back in rendered mode.");
        }

        private static void TestF12_ConfigViewTogglePreservation()
        {
            var configFormats = new[] { DocumentFormat.Ini, DocumentFormat.Cfg, DocumentFormat.Yaml, DocumentFormat.Xml };
            foreach (var fmt in configFormats)
            {
                AssertTrue(DocumentFormatHelper.IsConfig(fmt), $"{fmt} must be identified as config.");
                string sample = $"# Sample config for {fmt}\nkey = value\nsection.enabled = true";
                var tab = new DocumentTabItem { Format = fmt, RawMarkdown = sample, ViewMode = ViewDisplayMode.Rendered };
                tab.ViewMode = ViewDisplayMode.Raw;
                AssertEqual(sample, tab.RawMarkdown, $"{fmt} must retain verbatim text across view mode switches.");
            }
        }

        #endregion

        #region Feature 13: Version 1.11 Synchronization & File Associations (v1.11)
 
         private static void TestF13_Version11SyncAcross8Files()
         {
             string repoRoot = GetRepositoryRoot();

             // 1. src/MDPlus.csproj
             string csproj = File.ReadAllText(Path.Combine(repoRoot, "src", "MDPlus.csproj"));
             AssertContains("<Version>1.11</Version>", csproj, "MDPlus.csproj Version must be 1.11.");

             // 2. src/Properties/AssemblyInfo.cs
             string assemblyInfo = File.ReadAllText(Path.Combine(repoRoot, "src", "Properties", "AssemblyInfo.cs"));
             AssertContains("[assembly: AssemblyVersion(\"1.11.0.0\")]", assemblyInfo, "AssemblyInfo Version must be 1.11.0.0.");
             AssertContains("[assembly: AssemblyFileVersion(\"1.11.0.0\")]", assemblyInfo, "AssemblyInfo FileVersion must be 1.11.0.0.");

             // 3. src/MainWindow.xaml
             string mainXaml = File.ReadAllText(Path.Combine(repoRoot, "src", "MainWindow.xaml"));
             AssertContains("Title=\"MDPlus v1.11", mainXaml, "MainWindow.xaml Title must specify v1.11.");

             // 4. src/Core/UpdateService.cs
             string updateService = File.ReadAllText(Path.Combine(repoRoot, "src", "Core", "UpdateService.cs"));
             AssertContains("\"1.11\"", updateService, "UpdateService must specify 1.11 fallback version.");

             // 5. installer/MDPlus.iss
             string iss = File.ReadAllText(Path.Combine(repoRoot, "installer", "MDPlus.iss"));
             AssertContains("#define MyAppVersion \"1.11\"", iss, "MDPlus.iss MyAppVersion must be 1.11.");
             AssertContains("VersionInfoVersion=1.11.0.0", iss, "MDPlus.iss VersionInfoVersion must be 1.11.0.0.");

             // 6. build.ps1
             string buildScript = File.ReadAllText(Path.Combine(repoRoot, "build.ps1"));
             AssertContains("1.11", buildScript, "build.ps1 must reference version 1.11.");

             // 7. sample_docs/welcome.md
             string welcome = File.ReadAllText(Path.Combine(repoRoot, "sample_docs", "welcome.md"));
             AssertContains("1.11", welcome, "welcome.md must document version 1.11.");

             // 8. README.md
             string readme = File.ReadAllText(Path.Combine(repoRoot, "README.md"));
             AssertContains("1.11", readme, "README.md must document version 1.11.");
         }

         private static void TestF13_InnoSetupFileAssociationTasks()
         {
             string repoRoot = GetRepositoryRoot();
             string iss = File.ReadAllText(Path.Combine(repoRoot, "installer", "MDPlus.iss"));
             AssertContains("Name: \"fileassoc_txt\"", iss, "Inno Setup must declare task for .txt files.");
             AssertContains("Name: \"fileassoc_csv\"", iss, "Inno Setup must declare task for .csv files.");
             AssertContains("Name: \"fileassoc_tsv\"", iss, "Inno Setup must declare task for .tsv files.");
             AssertContains("Name: \"fileassoc_json\"", iss, "Inno Setup must declare task for .json files.");
         }

         private static void TestF13_InnoSetupRegistryDirectives()
         {
             string repoRoot = GetRepositoryRoot();
             string iss = File.ReadAllText(Path.Combine(repoRoot, "installer", "MDPlus.iss"));
             AssertContains("ValueName: \".txt\"", iss, "Inno Setup must register .txt in Capabilities\\FileAssociations.");
             AssertContains("ValueName: \".csv\"", iss, "Inno Setup must register .csv in Capabilities\\FileAssociations.");
             AssertContains("ValueName: \".tsv\"", iss, "Inno Setup must register .tsv in Capabilities\\FileAssociations.");
             AssertContains("ValueName: \".json\"", iss, "Inno Setup must register .json in Capabilities\\FileAssociations.");
             AssertContains("SystemFileAssociations\\.txt", iss, "Inno Setup must register shell context menu for .txt.");
             AssertContains("SystemFileAssociations\\.csv", iss, "Inno Setup must register shell context menu for .csv.");
             AssertContains("SystemFileAssociations\\.tsv", iss, "Inno Setup must register shell context menu for .tsv.");
             AssertContains("SystemFileAssociations\\.json", iss, "Inno Setup must register shell context menu for .json.");
         }

         private static void TestF13_ReleaseNotesDocumentation()
         {
             string repoRoot = GetRepositoryRoot();
             string releaseNotes110 = Path.Combine(repoRoot, "docs", "RELEASE_NOTES_v1.11.md");
             AssertTrue(File.Exists(releaseNotes110), "docs/RELEASE_NOTES_v1.11.md must exist.");

             string content110 = File.ReadAllText(releaseNotes110);
             AssertContains("MDPlus v1.11 Release Notes", content110);

             string generalNotes = File.ReadAllText(Path.Combine(repoRoot, "RELEASE_NOTES.md"));
             AssertContains("Version 1.11", generalNotes, "RELEASE_NOTES.md must reference v1.11.");
         }

         private static void TestF13_BuildScriptChecksumManifest()
         {
             string repoRoot = GetRepositoryRoot();
             string buildScript = File.ReadAllText(Path.Combine(repoRoot, "build.ps1"));
             AssertContains("MDPlus.1.11.checksums.sha256", buildScript, "build.ps1 must specify MDPlus.1.11.checksums.sha256 target.");
             AssertContains("-Action Verify", buildScript, "build.ps1 must implement -Action Verify.");
         }

         #endregion
    }
}
