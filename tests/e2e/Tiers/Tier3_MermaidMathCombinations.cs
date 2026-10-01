using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using MDPlus.Core;
using MDPlus.Core.Mermaid;
using MDPlus.Models;
using MDPlus.E2E.Harness;
using static MDPlus.E2E.Harness.E2ETestHarness;
using WpfTable = System.Windows.Documents.Table;

namespace MDPlus.E2E.Tiers
{
    /// <summary>
    /// Tier 3: Cross-Feature Combinations for Mermaid Flowcharts & Inline Math Typography.
    /// Exercises complex pairwise and multi-feature interactions across Features F1 through F15 (15 tests total).
    /// </summary>
    public static class Tier3_MermaidMathCombinations
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 3: Cross-Feature Combinations (Pairwise)   ");
            Console.WriteLine("==================================================");

            RunTest("Tier3", "T3.1: Diamond shapes with dotted arrows and edge labels (F4 + F5)", TestT3_1_DiamondWithDottedArrowsAndLabels);
            RunTest("Tier3", "T3.2: Subgraphs containing mixed node shapes and styled borders (F2 + F4 + F8)", TestT3_2_SubgraphsWithStyledBordersAndMixedShapes);
            RunTest("Tier3", "T3.3: Heading (H1-H6) and inline math inside list items (F11 + F12)", TestT3_3_HeadingWithInlineMathInsideListItems);
            RunTest("Tier3", "T3.4: Dynamic theme switching while viewing rendered Mermaid canvas (F8 + F9)", TestT3_4_ThemeSwitchingLiveCycleWithMermaidCanvas);
            RunTest("Tier3", "T3.5: Fallback code block with copy button adjacent to rendered math (F1 + F11)", TestT3_5_FallbackCodeBlockAdjacentToRenderedMath);
            RunTest("Tier3", "T3.6: 20-node flowchart benchmark under custom ThemePalette (F9 + F15)", TestT3_6_TwentyNodeBenchmarkUnderThemePalette);
            RunTest("Tier3", "T3.7: Cyclic feedback loops and labeled transitions in LR orientation (F3 + F5 + F6)", TestT3_7_CyclicFeedbackLoopsInLROrientation);
            RunTest("Tier3", "T3.8: Flowchart in BlockUIContainer round-trip serialization via CodeBlockTag (F1 + F8)", TestT3_8_FlowchartBlockSerializationRoundTrip);
            RunTest("Tier3", "T3.9: Inline math formulas inside GFM Table cells with right alignment (F11 + F12)", TestT3_9_InlineMathInTableCellsWithAlignment);
            RunTest("Tier3", "T3.10: Deeply chained arrows with alternating edge styles and node shapes (F4 + F5 + F6)", TestT3_10_DeeplyChainedArrowsWithAlternatingShapes);
            RunTest("Tier3", "T3.11: Mermaid flowchart followed by Query 2 financial math in list items (F8 + F13)", TestT3_11_FlowchartFollowedByQuery2FinancialList);
            RunTest("Tier3", "T3.12: Vertical subgraphs with cross-subgraph edges (F2 + F3 + F5 + F6)", TestT3_12_VerticalSubgraphsWithCrossEdges);
            RunTest("Tier3", "T3.13: Document with multiple consecutive diagrams and inline math (F1 + F8 + F11)", TestT3_13_MultiDiagramMultiMathDocument);
            RunTest("Tier3", "T3.14: Theme switching live stress across all 8 themes with mixed document (F9 + F11 + F12)", TestT3_14_ThemeSwitchingLiveStressMixedDocument);
            RunTest("Tier3", "T3.15: High-density graph with all 5 shapes, all 6 edges, and Sugiyama layout (F2 + F4 + F5 + F6 + F8)", TestT3_15_HighDensityGraphAllShapesAllEdges);
        }

        private static void TestT3_1_DiamondWithDottedArrowsAndLabels()
        {
            string diagram = @"graph TD
                Start([Start]) --> Condition{Evaluation OK?}
                Condition -.->|Approved| Proceed[Execution]
                Condition -.->|Rejected| Abort([Halt])";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(MermaidNodeShape.Diamond, g!.Nodes["Condition"].Shape);
            var dottedEdges = g.Edges.Where(e => e.Stroke == MermaidStrokeStyle.Dotted).ToList();
            AssertEqual(2, dottedEdges.Count);
            AssertEqual("Approved", dottedEdges[0].Label);
            AssertEqual("Rejected", dottedEdges[1].Label);

            var layout = MermaidLayoutEngine.Layout(g);
            AssertEqual(4, layout.Nodes.Count);
            AssertEqual(3, layout.Edges.Count);
        }

        private static void TestT3_2_SubgraphsWithStyledBordersAndMixedShapes()
        {
            string diagram = @"graph TD
                subgraph ClientTier [Client Cluster]
                    Browser([Web Client])
                    Mobile([Mobile App])
                end
                subgraph ServerTier [Backend Cluster]
                    Router{Ingress Router}
                    Service[API Service Worker]
                    DB[(Persistent Store)]
                end
                Browser --> Router
                Mobile --> Router
                Router --> Service --> DB";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(2, g!.Subgraphs.Count);
            AssertEqual(2, g.Subgraphs[0].NodeIds.Count);
            AssertEqual(3, g.Subgraphs[1].NodeIds.Count);

            var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(MermaidLayoutEngine.Layout(g), ThemePalette.Nord, diagram);
            AssertNotNull(visual);
        }

        private static void TestT3_3_HeadingWithInlineMathInsideListItems()
        {
            string md = @"# Heading 1 with $\alpha + \beta = \gamma$
## Heading 2 with $E = mc^2$

* Normal list item
* Financial metric: $ROIC > 18\%$ and $ROE > 20\%$
* Physics formula: $\int_0^\infty e^{-x} dx = 1$";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));

            // Check headings have scaled math container margins (-1.7)
            var h1 = (Paragraph)flowDoc.Blocks.ElementAt(0);
            var h1Uic = h1.Inlines.OfType<InlineUIContainer>().First();
            var h1Border = (Border)h1Uic.Child;
            AssertEqual(-1.7, h1Border.Margin.Top);

            // Check list items have standard math container margins (-1.0)
            var list = flowDoc.Blocks.OfType<List>().First();
            var item2 = list.ListItems.ElementAt(1);
            var item2Para = (Paragraph)item2.Blocks.FirstBlock!;
            var item2Uic = item2Para.Inlines.OfType<InlineUIContainer>().First();
            var item2Border = (Border)item2Uic.Child;
            AssertEqual(-1.0, item2Border.Margin.Top);
        }

        private static void TestT3_4_ThemeSwitchingLiveCycleWithMermaidCanvas()
        {
            string diagram = "graph TD\nNodeA --> NodeB --> NodeC";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);

            var allPresets = new[]
            {
                ThemePalette.GitHubDark,
                ThemePalette.GitHubLight,
                ThemePalette.Nord,
                ThemePalette.OneDark,
                ThemePalette.Monokai,
                ThemePalette.OneLight,
                ThemePalette.SolarizedLight,
                ThemePalette.QuietLight
            };

            foreach (var palette in allPresets)
            {
                var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, palette, diagram);
                AssertEqual(palette.CodeBg, visual.Background);
            }
        }

        private static void TestT3_5_FallbackCodeBlockAdjacentToRenderedMath()
        {
            string md = @"Here is valid math: $ROIC > 18\%$

```mermaid
invalid flowchart syntax %%% ###
```

And more math: $\le 30\times$";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));

            var para1 = (Paragraph)flowDoc.Blocks.ElementAt(0);
            AssertTrue(para1.Inlines.OfType<InlineUIContainer>().Any(), "First paragraph must contain math");

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic, "Fallback code block must exist");
            var border = buic!.Child as Border;
            AssertNotNull(border, "Fallback code block must be wrapped in a Border");
            var grid = border!.Child as Grid;
            AssertNotNull(grid, "Border child must be Grid");
            var header = grid!.Children.OfType<Grid>().FirstOrDefault();
            AssertNotNull(header, "Header grid must exist");
            var langLabel = header!.Children.OfType<TextBlock>().FirstOrDefault();
            AssertNotNull(langLabel, "Language label must exist");
            AssertEqual("MERMAID", langLabel!.Text);

            var para2 = (Paragraph)flowDoc.Blocks.ElementAt(2);
            AssertTrue(para2.Inlines.OfType<InlineUIContainer>().Any(), "Third paragraph must contain math");
        }

        private static void TestT3_6_TwentyNodeBenchmarkUnderThemePalette()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"Node_{i}[Process {i}] --> Node_{(i % 20) + 1}");
            string diagram = string.Join("\n", lines);

            var sw = Stopwatch.StartNew();
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.Monokai, diagram);
            sw.Stop();

            AssertNotNull(visual);
            AssertTrue(sw.ElapsedMilliseconds < 25, $"Total benchmark under Monokai took {sw.ElapsedMilliseconds} ms (< 25ms)");
        }

        private static void TestT3_7_CyclicFeedbackLoopsInLROrientation()
        {
            string diagram = @"graph LR
                Start([Input]) --> Stage1[Lexer]
                Stage1 --> Stage2[Parser]
                Stage2 --> Stage3{Syntax Valid?}
                Stage3 -->|Yes| Output([AST])
                Stage3 -.->|No, Retry| Stage1";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(MermaidOrientation.LeftToRight, g!.Orientation);

            var layout = MermaidLayoutEngine.Layout(g);
            AssertEqual(5, layout.Nodes.Count);
            AssertEqual(5, layout.Edges.Count);

            var feedbackEdge = layout.Edges.FirstOrDefault(e => e.IsFeedbackEdge);
            AssertNotNull(feedbackEdge, "Cyclic edge Stage3 -> Stage1 must be identified as feedback in LR");
            AssertEqual("Stage3", feedbackEdge!.Edge.SourceId);
            AssertEqual("Stage1", feedbackEdge.Edge.TargetId);
        }

        private static void TestT3_8_FlowchartBlockSerializationRoundTrip()
        {
            string diagram = "graph TD\nA[Initiate] --> B[Execute]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var buic = MermaidFlowchartRenderer.Render(g!, ThemePalette.GitHubDark, diagram);

            AssertNotNull(buic.Tag);
            AssertTrue(buic.Tag is CodeBlockTag);
            var tag = (CodeBlockTag)buic.Tag;
            AssertEqual("mermaid", tag.Language);
            AssertContains("graph TD", tag.Code);
            AssertContains("A[Initiate] --> B[Execute]", tag.Code);
        }

        private static void TestT3_9_InlineMathInTableCellsWithAlignment()
        {
            string tableMd = @"| Indicator | Formula | Threshold |
| :--- | :---: | ---: |
| ROIC | $\frac{\text{NOPAT}}{\text{IC}}$ | $> 18\%$ |
| ROE | $\frac{\text{Net Income}}{\text{Equity}}$ | $> 20\%$ |
| Valuation | P/E | $\le 30\times$ |";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(tableMd));

            var table = flowDoc.Blocks.OfType<WpfTable>().FirstOrDefault();
            AssertNotNull(table, "Table must be generated");
            AssertEqual(3, table!.Columns.Count);
        }

        private static void TestT3_10_DeeplyChainedArrowsWithAlternatingShapes()
        {
            string diagram = @"graph TD
                A[Start Box] --> B(Round Step) -.-> C{Diamond?} ==> D([Stadium End])";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(4, g!.Nodes.Count);
            AssertEqual(3, g.Edges.Count);

            AssertEqual(MermaidNodeShape.Rectangle, g.Nodes["A"].Shape);
            AssertEqual(MermaidNodeShape.RoundedRectangle, g.Nodes["B"].Shape);
            AssertEqual(MermaidNodeShape.Diamond, g.Nodes["C"].Shape);
            AssertEqual(MermaidNodeShape.Stadium, g.Nodes["D"].Shape);

            AssertEqual(MermaidStrokeStyle.Solid, g.Edges[0].Stroke);
            AssertEqual(MermaidStrokeStyle.Dotted, g.Edges[1].Stroke);
            AssertEqual(MermaidStrokeStyle.Thick, g.Edges[2].Stroke);

            var layout = MermaidLayoutEngine.Layout(g);
            AssertTrue(layout.Nodes["A"].Y < layout.Nodes["B"].Y);
            AssertTrue(layout.Nodes["B"].Y < layout.Nodes["C"].Y);
            AssertTrue(layout.Nodes["C"].Y < layout.Nodes["D"].Y);
        }

        private static void TestT3_11_FlowchartFollowedByQuery2FinancialList()
        {
            string md = @"```mermaid
graph LR
Filter[Screener Filter] --> Compounding{Meets Criteria?}
Compounding -->|Yes| Portfolio[Watchlist]
```

* **Target 1**: $ROIC > 18\%$ with moat expansion
* **Target 2**: Gross Margin YoY $\ge +1.5\%$
* **Target 3**: Forward P/E $\le 30\times$";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));

            AssertTrue(flowDoc.Blocks.OfType<BlockUIContainer>().Any(), "Document must contain flowchart BUIC");
            AssertTrue(flowDoc.Blocks.OfType<List>().Any(), "Document must contain Query 2 List");
        }

        private static void TestT3_12_VerticalSubgraphsWithCrossEdges()
        {
            string diagram = @"graph TD
                subgraph GroupA [Input Stage]
                    In1[Source 1]
                    In2[Source 2]
                end
                subgraph GroupB [Output Stage]
                    Out1[Sink 1]
                    Out2[Sink 2]
                end
                In1 --> Out1
                In2 --> Out2
                In1 --> Out2";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(2, g!.Subgraphs.Count);
            AssertEqual(3, g.Edges.Count);

            var layout = MermaidLayoutEngine.Layout(g);
            AssertEqual(4, layout.Nodes.Count);
            AssertTrue(layout.Nodes["In1"].Y < layout.Nodes["Out1"].Y);
            AssertTrue(layout.Nodes["In2"].Y < layout.Nodes["Out2"].Y);
        }

        private static void TestT3_13_MultiDiagramMultiMathDocument()
        {
            string md = @"# Document with Multiple Flowcharts & Math

Formula 1: $f(x) = \sin(x) + \cos(x)$

```mermaid
graph TD
A1 --> B1
```

Formula 2: $g(x) = \int_0^x t dt$

```mermaid
graph LR
A2 --> B2
```

Formula 3: $h(x) = \sum_{n=1}^\infty \frac{1}{n^2} = \frac{\pi^2}{6}$

```mermaid
graph BT
A3 --> B3
```";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));

            var diagrams = flowDoc.Blocks.OfType<BlockUIContainer>().ToList();
            AssertEqual(3, diagrams.Count);
        }

        private static void TestT3_14_ThemeSwitchingLiveStressMixedDocument()
        {
            string md = @"# Dynamic Stress Test
* Point 1 with $ROIC > 18\%$
* Point 2 with $\ge +1.5\%$

```mermaid
graph TD
Start --> Process --> Done
```";

            var parser = new MarkdownParser();
            var doc = parser.Parse(md);

            var presets = new[]
            {
                ThemePalette.GitHubDark,
                ThemePalette.GitHubLight,
                ThemePalette.Nord,
                ThemePalette.OneDark,
                ThemePalette.Monokai,
                ThemePalette.OneLight,
                ThemePalette.SolarizedLight,
                ThemePalette.QuietLight
            };

            foreach (var p in presets)
            {
                var converter = new MarkdownToWpfConverter("", p, enableLatex: true, enableHtml: true);
                var flowDoc = converter.Convert(doc);
                AssertNotNull(flowDoc);
            }
        }

        private static void TestT3_15_HighDensityGraphAllShapesAllEdges()
        {
            string diagram = @"graph TD
                R[Rectangle] --> RR(Rounded)
                RR -.-> S([Stadium])
                S ==> D{Diamond}
                D --- C((Circle))
                C -.- R
                D === RR";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(5, g!.Nodes.Count);
            AssertEqual(6, g.Edges.Count);

            var layout = MermaidLayoutEngine.Layout(g);
            AssertEqual(5, layout.Nodes.Count);
            AssertEqual(6, layout.Edges.Count);

            var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            AssertNotNull(visual);
        }
    }
}
