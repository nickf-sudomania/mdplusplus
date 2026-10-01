using System;
using System.Diagnostics;
using MDPlus.Core.Mermaid;

namespace MDPlus.Tests
{
    /// <summary>
    /// Comprehensive unit test suite for Mermaid Flowchart AST, Grammar, Tokenizer, and Error Fallback.
    /// Covers orientations, node shapes, connections, labels, chains, subgraphs, comments, error fallback, and benchmark.
    /// </summary>
    public static class MermaidTests
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string name)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception($"{name} mismatch. Expected: '{expected}', Actual: '{actual}'");
            }
        }

        /// <summary>
        /// Validates parsing of all orientation directives across both 'graph' and 'flowchart' keywords.
        /// </summary>
        public static void TestOrientationParsing()
        {
            // TD / TB (Top to Bottom)
            Assert(MermaidFlowchartParser.TryParse("graph TD\nA-->B", out var g1, out _), "graph TD should parse");
            AssertEqual(MermaidOrientation.TopToBottom, g1!.Orientation, "Orientation TD");
            AssertEqual(false, g1.IsFlowchartKeyword, "IsFlowchartKeyword for graph");

            Assert(MermaidFlowchartParser.TryParse("graph TB\nA-->B", out var g2, out _), "graph TB should parse");
            AssertEqual(MermaidOrientation.TopToBottom, g2!.Orientation, "Orientation TB");

            // BT (Bottom to Top)
            Assert(MermaidFlowchartParser.TryParse("graph BT\nA-->B", out var g3, out _), "graph BT should parse");
            AssertEqual(MermaidOrientation.BottomToTop, g3!.Orientation, "Orientation BT");

            // LR (Left to Right)
            Assert(MermaidFlowchartParser.TryParse("graph LR\nA-->B", out var g4, out _), "graph LR should parse");
            AssertEqual(MermaidOrientation.LeftToRight, g4!.Orientation, "Orientation LR");

            // RL (Right to Left)
            Assert(MermaidFlowchartParser.TryParse("graph RL\nA-->B", out var g5, out _), "graph RL should parse");
            AssertEqual(MermaidOrientation.RightToLeft, g5!.Orientation, "Orientation RL");

            // flowchart keyword equivalents
            Assert(MermaidFlowchartParser.TryParse("flowchart TD\nA-->B", out var g6, out _), "flowchart TD should parse");
            AssertEqual(MermaidOrientation.TopToBottom, g6!.Orientation, "Orientation flowchart TD");
            AssertEqual(true, g6.IsFlowchartKeyword, "IsFlowchartKeyword for flowchart");

            Assert(MermaidFlowchartParser.TryParse("flowchart TB\nA-->B", out var g7, out _), "flowchart TB should parse");
            AssertEqual(MermaidOrientation.TopToBottom, g7!.Orientation, "Orientation flowchart TB");

            Assert(MermaidFlowchartParser.TryParse("flowchart BT\nA-->B", out var g8, out _), "flowchart BT should parse");
            AssertEqual(MermaidOrientation.BottomToTop, g8!.Orientation, "Orientation flowchart BT");

            Assert(MermaidFlowchartParser.TryParse("flowchart LR\nA-->B", out var g9, out _), "flowchart LR should parse");
            AssertEqual(MermaidOrientation.LeftToRight, g9!.Orientation, "Orientation flowchart LR");

            Assert(MermaidFlowchartParser.TryParse("flowchart RL\nA-->B", out var g10, out _), "flowchart RL should parse");
            AssertEqual(MermaidOrientation.RightToLeft, g10!.Orientation, "Orientation flowchart RL");

            // Case insensitivity and whitespace handling
            Assert(MermaidFlowchartParser.TryParse("  GRAPH   td  ;  \n A --> B", out var g11, out _), "Case-insensitive GRAPH td");
            AssertEqual(MermaidOrientation.TopToBottom, g11!.Orientation, "Case-insensitive orientation");

            Assert(MermaidFlowchartParser.TryParse("FlowChart   lr  \n A --> B", out var g12, out _), "Case-insensitive FlowChart lr");
            AssertEqual(MermaidOrientation.LeftToRight, g12!.Orientation, "Case-insensitive orientation LR");
        }

        /// <summary>
        /// Validates parsing and label extraction for all 5 node shapes:
        /// Rectangle [], Rounded Rectangle (), Stadium ([]), Diamond {}, and Circle (()).
        /// </summary>
        public static void TestAllNodeShapesAndLabelExtraction()
        {
            string diagram = @"graph TD
    A[Process Block]
    B(Action Step)
    C([Start or Finish])
    D{Decision Required}
    E((State Junction))
    F
";
            Assert(MermaidFlowchartParser.TryParse(diagram, out var graph, out string? error), $"Parse failed: {error}");
            AssertEqual(6, graph!.Nodes.Count, "Node count");

            // 1. Rectangle [text]
            var nodeA = graph.Nodes["A"];
            AssertEqual("A", nodeA.Id, "Node A ID");
            AssertEqual("Process Block", nodeA.Text, "Node A Text");
            AssertEqual("Process Block", nodeA.Label, "Node A Label");
            AssertEqual(MermaidNodeShape.Rectangle, nodeA.Shape, "Node A Shape");
            AssertEqual("A[Process Block]", nodeA.ToString(), "Node A ToString");

            // 2. Rounded Rectangle (text)
            var nodeB = graph.Nodes["B"];
            AssertEqual("B", nodeB.Id, "Node B ID");
            AssertEqual("Action Step", nodeB.Text, "Node B Text");
            AssertEqual(MermaidNodeShape.RoundedRectangle, nodeB.Shape, "Node B Shape");
            AssertEqual("B(Action Step)", nodeB.ToString(), "Node B ToString");

            // 3. Stadium / Pill ([text])
            var nodeC = graph.Nodes["C"];
            AssertEqual("C", nodeC.Id, "Node C ID");
            AssertEqual("Start or Finish", nodeC.Text, "Node C Text");
            AssertEqual(MermaidNodeShape.Stadium, nodeC.Shape, "Node C Shape");
            AssertEqual("C([Start or Finish])", nodeC.ToString(), "Node C ToString");

            // 4. Diamond {text}
            var nodeD = graph.Nodes["D"];
            AssertEqual("D", nodeD.Id, "Node D ID");
            AssertEqual("Decision Required", nodeD.Text, "Node D Text");
            AssertEqual(MermaidNodeShape.Diamond, nodeD.Shape, "Node D Shape");
            AssertEqual("D{Decision Required}", nodeD.ToString(), "Node D ToString");

            // 5. Circle ((text))
            var nodeE = graph.Nodes["E"];
            AssertEqual("E", nodeE.Id, "Node E ID");
            AssertEqual("State Junction", nodeE.Text, "Node E Text");
            AssertEqual(MermaidNodeShape.Circle, nodeE.Shape, "Node E Shape");
            AssertEqual("E((State Junction))", nodeE.ToString(), "Node E ToString");

            // 6. Bare Node
            var nodeF = graph.Nodes["F"];
            AssertEqual("F", nodeF.Id, "Node F ID");
            AssertEqual("F", nodeF.Text, "Node F Text");
            AssertEqual(MermaidNodeShape.Rectangle, nodeF.Shape, "Node F Shape");
        }

        /// <summary>
        /// Validates double-quoted labels, escaped characters, and HTML break line preservation.
        /// </summary>
        public static void TestQuotedAndMultilineNodeLabels()
        {
            string diagram = @"graph LR
    A[""Process [step 1] & (step 2)""]
    B[""Say \""Hello\"" World""]
    C[""Line 1<br/>Line 2<br>Line 3<br />Line 4""]
    D[""$ROIC > 18%$ condition""]
    E[Normal text with (extra) info]
    F{""Diamond with \""quotes\""""}
";
            Assert(MermaidFlowchartParser.TryParse(diagram, out var graph, out string? error), $"Parse failed: {error}");
            AssertEqual(6, graph!.Nodes.Count, "Node count");

            // Quoted with brackets
            AssertEqual("Process [step 1] & (step 2)", graph.Nodes["A"].Text, "Node A Text with brackets");

            // Escaped quotes
            AssertEqual("Say \"Hello\" World", graph.Nodes["B"].Text, "Node B Text with escaped quotes");

            // Multiline HTML breaks converted to \n
            string expectedC = "Line 1\nLine 2\nLine 3\nLine 4";
            AssertEqual(expectedC, graph.Nodes["C"].Text, "Node C Multiline HTML breaks");

            // Mathematical formulas in quotes
            AssertEqual("$ROIC > 18%$ condition", graph.Nodes["D"].Text, "Node D Formula label");

            // Unquoted with internal parentheses
            AssertEqual("Normal text with (extra) info", graph.Nodes["E"].Text, "Node E Unquoted with parens");

            // Diamond with quotes
            AssertEqual("Diamond with \"quotes\"", graph.Nodes["F"].Text, "Node F Diamond with quotes");
            AssertEqual(MermaidNodeShape.Diamond, graph.Nodes["F"].Shape, "Node F Diamond shape");
        }

        /// <summary>
        /// Validates all 6 connection line types (solid, dotted, thick) and arrowheads (directed, undirected).
        /// </summary>
        public static void TestAllConnectionTypesAndArrows()
        {
            string diagram = @"graph TD
    A --> B
    B --- C
    C -.-> D
    D -.- E
    E ==> F
    F === G
";
            Assert(MermaidFlowchartParser.TryParse(diagram, out var graph, out string? error), $"Parse failed: {error}");
            AssertEqual(6, graph!.Edges.Count, "Edge count");

            // 1. Solid Arrow (-->)
            var e0 = graph.Edges[0];
            AssertEqual("A", e0.SourceId, "Edge 0 Source");
            AssertEqual("B", e0.TargetId, "Edge 0 Target");
            AssertEqual(MermaidStrokeStyle.Solid, e0.Stroke, "Edge 0 Stroke");
            AssertEqual(MermaidArrowHead.Arrow, e0.Arrow, "Edge 0 Arrow");
            AssertEqual(MermaidArrowHead.Arrow, e0.ArrowHead, "Edge 0 ArrowHead property");
            AssertEqual("A --> B", e0.ToString(), "Edge 0 ToString");

            // 2. Solid Link (---)
            var e1 = graph.Edges[1];
            AssertEqual("B", e1.SourceId, "Edge 1 Source");
            AssertEqual("C", e1.TargetId, "Edge 1 Target");
            AssertEqual(MermaidStrokeStyle.Solid, e1.Stroke, "Edge 1 Stroke");
            AssertEqual(MermaidArrowHead.None, e1.Arrow, "Edge 1 Arrow");
            AssertEqual("B --- C", e1.ToString(), "Edge 1 ToString");

            // 3. Dotted Arrow (-.->)
            var e2 = graph.Edges[2];
            AssertEqual("C", e2.SourceId, "Edge 2 Source");
            AssertEqual("D", e2.TargetId, "Edge 2 Target");
            AssertEqual(MermaidStrokeStyle.Dotted, e2.Stroke, "Edge 2 Stroke");
            AssertEqual(MermaidArrowHead.Arrow, e2.Arrow, "Edge 2 Arrow");
            AssertEqual("C -.-> D", e2.ToString(), "Edge 2 ToString");

            // 4. Dotted Link (-.-)
            var e3 = graph.Edges[3];
            AssertEqual("D", e3.SourceId, "Edge 3 Source");
            AssertEqual("E", e3.TargetId, "Edge 3 Target");
            AssertEqual(MermaidStrokeStyle.Dotted, e3.Stroke, "Edge 3 Stroke");
            AssertEqual(MermaidArrowHead.None, e3.Arrow, "Edge 3 Arrow");
            AssertEqual("D -.- E", e3.ToString(), "Edge 3 ToString");

            // 5. Thick Arrow (==>)
            var e4 = graph.Edges[4];
            AssertEqual("E", e4.SourceId, "Edge 4 Source");
            AssertEqual("F", e4.TargetId, "Edge 4 Target");
            AssertEqual(MermaidStrokeStyle.Thick, e4.Stroke, "Edge 4 Stroke");
            AssertEqual(MermaidArrowHead.Arrow, e4.Arrow, "Edge 4 Arrow");
            AssertEqual("E ==> F", e4.ToString(), "Edge 4 ToString");

            // 6. Thick Link (===)
            var e5 = graph.Edges[5];
            AssertEqual("F", e5.SourceId, "Edge 5 Source");
            AssertEqual("G", e5.TargetId, "Edge 5 Target");
            AssertEqual(MermaidStrokeStyle.Thick, e5.Stroke, "Edge 5 Stroke");
            AssertEqual(MermaidArrowHead.None, e5.Arrow, "Edge 5 Arrow");
            AssertEqual("F === G", e5.ToString(), "Edge 5 ToString");
        }

        /// <summary>
        /// Validates edge labels across pipe format (|label|) and inline format (-- label -->).
        /// </summary>
        public static void TestEdgeLabelsPipeAndInline()
        {
            string diagram = @"graph LR
    A -->|Pipe Label 1| B
    B -.->|Pipe Label 2| C
    C ==>|Pipe Label 3| D
    D ---|Pipe Link| E
    E -.-|Pipe Dotted Link| F
    F ===|Pipe Thick Link| G
    H -- Inline Solid --> I
    I -- Inline Link --- J
    J -. Inline Dotted .-> K
    K == Inline Thick ==> L
    M -->|ROIC > 18%| N
";
            Assert(MermaidFlowchartParser.TryParse(diagram, out var graph, out string? error), $"Parse failed: {error}");
            AssertEqual(11, graph!.Edges.Count, "Edge count");

            // Pipe labels
            AssertEqual("Pipe Label 1", graph.Edges[0].Label, "Edge 0 pipe label");
            AssertEqual(MermaidStrokeStyle.Solid, graph.Edges[0].Stroke, "Edge 0 stroke");
            AssertEqual(MermaidArrowHead.Arrow, graph.Edges[0].Arrow, "Edge 0 arrow");

            AssertEqual("Pipe Label 2", graph.Edges[1].Label, "Edge 1 pipe label");
            AssertEqual(MermaidStrokeStyle.Dotted, graph.Edges[1].Stroke, "Edge 1 stroke");

            AssertEqual("Pipe Label 3", graph.Edges[2].Label, "Edge 2 pipe label");
            AssertEqual(MermaidStrokeStyle.Thick, graph.Edges[2].Stroke, "Edge 2 stroke");

            AssertEqual("Pipe Link", graph.Edges[3].Label, "Edge 3 pipe label");
            AssertEqual(MermaidArrowHead.None, graph.Edges[3].Arrow, "Edge 3 arrow");

            AssertEqual("Pipe Dotted Link", graph.Edges[4].Label, "Edge 4 pipe label");
            AssertEqual(MermaidArrowHead.None, graph.Edges[4].Arrow, "Edge 4 arrow");

            AssertEqual("Pipe Thick Link", graph.Edges[5].Label, "Edge 5 pipe label");
            AssertEqual(MermaidArrowHead.None, graph.Edges[5].Arrow, "Edge 5 arrow");

            // Inline labels
            AssertEqual("Inline Solid", graph.Edges[6].Label, "Edge 6 inline label");
            AssertEqual(MermaidStrokeStyle.Solid, graph.Edges[6].Stroke, "Edge 6 stroke");
            AssertEqual(MermaidArrowHead.Arrow, graph.Edges[6].Arrow, "Edge 6 arrow");

            AssertEqual("Inline Link", graph.Edges[7].Label, "Edge 7 inline label");
            AssertEqual(MermaidStrokeStyle.Solid, graph.Edges[7].Stroke, "Edge 7 stroke");
            AssertEqual(MermaidArrowHead.None, graph.Edges[7].Arrow, "Edge 7 arrow");

            AssertEqual("Inline Dotted", graph.Edges[8].Label, "Edge 8 inline label");
            AssertEqual(MermaidStrokeStyle.Dotted, graph.Edges[8].Stroke, "Edge 8 stroke");
            AssertEqual(MermaidArrowHead.Arrow, graph.Edges[8].Arrow, "Edge 8 arrow");

            AssertEqual("Inline Thick", graph.Edges[9].Label, "Edge 9 inline label");
            AssertEqual(MermaidStrokeStyle.Thick, graph.Edges[9].Stroke, "Edge 9 stroke");
            AssertEqual(MermaidArrowHead.Arrow, graph.Edges[9].Arrow, "Edge 9 arrow");

            // Formula label with mathematical symbols
            AssertEqual("ROIC > 18%", graph.Edges[10].Label, "Edge 10 formula label");
        }

        /// <summary>
        /// Validates multi-node connection chains (e.g. A --> B --> C --> D).
        /// </summary>
        public static void TestMultiNodeChaining()
        {
            string diagram = @"graph TD
    A --> B --> C
    D[Start] -->|Step 1| E(Process) ==>|Step 2| F{Decision} -.->|Finish| G([Done]) --- H((End))
";
            Assert(MermaidFlowchartParser.TryParse(diagram, out var graph, out string? error), $"Parse failed: {error}");

            // First chain: A --> B --> C creates 2 edges
            AssertEqual("A", graph!.Edges[0].SourceId, "Chain 1 Edge 0 Source");
            AssertEqual("B", graph.Edges[0].TargetId, "Chain 1 Edge 0 Target");
            AssertEqual("B", graph.Edges[1].SourceId, "Chain 1 Edge 1 Source");
            AssertEqual("C", graph.Edges[1].TargetId, "Chain 1 Edge 1 Target");

            // Second chain: 5 nodes -> 4 edges
            AssertEqual("D", graph.Edges[2].SourceId, "Chain 2 Edge 0 Source");
            AssertEqual("E", graph.Edges[2].TargetId, "Chain 2 Edge 0 Target");
            AssertEqual("Step 1", graph.Edges[2].Label, "Chain 2 Edge 0 Label");

            AssertEqual("E", graph.Edges[3].SourceId, "Chain 2 Edge 1 Source");
            AssertEqual("F", graph.Edges[3].TargetId, "Chain 2 Edge 1 Target");
            AssertEqual("Step 2", graph.Edges[3].Label, "Chain 2 Edge 1 Label");
            AssertEqual(MermaidStrokeStyle.Thick, graph.Edges[3].Stroke, "Chain 2 Edge 1 Stroke");

            AssertEqual("F", graph.Edges[4].SourceId, "Chain 2 Edge 2 Source");
            AssertEqual("G", graph.Edges[4].TargetId, "Chain 2 Edge 2 Target");
            AssertEqual("Finish", graph.Edges[4].Label, "Chain 2 Edge 2 Label");
            AssertEqual(MermaidStrokeStyle.Dotted, graph.Edges[4].Stroke, "Chain 2 Edge 2 Stroke");

            AssertEqual("G", graph.Edges[5].SourceId, "Chain 2 Edge 3 Source");
            AssertEqual("H", graph.Edges[5].TargetId, "Chain 2 Edge 3 Target");
            AssertEqual(MermaidStrokeStyle.Solid, graph.Edges[5].Stroke, "Chain 2 Edge 3 Stroke");
            AssertEqual(MermaidArrowHead.None, graph.Edges[5].Arrow, "Chain 2 Edge 3 Arrow");

            // Verify node shapes in chain
            AssertEqual(MermaidNodeShape.Rectangle, graph.Nodes["D"].Shape, "Node D Shape");
            AssertEqual(MermaidNodeShape.RoundedRectangle, graph.Nodes["E"].Shape, "Node E Shape");
            AssertEqual(MermaidNodeShape.Diamond, graph.Nodes["F"].Shape, "Node F Shape");
            AssertEqual(MermaidNodeShape.Stadium, graph.Nodes["G"].Shape, "Node G Shape");
            AssertEqual(MermaidNodeShape.Circle, graph.Nodes["H"].Shape, "Node H Shape");
        }

        /// <summary>
        /// Validates logical subgraph declaration, bracketed titles, and node membership.
        /// </summary>
        public static void TestSubgraphsAndGrouping()
        {
            string diagram = @"graph TD
    subgraph Stage1 [Initial Phase]
        A[Start] --> B[Process]
    end
    subgraph Stage2 [Final Phase]
        C[Review] --> D[Deploy]
    end
    B --> C
";
            Assert(MermaidFlowchartParser.TryParse(diagram, out var graph, out string? error), $"Parse failed: {error}");
            AssertEqual(2, graph!.Subgraphs.Count, "Subgraph count");

            var sub1 = graph.Subgraphs[0];
            AssertEqual("Stage1", sub1.Id, "Subgraph 1 ID");
            AssertEqual("Initial Phase", sub1.Title, "Subgraph 1 Title");
            Assert(sub1.NodeIds.Contains("A"), "Subgraph 1 contains A");
            Assert(sub1.NodeIds.Contains("B"), "Subgraph 1 contains B");

            var sub2 = graph.Subgraphs[1];
            AssertEqual("Stage2", sub2.Id, "Subgraph 2 ID");
            AssertEqual("Final Phase", sub2.Title, "Subgraph 2 Title");
            Assert(sub2.NodeIds.Contains("C"), "Subgraph 2 contains C");
            Assert(sub2.NodeIds.Contains("D"), "Subgraph 2 contains D");

            AssertEqual(3, graph.Edges.Count, "Total edges count");
        }

        /// <summary>
        /// Validates that comments and diagram styling directives are cleanly ignored without error.
        /// </summary>
        public static void TestCommentsAndDirectivesIgnored()
        {
            string diagram = @"%% Global diagram comment
graph TD
    %% Header comment
    A[Start] --> B[Finish] %% Inline comment
    classDef default fill:#f9f,stroke:#333,stroke-width:4px;
    class A default;
    style B fill:#bbf,stroke:#f66,stroke-width:2px;
    click A ""https://github.com""
    linkStyle 0 stroke:#ff3,stroke-width:4px;
";
            Assert(MermaidFlowchartParser.TryParse(diagram, out var graph, out string? error), $"Parse failed: {error}");
            AssertEqual(2, graph!.Nodes.Count, "Node count");
            AssertEqual(1, graph.Edges.Count, "Edge count");
            AssertEqual("A", graph.Edges[0].SourceId, "Edge source");
            AssertEqual("B", graph.Edges[0].TargetId, "Edge target");
        }

        /// <summary>
        /// Validates graceful degradation and error handling:
        /// - Null/empty inputs
        /// - Unsupported diagram types (sequenceDiagram, classDiagram, etc.)
        /// - Malformed brackets, dangling arrows, unclosed quotes
        /// - Ensures zero unhandled exceptions.
        /// </summary>
        public static void TestGracefulFallbackAndErrorSafeguard()
        {
            // 1. Null / empty / whitespace
            Assert(!MermaidFlowchartParser.TryParse(null, out var gNull, out var err1), "Null input should return false");
            Assert(gNull == null, "Graph should be null on null input");
            Assert(!string.IsNullOrEmpty(err1), "Null input error message");

            Assert(!MermaidFlowchartParser.TryParse("", out var gEmpty, out var err2), "Empty input should return false");
            Assert(gEmpty == null, "Graph should be null on empty input");
            Assert(!string.IsNullOrEmpty(err2), "Empty input error message");

            Assert(!MermaidFlowchartParser.TryParse("   \n\t  ", out var gWs, out var err3), "Whitespace input should return false");
            Assert(gWs == null, "Graph should be null on whitespace input");
            Assert(!string.IsNullOrEmpty(err3), "Whitespace input error message");

            // 2. Unsupported diagram types
            string[] unsupported = new[]
            {
                "sequenceDiagram\nAlice->Bob: Hi",
                "classDiagram\nClass01 <|-- AveryLongClass",
                "stateDiagram-v2\n[*] --> Still",
                "erDiagram\nCUSTOMER ||--o{ ORDER : places",
                "gantt\ntitle A Gantt Diagram",
                "pie title Pets\n\"Dogs\" : 386",
                "gitGraph\ncommit",
                "mindmap\nroot((mindmap))"
            };

            foreach (var code in unsupported)
            {
                bool success = MermaidFlowchartParser.TryParse(code, out var g, out string? err);
                Assert(!success, $"Unsupported diagram '{code.Split('\n')[0]}' should return false");
                Assert(g == null, "Graph should be null on unsupported diagram");
                Assert(!string.IsNullOrEmpty(err) && err.Contains("Unsupported diagram type"), $"Error should indicate unsupported diagram: {err}");
            }

            // 3. Missing or unrecognized orientation
            Assert(!MermaidFlowchartParser.TryParse("graph", out var gDir1, out var errDir1), "Missing orientation directive");
            Assert(gDir1 == null, "Graph should be null on missing orientation");
            Assert(!string.IsNullOrEmpty(errDir1), "Missing orientation error message");

            Assert(!MermaidFlowchartParser.TryParse("graph UNKNOWN", out var gDir2, out var errDir2), "Invalid orientation directive");
            Assert(gDir2 == null, "Graph should be null on invalid orientation");
            Assert(!string.IsNullOrEmpty(errDir2), "Invalid orientation error message");

            // 4. Syntax errors: dangling arrows
            Assert(!MermaidFlowchartParser.TryParse("graph TD\nA -->", out var gArr1, out var errArrow1), "Dangling arrow should fail");
            Assert(gArr1 == null, "Graph should be null on dangling arrow");
            Assert(!string.IsNullOrEmpty(errArrow1), "Dangling arrow error message");

            // 5. Unclosed node brackets
            Assert(!MermaidFlowchartParser.TryParse("graph TD\nA[Unfinished", out var gBrk1, out var errBracket1), "Unclosed rectangle should fail");
            Assert(gBrk1 == null, "Graph should be null on unclosed rectangle");
            Assert(!string.IsNullOrEmpty(errBracket1), "Unclosed rectangle error message");

            Assert(!MermaidFlowchartParser.TryParse("graph TD\nA([Unfinished", out var gBrk2, out var errBracket2), "Unclosed stadium should fail");
            Assert(gBrk2 == null, "Graph should be null on unclosed stadium");
            Assert(!string.IsNullOrEmpty(errBracket2), "Unclosed stadium error message");

            Assert(!MermaidFlowchartParser.TryParse("graph TD\nA((Unfinished", out var gBrk3, out var errBracket3), "Unclosed circle should fail");
            Assert(gBrk3 == null, "Graph should be null on unclosed circle");
            Assert(!string.IsNullOrEmpty(errBracket3), "Unclosed circle error message");

            Assert(!MermaidFlowchartParser.TryParse("graph TD\nA{Unfinished", out var gBrk4, out var errBracket4), "Unclosed diamond should fail");
            Assert(gBrk4 == null, "Graph should be null on unclosed diamond");
            Assert(!string.IsNullOrEmpty(errBracket4), "Unclosed diamond error message");

            Assert(!MermaidFlowchartParser.TryParse("graph TD\nA(Unfinished", out var gBrk5, out var errBracket5), "Unclosed rounded should fail");
            Assert(gBrk5 == null, "Graph should be null on unclosed rounded");
            Assert(!string.IsNullOrEmpty(errBracket5), "Unclosed rounded error message");

            // 6. Unclosed quotes inside shapes
            Assert(!MermaidFlowchartParser.TryParse("graph TD\nA[\"Unclosed quote]", out var gQuote, out var errQuote), "Unclosed quote should fail");
            Assert(gQuote == null, "Graph should be null on unclosed quote");
            Assert(!string.IsNullOrEmpty(errQuote), "Unclosed quote error message");
        }

        /// <summary>
        /// Validates parsing speed for a 20-node flowchart with 25 edges and mixed shapes/labels.
        /// Target performance: < 5.0 ms per parse.
        /// </summary>
        public static void TestPerformanceBenchmark20Nodes()
        {
            string diagram = @"graph TD
    N01([Start Process]) -->|Init| N02[Load Configuration]
    N02 --> N03{Is Config Valid?}
    N03 -->|Yes| N04(Initialize Cache)
    N03 -->|No| N05([Halt: Invalid Config])
    N04 ==> N06[Establish Database Connection]
    N06 --> N07{Connection OK?}
    N07 -->|Retry| N06
    N07 -->|Success| N08((Worker Pool))
    N08 -.->|Spawn| N09[Worker Thread 1]
    N08 -.->|Spawn| N10[Worker Thread 2]
    N08 -.->|Spawn| N11[Worker Thread 3]
    N09 --> N12{Data Available?}
    N10 --> N12
    N11 --> N12
    N12 -->|Yes| N13[Fetch Batch Data]
    N12 -->|No| N14(Sleep 100ms)
    N14 --> N12
    N13 ==> N15[Transform Records]
    N15 --> N16{Validation Passed?}
    N16 -->|Pass| N17[Commit to Database]
    N16 -->|Fail| N18[Log Error to Sentry]
    N17 --> N19[Send Notification]
    N18 --> N19
    N19 --> N20([End of Execution])
";

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                MermaidFlowchartParser.TryParse(diagram, out _, out _);
            }

            int iterations = 100;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                bool ok = MermaidFlowchartParser.TryParse(diagram, out var g, out string? err);
                if (!ok || g == null)
                {
                    throw new Exception($"Benchmark parse failed at iteration {i}: {err}");
                }
            }
            sw.Stop();

            double totalMs = sw.Elapsed.TotalMilliseconds;
            double avgMs = totalMs / iterations;

            Console.Write($" [{iterations} iterations, avg {avgMs:F3} ms/parse] ");

            // Verify parse result correctness
            MermaidFlowchartParser.TryParse(diagram, out var benchmarkGraph, out _);
            AssertEqual(20, benchmarkGraph!.Nodes.Count, "Benchmark node count");
            Assert(benchmarkGraph.Edges.Count >= 20, "Benchmark edge count >= 20");

            // Ensure parse overhead is strictly under 5.0 ms threshold
            Assert(avgMs < 5.0, $"Parse speed ({avgMs:F3} ms) exceeded 5.0 ms target threshold.");
        }

        /// <summary>
        /// Empirical Challenger M2-1 Adversarial Test Suite:
        /// Stress-tests Mermaid flowchart parser against complex brackets, unicode, emoji, math,
        /// cycles, chained arrows, disconnected nodes, whitespace variations, and clean fallback.
        /// </summary>
        public static void TestEmpiricalChallengerM2_1AdversarialSuite()
        {
            // =========================================================================
            // 1. Complex Nested Brackets, Disambiguation & Unclosed Bracket Fallbacks
            // =========================================================================
            string complexBracketsDiagram = @"graph TD
    A[""[Bracket 1] and (Paren) and {Brace}""]
    B([""Stadium with [brackets] and (parens)""])
    C((""Circle with \""escaped quotes\"" and [brackets]""))
    D{""Diamond with {nested braces} and [brackets]""}
    E(""Rounded with [brackets]"")
";
            Assert(MermaidFlowchartParser.TryParse(complexBracketsDiagram, out var cbGraph, out var cbErr), $"Complex brackets parse failed: {cbErr}");
            AssertEqual(5, cbGraph!.Nodes.Count, "Complex brackets node count");
            AssertEqual("[Bracket 1] and (Paren) and {Brace}", cbGraph.Nodes["A"].Text, "Node A nested brackets in quotes");
            AssertEqual(MermaidNodeShape.Rectangle, cbGraph.Nodes["A"].Shape, "Node A shape");
            AssertEqual("Stadium with [brackets] and (parens)", cbGraph.Nodes["B"].Text, "Node B stadium nested in quotes");
            AssertEqual(MermaidNodeShape.Stadium, cbGraph.Nodes["B"].Shape, "Node B shape");
            AssertEqual("Circle with \"escaped quotes\" and [brackets]", cbGraph.Nodes["C"].Text, "Node C circle escaped quotes");
            AssertEqual(MermaidNodeShape.Circle, cbGraph.Nodes["C"].Shape, "Node C shape");
            AssertEqual("Diamond with {nested braces} and [brackets]", cbGraph.Nodes["D"].Text, "Node D diamond braces");
            AssertEqual(MermaidNodeShape.Diamond, cbGraph.Nodes["D"].Shape, "Node D shape");
            AssertEqual("Rounded with [brackets]", cbGraph.Nodes["E"].Text, "Node E rounded brackets");
            AssertEqual(MermaidNodeShape.RoundedRectangle, cbGraph.Nodes["E"].Shape, "Node E shape");

            // Lookahead disambiguation: ([ vs (, (( vs (, [ vs bare
            string disambigDiagram = @"graph LR
    S([StadiumCapsule])
    R(RoundedAction)
    C((CircleState))
    K[RectangleBlock]
    D{DecisionNode}
";
            Assert(MermaidFlowchartParser.TryParse(disambigDiagram, out var disGraph, out _), "Disambiguation diagram parse failed");
            AssertEqual(MermaidNodeShape.Stadium, disGraph!.Nodes["S"].Shape, "S is Stadium");
            AssertEqual(MermaidNodeShape.RoundedRectangle, disGraph.Nodes["R"].Shape, "R is RoundedRectangle");
            AssertEqual(MermaidNodeShape.Circle, disGraph.Nodes["C"].Shape, "C is Circle");
            AssertEqual(MermaidNodeShape.Rectangle, disGraph.Nodes["K"].Shape, "K is Rectangle");
            AssertEqual(MermaidNodeShape.Diamond, disGraph.Nodes["D"].Shape, "D is Diamond");

            // Adversarial Unclosed Brackets -> Must return false cleanly without exceptions
            string[] unclosedSamples = new[]
            {
                "graph TD\nA[",
                "graph TD\nA([",
                "graph TD\nA((",
                "graph TD\nA{",
                "graph TD\nA(",
                "graph TD\nA[Unclosed text",
                "graph TD\nA([Unclosed stadium",
                "graph TD\nA((Unclosed circle",
                "graph TD\nA{Unclosed diamond",
                "graph TD\nA(Unclosed rounded",
                "graph TD\nA[\"Unclosed quote in bracket]",
                "graph TD\nA[\"Unclosed quote no bracket",
                "graph TD\nA[\"Closed quote\" no bracket"
            };

            int contractViolationCount = 0;
            foreach (var unclosed in unclosedSamples)
            {
                bool parsed = MermaidFlowchartParser.TryParse(unclosed, out var badG, out var badErr);
                Assert(!parsed, $"Unclosed sample '{unclosed.Replace('\n', ' ')}' should fail");
                Assert(!string.IsNullOrEmpty(badErr), "Error message must be present on unclosed bracket");
                Assert(badG == null, $"Out graph must be null when TryParse returns false for '{unclosed.Replace('\n', ' ')}'");
                if (badG != null)
                {
                    contractViolationCount++;
                    Console.WriteLine($"  [BUG FOUND] Contract Violation: TryParse returned false but out graph is non-null for input: '{unclosed.Replace('\n', ' ')}'");
                }
            }

            // Empirical Check: Unclosed bracket followed by another line with a closing bracket
            string unclosedSwallowingInput = "graph TD\nA[Unclosed\nB --> C[Closed]";
            bool unclosedSwallowed = MermaidFlowchartParser.TryParse(unclosedSwallowingInput, out var swallowedGraph, out var swallowedErr);
            Assert(!unclosedSwallowed, "Unclosed bracket swallowing input must return false");
            Assert(swallowedGraph == null, "Out graph must be null on unclosed bracket failure");
            Assert(!string.IsNullOrEmpty(swallowedErr), "Error message must be present on unclosed bracket failure");

            // Empirical Check: Stray closing bracket parsed as node ID
            string strayBracketInput = "graph TD\nA[Process]]\nB --> C";
            bool strayParsed = MermaidFlowchartParser.TryParse(strayBracketInput, out var strayGraph, out var strayErr);
            Assert(!strayParsed, "Stray closing bracket input 'A[Process]]' must return false");
            Assert(strayGraph == null, "Out graph must be null on stray closing bracket failure");
            Assert(!string.IsNullOrEmpty(strayErr), "Error message must be present on stray closing bracket failure");



            // =========================================================================
            // 2. Unusual Unicode Labels, Emoji, Math Formulas & Special Characters
            // =========================================================================
            string unicodeDiagram = @"graph TD
    🚀[Mission Launch 🌟] --> 🌕[Lunar Orbit 🛰️] ==> 🛸[Landing Site]
    M1[""Math: $ROIC > 18\%$ & $\ge +1.5\%$ & $\le 30\times$""] --> M2[""Formula: $\sum_{i=1}^n x_i \approx \int f(x) dx$""]
    CJK[""日本語 / 中文 / 한국어""] --> RTL[""مرحبًا بالعالم""] --> EUR[""München, Genève, Malmö, Kraków""]
    节点_Alpha[""Alpha Node""] --> 节点_Beta[""Beta Node""]
    ""Quoted ID 1""[""Label 1""] --> ""Quoted ID 2""[""Label 2""]
";
            Assert(MermaidFlowchartParser.TryParse(unicodeDiagram, out var uniGraph, out var uniErr), $"Unicode diagram parse failed: {uniErr}");
            Assert(uniGraph!.Nodes.ContainsKey("🚀"), "Emoji node ID 🚀 present");
            AssertEqual("Mission Launch 🌟", uniGraph.Nodes["🚀"].Text, "Emoji node label");
            AssertEqual("Lunar Orbit 🛰️", uniGraph.Nodes["🌕"].Text, "Lunar node label");
            AssertEqual("Landing Site", uniGraph.Nodes["🛸"].Text, "Landing node label");

            AssertEqual("Math: $ROIC > 18\\%$ & $\\ge +1.5\\%$ & $\\le 30\\times$", uniGraph.Nodes["M1"].Text, "Math formula in M1");
            AssertEqual("Formula: $\\sum_{i=1}^n x_i \\approx \\int f(x) dx$", uniGraph.Nodes["M2"].Text, "Math formula in M2");

            AssertEqual("日本語 / 中文 / 한국어", uniGraph.Nodes["CJK"].Text, "CJK label");
            AssertEqual("مرحبًا بالعالم", uniGraph.Nodes["RTL"].Text, "RTL Arabic label");
            AssertEqual("München, Genève, Malmö, Kraków", uniGraph.Nodes["EUR"].Text, "European accented label");

            Assert(uniGraph.Nodes.ContainsKey("节点_Alpha"), "Unicode identifier 节点_Alpha present");
            Assert(uniGraph.Nodes.ContainsKey("节点_Beta"), "Unicode identifier 节点_Beta present");
            AssertEqual("Alpha Node", uniGraph.Nodes["节点_Alpha"].Text, "Unicode node text");

            Assert(uniGraph.Nodes.ContainsKey("Quoted ID 1"), "Quoted ID 1 present");
            Assert(uniGraph.Nodes.ContainsKey("Quoted ID 2"), "Quoted ID 2 present");

            // =========================================================================
            // 3. Deeply Chained Arrows, Cycles & Disconnected Nodes
            // =========================================================================
            // Deep chain with all arrow styles and pipe labels
            string deepChainDiagram = @"graph TD
    A --> B ==>|thick label| C -.->|dotted label| D --- E -.- F === G
";
            Assert(MermaidFlowchartParser.TryParse(deepChainDiagram, out var chainGraph, out var chainErr), $"Deep chain parse failed: {chainErr}");
            AssertEqual(7, chainGraph!.Nodes.Count, "Deep chain node count");
            AssertEqual(6, chainGraph.Edges.Count, "Deep chain edge count");

            // A --> B
            AssertEqual("A", chainGraph.Edges[0].SourceId, "Edge 0 Source");
            AssertEqual("B", chainGraph.Edges[0].TargetId, "Edge 0 Target");
            AssertEqual(MermaidStrokeStyle.Solid, chainGraph.Edges[0].Stroke, "Edge 0 Stroke");
            AssertEqual(MermaidArrowHead.Arrow, chainGraph.Edges[0].Arrow, "Edge 0 Arrow");
            AssertEqual(null, chainGraph.Edges[0].Label, "Edge 0 Label");

            // B ==>|thick label| C
            AssertEqual("B", chainGraph.Edges[1].SourceId, "Edge 1 Source");
            AssertEqual("C", chainGraph.Edges[1].TargetId, "Edge 1 Target");
            AssertEqual(MermaidStrokeStyle.Thick, chainGraph.Edges[1].Stroke, "Edge 1 Stroke");
            AssertEqual(MermaidArrowHead.Arrow, chainGraph.Edges[1].Arrow, "Edge 1 Arrow");
            AssertEqual("thick label", chainGraph.Edges[1].Label, "Edge 1 Label");

            // C -.->|dotted label| D
            AssertEqual("C", chainGraph.Edges[2].SourceId, "Edge 2 Source");
            AssertEqual("D", chainGraph.Edges[2].TargetId, "Edge 2 Target");
            AssertEqual(MermaidStrokeStyle.Dotted, chainGraph.Edges[2].Stroke, "Edge 2 Stroke");
            AssertEqual(MermaidArrowHead.Arrow, chainGraph.Edges[2].Arrow, "Edge 2 Arrow");
            AssertEqual("dotted label", chainGraph.Edges[2].Label, "Edge 2 Label");

            // D --- E
            AssertEqual("D", chainGraph.Edges[3].SourceId, "Edge 3 Source");
            AssertEqual("E", chainGraph.Edges[3].TargetId, "Edge 3 Target");
            AssertEqual(MermaidStrokeStyle.Solid, chainGraph.Edges[3].Stroke, "Edge 3 Stroke");
            AssertEqual(MermaidArrowHead.None, chainGraph.Edges[3].Arrow, "Edge 3 Arrow");

            // E -.- F
            AssertEqual("E", chainGraph.Edges[4].SourceId, "Edge 4 Source");
            AssertEqual("F", chainGraph.Edges[4].TargetId, "Edge 4 Target");
            AssertEqual(MermaidStrokeStyle.Dotted, chainGraph.Edges[4].Stroke, "Edge 4 Stroke");
            AssertEqual(MermaidArrowHead.None, chainGraph.Edges[4].Arrow, "Edge 4 Arrow");

            // F === G
            AssertEqual("F", chainGraph.Edges[5].SourceId, "Edge 5 Source");
            AssertEqual("G", chainGraph.Edges[5].TargetId, "Edge 5 Target");
            AssertEqual(MermaidStrokeStyle.Thick, chainGraph.Edges[5].Stroke, "Edge 5 Stroke");
            AssertEqual(MermaidArrowHead.None, chainGraph.Edges[5].Arrow, "Edge 5 Arrow");

            // Graph Cycles (triangle cycle, 2-cycle, self-loop, interlocking cycles)
            string cyclesDiagram = @"graph TD
    A --> B --> C --> A
    X --> Y
    Y --> X
    Z --> Z
    P --> Q --> R --> P
    Q --> S --> Q
";
            Assert(MermaidFlowchartParser.TryParse(cyclesDiagram, out var cycleGraph, out var cycleErr), $"Cycles parse failed: {cycleErr}");
            AssertEqual(10, cycleGraph!.Nodes.Count, "Cycle graph distinct node count (A, B, C, X, Y, Z, P, Q, R, S)");
            AssertEqual(11, cycleGraph.Edges.Count, "Cycle graph total edges");


            // Disconnected Nodes alongside Connected Edges
            string disconnectedDiagram = @"graph TD
    SoloA[Standalone Alpha]
    SoloB([Standalone Stadium])
    Connected1 --> Connected2
    SoloC{Standalone Diamond}
    SoloD
    SoloE((Standalone Circle))
";
            Assert(MermaidFlowchartParser.TryParse(disconnectedDiagram, out var discGraph, out var discErr), $"Disconnected parse failed: {discErr}");
            AssertEqual(7, discGraph!.Nodes.Count, "Disconnected diagram node count");
            AssertEqual(1, discGraph.Edges.Count, "Disconnected diagram edge count");
            AssertEqual(MermaidNodeShape.Rectangle, discGraph.Nodes["SoloA"].Shape, "SoloA shape");
            AssertEqual(MermaidNodeShape.Stadium, discGraph.Nodes["SoloB"].Shape, "SoloB shape");
            AssertEqual(MermaidNodeShape.Diamond, discGraph.Nodes["SoloC"].Shape, "SoloC shape");
            AssertEqual(MermaidNodeShape.Rectangle, discGraph.Nodes["SoloD"].Shape, "SoloD shape");
            AssertEqual(MermaidNodeShape.Circle, discGraph.Nodes["SoloE"].Shape, "SoloE shape");

            // =========================================================================
            // 4. Mixed Graph Keywords, Orientations & Whitespace Variations
            // =========================================================================
            string[] headerVariations = new[]
            {
                "   GRAPH   td   ;   \n A --> B",
                "flowchart   LR  ;\n A --> B",
                "graph RL;\n A --> B",
                "flowchart BT\n A --> B",
                "   graph   TB   ;\n A --> B",
                "FLOWCHART lr\n A --> B",
                "graph BT;\n A --> B"
            };

            foreach (var hCode in headerVariations)
            {
                Assert(MermaidFlowchartParser.TryParse(hCode, out var hGraph, out var hErr), $"Header variation failed: '{hCode}': {hErr}");
                AssertEqual(2, hGraph!.Nodes.Count, "Header variation node count");
                AssertEqual(1, hGraph.Edges.Count, "Header variation edge count");
            }

            // Semicolons and multiple statements per line
            string semicolonMultiLine = @"graph LR; A-->B; B-->C; C-->D";
            Assert(MermaidFlowchartParser.TryParse(semicolonMultiLine, out var semiGraph, out var semiErr), $"Semicolon multi-statement failed: {semiErr}");
            AssertEqual(4, semiGraph!.Nodes.Count, "Semicolon multi-statement node count");
            AssertEqual(3, semiGraph.Edges.Count, "Semicolon multi-statement edge count");

            // Multiple consecutive semicolons and blank lines
            string multiSemi = @"graph TD;;;
    A --> B;;;
    B --> C;;;
";
            Assert(MermaidFlowchartParser.TryParse(multiSemi, out var msGraph, out var msErr), $"Multiple semicolons failed: {msErr}");
            AssertEqual(3, msGraph!.Nodes.Count, "Multi-semicolon node count");
            AssertEqual(2, msGraph.Edges.Count, "Multi-semicolon edge count");

            // Extreme whitespace variations around tokens
            string extremeWhitespace = "graph TD\n  \t  A  [  Spaced Label  ]   -->   |  Spaced Pipe  |   B  (  Rounded Label  )  \n";
            Assert(MermaidFlowchartParser.TryParse(extremeWhitespace, out var wsGraph, out var wsErr), $"Whitespace variation failed: {wsErr}");
            AssertEqual("Spaced Label", wsGraph!.Nodes["A"].Text, "Node A text with stripped spaces");
            AssertEqual("Rounded Label", wsGraph.Nodes["B"].Text, "Node B text with stripped spaces");
            AssertEqual("Spaced Pipe", wsGraph.Edges[0].Label, "Edge pipe label with stripped spaces");

            // Comments everywhere
            string interspersedComments = @"%% Initial comment
%% Second comment
graph TD %% Header inline comment
    %% Pre-statement comment
    A[Start] --> B[End] %% Post-statement inline comment
    %% Footer comment
";
            Assert(MermaidFlowchartParser.TryParse(interspersedComments, out var comGraph, out var comErr), $"Comments failed: {comErr}");
            AssertEqual(2, comGraph!.Nodes.Count, "Comment diagram node count");
            AssertEqual(1, comGraph.Edges.Count, "Comment diagram edge count");

            // =========================================================================
            // 5. Non-Flowchart Mermaid Code (Clean Fallback Without Exceptions)
            // =========================================================================
            string[] unsupportedTypes = new[]
            {
                "sequenceDiagram\nAlice->>Bob: Hello",
                "classDiagram\nClass01 <|-- AveryLongClass",
                "stateDiagram\n[*] --> First",
                "stateDiagram-v2\n[*] --> Second",
                "erDiagram\nCUSTOMER ||--o{ ORDER : places",
                "journey\ntitle My working day",
                "gantt\ntitle A Gantt Diagram",
                "pie title Pets\n\"Dogs\": 386",
                "quadrantChart\ntitle Reach and engagement",
                "requirementDiagram\nrequirement test_req { id: 1 }",
                "gitGraph\ncommit",
                "c4Context\nPerson(personA, \"John\")",
                "mindmap\nroot((mindmap))",
                "timeline\ntitle History",
                "sankey-beta\nA,B,10",
                "xychart-beta\ntitle \"Sales\"",
                "block-beta\ncolumns 1",
                "packet-beta\n0-15: \"Source Port\"",
                "kanban\ntodo",
                "architecture-beta\ngroup api(logos:aws-lambda)[API]"
            };

            foreach (var uns in unsupportedTypes)
            {
                bool success = MermaidFlowchartParser.TryParse(uns, out var g, out string? err);
                Assert(!success, $"Diagram type '{uns.Split('\n')[0]}' must return false for graceful fallback");
                Assert(g == null, "Graph must be null on unsupported diagram");
                Assert(!string.IsNullOrEmpty(err) && err.Contains("Unsupported diagram type"), $"Expected unsupported diagram message, got: '{err}'");
            }

            // Malformed headers and dangling arrows
            string[] malformedSamples = new[]
            {
                "graph",
                "flowchart",
                "graph INVALID_ORIENTATION",
                "flowchart NOT_A_DIR",
                "randomSyntax TD\nA-->B",
                "---\ntitle: Frontmatter\n---\nflowchart TD",
                "graph TD\nA -->",
                "graph TD\nA ==>|thick|",
                "graph TD\nA -- inline -->",
                "graph TD\n--> B",
                "graph TD\n=== B",
                "graph TD\n-.-> B",
                "graph TD\nA --> B -->"
            };

            foreach (var malformed in malformedSamples)
            {
                bool parsed = MermaidFlowchartParser.TryParse(malformed, out var badG, out var badErr);
                Assert(!parsed, $"Malformed sample '{malformed.Replace('\n', ' ')}' must return false");
                Assert(!string.IsNullOrEmpty(badErr), "Error message must be present on malformed syntax");
                Assert(badG == null, $"Out graph must be null when TryParse returns false for '{malformed.Replace('\n', ' ')}'");
                if (badG != null)
                {
                    contractViolationCount++;
                    Console.WriteLine($"  [BUG FOUND] Contract Violation: TryParse returned false but out graph is non-null for input: '{malformed.Replace('\n', ' ')}'");
                }
            }

            AssertEqual(0, contractViolationCount, "Total TryParse out graph contract violations detected must be 0");


            // =========================================================================
            // 6. Scale & Stress Testing (100-Node Chain & Bipartite Graph)
            // =========================================================================
            var sb100 = new System.Text.StringBuilder();
            sb100.AppendLine("graph TD");
            for (int i = 0; i < 99; i++)
            {
                sb100.AppendLine($"    N{i:D3}[Node {i}] --> N{i + 1:D3}[Node {i + 1}]");
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            bool ok100 = MermaidFlowchartParser.TryParse(sb100.ToString(), out var g100, out var err100);
            sw.Stop();

            Assert(ok100, $"100-node chain failed to parse: {err100}");
            AssertEqual(100, g100!.Nodes.Count, "100-node chain node count");
            AssertEqual(99, g100.Edges.Count, "100-node chain edge count");
            Assert(sw.ElapsedMilliseconds < 50, $"100-node chain took too long: {sw.ElapsedMilliseconds} ms");

            // Dense Bipartite Graph K_5,5 (10 nodes, 25 edges)
            var sbBipartite = new System.Text.StringBuilder();
            sbBipartite.AppendLine("graph LR");
            for (int u = 0; u < 5; u++)
            {
                for (int v = 0; v < 5; v++)
                {
                    sbBipartite.AppendLine($"    U{u}(Left {u}) ==>|Link {u}-{v}| V{v}(Right {v})");
                }
            }

            bool okBip = MermaidFlowchartParser.TryParse(sbBipartite.ToString(), out var gBip, out var errBip);
            Assert(okBip, $"Bipartite graph failed: {errBip}");
            AssertEqual(10, gBip!.Nodes.Count, "Bipartite graph 10 nodes");
            AssertEqual(25, gBip.Edges.Count, "Bipartite graph 25 edges");
        }

        /// <summary>
        /// Empirical Challenger M2-2 Scale, Stress & Ambiguity Test Suite:
        /// 1. Scale & Stress: 50-node and 100-node flowcharts, verify parsing completes under 2ms with zero allocations blowup.
        /// 2. Ambiguous Keywords: nodes named like keywords (graph, subgraph, end, TD, LR, direction, style, classDef).
        /// 3. Arrow Lookaheads: disambiguation of --- vs --> vs -- label -->, dotted, thick, chained, and hyphenated IDs.
        /// </summary>
        public static void TestEmpiricalChallengerM2_2_ScaleStressAndAmbiguitySuite()
        {
            // =========================================================================
            // 1. Scale & Stress Testing: 50-Node Graph (< 2.0ms and Zero Allocation Blowup)
            // =========================================================================
            var sb50 = new System.Text.StringBuilder();
            sb50.AppendLine("graph TD");
            // Define 50 nodes with diverse shapes across 3 subgraphs
            sb50.AppendLine("    subgraph IngestionPhase [Ingestion Tier]");
            for (int i = 0; i < 15; i++)
            {
                if (i % 2 == 0)
                    sb50.AppendLine($"        N{i:D2}[Process {i}]");
                else
                    sb50.AppendLine($"        N{i:D2}(Action {i})");
            }
            sb50.AppendLine("    end");

            sb50.AppendLine("    subgraph ProcessingPhase [Processing Tier]");
            for (int i = 15; i < 35; i++)
            {
                if (i % 3 == 0)
                    sb50.AppendLine($"        N{i:D2}([Stadium {i}])");
                else if (i % 3 == 1)
                    sb50.AppendLine($"        N{i:D2}{{Decision {i}}}");
                else
                    sb50.AppendLine($"        N{i:D2}((Circle {i}))");
            }
            sb50.AppendLine("    end");

            sb50.AppendLine("    subgraph DeliveryPhase [Delivery Tier]");
            for (int i = 35; i < 50; i++)
            {
                sb50.AppendLine($"        N{i:D2}[Delivery {i}]");
            }
            sb50.AppendLine("    end");

            // Add 49 sequential chain edges with mixed styles and labels
            for (int i = 0; i < 49; i++)
            {
                int mod = i % 5;
                if (mod == 0)
                    sb50.AppendLine($"    N{i:D2} -->|Step {i}| N{i + 1:D2}");
                else if (mod == 1)
                    sb50.AppendLine($"    N{i:D2} -- Fast Track {i} --> N{i + 1:D2}");
                else if (mod == 2)
                    sb50.AppendLine($"    N{i:D2} ==>|Critical {i}| N{i + 1:D2}");
                else if (mod == 3)
                    sb50.AppendLine($"    N{i:D2} -.->|Async {i}| N{i + 1:D2}");
                else
                    sb50.AppendLine($"    N{i:D2} --- N{i + 1:D2}");
            }

            // Add 16 cross-layer edges (total 65 edges)
            for (int i = 0; i < 16; i++)
            {
                int target = (i * 3 + 7) % 50;
                sb50.AppendLine($"    N{i:D2} -.-> N{target:D2}");
            }

            string code50 = sb50.ToString();

            // Warmup
            for (int w = 0; w < 10; w++)
            {
                MermaidFlowchartParser.TryParse(code50, out _, out _);
            }

            // Benchmark 100 iterations
            int iters50 = 100;
            var sw50 = Stopwatch.StartNew();
            for (int i = 0; i < iters50; i++)
            {
                bool parsed = MermaidFlowchartParser.TryParse(code50, out var g50, out var err50);
                if (!parsed || g50 == null)
                {
                    throw new Exception($"50-node benchmark failed at iteration {i}: {err50}");
                }
            }
            sw50.Stop();

            double avgMs50 = sw50.Elapsed.TotalMilliseconds / iters50;
            Console.WriteLine($"\n[CHALLENGER-M2-2] 50-Node Graph: 100 iterations, avg {avgMs50:F3} ms/parse (Budget: < 2.0 ms)");

            // Measure memory allocation per parse
            long startAlloc50 = GC.GetAllocatedBytesForCurrentThread();
            int allocIters50 = 10;
            for (int i = 0; i < allocIters50; i++)
            {
                MermaidFlowchartParser.TryParse(code50, out _, out _);
            }
            long endAlloc50 = GC.GetAllocatedBytesForCurrentThread();
            long bytesPerParse50 = (endAlloc50 - startAlloc50) / allocIters50;
            Console.WriteLine($"[CHALLENGER-M2-2] 50-Node Graph Allocations: ~{bytesPerParse50 / 1024} KB/parse (Budget: < 150 KB)");

            // Integrity assertions
            MermaidFlowchartParser.TryParse(code50, out var finalG50, out _);
            AssertEqual(50, finalG50!.Nodes.Count, "50-node graph node count");
            AssertEqual(65, finalG50.Edges.Count, "50-node graph edge count");
            AssertEqual(3, finalG50.Subgraphs.Count, "50-node graph subgraph count");
            Assert(avgMs50 < 2.0, $"50-node parse speed ({avgMs50:F3} ms) exceeded 2.0 ms threshold!");
            Assert(bytesPerParse50 < 150_000, $"50-node allocation ({bytesPerParse50} bytes) exceeded 150 KB threshold!");

            // =========================================================================
            // 2. Scale & Stress Testing: 100-Node Graph (< 2.0ms and Zero Allocation Blowup)
            // =========================================================================
            var sb100 = new System.Text.StringBuilder();
            sb100.AppendLine("flowchart TD");
            // Define 100 nodes across 4 subgraphs with diverse shapes
            sb100.AppendLine("    subgraph S1 [Cluster Alpha]");
            for (int i = 0; i < 25; i++)
            {
                sb100.AppendLine($"        N{i:D3}[\"Node {i}<br/>Line 2\"]");
            }
            sb100.AppendLine("    end");

            sb100.AppendLine("    subgraph S2 [Cluster Beta]");
            for (int i = 25; i < 50; i++)
            {
                sb100.AppendLine($"        N{i:D3}(Node {i})");
            }
            sb100.AppendLine("    end");

            sb100.AppendLine("    subgraph S3 [Cluster Gamma]");
            for (int i = 50; i < 75; i++)
            {
                sb100.AppendLine($"        N{i:D3}{{{$"Decision {i}"}}}");
            }
            sb100.AppendLine("    end");

            sb100.AppendLine("    subgraph S4 [Cluster Delta]");
            for (int i = 75; i < 100; i++)
            {
                if (i % 2 == 0)
                    sb100.AppendLine($"        N{i:D3}(([Node {i}]))");
                else
                    sb100.AppendLine($"        N{i:D3}([Node {i}])");
            }
            sb100.AppendLine("    end");

            // 99 sequential edges
            for (int i = 0; i < 99; i++)
            {
                int mod = i % 4;
                if (mod == 0)
                    sb100.AppendLine($"    N{i:D3} --> N{i + 1:D3}");
                else if (mod == 1)
                    sb100.AppendLine($"    N{i:D3} ==>|Heavy Flow {i}| N{i + 1:D3}");
                else if (mod == 2)
                    sb100.AppendLine($"    N{i:D3} -.-> N{i + 1:D3}");
                else
                    sb100.AppendLine($"    N{i:D3} -- Link {i} --> N{i + 1:D3}");
            }

            // 45 cross-cluster and feedback cycle edges (total 144 edges)
            for (int i = 0; i < 40; i++)
            {
                int target = (i * 7 + 13) % 100;
                sb100.AppendLine($"    N{i:D3} --> N{target:D3}");
            }
            // Feedback cycles
            sb100.AppendLine("    N095 --> N010");
            sb100.AppendLine("    N085 ==> N025");
            sb100.AppendLine("    N070 -.-> N050");
            sb100.AppendLine("    N099 -- Loopback --> N000");
            sb100.AppendLine("    N049 === N025");

            string code100 = sb100.ToString();

            // Warmup
            for (int w = 0; w < 10; w++)
            {
                MermaidFlowchartParser.TryParse(code100, out _, out _);
            }

            // Benchmark 100 iterations
            int iters100 = 100;
            var sw100 = Stopwatch.StartNew();
            for (int i = 0; i < iters100; i++)
            {
                bool parsed = MermaidFlowchartParser.TryParse(code100, out var g100, out var err100);
                if (!parsed || g100 == null)
                {
                    throw new Exception($"100-node benchmark failed at iteration {i}: {err100}");
                }
            }
            sw100.Stop();

            double avgMs100 = sw100.Elapsed.TotalMilliseconds / iters100;
            Console.WriteLine($"[CHALLENGER-M2-2] 100-Node Graph: 100 iterations, avg {avgMs100:F3} ms/parse (Budget: < 2.0 ms)");

            // Measure memory allocation per parse
            long startAlloc100 = GC.GetAllocatedBytesForCurrentThread();
            int allocIters100 = 10;
            for (int i = 0; i < allocIters100; i++)
            {
                MermaidFlowchartParser.TryParse(code100, out _, out _);
            }
            long endAlloc100 = GC.GetAllocatedBytesForCurrentThread();
            long bytesPerParse100 = (endAlloc100 - startAlloc100) / allocIters100;
            Console.WriteLine($"[CHALLENGER-M2-2] 100-Node Graph Allocations: ~{bytesPerParse100 / 1024} KB/parse (Budget: < 300 KB)");

            // Integrity assertions
            MermaidFlowchartParser.TryParse(code100, out var finalG100, out _);
            AssertEqual(100, finalG100!.Nodes.Count, "100-node graph node count");


            AssertEqual(144, finalG100.Edges.Count, "100-node graph edge count");
            AssertEqual(4, finalG100.Subgraphs.Count, "100-node graph subgraph count");
            Assert(avgMs100 < 2.0, $"100-node parse speed ({avgMs100:F3} ms) exceeded 2.0 ms threshold!");
            Assert(bytesPerParse100 < 300_000, $"100-node allocation ({bytesPerParse100} bytes) exceeded 300 KB threshold!");

            // =========================================================================
            // 3. Ambiguous Syntax & Keyword Collisions (graph, subgraph, end, TD, LR, etc.)
            // =========================================================================

            // Case 3a: Node named 'graph'
            string graphKeywordDiagram = @"graph TD
    graph --> B
    B --> graph
    graph[The Graph Node] --> C[Consumer]
";
            Assert(MermaidFlowchartParser.TryParse(graphKeywordDiagram, out var gKw, out var gKwErr), $"Node 'graph' failed: {gKwErr}");
            Assert(gKw!.Nodes.ContainsKey("graph"), "Node 'graph' exists");
            AssertEqual("The Graph Node", gKw.Nodes["graph"].Text, "Node 'graph' text");
            AssertEqual(3, gKw.Edges.Count, "Node 'graph' edges count");

            // Case 3b: Node named 'TD', 'LR', 'BT', 'RL'
            string dirKeywordDiagram = @"graph LR
    TD --> LR
    LR --> BT
    BT --> RL
    TD[Top Down] --> RL([Right to Left])
";
            Assert(MermaidFlowchartParser.TryParse(dirKeywordDiagram, out var gDir, out var gDirErr), $"Dir keywords failed: {gDirErr}");
            AssertEqual(4, gDir!.Nodes.Count, "Dir keyword nodes count");
            AssertEqual(4, gDir.Edges.Count, "Dir keyword edges count");
            AssertEqual("Top Down", gDir.Nodes["TD"].Text, "Node 'TD' text");
            AssertEqual("Right to Left", gDir.Nodes["RL"].Text, "Node 'RL' text");

            // Case 3c: Node named 'end'
            // Target node: A --> end
            // Quoted source node: ""end"" --> B
            // Subgraph with node 'end'
            string endKeywordDiagram = @"graph TD
    A --> end
    ""end"" --> B
    subgraph S1 [Block 1]
        X --> end
    end
";
            Assert(MermaidFlowchartParser.TryParse(endKeywordDiagram, out var gEnd, out var gEndErr), $"Node 'end' failed: {gEndErr}");
            Assert(gEnd!.Nodes.ContainsKey("end"), "Node 'end' exists");
            AssertEqual(3, gEnd.Edges.Count, "Node 'end' edge count");
            Assert(gEnd.Subgraphs[0].NodeIds.Contains("end"), "Subgraph contains node 'end'");

            // Case 3d: Node named 'subgraph'
            // Target node: A --> subgraph
            // Quoted source node: ""subgraph"" --> B
            string subKeywordDiagram = @"graph TD
    A --> subgraph
    ""subgraph"" --> B
";
            Assert(MermaidFlowchartParser.TryParse(subKeywordDiagram, out var gSub, out var gSubErr), $"Node 'subgraph' failed: {gSubErr}");
            Assert(gSub!.Nodes.ContainsKey("subgraph"), "Node 'subgraph' exists");
            AssertEqual(2, gSub.Edges.Count, "Node 'subgraph' edge count");

            // Case 3e: Directives as node identifiers: direction, style, classDef, click, class
            string directKeywordDiagram = @"graph TD
    A --> direction
    A --> style
    A --> classDef
    A --> click
    ""direction"" --> B
    ""style"" --> C
";
            Assert(MermaidFlowchartParser.TryParse(directKeywordDiagram, out var gDkw, out var gDkwErr), $"Directive keyword nodes failed: {gDkwErr}");
            Assert(gDkw!.Nodes.ContainsKey("direction"), "Node 'direction' exists");
            Assert(gDkw.Nodes.ContainsKey("style"), "Node 'style' exists");
            Assert(gDkw.Nodes.ContainsKey("classDef"), "Node 'classDef' exists");
            Assert(gDkw.Nodes.ContainsKey("click"), "Node 'click' exists");
            AssertEqual(6, gDkw.Edges.Count, "Directive keyword edges count");

            // =========================================================================
            // 4. Arrow Lookaheads (--- vs --> vs -- label -->, dotted, thick, chained)
            // =========================================================================
            string lookaheadsDiagram = @"graph TD
    A1 --- B1
    A2 --> B2
    A3 -- Simple Label --> B3
    A4 -- Simple Link --- B4
    A5 -->|Pipe Arrow| B5
    A6 ---|Pipe Link| B6
    D1 -.- E1
    D2 -.-> E2
    D3 -. Dotted Inline .-> E3
    D4 -.-|Dotted Pipe| E4
    D5 -.->|Dotted Pipe Arrow| E5
    T1 === U1
    T2 ==> U2
    T3 == Thick Inline ==> U3
    T4 == Thick None === U4
    T5 ===|Thick Pipe| U5
    T6 ==>|Thick Pipe Arrow| U6
";
            Assert(MermaidFlowchartParser.TryParse(lookaheadsDiagram, out var gLook, out var gLookErr), $"Lookaheads parse failed: {gLookErr}");
            AssertEqual(17, gLook!.Edges.Count, "Lookaheads edge count");

            // Solid lookaheads
            AssertEqual(MermaidStrokeStyle.Solid, gLook.Edges[0].Stroke, "A1 --- B1 stroke");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[0].Arrow, "A1 --- B1 arrow");
            AssertEqual(null, gLook.Edges[0].Label, "A1 --- B1 label");

            AssertEqual(MermaidStrokeStyle.Solid, gLook.Edges[1].Stroke, "A2 --> B2 stroke");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[1].Arrow, "A2 --> B2 arrow");

            AssertEqual(MermaidStrokeStyle.Solid, gLook.Edges[2].Stroke, "A3 -- Simple Label --> B3 stroke");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[2].Arrow, "A3 -- Simple Label --> B3 arrow");
            AssertEqual("Simple Label", gLook.Edges[2].Label, "A3 -- Simple Label --> B3 label");

            AssertEqual(MermaidStrokeStyle.Solid, gLook.Edges[3].Stroke, "A4 -- Simple Link --- B4 stroke");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[3].Arrow, "A4 -- Simple Link --- B4 arrow");
            AssertEqual("Simple Link", gLook.Edges[3].Label, "A4 -- Simple Link --- B4 label");

            AssertEqual("Pipe Arrow", gLook.Edges[4].Label, "A5 -->|Pipe Arrow| B5 label");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[4].Arrow, "A5 -->|Pipe Arrow| B5 arrow");

            AssertEqual("Pipe Link", gLook.Edges[5].Label, "A6 ---|Pipe Link| B6 label");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[5].Arrow, "A6 ---|Pipe Link| B6 arrow");

            // Dotted lookaheads
            AssertEqual(MermaidStrokeStyle.Dotted, gLook.Edges[6].Stroke, "D1 -.- E1 stroke");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[6].Arrow, "D1 -.- E1 arrow");

            AssertEqual(MermaidStrokeStyle.Dotted, gLook.Edges[7].Stroke, "D2 -.-> E2 stroke");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[7].Arrow, "D2 -.-> E2 arrow");

            AssertEqual("Dotted Inline", gLook.Edges[8].Label, "D3 -. Dotted Inline .-> E3 label");
            AssertEqual(MermaidStrokeStyle.Dotted, gLook.Edges[8].Stroke, "D3 stroke");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[8].Arrow, "D3 arrow");

            AssertEqual("Dotted Pipe", gLook.Edges[9].Label, "D4 label");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[9].Arrow, "D4 arrow");

            AssertEqual("Dotted Pipe Arrow", gLook.Edges[10].Label, "D5 label");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[10].Arrow, "D5 arrow");

            // Thick lookaheads
            AssertEqual(MermaidStrokeStyle.Thick, gLook.Edges[11].Stroke, "T1 === U1 stroke");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[11].Arrow, "T1 === U1 arrow");

            AssertEqual(MermaidStrokeStyle.Thick, gLook.Edges[12].Stroke, "T2 ==> U2 stroke");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[12].Arrow, "T2 ==> U2 arrow");

            AssertEqual("Thick Inline", gLook.Edges[13].Label, "T3 label");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[13].Arrow, "T3 arrow");

            AssertEqual("Thick None", gLook.Edges[14].Label, "T4 label");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[14].Arrow, "T4 arrow");

            AssertEqual("Thick Pipe", gLook.Edges[15].Label, "T5 label");
            AssertEqual(MermaidArrowHead.None, gLook.Edges[15].Arrow, "T5 arrow");

            AssertEqual("Thick Pipe Arrow", gLook.Edges[16].Label, "T6 label");
            AssertEqual(MermaidArrowHead.Arrow, gLook.Edges[16].Arrow, "T6 arrow");

            // Multi-operator lookahead chain on single statement
            string multiChainDiagram = @"graph LR
    A --- B --> C -- Step 1 --> D ---|Step 2| E ==> F === G -.- H -.-> I
";
            Assert(MermaidFlowchartParser.TryParse(multiChainDiagram, out var gMulti, out var gMultiErr), $"Multi-chain failed: {gMultiErr}");
            AssertEqual(9, gMulti!.Nodes.Count, "Multi-chain node count");
            AssertEqual(8, gMulti.Edges.Count, "Multi-chain edge count");
            AssertEqual("Step 1", gMulti.Edges[2].Label, "Multi-chain edge 2 label");
            AssertEqual("Step 2", gMulti.Edges[3].Label, "Multi-chain edge 3 label");
            AssertEqual(MermaidStrokeStyle.Thick, gMulti.Edges[4].Stroke, "Multi-chain edge 4 stroke");
            AssertEqual(MermaidStrokeStyle.Dotted, gMulti.Edges[6].Stroke, "Multi-chain edge 6 stroke");

            // Edge cases in lookahead: IDs with hyphens, underscores, labels with hyphens
            string hyphenLookaheads = @"graph TD
    task-1 --> task-2
    node_alpha -- sub-task-label --> node_beta
    X1 -->|comparison: a >= 10 && b <= 20| X2
    X3 -->|math: x - y - z| X4
    S1 -- first-step --> S2; S3 -- second-step --- S4
";
            Assert(MermaidFlowchartParser.TryParse(hyphenLookaheads, out var gHyphen, out var gHyphenErr), $"Hyphen lookaheads failed: {gHyphenErr}");
            Assert(gHyphen!.Nodes.ContainsKey("task-1"), "Node task-1 exists");
            Assert(gHyphen.Nodes.ContainsKey("task-2"), "Node task-2 exists");
            AssertEqual("sub-task-label", gHyphen.Edges[1].Label, "Edge 1 label with hyphens");
            AssertEqual("comparison: a >= 10 && b <= 20", gHyphen.Edges[2].Label, "Edge 2 comparison label");
            AssertEqual("math: x - y - z", gHyphen.Edges[3].Label, "Edge 3 math label");
            AssertEqual("first-step", gHyphen.Edges[4].Label, "Edge 4 semicolon-separated label");
            AssertEqual("second-step", gHyphen.Edges[5].Label, "Edge 5 semicolon-separated label");
            AssertEqual(MermaidArrowHead.None, gHyphen.Edges[5].Arrow, "Edge 5 arrow none");

            Console.WriteLine("[CHALLENGER-M2-2] All Scale, Stress, Ambiguous Keywords, and Arrow Lookahead empirical tests PASSED successfully.");
        }
    }
}


