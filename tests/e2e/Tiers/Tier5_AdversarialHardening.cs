using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MDPlus.Core;
using MDPlus.E2E.Harness;
using MDPlus.Models;
using static MDPlus.E2E.Harness.E2ETestHarness;

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
            var bannerText = string.Concat(banner.Inlines.OfType<Run>().Select(r => r.Text));
            AssertTrue(bannerText.Contains("Showing first 2,500 of 5,001 lines") && bannerText.Contains("Ctrl+3"), "Banner indicates cap and directs to Raw view Ctrl+3.");
        }

        #endregion
    }
}
