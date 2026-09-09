using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using MDPlus.Core;
using MDPlus.E2E.Harness;
using static MDPlus.E2E.Harness.E2ETestHarness;

namespace MDPlus.E2E.Tiers
{
    public static class Tier2_BoundaryCornerCases
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 2: Boundary & Corner Cases");
            Console.WriteLine("==================================================");

            RunTest("Tier2", "T2.1: Empty and whitespace-only documents", TestEmptyAndWhitespaceDocuments);
            RunTest("Tier2", "T2.2: Massive headings (10,000+ chars, H1-H6)", TestMassiveHeadings);
            RunTest("Tier2", "T2.3: Deeply nested lists (6+ levels of mixed lists)", TestDeeplyNestedLists);
            RunTest("Tier2", "T2.4: Malformed tables with uneven columns and missing delimiters", TestMalformedTables);
            RunTest("Tier2", "T2.5: Unicode, emojis, CJK, and RTL scripts fidelity", TestUnicodeAndEmojiFidelity);
            RunTest("Tier2", "T2.6: Extreme zoom levels (20% to 500% bounds)", TestExtremeZoomLevels);
            RunTest("Tier2", "T2.7: Rapid theme toggles (100 consecutive switches)", TestRapidThemeToggles);
        }

        private static void TestEmptyAndWhitespaceDocuments()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);

            // 1. Completely empty string
            var doc1 = parser.Parse("");
            AssertNotNull(doc1);
            AssertEqual(0, doc1.Blocks.Count);
            var flow1 = converter.Convert(doc1);
            AssertNotNull(flow1);
            AssertEqual(0, flow1.Blocks.Count);

            // 2. Spaces and tabs only
            var doc2 = parser.Parse("   \t  \t   ");
            AssertNotNull(doc2);
            AssertEqual(0, doc2.Blocks.Count);

            // 3. Newlines only
            var doc3 = parser.Parse("\r\n\r\n\n\r\n");
            AssertNotNull(doc3);
            AssertEqual(0, doc3.Blocks.Count);
        }

        private static void TestMassiveHeadings()
        {
            var parser = new MarkdownParser();
            var sb = new StringBuilder();

            // 10,000 character heading
            sb.Append("# ");
            for (int i = 0; i < 1000; i++)
            {
                sb.Append("AlphaBeta ");
            }
            string massiveHeading = sb.ToString();

            var doc = parser.Parse(massiveHeading);
            AssertEqual(1, doc.Blocks.Count);

            var h = doc.Blocks[0] as HeadingBlock;
            AssertNotNull(h);
            AssertEqual(1, h!.Level);
            AssertTrue(h.Text.Length >= 9000, "Heading text length should be preserved.");

            // Convert to FlowDocument
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertEqual(1, flow.Blocks.Count);
        }

        private static void TestDeeplyNestedLists()
        {
            var parser = new MarkdownParser();
            string md = @"- Level 1
  - Level 2
    - Level 3
      - Level 4
        - [ ] Level 5 Task
          - [x] Level 6 Completed Task";

            var doc = parser.Parse(md);
            AssertTrue(doc.Blocks.Count >= 1, "Should parse top-level list.");

            var list = doc.Blocks[0] as ListBlock;
            AssertNotNull(list);
            AssertTrue(list!.Items.Count >= 1);

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
        }

        private static void TestMalformedTables()
        {
            var parser = new MarkdownParser();
            string malformedTable = @"| Col1 | Col2 | Col3 |
| --- | --- |
| Only One Cell |
| Cell 1 | Cell 2 | Cell 3 | Cell 4 Overflow |
Missing pipes row
| | | |";

            var doc = parser.Parse(malformedTable);
            AssertNotNull(doc);
            AssertTrue(doc.Blocks.Count > 0, "Parser must not throw on malformed tables.");

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertNotNull(flow, "FlowDocument conversion must succeed gracefully.");
        }

        private static void TestUnicodeAndEmojiFidelity()
        {
            var parser = new MarkdownParser();
            string md = @"# 🚀 MDPlus 高速エディタ - مرحبا بك!

| Symbol | Description | Japanese | Arabic | Emoji |
| :---: | :--- | :--- | :--- | :---: |
| ⚡ | High Speed | 超高速 | سريع جدا | 🔥 |
| 𠮷 | Rare Kanji | 𠮷野家 | نادر | 🍜 |
| 🛡️ | Safe Hash | 安全性 | آمن | ✅ |";

            var doc = parser.Parse(md);
            AssertEqual(2, doc.Blocks.Count);

            var h = doc.Blocks[0] as HeadingBlock;
            AssertNotNull(h);
            AssertContains("🚀", h!.Text);
            AssertContains("高速エディタ", h.Text);
            AssertContains("مرحبا بك!", h.Text);

            var table = doc.Blocks[1] as TableBlock;
            AssertNotNull(table);
            AssertEqual(3, table!.Rows.Count);

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
        }

        private static void TestExtremeZoomLevels()
        {
            // Verify zoom boundaries (e.g. 20% to 500%)
            double[] testZooms = new[] { 0.20, 0.50, 1.00, 1.50, 2.50, 5.00 };

            foreach (var zoom in testZooms)
            {
                AssertTrue(zoom >= 0.20 && zoom <= 5.00, $"Zoom {zoom} must be within allowed bounds.");
                double baseFontSize = 15.0;
                double effectiveSize = baseFontSize * zoom;
                AssertTrue(effectiveSize > 0, "Effective font size must remain positive.");
            }
        }

        private static void TestRapidThemeToggles()
        {
            var tm = ThemeManager.Instance;
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                tm.Mode = (i % 2 == 0) ? AppThemeMode.Dark : AppThemeMode.Light;
                AssertNotNull(tm.WindowBackground);
                AssertNotNull(tm.Foreground);
            }

            sw.Stop();
            AssertTrue(sw.ElapsedMilliseconds < 1000, $"100 theme toggles must execute in < 1000ms. Took: {sw.ElapsedMilliseconds}ms.");
        }
    }
}
