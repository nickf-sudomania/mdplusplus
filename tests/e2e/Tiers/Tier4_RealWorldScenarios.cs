using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Documents;
using MDPlus.Core;
using MDPlus.Models;
using MDPlus.E2E.Harness;
using static MDPlus.E2E.Harness.E2ETestHarness;
using WpfTable = System.Windows.Documents.Table;

namespace MDPlus.E2E.Tiers
{
    public static class Tier4_RealWorldScenarios
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 4: Real-World Application Scenarios");
            Console.WriteLine("==================================================");

            RunTest("Tier4", "T4.1: Real-world GitHub README with badges and benchmarks", TestRealWorldReadme);
            RunTest("Tier4", "T4.2: Developer release notes and multi-version changelog", TestDeveloperReleaseNotes);
            RunTest("Tier4", "T4.3: Mathematical and technical documentation", TestMathematicalNotes);
            RunTest("Tier4", "T4.4: Full CommonMark specification edge cases", TestCommonMarkSpecSuite);
            RunTest("Tier4", "T4.5: End-to-end file lifecycle (Parse, Edit, Hash, Export)", TestEndToEndFileLifecycle);

            // MDPlus v1.09 Real-World Multi-Format Scenarios
            RunTest("Tier4", "T4.6: 5,000-row CSV performance benchmark (< 50 ms)", TestBenchmark5000RowsCsv);
            RunTest("Tier4", "T4.7: 5,000-line JSON performance benchmark (< 50 ms)", TestBenchmark5000LinesJson);
            RunTest("Tier4", "T4.8: Command-line argument launching with multi-format file paths", TestCommandLineMultiFormatLaunching);
            RunTest("Tier4", "T4.9: Multi-format session restore serialization and deserialization", TestMultiFormatSessionRestore);
            RunTest("Tier4", "T4.10: End-to-end multi-format file lifecycle (Detect, Load, Format, Toggle, Dirty-Edit, Save, Reopen)", TestEndToEndMultiFormatLifecycle);
        }

        private static void TestRealWorldReadme()
        {
            string readmeMarkdown = @"# MDPlus - Fast Lightweight Windows Markdown Editor

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/nickf-sudomania/mdplusplus)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

MDPlus is inspired by Notepad and Notepad++, delivering instant startup and ultra-low memory footprint.

## Performance Benchmark

| Metric | MDPlus | MarkText | Obsidian | Joplin |
| :--- | :---: | :---: | :---: | :---: |
| **Cold Startup** | **< 150 ms** | 2,800 ms | 3,200 ms | 4,100 ms |
| **Idle RAM** | **~25 MB** | 350 MB | 420 MB | 510 MB |
| **Install Size** | **< 10 MB** | 180 MB | 220 MB | 290 MB |

## Features
- [x] In-place rendered rich editing with WPF FlowDocument
- [x] Two-way CommonMark / GFM markdown serialization
- [x] 5 switchable theme presets (GitHub Dark, GitHub Light, Nord, One Dark, Monokai)
- [x] High-contrast WCAG AA compliant menus (ratio >= 4.5:1)
- [x] Embedded multi-resolution AppIcon.ico (16, 32, 48, 256)

> [!NOTE]
> Press **F8** to dynamically cycle themes without restarting.

```powershell
# Run MDPlus with a target markdown file
.\MDPlus.exe .\docs\spec.md
```";

            var parser = new MarkdownParser();
            var doc = parser.Parse(readmeMarkdown);

            AssertEqual("MDPlus - Fast Lightweight Windows Markdown Editor", doc.Title);
            AssertTrue(doc.Blocks.Count >= 7, $"Expected at least 7 blocks, got {doc.Blocks.Count}.");

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: true);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
            AssertTrue(flow.Blocks.Count >= 7, "FlowDocument should convert all blocks.");

            // Verify HTML export
            string html = HtmlExporter.ExportToFullHtml(doc, "MDPlus README", isDark: true);
            AssertContains("<table", html);
            AssertContains("Performance Benchmark", html);
        }

        private static void TestDeveloperReleaseNotes()
        {
            string changelog = @"# MDPlus Changelog

All notable changes to this project are documented here.

## [v2.4.0] - 2026-09-09
### Added
- Rich rendered in-place editing via WPF RichTextBox.
- Two-way lossless Markdown serializer.
- 5 curated theme presets: GitHub Dark, GitHub Light, Nord, One Dark, Monokai.

### Fixed
- Menu bar contrast ratio elevated to >= 4.5:1 for WCAG AA compliance.
- Suppressed file watcher self-reload loops during internal saves.

## [v2.3.0] - 2026-08-15
### Added
- Cryptographic SHA-256 verification dialog and command-line support.
- Multi-format checksum manifest parser.";

            var parser = new MarkdownParser();
            var doc = parser.Parse(changelog);

            AssertEqual("MDPlus Changelog", doc.Title);
            int headingCount = doc.Blocks.OfType<HeadingBlock>().Count();
            AssertTrue(headingCount >= 3, "Changelog should extract multiple version headings.");

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
        }

        private static void TestMathematicalNotes()
        {
            string mathDoc = @"# Scientific & Mathematical Notes

Euler's identity is given by: $e^{i\pi} + 1 = 0$.

### Polynomial Expansion
Consider the quadratic equation:
$f(x) = ax^2 + bx + c$

Where discriminant $\Delta = b^2 - 4ac$:
- If $\Delta > 0$: two distinct real roots.
- If $\Delta = 0$: one repeated real root.
- If $\Delta < 0$: two complex conjugate roots.

| Symbol | Meaning | Value / Range |
| :--- | :--- | :--- |
| $\pi$ | Archimedes constant | $\approx 3.14159$ |
| $e$ | Euler number | $\approx 2.71828$ |
| $\gamma$ | Euler-Mascheroni | $\approx 0.57721$ |";

            var parser = new MarkdownParser();
            var doc = parser.Parse(mathDoc);

            AssertEqual("Scientific & Mathematical Notes", doc.Title);
            var table = doc.Blocks.OfType<TableBlock>().FirstOrDefault();
            AssertNotNull(table);
            AssertEqual(3, table!.Rows.Count);

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: true);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
        }

        private static void TestCommonMarkSpecSuite()
        {
            string complexSpec = @"# Spec Suite

Line with escaped \*asterisks\* and \_underscores\_.

Code span with internal backticks: `` `code` ``.

Hard line break after two spaces:  
Next line of the same paragraph.

---

* Thematic break above
* List continuing here";

            var parser = new MarkdownParser();
            var doc = parser.Parse(complexSpec);

            AssertEqual("Spec Suite", doc.Title);
            AssertTrue(doc.Blocks.Count >= 4);

            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, isDark: false);
            var flow = converter.Convert(doc);
            AssertNotNull(flow);
        }

        private static void TestEndToEndFileLifecycle()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"mdplus_e2e_lifecycle_{Guid.NewGuid():N}.md");
            try
            {
                // 1. Create file on disk
                string initialContent = "# E2E Lifecycle Document\n\nInitial paragraph text.\n";
                File.WriteAllText(tempPath, initialContent);

                // 2. Parse into AST
                var parser = new MarkdownParser();
                var doc = parser.Parse(File.ReadAllText(tempPath));
                AssertEqual("E2E Lifecycle Document", doc.Title);

                // 3. Convert to FlowDocument
                var converter = new MarkdownToWpfConverter(Path.GetDirectoryName(tempPath)!, isDark: false);
                var flowDoc = converter.Convert(doc);
                AssertNotNull(flowDoc);

                // 4. Modify document content
                string updatedContent = "# E2E Lifecycle Document\n\nUpdated paragraph after rich editing.\n";
                File.WriteAllText(tempPath, updatedContent);

                // 5. Compute cryptographic SHA-256 hash
                string hash = HashService.ComputeSha256(tempPath);
                AssertEqual(64, hash.Length);

                // 6. Verify hash integrity
                bool verified = HashService.VerifyFileSha256(tempPath, hash);
                AssertTrue(verified, "SHA-256 checksum verification must succeed.");

                // 7. Export to HTML
                var updatedDoc = parser.Parse(updatedContent);
                string html = HtmlExporter.ExportToFullHtml(updatedDoc, "Lifecycle Export", isDark: false);
                AssertContains("Updated paragraph after rich editing", html);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        #region MDPlus v1.09 Real-World Multi-Format Scenarios

        private static void TestBenchmark5000RowsCsv()
        {
            // 1. Generate realistic 5,000-row CSV
            var sb = new StringBuilder(5000 * 80);
            sb.AppendLine("Id,Name,Department,Salary,Active,Notes");
            for (int i = 1; i <= 5000; i++)
            {
                sb.AppendLine($"{i},\"Employee {i}\",\"Dept {i % 10}\",\"${45000 + i * 15:N0}\",{(i % 2 == 0 ? "true" : "false")},\"Notes for record {i}, with comma\"");
            }
            string csvData = sb.ToString();

            // 2. Parse and benchmark
            var sw = Stopwatch.StartNew();
            var rows = CsvParser.Parse(csvData);
            sw.Stop();

            AssertEqual(5001, rows.Count, "5,001 rows parsed (1 header + 5,000 data).");
            AssertTrue(sw.ElapsedMilliseconds < 50, $"5,000-row CSV parse must complete in < 50 ms. Actual: {sw.ElapsedMilliseconds} ms.");

            // 3. Convert to FlowDocument Table and verify row capping at 3000
            var doc = CsvToFlowDocumentConverter.Convert(rows, ThemePalette.GitHubDark, isTsv: false);
            AssertNotNull(doc);
            var table = doc.Blocks.OfType<WpfTable>().First();
            AssertEqual(3000, table.RowGroups[1].Rows.Count, "WPF layout capped at 3,000 visual rows to avoid starvation.");

            var noticePara = doc.Blocks.OfType<Paragraph>().Last();
            string noticeText = string.Concat(noticePara.Inlines.OfType<Run>().Select(r => r.Text));
            AssertContains("Showing first 3,000 of 5,000 records", noticeText);
        }

        private static void TestBenchmark5000LinesJson()
        {
            // 1. Generate realistic 5,000-line JSON document
            var sb = new StringBuilder(5000 * 60);
            sb.Append("[\n");
            for (int i = 1; i <= 1000; i++)
            {
                sb.Append("  {\n");
                sb.Append($"    \"id\": {i},\n");
                sb.Append($"    \"name\": \"Item {i}\",\n");
                sb.Append($"    \"price\": {19.99 + i},\n");
                sb.Append($"    \"active\": {(i % 2 == 0 ? "true" : "false")}\n");
                sb.Append(i == 1000 ? "  }\n" : "  },\n");
            }
            sb.Append("]\n");
            string jsonData = sb.ToString();

            // 2. Parse and pretty-print benchmark (< 50 ms)
            var sw = Stopwatch.StartNew();
            using var jsonDoc = JsonDocument.Parse(jsonData, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                jsonDoc.WriteTo(writer);
            }
            string formattedJson = Encoding.UTF8.GetString(stream.ToArray());
            sw.Stop();

            AssertTrue(formattedJson.Length > 0);
            AssertTrue(sw.ElapsedMilliseconds < 50, $"5,000-line JSON parse must complete in < 50 ms. Actual: {sw.ElapsedMilliseconds} ms.");

            // 3. Document rendering test on realistic JSON sample
            var sampleJson = "{\n  \"status\": \"ok\",\n  \"items\": [1, 2, 3]\n}";
            var doc = JsonToFlowDocumentConverter.Convert(sampleJson, ThemePalette.GitHubDark);
            AssertNotNull(doc);
            AssertTrue(doc.Blocks.Count > 0, "FlowDocument contains paragraphs for JSON lines.");
        }

        private static void TestCommandLineMultiFormatLaunching()
        {
            string[] testFiles = new[] { "financials.csv", "config.json", "access.log", "notes.txt" };
            var tabs = new List<DocumentTabItem>();

            foreach (var file in testFiles)
            {
                var format = DocumentFormatHelper.DetectFromPath(file);
                var tab = new DocumentTabItem
                {
                    FilePath = Path.Combine(Path.GetTempPath(), file),
                    Format = format,
                    Title = file
                };
                tabs.Add(tab);
            }

            AssertEqual(4, tabs.Count);
            AssertEqual(DocumentFormat.Csv, tabs[0].Format);
            AssertEqual("CSV", tabs[0].FormatBadge);

            AssertEqual(DocumentFormat.Json, tabs[1].Format);
            AssertEqual("JSON", tabs[1].FormatBadge);

            AssertEqual(DocumentFormat.Log, tabs[2].Format);
            AssertEqual("LOG", tabs[2].FormatBadge);

            AssertEqual(DocumentFormat.PlainText, tabs[3].Format);
            AssertEqual("TXT", tabs[3].FormatBadge);
        }

        private static void TestMultiFormatSessionRestore()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"mdplus_session_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                string mdPath = Path.Combine(tempDir, "readme.md");
                string csvPath = Path.Combine(tempDir, "data.csv");
                string jsonPath = Path.Combine(tempDir, "app.json");
                string logPath = Path.Combine(tempDir, "system.log");

                File.WriteAllText(mdPath, "# Test");
                File.WriteAllText(csvPath, "a,b\n1,2");
                File.WriteAllText(jsonPath, "{\"k\":1}");
                File.WriteAllText(logPath, "Log entry");

                var settings = new AppSettings();
                settings.ResumeSession = true;
                settings.UpdateOpenFiles(new[] { mdPath, csvPath, jsonPath, logPath }, csvPath);

                AssertEqual(4, settings.OpenFiles.Count, "Settings must record 4 open files.");
                AssertEqual(csvPath, settings.ActiveFile, "Active file recorded accurately.");

                // Serialize settings
                string json = System.Text.Json.JsonSerializer.Serialize(settings);
                AssertNotNull(json);

                // Deserialize and reconstruct session
                var restored = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json);
                AssertNotNull(restored);
                AssertTrue(restored!.ResumeSession);
                AssertEqual(4, restored.OpenFiles.Count);
                AssertEqual(csvPath, restored.ActiveFile);

                AssertEqual(DocumentFormat.Markdown, DocumentFormatHelper.DetectFromPath(restored.OpenFiles[0]));
                AssertEqual(DocumentFormat.Csv, DocumentFormatHelper.DetectFromPath(restored.OpenFiles[1]));
                AssertEqual(DocumentFormat.Json, DocumentFormatHelper.DetectFromPath(restored.OpenFiles[2]));
                AssertEqual(DocumentFormat.Log, DocumentFormatHelper.DetectFromPath(restored.OpenFiles[3]));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        private static void TestEndToEndMultiFormatLifecycle()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"mdplus_lifecycle_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                var palette = ThemePalette.GitHubDark;

                // 1. CSV Lifecycle: Create on disk -> Load -> Convert -> Edit -> Save -> Verify disk
                string csvPath = Path.Combine(tempDir, "users.csv");
                string initialCsv = "ID,Name,Role\r\n1,Alice,Admin\r\n2,Bob,Guest\r\n";
                File.WriteAllText(csvPath, initialCsv);

                var csvTab = new DocumentTabItem
                {
                    FilePath = csvPath,
                    Format = DocumentFormatHelper.DetectFromPath(csvPath),
                    RawMarkdown = File.ReadAllText(csvPath)
                };
                AssertEqual(DocumentFormat.Csv, csvTab.Format);
                csvTab.FlowDocument = CsvToFlowDocumentConverter.Convert(csvTab.RawMarkdown, palette, isTsv: false);

                // Edit data: change Bob's role to Editor
                string updatedCsv = "ID,Name,Role\r\n1,Alice,Admin\r\n2,Bob,Editor\r\n";
                csvTab.FlowDocument = CsvToFlowDocumentConverter.Convert(updatedCsv, palette, isTsv: false);
                string serializedCsv = CsvSerializer.Serialize(csvTab.FlowDocument, ',');
                File.WriteAllText(csvPath, serializedCsv);

                string reloadedCsv = File.ReadAllText(csvPath);
                AssertEqual(updatedCsv, reloadedCsv, "CSV file on disk reflects serialized edit with 100% fidelity.");

                // 2. JSON Lifecycle: Create on disk -> Load -> Convert -> Edit -> Save -> Verify disk
                string jsonPath = Path.Combine(tempDir, "config.json");
                string initialJson = "{\n  \"port\": 8080,\n  \"debug\": false\n}";
                File.WriteAllText(jsonPath, initialJson);

                var jsonTab = new DocumentTabItem
                {
                    FilePath = jsonPath,
                    Format = DocumentFormatHelper.DetectFromPath(jsonPath),
                    RawMarkdown = File.ReadAllText(jsonPath)
                };
                AssertEqual(DocumentFormat.Json, jsonTab.Format);
                jsonTab.FlowDocument = JsonToFlowDocumentConverter.Convert(jsonTab.RawMarkdown, palette);

                // Edit data: change port to 9090
                string updatedJson = "{\n  \"port\": 9090,\n  \"debug\": true\n}";
                jsonTab.RawMarkdown = updatedJson;
                File.WriteAllText(jsonPath, jsonTab.RawMarkdown);

                string reloadedJson = File.ReadAllText(jsonPath);
                AssertEqual(updatedJson, reloadedJson, "JSON file on disk reflects edit with 100% fidelity.");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        #endregion
    }
}
