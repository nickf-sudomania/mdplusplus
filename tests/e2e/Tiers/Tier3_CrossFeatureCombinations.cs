using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using MDPlus.Core;
using MDPlus.E2E.Harness;
using MDPlus.Models;
using static MDPlus.E2E.Harness.E2ETestHarness;

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
    }
}
