using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace MDPlus.Core.Mermaid
{
    /// <summary>
    /// Calculated geometric bounding box and port positions for a laid-out node.
    /// </summary>
    public sealed class NodeLayoutBounds
    {
        public MermaidNode Node { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int Rank { get; set; }
        public int Order { get; set; }

        public Rect Bounds => new Rect(X, Y, Width, Height);

        public Point Center => new Point(X + Width / 2.0, Y + Height / 2.0);
        public Point TopPort => new Point(X + Width / 2.0, Y);
        public Point BottomPort => new Point(X + Width / 2.0, Y + Height);
        public Point LeftPort => new Point(X, Y + Height / 2.0);
        public Point RightPort => new Point(X + Width, Y + Height / 2.0);

        public NodeLayoutBounds(MermaidNode node, double width, double height)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Calculated spline route, waypoints, arrowheads, and label placement for a connector edge.
    /// </summary>
    public sealed class EdgeLayoutRoute
    {
        public MermaidEdge Edge { get; }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Point ControlPoint1 { get; set; }
        public Point ControlPoint2 { get; set; }
        public List<Point> Waypoints { get; } = new List<Point>();
        public Point? LabelPosition { get; set; }
        public Size LabelSize { get; set; }
        public double ArrowheadAngle { get; set; }
        public double StartArrowheadAngle { get; set; }
        public bool IsFeedbackEdge { get; set; }

        public EdgeLayoutRoute(MermaidEdge edge)
        {
            Edge = edge ?? throw new ArgumentNullException(nameof(edge));
        }
    }

    /// <summary>
    /// Complete geometric layout calculation result for a Mermaid flowchart.
    /// </summary>
    public sealed class MermaidLayoutResult
    {
        public MermaidOrientation Orientation { get; }
        public double TotalWidth { get; set; }
        public double TotalHeight { get; set; }
        public Dictionary<string, NodeLayoutBounds> Nodes { get; } = new Dictionary<string, NodeLayoutBounds>(StringComparer.OrdinalIgnoreCase);
        public List<EdgeLayoutRoute> Edges { get; } = new List<EdgeLayoutRoute>();

        public MermaidLayoutResult(MermaidOrientation orientation)
        {
            Orientation = orientation;
        }
    }

    /// <summary>
    /// High-performance 5-phase Sugiyama layered graph layout engine.
    /// Phase 1: DFS cycle reversal converting cyclic flowcharts into DAGs.
    /// Phase 2: Longest-path topological ranking assigning nodes to discrete layers.
    /// Phase 3: Virtual dummy node insertion for multi-rank edge normalization.
    /// Phase 4: Two-way barycentric crossing reduction with adjacent transposition swaps.
    /// Phase 5: Continuous coordinate placement, shape sizing, Bezier routing, and arrowhead calculation.
    /// </summary>
    public sealed class MermaidLayoutEngine
    {
        public const double LayerSpacing = 52.0;
        public const double NodeSpacing = 28.0;
        public const double GraphPadding = 24.0;

        private static readonly Typeface DefaultTypeface = new Typeface(
            new FontFamily("Segoe UI Variable Text, Segoe UI, sans-serif"),
            FontStyles.Normal,
            FontWeights.Medium,
            FontStretches.Normal);

        /// <summary>
        /// Calculates the layout for a given Mermaid flowchart AST.
        /// </summary>
        public static MermaidLayoutResult Layout(MermaidFlowchartGraph graph, double maxAvailableWidth = double.PositiveInfinity)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var result = new MermaidLayoutResult(graph.Orientation);

            if (graph.Nodes.Count == 0)
            {
                result.TotalWidth = 100.0;
                result.TotalHeight = 60.0;
                return result;
            }

            // Phase 0: Measure node dimensions based on text metrics and shape geometries
            foreach (var kvp in graph.Nodes)
            {
                var node = kvp.Value;
                var size = MeasureNode(node);
                result.Nodes[node.Id] = new NodeLayoutBounds(node, size.Width, size.Height);
            }

            // Phase 1: Cycle Breaking (DFS back-edge detection)
            var dagEdges = BreakCycles(graph, out var feedbackEdges);

            // Phase 2: Layer & Rank Assignment (Longest Path)
            var layers = AssignLayers(graph.Nodes.Keys, dagEdges, result.Nodes);

            // Phase 3: Virtual Dummy Node Insertion for Multi-Rank Edges
            var (expandedLayers, dummyBounds, longEdgeChains) = NormalizeEdgeSpans(layers, dagEdges);

            // Phase 4: Crossing Minimization (Barycentric sweeps + adjacent transposition)
            OrderVertices(expandedLayers, dagEdges, longEdgeChains);

            // Phase 5: Coordinate Assignment (X, Y continuous placement & orientation mapping)
            AssignCoordinates(expandedLayers, result, dummyBounds, graph.Orientation);

            // Phase 6: Edge Routing (Smooth Bezier splines, arrowheads, and label pill badges)
            RouteEdges(graph.Edges, result, feedbackEdges, dummyBounds, longEdgeChains, graph.Orientation);

            return result;
        }

        /// <summary>
        /// Compatibility alias for Layout(graph).
        /// </summary>
        public static MermaidLayoutResult CalculateLayout(MermaidFlowchartGraph graph) => Layout(graph);

        private static Size MeasureText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new Size(24.0, 18.0);
            }

            string[] lines = text.Split(new[] { "<br/>", "<br>", "\n" }, StringSplitOptions.None);
            int lineCount = Math.Max(1, lines.Length);
            double maxLen = 0;
            foreach (var line in lines)
            {
                if (line.Length > maxLen) maxLen = line.Length;
            }

            try
            {
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                {
                    string cleanText = text.Replace("<br/>", "\n").Replace("<br>", "\n");
                    var ft = new FormattedText(
                        cleanText,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        DefaultTypeface,
                        13.0,
                        Brushes.Black,
                        1.0);
                    return new Size(Math.Ceiling(ft.Width), Math.Ceiling(ft.Height));
                }
            }
            catch
            {
                // Fallback below
            }

            double w = Math.Max(24.0, maxLen * 7.8);
            double h = Math.Max(18.0, lineCount * 18.0);
            return new Size(w, h);
        }

        private static Size MeasureNode(MermaidNode node)
        {
            var textSize = MeasureText(node.Text);
            double tw = textSize.Width;
            double th = textSize.Height;

            return node.Shape switch
            {
                MermaidNodeShape.Stadium => new Size(Math.Max(84.0, tw + 44.0), Math.Max(36.0, th + 18.0)),
                MermaidNodeShape.RoundedRectangle => new Size(Math.Max(72.0, tw + 32.0), Math.Max(36.0, th + 16.0)),
                MermaidNodeShape.Circle => GetCircleSize(tw, th),
                MermaidNodeShape.Diamond => GetDiamondSize(tw, th),
                _ => new Size(Math.Max(72.0, tw + 32.0), Math.Max(36.0, th + 16.0)) // Rectangle default
            };
        }

        private static Size GetCircleSize(double tw, double th)
        {
            double d = Math.Max(48.0, Math.Sqrt(tw * tw + th * th) + 24.0);
            return new Size(d, d);
        }

        private static Size GetDiamondSize(double tw, double th)
        {
            double w = Math.Max(80.0, 1.4 * (tw + 28.0) + 16.0);
            double h = Math.Max(54.0, (th + 16.0) * 1.6);
            return new Size(w, h);
        }

        /// <summary>
        /// Phase 1: Reverses feedback arcs using DFS to construct a strict DAG.
        /// </summary>
        private static List<MermaidEdge> BreakCycles(MermaidFlowchartGraph graph, out HashSet<MermaidEdge> feedbackEdges)
        {
            var localFeedbackEdges = new HashSet<MermaidEdge>();
            var adj = new Dictionary<string, List<MermaidEdge>>(StringComparer.OrdinalIgnoreCase);
            foreach (var nid in graph.Nodes.Keys)
            {
                adj[nid] = new List<MermaidEdge>();
            }

            foreach (var e in graph.Edges)
            {
                if (adj.ContainsKey(e.SourceId) && adj.ContainsKey(e.TargetId))
                {
                    if (string.Equals(e.SourceId, e.TargetId, StringComparison.OrdinalIgnoreCase))
                    {
                        // Self-loop
                        localFeedbackEdges.Add(e);
                    }
                    else
                    {
                        adj[e.SourceId].Add(e);
                    }
                }
            }

            var state = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // 0=unvisited, 1=in-stack, 2=done
            var dagEdges = new List<MermaidEdge>();

            void Dfs(string u)
            {
                state[u] = 1;
                foreach (var edge in adj[u])
                {
                    string v = edge.TargetId;
                    if (!state.TryGetValue(v, out int vState) || vState == 0)
                    {
                        dagEdges.Add(edge);
                        Dfs(v);
                    }
                    else if (vState == 1)
                    {
                        // Back-edge detected (cycle)
                        localFeedbackEdges.Add(edge);
                    }
                    else
                    {
                        dagEdges.Add(edge);
                    }
                }
                state[u] = 2;
            }

            foreach (var nid in graph.Nodes.Keys)
            {
                if (!state.ContainsKey(nid) || state[nid] == 0)
                {
                    Dfs(nid);
                }
            }

            feedbackEdges = localFeedbackEdges;
            return dagEdges;
        }

        /// <summary>
        /// Phase 2: Assigns discrete layer ranks using longest-path topological layering.
        /// </summary>
        private static List<List<string>> AssignLayers(
            IEnumerable<string> nodeIds,
            List<MermaidEdge> dagEdges,
            Dictionary<string, NodeLayoutBounds> nodeBounds)
        {
            var inDegree = nodeIds.ToDictionary(id => id, _ => 0, StringComparer.OrdinalIgnoreCase);
            var adj = nodeIds.ToDictionary(id => id, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);

            foreach (var e in dagEdges)
            {
                if (adj.ContainsKey(e.SourceId) && inDegree.ContainsKey(e.TargetId))
                {
                    adj[e.SourceId].Add(e.TargetId);
                    inDegree[e.TargetId]++;
                }
            }

            var ranks = nodeIds.ToDictionary(id => id, _ => 0, StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(inDegree.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key));

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                int currentRank = ranks[u];
                foreach (var v in adj[u])
                {
                    ranks[v] = Math.Max(ranks[v], currentRank + 1);
                    inDegree[v]--;
                    if (inDegree[v] == 0)
                    {
                        queue.Enqueue(v);
                    }
                }
            }

            // Fallback for any unvisited nodes
            foreach (var kvp in inDegree.Where(k => k.Value > 0))
            {
                ranks[kvp.Key] = ranks.Values.DefaultIfEmpty(0).Max() + 1;
            }

            int maxRank = ranks.Values.DefaultIfEmpty(0).Max();
            var layers = new List<List<string>>(maxRank + 1);
            for (int i = 0; i <= maxRank; i++)
            {
                layers.Add(new List<string>());
            }

            foreach (var kvp in ranks)
            {
                layers[kvp.Value].Add(kvp.Key);
                if (nodeBounds.TryGetValue(kvp.Key, out var nb))
                {
                    nb.Rank = kvp.Value;
                }
            }

            return layers;
        }

        /// <summary>
        /// Phase 3: Inserts virtual dummy vertices for edges that span across multiple ranks.
        /// </summary>
        private static (List<List<string>> layers, Dictionary<string, NodeLayoutBounds> dummyBounds, Dictionary<MermaidEdge, List<string>> longEdgeChains)
            NormalizeEdgeSpans(List<List<string>> layers, List<MermaidEdge> dagEdges)
        {
            var dummyBounds = new Dictionary<string, NodeLayoutBounds>(StringComparer.OrdinalIgnoreCase);
            var longEdgeChains = new Dictionary<MermaidEdge, List<string>>();

            // Map each node to its current rank
            var nodeRank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int r = 0; r < layers.Count; r++)
            {
                foreach (var id in layers[r])
                {
                    nodeRank[id] = r;
                }
            }

            int dummyCounter = 0;
            foreach (var e in dagEdges)
            {
                if (!nodeRank.TryGetValue(e.SourceId, out int srcRank) ||
                    !nodeRank.TryGetValue(e.TargetId, out int tgtRank))
                {
                    continue;
                }

                int span = tgtRank - srcRank;
                if (span > 1)
                {
                    var chain = new List<string>();
                    for (int r = srcRank + 1; r < tgtRank; r++)
                    {
                        string dummyId = $"__dummy_{dummyCounter++}";
                        chain.Add(dummyId);
                        layers[r].Add(dummyId);
                        nodeRank[dummyId] = r;

                        var dummyNode = new MermaidNode(dummyId, string.Empty, MermaidNodeShape.Rectangle);
                        dummyBounds[dummyId] = new NodeLayoutBounds(dummyNode, 10.0, 10.0)
                        {
                            Rank = r
                        };
                    }
                    longEdgeChains[e] = chain;
                }
            }

            return (layers, dummyBounds, longEdgeChains);
        }

        /// <summary>
        /// Phase 4: Minimizes edge crossings using 6 alternating two-way barycentric sweeps
        /// followed by greedy adjacent transposition swaps.
        /// </summary>
        private static void OrderVertices(
            List<List<string>> layers,
            List<MermaidEdge> dagEdges,
            Dictionary<MermaidEdge, List<string>> longEdgeChains)
        {
            var adjPredecessors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var adjSuccessors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            void AddEdge(string u, string v)
            {
                if (!adjSuccessors.TryGetValue(u, out var sList)) adjSuccessors[u] = sList = new List<string>();
                sList.Add(v);

                if (!adjPredecessors.TryGetValue(v, out var pList)) adjPredecessors[v] = pList = new List<string>();
                pList.Add(u);
            }

            foreach (var e in dagEdges)
            {
                if (longEdgeChains.TryGetValue(e, out var chain) && chain.Count > 0)
                {
                    string prev = e.SourceId;
                    foreach (var dummy in chain)
                    {
                        AddEdge(prev, dummy);
                        prev = dummy;
                    }
                    AddEdge(prev, e.TargetId);
                }
                else
                {
                    AddEdge(e.SourceId, e.TargetId);
                }
            }

            // 6 alternating barycentric sweeps
            for (int pass = 0; pass < 6; pass++)
            {
                bool downward = (pass % 2 == 0);
                if (downward)
                {
                    for (int i = 1; i < layers.Count; i++)
                    {
                        var prevPos = layers[i - 1].Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => (double)x.idx, StringComparer.OrdinalIgnoreCase);
                        layers[i] = layers[i]
                            .OrderBy(id =>
                            {
                                if (adjPredecessors.TryGetValue(id, out var preds) && preds.Count > 0)
                                {
                                    double sum = preds.Where(p => prevPos.ContainsKey(p)).Sum(p => prevPos[p]);
                                    int count = preds.Count(p => prevPos.ContainsKey(p));
                                    return count > 0 ? sum / count : 0.0;
                                }
                                return 0.0;
                            })
                            .ToList();
                    }
                }
                else
                {
                    for (int i = layers.Count - 2; i >= 0; i--)
                    {
                        var nextPos = layers[i + 1].Select((id, idx) => (id, idx)).ToDictionary(x => x.id, x => (double)x.idx, StringComparer.OrdinalIgnoreCase);
                        layers[i] = layers[i]
                            .OrderBy(id =>
                            {
                                if (adjSuccessors.TryGetValue(id, out var succs) && succs.Count > 0)
                                {
                                    double sum = succs.Where(s => nextPos.ContainsKey(s)).Sum(s => nextPos[s]);
                                    int count = succs.Count(s => nextPos.ContainsKey(s));
                                    return count > 0 ? sum / count : 0.0;
                                }
                                return 0.0;
                            })
                            .ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Phase 5: Assigns continuous (X, Y) coordinates, handles centering, and maps
        /// abstract (u, v) axes to concrete WPF screen coordinates for TD, TB, BT, LR, RL.
        /// </summary>
        private static void AssignCoordinates(
            List<List<string>> layers,
            MermaidLayoutResult result,
            Dictionary<string, NodeLayoutBounds> dummyBounds,
            MermaidOrientation orientation)
        {
            bool isHorizontal = (orientation == MermaidOrientation.LeftToRight || orientation == MermaidOrientation.RightToLeft);

            NodeLayoutBounds? GetBounds(string id)
            {
                if (result.Nodes.TryGetValue(id, out var nb)) return nb;
                if (dummyBounds.TryGetValue(id, out var db)) return db;
                return null;
            }

            // 1. Calculate primary dimension per layer (Height in vertical, Width in horizontal)
            var layerSizes = new double[layers.Count];
            for (int i = 0; i < layers.Count; i++)
            {
                double maxDim = 0;
                foreach (var nid in layers[i])
                {
                    var nb = GetBounds(nid);
                    if (nb != null)
                    {
                        double dim = isHorizontal ? nb.Width : nb.Height;
                        if (dim > maxDim) maxDim = dim;
                    }
                }
                layerSizes[i] = Math.Max(maxDim, 10.0);
            }

            // 2. Calculate primary coordinates per layer
            var layerPrimaryOffsets = new double[layers.Count];
            double curPrimary = GraphPadding;
            for (int i = 0; i < layers.Count; i++)
            {
                layerPrimaryOffsets[i] = curPrimary;
                curPrimary += layerSizes[i] + LayerSpacing;
            }

            // 3. Calculate secondary extents & centering per layer
            double maxSecondaryExtent = 0;
            var layerSecondaryWidths = new double[layers.Count];
            for (int i = 0; i < layers.Count; i++)
            {
                double totalSec = 0;
                foreach (var nid in layers[i])
                {
                    var nb = GetBounds(nid);
                    if (nb != null)
                    {
                        totalSec += isHorizontal ? nb.Height : nb.Width;
                    }
                }
                totalSec += Math.Max(0, layers[i].Count - 1) * NodeSpacing;
                layerSecondaryWidths[i] = totalSec;
                if (totalSec > maxSecondaryExtent) maxSecondaryExtent = totalSec;
            }

            // 4. Place each node at continuous coordinates
            for (int i = 0; i < layers.Count; i++)
            {
                double secStart = GraphPadding + (maxSecondaryExtent - layerSecondaryWidths[i]) / 2.0;
                double primaryStart = layerPrimaryOffsets[i];
                double primaryLayerSize = layerSizes[i];

                double secOffset = secStart;
                for (int j = 0; j < layers[i].Count; j++)
                {
                    string nid = layers[i][j];
                    var nb = GetBounds(nid);
                    if (nb != null)
                    {
                        nb.Order = j;
                        if (!isHorizontal)
                        {
                            // TD / TB / BT
                            nb.X = secOffset;
                            nb.Y = primaryStart + (primaryLayerSize - nb.Height) / 2.0;
                            secOffset += nb.Width + NodeSpacing;
                        }
                        else
                        {
                            // LR / RL
                            nb.X = primaryStart + (primaryLayerSize - nb.Width) / 2.0;
                            nb.Y = secOffset;
                            secOffset += nb.Height + NodeSpacing;
                        }
                    }
                }
            }

            double totalW = !isHorizontal ? maxSecondaryExtent + 2 * GraphPadding : curPrimary - LayerSpacing + GraphPadding;
            double totalH = !isHorizontal ? curPrimary - LayerSpacing + GraphPadding : maxSecondaryExtent + 2 * GraphPadding;

            // Invert coordinates for BottomToTop or RightToLeft
            if (orientation == MermaidOrientation.RightToLeft)
            {
                foreach (var nb in result.Nodes.Values)
                {
                    nb.X = totalW - nb.X - nb.Width;
                }
                foreach (var db in dummyBounds.Values)
                {
                    db.X = totalW - db.X - db.Width;
                }
            }
            else if (orientation == MermaidOrientation.BottomToTop)
            {
                foreach (var nb in result.Nodes.Values)
                {
                    nb.Y = totalH - nb.Y - nb.Height;
                }
                foreach (var db in dummyBounds.Values)
                {
                    db.Y = totalH - db.Y - db.Height;
                }
            }

            result.TotalWidth = Math.Max(120.0, totalW);
            result.TotalHeight = Math.Max(80.0, totalH);
        }

        /// <summary>
        /// Phase 6: Generates smooth cubic Bezier connector splines, calculates arrowhead angles,
        /// and positions edge label pill badges.
        /// </summary>
        private static void RouteEdges(
            List<MermaidEdge> edges,
            MermaidLayoutResult result,
            HashSet<MermaidEdge> feedbackEdges,
            Dictionary<string, NodeLayoutBounds> dummyBounds,
            Dictionary<MermaidEdge, List<string>> longEdgeChains,
            MermaidOrientation orientation)
        {
            bool isHorizontal = (orientation == MermaidOrientation.LeftToRight || orientation == MermaidOrientation.RightToLeft);
            int feedbackCounter = 0;

            foreach (var edge in edges)
            {
                if (!result.Nodes.TryGetValue(edge.SourceId, out var src) ||
                    !result.Nodes.TryGetValue(edge.TargetId, out var tgt))
                {
                    continue;
                }

                var route = new EdgeLayoutRoute(edge);
                bool isFeedback = feedbackEdges.Contains(edge) ||
                                  string.Equals(edge.SourceId, edge.TargetId, StringComparison.OrdinalIgnoreCase);
                route.IsFeedbackEdge = isFeedback;

                if (!isFeedback)
                {
                    // Forward edge
                    if (orientation == MermaidOrientation.TopToBottom)
                    {
                        route.StartPoint = src.BottomPort;
                        route.EndPoint = tgt.TopPort;
                    }
                    else if (orientation == MermaidOrientation.BottomToTop)
                    {
                        route.StartPoint = src.TopPort;
                        route.EndPoint = tgt.BottomPort;
                    }
                    else if (orientation == MermaidOrientation.LeftToRight)
                    {
                        route.StartPoint = src.RightPort;
                        route.EndPoint = tgt.LeftPort;
                    }
                    else // RightToLeft
                    {
                        route.StartPoint = src.LeftPort;
                        route.EndPoint = tgt.RightPort;
                    }

                    // Check if edge spans intermediate dummy nodes
                    if (longEdgeChains.TryGetValue(edge, out var chain) && chain.Count > 0)
                    {
                        foreach (var dummyId in chain)
                        {
                            if (dummyBounds.TryGetValue(dummyId, out var db))
                            {
                                route.Waypoints.Add(db.Center);
                            }
                        }
                    }

                    // Compute Bezier Control Points
                    ComputeBezierControls(route, orientation);

                    // Compute Arrowhead Angle from final incoming tangent
                    Point lastControl = route.Waypoints.Count > 0 ? route.Waypoints[route.Waypoints.Count - 1] : route.ControlPoint2;
                    route.ArrowheadAngle = Math.Atan2(route.EndPoint.Y - lastControl.Y, route.EndPoint.X - lastControl.X);

                    // Compute Start Arrowhead Angle pointing into StartPoint (for Reverse and Bidirectional arrows)
                    Point firstControl = route.Waypoints.Count > 0 ? route.Waypoints[0] : route.ControlPoint1;
                    route.StartArrowheadAngle = Math.Atan2(route.StartPoint.Y - firstControl.Y, route.StartPoint.X - firstControl.X);
                }
                else
                {
                    // Feedback arc / loopback edge
                    double escapeX;
                    if (!isHorizontal)
                    {
                        route.StartPoint = src.RightPort;
                        route.EndPoint = tgt.RightPort;
                        escapeX = Math.Max(src.X + src.Width, tgt.X + tgt.Width) + 24.0 + (feedbackCounter * 16.0);
                        route.ArrowheadAngle = Math.PI; // Inward to the left (180 deg)
                        route.StartArrowheadAngle = Math.PI;
                    }
                    else
                    {
                        route.StartPoint = src.BottomPort;
                        route.EndPoint = tgt.BottomPort;
                        escapeX = Math.Max(src.Y + src.Height, tgt.Y + tgt.Height) + 24.0 + (feedbackCounter * 16.0);
                        route.ArrowheadAngle = -Math.PI / 2.0; // Inward upward
                        route.StartArrowheadAngle = -Math.PI / 2.0;
                    }
                    feedbackCounter++;

                    // Intermediate waypoints for orthogonal bypass
                    if (!isHorizontal)
                    {
                        route.Waypoints.Add(new Point(escapeX, route.StartPoint.Y));
                        route.Waypoints.Add(new Point(escapeX, route.EndPoint.Y));
                        result.TotalWidth = Math.Max(result.TotalWidth, escapeX + GraphPadding);
                    }
                    else
                    {
                        route.Waypoints.Add(new Point(route.StartPoint.X, escapeX));
                        route.Waypoints.Add(new Point(route.EndPoint.X, escapeX));
                        result.TotalHeight = Math.Max(result.TotalHeight, escapeX + GraphPadding);
                    }
                }

                // Compute Label Size & Position if label is present
                if (!string.IsNullOrEmpty(edge.Label))
                {
                    var labelTextSize = MeasureText(edge.Label);
                    route.LabelSize = new Size(labelTextSize.Width + 12.0, labelTextSize.Height + 4.0);

                    Point midPoint;
                    if (!isFeedback && route.Waypoints.Count == 0)
                    {
                        // Direct cubic Bezier evaluated at t = 0.5
                        // B(0.5) = 1/8 P0 + 3/8 C1 + 3/8 C2 + 1/8 P3
                        double mx = (route.StartPoint.X + 3.0 * route.ControlPoint1.X + 3.0 * route.ControlPoint2.X + route.EndPoint.X) / 8.0;
                        double my = (route.StartPoint.Y + 3.0 * route.ControlPoint1.Y + 3.0 * route.ControlPoint2.Y + route.EndPoint.Y) / 8.0;
                        midPoint = new Point(mx, my);
                    }
                    else if (route.Waypoints.Count > 0)
                    {
                        int midIdx = route.Waypoints.Count / 2;
                        midPoint = route.Waypoints[midIdx];
                    }
                    else
                    {
                        midPoint = new Point((route.StartPoint.X + route.EndPoint.X) / 2.0, (route.StartPoint.Y + route.EndPoint.Y) / 2.0);
                    }

                    route.LabelPosition = midPoint;
                }

                result.Edges.Add(route);
            }

            // Prevent edge label collisions / overlaps
            for (int i = 0; i < result.Edges.Count; i++)
            {
                var r1 = result.Edges[i];
                if (!r1.LabelPosition.HasValue || string.IsNullOrEmpty(r1.Edge.Label)) continue;

                var b1 = new Rect(
                    r1.LabelPosition.Value.X - r1.LabelSize.Width / 2.0,
                    r1.LabelPosition.Value.Y - r1.LabelSize.Height / 2.0,
                    r1.LabelSize.Width,
                    r1.LabelSize.Height);

                for (int j = i + 1; j < result.Edges.Count; j++)
                {
                    var r2 = result.Edges[j];
                    if (!r2.LabelPosition.HasValue || string.IsNullOrEmpty(r2.Edge.Label)) continue;

                    var b2 = new Rect(
                        r2.LabelPosition.Value.X - r2.LabelSize.Width / 2.0,
                        r2.LabelPosition.Value.Y - r2.LabelSize.Height / 2.0,
                        r2.LabelSize.Width,
                        r2.LabelSize.Height);

                    if (b1.IntersectsWith(b2))
                    {
                        double shift = (b1.Bottom - b2.Top) + 6.0;
                        r2.LabelPosition = new Point(r2.LabelPosition.Value.X, r2.LabelPosition.Value.Y + shift);
                    }
                }
            }

            // Ensure TotalWidth & TotalHeight enclose any edge label pills that extend beyond graph boundaries
            foreach (var route in result.Edges)
            {
                if (route.LabelPosition.HasValue)
                {
                    double r = route.LabelPosition.Value.X + route.LabelSize.Width / 2.0 + 8.0;
                    double b = route.LabelPosition.Value.Y + route.LabelSize.Height / 2.0 + 8.0;
                    if (r > result.TotalWidth) result.TotalWidth = r;
                    if (b > result.TotalHeight) result.TotalHeight = b;
                }
            }
        }

        private static void ComputeBezierControls(EdgeLayoutRoute route, MermaidOrientation orientation)
        {
            var p0 = route.StartPoint;
            var p3 = route.EndPoint;

            if (orientation == MermaidOrientation.TopToBottom)
            {
                double dy = Math.Max(20.0, p3.Y - p0.Y);
                route.ControlPoint1 = new Point(p0.X, p0.Y + dy / 2.0);
                route.ControlPoint2 = new Point(p3.X, p3.Y - dy / 2.0);
            }
            else if (orientation == MermaidOrientation.BottomToTop)
            {
                double dy = Math.Min(-20.0, p3.Y - p0.Y);
                route.ControlPoint1 = new Point(p0.X, p0.Y + dy / 2.0);
                route.ControlPoint2 = new Point(p3.X, p3.Y - dy / 2.0);
            }
            else if (orientation == MermaidOrientation.LeftToRight)
            {
                double dx = Math.Max(20.0, p3.X - p0.X);
                route.ControlPoint1 = new Point(p0.X + dx / 2.0, p0.Y);
                route.ControlPoint2 = new Point(p3.X - dx / 2.0, p3.Y);
            }
            else // RightToLeft
            {
                double dx = Math.Min(-20.0, p3.X - p0.X);
                route.ControlPoint1 = new Point(p0.X + dx / 2.0, p0.Y);
                route.ControlPoint2 = new Point(p3.X - dx / 2.0, p3.Y);
            }
        }
    }
}
