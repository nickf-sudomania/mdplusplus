using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MDPlus.Core.Mermaid
{
    /// <summary>
    /// High-performance zero-dependency tokenizer and parser for Mermaid flowcharts.
    /// Parses standard 'graph' and 'flowchart' directives into lightweight AST models.
    /// Safely degrades on unsupported diagram types or syntax errors without throwing exceptions.
    /// </summary>
    public static class MermaidFlowchartParser
    {
        private static readonly HashSet<string> UnsupportedDiagramKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sequenceDiagram",
            "classDiagram",
            "stateDiagram",
            "stateDiagram-v2",
            "erDiagram",
            "journey",
            "gantt",
            "pie",
            "quadrantChart",
            "requirementDiagram",
            "gitGraph",
            "c4Context",
            "mindmap",
            "timeline",
            "sankey-beta",
            "xychart-beta",
            "block-beta",
            "packet-beta",
            "kanban",
            "architecture-beta"
        };

        private static readonly HashSet<string> IgnoredStatementDirectives = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "classDef",
            "class",
            "style",
            "click",
            "linkStyle",
            "accTitle:",
            "accDescr:",
            "accDescr",
            "direction"
        };

        /// <summary>
        /// Attempts to parse Mermaid flowchart source code into a MermaidFlowchartGraph AST.
        /// </summary>
        /// <param name="text">The raw Mermaid diagram text.</param>
        /// <param name="graph">The parsed graph if successful; otherwise null.</param>
        /// <param name="errorMessage">Descriptive error message if parsing fails; otherwise null.</param>
        /// <returns>True if the flowchart was parsed successfully; otherwise false.</returns>
        public static bool TryParse(
            string? text,
            [NotNullWhen(true)] out MermaidFlowchartGraph? graph,
            out string? errorMessage)
        {
            graph = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                errorMessage = "Mermaid code block is empty or whitespace.";
                return false;
            }

            try
            {
                var scanner = new Scanner(text);
                bool success = scanner.Parse(out graph, out errorMessage);
                if (!success)
                {
                    graph = null;
                }
                return success;
            }
            catch (Exception ex)
            {
                graph = null;
                errorMessage = $"Mermaid parsing error: {ex.Message}";
                return false;
            }
        }

        private sealed class Scanner
        {
            private readonly string _source;
            private readonly int _length;
            private int _pos;
            private int _line = 1;
            private readonly Stack<MermaidSubgraph> _subgraphStack = new Stack<MermaidSubgraph>();

            public Scanner(string source)
            {
                _source = source;
                _length = source.Length;
            }

            private char Peek(int offset = 0)
            {
                int index = _pos + offset;
                return index < _length ? _source[index] : '\0';
            }

            private void Advance(int count = 1)
            {
                for (int i = 0; i < count && _pos < _length; i++)
                {
                    if (_source[_pos] == '\n')
                    {
                        _line++;
                    }
                    _pos++;
                }
            }

            private void SkipHorizontalWhitespace()
            {
                while (_pos < _length && (_source[_pos] == ' ' || _source[_pos] == '\t'))
                {
                    _pos++;
                }
            }

            private void SkipLineBreakOrSemicolon()
            {
                while (_pos < _length)
                {
                    char c = _source[_pos];
                    if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == ';')
                    {
                        if (c == '\n') _line++;
                        _pos++;
                    }
                    else if (c == '%' && Peek(1) == '%')
                    {
                        SkipComment();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            private void SkipComment()
            {
                if (Peek() == '%' && Peek(1) == '%')
                {
                    while (_pos < _length && _source[_pos] != '\r' && _source[_pos] != '\n')
                    {
                        _pos++;
                    }
                }
            }

            public bool Parse(out MermaidFlowchartGraph? graph, out string? errorMessage)
            {
                graph = null;
                errorMessage = null;

                // 1. Skip leading comments and whitespace
                SkipLineBreakOrSemicolon();

                if (_pos >= _length)
                {
                    errorMessage = "Mermaid content contains only comments or whitespace.";
                    return false;
                }

                // 2. Parse Diagram Header
                if (!ParseHeader(out var orientation, out bool isFlowchart, out errorMessage))
                {
                    return false;
                }

                var resultGraph = new MermaidFlowchartGraph
                {
                    Orientation = orientation,
                    IsFlowchartKeyword = isFlowchart,
                    RawSource = _source
                };

                // 3. Parse Statements
                while (_pos < _length)
                {
                    SkipHorizontalWhitespace();

                    if (_pos >= _length)
                    {
                        break;
                    }

                    char c = _source[_pos];

                    // Empty statement / newline / semicolon
                    if (c == '\r' || c == '\n' || c == ';')
                    {
                        if (c == '\n') _line++;
                        _pos++;
                        continue;
                    }

                    // Comment
                    if (c == '%' && Peek(1) == '%')
                    {
                        SkipComment();
                        continue;
                    }

                    // Check for stray closing delimiters before attempting to read a statement
                    if (c == ']' || c == ')' || c == '}')
                    {
                        errorMessage = $"Syntax error: unexpected closing delimiter '{c}' at line {_line}.";
                        return false;
                    }

                    // Check for keyword statements: subgraph, end, classDef, etc.
                    int startPos = _pos;
                    string word = ReadWord();

                    if (string.Equals(word, "subgraph", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!ParseSubgraph(resultGraph, out errorMessage))
                        {
                            return false;
                        }
                        continue;
                    }

                    if (string.Equals(word, "end", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_subgraphStack.Count > 0)
                        {
                            _subgraphStack.Pop();
                        }
                        SkipToStatementEnd();
                        continue;
                    }

                    if (IgnoredStatementDirectives.Contains(word))
                    {
                        SkipToStatementEnd();
                        continue;
                    }

                    // Rewind to start of word to parse as node or edge
                    _pos = startPos;

                    if (!ParseNodeOrEdgeChain(resultGraph, out errorMessage))
                    {
                        return false;
                    }
                }

                graph = resultGraph;
                return true;
            }

            private bool ParseHeader(out MermaidOrientation orientation, out bool isFlowchart, out string? errorMessage)
            {
                orientation = MermaidOrientation.TopToBottom;
                isFlowchart = false;
                errorMessage = null;

                int startPos = _pos;
                string keyword = ReadWord();

                if (UnsupportedDiagramKeywords.Contains(keyword))
                {
                    errorMessage = $"Unsupported diagram type: '{keyword}'. MDPlus currently supports Mermaid flowcharts ('graph' or 'flowchart').";
                    return false;
                }

                bool isGraph = string.Equals(keyword, "graph", StringComparison.OrdinalIgnoreCase);
                isFlowchart = string.Equals(keyword, "flowchart", StringComparison.OrdinalIgnoreCase);

                if (!isGraph && !isFlowchart)
                {
                    errorMessage = $"Unrecognized Mermaid diagram header '{keyword}' at line {_line}. Diagram must begin with 'graph' or 'flowchart'.";
                    return false;
                }

                SkipHorizontalWhitespace();

                string dirWord = ReadWord();
                if (string.IsNullOrWhiteSpace(dirWord))
                {
                    errorMessage = $"Missing orientation directive after '{keyword}' at line {_line}. Expected TD, TB, BT, LR, or RL.";
                    return false;
                }

                switch (dirWord.ToUpperInvariant())
                {
                    case "TD":
                    case "TB":
                        orientation = MermaidOrientation.TopToBottom;
                        break;
                    case "BT":
                        orientation = MermaidOrientation.BottomToTop;
                        break;
                    case "LR":
                        orientation = MermaidOrientation.LeftToRight;
                        break;
                    case "RL":
                        orientation = MermaidOrientation.RightToLeft;
                        break;
                    default:
                        errorMessage = $"Invalid orientation directive '{dirWord}' at line {_line}. Expected TD, TB, BT, LR, or RL.";
                        return false;
                }

                // Consume optional trailing whitespace or semicolon on the header line
                SkipHorizontalWhitespace();
                if (_pos < _length && _source[_pos] == ';')
                {
                    _pos++;
                }

                return true;
            }

            private bool ParseSubgraph(MermaidFlowchartGraph graph, out string? errorMessage)
            {
                errorMessage = null;
                SkipHorizontalWhitespace();

                string id = string.Empty;
                string title = string.Empty;

                if (_pos < _length && _source[_pos] == '"')
                {
                    // Quoted title: subgraph "Title Here"
                    var quotedTitle = ReadQuotedString();
                    if (quotedTitle == null)
                    {
                        errorMessage = $"Unclosed quote in subgraph title at line {_line}.";
                        return false;
                    }
                    title = quotedTitle;
                    id = title;
                }
                else
                {
                    // Read identifier or word
                    id = ReadWord();
                    SkipHorizontalWhitespace();

                    // Check for optional bracketed title: subgraph id [Title Here]
                    if (_pos < _length && _source[_pos] == '[')
                    {
                        Advance(1); // skip [
                        title = ReadUntil(']');
                        if (_pos < _length && _source[_pos] == ']')
                        {
                            Advance(1); // skip ]
                        }
                    }
                    else if (!string.IsNullOrEmpty(id))
                    {
                        title = id;
                    }
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    id = $"subgraph_{graph.Subgraphs.Count + 1}";
                    title = id;
                }

                var subgraph = new MermaidSubgraph(id, title);
                graph.Subgraphs.Add(subgraph);
                _subgraphStack.Push(subgraph);

                SkipToStatementEnd();
                return true;
            }

            private bool ParseNodeOrEdgeChain(MermaidFlowchartGraph graph, out string? errorMessage)
            {
                errorMessage = null;

                if (!TryReadNodeDef(graph, out var currentNode, out errorMessage))
                {
                    return false;
                }

                RegisterNodeInActiveSubgraphs(currentNode);

                // Chain loop: parse optional edge operator and subsequent target node(s)
                while (true)
                {
                    SkipHorizontalWhitespace();

                    if (_pos >= _length || _source[_pos] == '\r' || _source[_pos] == '\n' || _source[_pos] == ';')
                    {
                        break;
                    }

                    if (_source[_pos] == '%' && Peek(1) == '%')
                    {
                        SkipComment();
                        break;
                    }

                    int edgeStartLine = _line;
                    if (!TryReadEdgeOperator(out var stroke, out var arrow, out var edgeLabel))
                    {
                        // Check for stray closing delimiters on current line
                        if (_pos < _length && (_source[_pos] == ']' || _source[_pos] == ')' || _source[_pos] == '}'))
                        {
                            errorMessage = $"Syntax error: unexpected closing delimiter '{_source[_pos]}' at line {_line}.";
                            return false;
                        }
                        // No edge operator; statement ends or next statement begins
                        break;
                    }

                    SkipHorizontalWhitespace();

                    if (_pos >= _length || _source[_pos] == '\r' || _source[_pos] == '\n' || _source[_pos] == ';' ||
                        (_source[_pos] == '%' && Peek(1) == '%'))
                    {
                        errorMessage = $"Syntax error: edge operator has no target node at line {edgeStartLine}.";
                        return false;
                    }

                    if (!TryReadNodeDef(graph, out var targetNode, out errorMessage))
                    {
                        return false;
                    }

                    RegisterNodeInActiveSubgraphs(targetNode);

                    // Add edge connecting current node to target
                    var edge = new MermaidEdge(currentNode.Id, targetNode.Id, stroke, arrow, edgeLabel, edgeStartLine);
                    graph.Edges.Add(edge);

                    // For chained edges (e.g. A --> B --> C), target becomes the new source
                    currentNode = targetNode;
                }

                return true;
            }

            private void RegisterNodeInActiveSubgraphs(MermaidNode node)
            {
                if (_subgraphStack.Count > 0)
                {
                    foreach (var sub in _subgraphStack)
                    {
                        if (!sub.NodeIds.Contains(node.Id))
                        {
                            sub.NodeIds.Add(node.Id);
                        }
                    }
                }
            }

            private bool TryReadNodeDef(
                MermaidFlowchartGraph graph,
                [NotNullWhen(true)] out MermaidNode? node,
                out string? errorMessage)
            {
                node = null;
                errorMessage = null;

                SkipHorizontalWhitespace();

                if (_pos >= _length)
                {
                    errorMessage = $"Unexpected end of input when expecting node at line {_line}.";
                    return false;
                }

                int nodeLine = _line;

                // Check for stray closing delimiters
                if (_pos < _length && (_source[_pos] == ']' || _source[_pos] == ')' || _source[_pos] == '}'))
                {
                    errorMessage = $"Syntax error: unexpected closing delimiter '{_source[_pos]}' at line {_line}.";
                    return false;
                }

                string nodeId = ReadNodeId();

                if (string.IsNullOrEmpty(nodeId))
                {
                    if (_pos < _length && (_source[_pos] == ']' || _source[_pos] == ')' || _source[_pos] == '}'))
                    {
                        errorMessage = $"Syntax error: unexpected closing delimiter '{_source[_pos]}' at line {_line}.";
                        return false;
                    }
                    errorMessage = $"Expected node identifier at line {_line}.";
                    return false;
                }

                // Check for optional shape specification immediately following or after whitespace
                SkipHorizontalWhitespace();

                MermaidNodeShape? shape = null;
                string? text = null;

                if (_pos < _length)
                {
                    char c = _source[_pos];

                    if (c == '(')
                    {
                        if (Peek(1) == '[')
                        {
                            // Stadium: ([ text ])
                            Advance(2);
                            if (!ReadShapeContent("])", out text, out errorMessage))
                            {
                                return false;
                            }
                            shape = MermaidNodeShape.Stadium;
                        }
                        else if (Peek(1) == '(')
                        {
                            // Circle: (( text ))
                            Advance(2);
                            if (!ReadShapeContent("))", out text, out errorMessage))
                            {
                                return false;
                            }
                            shape = MermaidNodeShape.Circle;
                        }
                        else
                        {
                            // Rounded Rectangle: ( text )
                            Advance(1);
                            if (!ReadShapeContent(")", out text, out errorMessage))
                            {
                                return false;
                            }
                            shape = MermaidNodeShape.RoundedRectangle;
                        }
                    }
                    else if (c == '[')
                    {
                        // Rectangle: [ text ]
                        Advance(1);
                        if (!ReadShapeContent("]", out text, out errorMessage))
                        {
                            return false;
                        }
                        shape = MermaidNodeShape.Rectangle;
                    }
                    else if (c == '{')
                    {
                        // Diamond: { text }
                        Advance(1);
                        if (!ReadShapeContent("}", out text, out errorMessage))
                        {
                            return false;
                        }
                        shape = MermaidNodeShape.Diamond;
                    }
                }

                node = graph.GetOrCreateNode(nodeId, text, shape, nodeLine);
                return true;
            }

            private bool ReadShapeContent(string closingDelimiter, out string content, out string? errorMessage)
            {
                content = string.Empty;
                errorMessage = null;
                SkipHorizontalWhitespace();

                var sb = new StringBuilder();

                // If content begins with double quotes, read quoted string
                if (_pos < _length && _source[_pos] == '"')
                {
                    Advance(1); // skip opening "
                    bool quoteClosed = false;

                    while (_pos < _length)
                    {
                        char c = _source[_pos];

                        if (c == '\r' || c == '\n')
                        {
                            errorMessage = $"Unclosed quote inside node shape label at line {_line}.";
                            return false;
                        }

                        if (c == '\\' && _pos + 1 < _length)
                        {
                            char next = _source[_pos + 1];
                            if (next == '"' || next == '\\')
                            {
                                sb.Append(next);
                                Advance(2);
                                continue;
                            }
                            if (next == 'n')
                            {
                                sb.Append('\n');
                                Advance(2);
                                continue;
                            }
                        }

                        if (c == '"')
                        {
                            Advance(1); // skip closing "
                            quoteClosed = true;
                            break;
                        }

                        sb.Append(c);
                        Advance(1);
                    }

                    if (!quoteClosed)
                    {
                        errorMessage = $"Unclosed quote inside node shape label at line {_line}.";
                        return false;
                    }

                    // After quoted string, skip whitespace to closing delimiter
                    SkipHorizontalWhitespace();

                    if (!MatchAndConsume(closingDelimiter))
                    {
                        errorMessage = $"Expected closing delimiter '{closingDelimiter}' after quoted node label at line {_line}.";
                        return false;
                    }

                    content = CleanLabelText(sb.ToString());
                    return true;
                }

                // Unquoted content: read until closing delimiter sequence
                int delimLen = closingDelimiter.Length;
                bool foundDelimiter = false;

                while (_pos < _length)
                {
                    // Check if closing delimiter matches here
                    if (MatchesAtCurrent(closingDelimiter))
                    {
                        Advance(delimLen);
                        foundDelimiter = true;
                        break;
                    }

                    char c = _source[_pos];
                    if (c == '\r' || c == '\n')
                    {
                        errorMessage = $"Unclosed bracket: expected '{closingDelimiter}' before end of line at line {_line}.";
                        return false;
                    }

                    sb.Append(c);
                    Advance(1);
                }

                if (!foundDelimiter)
                {
                    errorMessage = $"Unclosed bracket: expected '{closingDelimiter}' at line {_line}.";
                    return false;
                }

                content = CleanLabelText(sb.ToString());
                return true;
            }

            private bool TryReadEdgeOperator(
                out MermaidStrokeStyle stroke,
                out MermaidArrowHead arrow,
                out string? label)
            {
                stroke = MermaidStrokeStyle.Solid;
                arrow = MermaidArrowHead.Arrow;
                label = null;

                SkipHorizontalWhitespace();

                if (_pos >= _length)
                {
                    return false;
                }

                // 1. Thick connector starting with '=='
                if (Peek() == '=' && Peek(1) == '=')
                {
                    if (Peek(2) == '>')
                    {
                        // ==>
                        Advance(3);
                        stroke = MermaidStrokeStyle.Thick;
                        arrow = MermaidArrowHead.Arrow;
                        label = ReadOptionalPipeLabel();
                        return true;
                    }
                    if (Peek(2) == '=')
                    {
                        // ===
                        Advance(3);
                        stroke = MermaidStrokeStyle.Thick;
                        arrow = MermaidArrowHead.None;
                        label = ReadOptionalPipeLabel();
                        return true;
                    }

                    // Check for inline thick label: == label ==> or == label ===
                    if (TryReadInlineConnector("==", "==>", out label))
                    {
                        stroke = MermaidStrokeStyle.Thick;
                        arrow = MermaidArrowHead.Arrow;
                        return true;
                    }
                    if (TryReadInlineConnector("==", "===", out label))
                    {
                        stroke = MermaidStrokeStyle.Thick;
                        arrow = MermaidArrowHead.None;
                        return true;
                    }

                    return false;
                }

                // 2. Dotted connector starting with '-.'
                if (Peek() == '-' && Peek(1) == '.')
                {
                    if (Peek(2) == '-' && Peek(3) == '>')
                    {
                        // -.->
                        Advance(4);
                        stroke = MermaidStrokeStyle.Dotted;
                        arrow = MermaidArrowHead.Arrow;
                        label = ReadOptionalPipeLabel();
                        return true;
                    }
                    if (Peek(2) == '-')
                    {
                        // -.-
                        Advance(3);
                        stroke = MermaidStrokeStyle.Dotted;
                        arrow = MermaidArrowHead.None;
                        label = ReadOptionalPipeLabel();
                        return true;
                    }

                    // Check for inline dotted label: -. label .->
                    if (TryReadInlineConnector("-.", ".->", out label))
                    {
                        stroke = MermaidStrokeStyle.Dotted;
                        arrow = MermaidArrowHead.Arrow;
                        return true;
                    }

                    return false;
                }

                // 3. Solid connector starting with '--'
                if (Peek() == '-' && Peek(1) == '-')
                {
                    if (Peek(2) == '>')
                    {
                        // -->
                        Advance(3);
                        stroke = MermaidStrokeStyle.Solid;
                        arrow = MermaidArrowHead.Arrow;
                        label = ReadOptionalPipeLabel();
                        return true;
                    }
                    if (Peek(2) == '-')
                    {
                        // ---
                        Advance(3);
                        stroke = MermaidStrokeStyle.Solid;
                        arrow = MermaidArrowHead.None;
                        label = ReadOptionalPipeLabel();
                        return true;
                    }

                    // Check for inline solid label: -- label --> or -- label ---
                    if (TryReadInlineConnector("--", "-->", out label))
                    {
                        stroke = MermaidStrokeStyle.Solid;
                        arrow = MermaidArrowHead.Arrow;
                        return true;
                    }
                    if (TryReadInlineConnector("--", "---", out label))
                    {
                        stroke = MermaidStrokeStyle.Solid;
                        arrow = MermaidArrowHead.None;
                        return true;
                    }

                    return false;
                }

                return false;
            }

            private string? ReadOptionalPipeLabel()
            {
                int savedPos = _pos;
                SkipHorizontalWhitespace();

                if (_pos < _length && _source[_pos] == '|')
                {
                    Advance(1); // skip opening |
                    var sb = new StringBuilder();

                    while (_pos < _length)
                    {
                        char c = _source[_pos];
                        if (c == '|')
                        {
                            Advance(1); // skip closing |
                            return CleanLabelText(sb.ToString());
                        }
                        if (c == '\r' || c == '\n' || c == ';')
                        {
                            // Missing closing pipe, rewind and ignore
                            _pos = savedPos;
                            return null;
                        }

                        sb.Append(c);
                        Advance(1);
                    }

                    _pos = savedPos;
                    return null;
                }

                _pos = savedPos;
                return null;
            }

            private bool TryReadInlineConnector(string openPrefix, string closeSuffix, out string? label)
            {
                label = null;
                if (!MatchesAtCurrent(openPrefix))
                {
                    return false;
                }

                int searchStart = _pos + openPrefix.Length;
                int lineEnd = _source.IndexOfAny(new[] { '\r', '\n', ';' }, searchStart);
                if (lineEnd < 0) lineEnd = _length;

                int closeIndex = _source.IndexOf(closeSuffix, searchStart, lineEnd - searchStart, StringComparison.Ordinal);
                if (closeIndex < 0)
                {
                    return false;
                }

                string rawLabel = _source.Substring(searchStart, closeIndex - searchStart);
                _pos = closeIndex + closeSuffix.Length;
                label = CleanLabelText(rawLabel);
                return true;
            }

            private string ReadNodeId()
            {
                SkipHorizontalWhitespace();

                if (_pos >= _length)
                {
                    return string.Empty;
                }

                if (_source[_pos] == '"')
                {
                    return ReadQuotedString() ?? string.Empty;
                }

                var sb = new StringBuilder();
                while (_pos < _length)
                {
                    char c = _source[_pos];

                    // Check for shape openers or closers
                    if (c == '[' || c == '(' || c == '{' || c == ']' || c == ')' || c == '}')
                    {
                        break;
                    }

                    // Check for edge openers
                    if (c == '-')
                    {
                        if (Peek(1) == '-' || Peek(1) == '.')
                        {
                            break;
                        }
                    }
                    if (c == '=' && Peek(1) == '=')
                    {
                        break;
                    }

                    // Whitespace or statement delimiters
                    if (char.IsWhiteSpace(c) || c == ';' || c == '|')
                    {
                        break;
                    }

                    sb.Append(c);
                    Advance(1);
                }

                return sb.ToString().Trim();
            }

            private string? ReadQuotedString()
            {
                if (_pos >= _length || _source[_pos] != '"')
                {
                    return string.Empty;
                }

                Advance(1); // skip "
                var sb = new StringBuilder();
                bool closed = false;

                while (_pos < _length)
                {
                    char c = _source[_pos];
                    if (c == '\r' || c == '\n')
                    {
                        return null;
                    }

                    if (c == '\\' && _pos + 1 < _length)
                    {
                        char next = _source[_pos + 1];
                        if (next == '"' || next == '\\')
                        {
                            sb.Append(next);
                            Advance(2);
                            continue;
                        }
                    }

                    if (c == '"')
                    {
                        Advance(1); // skip "
                        closed = true;
                        break;
                    }

                    sb.Append(c);
                    Advance(1);
                }

                return closed ? sb.ToString() : null;
            }

            private string ReadWord()
            {
                SkipHorizontalWhitespace();
                var sb = new StringBuilder();

                while (_pos < _length)
                {
                    char c = _source[_pos];
                    if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == ':')
                    {
                        sb.Append(c);
                        Advance(1);
                    }
                    else
                    {
                        break;
                    }
                }

                return sb.ToString();
            }

            private string ReadUntil(char stopChar)
            {
                var sb = new StringBuilder();
                while (_pos < _length && _source[_pos] != stopChar && _source[_pos] != '\r' && _source[_pos] != '\n')
                {
                    sb.Append(_source[_pos]);
                    Advance(1);
                }
                return sb.ToString().Trim();
            }

            private bool MatchesAtCurrent(string pattern)
            {
                if (_pos + pattern.Length > _length) return false;
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (_source[_pos + i] != pattern[i]) return false;
                }
                return true;
            }

            private bool MatchAndConsume(string pattern)
            {
                if (MatchesAtCurrent(pattern))
                {
                    Advance(pattern.Length);
                    return true;
                }
                return false;
            }

            private void SkipToStatementEnd()
            {
                while (_pos < _length)
                {
                    char c = _source[_pos];
                    if (c == ';' || c == '\n')
                    {
                        if (c == '\n') _line++;
                        _pos++;
                        break;
                    }
                    if (c == '\r')
                    {
                        _pos++;
                        if (_pos < _length && _source[_pos] == '\n')
                        {
                            _line++;
                            _pos++;
                        }
                        break;
                    }
                    _pos++;
                }
            }

            private static string CleanLabelText(string raw)
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return string.Empty;
                }

                string trimmed = raw.Trim();

                // Strip outer quotes if still wrapped
                if (trimmed.Length >= 2 && trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
                {
                    trimmed = trimmed.Substring(1, trimmed.Length - 2).Trim();
                }

                // Normalize HTML line breaks
                trimmed = trimmed.Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
                                 .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
                                 .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase);

                return trimmed;
            }
        }
    }
}
