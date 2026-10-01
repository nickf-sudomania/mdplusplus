using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MDPlus.Core;
using MDPlus.Core.Mermaid;
using MDPlus.E2E.Harness;
using MDPlus.Models;
using static MDPlus.E2E.Harness.E2ETestHarness;
using WpfTable = System.Windows.Documents.Table;

namespace MDPlus.E2E.Tiers
{
    public static class Tier5_AdversarialHardening
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 5: Adversarial Hardening (Challenger)");
            Console.WriteLine("==================================================");

            RunTest("Tier5", "T5.1: WCAG AA contrast ratio matrix across all 8 theme presets", TestWCAGContrastMatrixAll8Palettes);
            RunTest("Tier5", "T5.2: Multi-resolution icon mipmap binary inspection and decoders", TestAppIconMipmapFramesAndDecoders);
            RunTest("Tier5", "T5.3: Rapid theme cycling (100 loops / 800 switches) & brush immutability", TestRapidThemeCycling100LoopsThroughAllPresets);
            RunTest("Tier5", "T5.4: Tab lifecycle stress, dirty transitions, and theme resilience", TestTabLifecycleStressAndDirtyTransitions);
            RunTest("Tier5", "T5.5: TOC sidebar high contrast readability and theme menu grouping", TestTocThemeReadabilityAndMenuGrouping);

            // MDPlus v1.09 Multi-Format Adversarial Stress Tests
            RunTest("Tier5", "T5.6: Adversarial CSV with unbalanced quotes, null bytes, and extreme field lengths", TestAdversarialCsvParsingAndRecovery);
            RunTest("Tier5", "T5.7: Adversarial JSON with deeply nested structures, unicode escapes, and extreme numbers", TestAdversarialJsonParsingAndHighlighting);

            // MDPlus Mermaid & Inline Math Typography Adversarial Hardening (Challenger M4-2)
            RunTest("Tier5", "T5.8: Math typography zero-sum margin invariance across Paragraph, List, Headings H1-H6, and Table", TestMathTypographyZeroSumMarginInvariance);
            RunTest("Tier5", "T5.9: Financial valuation expressions ($ROIC > 18\\%$, $\\ge +1.5\\%$, $\\le 30\\times$) optical layout and non-elevation", TestFinancialValuationExpressionsOpticalBounds);
            RunTest("Tier5", "T5.10: Extreme math inputs (multilevel fractions, exponent towers, deep subscripts, large operators)", TestExtremeMathInputsZeroClipping);
            RunTest("Tier5", "T5.11: Currency expressions ($100 collision vs \\$100 math) and adversarial LaTeX syntax resilience", TestCurrencyCollisionAndMalformedLatexResilience);
            RunTest("Tier5", "T5.12: 20-node flowchart layout (< 20ms) and render (< 25ms) benchmark with orientation & scale stress", TestMermaid20NodeBenchmarkAndScaleStress);
            RunTest("Tier5", "T5.13: Rapid theme switching stress with active rendered math and Mermaid visuals", TestRapidThemeSwitchingWithMathAndMermaid);
        }

        private static void TestWCAGContrastMatrixAll8Palettes()
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

            foreach (var preset in presets)
            {
                var palette = ThemePalette.GetPalette(preset);

                // 1. Text vs Background
                double textBgContrast = ThemePalette.CalculateContrast(palette.EditorFg.Color, palette.EditorBg.Color);
                AssertTrue(textBgContrast >= 4.5, $"{preset} Text vs Background ({textBgContrast:F2}:1) must be >= 4.5:1");

                // 2. Menu Item Text vs Menu Background
                double menuContrast = ThemePalette.CalculateContrast(palette.MenuFg.Color, palette.MenuBg.Color);
                AssertTrue(menuContrast >= 4.5, $"{preset} Menu Text vs Menu Bg ({menuContrast:F2}:1) must be >= 4.5:1");

                // 3. Menu Item Text vs Menu Hover Background
                // 3a. MenuHoverFg (active hover text) vs MenuHoverBg
                double hoverFgContrast = ThemePalette.CalculateContrast(palette.MenuHoverFg.Color, palette.MenuHoverBg.Color);
                AssertTrue(hoverFgContrast >= 4.5, $"{preset} Menu Hover Text vs Menu Hover Bg ({hoverFgContrast:F2}:1) must be >= 4.5:1");

                // 4. Status Bar Text vs Status Bar Background
                double statusContrast = ThemePalette.CalculateContrast(palette.StatusFg.Color, palette.StatusBg.Color);
                AssertTrue(statusContrast >= 4.5, $"{preset} Status Text vs Status Bg ({statusContrast:F2}:1) must be >= 4.5:1");

                // 5. Table Header Text vs Table Header Background
                // HeadingFg is used for table header row text
                double tableHeaderContrast = ThemePalette.CalculateContrast(palette.HeadingFg.Color, palette.TableHeaderBg.Color);
                AssertTrue(tableHeaderContrast >= 4.5, $"{preset} Table Header Text (HeadingFg) vs Table Header Bg ({tableHeaderContrast:F2}:1) must be >= 4.5:1");

                // Also verify EditorFg vs TableHeaderBg
                double tableEditorContrast = ThemePalette.CalculateContrast(palette.EditorFg.Color, palette.TableHeaderBg.Color);
                AssertTrue(tableEditorContrast >= 4.5, $"{preset} Table EditorFg vs Table Header Bg ({tableEditorContrast:F2}:1) must be >= 4.5:1");

                // 6. Sidebar / TOC Text vs Sidebar Background (WCAG AA >= 4.5:1)
                double tocHeadingContrast = ThemePalette.CalculateContrast(palette.HeadingFg.Color, palette.SidebarBg.Color);
                AssertTrue(tocHeadingContrast >= 4.5, $"{preset} TOC HeadingFg vs SidebarBg ({tocHeadingContrast:F2}:1) must be >= 4.5:1");

                double tocEditorContrast = ThemePalette.CalculateContrast(palette.EditorFg.Color, palette.SidebarBg.Color);
                AssertTrue(tocEditorContrast >= 4.5, $"{preset} TOC EditorFg vs SidebarBg ({tocEditorContrast:F2}:1) must be >= 4.5:1");

                // 7. Sidebar MutedFg (header title / close button) vs MenuBg (header border) (WCAG AA >= 4.5:1)
                double headerTitleContrast = ThemePalette.CalculateContrast(palette.MutedFg.Color, palette.MenuBg.Color);
                AssertTrue(headerTitleContrast >= 4.5, $"{preset} Sidebar Header Title (MutedFg) vs MenuBg ({headerTitleContrast:F2}:1) must be >= 4.5:1");
            }
        }

        private static void TestAppIconMipmapFramesAndDecoders()
        {
            string repoRoot = GetRepositoryRoot();
            string iconPath = Path.Combine(repoRoot, "src", "Resources", "AppIcon.ico");

            AssertTrue(File.Exists(iconPath), $"AppIcon.ico must exist at {iconPath}");
            byte[] bytes = File.ReadAllBytes(iconPath);
            AssertTrue(bytes.Length > 20000, $"AppIcon.ico size ({bytes.Length} bytes) must be > 20KB to contain 4 resolutions.");

            // 1. Binary header inspection
            var entries = ParseIco(bytes);
            AssertEqual(4, entries.Count, "AppIcon.ico must contain exactly 4 mipmap directory entries.");

            var widths = entries.Select(e => e.Width).OrderBy(w => w).ToList();
            AssertEqual(16, widths[0], "First mipmap width must be 16.");
            AssertEqual(32, widths[1], "Second mipmap width must be 32.");
            AssertEqual(48, widths[2], "Third mipmap width must be 48.");
            AssertEqual(256, widths[3], "Fourth mipmap width must be 256.");

            foreach (var entry in entries)
            {
                AssertEqual(32, entry.BitCount, $"Mipmap {entry.Width}x{entry.Height} must have 32 bpp color depth.");
                AssertTrue(entry.ImageOffset > 0, $"Mipmap {entry.Width} image offset must be valid.");
                AssertTrue(entry.BytesInRes > 0, $"Mipmap {entry.Width} resource size must be > 0.");
            }

            // Verify PNG signature on 256x256 entry
            var entry256 = entries.First(e => e.Width == 256);
            AssertTrue(entry256.ImageOffset + 8 <= bytes.Length, "256x256 offset must fit in file.");
            bool isPng = bytes[entry256.ImageOffset] == 0x89 &&
                         bytes[entry256.ImageOffset + 1] == 0x50 &&
                         bytes[entry256.ImageOffset + 2] == 0x4E &&
                         bytes[entry256.ImageOffset + 3] == 0x47;
            AssertTrue(isPng, "256x256 frame payload must have valid PNG magic signature.");

            // 2. WPF IconBitmapDecoder verification
            using var ms = new MemoryStream(bytes);
            var decoder = new IconBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
            AssertEqual(4, decoder.Frames.Count, "IconBitmapDecoder must parse exactly 4 frames.");

            var decodedSizes = decoder.Frames.Select(f => f.PixelWidth).OrderBy(s => s).ToList();
            AssertEqual(16, decodedSizes[0]);
            AssertEqual(32, decodedSizes[1]);
            AssertEqual(48, decodedSizes[2]);
            AssertEqual(256, decodedSizes[3]);

            foreach (var frame in decoder.Frames)
            {
                AssertEqual(PixelFormats.Bgra32, frame.Format, $"Frame {frame.PixelWidth}x{frame.PixelHeight} must be Bgra32.");
            }
        }

        private static void TestRapidThemeCycling100LoopsThroughAllPresets()
        {
            var tm = ThemeManager.Instance;
            AssertNotNull(tm);

            var parser = new MarkdownParser();
            string sampleMarkdown = @"# Stress Test Document
Here is **bold** text and `inline code`.

| Name | Role | Status |
| :--- | :---: | ---: |
| Alpha | Admin | Active |
| Beta | Guest | Pending |

```csharp
public static void Main() => Console.WriteLine(""Stress"");
```

> [!NOTE]
> Testing rapid theme toggling under heavy FlowDocument load.

- [x] Item 1
- [ ] Item 2
";
            var doc = parser.Parse(sampleMarkdown);

            // Pre-create 10 document tabs with rendered FlowDocuments
            var tabs = new List<DocumentTabItem>();
            for (int t = 0; t < 10; t++)
            {
                var tab = new DocumentTabItem
                {
                    FilePath = $@"C:\temp\stress_doc_{t}.md",
                    Title = $"stress_doc_{t}.md",
                    RawMarkdown = sampleMarkdown,
                    Document = doc
                };
                var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, tm.CurrentPalette);
                tab.FlowDocument = converter.Convert(doc);
                tabs.Add(tab);
            }

            // Force initial GC to establish baseline memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long memoryBefore = GC.GetTotalMemory(true);

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

            var sw = Stopwatch.StartNew();
            int totalSwitches = 0;

            // Execute 100 full loops through all 8 presets = 800 theme changes
            for (int loop = 0; loop < 100; loop++)
            {
                for (int p = 0; p < presets.Length; p++)
                {
                    var targetPreset = presets[p];
                    tm.SetPreset(targetPreset);
                    totalSwitches++;

                    AssertEqual(targetPreset, tm.CurrentPreset, $"CurrentPreset must match {targetPreset}");
                    var currentPalette = tm.CurrentPalette;

                    // Verify frozen brush immutability - must NEVER throw InvalidOperationException
                    AssertTrue(currentPalette.WindowBg.IsFrozen, "WindowBg brush must remain frozen");
                    AssertTrue(currentPalette.EditorBg.IsFrozen, "EditorBg brush must remain frozen");
                    AssertTrue(currentPalette.EditorFg.IsFrozen, "EditorFg brush must remain frozen");
                    AssertTrue(currentPalette.MenuBg.IsFrozen, "MenuBg brush must remain frozen");
                    AssertTrue(currentPalette.MenuFg.IsFrozen, "MenuFg brush must remain frozen");
                    AssertTrue(currentPalette.MenuHoverBg.IsFrozen, "MenuHoverBg brush must remain frozen");
                    AssertTrue(currentPalette.MenuHoverFg.IsFrozen, "MenuHoverFg brush must remain frozen");
                    AssertTrue(currentPalette.StatusBg.IsFrozen, "StatusBg brush must remain frozen");
                    AssertTrue(currentPalette.StatusFg.IsFrozen, "StatusFg brush must remain frozen");
                    AssertTrue(currentPalette.TableHeaderBg.IsFrozen, "TableHeaderBg brush must remain frozen");
                    AssertTrue(currentPalette.HeadingFg.IsFrozen, "HeadingFg brush must remain frozen");
                    AssertTrue(currentPalette.CodeBg.IsFrozen, "CodeBg brush must remain frozen");
                    AssertTrue(currentPalette.Accent.IsFrozen, "Accent brush must remain frozen");

                    // Re-render FlowDocuments for open tabs to stress allocation and rendering
                    if (loop % 10 == 0 && p == 0)
                    {
                        foreach (var tab in tabs)
                        {
                            var tabConverter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, currentPalette);
                            tab.FlowDocument = tabConverter.Convert(tab.Document);
                            AssertNotNull(tab.FlowDocument);
                            AssertEqual(currentPalette.EditorFg.Color, ((SolidColorBrush)tab.FlowDocument.Foreground).Color);
                        }
                    }
                }
            }

            sw.Stop();
            AssertEqual(800, totalSwitches, "Should have completed exactly 800 theme switches.");

            // Post-stress GC and memory verification
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long memoryAfter = GC.GetTotalMemory(true);
            long memoryGrowthBytes = memoryAfter - memoryBefore;

            // Memory growth should be negligible (< 10 MB after 500 theme changes)
            AssertTrue(memoryGrowthBytes < 10 * 1024 * 1024,
                $"Memory growth after 500 theme switches must be < 10MB. Grew: {memoryGrowthBytes / 1024} KB.");

            // Restore baseline preset
            tm.SetPreset(ThemePreset.GitHubDark);
        }

        private static void TestTabLifecycleStressAndDirtyTransitions()
        {
            var tm = ThemeManager.Instance;
            tm.SetPreset(ThemePreset.GitHubDark);

            var parser = new MarkdownParser();
            var tabs = new List<DocumentTabItem>();

            // 1. Rapidly open 50 tabs
            for (int i = 0; i < 50; i++)
            {
                string md = $"# Document {i}\n\nContent for doc {i}.";
                var doc = parser.Parse(md);
                var tab = new DocumentTabItem
                {
                    FilePath = $@"C:\workspace\file_{i}.md",
                    Title = $"file_{i}.md",
                    RawMarkdown = md,
                    Document = doc
                };
                tabs.Add(tab);
            }

            AssertEqual(50, tabs.Count);

            // 2. Mark odd tabs dirty
            for (int i = 0; i < tabs.Count; i++)
            {
                if (i % 2 == 1)
                {
                    tabs[i].MarkDirty();
                    AssertTrue(tabs[i].IsDirty, $"Tab {i} must be dirty.");
                    AssertEqual($"file_{i}.md *", tabs[i].DisplayTitle, $"Tab {i} DisplayTitle must include asterisk.");
                }
                else
                {
                    AssertFalse(tabs[i].IsDirty, $"Tab {i} must not be dirty.");
                    AssertEqual($"file_{i}.md", tabs[i].DisplayTitle, $"Tab {i} DisplayTitle must not include asterisk.");
                }
            }

            // 3. Switch themes while tabs have dirty state
            tm.CycleNextTheme(); // GitHubLight
            AssertEqual(ThemePreset.GitHubLight, tm.CurrentPreset);

            // Verify dirty states are preserved across theme switch
            for (int i = 0; i < tabs.Count; i++)
            {
                if (i % 2 == 1)
                {
                    AssertTrue(tabs[i].IsDirty, $"Tab {i} dirty state preserved after theme change.");
                    AssertEqual($"file_{i}.md *", tabs[i].DisplayTitle);
                }
                else
                {
                    AssertFalse(tabs[i].IsDirty);
                    AssertEqual($"file_{i}.md", tabs[i].DisplayTitle);
                }
            }

            // 4. Mark all clean and verify
            foreach (var tab in tabs)
            {
                tab.MarkClean();
                AssertFalse(tab.IsDirty);
                AssertFalse(tab.DisplayTitle.EndsWith(" *"));
            }

            // 5. Stress close tabs in reverse order
            while (tabs.Count > 0)
            {
                var lastTab = tabs[tabs.Count - 1];
                tabs.RemoveAt(tabs.Count - 1);
                AssertNotNull(lastTab);
            }

            AssertEqual(0, tabs.Count, "All tabs must be closed cleanly.");

            // Reset preset
            tm.SetPreset(ThemePreset.GitHubDark);
        }

        private static void TestTocThemeReadabilityAndMenuGrouping()
        {
            // 1. Verify GitHub Light TOC contrast exceeds 7:1 (WCAG AAA)
            var ghLight = ThemePalette.GitHubLight;
            double ghHeadingContrast = ThemePalette.CalculateContrast(ghLight.SidebarBg.Color, ghLight.HeadingFg.Color);
            double ghEditorContrast = ThemePalette.CalculateContrast(ghLight.SidebarBg.Color, ghLight.EditorFg.Color);
            AssertTrue(ghHeadingContrast >= 7.0, $"GitHub Light Heading contrast ({ghHeadingContrast:F2}:1) must exceed WCAG AAA >= 7:1");
            AssertTrue(ghEditorContrast >= 7.0, $"GitHub Light Editor contrast ({ghEditorContrast:F2}:1) must exceed WCAG AAA >= 7:1");

            // 2. Verify all Light Themes have high TOC contrast
            foreach (var preset in ThemePalette.LightPresets)
            {
                var palette = ThemePalette.GetPalette(preset);
                double hContrast = ThemePalette.CalculateContrast(palette.SidebarBg.Color, palette.HeadingFg.Color);
                double eContrast = ThemePalette.CalculateContrast(palette.SidebarBg.Color, palette.EditorFg.Color);
                AssertTrue(hContrast >= 4.5, $"{preset} TOC Heading contrast ({hContrast:F2}:1) must satisfy WCAG AA >= 4.5:1");
                AssertTrue(eContrast >= 4.5, $"{preset} TOC Editor contrast ({eContrast:F2}:1) must satisfy WCAG AA >= 4.5:1");
            }

            // 3. Verify MainWindow.xaml markup includes grouped submenus
            string repoRoot = GetRepositoryRoot();
            string xamlPath = Path.Combine(repoRoot, "src", "MainWindow.xaml");
            AssertTrue(File.Exists(xamlPath), $"MainWindow.xaml must exist at {xamlPath}");
            string xaml = File.ReadAllText(xamlPath);

            AssertTrue(xaml.Contains("Name=\"ThemeDarkThemesMenu\""), "MainWindow.xaml must include ThemeDarkThemesMenu");
            AssertTrue(xaml.Contains("Name=\"ThemeLightThemesMenu\""), "MainWindow.xaml must include ThemeLightThemesMenu");
            AssertTrue(xaml.Contains("Name=\"HamburgerThemeDarkThemesMenu\""), "MainWindow.xaml must include HamburgerThemeDarkThemesMenu");
            AssertTrue(xaml.Contains("Name=\"HamburgerThemeLightThemesMenu\""), "MainWindow.xaml must include HamburgerThemeLightThemesMenu");
            AssertTrue(xaml.Contains("Header=\"_GitHub Light\""), "MainMenu ThemeGitHubLightItem access key is _G");
            AssertTrue(xaml.Contains("Header=\"_One Light\""), "MainMenu ThemeOneLightItem access key is _O");
            AssertTrue(xaml.Contains("Value=\"{DynamicResource HeadingForegroundBrush}\""), "TOC Level 1 must use DynamicResource HeadingForegroundBrush");
            AssertTrue(xaml.Contains("Name=\"SidebarHeaderBorder\""), "SidebarHeaderBorder must exist for dynamic theming");
            AssertTrue(xaml.Contains("ToolTip=\"{Binding Text}\""), "TOC item template provides ToolTip for truncated headings");
            AssertTrue(xaml.Contains("Name=\"ContentSplitter\"") && xaml.Contains("Background=\"{DynamicResource BorderBrush}\""), "ContentSplitter must use dynamic BorderBrush");
        }

        #region MDPlus v1.09 Multi-Format Adversarial Stress Tests

        private static void TestAdversarialCsvParsingAndRecovery()
        {
            // 1. Unclosed quote at EOF
            string unclosedAtEof = "col1,col2\r\nval1,\"unclosed quote at the end of stream";
            var rows1 = CsvParser.Parse(unclosedAtEof);
            AssertEqual(2, rows1.Count, "Parser must recover from unclosed quote at EOF without hang or crash.");
            AssertEqual("unclosed quote at the end of stream", rows1[1][1]);

            // 2. Extreme field length (50,000 characters in a single cell)
            string massiveField = new string('Z', 50000);
            string extremeCsv = $"H1,H2\r\n\"prefix_{massiveField}_suffix\",normal";
            var rows2 = CsvParser.Parse(extremeCsv);
            AssertEqual(2, rows2.Count);
            AssertEqual(50000 + 14, rows2[1][0].Length);

            // 3. Serializer round-trip on extreme field
            var doc = CsvToFlowDocumentConverter.Convert(extremeCsv, ThemePalette.GitHubDark);
            string serialized = CsvSerializer.Serialize(doc, ',');
            var rowsSerialized = CsvParser.Parse(serialized);
            AssertEqual(rows2[1][0], rowsSerialized[1][0], "Massive field serialized and parsed back losslessly.");

            // 4. Mid-field unescaped quote handling (Vulnerability A remediation)
            string midField = "Item,Description\r\nPipe,12\" steel pipe\r\na,b\"c,d\r\ne,f,g";
            var rows4 = CsvParser.Parse(midField);
            AssertEqual(4, rows4.Count, "Mid-field quote must preserve all rows without collapsing.");
            AssertEqual("12\" steel pipe", rows4[1][1]);
            AssertEqual("b\"c", rows4[2][1]);
            AssertEqual("e", rows4[3][0]);
        }

        private static void TestAdversarialJsonParsingAndHighlighting()
        {
            var palette = ThemePalette.GitHubDark;

            // 1. Deeply nested JSON structure (20 levels)
            var sb = new StringBuilder();
            for (int i = 0; i < 20; i++) sb.Append($"{{\"level_{i}\": ");
            sb.Append("42");
            for (int i = 0; i < 20; i++) sb.Append("}");
            string deeplyNested = sb.ToString();

            var doc1 = JsonToFlowDocumentConverter.Convert(deeplyNested, palette);
            AssertNotNull(doc1, "Deeply nested JSON converts to FlowDocument without stack overflow.");
            AssertTrue(doc1.Blocks.Count > 10, "Formatted nested JSON contains multiple indented paragraphs.");

            // 2. Unicode escapes and control characters
            string unicodeEscapes = "{\n  \"greeting\": \"\\u0048\\u0065\\u006c\\u006c\\u006f\\u0020\\u0057\\u006f\\u0072\\u006c\\u0064\",\n  \"extreme_num\": 1.7976931348623157E+308\n}";
            var doc2 = JsonToFlowDocumentConverter.Convert(unicodeEscapes, palette);
            AssertNotNull(doc2);
            var runs = doc2.Blocks.OfType<Paragraph>().SelectMany(p => p.Inlines.OfType<Run>()).ToList();
            AssertTrue(runs.Any(r => r.Text.Contains("Hello World") || r.Text.Contains("\\u0048")), "Unicode escaped strings handled.");

            // 3. 5,000-line JSON FlowDocument layout capping at 2,500 visual lines (Vulnerability B remediation)
            var sbLarge = new StringBuilder(5000 * 30);
            sbLarge.Append("[\n");
            for (int i = 1; i <= 4998; i++)
            {
                sbLarge.Append($"  \"line_{i}\",\n");
            }
            sbLarge.Append("  \"line_4999\"\n]");
            string largeJson = sbLarge.ToString();

            var swLayout = Stopwatch.StartNew();
            var docLarge = JsonToFlowDocumentConverter.Convert(largeJson, palette);
            swLayout.Stop();

            AssertNotNull(docLarge);
            AssertEqual(2501, docLarge.Blocks.Count, "Large JSON must cap visual blocks at MaxVisualLines (2500) + 1 banner.");
            AssertTrue(swLayout.ElapsedMilliseconds < 500, $"Layout conversion took {swLayout.ElapsedMilliseconds}ms, must not freeze UI thread.");
            var banner = docLarge.Blocks.LastBlock as Paragraph;
            AssertNotNull(banner);
            var bannerText = string.Concat(banner!.Inlines.OfType<Run>().Select(r => r.Text));
            AssertTrue(bannerText.Contains("Showing first 2,500 of 5,001 lines") && bannerText.Contains("Ctrl+3"), "Banner indicates cap and directs to Raw view Ctrl+3.");
        }

        #endregion

        #region MDPlus Mermaid & Inline Math Typography Adversarial Hardening (Challenger M4-2)

        /// <summary>
        /// T5.8: Validates zero-sum margin invariance (Top + Bottom == 0) and BaselineAlignment.Center
        /// across all markdown block contexts: Paragraph (LineHeight=24), ListItem (LineHeight=NaN),
        /// Headings (H1 through H6 with respective font sizes 26, 20, 17, 15, 13.5, 12.5), and GFM Tables.
        /// </summary>
        private static void TestMathTypographyZeroSumMarginInvariance()
        {
            var parser = new MarkdownParser();
            var palette = ThemePalette.GitHubDark;
            var converter = new MarkdownToWpfConverter("", palette, enableLatex: true, enableHtml: true);

            // 1. Paragraph context (LineHeight = 24)
            string paraMd = "Standard paragraph with inline formula $A = k \\times B$ and $ROIC > 18\\%$ text.";
            var paraDoc = converter.Convert(parser.Parse(paraMd));
            var para = paraDoc.Blocks.OfType<Paragraph>().FirstOrDefault();
            AssertNotNull(para, "Paragraph must exist");
            AssertEqual(24.0, para!.LineHeight, "Paragraph LineHeight must be 24.0");

            var paraContainers = para.Inlines.OfType<InlineUIContainer>().ToList();
            AssertEqual(2, paraContainers.Count, "Paragraph must contain 2 math containers");
            foreach (var uic in paraContainers)
            {
                AssertEqual(BaselineAlignment.Center, uic.BaselineAlignment, "Container BaselineAlignment must be Center");
                AssertTrue(uic.Child is Border, "Child must be a Border container");
                var b = (Border)uic.Child;
                AssertEqual(2.5, b.Margin.Top, "Paragraph math top margin must be 2.5 DIPs");
                AssertEqual(-2.5, b.Margin.Bottom, "Paragraph math bottom margin must be -2.5 DIPs");
                AssertEqual(0.0, b.Margin.Top + b.Margin.Bottom, "Zero-sum vertical margin invariant violated in Paragraph");
            }

            // 2. ListItem context (LineHeight = NaN)
            string listMd = "* List item with formula $ROE > 20\\%$\n* Second item with formula $\\ge +1.5\\%$";
            var listDoc = converter.Convert(parser.Parse(listMd));
            var list = listDoc.Blocks.OfType<List>().FirstOrDefault();
            AssertNotNull(list, "List must exist");
            AssertEqual(2, list!.ListItems.Count, "List must have 2 items");

            foreach (var item in list.ListItems)
            {
                var itemPara = item.Blocks.OfType<Paragraph>().FirstOrDefault();
                AssertNotNull(itemPara, "ListItem must contain a Paragraph");
                AssertTrue(itemPara!.ReadLocalValue(Block.LineHeightProperty) == DependencyProperty.UnsetValue,
                    "ListItem paragraph LineHeight must be unforced locally (inheriting naturally from FlowDocument)");
                var uic = itemPara.Inlines.OfType<InlineUIContainer>().FirstOrDefault();
                AssertNotNull(uic, "ListItem must contain a math container");
                AssertEqual(BaselineAlignment.Center, uic!.BaselineAlignment, "ListItem container BaselineAlignment must be Center");
                var b = (Border)uic.Child;
                AssertEqual(2.5, b.Margin.Top, "ListItem math top margin must be 2.5 DIPs");
                AssertEqual(-2.5, b.Margin.Bottom, "ListItem math bottom margin must be -2.5 DIPs");
                AssertEqual(0.0, b.Margin.Top + b.Margin.Bottom, "Zero-sum vertical margin invariant violated in ListItem");
            }

            // 3. Headings H1 through H6 with font-proportional scaling
            string headingsMd =
                "# Heading 1 $x + y = z$\n" +
                "## Heading 2 $x + y = z$\n" +
                "### Heading 3 $x + y = z$\n" +
                "#### Heading 4 $x + y = z$\n" +
                "##### Heading 5 $x + y = z$\n" +
                "###### Heading 6 $x + y = z$";

            var headingsDoc = converter.Convert(parser.Parse(headingsMd));
            var headingParas = headingsDoc.Blocks.OfType<Paragraph>().ToList();
            AssertEqual(6, headingParas.Count, "Must produce 6 heading paragraphs");

            double[] expectedFontSizes = new double[] { 26.0, 20.0, 17.0, 15.0, 13.5, 12.5 };
            for (int h = 0; h < 6; h++)
            {
                var hp = headingParas[h];
                double expectedFont = expectedFontSizes[h];
                AssertEqual(expectedFont, hp.FontSize, $"Heading H{h + 1} FontSize mismatch");

                double expectedVOffset = Math.Round(expectedFont * (2.5 / 14.5), 1);
                var uic = hp.Inlines.OfType<InlineUIContainer>().FirstOrDefault();
                AssertNotNull(uic, $"Heading H{h + 1} must contain math container");
                AssertEqual(BaselineAlignment.Center, uic!.BaselineAlignment, $"Heading H{h + 1} BaselineAlignment must be Center");

                var b = (Border)uic.Child;
                AssertEqual(expectedVOffset, b.Margin.Top, $"Heading H{h + 1} top margin mismatch");
                AssertEqual(-expectedVOffset, b.Margin.Bottom, $"Heading H{h + 1} bottom margin mismatch");
                AssertEqual(0.0, b.Margin.Top + b.Margin.Bottom, $"Zero-sum vertical margin invariant violated in Heading H{h + 1}");
            }

            // 4. GFM Table context
            string tableMd =
                "| Metric | Minimum Threshold |\n" +
                "|---|---|\n" +
                "| Return on Capital | $ROIC > 18\\%$ |\n" +
                "| Valuation Multiple | $\\le 30\\times$ |";

            var tableDoc = converter.Convert(parser.Parse(tableMd));
            var table = tableDoc.Blocks.OfType<WpfTable>().FirstOrDefault();
            AssertNotNull(table, "Table must exist");
            var tableUics = table!.RowGroups.SelectMany(rg => rg.Rows)
                .SelectMany(r => r.Cells)
                .SelectMany(c => c.Blocks.OfType<Paragraph>())
                .SelectMany(p => p.Inlines.OfType<InlineUIContainer>())
                .ToList();

            AssertEqual(2, tableUics.Count, "Table must contain 2 math containers");
            foreach (var tuic in tableUics)
            {
                AssertEqual(BaselineAlignment.Center, tuic.BaselineAlignment, "Table math BaselineAlignment must be Center");
                var b = (Border)tuic.Child;
                AssertEqual(2.5, b.Margin.Top, "Table math top margin must be 2.5 DIPs");
                AssertEqual(-2.5, b.Margin.Bottom, "Table math bottom margin must be -2.5 DIPs");
                AssertEqual(0.0, b.Margin.Top + b.Margin.Bottom, "Zero-sum vertical margin invariant violated in Table");
            }

            // 5. Layout pass verification
            var rtb = new RichTextBox { Width = 850, Document = headingsDoc, IsReadOnly = true };
            rtb.Measure(new Size(850, 3000));
            rtb.Arrange(new Rect(0, 0, 850, rtb.DesiredSize.Height));
            rtb.UpdateLayout();
            AssertTrue(rtb.ActualWidth > 0 && rtb.ActualHeight > 0, "RichTextBox layout pass must succeed");
        }

        /// <summary>
        /// T5.9: Validates optical alignment and non-elevation bounds for financial valuation expressions
        /// ($ROIC > 18\%$, $ROE > 20\%$, $\ge +1.5\%$, $\le 30\times$, $> 18\%$, $> 0$, $\approx 15\%$, $< -2.5\%$).
        /// </summary>
        private static void TestFinancialValuationExpressionsOpticalBounds()
        {
            var parser = new MarkdownParser();
            var palette = ThemePalette.GitHubDark;
            var converter = new MarkdownToWpfConverter("", palette, enableLatex: true, enableHtml: true);

            string financialMd =
                "Financial Valuation Screening Metrics:\n\n" +
                "* Core Hurdle: $ROIC > 18\\%$ compounder return on capital.\n" +
                "* Equity Yield: $ROE > 20\\%$ sustained return on equity.\n" +
                "* Margin Expansion: $\\ge +1.5\\%$ gross profit margin YoY.\n" +
                "* Multiple Ceiling: $\\le 30\\times$ forward enterprise valuation.\n" +
                "* Hurdle Rate: $> 18\\%$ internal rate of return benchmark.\n" +
                "* Organic Growth: $> 0$ positive operating free cash flow.\n" +
                "* WACC Benchmark: $\\approx 15\\%$ cost of capital proxy.\n" +
                "* Downside Cushion: $< -2.5\\%$ max acceptable quarterly variance.";

            var doc = converter.Convert(parser.Parse(financialMd));
            var rtb = new RichTextBox
            {
                Width = 850,
                Document = doc,
                IsReadOnly = true
            };
            rtb.Measure(new Size(850, 3000));
            rtb.Arrange(new Rect(0, 0, 850, rtb.DesiredSize.Height));
            rtb.UpdateLayout();

            AssertTrue(rtb.ActualWidth > 0 && rtb.ActualHeight > 0, "RichTextBox must measure cleanly");

            string[] expectedExpressions = new[]
            {
                "ROIC > 18\\%",
                "ROE > 20\\%",
                "\\ge +1.5\\%",
                "\\le 30\\times",
                "> 18\\%",
                "> 0",
                "\\approx 15\\%",
                "< -2.5\\%"
            };

            var list = doc.Blocks.OfType<List>().FirstOrDefault();
            AssertNotNull(list, "List must exist in financial document");

            int verifiedCount = 0;
            foreach (var expr in expectedExpressions)
            {
                InlineUIContainer? targetUic = null;
                Paragraph? targetPara = null;

                foreach (var item in list!.ListItems)
                {
                    foreach (var p in item.Blocks.OfType<Paragraph>())
                    {
                        foreach (var inl in p.Inlines)
                        {
                            if (inl is InlineUIContainer u && (u.Tag as MathTag)?.Expression == expr)
                            {
                                targetUic = u;
                                targetPara = p;
                                break;
                            }
                        }
                        if (targetUic != null) break;
                    }
                    if (targetUic != null) break;
                }

                AssertNotNull(targetUic, $"Expression '{expr}' must be found in FlowDocument");
                AssertNotNull(targetPara, $"Expression '{expr}' must have parent Paragraph");

                AssertEqual(BaselineAlignment.Center, targetUic!.BaselineAlignment, $"Expression '{expr}' must have BaselineAlignment.Center");
                var border = (Border)targetUic.Child;
                AssertEqual(2.5, border.Margin.Top, $"Expression '{expr}' Top margin must be 2.5 DIPs");
                AssertEqual(-2.5, border.Margin.Bottom, $"Expression '{expr}' Bottom margin must be -2.5 DIPs");
                AssertTrue(border.ActualWidth > 0, $"Expression '{expr}' ActualWidth must be > 0 (no horizontal clipping)");
                AssertTrue(border.ActualHeight > 0, $"Expression '{expr}' ActualHeight must be > 0 (no vertical clipping)");

                // Measure character rect of surrounding text vs math container
                Run? siblingRun = targetPara!.Inlines.OfType<Run>().FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.Text));
                AssertNotNull(siblingRun, $"Expression '{expr}' must have sibling text Run for baseline comparison");

                Rect textRect = siblingRun!.ContentStart.GetCharacterRect(LogicalDirection.Forward);
                Rect mathRect = targetUic.ContentStart.GetCharacterRect(LogicalDirection.Forward);
                AssertTrue(textRect.Height > 0, "Text character rect must have positive height");
                AssertTrue(mathRect.Height > 0, "Math character rect must have positive height");

                Point borderPos = border.TranslatePoint(new Point(0, 0), rtb);
                Rect borderRect = new Rect(borderPos, new Size(border.ActualWidth, border.ActualHeight));

                // Optical centering check: delta <= 1.5 DIPs
                double mathCenterY = mathRect.Top + mathRect.Height / 2.0;
                double textCenterY = textRect.Top + textRect.Height / 2.0;
                double opticalDelta = Math.Abs(mathCenterY - textCenterY);
                AssertTrue(opticalDelta <= 1.5,
                    $"Expression '{expr}' optical delta ({opticalDelta:F2} DIPs) must be <= 1.5 DIPs (mathCenter={mathCenterY:F2}, textCenter={textCenterY:F2})");

                // Non-elevation guard: formula must not sit higher than text Top - 2.0 DIPs (superscript elevation prevention)
                AssertTrue(borderRect.Top >= textRect.Top - 2.0,
                    $"Expression '{expr}' non-elevation guard violated: borderRect.Top ({borderRect.Top:F2}) is elevated above text Top ({textRect.Top:F2})");

                verifiedCount++;
            }

            AssertEqual(expectedExpressions.Length, verifiedCount, "All 8 financial expressions verified");
        }

        /// <summary>
        /// T5.10: Validates zero-clipping and finite positive dimensions under extreme math inputs:
        /// multilevel nested fractions, deep continued fractions, exponent towers, deep subscripts,
        /// and large operators with upper/lower limits.
        /// </summary>
        private static void TestExtremeMathInputsZeroClipping()
        {
            var parser = new MarkdownParser();
            var palette = ThemePalette.GitHubDark;
            var converter = new MarkdownToWpfConverter("", palette, enableLatex: true, enableHtml: true);

            string extremeMd =
                "Extreme Math Inputs:\n\n" +
                "1. Nested Fractions: $\\frac{\\frac{a}{b}}{\\frac{c}{d}}$\n" +
                "2. Continued Fractions: $\\frac{1}{1 + \\frac{1}{1 + \\frac{1}{x}}}$\n" +
                "3. Exponent Towers: $x^{y^{z^w}}$ and $2^{2^{2^2}}$\n" +
                "4. Deep Subscripts: $A_{i_{j_k}}$ and $B_{1_{2_3}}$\n" +
                "5. Radicals with Degrees: $\\sqrt[3]{\\frac{a+b}{c-d}}$\n" +
                "6. Integrals with Limits: $\\int_{-\\infty}^{+\\infty} \\frac{e^{-x^2}}{\\sqrt{2\\pi}} dx$\n" +
                "7. Large Summation: $\\sum_{k=1}^n \\frac{1}{k^2} = \\frac{\\pi^2}{6}$";

            var doc = converter.Convert(parser.Parse(extremeMd));
            var rtb = new RichTextBox
            {
                Width = 850,
                Document = doc,
                IsReadOnly = true
            };
            rtb.Measure(new Size(850, 4000));
            rtb.Arrange(new Rect(0, 0, 850, rtb.DesiredSize.Height));
            rtb.UpdateLayout();

            AssertTrue(rtb.ActualWidth > 0 && rtb.ActualHeight > 0, "Extreme math document must measure cleanly");

            var allUics = doc.Blocks.OfType<List>()
                .SelectMany(l => l.ListItems)
                .SelectMany(it => it.Blocks.OfType<Paragraph>())
                .SelectMany(p => p.Inlines.OfType<InlineUIContainer>())
                .ToList();

            AssertTrue(allUics.Count >= 8, $"Must produce at least 8 extreme math containers, found {allUics.Count}");

            foreach (var uic in allUics)
            {
                var tag = uic.Tag as MathTag;
                string expr = tag?.Expression ?? "unknown";

                AssertEqual(BaselineAlignment.Center, uic.BaselineAlignment, $"Container for '{expr}' must be Center");
                var border = (Border)uic.Child;

                AssertTrue(border.ActualWidth > 0 && !double.IsNaN(border.ActualWidth) && !double.IsInfinity(border.ActualWidth),
                    $"Expression '{expr}' ActualWidth ({border.ActualWidth}) must be finite and positive (zero clipping)");
                AssertTrue(border.ActualHeight > 0 && !double.IsNaN(border.ActualHeight) && !double.IsInfinity(border.ActualHeight),
                    $"Expression '{expr}' ActualHeight ({border.ActualHeight}) must be finite and positive (zero clipping)");

                // Measure character rect
                Rect charRect = uic.ContentStart.GetCharacterRect(LogicalDirection.Forward);
                AssertTrue(charRect.Height > 0 && !charRect.IsEmpty, $"Character rect for '{expr}' must be valid");
            }
        }

        /// <summary>
        /// T5.11: Validates currency expression parsing ($100 collision protection) and graceful resilience
        /// against malformed or adversarial LaTeX syntax.
        /// </summary>
        private static void TestCurrencyCollisionAndMalformedLatexResilience()
        {
            var parser = new MarkdownParser();
            var palette = ThemePalette.GitHubDark;
            var converter = new MarkdownToWpfConverter("", palette, enableLatex: true, enableHtml: true);

            // 1. Currency collision protection: raw dollar amounts must NOT be parsed as LaTeX math
            string currencyText = "The stock closed at $100 today, between $50 and $200 per share, with total $100 to $250 range.";
            var currencyDoc = parser.Parse(currencyText);
            var para = currencyDoc.Blocks.OfType<ParagraphBlock>().FirstOrDefault();
            AssertNotNull(para, "Paragraph block must exist");
            var mathInlines = para!.Inlines.OfType<MathInline>().ToList();
            AssertEqual(0, mathInlines.Count, "Raw currency amounts like $100 and $200 must NOT be parsed as MathInline");

            // 1b. Multiple dollar amounts in a financial sentence
            string multiDollar = "Revenue was $100M with net income of $25M and dividend of $1.50 per share.";
            var multiDoc = parser.Parse(multiDollar);
            var multiPara = multiDoc.Blocks.OfType<ParagraphBlock>().FirstOrDefault();
            AssertNotNull(multiPara);
            AssertEqual(0, multiPara!.Inlines.OfType<MathInline>().Count(), "Multiple currency figures must NOT trigger math inlines");

            // 1c. Explicit escaped dollar syntax in LaTeX formula
            string escapedMath = "Formula with dollar: $\\\\$100$ and $\\\\$250$ in analysis.";
            var escapedDoc = parser.Parse(escapedMath);
            var escapedPara = escapedDoc.Blocks.OfType<ParagraphBlock>().FirstOrDefault();
            AssertNotNull(escapedPara);
            var escapedInlines = escapedPara!.Inlines.OfType<MathInline>().ToList();
            AssertEqual(2, escapedInlines.Count, "Formulas with escaped dollar signs must parse as MathInline");

            // 2. Adversarial malformed LaTeX syntax: must NOT throw unhandled exceptions or crash
            string[] emptySnippets = new[] { "  ", "" };
            foreach (var snippet in emptySnippets)
            {
                UIElement elem = LatexMathRenderer.RenderMath(snippet, palette, 14.5, isDisplay: false);
                AssertNotNull(elem, $"RenderMath must return non-null element for empty snippet '{snippet}'");
                AssertTrue(elem is TextBlock, $"RenderMath must return empty TextBlock for empty snippet '{snippet}'");
            }

            string[] malformedSnippets = new[]
            {
                "$$",                           // Literal dollar signs
                "\\frac{a}{b",                  // Unclosed brace
                "\\frac{}{}",                   // Empty fraction
                "\\sqrt[3]{",                   // Unclosed radical index
                "\\begin{matrix} 1 & 2 \\\\ 3", // Unclosed matrix environment
                "\\left( \\frac{1}{2}",         // Unclosed left delimiter
                "\\unknowncommand{xyz}",        // Unknown command
                "\\\\\\\\",                     // Multiple backslashes
                "^2",                           // Leading superscript without base
                "_1",                           // Leading subscript without base
                "\\mathbf{",                    // Unclosed font modifier
                "\\text{"                       // Unclosed text modifier
            };

            foreach (var snippet in malformedSnippets)
            {
                UIElement elem = null!;
                try
                {
                    elem = LatexMathRenderer.RenderMath(snippet, palette, 14.5, isDisplay: false);
                }
                catch (Exception ex)
                {
                    AssertTrue(false, $"LatexMathRenderer crashed on malformed snippet: '{snippet}' - {ex.Message}");
                }

                AssertNotNull(elem, $"RenderMath must return non-null element for '{snippet}'");
                AssertTrue(elem is Border, $"RenderMath must wrap result in Border for '{snippet}'");
                var b = (Border)elem;
                AssertEqual(2.5, b.Margin.Top, $"Top margin must be preserved for '{snippet}'");
                AssertEqual(-2.5, b.Margin.Bottom, $"Bottom margin must be preserved for '{snippet}'");
            }
        }

        /// <summary>
        /// T5.12: Validates the 20-node flowchart benchmark (< 20ms layout engine, < 25ms total pipeline)
        /// and conducts orientation stress testing (TD, TB, BT, LR, RL) plus 50-node and 100-node scale stress.
        /// </summary>
        private static void TestMermaid20NodeBenchmarkAndScaleStress()
        {
            // 1. 20-node flowchart with diverse shapes, connections, labels, and cycles
            string benchmark20Source = @"graph TD
                A([Client Web SPA]) -->|HTTP REST| B[Ingress Gateway]
                A -.->|WebSocket| C[Push Notifications]
                B --> D{Authorized?}
                D -->|Yes| E[Order Processing]
                D -->|No| F[401 Challenge]
                E ==> G[(PostgreSQL Primary)]
                E --> H[Inventory Service]
                H --> I{Stock Available?}
                I -->|In Stock| J[Stripe Billing]
                I -->|Out of Stock| K[Backorder Queue]
                J --> L[Fulfillment Engine]
                L --> M[Warehouse Dispatch]
                M --> N([Shipment Tracking])
                K -.->|Wait 24h| I
                E --> O[Kafka Event Bus]
                O --> P[Analytics Pipeline]
                P --> Q[Data Lake S3]
                O --> R[Email Dispatcher]
                R --> S([Customer Inbox])
                F --> T[Security Audit Log]";

            AssertTrue(MermaidFlowchartParser.TryParse(benchmark20Source, out var graph20, out _), "20-node graph must parse");
            AssertEqual(20, graph20!.Nodes.Count, "Graph must contain exactly 20 nodes");

            // Warm-up run
            var warmLayout = MermaidLayoutEngine.Layout(graph20);
            var warmVisual = MermaidFlowchartRenderer.CreateFlowchartVisual(warmLayout, ThemePalette.GitHubDark, benchmark20Source);
            AssertNotNull(warmVisual);

            // Benchmark Layout Engine (< 20.0 ms budget)
            const int iterations = 50;
            var swLayout = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var layout = MermaidLayoutEngine.Layout(graph20);
            }
            swLayout.Stop();
            double avgLayoutMs = (double)swLayout.ElapsedMilliseconds / iterations;
            Console.WriteLine($"  [BENCHMARK] 20-Node Mermaid Layout: {iterations} runs, avg {avgLayoutMs:F3} ms/layout (Budget: < 20.0 ms)");
            AssertTrue(avgLayoutMs < 20.0, $"20-Node layout time {avgLayoutMs:F3}ms exceeds 20.0ms budget");

            // Benchmark Total Pipeline (Parse + Layout + Render Visual) (< 25.0 ms budget)
            var swTotal = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                MermaidFlowchartParser.TryParse(benchmark20Source, out var g, out _);
                var layout = MermaidLayoutEngine.Layout(g!);
                var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, benchmark20Source);
            }
            swTotal.Stop();
            double avgTotalMs = (double)swTotal.ElapsedMilliseconds / iterations;
            Console.WriteLine($"  [BENCHMARK] 20-Node Total Pipeline: {iterations} runs, avg {avgTotalMs:F3} ms/total (Budget: < 25.0 ms)");
            AssertTrue(avgTotalMs < 25.0, $"20-Node total pipeline time {avgTotalMs:F3}ms exceeds 25.0ms budget");

            // 2. Orientation stress across all 5 directives on the 20-node graph
            string[] orientations = new[] { "TD", "TB", "BT", "LR", "RL" };
            foreach (var ori in orientations)
            {
                string orientedSource = $"graph {ori}\n" + benchmark20Source.Substring(benchmark20Source.IndexOf('\n') + 1);
                AssertTrue(MermaidFlowchartParser.TryParse(orientedSource, out var oriGraph, out _), $"Parse oriented graph {ori}");

                var swOri = Stopwatch.StartNew();
                var oriLayout = MermaidLayoutEngine.Layout(oriGraph!);
                swOri.Stop();

                AssertTrue(swOri.ElapsedMilliseconds < 20.0, $"Oriented {ori} layout latency ({swOri.ElapsedMilliseconds} ms) must be < 20ms");
                AssertEqual(20, oriLayout.Nodes.Count, $"Oriented {ori} node count mismatch");
                AssertTrue(oriLayout.TotalWidth > 0 && oriLayout.TotalHeight > 0, $"Oriented {ori} dimensions must be positive");

                // Verify non-overlapping node bounds
                var nodeList = oriLayout.Nodes.Values.ToList();
                for (int a = 0; a < nodeList.Count; a++)
                {
                    for (int b = a + 1; b < nodeList.Count; b++)
                    {
                        var rectA = nodeList[a].Bounds;
                        var rectB = nodeList[b].Bounds;
                        bool intersects = rectA.IntersectsWith(rectB);
                        AssertFalse(intersects, $"Orientation {ori}: Node {nodeList[a].Node.Id} overlaps {nodeList[b].Node.Id}");
                    }
                }
            }

            // 3. Scale Stress: 50-node and 100-node graphs
            var lines50 = new List<string> { "graph TD" };
            for (int i = 1; i <= 50; i++)
            {
                lines50.Add($"N{i}[Node {i}] --> N{(i % 50) + 1}[Node {(i % 50) + 1}]");
                if (i % 5 == 0) lines50.Add($"N{i} --> N{(i + 13) % 50 + 1}");
            }
            string graph50Source = string.Join("\n", lines50);
            AssertTrue(MermaidFlowchartParser.TryParse(graph50Source, out var graph50, out _));

            var sw50 = Stopwatch.StartNew();
            var layout50 = MermaidLayoutEngine.Layout(graph50!);
            sw50.Stop();
            Console.WriteLine($"  [SCALE] 50-Node Layout Latency: {sw50.ElapsedMilliseconds} ms (Budget: < 10.0 ms)");
            AssertTrue(sw50.ElapsedMilliseconds < 10.0, $"50-Node layout ({sw50.ElapsedMilliseconds} ms) must be < 10.0 ms");
            AssertEqual(50, layout50.Nodes.Count);

            var lines100 = new List<string> { "graph LR" };
            for (int i = 1; i <= 100; i++)
            {
                lines100.Add($"Node_{i}([Label {i}]) --> Node_{(i % 100) + 1}([Label {(i % 100) + 1}])");
            }
            string graph100Source = string.Join("\n", lines100);
            AssertTrue(MermaidFlowchartParser.TryParse(graph100Source, out var graph100, out _));

            var sw100 = Stopwatch.StartNew();
            var layout100 = MermaidLayoutEngine.Layout(graph100!);
            sw100.Stop();
            Console.WriteLine($"  [SCALE] 100-Node Layout Latency: {sw100.ElapsedMilliseconds} ms (Budget: < 25.0 ms)");
            AssertTrue(sw100.ElapsedMilliseconds < 25.0, $"100-Node layout ({sw100.ElapsedMilliseconds} ms) must be < 25.0 ms");
            AssertEqual(100, layout100.Nodes.Count);
        }

        /// <summary>
        /// T5.13: Validates rapid theme switching stress (50 loops / 400 palette switches) across all 8 theme
        /// presets while actively holding and re-rendering both LaTeX inline math and Mermaid vector visuals.
        /// </summary>
        private static void TestRapidThemeSwitchingWithMathAndMermaid()
        {
            string mixedDocMarkdown =
                "# System Architecture & Valuation Analysis $V_0 = \\sum_{t=1}^n \\frac{CF_t}{(1+r)^t}$\n\n" +
                "The target company generates compound returns with $ROIC > 18\\%$ and expanding gross margins $\\ge +1.5\\%$.\n\n" +
                "* Valuation Ceiling: $\\le 30\\times$ NTM EPS\n" +
                "* Cost of Capital: $WACC \\approx 8.5\\%$\n\n" +
                "```mermaid\n" +
                "graph TD\n" +
                "A[Market Opportunity] --> B{Screener Criteria}\n" +
                "B -->|ROIC > 18%| C[High Moat Portfolio]\n" +
                "B -->|ROIC <= 18%| D[Watchlist]\n" +
                "C --> E[(DCF Valuation Model)]\n" +
                "```\n\n" +
                "| Parameter | Formula | Target |\n" +
                "|---|---|---|\n" +
                "| Spread | $ROIC - WACC$ | $\\ge +9.5\\%$ |\n" +
                "| Multiple | $P / E$ | $\\le 30\\times$ |";

            var parser = new MarkdownParser();
            var docAst = parser.Parse(mixedDocMarkdown);

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

            const int cycles = 50; // 50 * 8 = 400 conversions
            var sw = Stopwatch.StartNew();

            for (int c = 0; c < cycles; c++)
            {
                foreach (var preset in presets)
                {
                    var palette = ThemePalette.GetPalette(preset);
                    var converter = new MarkdownToWpfConverter("", palette, enableLatex: true, enableHtml: true);
                    var flowDoc = converter.Convert(docAst);

                    AssertNotNull(flowDoc);
                    AssertTrue(flowDoc.Blocks.Count >= 5, "FlowDocument must retain all blocks during theme switch");
                }
            }

            sw.Stop();
            double avgPerSwitchMs = (double)sw.ElapsedMilliseconds / (cycles * presets.Length);
            Console.WriteLine($"  [THEME STRESS] {cycles * presets.Length} theme switches completed in {sw.ElapsedMilliseconds} ms (avg {avgPerSwitchMs:F3} ms/switch)");
            AssertTrue(avgPerSwitchMs < 15.0, $"Average theme switch latency ({avgPerSwitchMs:F3} ms) must be < 15.0 ms");
        }

        #endregion
    }
}

