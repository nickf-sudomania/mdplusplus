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
    /// Tier 4: Real-World Application Scenarios for Mermaid Flowcharts & Inline Math Typography.
    /// Implements the 8 complex integration scenarios specified in TEST_INFRA.md and DISPATCH.md.
    /// </summary>
    public static class Tier4_MermaidMathScenarios
    {
        public static void RunAll()
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("  Tier 4: Real-World Application Scenarios (8)    ");
            Console.WriteLine("==================================================");

            RunTest("Tier4", "Scenario 1: Financial Screener Schema Query 2 Document ($ROIC > 18\\%$, $\\ge +1.5\\%$, $\\le 30\\times$)", TestScenario1_FinancialScreenerQuery2);
            RunTest("Tier4", "Scenario 2: Software Architecture Multi-Tier Flowchart (20+ nodes)", TestScenario2_SoftwareArchitecture20Nodes);
            RunTest("Tier4", "Scenario 3: Decision Tree with Diamond Nodes & Labeled Branches", TestScenario3_DecisionTreeWithDiamonds);
            RunTest("Tier4", "Scenario 4: State Machine with Cyclic Feedback Loops & Dotted Transitions", TestScenario4_StateMachineWithCycles);
            RunTest("Tier4", "Scenario 5: Mixed Document with Headings, Inline Math, Lists, and Flowcharts", TestScenario5_MixedDocumentFullPipeline);
            RunTest("Tier4", "Scenario 6: Theme Switching Live Cycle with Rendered Flowchart & Math", TestScenario6_ThemeSwitchingLiveCycle);
            RunTest("Tier4", "Scenario 7: Degraded Malformed Code Block with Mixed Syntax", TestScenario7_DegradedMalformedCodeBlocks);
            RunTest("Tier4", "Scenario 8: Large 25-Node Benchmark Graph Performance Under 25ms", TestScenario8_Large25NodeBenchmark);
        }

        /// <summary>
        /// Scenario 1: Financial Screener Schema Query 2 Document
        /// Validates optical vertical centering, zero superscript floating, and layout measurement
        /// for real-world financial expressions ($ROIC > 18\%$, $ROE > 20\%$, $\ge +1.5\%$, $\le 30\times$).
        /// </summary>
        private static void TestScenario1_FinancialScreenerQuery2()
        {
            string query2Markdown = @"# Owl Rock Financial Screener

## Query 2: Elite High-Return Compounding Businesses

* **Analyst Description:** Identify elite compounding businesses with wide economic moats generating high returns on invested capital ($ROIC > 18\%$, $ROE > 20\%$) where pricing power is actively expanding gross margins by at least 150 bps year-over-year while operating margins expand sequentially.
* **Screening Criteria:**
  * Return on Invested Capital: ROIC $> 18\%$
  * Return on Equity: ROE $> 20\%$
  * Gross Margin YoY Expansion: $\ge +1.5\%$ (+150 bps)
  * Operating Margin QoQ Expansion: $> 0$
  * Valuation Multiple: Forward P/E $\le 30\times$";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(query2Markdown));

            var rtb = new RichTextBox
            {
                Document = flowDoc,
                Width = 850,
                IsReadOnly = true
            };
            rtb.Measure(new Size(850, 3000));
            rtb.Arrange(new Rect(0, 0, 850, rtb.DesiredSize.Height));
            rtb.UpdateLayout();

            AssertTrue(rtb.ActualWidth > 0, "RichTextBox must measure width");
            AssertTrue(rtb.ActualHeight > 0, "RichTextBox must measure height");

            // Extract all InlineUIContainer elements
            var containers = new List<InlineUIContainer>();
            foreach (var block in flowDoc.Blocks)
            {
                if (block is Paragraph p)
                {
                    containers.AddRange(p.Inlines.OfType<InlineUIContainer>());
                }
                else if (block is List list)
                {
                    foreach (var item in list.ListItems)
                    {
                        foreach (var ib in item.Blocks)
                        {
                            if (ib is Paragraph ip)
                            {
                                containers.AddRange(ip.Inlines.OfType<InlineUIContainer>());
                            }
                            else if (ib is List nestedList)
                            {
                                foreach (var nestedItem in nestedList.ListItems)
                                {
                                    foreach (var nb in nestedItem.Blocks)
                                    {
                                        if (nb is Paragraph np)
                                        {
                                            containers.AddRange(np.Inlines.OfType<InlineUIContainer>());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            AssertTrue(containers.Count >= 6, $"Query 2 must contain at least 6 math expressions, found {containers.Count}");

            foreach (var uic in containers)
            {
                AssertEqual(BaselineAlignment.Center, uic.BaselineAlignment, "Math container must have BaselineAlignment.Center");
                var border = (Border)uic.Child;
                AssertEqual(2.5, border.Margin.Top, "Top margin must be 2.5 DIPs");
                AssertEqual(-2.5, border.Margin.Bottom, "Bottom margin must be -2.5 DIPs");
                AssertTrue(border.ActualWidth > 0, "ActualWidth must be positive (no clipping)");
                AssertTrue(border.ActualHeight > 0, "ActualHeight must be positive (no clipping)");
            }
        }

        /// <summary>
        /// Scenario 2: Software Architecture Multi-Tier Flowchart (20+ nodes)
        /// Validates layered placement, non-overlapping bounds, and vector rendering of a 22-node system architecture.
        /// </summary>
        private static void TestScenario2_SoftwareArchitecture20Nodes()
        {
            string diagram = @"graph TD
                subgraph ClientTier [Edge Clients]
                    Web([Web SPA])
                    Mobile([iOS / Android])
                    CLI([Developer CLI])
                end

                subgraph GatewayTier [Ingress Gateway]
                    CDN[Cloudflare CDN]
                    LB[NLB Load Balancer]
                    APIGW{Kong API Gateway}
                end

                subgraph ServiceTier [Microservices]
                    AuthSvc[Auth Service]
                    OrderSvc[Order Processing]
                    InventorySvc[Inventory Service]
                    BillingSvc[Stripe Billing]
                    NotificationSvc[Push Notifications]
                end

                subgraph MessagingTier [Event Bus]
                    Kafka[Apache Kafka Event Stream]
                end

                subgraph DataTier [Storage]
                    Redis[Redis Cluster Cache]
                    Postgres[PostgreSQL Primary]
                    ReadReplica[PostgreSQL Replica]
                    S3[Object Storage]
                end

                subgraph ObservabilityTier [Telemetry]
                    Prometheus[Prometheus Metrics]
                    Grafana[Grafana Dashboard]
                    OtelCollector[OpenTelemetry]
                    AlertManager[Alert Manager]
                    Jaeger[Distributed Tracing]
                end

                Web --> CDN --> LB --> APIGW
                Mobile --> APIGW
                CLI --> APIGW

                APIGW --> AuthSvc
                APIGW --> OrderSvc
                APIGW --> InventorySvc

                AuthSvc --> Redis
                OrderSvc --> Postgres
                OrderSvc --> Kafka
                InventorySvc --> Postgres
                BillingSvc --> S3

                Kafka --> BillingSvc
                Kafka --> NotificationSvc

                Postgres -.-> ReadReplica
                OrderSvc -.-> OtelCollector --> Prometheus --> Grafana
                Prometheus --> AlertManager
                OtelCollector --> Jaeger";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out string? error), $"Parse error: {error}");
            AssertTrue(g!.Nodes.Count >= 20, $"Node count must be >= 20, got {g.Nodes.Count}");
            AssertTrue(g.Subgraphs.Count >= 5, "Subgraphs count must be >= 5");

            var layout = MermaidLayoutEngine.Layout(g);
            AssertTrue(layout.TotalWidth > 0);
            AssertTrue(layout.TotalHeight > 0);

            // Verify non-overlapping bounds
            var nodes = layout.Nodes.Values.ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    AssertFalse(nodes[i].Bounds.IntersectsWith(nodes[j].Bounds),
                        $"Overlap detected between {nodes[i].Node.Id} and {nodes[j].Node.Id}");
                }
            }

            var visual = (Border)MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            AssertNotNull(visual);
        }

        /// <summary>
        /// Scenario 3: Decision Tree with Diamond Nodes & Labeled Branches
        /// Validates loan origination decision tree with multiple branching condition nodes.
        /// </summary>
        private static void TestScenario3_DecisionTreeWithDiamonds()
        {
            string diagram = @"graph TD
                Applicant([Loan Applicant]) --> CheckCredit{Credit Score >= 700?}
                CheckCredit -->|Yes| CheckDTI{DTI <= 35%?}
                CheckCredit -->|No| SubprimeCheck{Credit Score >= 620?}

                CheckDTI -->|Yes| AutoApprove([Instant Approval])
                CheckDTI -->|No| UnderwriterReview[Manual Underwriter Review]

                SubprimeCheck -->|Yes| CollateralCheck{Sufficient Collateral?}
                SubprimeCheck -->|No| Decline([Application Declined])

                CollateralCheck -->|Yes| ManualReview[Senior Review]
                CollateralCheck -->|No| Decline";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            var diamondNodes = g!.Nodes.Values.Where(n => n.Shape == MermaidNodeShape.Diamond).ToList();
            AssertTrue(diamondNodes.Count >= 3, "Must have at least 3 decision diamond nodes");

            var labeledEdges = g.Edges.Where(e => !string.IsNullOrEmpty(e.Label)).ToList();
            AssertTrue(labeledEdges.Count >= 6, "Must have at least 6 labeled branch edges");

            var layout = MermaidLayoutEngine.Layout(g);
            AssertEqual(g.Nodes.Count, layout.Nodes.Count);
        }

        /// <summary>
        /// Scenario 4: State Machine with Cyclic Feedback Loops & Dotted Transitions
        /// Validates connection lifecycle state machine with cyclic feedback and dotted retry edges.
        /// </summary>
        private static void TestScenario4_StateMachineWithCycles()
        {
            string diagram = @"graph LR
                Disconnected([Disconnected]) -->|Connect| Connecting([Connecting])
                Connecting -->|Ack| Connected([Connected])
                Connected -->|Heartbeat| Connected
                Connected -->|Tx Data| InFlight[Transmitting]
                InFlight -->|Ack| Connected
                InFlight -.->|Timeout| RetryCheck{Retries < 3?}
                RetryCheck -->|Yes| Connecting
                RetryCheck -.->|No, Fatal| Disconnected";

            AssertTrue(MermaidFlowchartParser.TryParse(diagram, out var g, out _));
            AssertEqual(MermaidOrientation.LeftToRight, g!.Orientation);

            var layout = MermaidLayoutEngine.Layout(g);
            var feedbackEdges = layout.Edges.Where(e => e.IsFeedbackEdge).ToList();
            AssertTrue(feedbackEdges.Count >= 1, "Must identify cyclic feedback edge");

            var dottedEdges = layout.Edges.Where(e => e.Edge.Stroke == MermaidStrokeStyle.Dotted).ToList();
            AssertTrue(dottedEdges.Count >= 2, "Must identify dotted transition edges");
        }

        /// <summary>
        /// Scenario 5: Mixed Document with Headings, Inline Math, Lists, and Flowcharts
        /// Validates full FlowDocument generation and layout measurement of a comprehensive technical document.
        /// </summary>
        private static void TestScenario5_MixedDocumentFullPipeline()
        {
            string md = @"# Machine Learning Optimization Pipeline

Optimization minimizes the loss function $L(\theta) = -\frac{1}{N}\sum_{i=1}^N \log p_\theta(y_i|x_i)$ using gradient descent:

$$\theta_{t+1} = \theta_t - \eta \nabla L(\theta_t)$$

## Workflow Architecture

```mermaid
graph TD
    Data[Training Dataset] --> Preprocess[Tokenization & Normalization]
    Preprocess --> Model[Transformer Model]
    Model --> LossCalc{Loss < Threshold?}
    LossCalc -->|Yes| Export[Saved Artifact]
    LossCalc -.->|No| Backprop[Backpropagation] --> Model
```

### Convergence Criteria
* Target Validation Accuracy: $\ge 98.5\%$
* Learning Rate Decay: $\eta_t = \eta_0 \times (1 + \lambda t)^{-1}$
* Gradient Norm Clip: $\|g\| \le 1.0$";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));

            var rtb = new RichTextBox
            {
                Document = flowDoc,
                Width = 800,
                IsReadOnly = true
            };
            rtb.Measure(new Size(800, 4000));
            rtb.Arrange(new Rect(0, 0, 800, rtb.DesiredSize.Height));
            rtb.UpdateLayout();

            AssertTrue(rtb.ActualHeight > 0);
            AssertTrue(flowDoc.Blocks.OfType<BlockUIContainer>().Any(), "Must contain flowchart BlockUIContainer");
            AssertTrue(flowDoc.Blocks.OfType<List>().Any(), "Must contain List");
        }

        /// <summary>
        /// Scenario 6: Theme Switching Live Cycle with Rendered Flowchart & Math
        /// Cycles through all 8 light and dark themes on a rendered document and validates palette consistency.
        /// </summary>
        private static void TestScenario6_ThemeSwitchingLiveCycle()
        {
            string md = @"# Theme Stress Document

Math formula: $E = mc^2$ and $ROIC > 18\%$

```mermaid
graph TD
A[Alpha] --> B[Beta] --> C[Gamma]
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
                ThemePalette.QuietLight,
                ThemePalette.GitHubDark
            };

            foreach (var palette in presets)
            {
                var converter = new MarkdownToWpfConverter("", palette, enableLatex: true, enableHtml: true);
                var flowDoc = converter.Convert(doc);

                var buic = flowDoc.Blocks.OfType<BlockUIContainer>().First();
                var border = (Border)buic.Child;
                AssertEqual(palette.CodeBg, border.Background, $"CodeBg must match palette {palette.Name}");
            }
        }

        /// <summary>
        /// Scenario 7: Degraded Malformed Code Block with Mixed Syntax
        /// Validates coexistence of valid C#, malformed Mermaid, and valid Mermaid in a single document.
        /// </summary>
        private static void TestScenario7_DegradedMalformedCodeBlocks()
        {
            string md = @"# Multi-Block Document

```csharp
public class Worker { public int Id { get; set; } }
```

```mermaid
graph TD
Broken[Unclosed delimiter
```

```mermaid
graph TD
CleanA --> CleanB
```";

            var parser = new MarkdownParser();
            var converter = new MarkdownToWpfConverter("", ThemePalette.GitHubDark, enableLatex: true, enableHtml: true);
            var flowDoc = converter.Convert(parser.Parse(md));

            var buics = flowDoc.Blocks.OfType<BlockUIContainer>().ToList();
            AssertEqual(3, buics.Count, "Must produce 3 BlockUIContainers");

            // Block 1: C# -> TextBlock
            var b1Grid = (Grid)((Border)buics[0].Child).Child;
            AssertFalse(b1Grid.Children.OfType<MermaidScrollViewer>().Any());

            // Block 2: Broken Mermaid -> TextBlock (fallback)
            var b2Grid = (Grid)((Border)buics[1].Child).Child;
            AssertFalse(b2Grid.Children.OfType<MermaidScrollViewer>().Any(), "Broken mermaid must fall back without scroll viewer");
            AssertTrue(b2Grid.Children.OfType<TextBlock>().Any(), "Broken mermaid must display raw syntax highlighted code");

            // Block 3: Clean Mermaid -> MermaidScrollViewer
            var b3Grid = (Grid)((Border)buics[2].Child).Child;
            AssertTrue(b3Grid.Children.OfType<MermaidScrollViewer>().Any(), "Clean mermaid must render flowchart scroll viewer");
        }

        /// <summary>
        /// Scenario 8: Large 25-Node Benchmark Graph Performance Under 25ms
        /// Validates that a large 25-node graph with multiple branches and cycles parses, layouts,
        /// and renders in < 25ms total pipeline latency (< 20ms layout).
        /// </summary>
        private static void TestScenario8_Large25NodeBenchmark()
        {
            var lines = new List<string> { "graph TD" };
            // Generate a 25-node interconnected graph
            for (int i = 1; i <= 25; i++)
            {
                lines.Add($"N{i}[Node {i}] --> N{(i % 25) + 1}[Node {(i % 25) + 1}]");
                if (i % 5 == 0)
                {
                    lines.Add($"N{i} --> N{Math.Max(1, (i + 7) % 25)}");
                }
            }
            string diagram = string.Join("\n", lines);

            var swTotal = Stopwatch.StartNew();
            bool parsed = MermaidFlowchartParser.TryParse(diagram, out var g, out _);
            AssertTrue(parsed, "25-node graph must parse");

            var swLayout = Stopwatch.StartNew();
            var layout = MermaidLayoutEngine.Layout(g!);
            swLayout.Stop();

            var visual = MermaidFlowchartRenderer.CreateFlowchartVisual(layout, ThemePalette.GitHubDark, diagram);
            swTotal.Stop();

            AssertNotNull(visual);
            AssertEqual(25, layout.Nodes.Count);

            AssertTrue(swLayout.ElapsedMilliseconds < 20.0,
                $"25-node layout latency ({swLayout.ElapsedMilliseconds} ms) must be < 20.0 ms");
            AssertTrue(swTotal.ElapsedMilliseconds < 25.0,
                $"25-node total pipeline latency ({swTotal.ElapsedMilliseconds} ms) must be < 25.0 ms");
        }
    }
}
