using System;
using System.IO;
using System.Linq;
using MDPlus.Core;
using MDPlus.E2E.Harness;
using static MDPlus.E2E.Harness.E2ETestHarness;

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
    }
}
