using System;
using System.Collections.Generic;

namespace MDPlus.Core.Mermaid
{
    /// <summary>
    /// Flowchart layout orientation directives.
    /// </summary>
    public enum MermaidOrientation
    {
        TopToBottom,    // TD, TB (Default)
        BottomToTop,    // BT
        LeftToRight,    // LR
        RightToLeft     // RL
    }

    /// <summary>
    /// Supported node boundary shapes.
    /// </summary>
    public enum MermaidNodeShape
    {
        Rectangle,          // [text]  - default process block
        RoundedRectangle,   // (text)  - rounded action
        Stadium,            // ([text]) - pill / start / end capsule
        Diamond,            // {text}  - decision condition
        Circle              // ((text)) - state / junction
    }

    /// <summary>
    /// Connector stroke line appearance.
    /// </summary>
    public enum MermaidStrokeStyle
    {
        Solid,      // --> or ---
        Dotted,     // -.-> or -.-
        Thick       // ==> or ===
    }

    /// <summary>
    /// Connector termination arrowhead type.
    /// </summary>
    public enum MermaidArrowHead
    {
        None,           // Undirected link (---, -.-, ===)
        Arrow,          // Directed forward arrow (--> , -.->, ==>)
        Reverse,        // Directed reverse arrow (<--, <-.-, <==)
        Bidirectional   // Bidirectional arrow (<-->, <-.->, <==>)
    }

    /// <summary>
    /// Represents a single node in the Mermaid diagram AST.
    /// </summary>
    public sealed class MermaidNode
    {
        public string Id { get; }
        public string Text { get; set; }

        /// <summary>
        /// Synonym for Text for label extraction compatibility.
        /// </summary>
        public string Label
        {
            get => Text;
            set => Text = value ?? string.Empty;
        }

        public MermaidNodeShape Shape { get; set; }
        public int SourceLine { get; }

        public MermaidNode(string id, string text, MermaidNodeShape shape = MermaidNodeShape.Rectangle, int sourceLine = 0)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Text = text ?? id;
            Shape = shape;
            SourceLine = sourceLine;
        }

        public override string ToString() => $"{Id}{GetShapePrefix()}{Text}{GetShapeSuffix()}";

        private string GetShapePrefix() => Shape switch
        {
            MermaidNodeShape.Stadium => "([",
            MermaidNodeShape.Circle => "((",
            MermaidNodeShape.RoundedRectangle => "(",
            MermaidNodeShape.Diamond => "{",
            _ => "["
        };

        private string GetShapeSuffix() => Shape switch
        {
            MermaidNodeShape.Stadium => "])",
            MermaidNodeShape.Circle => "))",
            MermaidNodeShape.RoundedRectangle => ")",
            MermaidNodeShape.Diamond => "}",
            _ => "]"
        };
    }

    /// <summary>
    /// Represents a directed or undirected connection between two nodes in the diagram AST.
    /// </summary>
    public sealed class MermaidEdge
    {
        public string SourceId { get; }
        public string TargetId { get; }
        public MermaidStrokeStyle Stroke { get; }
        public MermaidArrowHead Arrow { get; }

        /// <summary>
        /// Property alias for Arrow.
        /// </summary>
        public MermaidArrowHead ArrowHead => Arrow;

        public string? Label { get; }
        public int SourceLine { get; }

        public MermaidEdge(
            string sourceId,
            string targetId,
            MermaidStrokeStyle stroke = MermaidStrokeStyle.Solid,
            MermaidArrowHead arrow = MermaidArrowHead.Arrow,
            string? label = null,
            int sourceLine = 0)
        {
            SourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
            TargetId = targetId ?? throw new ArgumentNullException(nameof(targetId));
            Stroke = stroke;
            Arrow = arrow;
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
            SourceLine = sourceLine;
        }

        public override string ToString()
        {
            string labelPart = Label != null ? $"|{Label}|" : string.Empty;
            string op = Stroke switch
            {
                MermaidStrokeStyle.Dotted => Arrow switch
                {
                    MermaidArrowHead.Arrow => "-.->",
                    MermaidArrowHead.Reverse => "<-.-",
                    MermaidArrowHead.Bidirectional => "<-.->",
                    _ => "-.-"
                },
                MermaidStrokeStyle.Thick => Arrow switch
                {
                    MermaidArrowHead.Arrow => "==>",
                    MermaidArrowHead.Reverse => "<==",
                    MermaidArrowHead.Bidirectional => "<==>",
                    _ => "==="
                },
                _ => Arrow switch
                {
                    MermaidArrowHead.Arrow => "-->",
                    MermaidArrowHead.Reverse => "<--",
                    MermaidArrowHead.Bidirectional => "<-->",
                    _ => "---"
                }
            };
            return $"{SourceId} {op}{labelPart} {TargetId}";
        }
    }

    /// <summary>
    /// Represents an optional logical grouping / container of nodes in the diagram AST.
    /// </summary>
    public sealed class MermaidSubgraph
    {
        public string Id { get; }
        public string Title { get; }
        public List<string> NodeIds { get; } = new List<string>();

        public MermaidSubgraph(string id, string title)
        {
            Id = id ?? string.Empty;
            Title = title ?? id ?? string.Empty;
        }

        public override string ToString() => $"subgraph {Id} [{Title}] ({NodeIds.Count} nodes)";
    }

    /// <summary>
    /// Root AST model representing a fully parsed Mermaid flowchart diagram.
    /// </summary>
    public sealed class MermaidFlowchartGraph
    {
        public MermaidOrientation Orientation { get; set; } = MermaidOrientation.TopToBottom;
        public bool IsFlowchartKeyword { get; set; }
        public string? Title { get; set; }

        /// <summary>
        /// Nodes indexed by ID (case-insensitive for robust matching).
        /// </summary>
        public Dictionary<string, MermaidNode> Nodes { get; } = new Dictionary<string, MermaidNode>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Directed or undirected edges connecting nodes.
        /// </summary>
        public List<MermaidEdge> Edges { get; } = new List<MermaidEdge>();

        /// <summary>
        /// Logical subgraphs defined in the diagram.
        /// </summary>
        public List<MermaidSubgraph> Subgraphs { get; } = new List<MermaidSubgraph>();

        /// <summary>
        /// Raw source text preserved for clipboard, rendering, and serialization round-tripping.
        /// </summary>
        public string RawSource { get; set; } = string.Empty;

        /// <summary>
        /// Property alias for RawSource.
        /// </summary>
        public string RawText
        {
            get => RawSource;
            set => RawSource = value ?? string.Empty;
        }

        /// <summary>
        /// Retrieves an existing node by ID or creates a new node in the graph.
        /// If the node already exists and updated text/shape are supplied, updates the existing instance.
        /// </summary>
        public MermaidNode GetOrCreateNode(string id, string? text = null, MermaidNodeShape? shape = null, int line = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Node ID cannot be null or empty.", nameof(id));
            }

            if (Nodes.TryGetValue(id, out var existing))
            {
                if (text != null)
                {
                    existing.Text = text;
                }
                if (shape.HasValue)
                {
                    existing.Shape = shape.Value;
                }
                return existing;
            }

            var node = new MermaidNode(id, text ?? id, shape ?? MermaidNodeShape.Rectangle, line);
            Nodes[id] = node;
            return node;
        }
    }
}
