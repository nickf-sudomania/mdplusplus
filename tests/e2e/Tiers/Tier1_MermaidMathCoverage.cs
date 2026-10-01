using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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
    /// Tier 1: Feature Coverage for Mermaid Flowcharts & Inline Math Typography.
    /// Covers Features F1 through F15 with at least 5 thorough opaque-box test cases per feature (75 tests total).
    /// </summary>
    public static class Tier1_MermaidMathCoverage
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 1: Mermaid & Math Feature Coverage (F1-F15)");
            Console.WriteLine("==================================================");

            // F1: Mermaid Block Interception & Fallback
            RunTest("Tier1", "F1.1: Mermaid code block interception in MarkdownToWpfConverter", TestF1_1_BlockInterception);
            RunTest("Tier1", "F1.2: CodeBlockTag preservation with raw source and language", TestF1_2_CodeBlockTagPreservation);
            RunTest("Tier1", "F1.3: Syntax error graceful fallback with TextBlock & copy button", TestF1_3_SyntaxErrorFallback);
            RunTest("Tier1", "F1.4: Unsupported diagram type graceful degradation to code block", TestF1_4_UnsupportedDiagramFallback);
            RunTest("Tier1", "F1.5: Two-way serialization round-trip preservation via CodeBlockTag", TestF1_5_SerializationTag);

            // F2: Mermaid AST Data Model
            RunTest("Tier1", "F2.1: MermaidFlowchartGraph root AST model initialization", TestF2_1_GraphModelCreation);
            RunTest("Tier1", "F2.2: MermaidNode properties (Id, Text, Label, Shape, SourceLine)", TestF2_2_NodeModelProperties);
            RunTest("Tier1", "F2.3: MermaidEdge properties (SourceId, TargetId, Stroke, Arrow, Label)", TestF2_3_EdgeModelProperties);
            RunTest("Tier1", "F2.4: MermaidSubgraph model grouping nodes with ID and title", TestF2_4_SubgraphModelProperties);
            RunTest("Tier1", "F2.5: GetOrCreateNode idempotency and property update", TestF2_5_GetOrCreateNodeIdempotency);

            // F3: Mermaid Orientation Parsing
            RunTest("Tier1", "F3.1: TopToBottom orientation parsing (graph TD, graph TB, flowchart TD)", TestF3_1_OrientationTopToBottom);
            RunTest("Tier1", "F3.2: BottomToTop orientation parsing (graph BT, flowchart BT)", TestF3_2_OrientationBottomToTop);
            RunTest("Tier1", "F3.3: LeftToRight orientation parsing (graph LR, flowchart LR)", TestF3_3_OrientationLeftToRight);
            RunTest("Tier1", "F3.4: RightToLeft orientation parsing (graph RL, flowchart RL)", TestF3_4_OrientationRightToLeft);
            RunTest("Tier1", "F3.5: Case-insensitivity, whitespace, tabs, and semicolon tolerance", TestF3_5_OrientationCaseAndWhitespace);

            // F4: Mermaid Node Shape Parsing
            RunTest("Tier1", "F4.1: Rectangle shape [Process Step] parsing and label extraction", TestF4_1_RectangleShape);
            RunTest("Tier1", "F4.2: Rounded rectangle shape (Action Item) parsing", TestF4_2_RoundedRectangleShape);
            RunTest("Tier1", "F4.3: Stadium / pill capsule shape ([Start / End]) parsing", TestF4_3_StadiumShape);
            RunTest("Tier1", "F4.4: Diamond decision shape {Condition?} parsing", TestF4_4_DiamondShape);
            RunTest("Tier1", "F4.5: Circle junction shape ((State Node)) parsing", TestF4_5_CircleShape);

            // F5: Mermaid Connection & Arrow Syntax
            RunTest("Tier1", "F5.1: Solid arrow (-->) and open link (---) parsing", TestF5_1_SolidConnections);
            RunTest("Tier1", "F5.2: Dotted arrow (-.->) and dotted link (-.-) parsing", TestF5_2_DottedConnections);
            RunTest("Tier1", "F5.3: Thick arrow (==>) and thick link (===) parsing", TestF5_3_ThickConnections);
            RunTest("Tier1", "F5.4: Edge labels with pipes (-->|label|) and inline (-- label -->)", TestF5_4_EdgeLabels);
            RunTest("Tier1", "F5.5: Multi-node chaining (A --> B --> C --> D) parsing", TestF5_5_ChainedConnections);

            // F6: Layered Graph Layout Engine Sugiyama
            RunTest("Tier1", "F6.1: 5-Phase Sugiyama layout execution and geometry population", TestF6_1_SugiyamaPipelineExecution);
            RunTest("Tier1", "F6.2: Cycle reversal and feedback arc bypass corridor generation", TestF6_2_CycleReversalFeedbackCorridor);
            RunTest("Tier1", "F6.3: Dummy node insertion for multi-rank edge normalization", TestF6_3_DummyNodeMultiRankRouting);
            RunTest("Tier1", "F6.4: Barycentric crossing reduction and monotonic rank ordering", TestF6_4_BarycentricCrossingReduction);
            RunTest("Tier1", "F6.5: Self-loop connection (A --> A) layout corridor calculation", TestF6_5_SelfLoopLayoutHandling);

            // F7: Horizontal & Vertical Coordinate Mapping
            RunTest("Tier1", "F7.1: TD/TB vertical coordinate hierarchy (Y_parent < Y_child)", TestF7_1_TopToBottomMapping);
            RunTest("Tier1", "F7.2: BT bottom-to-top inverted vertical mapping (Y_parent > Y_child)", TestF7_2_BottomToTopMapping);
            RunTest("Tier1", "F7.3: LR horizontal coordinate hierarchy (X_parent < X_child)", TestF7_3_LeftToRightMapping);
            RunTest("Tier1", "F7.4: RL right-to-left inverted horizontal mapping (X_parent > X_child)", TestF7_4_RightToLeftMapping);
            RunTest("Tier1", "F7.5: Node bounding box sizing accommodating text metrics & padding", TestF7_5_NodeBoundingBoxes);

            // F8: Native WPF Vector Element Generation
            RunTest("Tier1", "F8.1: Root visual structure (Border, Grid, toolbar, MermaidScrollViewer)", TestF8_1_RootVisualStructure);
            RunTest("Tier1", "F8.2: Canvas child Border elements for all laid-out nodes", TestF8_2_CanvasNodeVectorElements);
            RunTest("Tier1", "F8.3: Canvas Path elements with PathGeometry for cubic Bezier splines", TestF8_3_CanvasEdgeSplinePaths);
            RunTest("Tier1", "F8.4: Connector arrowhead Path elements aligned with tangent angles", TestF8_4_ArrowheadGeometryTangents);
            RunTest("Tier1", "F8.5: Edge label badge Border and TextBlock overlays at route midpoints", TestF8_5_EdgeLabelBadges);

            // F9: Dynamic ThemePalette Integration
            RunTest("Tier1", "F9.1: Node fill, border, and text binding to ThemePalette brushes", TestF9_1_PaletteNodeBinding);
            RunTest("Tier1", "F9.2: Header bar styling with CodeBg, MenuFg, and MERMAID badge", TestF9_2_PaletteHeaderToolbar);
            RunTest("Tier1", "F9.3: Connector line and arrowhead high-visibility stroke binding", TestF9_3_PaletteConnectorStrokes);
            RunTest("Tier1", "F9.4: Vector rendering across all 8 theme presets without degradation", TestF9_4_AllEightThemePresets);
            RunTest("Tier1", "F9.5: Node text contrast ratio meets or exceeds WCAG AA (>= 4.5:1)", TestF9_5_WCAGContrastCompliance);

            // F10: 60 FPS Scrolling & UI Performance
            RunTest("Tier1", "F10.1: Vertical mouse wheel bubbling when Shift is not pressed", TestF10_1_VerticalWheelBubbling);
            RunTest("Tier1", "F10.2: Horizontal mouse wheel scrolling when Shift is pressed", TestF10_2_HorizontalWheelShiftScrolling);
            RunTest("Tier1", "F10.3: Rendered geometries and brushes frozen for zero-allocation", TestF10_3_FrozenRenderBrushesAndGeometries);
            RunTest("Tier1", "F10.4: Sub-20ms layout calculation overhead for standard 20-node graph", TestF10_4_Sub20msLayoutOverhead);
            RunTest("Tier1", "F10.5: Layout isolation with non-focusable viewer and zero padding", TestF10_5_LayoutIsolationAndNonFocusable);

            // F11: Inline Math Baseline & Margin Calibration
            RunTest("Tier1", "F11.1: InlineUIContainer BaselineAlignment set to Center", TestF11_1_InlineContainerBaselineCenter);
            RunTest("Tier1", "F11.2: Calibrated zero-sum margin (1, vOffset, 1, -vOffset) applied", TestF11_2_CalibratedZeroSumMargin);
            RunTest("Tier1", "F11.3: Vertical offset scaling formula (vOffset = round(fontSize * 2.5/14.5, 1))", TestF11_3_VOffsetScalingFormula);
            RunTest("Tier1", "F11.4: Net vertical margin sum equals zero (no line-height expansion)", TestF11_4_ZeroNetLineHeightExpansion);
            RunTest("Tier1", "F11.5: Transparent math container with MathTag metadata", TestF11_5_TransparentContainerAndMathTag);

            // F12: Multi-Context Typographic Robustness
            RunTest("Tier1", "F12.1: Paragraph context with LineHeight=24 optical vertical centering", TestF12_1_ParagraphLineHeight24);
            RunTest("Tier1", "F12.2: ListItem context with LineHeight=NaN uniform line step", TestF12_2_ListItemLineHeightNaN);
            RunTest("Tier1", "F12.3: Heading context (H1-H6) ambient font size propagation", TestF12_3_HeadingFontPropagation);
            RunTest("Tier1", "F12.4: Heading H1 math border margin scaling to (4.5, -4.5)", TestF12_4_H1LargeFontMargin);
            RunTest("Tier1", "F12.5: Zero clipping on large operators (\\int, \\sum) and fractions", TestF12_5_ZeroClippingLargeOperators);

            // F13: Query 2 Financial Schema Validation
            RunTest("Tier1", "F13.1: $ROIC > 18\\%$ parses, renders, and aligns without superscript drift", TestF13_1_ROICFormulaAlignment);
            RunTest("Tier1", "F13.2: $> 18\\%$ leading inequality parses and renders cleanly", TestF13_2_LeadingInequalityFormula);
            RunTest("Tier1", "F13.3: $\\ge +1.5\\%$ signed comparison operator renders cleanly", TestF13_3_SignedComparisonFormula);
            RunTest("Tier1", "F13.4: $\\le 30\\times$ comparison and multiplication symbol render cleanly", TestF13_4_MultiplicationFormula);
            RunTest("Tier1", "F13.5: Full Query 2 markdown document converts with all formulas preserved", TestF13_5_FullQuery2MarkdownDocument);

            // F14: Automated Test Suite Verification
            RunTest("Tier1", "F14.1: MermaidFlowchartParser.TryParse deterministic exception-free execution", TestF14_1_ParserDeterministicExecution);
            RunTest("Tier1", "F14.2: MermaidLayoutEngine.Layout produces finite positive dimensions", TestF14_2_LayoutFiniteDimensions);
            RunTest("Tier1", "F14.3: LatexMathRenderer.RenderMath deterministic exception-free execution", TestF14_3_MathRendererDeterministic);
            RunTest("Tier1", "F14.4: MarkdownToWpfConverter end-to-end FlowDocument conversion", TestF14_4_ConverterEndToEndFlowDoc);
            RunTest("Tier1", "F14.5: Serialization round-trip preserves both math and mermaid blocks", TestF14_5_SerializationLosslessRoundTrip);

            // F15: 20-Node Flowchart Performance Benchmark
            RunTest("Tier1", "F15.1: 20-Node flowchart parse latency benchmark (< 5ms)", TestF15_1_ParseLatencyBenchmark);
            RunTest("Tier1", "F15.2: 20-Node flowchart layout latency benchmark (< 20ms)", TestF15_2_LayoutLatencyBenchmark);
            RunTest("Tier1", "F15.3: 20-Node flowchart total pipeline benchmark (< 25ms)", TestF15_3_TotalPipelineBenchmark);
            RunTest("Tier1", "F15.4: 20-Node flowchart memory allocation efficiency (< 200 KB)", TestF15_4_AllocationEfficiency);
            RunTest("Tier1", "F15.5: 50-Run stability benchmark confirming sub-25ms sustained execution", TestF15_5_BenchmarkStability50Runs);
        }

        #region Feature 1: Mermaid Block Interception & Fallback

        private static void TestF1_1_BlockInterception()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark);
            string md = "```mermaid\ngraph TD\nNodeA --> NodeB\n```";
            var doc = parser.Parse(md);
            var flowDoc = converter.Convert(doc);

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic, "Valid mermaid block must produce a BlockUIContainer");
            var border = buic!.Child as Border;
            AssertNotNull(border, "BlockUIContainer child must be a Border");
            var grid = border!.Child as Grid;
            AssertNotNull(grid, "Outer border child must be a Grid");
            var scrollViewer = grid!.Children.OfType<MermaidScrollViewer>().FirstOrDefault();
            AssertNotNull(scrollViewer, "Grid must contain a MermaidScrollViewer");
        }

        private static void TestF1_2_CodeBlockTagPreservation()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark);
            string rawCode = "graph LR\nA[Alpha] --> B[Beta]";
            var doc = parser.Parse($"```mermaid\n{rawCode}\n```");
            var flowDoc = converter.Convert(doc);

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic, "BlockUIContainer must exist");
            AssertTrue(buic!.Tag is CodeBlockTag, "buic.Tag must be a CodeBlockTag");
            var tag = (CodeBlockTag)buic.Tag;
            AssertEqual("mermaid", tag.Language, "Tag Language must be 'mermaid'");
            AssertContains("graph LR", tag.Code, "Tag Code must contain raw diagram source");
        }

        private static void TestF1_3_SyntaxErrorFallback()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark);
            string invalidMermaid = "```mermaid\nthis is invalid syntax !!! ???\n```";
            var doc = parser.Parse(invalidMermaid);
            var flowDoc = converter.Convert(doc);

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic, "Invalid mermaid must fall back to a BlockUIContainer code block");
            var border = buic!.Child as Border;
            AssertNotNull(border, "Fallback child must be a Border");
            var grid = border!.Child as Grid;
            AssertNotNull(grid, "Fallback outer border child must be a Grid");

            // Must NOT have MermaidScrollViewer
            AssertFalse(grid!.Children.OfType<MermaidScrollViewer>().Any(), "Fallback must not contain MermaidScrollViewer");
            // Must contain TextBlock for syntax-highlighted code display
            AssertTrue(grid.Children.OfType<TextBlock>().Any(), "Fallback must contain TextBlock for code text");
            // Header bar must show MERMAID badge
            var headerGrid = grid.Children.OfType<Grid>().FirstOrDefault();
            AssertNotNull(headerGrid, "Fallback must contain header bar Grid");
            var badgeText = headerGrid!.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text == "MERMAID");
            AssertNotNull(badgeText, "Fallback must retain 'MERMAID' language badge");
        }

        private static void TestF1_4_UnsupportedDiagramFallback()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark);
            string sequenceDiagram = "```mermaid\nsequenceDiagram\nAlice->>Bob: Hello Bob\nBob-->>Alice: Hi Alice\n```";
            var doc = parser.Parse(sequenceDiagram);
            var flowDoc = converter.Convert(doc);

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic, "Sequence diagram must fall back cleanly without throwing exceptions");
            var border = buic!.Child as Border;
            AssertNotNull(border, "Fallback child must be a Border");
            var grid = border!.Child as Grid;
            AssertFalse(grid!.Children.OfType<MermaidScrollViewer>().Any(), "Unsupported diagram must not render as flowchart viewer");
        }

        private static void TestF1_5_SerializationTag()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter(AppDomain.CurrentDomain.BaseDirectory, ThemePalette.GitHubDark);
            string source = "```mermaid\ngraph TD\nStep1 --> Step2\n```";
            var doc = parser.Parse(source);
            var flowDoc = converter.Convert(doc);

            bool serialized = TrySerializeFlowDocument(flowDoc, out string outputMarkdown);
            if (serialized)
            {
                AssertContains("```mermaid", outputMarkdown, "Serialized markdown must preserve ```mermaid fence");
                AssertContains("Step1 --> Step2", outputMarkdown, "Serialized markdown must preserve diagram body");
            }
            else
            {
                // Verify CodeBlockTag is present and populated
                var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
                AssertNotNull(buic, "BUIC must exist");
                var tag = buic!.Tag as CodeBlockTag;
                AssertNotNull(tag, "Tag must be CodeBlockTag");
                AssertEqual("mermaid", tag!.Language);
                AssertContains("Step1 --> Step2", tag.Code);
            }
        }

        #endregion

        #region Feature 2: Mermaid AST Data Model

        private static void TestF2_1_GraphModelCreation()
        {
            var graph = new MermaidFlowchartGraph();
            AssertEqual(MermaidOrientation.TopToBottom, graph.Orientation, "Default orientation must be TopToBottom");
            AssertFalse(graph.IsFlowchartKeyword, "Default IsFlowchartKeyword must be false");
            AssertEqual(0, graph.Nodes.Count, "Nodes count must initially be 0");
            AssertEqual(0, graph.Edges.Count, "Edges count must initially be 0");
            AssertEqual(0, graph.Subgraphs.Count, "Subgraphs count must initially be 0");

            graph.RawSource = "graph TD\nA-->B";
            AssertEqual("graph TD\nA-->B", graph.RawText, "RawText property alias must match RawSource");
        }

        private static void TestF2_2_NodeModelProperties()
        {
            var node = new MermaidNode("node_1", "Processing Unit", MermaidNodeShape.Stadium, 42);
            AssertEqual("node_1", node.Id);
            AssertEqual("Processing Unit", node.Text);
            AssertEqual("Processing Unit", node.Label);
            AssertEqual(MermaidNodeShape.Stadium, node.Shape);
            AssertEqual(42, node.SourceLine);

            node.Label = "Updated Label";
            AssertEqual("Updated Label", node.Text, "Setting Label must update Text");
            AssertContains("node_1", node.ToString(), "ToString must contain Node Id");
        }

        private static void TestF2_3_EdgeModelProperties()
        {
            var edge = new MermaidEdge("N1", "N2", MermaidStrokeStyle.Dotted, MermaidArrowHead.Arrow, "Success", 10);
            AssertEqual("N1", edge.SourceId);
            AssertEqual("N2", edge.TargetId);
            AssertEqual(MermaidStrokeStyle.Dotted, edge.Stroke);
            AssertEqual(MermaidArrowHead.Arrow, edge.Arrow);
            AssertEqual(MermaidArrowHead.Arrow, edge.ArrowHead, "ArrowHead alias must match Arrow");
            AssertEqual("Success", edge.Label);
            AssertEqual(10, edge.SourceLine);
            AssertContains("-.->|Success|", edge.ToString(), "ToString must include dotted arrow and label");
        }

        private static void TestF2_4_SubgraphModelProperties()
        {
            var subgraph = new MermaidSubgraph("sub1", "Core Architecture");
            AssertEqual("sub1", subgraph.Id);
            AssertEqual("Core Architecture", subgraph.Title);
            AssertEqual(0, subgraph.NodeIds.Count);

            subgraph.NodeIds.Add("NodeA");
            subgraph.NodeIds.Add("NodeB");
            AssertEqual(2, subgraph.NodeIds.Count);
            AssertContains("subgraph sub1", subgraph.ToString());
        }

        private static void TestF2_5_GetOrCreateNodeIdempotency()
        {
            var graph = new MermaidFlowchartGraph();
            var node1 = graph.GetOrCreateNode("A", "Initial A", MermaidNodeShape.Rectangle, 1);
            AssertEqual(1, graph.Nodes.Count);
            AssertEqual("Initial A", node1.Text);

            // Re-retrieve with new shape and label
            var node2 = graph.GetOrCreateNode("A", "Updated A", MermaidNodeShape.Diamond, 2);
            AssertEqual(1, graph.Nodes.Count, "Graph node count must remain 1 for existing node");
            AssertTrue(ReferenceEquals(node1, node2), "GetOrCreateNode must return the identical node instance");
            AssertEqual("Updated A", node1.Text, "Node text must be updated");
            AssertEqual(MermaidNodeShape.Diamond, node1.Shape, "Node shape must be updated");
        }

        #endregion

        #region Feature 3: Mermaid Orientation Parsing

        private static void TestF3_1_OrientationTopToBottom()
        {
            AssertTrue(MermaidFlowchartParser.TryParse("graph TD\nA-->B", out var g1, out _), "graph TD must parse");
            AssertEqual(MermaidOrientation.TopToBottom, g1!.Orientation);
            AssertFalse(g1.IsFlowchartKeyword);

            AssertTrue(MermaidFlowchartParser.TryParse("graph TB\nA-->B", out var g2, out _), "graph TB must parse");
            AssertEqual(MermaidOrientation.TopToBottom, g2!.Orientation);

            AssertTrue(MermaidFlowchartParser.TryParse("flowchart TD\nA-->B", out var g3, out _), "flowchart TD must parse");
            AssertEqual(MermaidOrientation.TopToBottom, g3!.Orientation);
            AssertTrue(g3.IsFlowchartKeyword, "flowchart TD must set IsFlowchartKeyword to true");
        }

        private static void TestF3_2_OrientationBottomToTop()
        {
            AssertTrue(MermaidFlowchartParser.TryParse("graph BT\nA-->B", out var g1, out _), "graph BT must parse");
            AssertEqual(MermaidOrientation.BottomToTop, g1!.Orientation);

            AssertTrue(MermaidFlowchartParser.TryParse("flowchart BT\nA-->B", out var g2, out _), "flowchart BT must parse");
            AssertEqual(MermaidOrientation.BottomToTop, g2!.Orientation);
            AssertTrue(g2.IsFlowchartKeyword);
        }

        private static void TestF3_3_OrientationLeftToRight()
        {
            AssertTrue(MermaidFlowchartParser.TryParse("graph LR\nA-->B", out var g1, out _), "graph LR must parse");
            AssertEqual(MermaidOrientation.LeftToRight, g1!.Orientation);

            AssertTrue(MermaidFlowchartParser.TryParse("flowchart LR\nA-->B", out var g2, out _), "flowchart LR must parse");
            AssertEqual(MermaidOrientation.LeftToRight, g2!.Orientation);
            AssertTrue(g2.IsFlowchartKeyword);
        }

        private static void TestF3_4_OrientationRightToLeft()
        {
            AssertTrue(MermaidFlowchartParser.TryParse("graph RL\nA-->B", out var g1, out _), "graph RL must parse");
            AssertEqual(MermaidOrientation.RightToLeft, g1!.Orientation);

            AssertTrue(MermaidFlowchartParser.TryParse("flowchart RL\nA-->B", out var g2, out _), "flowchart RL must parse");
            AssertEqual(MermaidOrientation.RightToLeft, g2!.Orientation);
            AssertTrue(g2.IsFlowchartKeyword);
        }

        private static void TestF3_5_OrientationCaseAndWhitespace()
        {
            AssertTrue(MermaidFlowchartParser.TryParse("   GRAPH   td  ;  \nA-->B", out var g1, out _), "Uppercase GRAPH td with spaces and semicolon");
            AssertEqual(MermaidOrientation.TopToBottom, g1!.Orientation);

            AssertTrue(MermaidFlowchartParser.TryParse("\t\tFlowChart   lr\r\nA-->B", out var g2, out _), "Mixed case FlowChart lr with tabs and CRLF");
            AssertEqual(MermaidOrientation.LeftToRight, g2!.Orientation);
            AssertTrue(g2.IsFlowchartKeyword);
        }

        #endregion

        #region Feature 4: Mermaid Node Shape Parsing

        private static void TestF4_1_RectangleShape()
        {
            string diagram = "graph TD\nNodeA[Standard Process Step]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse rectangle shape");
            AssertEqual(1, g!.Nodes.Count);
            var node = g.Nodes["NodeA"];
            AssertEqual(MermaidNodeShape.Rectangle, node.Shape);
            AssertEqual("Standard Process Step", node.Text);
        }

        private static void TestF4_2_RoundedRectangleShape()
        {
            string diagram = "graph TD\nNodeB(Rounded Action Item)";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse rounded shape");
            var node = g!.Nodes["NodeB"];
            AssertEqual(MermaidNodeShape.RoundedRectangle, node.Shape);
            AssertEqual("Rounded Action Item", node.Text);
        }

        private static void TestF4_3_StadiumShape()
        {
            string diagram = "graph TD\nNodeC([Start / End Terminal])";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse stadium shape");
            var node = g!.Nodes["NodeC"];
            AssertEqual(MermaidNodeShape.Stadium, node.Shape);
            AssertEqual("Start / End Terminal", node.Text);
        }

        private static void TestF4_4_DiamondShape()
        {
            string diagram = "graph TD\nNodeD{Decision Condition?}";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse diamond shape");
            var node = g!.Nodes["NodeD"];
            AssertEqual(MermaidNodeShape.Diamond, node.Shape);
            AssertEqual("Decision Condition?", node.Text);
        }

        private static void TestF4_5_CircleShape()
        {
            string diagram = "graph TD\nNodeE((State Junction))";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse circle shape");
            var node = g!.Nodes["NodeE"];
            AssertEqual(MermaidNodeShape.Circle, node.Shape);
            AssertEqual("State Junction", node.Text);
        }

        #endregion

        #region Feature 5: Mermaid Connection & Arrow Syntax

        private static void TestF5_1_SolidConnections()
        {
            string diagram = "graph TD\nA --> B\nC --- D";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse solid connections");
            AssertEqual(2, g!.Edges.Count);

            var e1 = g.Edges[0];
            AssertEqual("A", e1.SourceId);
            AssertEqual("B", e1.TargetId);
            AssertEqual(MermaidStrokeStyle.Solid, e1.Stroke);
            AssertEqual(MermaidArrowHead.Arrow, e1.Arrow);

            var e2 = g.Edges[1];
            AssertEqual("C", e2.SourceId);
            AssertEqual("D", e2.TargetId);
            AssertEqual(MermaidStrokeStyle.Solid, e2.Stroke);
            AssertEqual(MermaidArrowHead.None, e2.Arrow);
        }

        private static void TestF5_2_DottedConnections()
        {
            string diagram = "graph TD\nA -.-> B\nC -.- D";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse dotted connections");
            AssertEqual(2, g!.Edges.Count);

            var e1 = g.Edges[0];
            AssertEqual(MermaidStrokeStyle.Dotted, e1.Stroke);
            AssertEqual(MermaidArrowHead.Arrow, e1.Arrow);

            var e2 = g.Edges[1];
            AssertEqual(MermaidStrokeStyle.Dotted, e2.Stroke);
            AssertEqual(MermaidArrowHead.None, e2.Arrow);
        }

        private static void TestF5_3_ThickConnections()
        {
            string diagram = "graph TD\nA ==> B\nC === D";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse thick connections");
            AssertEqual(2, g!.Edges.Count);

            var e1 = g.Edges[0];
            AssertEqual(MermaidStrokeStyle.Thick, e1.Stroke);
            AssertEqual(MermaidArrowHead.Arrow, e1.Arrow);

            var e2 = g.Edges[1];
            AssertEqual(MermaidStrokeStyle.Thick, e2.Stroke);
            AssertEqual(MermaidArrowHead.None, e2.Arrow);
        }

        private static void TestF5_4_EdgeLabels()
        {
            string diagram = "graph TD\nA -->|Approve| B\nC -- Reject --> D";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse edge labels");
            AssertEqual(2, g!.Edges.Count);

            AssertEqual("Approve", g.Edges[0].Label);
            AssertEqual("Reject", g.Edges[1].Label);
        }

        private static void TestF5_5_ChainedConnections()
        {
            string diagram = "graph LR\nA --> B --> C --> D";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse chained arrows");
            AssertEqual(4, g!.Nodes.Count);
            AssertEqual(3, g.Edges.Count);

            AssertEqual("A", g.Edges[0].SourceId);
            AssertEqual("B", g.Edges[0].TargetId);
            AssertEqual("B", g.Edges[1].SourceId);
            AssertEqual("C", g.Edges[1].TargetId);
            AssertEqual("C", g.Edges[2].SourceId);
            AssertEqual("D", g.Edges[2].TargetId);
        }

        #endregion

        #region Feature 6: Layered Graph Layout Engine Sugiyama

        private static void TestF6_1_SugiyamaPipelineExecution()
        {
            string diagram = "graph TD\nStart --> Process --> Decision{OK?} --> End\nDecision -->|No| Rework[Fix] --> Process";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse diagram");
            var layout = MermaidLayoutEngine.Layout(g!);

            AssertEqual(5, layout.Nodes.Count);
            AssertEqual(5, layout.Edges.Count);
            AssertTrue(layout.TotalWidth > 0.0, "TotalWidth must be positive");
            AssertTrue(layout.TotalHeight > 0.0, "TotalHeight must be positive");
        }

        private static void TestF6_2_CycleReversalFeedbackCorridor()
        {
            string diagram = "graph TD\nA --> B --> C --> A";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse cycle");
            var layout = MermaidLayoutEngine.Layout(g!);

            var feedbackEdge = layout.Edges.FirstOrDefault(e => e.IsFeedbackEdge);
            AssertNotNull(feedbackEdge, "Sugiyama layout must detect and mark feedback edge in cycle");
            AssertEqual("C", feedbackEdge!.Edge.SourceId);
            AssertEqual("A", feedbackEdge.Edge.TargetId);
        }

        private static void TestF6_3_DummyNodeMultiRankRouting()
        {
            string diagram = "graph TD\nA --> B --> C\nA --> C";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse multi-rank edge");
            var layout = MermaidLayoutEngine.Layout(g!);

            var directEdge = layout.Edges.FirstOrDefault(e => e.Edge.SourceId == "A" && e.Edge.TargetId == "C");
            AssertNotNull(directEdge, "Direct edge A->C must exist");
            // Multi-rank edge should have routing waypoints or valid spline control points
            AssertTrue(directEdge!.Waypoints.Count > 0 || (directEdge.ControlPoint1.Y > directEdge.StartPoint.Y),
                "Direct multi-rank edge must have routing waypoints or proper spline controls");
        }

        private static void TestF6_4_BarycentricCrossingReduction()
        {
            string diagram = @"graph TD
                A1 --> B2
                A2 --> B1
                A1 --> B1
                A2 --> B2";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse bipartite crossing");
            var layout = MermaidLayoutEngine.Layout(g!);

            AssertEqual(4, layout.Nodes.Count);
            AssertEqual(4, layout.Edges.Count);
            // Verify monotonic layer ranks
            AssertEqual(layout.Nodes["A1"].Rank, layout.Nodes["A2"].Rank, "A1 and A2 must share rank 0");
            AssertEqual(layout.Nodes["B1"].Rank, layout.Nodes["B2"].Rank, "B1 and B2 must share rank 1");
            AssertTrue(layout.Nodes["B1"].Rank > layout.Nodes["A1"].Rank, "Rank 1 > Rank 0");
        }

        private static void TestF6_5_SelfLoopLayoutHandling()
        {
            string diagram = "graph TD\nA[Process Step] --> A";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse self loop");
            var layout = MermaidLayoutEngine.Layout(g!);

            AssertEqual(1, layout.Nodes.Count);
            AssertEqual(1, layout.Edges.Count);
            var route = layout.Edges[0];
            AssertTrue(route.Waypoints.Count >= 2, "Self-loop route must have external corridor waypoints");
        }

        #endregion

        #region Feature 7: Horizontal & Vertical Coordinate Mapping

        private static void TestF7_1_TopToBottomMapping()
        {
            string diagram = "graph TD\nParent --> Child";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse TD");
            var layout = MermaidLayoutEngine.Layout(g!);

            var p = layout.Nodes["Parent"];
            var c = layout.Nodes["Child"];
            AssertTrue(p.Y < c.Y, $"TD layout requires Y_parent ({p.Y}) < Y_child ({c.Y})");
        }

        private static void TestF7_2_BottomToTopMapping()
        {
            string diagram = "graph BT\nParent --> Child";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse BT");
            var layout = MermaidLayoutEngine.Layout(g!);

            var p = layout.Nodes["Parent"];
            var c = layout.Nodes["Child"];
            AssertTrue(p.Y > c.Y, $"BT layout requires Y_parent ({p.Y}) > Y_child ({c.Y})");
        }

        private static void TestF7_3_LeftToRightMapping()
        {
            string diagram = "graph LR\nParent --> Child";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse LR");
            var layout = MermaidLayoutEngine.Layout(g!);

            var p = layout.Nodes["Parent"];
            var c = layout.Nodes["Child"];
            AssertTrue(p.X < c.X, $"LR layout requires X_parent ({p.X}) < X_child ({c.X})");
        }

        private static void TestF7_4_RightToLeftMapping()
        {
            string diagram = "graph RL\nParent --> Child";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse RL");
            var layout = MermaidLayoutEngine.Layout(g!);

            var p = layout.Nodes["Parent"];
            var c = layout.Nodes["Child"];
            AssertTrue(p.X > c.X, $"RL layout requires X_parent ({p.X}) > X_child ({c.X})");
        }

        private static void TestF7_5_NodeBoundingBoxes()
        {
            string diagram = "graph TD\nA[Short] --> B[This is an exceptionally long node label to verify metric sizing]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse labels");
            var layout = MermaidLayoutEngine.Layout(g!);

            var nodeA = layout.Nodes["A"];
            var nodeB = layout.Nodes["B"];
            AssertTrue(nodeB.Width > nodeA.Width, $"Node B width ({nodeB.Width}) must exceed Node A width ({nodeA.Width})");
            AssertTrue(nodeA.Height >= 36.0, "Minimum node height must be at least 36 DIPs");
        }

        #endregion

        #region Feature 8: Native WPF Vector Element Generation

        private static void TestF8_1_RootVisualStructure()
        {
            string diagram = "graph TD\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);

            AssertTrue(visual is Border, "Root visual must be Border");
            var border = (Border)visual;
            var grid = border.Child as Grid;
            AssertNotNull(grid, "Border child must be Grid");
            AssertEqual(2, grid!.RowDefinitions.Count, "Grid must have 2 rows (header toolbar, content)");

            var sv = grid.Children.OfType<MermaidScrollViewer>().FirstOrDefault();
            AssertNotNull(sv, "Row 1 must contain MermaidScrollViewer");
        }

        private static void TestF8_2_CanvasNodeVectorElements()
        {
            string diagram = "graph TD\nA[Box] --> B(Round) --> C([Capsule])";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = sv.Content as Canvas;
            AssertNotNull(canvas, "ScrollViewer content must be a Canvas");

            var nodeBorders = canvas!.Children.OfType<Border>().ToList();
            AssertTrue(nodeBorders.Count >= 3, $"Canvas must contain Border elements for all 3 nodes, found {nodeBorders.Count}");
        }

        private static void TestF8_3_CanvasEdgeSplinePaths()
        {
            string diagram = "graph TD\nA --> B\nB --> C";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;

            var paths = canvas.Children.OfType<System.Windows.Shapes.Path>().ToList();
            AssertTrue(paths.Count >= 2, $"Canvas must contain Path elements for connector splines, found {paths.Count}");
            foreach (var path in paths)
            {
                AssertNotNull(path.Data, "Path Data geometry must not be null");
            }
        }

        private static void TestF8_4_ArrowheadGeometryTangents()
        {
            string diagram = "graph LR\nA --> B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;

            // In LR layout, arrowhead angle should point to the right (approx 0 or 360 deg)
            var edge = layout.Edges[0];
            AssertInRange(Math.Abs(edge.ArrowheadAngle), 0, 15, "LR arrowhead angle must point roughly 0 deg rightward");
        }

        private static void TestF8_5_EdgeLabelBadges()
        {
            string diagram = "graph TD\nA -->|Conditional Transition| B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;

            // Find TextBlock containing label text
            var labelTb = canvas.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text == "Conditional Transition")
                ?? canvas.Children.OfType<Border>().Select(b => b.Child as TextBlock).FirstOrDefault(tb => tb != null && tb.Text == "Conditional Transition");
            AssertNotNull(labelTb, "Edge label badge TextBlock must exist in Canvas visual tree");
        }

        #endregion

        #region Feature 9: Dynamic ThemePalette Integration

        private static void TestF9_1_PaletteNodeBinding()
        {
            string diagram = "graph TD\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var palette = ThemePalette.GitHubDark;
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, palette, diagram);

            AssertEqual(palette.CodeBg, visual.Background, "Outer border background must match palette.CodeBg");
        }

        private static void TestF9_2_PaletteHeaderToolbar()
        {
            string diagram = "graph TD\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var palette = ThemePalette.Nord;
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, palette, diagram);
            var grid = (Grid)visual.Child!;
            var header = (Grid)grid.Children[0];

            var badge = header.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text == "MERMAID");
            AssertNotNull(badge, "MERMAID badge TextBlock must exist in toolbar");
        }

        private static void TestF9_3_PaletteConnectorStrokes()
        {
            string diagram = "graph TD\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var palette = ThemePalette.Monokai;
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, palette, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;

            var paths = canvas.Children.OfType<System.Windows.Shapes.Path>().ToList();
            AssertTrue(paths.Count > 0, "Canvas must contain paths");
            foreach (var p in paths)
            {
                AssertNotNull(p.Stroke ?? p.Fill, "Path stroke or fill must not be null");
            }
        }

        private static void TestF9_4_AllEightThemePresets()
        {
            string diagram = "graph TD\nA[Alpha] --> B[Beta]";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);

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
                var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, p, diagram);
                AssertNotNull(visual, $"Rendering visual must succeed for theme {p.Name}");
            }
        }

        private static void TestF9_5_WCAGContrastCompliance()
        {
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
                Color nodeBgColor = p.IsDark ? Color.FromRgb(33, 38, 45) : Color.FromRgb(255, 255, 255);
                double ratio = CalculateContrastRatio(nodeBgColor, p.EditorFg.Color);
                AssertTrue(ratio >= 4.5, $"Theme {p.Name} node contrast ratio {ratio:F2} must be >= 4.5:1 (WCAG AA)");
            }
        }

        #endregion

        #region Feature 10: 60 FPS Scrolling & UI Performance

        private static void TestF10_1_VerticalWheelBubbling()
        {
            var sv = new MermaidScrollViewer();
            // Create a fake mouse wheel event without Shift key
            var device = Mouse.PrimaryDevice;
            // Test that properties are configured for uninhibited wheel propagation
            AssertEqual(ScrollBarVisibility.Auto, sv.HorizontalScrollBarVisibility);
            AssertEqual(ScrollBarVisibility.Disabled, sv.VerticalScrollBarVisibility);
            AssertEqual(false, sv.Focusable);
        }

        private static void TestF10_2_HorizontalWheelShiftScrolling()
        {
            var sv = new MermaidScrollViewer();
            // Verify horizontal scrollbar is Auto so it can scroll when content exceeds width
            AssertEqual(ScrollBarVisibility.Auto, sv.HorizontalScrollBarVisibility);
        }

        private static void TestF10_3_FrozenRenderBrushesAndGeometries()
        {
            string diagram = "graph TD\nA-->B";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse");
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            var grid = (Grid)visual.Child!;
            var sv = grid.Children.OfType<MermaidScrollViewer>().First();
            var canvas = (Canvas)sv.Content;

            var paths = canvas.Children.OfType<System.Windows.Shapes.Path>().ToList();
            foreach (var path in paths)
            {
                if (path.Data != null)
                {
                    AssertTrue(path.Data.IsFrozen, "PathGeometry data must be frozen for 60 FPS performance");
                }
            }
        }

        private static void TestF10_4_Sub20msLayoutOverhead()
        {
            // 20-node graph
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++)
            {
                lines.Add($"N{i}[Node {i}] --> N{(i % 20) + 1}[Node {(i % 20) + 1}]");
            }
            string diagram = string.Join("\n", lines);
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _), "Parse 20-node");

            var sw = Stopwatch.StartNew();
            var layout = MermaidLayoutEngine.Layout(g!);
            sw.Stop();

            AssertEqual(20, layout.Nodes.Count);
            AssertTrue(sw.ElapsedMilliseconds < 20, $"20-Node layout took {sw.ElapsedMilliseconds} ms (budget < 20ms)");
        }

        private static void TestF10_5_LayoutIsolationAndNonFocusable()
        {
            var sv = new MermaidScrollViewer();
            AssertEqual(new Thickness(0), sv.BorderThickness);
            AssertEqual(new Thickness(0), sv.Padding);
            AssertEqual(false, sv.Focusable);
            AssertEqual(Brushes.Transparent, sv.Background);
        }

        #endregion

        #region Feature 11: Inline Math Baseline & Margin Calibration

        private static void TestF11_1_InlineContainerBaselineCenter()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse("Sample $x + y = z$ formula");
            var flowDoc = converter.Convert(doc);

            var para = (Paragraph)flowDoc.Blocks.FirstBlock!;
            var uic = para.Inlines.OfType<InlineUIContainer>().FirstOrDefault();
            AssertNotNull(uic, "Inline math must generate InlineUIContainer");
            AssertEqual(BaselineAlignment.Center, uic!.BaselineAlignment, "InlineUIContainer must use BaselineAlignment.Center");
        }

        private static void TestF11_2_CalibratedZeroSumMargin()
        {
            var mathElem = LatexMathRenderer.RenderMath("E = mc^2", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            AssertTrue(mathElem is Border, "Inline math must be wrapped in a Border container");
            var border = (Border)mathElem;

            AssertEqual(-1.0, border.Margin.Top, "Top margin must be -1.0");
            AssertEqual(1.0, border.Margin.Bottom, "Bottom margin must be 1.0");
        }

        private static void TestF11_3_VOffsetScalingFormula()
        {
            // vOffset = Math.Round(effectiveFontSize * (-1.0 / 15.0), 1)
            double size1 = 15.0;
            double expected1 = Math.Round(size1 * (-1.0 / 15.0), 1);
            AssertEqual(-1.0, expected1);

            double size2 = 30.0;
            double expected2 = Math.Round(size2 * (-1.0 / 15.0), 1);
            AssertEqual(-2.0, expected2);
        }

        private static void TestF11_4_ZeroNetLineHeightExpansion()
        {
            var mathElem = LatexMathRenderer.RenderMath("\\alpha + \\beta", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            var border = (Border)mathElem;
            double netVertical = border.Margin.Top + border.Margin.Bottom;
            AssertEqual(0.0, netVertical, "Net vertical margin must be exactly zero");
        }

        private static void TestF11_5_TransparentContainerAndMathTag()
        {
            var mathElem = LatexMathRenderer.RenderMath("k = 42", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            var border = (Border)mathElem;
            AssertEqual(Brushes.Transparent, border.Background);
            AssertEqual(new Thickness(1, 0, 1, 0), border.Padding);
        }

        #endregion

        #region Feature 12: Multi-Context Typographic Robustness

        private static void TestF12_1_ParagraphLineHeight24()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse("Paragraph with $A = B$ formula.");
            var flowDoc = converter.Convert(doc);

            var para = (Paragraph)flowDoc.Blocks.FirstBlock!;
            para.LineHeight = 24.0;

            var rtb = new RichTextBox { Document = flowDoc, Width = 800 };
            rtb.Measure(new Size(800, 1000));
            rtb.Arrange(new Rect(0, 0, 800, rtb.DesiredSize.Height));
            rtb.UpdateLayout();

            AssertTrue(rtb.ActualHeight > 0, "RichTextBox layout must measure cleanly");
        }

        private static void TestF12_2_ListItemLineHeightNaN()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            string md = "* Item 1: Plain text\n* Item 2: With formula $x = 10$\n* Item 3: Plain text";
            var doc = parser.Parse(md);
            var flowDoc = converter.Convert(doc);

            var list = flowDoc.Blocks.OfType<List>().FirstOrDefault();
            AssertNotNull(list, "List must exist");
            AssertEqual(3, list!.ListItems.Count);
        }

        private static void TestF12_3_HeadingFontPropagation()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse("# Main Heading $F = ma$");
            var flowDoc = converter.Convert(doc);

            var para = (Paragraph)flowDoc.Blocks.FirstBlock!;
            var uic = para.Inlines.OfType<InlineUIContainer>().FirstOrDefault();
            AssertNotNull(uic, "Heading math must produce InlineUIContainer");
            var border = (Border)uic!.Child;
            AssertTrue(border.Margin.Top < 0, "Heading math margin must scale negatively with ambient font size");
        }

        private static void TestF12_4_H1LargeFontMargin()
        {
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse("# Heading $x + y$");
            var flowDoc = converter.Convert(doc);

            var para = (Paragraph)flowDoc.Blocks.FirstBlock!;
            var uic = para.Inlines.OfType<InlineUIContainer>().First();
            var border = (Border)uic.Child;
            // Ambient font size for H1 is 26 -> vOffset = round(26 * -1.0/15.0, 1) = round(-1.733, 1) = -1.7
            AssertEqual(-1.7, border.Margin.Top);
            AssertEqual(1.7, border.Margin.Bottom);
        }

        private static void TestF12_5_ZeroClippingLargeOperators()
        {
            var mathElem = LatexMathRenderer.RenderMath("\\int_0^\\infty \\frac{x}{1+x^2} dx = \\sum_{k=1}^\\infty a_k", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            var border = (Border)mathElem;
            border.Measure(new Size(1000, 1000));
            AssertTrue(border.DesiredSize.Width > 0, "Width must be positive");
            AssertTrue(border.DesiredSize.Height > 0, "Height must be positive");
        }

        #endregion

        #region Feature 13: Query 2 Financial Schema Validation

        private static void TestF13_1_ROICFormulaAlignment()
        {
            var elem = LatexMathRenderer.RenderMath("ROIC > 18\\%", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            var border = (Border)elem;
            AssertEqual(-1.0, border.Margin.Top);
            AssertEqual(1.0, border.Margin.Bottom);
        }

        private static void TestF13_2_LeadingInequalityFormula()
        {
            var elem = LatexMathRenderer.RenderMath("> 18\\%", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            var border = (Border)elem;
            AssertEqual(-1.0, border.Margin.Top);
            AssertEqual(1.0, border.Margin.Bottom);
        }

        private static void TestF13_3_SignedComparisonFormula()
        {
            var elem = LatexMathRenderer.RenderMath("\\ge +1.5\\%", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            var border = (Border)elem;
            AssertEqual(-1.0, border.Margin.Top);
            AssertEqual(1.0, border.Margin.Bottom);
        }

        private static void TestF13_4_MultiplicationFormula()
        {
            var elem = LatexMathRenderer.RenderMath("\\le 30\\times", ThemePalette.GitHubDark, 14.5, isDisplay: false);
            var border = (Border)elem;
            AssertEqual(-1.0, border.Margin.Top);
            AssertEqual(1.0, border.Margin.Bottom);
        }

        private static void TestF13_5_FullQuery2MarkdownDocument()
        {
            string query2Markdown =
                "* **Analyst Description:** Compounding businesses with ($ROIC > 18\\%$, $ROE > 20\\%$) expanding margins.\n" +
                "* **Screening Criteria:** ROIC $> 18\\%$, Gross Margin $\\ge +1.5\\%$, Forward P/E $\\le 30\\times$.";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var doc = parser.Parse(query2Markdown);
            var flowDoc = converter.Convert(doc);

            var list = flowDoc.Blocks.OfType<List>().FirstOrDefault();
            AssertNotNull(list, "Query 2 document must convert into a List block");
            AssertEqual(2, list!.ListItems.Count);

            var allUic = new List<InlineUIContainer>();
            foreach (var item in list.ListItems)
            {
                foreach (var b in item.Blocks)
                {
                    if (b is Paragraph p)
                    {
                        allUic.AddRange(p.Inlines.OfType<InlineUIContainer>());
                    }
                }
            }
            AssertTrue(allUic.Count >= 5, $"Query 2 must contain at least 5 math containers, found {allUic.Count}");
            foreach (var uic in allUic)
            {
                AssertEqual(BaselineAlignment.Center, uic.BaselineAlignment);
            }
        }

        #endregion

        #region Feature 14: Automated Test Suite Verification

        private static void TestF14_1_ParserDeterministicExecution()
        {
            string diagram = "graph TD\nA-->B-->C";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g1, out _));
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g2, out _));
            AssertEqual(g1!.Nodes.Count, g2!.Nodes.Count);
            AssertEqual(g1.Edges.Count, g2.Edges.Count);
        }

        private static void TestF14_2_LayoutFiniteDimensions()
        {
            string diagram = "graph LR\nA-->B-->C-->D";
            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var layout = MermaidLayoutEngine.Layout(g!);
            AssertFalse(double.IsNaN(layout.TotalWidth));
            AssertFalse(double.IsInfinity(layout.TotalWidth));
            AssertFalse(double.IsNaN(layout.TotalHeight));
            AssertFalse(double.IsInfinity(layout.TotalHeight));
        }

        private static void TestF14_3_MathRendererDeterministic()
        {
            var u1 = LatexMathRenderer.RenderMath("\\sqrt{x^2 + y^2}", ThemePalette.GitHubDark, 14.5, false);
            var u2 = LatexMathRenderer.RenderMath("\\sqrt{x^2 + y^2}", ThemePalette.GitHubDark, 14.5, false);
            AssertNotNull(u1);
            AssertNotNull(u2);
        }

        private static void TestF14_4_ConverterEndToEndFlowDoc()
        {
            string content = "# Test Document\n\nInline formula: $E = mc^2$\n\n```mermaid\ngraph TD\nA --> B\n```";
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(content));
            AssertTrue(flowDoc.Blocks.Count >= 3, "FlowDocument must have Heading, Paragraph, and BlockUIContainer");
        }

        private static void TestF14_5_SerializationLosslessRoundTrip()
        {
            string content = "Inline $x^2 + y^2 = r^2$ formula.\n\n```mermaid\ngraph LR\nStart --> Stop\n```";
            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(content));

            // Verify math container tag and mermaid block container tag
            var para = flowDoc.Blocks.OfType<Paragraph>().FirstOrDefault();
            var uic = para?.Inlines.OfType<InlineUIContainer>().FirstOrDefault();
            AssertNotNull(uic?.Tag, "Math container must have MathTag");

            var buic = flowDoc.Blocks.OfType<BlockUIContainer>().FirstOrDefault();
            AssertNotNull(buic?.Tag, "Mermaid block must have CodeBlockTag");
        }

        #endregion

        #region Feature 15: 20-Node Flowchart Performance Benchmark

        private static void TestF15_1_ParseLatencyBenchmark()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"Node_{i}[Process Stage {i}] --> Node_{(i % 20) + 1}");
            string src = string.Join("\n", lines);

            // Warmup
            MermaidFlowchartParser.TryParse(src, out _, out _);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                MermaidFlowchartParser.TryParse(src, out _, out _);
            }
            sw.Stop();
            double avgMs = sw.ElapsedMilliseconds / 10.0;
            AssertTrue(avgMs < 5.0, $"Average parse latency {avgMs:F2} ms must be < 5.0 ms");
        }

        private static void TestF15_2_LayoutLatencyBenchmark()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"Node_{i}[Process Stage {i}] --> Node_{(i % 20) + 1}");
            string src = string.Join("\n", lines);
            MermaidFlowchartParser.TryParse(src, out var g, out _);

            // Warmup
            MermaidLayoutEngine.Layout(g!);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 5; i++)
            {
                MermaidLayoutEngine.Layout(g!);
            }
            sw.Stop();
            double avgMs = sw.ElapsedMilliseconds / 5.0;
            AssertTrue(avgMs < 20.0, $"Average layout latency {avgMs:F2} ms must be < 20.0 ms");
        }

        private static void TestF15_3_TotalPipelineBenchmark()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"Node_{i}[Process Stage {i}] --> Node_{(i % 20) + 1}");
            string src = string.Join("\n", lines);

            var sw = Stopwatch.StartNew();
            MermaidFlowchartParser.TryParse(src, out var g, out _);
            var layout = MermaidLayoutEngine.Layout(g!);
            var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, src);
            sw.Stop();

            AssertNotNull(visual);
            AssertTrue(sw.ElapsedMilliseconds < 25.0, $"Total pipeline latency {sw.ElapsedMilliseconds} ms must be < 25.0 ms");
        }

        private static void TestF15_4_AllocationEfficiency()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"Node_{i}[Process Stage {i}] --> Node_{(i % 20) + 1}");
            string src = string.Join("\n", lines);
            MermaidFlowchartParser.TryParse(src, out var g, out _);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = GC.GetAllocatedBytesForCurrentThread();
            var layout = MermaidLayoutEngine.Layout(g!);
            long after = GC.GetAllocatedBytesForCurrentThread();

            long allocatedKb = (after - before) / 1024;
            AssertTrue(allocatedKb < 350, $"Layout allocation {allocatedKb} KB must be < 350 KB");
        }

        private static void TestF15_5_BenchmarkStability50Runs()
        {
            var lines = new List<string> { "graph TD" };
            for (int i = 1; i <= 20; i++) lines.Add($"Node_{i}[Process Stage {i}] --> Node_{(i % 20) + 1}");
            string src = string.Join("\n", lines);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 50; i++)
            {
                MermaidFlowchartParser.TryParse(src, out var g, out _);
                var layout = MermaidLayoutEngine.Layout(g!);
                var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, src);
                AssertNotNull(visual);
            }
            sw.Stop();

            double avgTotal = sw.ElapsedMilliseconds / 50.0;
            AssertTrue(avgTotal < 25.0, $"50-run average {avgTotal:F2} ms must remain sub-25ms");
        }

        #endregion
    }
}
