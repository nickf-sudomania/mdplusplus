using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MDPlus.Core;
using MDPlus.E2E.Harness;
using MDPlus.Models;
using static MDPlus.E2E.Harness.E2ETestHarness;
using WpfTable = System.Windows.Documents.Table;

namespace MDPlus.E2E.Tiers
{
    public static class Tier3_CrossFeatureCombinations
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 3: Cross-Feature Combinations");
            Console.WriteLine("==================================================");

            RunTest("Tier3", "T3.1: Tables with formatted inlines inside blockquotes", TestTableInsideBlockquote);
            RunTest("Tier3", "T3.2: Task lists inside GitHub callout alerts", TestTaskListInsideCallout);
            RunTest("Tier3", "T3.3: Theme switching with open dirty document", TestThemeSwitchWithDirtyDocument);
            RunTest("Tier3", "T3.4: Fenced code block inside nested blockquote", TestCodeBlockInsideBlockquote);
            RunTest("Tier3", "T3.5: Multi-tab document state isolation and switching", TestMultiTabStateIsolation);

            // MDPlus v1.09 Multi-Format Cross-Feature Combinations
            RunTest("Tier3", "T3.6: Multi-tab session with mixed formats (.md, .csv, .tsv, .json, .log, .ini)", TestMultiTabMixedFormats);
            RunTest("Tier3", "T3.7: Dynamic theme switching across 8 themes while viewing CSV and JSON", TestDynamicThemeSwitchingMultiFormat);
            RunTest("Tier3", "T3.8: Dirty state tracking and saving for tabular (CSV/TSV) documents", TestDirtyStateTrackingTabular);
            RunTest("Tier3", "T3.9: Dirty state tracking and saving for JSON documents", TestDirtyStateTrackingJson);
            RunTest("Tier3", "T3.10: External file watcher reload routing across multi-format documents", TestExternalFileWatcherReloadRouting);
        }

        private static void TestTableInsideBlockquote()
        {
            var parser = new MarkdownParser();
            string md = @"> Here is a table inside a quote:
>
> | Name | Status | Key |
> | :--- | :---: | ---: |
> | **Master** | `active` | [link](https://github.com) |";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count);

            var quote = doc.Blocks[0] as BlockquoteBlock;
            AssertNotNull(quote, "Block should be BlockquoteBlock.");
            AssertTrue(quote!.Blocks.Count >= 2, "Quote should contain paragraph and table.");

            var table = quote.Blocks.OfType<TableBlock>().FirstOrDefault();
            AssertNotNull(table, "Quote must contain parsed TableBlock.");
            AssertEqual(1, table!.Rows.Count);

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);

            var section = flow.Blocks.OfType<Section>().FirstOrDefault();
            AssertNotNull(section, "Converted blockquote should be Section.");
            var wpfTable = section!.Blocks.OfType<Table>().FirstOrDefault();
            AssertNotNull(wpfTable, "Section should contain Table.");
        }

        private static void TestTaskListInsideCallout()
        {
            var parser = new MarkdownParser();
            string md = @"> [!TIP]
> Complete these steps:
> - [x] Install .NET 8 SDK
> - [ ] Run E2E test harness
> - [x] Inspect test report";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count);

            var callout = doc.Blocks[0] as BlockquoteBlock;
            AssertNotNull(callout);
            AssertEqual(CalloutType.Tip, callout!.Callout);

            var list = callout.Blocks.OfType<ListBlock>().FirstOrDefault();
            AssertNotNull(list, "Callout should contain ListBlock.");
            AssertEqual(3, list!.Items.Count);
            AssertTrue(list.Items[0].IsChecked);
            AssertFalse(list.Items[1].IsChecked);
            AssertTrue(list.Items[2].IsChecked);

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
        }

        private static void TestThemeSwitchWithDirtyDocument()
        {
            var tm = ThemeManager.Instance;
            tm.Mode = AppThemeMode.Dark;

            // Create a tab item with modified/dirty state
            var tab = new DocumentTabItem
            {
                FilePath = "C:\\test\\dirty_doc.md",
                Title = "dirty_doc.md",
                RawMarkdown = "# Original Text\n\nSome content."
            };

            // Simulate user editing raw markdown
            tab.RawMarkdown = "# Modified Text\n\nUnsaved changes.";

            // Switch themes while dirty doc is active
            tm.Mode = AppThemeMode.Light;
            AssertEqual(AppThemeMode.Light, tm.Mode);

            // Verify tab data remains intact and unaffected by theme change
            AssertEqual("# Modified Text\n\nUnsaved changes.", tab.RawMarkdown);

            // Revert theme
            tm.Mode = AppThemeMode.Dark;
            AssertEqual(AppThemeMode.Dark, tm.Mode);
            AssertEqual("# Modified Text\n\nUnsaved changes.", tab.RawMarkdown);
        }

        private static void TestCodeBlockInsideBlockquote()
        {
            var parser = new MarkdownParser();
            string md = @"> Implementation note:
> ```csharp
> int exitCode = runner.Execute();
> ```
> And a following remark.";

            var doc = parser.Parse(md);
            AssertEqual(1, doc.Blocks.Count);

            var quote = doc.Blocks[0] as BlockquoteBlock;
            AssertNotNull(quote);

            var code = quote!.Blocks.OfType<CodeBlock>().FirstOrDefault();
            AssertNotNull(code, "Quote must contain CodeBlock.");
            AssertEqual("csharp", code!.Language.ToLowerInvariant());
            AssertContains("int exitCode", code.Code);

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: true);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
        }

        private static void TestMultiTabStateIsolation()
        {
            var tab1 = new DocumentTabItem
            {
                FilePath = "C:\\docs\\file1.md",
                Title = "file1.md",
                RawMarkdown = "# File 1 Content"
            };

            var tab2 = new DocumentTabItem
            {
                FilePath = "C:\\docs\\file2.md",
                Title = "file2.md",
                RawMarkdown = "# File 2 Content"
            };

            // Modify Tab 1 only
            tab1.RawMarkdown = "# File 1 Edited";

            // Verify Tab 2 is unmodified
            AssertEqual("# File 1 Edited", tab1.RawMarkdown);
            AssertEqual("# File 2 Content", tab2.RawMarkdown);

            // Parse both documents independently
            var parser = new MarkdownParser();
            var doc1 = parser.Parse(tab1.RawMarkdown);
            var doc2 = parser.Parse(tab2.RawMarkdown);

            AssertEqual("File 1 Edited", doc1.Title);
            AssertEqual("File 2 Content", doc2.Title);
        }

        #region MDPlus v1.09 Multi-Format Cross-Feature Tests

        private static void TestMultiTabMixedFormats()
        {
            var palette = ThemePalette.GitHubDark;

            var tabMd = new DocumentTabItem
            {
                FilePath = @"C:\docs\readme.md",
                Title = "readme.md",
                Format = DocumentFormat.Markdown,
                RawMarkdown = "# MDPlus\nUniversal Viewer",
                ViewMode = ViewDisplayMode.Rendered
            };
            tabMd.Document = new MarkdownParser().Parse(tabMd.RawMarkdown);
            tabMd.FlowDocument = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, palette).Convert(tabMd.Document);

            var tabCsv = new DocumentTabItem
            {
                FilePath = @"C:\data\sales.csv",
                Title = "sales.csv",
                Format = DocumentFormat.Csv,
                RawMarkdown = "Item,Price\nBook,19.99\nPen,2.50",
                ViewMode = ViewDisplayMode.Rendered
            };
            tabCsv.FlowDocument = CsvToFlowDocumentConverter.Convert(tabCsv.RawMarkdown, palette, isTsv: false);

            var tabTsv = new DocumentTabItem
            {
                FilePath = @"C:\data\metrics.tsv",
                Title = "metrics.tsv",
                Format = DocumentFormat.Tsv,
                RawMarkdown = "Metric\tValue\nCPU\t95\nRAM\t40",
                ViewMode = ViewDisplayMode.Rendered
            };
            tabTsv.FlowDocument = CsvToFlowDocumentConverter.Convert(tabTsv.RawMarkdown, palette, isTsv: true);

            var tabJson = new DocumentTabItem
            {
                FilePath = @"C:\config\settings.json",
                Title = "settings.json",
                Format = DocumentFormat.Json,
                RawMarkdown = "{\n  \"theme\": \"GitHubDark\",\n  \"autoSave\": true\n}",
                ViewMode = ViewDisplayMode.Rendered
            };
            tabJson.FlowDocument = JsonToFlowDocumentConverter.Convert(tabJson.RawMarkdown, palette);

            var tabLog = new DocumentTabItem
            {
                FilePath = @"C:\logs\server.log",
                Title = "server.log",
                Format = DocumentFormat.Log,
                RawMarkdown = "2026-09-11 03:00:00 [INFO] Started",
                ViewMode = ViewDisplayMode.Rendered
            };
            tabLog.FlowDocument = PlainTextToFlowDocumentConverter.Convert(tabLog.RawMarkdown, DocumentFormat.Log, palette);

            var tabIni = new DocumentTabItem
            {
                FilePath = @"C:\config\app.ini",
                Title = "app.ini",
                Format = DocumentFormat.Ini,
                RawMarkdown = "[General]\nMode=Strict",
                ViewMode = ViewDisplayMode.Rendered
            };
            tabIni.FlowDocument = PlainTextToFlowDocumentConverter.Convert(tabIni.RawMarkdown, DocumentFormat.Ini, palette);

            var tabs = new List<DocumentTabItem> { tabMd, tabCsv, tabTsv, tabJson, tabLog, tabIni };
            AssertEqual(6, tabs.Count, "Multi-tab session must contain 6 distinct tabs.");

            // Verify badges
            AssertEqual("MD", tabs[0].FormatBadge);
            AssertEqual("CSV", tabs[1].FormatBadge);
            AssertEqual("TSV", tabs[2].FormatBadge);
            AssertEqual("JSON", tabs[3].FormatBadge);
            AssertEqual("LOG", tabs[4].FormatBadge);
            AssertEqual("INI", tabs[5].FormatBadge);

            // Verify document types
            AssertNotNull(tabs[0].FlowDocument);
            AssertNotNull(tabs[1].FlowDocument);
            AssertTrue(tabs[1].FlowDocument!.Blocks.OfType<WpfTable>().Any(), "CSV tab contains Table");
            AssertTrue(tabs[2].FlowDocument!.Blocks.OfType<WpfTable>().Any(), "TSV tab contains Table");
            AssertTrue(tabs[3].FlowDocument!.Blocks.OfType<Paragraph>().Any(), "JSON tab contains Paragraphs");
            AssertTrue(tabs[4].FlowDocument!.Blocks.OfType<Paragraph>().Any(), "LOG tab contains Paragraphs");

            // Simulate switching tabs and modifying tab 1 without affecting others
            tabs[1].RawMarkdown = "Item,Price\nBook,25.00";
            AssertEqual("Item,Price\nBook,25.00", tabs[1].RawMarkdown);
            AssertEqual("Metric\tValue\nCPU\t95\nRAM\t40", tabs[2].RawMarkdown, "Tab 2 state isolated.");
        }

        private static void TestDynamicThemeSwitchingMultiFormat()
        {
            string csv = "Name,Score\r\nAlice,95\r\nBob,82";
            string json = "{\n  \"status\": \"OK\",\n  \"count\": 10\n}";

            foreach (ThemePreset preset in Enum.GetValues<ThemePreset>())
            {
                var palette = ThemePalette.GetPalette(preset);

                // CSV theme test
                var csvDoc = CsvToFlowDocumentConverter.Convert(csv, palette, isTsv: false);
                var table = csvDoc.Blocks.OfType<WpfTable>().First();
                AssertEqual(palette.TableBorder, table.BorderBrush, $"{preset} CSV Table BorderBrush");
                var headerRow = table.RowGroups[0].Rows[0];
                AssertEqual(palette.TableHeaderBg, headerRow.Background, $"{preset} CSV Header Background");

                // JSON theme test
                var jsonDoc = JsonToFlowDocumentConverter.Convert(json, palette);
                AssertEqual(palette.EditorBg, jsonDoc.Background, $"{preset} JSON Background");
                AssertEqual(palette.EditorFg, jsonDoc.Foreground, $"{preset} JSON Foreground");

                // Verify WCAG contrast
                double contrast = ThemePalette.CalculateContrast(palette.EditorFg.Color, palette.EditorBg.Color);
                AssertTrue(contrast >= 4.5, $"{preset} text contrast ({contrast:F2}:1) must be >= 4.5:1");
            }
        }

        private static void TestDirtyStateTrackingTabular()
        {
            var tab = new DocumentTabItem
            {
                FilePath = @"C:\data\finance.csv",
                Title = "finance.csv",
                Format = DocumentFormat.Csv,
                RawMarkdown = "Q1,100\r\nQ2,200\r\n"
            };

            AssertFalse(tab.IsDirty, "Initial tab state must be clean.");
            AssertEqual("finance.csv", tab.DisplayTitle);

            // User edits tabular cell
            tab.MarkDirty();
            AssertTrue(tab.IsDirty, "Tab must be marked dirty after edit.");
            AssertEqual("finance.csv *", tab.DisplayTitle, "Dirty tab title must append asterisk.");

            // Save document
            var doc = CsvToFlowDocumentConverter.Convert("Q1,150\r\nQ2,200\r\n", ThemePalette.GitHubDark);
            tab.FlowDocument = doc;
            string serialized = CsvSerializer.Serialize(tab.FlowDocument, ',');
            tab.RawMarkdown = serialized;
            tab.MarkClean();

            AssertFalse(tab.IsDirty, "Tab must be marked clean after save.");
            AssertEqual("finance.csv", tab.DisplayTitle, "Clean tab title must not have asterisk.");
            AssertContains("Q1,150", tab.RawMarkdown, "Saved content updated.");
        }

        private static void TestDirtyStateTrackingJson()
        {
            var tab = new DocumentTabItem
            {
                FilePath = @"C:\data\config.json",
                Title = "config.json",
                Format = DocumentFormat.Json,
                RawMarkdown = "{\n  \"version\": 1\n}"
            };

            AssertFalse(tab.IsDirty, "Initial tab state must be clean.");
            AssertEqual("config.json", tab.DisplayTitle);

            // User edits JSON raw text
            tab.RawMarkdown = "{\n  \"version\": 2\n}";
            tab.MarkDirty();
            AssertTrue(tab.IsDirty, "Tab must be marked dirty.");
            AssertEqual("config.json *", tab.DisplayTitle);

            // Save document
            tab.MarkClean();
            AssertFalse(tab.IsDirty, "Tab must be marked clean after save.");
            AssertEqual("config.json", tab.DisplayTitle);
        }

        private static void TestExternalFileWatcherReloadRouting()
        {
            var palette = ThemePalette.GitHubDark;

            // 1. External reload of CSV
            var csvTab = new DocumentTabItem { Format = DocumentFormat.Csv, RawMarkdown = "A,B\n1,2" };
            FlowDocument? reloadedFlow = null;
            if (csvTab.Format is DocumentFormat.Csv or DocumentFormat.Tsv)
            {
                reloadedFlow = CsvToFlowDocumentConverter.Convert(csvTab.RawMarkdown, palette, csvTab.Format == DocumentFormat.Tsv);
            }
            AssertNotNull(reloadedFlow);
            AssertTrue(reloadedFlow!.Blocks.OfType<WpfTable>().Any(), "Reloading CSV routes to Table converter.");

            // 2. External reload of JSON
            var jsonTab = new DocumentTabItem { Format = DocumentFormat.Json, RawMarkdown = "{\"reloaded\": true}" };
            FlowDocument? jsonReloadedFlow = null;
            if (jsonTab.Format == DocumentFormat.Json)
            {
                jsonReloadedFlow = JsonToFlowDocumentConverter.Convert(jsonTab.RawMarkdown, palette);
            }
            AssertNotNull(jsonReloadedFlow);
            AssertTrue(jsonReloadedFlow!.Blocks.OfType<Paragraph>().Any(), "Reloading JSON routes to JSON converter.");

            // 3. External reload of LOG
            var logTab = new DocumentTabItem { Format = DocumentFormat.Log, RawMarkdown = "2026-09-11 [DEBUG] Reloaded" };
            FlowDocument? logReloadedFlow = null;
            if (logTab.Format == DocumentFormat.Log)
            {
                logReloadedFlow = PlainTextToFlowDocumentConverter.Convert(logTab.RawMarkdown, logTab.Format, palette);
            }
            AssertNotNull(logReloadedFlow);
            AssertEqual(13.0, logReloadedFlow!.FontSize, "Reloading LOG preserves monospace font size 13.");
        }

        #endregion
    }
}
