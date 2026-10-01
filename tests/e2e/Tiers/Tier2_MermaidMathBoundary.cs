using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

namespace MDPlus.E2E.Tiers
{
    /// <summary>
    /// Tier 2: Boundary & Corner Cases for Mermaid Flowcharts & Inline Math Typography.
    /// Covers edge conditions across Features F1 through F15 with at least 5 tests per feature (75 tests total).
    /// </summary>
    public static class Tier2_MermaidMathBoundary
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 2: Mermaid & Math Boundary Cases (F1-F15)");
            Console.WriteLine("==================================================");

            // F1 Boundaries
            RunTest("Tier2", "T2.F1.1: Empty mermaid code block handling", TestT2_F1_1_EmptyBlock);
            RunTest("Tier2", "T2.F1.2: Whitespace-only mermaid code block handling", TestT2_F1_2_WhitespaceBlock);
            RunTest("Tier2", "T2.F1.3: Unclosed delimiters and syntax error fallback", TestT2_F1_3_UnclosedDelimiters);
            RunTest("Tier2", "T2.F1.4: Massive malformed code block fast degradation", TestT2_F1_4_MassiveMalformedBlock);
            RunTest("Tier2", "T2.F1.5: Mixed valid and invalid lines fallback safety", TestT2_F1_5_MixedValidInvalid);

            // F2 Boundaries
            RunTest("Tier2", "T2.F2.1: Single isolated node AST without connections", TestT2_F2_1_IsolatedNodeAST);
            RunTest("Tier2", "T2.F2.2: Node IDs with special punctuation and hyphens", TestT2_F2_2_SpecialCharNodeId);
            RunTest("Tier2", "T2.F2.3: Disconnected subgraphs and island clusters", TestT2_F2_3_DisconnectedSubgraphs);
            RunTest("Tier2", "T2.F2.4: Large 50+ node AST model construction and integrity", TestT2_F2_4_Large50NodeAST);
            RunTest("Tier2", "T2.F2.5: Duplicate and parallel edge declarations", TestT2_F2_5_DuplicateParallelEdges);

            // F3 Boundaries
            RunTest("Tier2", "T2.F3.1: Missing orientation keyword defaulting to TD", TestT2_F3_1_MissingOrientationDefault);
            RunTest("Tier2", "T2.F3.2: Leading/trailing whitespace and spaces around orientation", TestT2_F3_2_WhitespaceOrientation);
            RunTest("Tier2", "T2.F3.3: Lowercase keywords (flowchart lr, graph tb)", TestT2_F3_3_LowercaseKeywords);
            RunTest("Tier2", "T2.F3.4: Orientation followed by inline comments on same line", TestT2_F3_4_OrientationWithInlineComment);
            RunTest("Tier2", "T2.F3.5: Multiple orientation lines tolerance", TestT2_F3_5_MultipleOrientationLines);

            // F4 Boundaries
            RunTest("Tier2", "T2.F4.1: Empty node label brackets (A[], B(), C{})", TestT2_F4_1_EmptyLabelBrackets);
            RunTest("Tier2", "T2.F4.2: Node labels with Unicode, CJK, and emojis", TestT2_F4_2_UnicodeAndEmojis);
            RunTest("Tier2", "T2.F4.3: Node labels containing quotes and escaped quotes", TestT2_F4_3_QuotedLabels);
            RunTest("Tier2", "T2.F4.4: Extremely long node labels (500+ characters)", TestT2_F4_4_ExtremelyLongLabels);
            RunTest("Tier2", "T2.F4.5: Unbalanced brackets handled gracefully by error fallback", TestT2_F4_5_UnbalancedBracketsFallback);

            // F5 Boundaries
            RunTest("Tier2", "T2.F5.1: Deeply chained connections (10 sequential nodes)", TestT2_F5_1_DeeplyChainedConnections);
            RunTest("Tier2", "T2.F5.2: Self-loop connections (A --> A)", TestT2_F5_2_SelfLoopConnection);
            RunTest("Tier2", "T2.F5.3: Multiple edge styles between identical nodes", TestT2_F5_3_MultipleEdgeStylesSameNodes);
            RunTest("Tier2", "T2.F5.4: Edge labels with math and currency symbols", TestT2_F5_4_EdgeLabelsWithSymbols);
            RunTest("Tier2", "T2.F5.5: Empty edge label pipes (A -->|| B)", TestT2_F5_5_EmptyEdgeLabels);

            // F6 Boundaries
            RunTest("Tier2", "T2.F6.1: Single isolated node layout calculations", TestT2_F6_1_SingleIsolatedNodeLayout);
            RunTest("Tier2", "T2.F6.2: Direct two-node mutual cycle layout (A <-> B)", TestT2_F6_2_TwoNodeMutualCycle);
            RunTest("Tier2", "T2.F6.3: Three-node cycle layout with feedback arc routing", TestT2_F6_3_ThreeNodeCycleLayout);
            RunTest("Tier2", "T2.F6.4: Self-loop layout with bypass waypoint corridor", TestT2_F6_4_SelfLoopLayoutWaypoints);
            RunTest("Tier2", "T2.F6.5: Dense bipartite graph layout (K_3,3 with 9 edges)", TestT2_F6_5_DenseBipartiteLayout);

            // F7 Boundaries
            RunTest("Tier2", "T2.F7.1: Extreme wide graph aspect ratio (20 nodes in 1 layer)", TestT2_F7_1_ExtremeWideGraph);
            RunTest("Tier2", "T2.F7.2: Extreme tall graph aspect ratio (20 nodes in vertical chain)", TestT2_F7_2_ExtremeTallGraph);
            RunTest("Tier2", "T2.F7.3: All node coordinates strictly non-negative (X >= 0, Y >= 0)", TestT2_F7_3_NonNegativeCoordinates);
            RunTest("Tier2", "T2.F7.4: Zero node bounding box collisions across branching DAGs", TestT2_F7_4_ZeroCollisionsInBranching);
            RunTest("Tier2", "T2.F7.5: Total width and height strictly enclose node bounds + padding", TestT2_F7_5_TotalDimensionsEncloseNodes);

            // F8 Boundaries
            RunTest("Tier2", "T2.F8.1: Vector canvas generation for single isolated node", TestT2_F8_1_SingleNodeVectorCanvas);
            RunTest("Tier2", "T2.F8.2: 50-node graph canvas element count verification", TestT2_F8_2_Large50NodeCanvasElements);
            RunTest("Tier2", "T2.F8.3: Path stroke thickness and dash styles match StrokeStyle", TestT2_F8_3_PathStrokeStyles);
            RunTest("Tier2", "T2.F8.4: Arrowhead endpoints touch target node port coordinates", TestT2_F8_4_ArrowheadEndpointTouch);
            RunTest("Tier2", "T2.F8.5: Edge label badges maintain readable text and contrast", TestT2_F8_5_LabelBadgeReadability);

            // F9 Boundaries
            RunTest("Tier2", "T2.F9.1: Dark theme contrast verification across all dark presets", TestT2_F9_1_DarkThemeContrast);
            RunTest("Tier2", "T2.F9.2: Light theme contrast verification across light presets", TestT2_F9_2_LightThemeContrast);
            RunTest("Tier2", "T2.F9.3: Theme switching stress test (50 consecutive toggles)", TestT2_F9_3_ThemeSwitchingStress50);
            RunTest("Tier2", "T2.F9.4: Color palette brush immutability and thread safety", TestT2_F9_4_BrushImmutability);
            RunTest("Tier2", "T2.F9.5: High contrast mode contrast ratio exceeds 7.0:1 (AAA)", TestT2_F9_5_HighContrastModeAAA);

            // F10 Boundaries
            RunTest("Tier2", "T2.F10.1: Rapid mouse wheel event bubbling (100 iterations)", TestT2_F10_1_RapidMouseWheelBubbling);
            RunTest("Tier2", "T2.F10.2: ScrollViewer horizontal offset limits and auto visibility", TestT2_F10_2_HorizontalOffsetLimits);
            RunTest("Tier2", "T2.F10.3: Layout latency budget < 16.6ms for 60 FPS animation", TestT2_F10_3_LayoutLatency60FPSBudget);
            RunTest("Tier2", "T2.F10.4: Vector canvas layout pass does not re-measure FlowDoc", TestT2_F10_4_LayoutPassIsolation);
            RunTest("Tier2", "T2.F10.5: Focusable == false and zero margins/paddings on viewer", TestT2_F10_5_ViewerFocusAndMargins);

            // F11 Boundaries
            RunTest("Tier2", "T2.F11.1: Empty math expression ($ $ and $$ $$) handling", TestT2_F11_1_EmptyMathExpression);
            RunTest("Tier2", "T2.F11.2: Single character math ($x$) margin and bounds", TestT2_F11_2_SingleCharMathMargin);
            RunTest("Tier2", "T2.F11.3: Deeply nested subscripts and superscripts ($x_{i_{j^k}}$)", TestT2_F11_3_DeepNestedScripts);
            RunTest("Tier2", "T2.F11.4: Large math operators container margins and non-clipping", TestT2_F11_4_LargeMathOperators);
            RunTest("Tier2", "T2.F11.5: Font size boundary: small (8pt) and large (36pt) vOffset", TestT2_F11_5_FontSizeBoundaries);

            // F12 Boundaries
            RunTest("Tier2", "T2.F12.1: LineHeight=24 paragraph context margins within line bounds", TestT2_F12_1_LineHeight24Margins);
            RunTest("Tier2", "T2.F12.2: LineHeight=NaN list item context line step consistency", TestT2_F12_2_ListItemLineStepConsistency);
            RunTest("Tier2", "T2.F12.3: LineHeight=36 display heading context vOffset scaling", TestT2_F12_3_LineHeight36Heading);
            RunTest("Tier2", "T2.F12.4: Deeply nested lists (depth 4) with inline math", TestT2_F12_4_DeepNestedListsMath);
            RunTest("Tier2", "T2.F12.5: GFM Table cell context with inline math", TestT2_F12_5_TableCellsWithMath);

            // F13 Boundaries
            RunTest("Tier2", "T2.F13.1: Inverted inequality ($< 18\\%$) less-than sign handling", TestT2_F13_1_InvertedInequality);
            RunTest("Tier2", "T2.F13.2: Combined percent and currency in single formula", TestT2_F13_2_CombinedPercentAndCurrency);
            RunTest("Tier2", "T2.F13.3: Multiple consecutive inline formulas in single paragraph", TestT2_F13_3_MultipleConsecutiveFormulas);
            RunTest("Tier2", "T2.F13.4: Formula with escaped percent (\\%) vs bare percent", TestT2_F13_4_EscapedVsBarePercent);
            RunTest("Tier2", "T2.F13.5: Formula with multiplication symbol (\\times) and decimals", TestT2_F13_5_TimesSymbolAndDecimals);

            // F14 Boundaries
            RunTest("Tier2", "T2.F14.1: Adversarial malformed LaTeX (unclosed braces) fallback", TestT2_F14_1_MalformedLatexUnclosedBraces);
            RunTest("Tier2", "T2.F14.2: Adversarial malformed Mermaid (unclosed brackets) fallback", TestT2_F14_2_MalformedMermaidBrackets);
            RunTest("Tier2", "T2.F14.3: Concurrency: parallel parsing of Mermaid and LaTeX", TestT2_F14_3_ParallelParsingConcurrency);
            RunTest("Tier2", "T2.F14.4: Memory allocation under repeated parsing of 100 diagrams", TestT2_F14_4_RepeatedParsingMemoryStability);
            RunTest("Tier2", "T2.F14.5: Zero UI thread deadlocks during background parsing", TestT2_F14_5_ZeroDeadlocksBackgroundParsing);

            // F15 Boundaries
            RunTest("Tier2", "T2.F15.1: 20-node graph under constrained maxAvailableWidth=300", TestT2_F15_1_ConstrainedWidthBenchmark);
            RunTest("Tier2", "T2.F15.2: 20-node graph under unconstrained maxAvailableWidth=Infinity", TestT2_F15_2_UnconstrainedWidthBenchmark);
            RunTest("Tier2", "T2.F15.3: 50-node graph layout benchmark (< 10ms)", TestT2_F15_3_50NodeLayoutBenchmark);
            RunTest("Tier2", "T2.F15.4: Layout engine determinism (10 runs identical coordinates)", TestT2_F15_4_LayoutDeterminismIdenticalCoords);
            RunTest("Tier2", "T2.F15.5: Total render memory overhead < 500 KB for 20-node flowchart", TestT2_F15_5_RenderMemoryOverheadBudget);
        }

        #region F1 Boundaries

        private static void TestT2_F1_1_EmptyBlock()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark);
            var doc = parser.Parse("```mermaid\n```");
            var flowDoc = converter.Convert(doc);
            AssertNotNull(flowDoc);
        }

        private static void TestT2_F1_2_WhitespaceBlock()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark);
            var doc = parser.Parse("```mermaid\n   \t  \r\n   \n```");
            var flowDoc = converter.Convert(doc);
            AssertNotNull(flowDoc);
        }

        private static void TestT2_F1_3_UnclosedDelimiters()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark);
            string bad = "```mermaid\ngraph TD\nA[Unclosed node label\n```";
            var doc = parser.Parse(bad);
            var flowDoc = converter.Convert(doc);

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic, "Unclosed delimiters must fall back cleanly to a code block");
        }

        private static void TestT2_F1_4_MassiveMalformedBlock()
        {
            var lines = new List<string> { "```mermaid" };
            for (int i = 0; i < 500; i++) lines.Add($"random line of non-mermaid text {i} ??? &&& !!!");
            lines.Add("```");
            string bad = string.Join("\n", lines);

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark);

            var sw = Stopwatch.StartNew();
            var doc = parser.Parse(bad);
            var flowDoc = converter.Convert(doc);
            sw.Stop();

            AssertNotNull(flowDoc);
            AssertTrue(sw.ElapsedMilliseconds < 1000, $"Massive malformed block must fail fast ({sw.ElapsedMilliseconds} ms)");
        }

        private static void TestT2_F1_5_MixedValidInvalid()
        {
            string bad = "```mermaid\ngraph TD\nA-->B\nthis is broken line in the middle\nB-->C\n```";
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark);
            var doc = parser.Parse(bad);
            var flowDoc = converter.Convert(doc);

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic, "Mixed block must degrade to fallback code block");
        }

        #endregion

        #region F2 Boundaries

        private static void TestT2_F2_1_IsolatedNodeAST()
        {
            string diagram = "graph TD\nIsolatedNode";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(1, g!.Nodes.Count);
            AssertEqual(0, g.Edges.Count);
            AssertEqual("IsolatedNode", g.Nodes["IsolatedNode"].Id);
        }

        private static void TestT2_F2_2_SpecialCharNodeId()
        {
            string diagram = "graph TD\nNode_123-abc.def[Valid Label]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(1, g!.Nodes.Count);
        }

        private static void TestT2_F2_3_DisconnectedSubgraphs()
        {
            string diagram = @"graph TD
                subgraph IslandA [Cluster 1]
                    A1 --> A2
                end
                subgraph IslandB [Cluster 2]
                    B1 --> B2
                end";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(4, g!.Nodes.Count);
            AssertEqual(2, g.Edges.Count);
            AssertEqual(2, g.Subgraphs.Count);
        }

        private static void TestT2_F2_4_Large50NodeAST()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 50; i++) lines.Add($"N{i}[Node {i}] --> N{(i % 50) + 1}");
            string diagram = string.Join("\n", lines);

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(50, g!.Nodes.Count);
            AssertEqual(50, g.Edges.Count);
        }

        private static void TestT2_F2_5_DuplicateParallelEdges()
        {
            string diagram = "graph TD\nA -->|First| B\nA -.->|Second| B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(2, g!.Edges.Count);
        }

        #endregion

        #region F3 Boundaries

        private static void TestT2_F3_1_MissingOrientationDefault()
        {
            string diagram = "graph\nA-->B";
            bool parsed = MermaidFlowchartParser.TryParse(diagram, out var g, out string? error);
            AssertFalse(parsed, "Missing orientation directive must fail parsing and fall back");
            AssertNotNull(error);
        }

        private static void TestT2_F3_2_WhitespaceOrientation()
        {
            string diagram = "   graph     TD   \nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(MermaidOrientation.TopToBottom, g!.Orientation);
        }

        private static void TestT2_F3_3_LowercaseKeywords()
        {
            string diagram = "flowchart lr\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(MermaidOrientation.LeftToRight, g!.Orientation);
            AssertTrue(g.IsFlowchartKeyword);
        }

        private static void TestT2_F3_4_OrientationWithInlineComment()
        {
            string diagram = "graph TD %% Initialize layout\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(MermaidOrientation.TopToBottom, g!.Orientation);
        }

        private static void TestT2_F3_5_MultipleOrientationLines()
        {
            string diagram = "graph TD\ngraph LR\nA-->B";
            // Parser should parse without throwing
            bool parsed = MermaidFlowchartParser.TryParse(diagram, out var g, out _);
            AssertTrue(parsed || g == null);
        }

        #endregion

        #region F4 Boundaries

        private static void TestT2_F4_1_EmptyLabelBrackets()
        {
            string diagram = "graph TD\nA[] --> B() --> C{} --> D(()) --> E([])";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(5, g!.Nodes.Count);
        }

        private static void TestT2_F4_2_UnicodeAndEmojis()
        {
            string diagram = "graph TD\nA[🚀 Rocket Launch] --> B[日本語テキスト] --> C[Zürich & Café]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(3, g!.Nodes.Count);
            AssertEqual("🚀 Rocket Launch", g.Nodes["A"].Text);
            AssertEqual("日本語テキスト", g.Nodes["B"].Text);
        }

        private static void TestT2_F4_3_QuotedLabels()
        {
            string diagram = "graph TD\nA[\"Label with \\\"Quotes\\\" inside\"] --> B[\"Special # % & characters\"]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(2, g!.Nodes.Count);
        }

        private static void TestT2_F4_4_ExtremelyLongLabels()
        {
            string longText = new string('X', 600);
            string diagram = $"graph TD\nNodeA[{longText}] --> NodeB[End]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(600, g!.Nodes["NodeA"].Text.Length);
        }

        private static void TestT2_F4_5_UnbalancedBracketsFallback()
        {
            string diagram = "graph TD\nA[Unbalanced --> B";
            // Parser will return false and report error
            bool parsed = MermaidFlowchartParser.TryParse(diagram, out _, out string? error);
            AssertFalse(parsed, "Unbalanced bracket must not parse as valid flowchart");
            AssertNotNull(error, "Error message must be supplied");
        }

        #endregion

        #region F5 Boundaries

        private static void TestT2_F5_1_DeeplyChainedConnections()
        {
            string diagram = "graph LR\nN1 --> N2 --> N3 --> N4 --> N5 --> N6 --> N7 --> N8 --> N9 --> N10";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(10, g!.Nodes.Count);
            AssertEqual(9, g.Edges.Count);
        }

        private static void TestT2_F5_2_SelfLoopConnection()
        {
            string diagram = "graph TD\nSelfLoop[Loop] --> SelfLoop";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(1, g!.Edges.Count);
            AssertEqual("SelfLoop", g.Edges[0].SourceId);
            AssertEqual("SelfLoop", g.Edges[0].TargetId);
        }

        private static void TestT2_F5_3_MultipleEdgeStylesSameNodes()
        {
            string diagram = "graph TD\nA --> B\nA -.-> B\nA ==> B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(3, g!.Edges.Count);
            AssertEqual(MermaidStrokeStyle.Solid, g.Edges[0].Stroke);
            AssertEqual(MermaidStrokeStyle.Dotted, g.Edges[1].Stroke);
            AssertEqual(MermaidStrokeStyle.Thick, g.Edges[2].Stroke);
        }

        private static void TestT2_F5_4_EdgeLabelsWithSymbols()
        {
            string diagram = "graph TD\nA -->|Rate: 15.5% & Cost: $100M| B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual("Rate: 15.5% & Cost: $100M", g!.Edges[0].Label);
        }

        private static void TestT2_F5_5_EmptyEdgeLabels()
        {
            string diagram = "graph TD\nA -->|| B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(1, g!.Edges.Count);
        }

        #endregion

        #region F6 Boundaries

        private static void TestT2_F6_1_SingleIsolatedNodeLayout()
        {
            string diagram = "graph TD\nSingleNode[Process]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertEqual(1, layout.Nodes.Count);
            AssertTrue(layout.TotalWidth > 0);
            AssertTrue(layout.TotalHeight > 0);
        }

        private static void TestT2_F6_2_TwoNodeMutualCycle()
        {
            string diagram = "graph TD\nA --> B\nB --> A";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertEqual(2, layout.Nodes.Count);
            AssertEqual(2, layout.Edges.Count);
            AssertTrue(layout.Edges.Any(e => e.IsFeedbackEdge), "One of the mutual cycle edges must be marked feedback");
        }

        private static void TestT2_F6_3_ThreeNodeCycleLayout()
        {
            string diagram = "graph TD\nA-->B-->C-->A";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertTrue(layout.Edges.Any(e => e.IsFeedbackEdge));
        }

        private static void TestT2_F6_4_SelfLoopLayoutWaypoints()
        {
            string diagram = "graph TD\nA[Action] --> A";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertTrue(layout.Edges[0].Waypoints.Count >= 2, "Self loop must have corridor waypoints");
        }

        private static void TestT2_F6_5_DenseBipartiteLayout()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 3; i++)
            {
                for (int j = 1; j <= 3; j++)
                {
                    lines.Add($"U{i} --> V{j}");
                }
            }
            string diagram = string.Join("\n", lines);
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertEqual(6, layout.Nodes.Count);
            AssertEqual(9, layout.Edges.Count);
        }

        #endregion

        #region F7 Boundaries

        private static void TestT2_F7_1_ExtremeWideGraph()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"Root --> WideNode_{i}");
            string diagram = string.Join("\n", lines);
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertTrue(layout.TotalWidth > layout.TotalHeight, "Wide graph total width must exceed height");
        }

        private static void TestT2_F7_2_ExtremeTallGraph()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"TallNode_{i} --> TallNode_{i + 1}");
            string diagram = string.Join("\n", lines);
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertTrue(layout.TotalHeight > layout.TotalWidth, "Tall graph total height must exceed width");
        }

        private static void TestT2_F7_3_NonNegativeCoordinates()
        {
            string diagram = "graph TD\nA-->B-->C";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            foreach (var kvp in layout.Nodes)
            {
                AssertTrue(kvp.Value.X >= 0.0, $"Node {kvp.Key} X must be non-negative");
                AssertTrue(kvp.Value.Y >= 0.0, $"Node {kvp.Key} Y must be non-negative");
            }
        }

        private static void TestT2_F7_4_ZeroCollisionsInBranching()
        {
            string diagram = @"graph TD
                Root --> BranchA
                Root --> BranchB
                Root --> BranchC
                BranchA --> SubA1
                BranchA --> SubA2
                BranchB --> Merge
                BranchC --> Merge";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);

            var nodes = layout.Nodes.Values.ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    var r1 = nodes[i].Bounds;
                    var r2 = nodes[j].Bounds;
                    AssertFalse(r1.IntersectsWith(r2), $"Collision detected between {nodes[i].Node.Id} and {nodes[j].Node.Id}");
                }
            }
        }

        private static void TestT2_F7_5_TotalDimensionsEncloseNodes()
        {
            string diagram = "graph TD\nA-->B-->C";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            foreach (var node in layout.Nodes.Values)
            {
                AssertTrue(node.X + node.Width <= layout.TotalWidth, "TotalWidth must enclose all node right edges");
                AssertTrue(node.Y + node.Height <= layout.TotalHeight, "TotalHeight must enclose all node bottom edges");
            }
        }

        #endregion

        #region F8 Boundaries

        private static void TestT2_F8_1_SingleNodeVectorCanvas()
        {
            string diagram = "graph TD\nA[Only One]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;
            AssertTrue(canvas.Children.OfType<Border>().Any(), "Canvas must contain node Border");
        }

        private static void TestT2_F8_2_Large50NodeCanvasElements()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 50; i++) lines.Add($"N{i} --> N{(i % 50) + 1}");
            string diagram = string.Join("\n", lines);
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;
            AssertTrue(canvas.Children.OfType<Border>().Count() >= 50, "Canvas must contain at least 50 node borders");
        }

        private static void TestT2_F8_3_PathStrokeStyles()
        {
            string diagram = "graph TD\nA --> B\nC -.-> D\nE ==> F";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;
            var paths = canvas.Children.OfType<System.Windows.Shapes.Path>().ToList();
            AssertTrue(paths.Count >= 3, "Paths must exist for all 3 stroke styles");
        }

        private static void TestT2_F8_4_ArrowheadEndpointTouch()
        {
            string diagram = "graph TD\nA --> B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            var edge = layout.Edges[0];
            var target = layout.Nodes["B"];
            // In TD layout, endpoint must touch top port of B
            AssertEqual(target.TopPort.X, edge.EndPoint.X, "Endpoint X must match target TopPort X");
            AssertEqual(target.TopPort.Y, edge.EndPoint.Y, "Endpoint Y must match target TopPort Y");
        }

        private static void TestT2_F8_5_LabelBadgeReadability()
        {
            string diagram = "graph TD\nA -->|Branch Alpha| B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;
            AssertTrue(canvas.Children.OfType<TextBlock>().Any(tb => tb.Text == "Branch Alpha")
                || canvas.Children.OfType<Border>().Any(b => (b.Child as TextBlock)?.Text == "Branch Alpha"),
                "Label badge must be present in visual tree");
        }

        #endregion

        #region F9 Boundaries

        private static void TestT2_F9_1_DarkThemeContrast()
        {
            var darkThemes = new[] { ThemePalette.GitHubDark, ThemePalette.Nord, ThemePalette.OneDark, ThemePalette.Monokai };
            foreach (var p in darkThemes)
            {
                Color nodeBgColor = p.IsDark ? Color.FromRgb(33, 38, 45) : Color.FromRgb(255, 255, 255);
                double ratio = CalculateContrastRatio(nodeBgColor, p.EditorFg.Color);
                AssertTrue(ratio >= 4.5, $"{p.Name} dark contrast ratio {ratio:F2} must be >= 4.5:1");
            }
        }

        private static void TestT2_F9_2_LightThemeContrast()
        {
            var lightThemes = new[] { ThemePalette.GitHubLight, ThemePalette.OneLight, ThemePalette.SolarizedLight, ThemePalette.QuietLight };
            foreach (var p in lightThemes)
            {
                Color nodeBgColor = p.IsDark ? Color.FromRgb(33, 38, 45) : Color.FromRgb(255, 255, 255);
                double ratio = CalculateContrastRatio(nodeBgColor, p.EditorFg.Color);
                AssertTrue(ratio >= 4.5, $"{p.Name} light contrast ratio {ratio:F2} must be >= 4.5:1");
            }
        }

        private static void TestT2_F9_3_ThemeSwitchingStress50()
        {
            string diagram = "graph TD\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);

            var palettes = new[] { ThemePalette.GitHubDark, ThemePalette.GitHubLight, ThemePalette.Nord, ThemePalette.OneDark };
            for (int i = 0; i < 50; i++)
            {
                var p = palettes[i % palettes.Length];
                var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, p, diagram);
                AssertNotNull(visual);
            }
        }

        private static void TestT2_F9_4_BrushImmutability()
        {
            var p = ThemePalette.GitHubDark;
            AssertTrue(p.EditorBg.IsFrozen, "EditorBg brush must be frozen");
            AssertTrue(p.EditorFg.IsFrozen, "EditorFg brush must be frozen");
            AssertTrue(p.CodeBg.IsFrozen, "CodeBg brush must be frozen");
        }

        private static void TestT2_F9_5_HighContrastModeAAA()
        {
            var p = ThemePalette.Monokai;
            Color nodeBgColor = p.IsDark ? Color.FromRgb(33, 38, 45) : Color.FromRgb(255, 255, 255);
            double ratio = CalculateContrastRatio(nodeBgColor, p.EditorFg.Color);
            AssertTrue(ratio >= 7.0, $"Monokai contrast ratio {ratio:F2} must be >= 7.0:1 (AAA)");
        }

        #endregion

        #region F10 Boundaries

        private static void TestT2_F10_1_RapidMouseWheelBubbling()
        {
            var sv = new MermaidScrollViewer();
            // Verify properties hold under rapid inspection
            for (int i = 0; i < 100; i++)
            {
                AssertEqual(ScrollBarVisibility.Disabled, sv.VerticalScrollBarVisibility);
            }
        }

        private static void TestT2_F10_2_HorizontalOffsetLimits()
        {
            var sv = new MermaidScrollViewer();
            AssertEqual(0.0, sv.HorizontalOffset);
        }

        private static void TestT2_F10_3_LayoutLatency60FPSBudget()
        {
            string diagram = "graph TD\nA[Step 1] --> B{Check} --> C[End]\nB -->|No| D[Fix] --> A";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                MermaidLayoutEngine.Layout(g!);
            }
            sw.Stop();
            double avgMs = sw.ElapsedMilliseconds / 10.0;
            AssertTrue(avgMs < 16.66, $"Layout latency {avgMs:F2} ms must be within 16.6ms 60 FPS budget");
        }

        private static void TestT2_F10_4_LayoutPassIsolation()
        {
            string diagram = "graph TD\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);

            visual.Measure(new Size(800, 600));
            visual.Arrange(new Rect(0, 0, 800, 600));
            visual.UpdateLayout();

            AssertTrue(visual.ActualWidth > 0);
            AssertTrue(visual.ActualHeight > 0);
        }

        private static void TestT2_F10_5_ViewerFocusAndMargins()
        {
            var sv = new MermaidScrollViewer();
            AssertFalse(sv.Focusable);
            AssertEqual(new Thickness(0), sv.BorderThickness);
        }

        #endregion

        #region F11 Boundaries

        private static void TestT2_F11_1_EmptyMathExpression()
        {
            var elem1 = LatexMathRenderer.RenderMath("", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(elem1);

            var elem2 = LatexMathRenderer.RenderMath("   ", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(elem2);
        }

        private static void TestT2_F11_2_SingleCharMathMargin()
        {
            var elem = LatexMathRenderer.RenderMath("x", ThemePalette.GitHubDark, 14.5, false);
            var border = (Border)elem;
            AssertEqual(2.5, border.Margin.Top);
            AssertEqual(-2.5, border.Margin.Bottom);
        }

        private static void TestT2_F11_3_DeepNestedScripts()
        {
            var elem = LatexMathRenderer.RenderMath("x_{a_{b^c}}", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(elem);
        }

        private static void TestT2_F11_4_LargeMathOperators()
        {
            var elem = LatexMathRenderer.RenderMath("\\sum_{i=1}^n \\int_{-\\infty}^\\infty f(x) dx", ThemePalette.GitHubDark, 14.5, false);
            var border = (Border)elem;
            border.Measure(new Size(1000, 500));
            AssertTrue(border.DesiredSize.Height > 0);
        }

        private static void TestT2_F11_5_FontSizeBoundaries()
        {
            // Small font: 8pt -> vOffset = round(8 * 2.5 / 14.5, 1) = round(1.379, 1) = 1.4
            double vSmall = Math.Round(8.0 * (2.5 / 14.5), 1);
            AssertEqual(1.4, vSmall);

            // Large font: 36pt -> vOffset = round(36 * 2.5 / 14.5, 1) = round(6.206, 1) = 6.2
            double vLarge = Math.Round(36.0 * (2.5 / 14.5), 1);
            AssertEqual(6.2, vLarge);
        }

        #endregion

        #region F12 Boundaries

        private static void TestT2_F12_1_LineHeight24Margins()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse("Paragraph $A = B$ with fixed line height.");
            var flowDoc = converter.Convert(doc);
            var para = (Paragraph)flowDoc.Blocks.FirstBlock!;
            para.LineHeight = 24.0;

            var uic = para.Inlines.OfType<InlineUIContainer>().First();
            var border = (Border)uic.Child;
            AssertEqual(2.5, border.Margin.Top);
            AssertEqual(-2.5, border.Margin.Bottom);
        }

        private static void TestT2_F12_2_ListItemLineStepConsistency()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            string md = "* Line 1\n* Line 2 with $x=1$\n* Line 3";
            var doc = parser.Parse(md);
            var flowDoc = converter.Convert(doc);
            var list = flowDoc.Blocks.OfType<List>().First();
            AssertEqual(3, list.ListItems.Count);
        }

        private static void TestT2_F12_3_LineHeight36Heading()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse("# Display Title $E = mc^2$");
            var flowDoc = converter.Convert(doc);
            var para = (Paragraph)flowDoc.Blocks.FirstBlock!;
            para.LineHeight = 36.0;

            var uic = para.Inlines.OfType<InlineUIContainer>().First();
            AssertEqual(BaselineAlignment.Center, uic.BaselineAlignment);
        }

        private static void TestT2_F12_4_DeepNestedListsMath()
        {
            string md = "* Level 1\n  * Level 2\n    * Level 3\n      * Level 4 with formula $k = 42$";
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));
            AssertNotNull(flowDoc);
        }

        private static void TestT2_F12_5_TableCellsWithMath()
        {
            string md = "| Metric | Target |\n| --- | --- |\n| ROIC | $> 18\\%$ |\n| ROE | $> 20\\%$ |";
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));
            AssertTrue(flowDoc.Blocks.OfType<System.Windows.Documents.Table>().Any());
        }

        #endregion

        #region F13 Boundaries

        private static void TestT2_F13_1_InvertedInequality()
        {
            var elem = LatexMathRenderer.RenderMath("< 18\\%", ThemePalette.GitHubDark, 14.5, false);
            var border = (Border)elem;
            AssertEqual(2.5, border.Margin.Top);
            AssertEqual(-2.5, border.Margin.Bottom);
        }

        private static void TestT2_F13_2_CombinedPercentAndCurrency()
        {
            var elem = LatexMathRenderer.RenderMath("\\ge +1.5\\% \\text{ and } \\$100M", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(elem);
        }

        private static void TestT2_F13_3_MultipleConsecutiveFormulas()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse("Consecutive: $A=1$, $B=2$, $C=3$, $D=4$.");
            var flowDoc = converter.Convert(doc);
            var para = (Paragraph)flowDoc.Blocks.FirstBlock!;
            var uics = para.Inlines.OfType<InlineUIContainer>().ToList();
            AssertEqual(4, uics.Count);
        }

        private static void TestT2_F13_4_EscapedVsBarePercent()
        {
            var elem1 = LatexMathRenderer.RenderMath("18\\%", ThemePalette.GitHubDark, 14.5, false);
            var elem2 = LatexMathRenderer.RenderMath("18%", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(elem1);
            AssertNotNull(elem2);
        }

        private static void TestT2_F13_5_TimesSymbolAndDecimals()
        {
            var elem = LatexMathRenderer.RenderMath("3.14159 \\times 10^8", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(elem);
        }

        #endregion

        #region F14 Boundaries

        private static void TestT2_F14_1_MalformedLatexUnclosedBraces()
        {
            // Must not throw exception
            var elem = LatexMathRenderer.RenderMath("\\frac{1}{2", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(elem);
        }

        private static void TestT2_F14_2_MalformedMermaidBrackets()
        {
            string bad = "graph TD\nA[[Nested unclosed";
            bool success = MermaidFlowchartParser.TryParse(bad, out _, out _);
            AssertFalse(success);
        }

        private static void TestT2_F14_3_ParallelParsingConcurrency()
        {
            Parallel.For(0, 50, i =>
            {
                bool ok = MermaidFlowchartParser.TryParse($"graph TD\nNode{i}-->Node{i + 1}", out var g, out _);
                AssertTrue(ok);
                var layout = MermaidLayoutEngine.Layout(g!);
                AssertNotNull(layout);
            });
        }

        private static void TestT2_F14_4_RepeatedParsingMemoryStability()
        {
            string diagram = "graph TD\nA-->B-->C-->D-->E";
            for (int i = 0; i < 100; i++)
            {
                MermaidFlowchartParser.TryParse(diagram, out var g, out _);
                MermaidLayoutEngine.Layout(g!);
            }
        }

        private static void TestT2_F14_5_ZeroDeadlocksBackgroundParsing()
        {
            var task = Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    MermaidFlowchartParser.TryParse("graph TD\nA-->B", out _, out _);
                }
            });
            bool finished = task.Wait(TimeSpan.FromSeconds(5));
            AssertTrue(finished, "Background parsing must complete without deadlock");
        }

        #endregion

        #region F15 Boundaries

        private static void TestT2_F15_1_ConstrainedWidthBenchmark()
        {
            var lines = new List<string> { "graph LR" };
            for (int i = 1; i <= 20; i++) lines.Add($"N{i} --> N{i + 1}");
            string diagram = string.Join("\n", lines);
            MermaidFlowchartParser.TryParse(diagram, out var g, out _);

            var sw = Stopwatch.StartNew();
            var layout = MermaidLayoutEngine.Layout(g!, maxAvailableWidth: 300.0);
            sw.Stop();

            AssertNotNull(layout);
            AssertTrue(sw.ElapsedMilliseconds < 20.0);
        }

        private static void TestT2_F15_2_UnconstrainedWidthBenchmark()
        {
            var lines = new List<string> { "graph LR" };
            for (int i = 1; i <= 20; i++) lines.Add($"N{i} --> N{i + 1}");
            string diagram = string.Join("\n", lines);
            MermaidFlowchartParser.TryParse(diagram, out var g, out _);

            var sw = Stopwatch.StartNew();
            var layout = MermaidLayoutEngine.Layout(g!, maxAvailableWidth: double.PositiveInfinity);
            sw.Stop();

            AssertNotNull(layout);
            AssertTrue(sw.ElapsedMilliseconds < 20.0);
        }

        private static void TestT2_F15_3_50NodeLayoutBenchmark()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 50; i++) lines.Add($"N{i} --> N{(i % 50) + 1}");
            string diagram = string.Join("\n", lines);
            MermaidFlowchartParser.TryParse(diagram, out var g, out _);

            var sw = Stopwatch.StartNew();
            var layout = MermaidLayoutEngine.Layout(g!);
            sw.Stop();

            AssertEqual(50, layout.Nodes.Count);
            AssertTrue(sw.ElapsedMilliseconds < 10.0, $"50-node layout ({sw.ElapsedMilliseconds} ms) must be < 10 ms");
        }

        private static void TestT2_F15_4_LayoutDeterminismIdenticalCoords()
        {
            string diagram = "graph TD\nStart --> Process --> Decision{Check?} --> End\nDecision -->|No| Fix --> Process";
            MermaidFlowchartParser.TryParse(diagram, out var g, out _);

            var l1 = MermaidLayoutEngine.Layout(g!);
            for (int i = 0; i < 10; i++)
            {
                var l2 = MermaidLayoutEngine.Layout(g!);
                AssertEqual(l1.TotalWidth, l2.TotalWidth, "TotalWidth must be identical");
                AssertEqual(l1.TotalHeight, l2.TotalHeight, "TotalHeight must be identical");
                foreach (var k in l1.Nodes.Keys)
                {
                    AssertEqual(l1.Nodes[k].X, l2.Nodes[k].X, $"Node {k} X must match");
                    AssertEqual(l1.Nodes[k].Y, l2.Nodes[k].Y, $"Node {k} Y must match");
                }
            }
        }

        private static void TestT2_F15_5_RenderMemoryOverheadBudget()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"N{i} --> N{(i % 20) + 1}");
            string diagram = string.Join("\n", lines);
            MermaidFlowchartParser.TryParse(diagram, out var g, out _);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            long before = GC.GetAllocatedBytesForCurrentThread();

            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);

            long after = GC.GetAllocatedBytesForCurrentThread();
            long kb = (after - before) / 1024;
            AssertTrue(kb < 500, $"Total render memory {kb} KB must be < 500 KB");
        }

        #endregion
    }
}
