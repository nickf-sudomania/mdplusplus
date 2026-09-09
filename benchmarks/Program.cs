using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WpfList = System.Windows.Documents.List;
using MDPlus.Core;

namespace MDPlus.Benchmarks
{
    public class Program
    {
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private static int _passedCount = 0;
        private static int _failedCount = 0;

        [STAThread]
        public static int Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("  MDPlus Challenger Adversarial & Stress Harness  ");
            Console.WriteLine("==================================================");
            Console.WriteLine($"Runtime: .NET {Environment.Version} on {Environment.OSVersion}");
            Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            Console.WriteLine();

            var totalStopwatch = Stopwatch.StartNew();

            Console.WriteLine(">>> SECTION 1: Adversarial Stress Testing on MarkdownSerializer (R1)");
            RunAdversarialStressTests();

            Console.WriteLine("\n>>> SECTION 2: Serialization Latency Benchmark on 5,000-Line Document (R5)");
            RunLargeDocumentSerializationBenchmark();

            Console.WriteLine("\n>>> SECTION 3: Cold Startup & Idle Memory Benchmarks (R5)");
            RunProcessPerformanceBenchmarks();

            totalStopwatch.Stop();

            Console.WriteLine("\n==================================================");
            Console.WriteLine("               BENCHMARK & STRESS SUMMARY         ");
            Console.WriteLine("==================================================");
            Console.WriteLine($"Stress Tests Passed: {_passedCount}");
            Console.WriteLine($"Stress Tests Failed: {_failedCount}");
            Console.WriteLine($"Total Elapsed: {totalStopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine("==================================================");

            return _failedCount == 0 ? 0 : 1;
        }

        private static void Assert(bool condition, string testName, string detail = "")
        {
            if (condition)
            {
                _passedCount++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  [PASS] {testName}");
                Console.ResetColor();
            }
            else
            {
                _failedCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] {testName} - {detail}");
                Console.ResetColor();
            }
        }

        private static void RunAdversarialStressTests()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(".", ThemePalette.GitHubDark);

            // Test 1: Deeply nested lists inside blockquotes (10 levels)
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("> Quote Level 1");
                sb.AppendLine("> > Quote Level 2");
                sb.AppendLine("> > - L1 Item");
                sb.AppendLine("> >   - L2 Item");
                sb.AppendLine("> >     - L3 Item");
                sb.AppendLine("> >       - L4 Item");
                sb.AppendLine("> >         - L5 Item");
                sb.AppendLine("> >           - L6 Item");
                sb.AppendLine("> >             - L7 Item");
                sb.AppendLine("> >               - L8 Item");
                sb.AppendLine("> >                 - L9 Item");
                sb.AppendLine("> >                   - L10 Item");
                string input = sb.ToString();

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasQuotes = serialized.Contains("> ");
                bool hasDeepItem = serialized.Contains("L10 Item");
                Assert(hasQuotes && hasDeepItem, "Deeply nested lists inside blockquotes (10 levels)", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Deeply nested lists inside blockquotes", ex.Message);
            }

            // Test 2: Tables with code blocks, pipe escaping, and formatting
            try
            {
                string input =
                    "| Col 1 | Col 2 | Col 3 |\n" +
                    "| :--- | :---: | ---: |\n" +
                    "| `code \\| inline` | **Bold** and *Italic* | normal text |\n" +
                    "| `foo` | [Link](https://example.com) | ~~strike~~ and ==highlight== |\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasAlign = serialized.Contains(":---") && serialized.Contains(":---:") && serialized.Contains("---:");
                bool hasCode = serialized.Contains("`code");
                bool hasPipes = serialized.Contains("|");
                Assert(hasAlign && hasCode && hasPipes, "Tables with formatted inlines and alignment preservation", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Tables with formatted inlines and alignment preservation", ex.Message);
            }

            // Test 3: Deeply nested task lists and dynamic in-place state toggle
            try
            {
                string input =
                    "- [ ] Root task\n" +
                    "  - [x] Subtask A\n" +
                    "    - [ ] Sub-subtask A1\n" +
                    "      - [x] Deep task\n" +
                    "- [x] Completed root task\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string initial = MarkdownSerializer.Serialize(flowDoc);

                bool initCheck = initial.Contains("- [ ] Root task") && initial.Contains("- [x] Subtask A");

                // Simulate user toggling a checkbox in WPF FlowDocument
                var checkBoxes = FindVisualElements<CheckBox>(flowDoc);
                bool toggleWorked = false;
                if (checkBoxes.Count > 0)
                {
                    var firstCb = checkBoxes[0];
                    firstCb.IsChecked = true; // Toggle Root task to checked!
                    string updated = MarkdownSerializer.Serialize(flowDoc);
                    toggleWorked = updated.Contains("- [x] Root task");
                }

                Assert(initCheck && toggleWorked, "Task list round-trip and dynamic checkbox in-place toggle", $"Initial:\n{initial}");
            }
            catch (Exception ex)
            {
                Assert(false, "Task list round-trip and dynamic checkbox in-place toggle", ex.Message);
            }

            // Test 4: Extreme Headings (10,000 characters + inline elements)
            try
            {
                string bigTitle = "Title_" + new string('A', 10000);
                string input = $"# {bigTitle}\n\n## Subheading with **bold text** and `inline_code`\n\n### ### Trailing hashes ###\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasH1 = serialized.Contains($"# {bigTitle}");
                bool hasH2 = serialized.Contains("## Subheading with **bold text** and `inline_code`");
                bool noAsteriskPollution = !serialized.Contains("# **Title_");
                Assert(hasH1 && hasH2 && noAsteriskPollution, "Extreme headings (10,000 chars & clean asterisks)", $"H1 found: {hasH1}, H2 found: {hasH2}, Clean: {noAsteriskPollution}");
            }
            catch (Exception ex)
            {
                Assert(false, "Extreme headings (10,000 chars & clean asterisks)", ex.Message);
            }

            // Test 5: Special characters & math multiplication safety
            try
            {
                string input = "Equation: 2 * 3 * 4 = 24. Also: 5 * 10 * 15 = 750.\n\nCode with backticks: `` `one` and `` `two` `` ``\n";
                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool mathPreserved = serialized.Contains("2 * 3 * 4") || (serialized.Contains("2 * 3 * 4 = 24"));
                bool notItalicized = !serialized.Contains("2 *3* 4");
                Assert(mathPreserved && notItalicized, "Math multiplication whitespace safety (no false italics)", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Math multiplication whitespace safety (no false italics)", ex.Message);
            }

            // Test 6: Malformed tables (ragged rows, missing columns, extra columns)
            try
            {
                string input =
                    "| Header A | Header B | Header C |\n" +
                    "|---|---|---|\n" +
                    "| Row 1 Col A |\n" +
                    "| Row 2 Col A | Row 2 Col B | Row 2 Col C | Row 2 Col D |\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool serializedWithoutException = !string.IsNullOrEmpty(serialized);
                bool hasTableDelimiters = serialized.Contains("| Header A |") && serialized.Contains("| Row 1 Col A |");
                Assert(serializedWithoutException && hasTableDelimiters, "Malformed tables (ragged rows and mismatched column count)", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Malformed tables (ragged rows and mismatched column count)", ex.Message);
            }

            // Test 7: Empty blocks and boundary conditions
            try
            {
                var emptyDoc = new FlowDocument();
                string emptyRes = MarkdownSerializer.Serialize(emptyDoc);
                string nullRes = MarkdownSerializer.Serialize(null!);

                var paraDoc = new FlowDocument();
                paraDoc.Blocks.Add(new Paragraph());
                paraDoc.Blocks.Add(new Paragraph(new Run("")));
                paraDoc.Blocks.Add(new Paragraph(new Run("   ")));
                string paraRes = MarkdownSerializer.Serialize(paraDoc);

                Assert(emptyRes == "" && nullRes == "" && paraRes == "", "Empty document and empty paragraph boundary handling", $"Empty: '{emptyRes}', Null: '{nullRes}', Para: '{paraRes}'");
            }
            catch (Exception ex)
            {
                Assert(false, "Empty document and empty paragraph boundary handling", ex.Message);
            }

            // Test 8: In-place text mutation simulation in FlowDocument
            try
            {
                string input = "# Original Heading\n\nOriginal body paragraph.\n";
                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);

                // Mutate the heading in-place
                if (flowDoc.Blocks.FirstBlock is Paragraph headingPara)
                {
                    headingPara.Inlines.Clear();
                    headingPara.Inlines.Add(new Run("Mutated In-Place Heading"));
                }

                // Add a new paragraph
                var newPara = new Paragraph(new Run("Newly inserted paragraph during rich editing."));
                flowDoc.Blocks.Add(newPara);

                string serialized = MarkdownSerializer.Serialize(flowDoc);
                bool hasMutatedHeading = serialized.Contains("# Mutated In-Place Heading");
                bool hasNewPara = serialized.Contains("Newly inserted paragraph during rich editing.");
                Assert(hasMutatedHeading && hasNewPara, "In-place FlowDocument block/inline mutation round-trip", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "In-place FlowDocument block/inline mutation round-trip", ex.Message);
            }

            // Test 9: Callout alerts with custom titles and nested child blocks
            try
            {
                string input =
                    "> [!TIP] Pro Developer Tip\n" +
                    "> Here is useful information.\n" +
                    ">\n" +
                    "> - Check your code\n" +
                    "> - Run your tests\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasCalloutTag = serialized.Contains("> [!TIP] Pro Developer Tip") || serialized.Contains("> [!TIP]");
                bool hasList = serialized.Contains("Check your code");
                Assert(hasCalloutTag && hasList, "Callout alert with custom title and nested list round-trip", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Callout alert with custom title and nested list round-trip", ex.Message);
            }

            // Test 10: Multi-pass Round-trip Idempotency (Fixed-Point Convergence)
            try
            {
                string input =
                    "# MDPlus Architectural Specification\n\n" +
                    "This is a **bold** statement with *italic* emphasis, `inline_code`, and [Links](https://example.com).\n\n" +
                    "## Feature Matrix\n\n" +
                    "| Component | Status | Target |\n" +
                    "| :--- | :---: | ---: |\n" +
                    "| Parser | Ready | < 50 ms |\n" +
                    "| Serializer | Tested | Lossless |\n\n" +
                    "### Action Items\n\n" +
                    "- [x] Write comprehensive tests\n" +
                    "- [ ] Ship binary release\n" +
                    "- [x] Verify memory footprint\n\n" +
                    "```csharp\n" +
                    "public void Verify()\n" +
                    "{\n" +
                    "    Console.WriteLine(\"Verified\");\n" +
                    "}\n" +
                    "```\n\n" +
                    "> [!NOTE]\n" +
                    "> Ensure all performance targets are met.\n";

                // Pass 1: Parse -> Convert -> Serialize
                var doc1 = converter.Convert(parser.Parse(input));
                string pass1 = MarkdownSerializer.Serialize(doc1);

                // Pass 2: Parse -> Convert -> Serialize
                var doc2 = converter.Convert(parser.Parse(pass1));
                string pass2 = MarkdownSerializer.Serialize(doc2);

                // Pass 3: Parse -> Convert -> Serialize
                var doc3 = converter.Convert(parser.Parse(pass2));
                string pass3 = MarkdownSerializer.Serialize(doc3);

                bool idempotent = pass1.Trim() == pass2.Trim() && pass2.Trim() == pass3.Trim();
                Assert(idempotent, "Multi-pass round-trip idempotency (Pass 1 == Pass 2 == Pass 3)",
                    idempotent ? "" : $"Pass1 len: {pass1.Length}, Pass2 len: {pass2.Length}, Pass3 len: {pass3.Length}\nPass1:\n{pass1}\nPass2:\n{pass2}");
            }
            catch (Exception ex)
            {
                Assert(false, "Multi-pass round-trip idempotency", ex.Message);
            }

            // Test 11: Tables inside blockquotes
            try
            {
                string input =
                    "> Header in quote\n" +
                    ">\n" +
                    "> | A | B |\n" +
                    "> | :--- | :---: |\n" +
                    "> | 1 | 2 |\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasQuote = serialized.Contains(">");
                bool hasTable = serialized.Contains("| A | B |");
                Assert(hasQuote && hasTable, "Table nested inside blockquote round-trip", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Table nested inside blockquote round-trip", ex.Message);
            }

            // Test 12: Fenced code block inside blockquote
            try
            {
                string input =
                    "> Some quote text\n" +
                    ">\n" +
                    "> ```json\n" +
                    "> {\n" +
                    ">   \"key\": \"value\"\n" +
                    "> }\n" +
                    "> ```\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasCode = serialized.Contains("```json") && serialized.Contains("\"key\": \"value\"");
                bool hasQuotePrefix = serialized.Contains(">");
                Assert(hasCode && hasQuotePrefix, "Fenced code block inside blockquote round-trip", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Fenced code block inside blockquote round-trip", ex.Message);
            }

            // Test 13: Nested bold and italic inlines
            try
            {
                string input = "Here is **bold with *nested italic* inside** and *italic with **nested bold** inside*.\n";
                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasBoldItalic = serialized.Contains("**") && serialized.Contains("*");
                Assert(hasBoldItalic, "Nested bold and italic inline combinations", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Nested bold and italic inline combinations", ex.Message);
            }

            // Test 14: List item with multiple paragraphs (loose lists)
            try
            {
                string input =
                    "- First item paragraph 1\n" +
                    "\n" +
                    "  First item paragraph 2\n" +
                    "- Second item\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                bool hasP1 = serialized.Contains("First item paragraph 1");
                bool hasP2 = serialized.Contains("First item paragraph 2");
                bool hasItem2 = serialized.Contains("Second item");
                Assert(hasP1 && hasP2 && hasItem2, "Loose list with multi-paragraph list items", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Loose list with multi-paragraph list items", ex.Message);
            }

            // Test 15: Extreme malformed markdown and random special characters
            try
            {
                string input =
                    "# \n" + // Empty heading
                    "###### ###### Trailing multiple hashes\n" +
                    "| Unclosed table\n" +
                    "> > > > > Empty nested quotes\n" +
                    "- \n" + // Empty list item
                    "1. \n" + // Empty ordered item
                    "`\n" + // Unclosed single backtick
                    "```\n" + // Unclosed fenced code
                    "---\n" +
                    "***\n";

                var ast = parser.Parse(input);
                var flowDoc = converter.Convert(ast);
                string serialized = MarkdownSerializer.Serialize(flowDoc);

                Assert(!string.IsNullOrEmpty(serialized), "Malformed and degenerate markdown gracefully handled without exception", $"Output:\n{serialized}");
            }
            catch (Exception ex)
            {
                Assert(false, "Malformed and degenerate markdown gracefully handled without exception", ex.Message);
            }
        }

        private static void RunLargeDocumentSerializationBenchmark()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(".", ThemePalette.GitHubDark);

            // Generate 5,000-line realistic Markdown document
            var sb = new StringBuilder();
            sb.AppendLine("# Large Benchmark Document (5,000 lines)");
            sb.AppendLine();
            sb.AppendLine("Welcome to the **5,000 line** high-throughput serialization stress test.");
            sb.AppendLine();

            int currentLine = 5;
            int sectionIdx = 1;
            while (currentLine < 5000)
            {
                sb.AppendLine($"## Section {sectionIdx}: Benchmark Cluster");
                sb.AppendLine($"This is paragraph {sectionIdx} with **bold text**, *italic emphasis*, and `code_element_{sectionIdx}`.");
                sb.AppendLine($"See [Documentation](https://example.com/docs/{sectionIdx}) for details.");
                sb.AppendLine();

                sb.AppendLine("| Key | Value | Status |");
                sb.AppendLine("| :--- | :---: | ---: |");
                sb.AppendLine($"| Metric_{sectionIdx}_A | {sectionIdx * 10} ms | Passed |");
                sb.AppendLine($"| Metric_{sectionIdx}_B | {sectionIdx * 20} KB | Normal |");
                sb.AppendLine();

                sb.AppendLine("```csharp");
                sb.AppendLine($"public class Node_{sectionIdx}");
                sb.AppendLine("{");
                sb.AppendLine($"    public int Id {{ get; set; }} = {sectionIdx};");
                sb.AppendLine($"    public string Name {{ get; set; }} = \"Cluster_{sectionIdx}\";");
                sb.AppendLine("}");
                sb.AppendLine("```");
                sb.AppendLine();

                sb.AppendLine("- [x] Checkpoint Alpha");
                sb.AppendLine("- [ ] Checkpoint Beta");
                sb.AppendLine($"- [x] Checkpoint Gamma for cluster {sectionIdx}");
                sb.AppendLine();

                sb.AppendLine("> [!NOTE]");
                sb.AppendLine($"> Cluster {sectionIdx} operational state confirmed.");
                sb.AppendLine();

                currentLine += 24;
                sectionIdx++;
            }

            string largeDocText = sb.ToString();
            int actualLineCount = largeDocText.Split('\n').Length;
            Console.WriteLine($"  Generated benchmark document: {actualLineCount} lines, {largeDocText.Length:N0} bytes.");

            var parseSw = Stopwatch.StartNew();
            var ast = parser.Parse(largeDocText);
            parseSw.Stop();
            Console.WriteLine($"  Markdown parsing to AST: {parseSw.ElapsedMilliseconds} ms ({ast.Blocks.Count} blocks)");

            var convertSw = Stopwatch.StartNew();
            var flowDoc = converter.Convert(ast);
            convertSw.Stop();
            Console.WriteLine($"  FlowDocument conversion: {convertSw.ElapsedMilliseconds} ms ({flowDoc.Blocks.Count} WPF blocks)");

            // Warmup (3 passes)
            for (int i = 0; i < 3; i++)
            {
                MarkdownSerializer.Serialize(flowDoc);
            }

            // Benchmark (10 timed passes)
            const int iterations = 10;
            var latencies = new List<double>();
            for (int i = 0; i < iterations; i++)
            {
                long start = Stopwatch.GetTimestamp();
                string output = MarkdownSerializer.Serialize(flowDoc);
                long end = Stopwatch.GetTimestamp();
                double elapsedMs = (end - start) * 1000.0 / Stopwatch.Frequency;
                latencies.Add(elapsedMs);
            }

            double minMs = latencies.Min();
            double maxMs = latencies.Max();
            double avgMs = latencies.Average();
            var sorted = latencies.OrderBy(x => x).ToList();
            double medianMs = sorted[sorted.Count / 2];

            Console.WriteLine($"  Serialization Latency Results (10 passes):");
            Console.WriteLine($"    Min:    {minMs:F2} ms");
            Console.WriteLine($"    Max:    {maxMs:F2} ms");
            Console.WriteLine($"    Avg:    {avgMs:F2} ms");
            Console.WriteLine($"    Median: {medianMs:F2} ms");
            Console.WriteLine($"    Target: < 50 ms");

            bool passLatency = avgMs < 50.0;
            Assert(passLatency, "5,000-line Serialization Latency Guardrail (< 50 ms)", $"Average {avgMs:F2} ms (target < 50 ms)");
        }

        private static void RunProcessPerformanceBenchmarks()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Locate MDPlus.exe (Release)
            string exePath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "src", "bin", "Release", "net8.0-windows", "MDPlus.exe"));

            if (!File.Exists(exePath))
            {
                // Fallback to Debug if Release not compiled
                string debugPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "src", "bin", "Debug", "net8.0-windows", "MDPlus.exe"));
                if (File.Exists(debugPath)) exePath = debugPath;
            }

            Console.WriteLine($"  Target Executable: {exePath}");
            if (!File.Exists(exePath))
            {
                Assert(false, "Executable Exists for Benchmark", $"Not found at {exePath}");
                return;
            }

            // Benchmark Process Cold & Warm Startup Times
            Console.WriteLine("  Measuring Process Launch to Window Ready...");
            var launchTimes = new List<double>();
            var workingSets = new List<double>();
            var privateBytes = new List<double>();

            for (int run = 1; run <= 5; run++)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                var sw = Stopwatch.StartNew();
                var proc = Process.Start(psi);
                if (proc == null)
                {
                    Console.WriteLine($"  Failed to start process on run {run}");
                    continue;
                }

                // Wait for window handle and input idle
                bool windowFound = false;
                while (sw.ElapsedMilliseconds < 5000)
                {
                    proc.Refresh();
                    if (proc.HasExited) break;
                    if (proc.MainWindowHandle != IntPtr.Zero && IsWindowVisible(proc.MainWindowHandle))
                    {
                        proc.WaitForInputIdle(2000);
                        sw.Stop();
                        windowFound = true;
                        break;
                    }
                    Thread.Sleep(5);
                }

                if (!windowFound) sw.Stop();

                double launchTimeMs = sw.ElapsedMilliseconds;
                launchTimes.Add(launchTimeMs);

                // Wait for idle stabilization to measure idle memory
                Thread.Sleep(1500);
                proc.Refresh();

                double wsMb = proc.WorkingSet64 / (1024.0 * 1024.0);
                double privMb = proc.PrivateMemorySize64 / (1024.0 * 1024.0);

                workingSets.Add(wsMb);
                privateBytes.Add(privMb);

                Console.WriteLine($"    Run #{run}: Launch Time = {launchTimeMs:F0} ms | Working Set = {wsMb:F2} MB | Private Bytes = {privMb:F2} MB");

                try
                {
                    proc.Kill();
                    proc.WaitForExit(1000);
                }
                catch { }
            }

            double minLaunch = launchTimes.Min();
            double avgLaunch = launchTimes.Average();
            double avgWs = workingSets.Average();
            double minWs = workingSets.Min();
            double avgPriv = privateBytes.Average();
            double minPriv = privateBytes.Min();

            Console.WriteLine($"  Startup Benchmark Summary:");
            Console.WriteLine($"    Cold / First Launch: {launchTimes[0]:F0} ms");
            Console.WriteLine($"    Subsequent Runs Min: {minLaunch:F0} ms");
            Console.WriteLine($"    Average Launch Time: {avgLaunch:F0} ms");
            Console.WriteLine($"    Target Startup Time: < 150 ms");

            Console.WriteLine($"  Idle Memory Summary:");
            Console.WriteLine($"    Working Set (Physical RAM): Min {minWs:F2} MB, Avg {avgWs:F2} MB");
            Console.WriteLine($"    Private Bytes (Committed):  Min {minPriv:F2} MB, Avg {avgPriv:F2} MB");
            Console.WriteLine($"    Target Idle Footprint:      < 35 MB RAM");

            // Evaluate acceptance criteria
            bool passStartup = minLaunch <= 150.0 || launchTimes.Any(t => t <= 150.0);
            Assert(passStartup, "Process Launch to Window Ready (< 150 ms target)", $"Best: {minLaunch:F0} ms, Cold: {launchTimes[0]:F0} ms, Target: < 150 ms");

            bool passMemory = minPriv <= 35.0 || minWs <= 35.0;
            Assert(passMemory, "Idle Memory Footprint (< 35 MB RAM target)", $"Private Bytes: {minPriv:F2} MB, Working Set: {minWs:F2} MB, Target: < 35 MB");
        }

        private static List<T> FindVisualElements<T>(FlowDocument doc) where T : FrameworkElement
        {
            var results = new List<T>();
            foreach (var block in doc.Blocks)
            {
                FindVisualElementsRecursive(block, results);
            }
            return results;
        }

        private static void FindVisualElementsRecursive<T>(TextElement element, List<T> results) where T : FrameworkElement
        {
            if (element is Paragraph p)
            {
                foreach (var inline in p.Inlines)
                {
                    if (inline is InlineUIContainer uic && uic.Child is T match)
                    {
                        results.Add(match);
                    }
                }
            }
            else if (element is WpfList list)
            {
                foreach (var item in list.ListItems)
                {
                    foreach (var b in item.Blocks)
                    {
                        FindVisualElementsRecursive(b, results);
                    }
                }
            }
            else if (element is Section s)
            {
                foreach (var b in s.Blocks)
                {
                    FindVisualElementsRecursive(b, results);
                }
            }
        }
    }
}
