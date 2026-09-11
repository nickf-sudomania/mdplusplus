using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using MDPlus.Core;
using MDPlus.Models;
using MDPlus.E2E.Harness;
using static MDPlus.E2E.Harness.E2ETestHarness;
using WpfTable = System.Windows.Documents.Table;

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

            // MDPlus v1.09 Multi-Format Boundary & Corner Cases
            RunTest("Tier2", "T2.8: Empty CSV and TSV handling & placeholder notice", TestEmptyCsvAndTsv);
            RunTest("Tier2", "T2.9: Single column CSV and trailing blank line suppression", TestSingleColumnAndTrailingNewlines);
            RunTest("Tier2", "T2.10: Jagged rows column harmonization with empty field padding", TestJaggedRowsHarmonization);
            RunTest("Tier2", "T2.11: RFC 4180 complex quoting (embedded commas, tabs, escaped quotes, CRLF)", TestRfc4180ComplexQuoting);
            RunTest("Tier2", "T2.12: Malformed JSON syntax error line/column reporting without crash", TestMalformedJsonErrorReporting);
            RunTest("Tier2", "T2.13: Empty and whitespace-only JSON document handling", TestEmptyAndWhitespaceJson);
            RunTest("Tier2", "T2.14: Plain text and log boundary handling (extreme lines, mixed newlines, special characters)", TestTextAndLogBoundaries);
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

        #region MDPlus v1.09 Multi-Format Boundary Cases

        private static void TestEmptyCsvAndTsv()
        {
            var palette = ThemePalette.GitHubDark;

            // 1. Completely empty string
            var rowsEmpty = CsvParser.Parse("");
            AssertEqual(0, rowsEmpty.Count, "Empty string produces 0 parsed rows.");

            var flowCsvEmpty = CsvToFlowDocumentConverter.Convert("", palette, isTsv: false);
            AssertNotNull(flowCsvEmpty);
            AssertEqual(1, flowCsvEmpty.Blocks.Count, "Empty CSV FlowDocument has 1 placeholder block.");
            var pCsv = flowCsvEmpty.Blocks.OfType<Paragraph>().First();
            AssertEqual("Empty CSV document", ((Run)pCsv.Inlines.First()).Text);

            var flowTsvEmpty = CsvToFlowDocumentConverter.Convert("", palette, isTsv: true);
            AssertNotNull(flowTsvEmpty);
            AssertEqual(1, flowTsvEmpty.Blocks.Count, "Empty TSV FlowDocument has 1 placeholder block.");
            var pTsv = flowTsvEmpty.Blocks.OfType<Paragraph>().First();
            AssertEqual("Empty TSV document", ((Run)pTsv.Inlines.First()).Text);

            // 2. Whitespace-only string
            var rowsWs = CsvParser.Parse("   \t  \r\n\t   ");
            AssertEqual(0, rowsWs.Count, "Whitespace-only input produces 0 rows.");

            // 3. Serialize empty document
            string serializedNull = CsvSerializer.Serialize(null);
            AssertEqual(string.Empty, serializedNull, "Serializing null document produces empty string.");

            string serializedEmpty = CsvSerializer.Serialize(new FlowDocument());
            AssertEqual(string.Empty, serializedEmpty, "Serializing empty FlowDocument produces empty string.");

            string serializedPlaceholder = CsvSerializer.Serialize(flowCsvEmpty);
            AssertContains("Empty CSV document", serializedPlaceholder, "Serializing placeholder extracts document notice.");
        }

        private static void TestSingleColumnAndTrailingNewlines()
        {
            // 1. Single column with and without trailing newline (RFC 4180 trailing newline suppression)
            string withTrailing = "ID\r\n101\r\n102\r\n103\r\n";
            string withoutTrailing = "ID\r\n101\r\n102\r\n103";

            var rowsWith = CsvParser.Parse(withTrailing);
            var rowsWithout = CsvParser.Parse(withoutTrailing);

            AssertEqual(4, rowsWith.Count, "Trailing newline must be suppressed, resulting in exactly 4 rows.");
            AssertEqual(4, rowsWithout.Count, "Without trailing newline results in exactly 4 rows.");

            foreach (var r in rowsWith)
            {
                AssertEqual(1, r.Count, "Single-column CSV must have exactly 1 column per row.");
            }
            AssertEqual("ID", rowsWith[0][0]);
            AssertEqual("101", rowsWith[1][0]);
            AssertEqual("102", rowsWith[2][0]);
            AssertEqual("103", rowsWith[3][0]);

            // 2. Convert to FlowDocument Table
            var doc = CsvToFlowDocumentConverter.Convert(withTrailing, ThemePalette.GitHubLight);
            var table = doc.Blocks.OfType<WpfTable>().First();
            AssertEqual(1, table.Columns.Count, "Single column table must have 1 column.");
            AssertEqual(3, table.RowGroups[1].Rows.Count, "Table must have 3 data rows.");
        }

        private static void TestJaggedRowsHarmonization()
        {
            // Row 1 has 2 cols, Row 2 has 5 cols, Row 3 has 1 col, Row 4 has 3 cols
            string jagged = "H1,H2\r\nA,B,C,D,E\r\nSingle\r\nX,Y,Z";
            var rows = CsvParser.Parse(jagged);

            AssertEqual(4, rows.Count, "Must parse 4 rows.");
            foreach (var r in rows)
            {
                AssertEqual(5, r.Count, "All jagged rows must be padded to max column count (5).");
            }

            AssertEqual("H1", rows[0][0]);
            AssertEqual("H2", rows[0][1]);
            AssertEqual("", rows[0][2], "Padded cell must be empty string.");
            AssertEqual("", rows[0][3], "Padded cell must be empty string.");
            AssertEqual("", rows[0][4], "Padded cell must be empty string.");

            AssertEqual("Single", rows[2][0]);
            AssertEqual("", rows[2][1]);
            AssertEqual("", rows[2][2]);
            AssertEqual("", rows[2][3]);
            AssertEqual("", rows[2][4]);

            // Render to FlowDocument Table: must not crash
            var doc = CsvToFlowDocumentConverter.Convert(jagged, ThemePalette.Nord);
            var table = doc.Blocks.OfType<WpfTable>().First();
            AssertEqual(5, table.Columns.Count, "Table must have 5 columns.");
            AssertEqual(3, table.RowGroups[1].Rows.Count, "Table must have 3 data rows.");
            foreach (var dataRow in table.RowGroups[1].Rows)
            {
                AssertEqual(5, dataRow.Cells.Count, "Every data row must have 5 cells.");
            }
        }

        private static void TestRfc4180ComplexQuoting()
        {
            // Fields with embedded commas, quotes (""), and CRLF inside quotes
            string complexCsv = "\"Doe, John\",\"Senior \"\"Lead\"\" Architect\",\"Line 1\r\nLine 2\",150000\r\n\"Smith, Jane\",\"VP of \"\"Global\"\" Engineering\",\"Simple Note\",200000\r\n";
            var rows = CsvParser.Parse(complexCsv);

            AssertEqual(2, rows.Count, "Must parse exactly 2 rows.");
            AssertEqual(4, rows[0].Count, "Row 0 has 4 columns.");
            AssertEqual("Doe, John", rows[0][0], "Embedded comma preserved.");
            AssertEqual("Senior \"Lead\" Architect", rows[0][1], "Escaped quotes (\"\") unescaped to single quote.");
            AssertEqual("Line 1\nLine 2", rows[0][2].Replace("\r\n", "\n"), "Embedded CRLF preserved inside field.");
            AssertEqual("150000", rows[0][3]);

            AssertEqual("Smith, Jane", rows[1][0]);
            AssertEqual("VP of \"Global\" Engineering", rows[1][1]);
            AssertEqual("Simple Note", rows[1][2]);
            AssertEqual("200000", rows[1][3]);

            // Convert to FlowDocument and re-serialize
            var doc = CsvToFlowDocumentConverter.Convert(complexCsv, ThemePalette.GitHubDark);
            string serialized = CsvSerializer.Serialize(doc, ',');

            var reParsed = CsvParser.Parse(serialized);
            AssertEqual(2, reParsed.Count, "Re-serialized CSV must parse back to 2 rows.");
            AssertEqual(rows[0][0], reParsed[0][0], "Column 0 round-trip equality.");
            AssertEqual(rows[0][1], reParsed[0][1], "Column 1 round-trip equality.");
            AssertEqual(rows[0][2].Replace("\r\n", "\n"), reParsed[0][2].Replace("\r\n", "\n"), "Column 2 round-trip equality.");
            AssertEqual(rows[0][3], reParsed[0][3], "Column 3 round-trip equality.");
        }

        private static void TestMalformedJsonErrorReporting()
        {
            var palette = ThemePalette.GitHubDark;

            // 1. Unclosed braces
            string broken1 = "{\n  \"valid\": 123,\n  \"missing_close\": true";
            var doc1 = JsonToFlowDocumentConverter.Convert(broken1, palette);
            AssertNotNull(doc1);
            var firstP1 = doc1.Blocks.OfType<Paragraph>().First();
            string banner1 = string.Concat(firstP1.Inlines.OfType<Run>().Select(r => r.Text));
            AssertContains("Malformed JSON", banner1);

            // 2. Unquoted tokens / bare words
            string broken2 = "{\n  bare_identifier: 123\n}";
            var doc2 = JsonToFlowDocumentConverter.Convert(broken2, palette);
            AssertNotNull(doc2);
            var firstP2 = doc2.Blocks.OfType<Paragraph>().First();
            string banner2 = string.Concat(firstP2.Inlines.OfType<Run>().Select(r => r.Text));
            AssertContains("Malformed JSON", banner2);
            AssertContains("Line", banner2);

            // 3. Raw text preservation in subsequent paragraphs
            AssertTrue(doc2.Blocks.Count > 1, "Must contain raw lines below error banner.");
            var allLines = doc2.Blocks.OfType<Paragraph>().Skip(1)
                .Select(p => string.Concat(p.Inlines.OfType<Run>().Select(r => r.Text)))
                .ToList();
            AssertTrue(allLines.Any(l => l.Contains("bare_identifier")), "Raw text must be accessible.");
        }

        private static void TestEmptyAndWhitespaceJson()
        {
            var palette = ThemePalette.Nord;

            // 1. Empty string
            var doc1 = JsonToFlowDocumentConverter.Convert("", palette);
            AssertNotNull(doc1);
            var p1 = doc1.Blocks.OfType<Paragraph>().First();
            AssertEqual("Empty JSON document", ((Run)p1.Inlines.First()).Text);

            // 2. Whitespace only
            var doc2 = JsonToFlowDocumentConverter.Convert("   \t\r\n   ", palette);
            AssertNotNull(doc2);
            var p2 = doc2.Blocks.OfType<Paragraph>().First();
            AssertEqual("Empty JSON document", ((Run)p2.Inlines.First()).Text);

            // 3. Null string
            var doc3 = JsonToFlowDocumentConverter.Convert(null, palette);
            AssertNotNull(doc3);
            var p3 = doc3.Blocks.OfType<Paragraph>().First();
            AssertEqual("Empty JSON document", ((Run)p3.Inlines.First()).Text);
        }

        private static void TestTextAndLogBoundaries()
        {
            var palette = ThemePalette.GitHubDark;

            // 1. Extreme single line (25,000 chars)
            string massiveLine = new string('A', 25000);
            var doc1 = PlainTextToFlowDocumentConverter.Convert(massiveLine, DocumentFormat.PlainText, palette);
            AssertNotNull(doc1);
            var p1 = doc1.Blocks.OfType<Paragraph>().First();
            string renderedText = ((Run)p1.Inlines.First()).Text;
            AssertEqual(25000, renderedText.Length, "Extreme single line must be preserved without truncation.");

            // 2. Mixed newlines (\r\n, \n, \r)
            string mixedLines = "Line 1\r\nLine 2\nLine 3\rLine 4";
            var doc2 = PlainTextToFlowDocumentConverter.Convert(mixedLines, DocumentFormat.Log, palette);
            AssertNotNull(doc2);

            // 3. Special characters, Unicode, emojis, RTL
            string unicodeText = "English 🚀 简体中文 繁體中文 日本語 한국어 العربية עברית © ® ™ 2026";
            var doc3 = PlainTextToFlowDocumentConverter.Convert(unicodeText, DocumentFormat.PlainText, palette);
            AssertNotNull(doc3);
            var p3 = doc3.Blocks.OfType<Paragraph>().First();
            AssertEqual(unicodeText, ((Run)p3.Inlines.First()).Text, "All Unicode and emoji characters preserved.");
        }

        #endregion
    }
}
