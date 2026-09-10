using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MDPlus.Core
{
    public class MathTag
    {
        public string Expression { get; set; } = string.Empty;
        public bool IsDisplay { get; set; } = false;
    }

    /// <summary>
    /// High-performance, zero-overhead vector LaTeX math renderer for WPF.
    /// Converts LaTeX formulas ($...$ and $$...$$) into crisp native vector WPF elements.
    /// Supports fractions, radicals, subscripts, superscripts, integrals, summations,
    /// Greek symbols, blackboard bold, operators, matrices, and functions.
    /// </summary>
    public static class LatexMathRenderer
    {
        private static readonly FontFamily MathFont = new FontFamily("Cambria Math, Segoe UI Symbol, Times New Roman, serif");
        private static readonly FontFamily TextFont = new FontFamily("Segoe UI Variable Text, Segoe UI, sans-serif");
        private static readonly FontFamily FunctionFont = new FontFamily("Segoe UI, Arial, sans-serif");

        private static readonly Dictionary<string, string> Symbols = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Greek Lowercase
            { "alpha", "α" }, { "beta", "β" }, { "gamma", "γ" }, { "delta", "δ" },
            { "epsilon", "ε" }, { "varepsilon", "ε" }, { "zeta", "ζ" }, { "eta", "η" },
            { "theta", "θ" }, { "vartheta", "ϑ" }, { "iota", "ι" }, { "kappa", "κ" },
            { "lambda", "λ" }, { "mu", "μ" }, { "nu", "ν" }, { "xi", "ξ" },
            { "pi", "π" }, { "varpi", "ϖ" }, { "rho", "ρ" }, { "varrho", "ϱ" },
            { "sigma", "σ" }, { "varsigma", "ς" }, { "tau", "τ" }, { "upsilon", "υ" },
            { "phi", "φ" }, { "varphi", "ϕ" }, { "chi", "χ" }, { "psi", "ψ" }, { "omega", "ω" },

            // Greek Uppercase
            { "Gamma", "Γ" }, { "Delta", "Δ" }, { "Theta", "Θ" }, { "Lambda", "Λ" },
            { "Xi", "Ξ" }, { "Pi", "Π" }, { "Sigma", "Σ" }, { "Upsilon", "Υ" },
            { "Phi", "Φ" }, { "Psi", "Ψ" }, { "Omega", "Ω" },

            // Binary Operators & Relations
            { "pm", "±" }, { "mp", "∓" }, { "times", "×" }, { "div", "÷" },
            { "cdot", "·" }, { "circ", "∘" }, { "bullet", "•" }, { "ast", "∗" }, { "star", "⋆" },
            { "leq", "≤" }, { "le", "≤" }, { "geq", "≥" }, { "ge", "≥" },
            { "neq", "≠" }, { "ne", "≠" }, { "approx", "≈" }, { "equiv", "≡" },
            { "sim", "∼" }, { "simeq", "≃" }, { "cong", "≅" }, { "propto", "∝" },
            { "ll", "≪" }, { "gg", "≫" },
            { "angle", "∠" }, { "perp", "⊥" }, { "parallel", "∥" }, { "mid", "|" },
            { "prime", "′" }, { "dagger", "†" }, { "ddagger", "‡" }, { "aleph", "ℵ" },
            { "oplus", "⊕" }, { "otimes", "⊗" }, { "odot", "⊙" },

            // Sets & Logic
            { "in", "∈" }, { "notin", "∉" }, { "ni", "∋" },
            { "subset", "⊂" }, { "supset", "⊃" }, { "subseteq", "⊆" }, { "supseteq", "⊇" },
            { "cup", "∪" }, { "cap", "∩" }, { "setminus", "∖" }, { "emptyset", "∅" },
            { "forall", "∀" }, { "exists", "∃" }, { "nexists", "∄" },
            { "neg", "¬" }, { "land", "∧" }, { "lor", "∨" },

            // Ellipses & Dots
            { "dots", "…" }, { "ldots", "…" }, { "cdots", "⋯" }, { "vdots", "⋮" }, { "ddots", "⋱" },

            // Calculus & Analysis
            { "infty", "∞" }, { "partial", "∂" }, { "nabla", "∇" },
            { "hbar", "ℏ" }, { "ell", "ℓ" },

            // Arrows
            { "to", "→" }, { "rightarrow", "→" }, { "leftarrow", "←" },
            { "Rightarrow", "⇒" }, { "Leftarrow", "⇐" }, { "Leftrightarrow", "⇔" },
            { "iff", "⟺" }, { "implies", "⟹" }, { "leftrightarrow", "↔" }, { "gets", "←" },
            { "mapsto", "↦" }, { "uparrow", "↑" }, { "downarrow", "↓" },

            // Delimiters
            { "langle", "⟨" }, { "rangle", "⟩" },
            { "{", "{" }, { "}", "}" },

            // Blackboard Bold Shortcuts
            { "Re", "ℜ" }, { "Im", "ℑ" },
        };

        private static readonly HashSet<string> Functions = new HashSet<string>(StringComparer.Ordinal)
        {
            "sin", "cos", "tan", "sec", "csc", "cot",
            "arcsin", "arccos", "arctan", "sinh", "cosh", "tanh",
            "ln", "log", "exp", "lim", "max", "min", "sup", "inf",
            "det", "gcd", "dim", "ker", "deg", "arg"
        };

        private static readonly Dictionary<string, string> BlackboardBold = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "R", "ℝ" }, { "N", "ℕ" }, { "Z", "ℤ" }, { "Q", "ℚ" },
            { "C", "ℂ" }, { "P", "ℙ" }, { "H", "ℍ" }, { "E", "𝔼" }
        };

        public static UIElement RenderMath(string latex, ThemePalette palette, double fontSize = 15, bool isDisplay = false)
        {
            if (string.IsNullOrWhiteSpace(latex))
            {
                return new TextBlock();
            }

            var fgBrush = palette?.EditorFg ?? Brushes.White;
            double effectiveFontSize = isDisplay ? fontSize * 1.15 : fontSize;

            UIElement mathContent;
            try
            {
                var parser = new MathLexer(latex.Trim());
                mathContent = parser.ParseExpression(fgBrush, effectiveFontSize, isDisplay);
            }
            catch
            {
                // Fallback graceful rendering
                var fallbackTb = new TextBlock
                {
                    Text = latex,
                    Foreground = fgBrush,
                    FontSize = effectiveFontSize,
                    FontFamily = MathFont,
                    FontStyle = FontStyles.Italic,
                    VerticalAlignment = VerticalAlignment.Center
                };
                mathContent = fallbackTb;
            }

            if (isDisplay)
            {
                var border = new Border
                {
                    Background = palette != null && palette.IsDark
                        ? new SolidColorBrush(Color.FromArgb(20, 255, 255, 255))
                        : new SolidColorBrush(Color.FromArgb(15, 0, 0, 0)),
                    BorderBrush = palette?.Border ?? Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(16, 10, 16, 10),
                    Margin = new Thickness(0, 10, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    ToolTip = "$$\n" + latex + "\n$$",
                    Child = mathContent
                };
                return border;
            }
            else
            {
                var container = new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(1, 0, 1, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    ToolTip = "$" + latex + "$",
                    Child = mathContent
                };
                return container;
            }
        }

        private sealed class MathLexer
        {
            private readonly string _src;
            private int _pos;
            private readonly int _len;

            public MathLexer(string src)
            {
                _src = src;
                _pos = 0;
                _len = src.Length;
            }

            private char Peek() => _pos < _len ? _src[_pos] : '\0';
            private char Read() => _pos < _len ? _src[_pos++] : '\0';
            private bool IsEof => _pos >= _len;

            private void SkipWhitespace()
            {
                while (_pos < _len && char.IsWhiteSpace(_src[_pos])) _pos++;
            }

            public UIElement ParseExpression(Brush fg, double fontSize, bool isDisplay)
            {
                var panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                while (!IsEof)
                {
                    SkipWhitespace();
                    if (IsEof) break;

                    char c = Peek();
                    if (c == '}' || c == '&' || c == '\\' && PeekNextIsCommand("end"))
                    {
                        break;
                    }

                    var item = ParseItem(fg, fontSize, isDisplay);
                    if (item != null)
                    {
                        panel.Children.Add(item);
                    }
                }

                if (panel.Children.Count == 1)
                {
                    var single = panel.Children[0];
                    panel.Children.Clear();
                    return single;
                }

                return panel;
            }

            private bool PeekNextIsCommand(string cmd)
            {
                if (_pos + 1 + cmd.Length <= _len && _src[_pos] == '\\')
                {
                    return string.CompareOrdinal(_src, _pos + 1, cmd, 0, cmd.Length) == 0;
                }
                return false;
            }

            private UIElement? ParseItem(Brush fg, double fontSize, bool isDisplay)
            {
                UIElement? baseElement = ParseBase(fg, fontSize, isDisplay);
                if (baseElement == null) return null;

                // Check for subscripts and superscripts: base_{sub}^{sup} or base^{sup}_{sub}
                UIElement? sup = null;
                UIElement? sub = null;

                while (!IsEof)
                {
                    SkipWhitespace();
                    if (Peek() == '^')
                    {
                        Read(); // consume '^'
                        sup = ParseScriptGroup(fg, fontSize * 0.72, isDisplay);
                    }
                    else if (Peek() == '_')
                    {
                        Read(); // consume '_'
                        sub = ParseScriptGroup(fg, fontSize * 0.72, isDisplay);
                    }
                    else
                    {
                        break;
                    }
                }

                if (sup != null || sub != null)
                {
                    return CreateScriptedElement(baseElement, sup, sub, fontSize);
                }

                return baseElement;
            }

            private UIElement? ParseScriptGroup(Brush fg, double fontSize, bool isDisplay)
            {
                SkipWhitespace();
                if (IsEof) return null;

                if (Peek() == '{')
                {
                    Read(); // consume '{'
                    var expr = ParseExpression(fg, fontSize, isDisplay);
                    if (Peek() == '}') Read(); // consume '}'
                    return expr;
                }

                // Single char or symbol
                if (Peek() == '\\')
                {
                    return ParseCommand(fg, fontSize, isDisplay);
                }

                char c = Read();
                return CreateGlyph(c.ToString(), fg, fontSize, char.IsLetter(c));
            }

            private UIElement? ParseBase(Brush fg, double fontSize, bool isDisplay)
            {
                SkipWhitespace();
                if (IsEof) return null;

                char c = Peek();

                // 1. Group in braces: { ... }
                if (c == '{')
                {
                    Read();
                    var expr = ParseExpression(fg, fontSize, isDisplay);
                    if (Peek() == '}') Read();
                    return expr;
                }

                // 2. Commands starting with '\'
                if (c == '\\')
                {
                    return ParseCommand(fg, fontSize, isDisplay);
                }

                // 3. Numbers
                if (char.IsDigit(c))
                {
                    var sb = new StringBuilder();
                    while (!IsEof && (char.IsDigit(Peek()) || Peek() == '.'))
                    {
                        sb.Append(Read());
                    }
                    return CreateGlyph(sb.ToString(), fg, fontSize, isItalic: false);
                }

                // 4. Letters (Math Variables)
                if (char.IsLetter(c))
                {
                    char letter = Read();
                    return CreateGlyph(letter.ToString(), fg, fontSize, isItalic: true);
                }

                // 5. Operators & Delimiters
                Read();
                string op = c.ToString();
                switch (c)
                {
                    case '\'':
                        return CreateGlyph("′", fg, fontSize, isItalic: false);
                    case '+':
                    case '-':
                    case '=':
                    case '<':
                    case '>':
                        return CreateGlyph($" {op} ", fg, fontSize, isItalic: false);
                    case '*':
                        return CreateGlyph(" · ", fg, fontSize, isItalic: false);
                    default:
                        return CreateGlyph(op, fg, fontSize, isItalic: false);
                }
            }

            private UIElement? ParseCommand(Brush fg, double fontSize, bool isDisplay)
            {
                Read(); // consume '\'
                if (IsEof) return null;

                // Special single-character escapes
                char nextChar = Peek();
                if (nextChar == '{' || nextChar == '}' || nextChar == '$' || nextChar == '%' || nextChar == '_' || nextChar == '&' || nextChar == '#')
                {
                    Read();
                    return CreateGlyph(nextChar.ToString(), fg, fontSize, isItalic: false);
                }
                if (nextChar == ',')
                {
                    Read();
                    return new Border { Width = fontSize * 0.2 };
                }
                if (nextChar == ' ' || nextChar == ';')
                {
                    Read();
                    return new Border { Width = fontSize * 0.35 };
                }
                if (nextChar == '!')
                {
                    Read();
                    return new Border { Width = 0 };
                }
                if (nextChar == '\\')
                {
                    Read();
                    return new Border { Width = fontSize * 0.4 };
                }

                // Command word
                var cmdSb = new StringBuilder();
                while (!IsEof && char.IsLetter(Peek()))
                {
                    cmdSb.Append(Read());
                }
                string cmd = cmdSb.ToString();

                // 1. Fractions: \frac{num}{den}
                if (cmd == "frac" || cmd == "dfrac" || cmd == "tfrac")
                {
                    var num = ParseArgument(fg, fontSize * 0.85, isDisplay);
                    var den = ParseArgument(fg, fontSize * 0.85, isDisplay);
                    return CreateFraction(num, den, fg, fontSize);
                }

                // 2. Radicals: \sqrt{arg} or \sqrt[n]{arg}
                if (cmd == "sqrt")
                {
                    UIElement? rootDegree = null;
                    SkipWhitespace();
                    if (Peek() == '[')
                    {
                        Read();
                        rootDegree = ParseUntil(']', fg, fontSize * 0.65, isDisplay);
                        if (Peek() == ']') Read();
                    }
                    var radicand = ParseArgument(fg, fontSize, isDisplay);
                    return CreateRadical(radicand, rootDegree, fg, fontSize);
                }

                // 3. Large Operators: \int, \sum, \prod, etc.
                if (cmd == "int" || cmd == "iint" || cmd == "iiint" || cmd == "oint" ||
                    cmd == "sum" || cmd == "prod" || cmd == "coprod")
                {
                    return CreateLargeOperator(cmd, fg, fontSize, isDisplay);
                }

                // 4. Blackboard Bold: \mathbb{R}
                if (cmd == "mathbb")
                {
                    string letter = ReadArgumentText().Trim();
                    if (BlackboardBold.TryGetValue(letter, out string? bb))
                    {
                        return CreateGlyph(bb, fg, fontSize, isItalic: false);
                    }
                    return CreateGlyph(letter, fg, fontSize, isItalic: false);
                }

                // 5. Text and Font Modifiers: \text{...}, \mathrm{...}, \mathbf{...}
                if (cmd == "text" || cmd == "mathrm" || cmd == "mathbf" || cmd == "mathit" || cmd == "textbf")
                {
                    string inner = ReadArgumentText();
                    bool bold = cmd == "mathbf" || cmd == "textbf";
                    bool italic = cmd == "mathit";
                    var tb = new TextBlock
                    {
                        Text = inner,
                        Foreground = fg,
                        FontSize = fontSize,
                        FontFamily = cmd == "text" ? TextFont : MathFont,
                        FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                        FontStyle = italic ? FontStyles.Italic : FontStyles.Normal,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    return tb;
                }

                // 6. Spacing Commands
                if (cmd == "quad") return new Border { Width = fontSize * 0.8 };
                if (cmd == "qquad") return new Border { Width = fontSize * 1.6 };

                // 7. Accents: \hat{x}, \vec{x}, \bar{x}, \dot{x}, \ddot{x}, \tilde{x}
                if (cmd == "hat" || cmd == "vec" || cmd == "bar" || cmd == "dot" || cmd == "ddot" || cmd == "tilde")
                {
                    var baseArg = ParseArgument(fg, fontSize, isDisplay);
                    return CreateAccent(baseArg, cmd, fg, fontSize);
                }

                // 8. Delimiter keywords: \left and \right
                if (cmd == "left")
                {
                    SkipWhitespace();
                    char delim = Read();
                    return CreateGlyph(delim.ToString(), fg, fontSize * 1.2, isItalic: false);
                }
                if (cmd == "right")
                {
                    SkipWhitespace();
                    char delim = Read();
                    return CreateGlyph(delim.ToString(), fg, fontSize * 1.2, isItalic: false);
                }

                // 9. Matrix environments: \begin{matrix}, \begin{pmatrix}, \begin{cases}
                if (cmd == "begin")
                {
                    string env = ReadArgumentText().Trim();
                    return ParseEnvironment(env, fg, fontSize, isDisplay);
                }

                // 10. Math functions: \sin, \cos, \log, etc.
                if (Functions.Contains(cmd))
                {
                    return new TextBlock
                    {
                        Text = cmd + " ",
                        Foreground = fg,
                        FontSize = fontSize,
                        FontFamily = FunctionFont,
                        FontStyle = FontStyles.Normal,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }

                // 11. Known Symbols & Greek Letters
                if (Symbols.TryGetValue(cmd, out string? symbolGlyph))
                {
                    return CreateGlyph(symbolGlyph, fg, fontSize, isItalic: false);
                }

                // Fallback: render command name
                return CreateGlyph(cmd, fg, fontSize, isItalic: false);
            }

            private UIElement ParseArgument(Brush fg, double fontSize, bool isDisplay)
            {
                SkipWhitespace();
                if (IsEof) return new TextBlock();

                if (Peek() == '{')
                {
                    Read();
                    var expr = ParseExpression(fg, fontSize, isDisplay);
                    if (Peek() == '}') Read();
                    return expr;
                }

                // Single token
                return ParseItem(fg, fontSize, isDisplay) ?? new TextBlock();
            }

            private string ReadArgumentText()
            {
                SkipWhitespace();
                if (IsEof) return string.Empty;

                if (Peek() == '{')
                {
                    Read();
                    int start = _pos;
                    int depth = 1;
                    while (!IsEof && depth > 0)
                    {
                        char c = Read();
                        if (c == '{') depth++;
                        else if (c == '}') depth--;
                    }
                    int end = depth == 0 ? _pos - 1 : _pos;
                    return end > start ? _src.Substring(start, end - start) : string.Empty;
                }

                return Read().ToString();
            }

            private UIElement ParseUntil(char stopChar, Brush fg, double fontSize, bool isDisplay)
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                while (!IsEof && Peek() != stopChar)
                {
                    var item = ParseItem(fg, fontSize, isDisplay);
                    if (item != null) panel.Children.Add(item);
                }
                return panel;
            }

            private UIElement ParseEnvironment(string env, Brush fg, double fontSize, bool isDisplay)
            {
                string endCmd = "\\end{" + env + "}";
                int endIdx = _src.IndexOf(endCmd, _pos, StringComparison.Ordinal);
                string envContent = endIdx >= _pos ? _src.Substring(_pos, endIdx - _pos) : _src.Substring(_pos);
                _pos = endIdx >= 0 ? endIdx + endCmd.Length : _len;

                var grid = new Grid { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 2, 4, 2) };

                string[] rows = envContent.Split(new[] { "\\\\" }, StringSplitOptions.None);
                int maxCols = 1;

                var rowElements = new List<List<UIElement>>();
                foreach (var rowStr in rows)
                {
                    var colElements = new List<UIElement>();
                    string[] cols = rowStr.Split('&');
                    if (cols.Length > maxCols) maxCols = cols.Length;

                    foreach (var cellStr in cols)
                    {
                        var cellLexer = new MathLexer(cellStr.Trim());
                        colElements.Add(cellLexer.ParseExpression(fg, fontSize * 0.9, isDisplay));
                    }
                    rowElements.Add(colElements);
                }

                for (int r = 0; r < rowElements.Count; r++)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                for (int c = 0; c < maxCols; c++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }

                for (int r = 0; r < rowElements.Count; r++)
                {
                    for (int c = 0; c < rowElements[r].Count; c++)
                    {
                        var cell = rowElements[r][c];
                        var border = new Border
                        {
                            Padding = new Thickness(6, 2, 6, 2),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Child = cell
                        };
                        Grid.SetRow(border, r);
                        Grid.SetColumn(border, c);
                        grid.Children.Add(border);
                    }
                }

                // Wrap in parentheses / brackets if needed
                string leftDelim = env == "pmatrix" ? "(" : env == "bmatrix" ? "[" : env == "cases" ? "{" : "";
                string rightDelim = env == "pmatrix" ? ")" : env == "bmatrix" ? "]" : env == "cases" ? "" : "";

                if (!string.IsNullOrEmpty(leftDelim) || !string.IsNullOrEmpty(rightDelim))
                {
                    var sp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                    if (!string.IsNullOrEmpty(leftDelim))
                    {
                        sp.Children.Add(new TextBlock
                        {
                            Text = leftDelim,
                            Foreground = fg,
                            FontSize = fontSize * 1.5,
                            FontFamily = MathFont,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                    }
                    sp.Children.Add(grid);
                    if (!string.IsNullOrEmpty(rightDelim))
                    {
                        sp.Children.Add(new TextBlock
                        {
                            Text = rightDelim,
                            Foreground = fg,
                            FontSize = fontSize * 1.5,
                            FontFamily = MathFont,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                    }
                    return sp;
                }

                return grid;
            }

            private static TextBlock CreateGlyph(string text, Brush fg, double fontSize, bool isItalic)
            {
                return new TextBlock
                {
                    Text = text,
                    Foreground = fg,
                    FontSize = fontSize,
                    FontFamily = MathFont,
                    FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            private static UIElement CreateFraction(UIElement num, UIElement den, Brush fg, double fontSize)
            {
                var grid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 2, 0)
                };
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var numBorder = new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(1, 0, 1, 1),
                    Child = num
                };
                Grid.SetRow(numBorder, 0);
                grid.Children.Add(numBorder);

                var line = new Rectangle
                {
                    Height = Math.Max(1.2, fontSize * 0.08),
                    Fill = fg,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 1, 0, 1)
                };
                Grid.SetRow(line, 1);
                grid.Children.Add(line);

                var denBorder = new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(1, 1, 1, 0),
                    Child = den
                };
                Grid.SetRow(denBorder, 2);
                grid.Children.Add(denBorder);

                return grid;
            }

            private static UIElement CreateRadical(UIElement radicand, UIElement? rootDegree, Brush fg, double fontSize)
            {
                var grid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 2, 0)
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Col 0: Radical symbol with optional index
                if (rootDegree != null)
                {
                    var rootSp = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var degBorder = new Border
                    {
                        Margin = new Thickness(0, 0, -3, fontSize * 0.35),
                        Child = rootDegree
                    };
                    rootSp.Children.Add(degBorder);
                    rootSp.Children.Add(new TextBlock
                    {
                        Text = "√",
                        Foreground = fg,
                        FontSize = fontSize * 1.25,
                        FontFamily = MathFont,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    Grid.SetColumn(rootSp, 0);
                    grid.Children.Add(rootSp);
                }
                else
                {
                    var tick = new TextBlock
                    {
                        Text = "√",
                        Foreground = fg,
                        FontSize = fontSize * 1.25,
                        FontFamily = MathFont,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(tick, 0);
                    grid.Children.Add(tick);
                }

                // Col 1: Overbar and radicand content
                var overbarBorder = new Border
                {
                    BorderThickness = new Thickness(0, Math.Max(1.2, fontSize * 0.08), 0, 0),
                    BorderBrush = fg,
                    Padding = new Thickness(1, 2, 2, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = radicand
                };
                Grid.SetColumn(overbarBorder, 1);
                grid.Children.Add(overbarBorder);

                return grid;
            }

            private static UIElement CreateLargeOperator(string op, Brush fg, double fontSize, bool isDisplay)
            {
                string glyph = op switch
                {
                    "int" => "∫",
                    "iint" => "∬",
                    "iiint" => "∭",
                    "oint" => "∮",
                    "sum" => "∑",
                    "prod" => "∏",
                    "coprod" => "∐",
                    _ => op
                };

                double opSize = isDisplay ? fontSize * 1.45 : fontSize * 1.15;
                return new TextBlock
                {
                    Text = glyph,
                    Foreground = fg,
                    FontSize = opSize,
                    FontFamily = MathFont,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(1, 0, 1, 0)
                };
            }

            private static UIElement CreateAccent(UIElement baseElement, string accent, Brush fg, double fontSize)
            {
                string glyph = accent switch
                {
                    "hat" => "^",
                    "vec" => "→",
                    "bar" => "¯",
                    "dot" => "˙",
                    "ddot" => "¨",
                    "tilde" => "~",
                    "acute" => "´",
                    "grave" => "`",
                    "check" => "ˇ",
                    "breve" => "˘",
                    _ => ""
                };

                var grid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(1, 0, 1, 0)
                };
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var accentTb = new TextBlock
                {
                    Text = glyph,
                    Foreground = fg,
                    FontSize = fontSize * 0.75,
                    FontFamily = MathFont,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, -fontSize * 0.3)
                };
                Grid.SetRow(accentTb, 0);
                grid.Children.Add(accentTb);

                var baseBorder = new Border { Child = baseElement };
                Grid.SetRow(baseBorder, 1);
                grid.Children.Add(baseBorder);

                return grid;
            }

            private static UIElement CreateScriptedElement(UIElement baseElement, UIElement? sup, UIElement? sub, double fontSize)
            {
                var grid = new Grid
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                Grid.SetColumn(baseElement, 0);
                grid.Children.Add(baseElement);

                var scriptsGrid = new Grid
                {
                    VerticalAlignment = (sup != null && sub != null) ? VerticalAlignment.Center : (sup != null ? VerticalAlignment.Top : VerticalAlignment.Bottom),
                    Margin = new Thickness(1, 0, 0, 0)
                };
                scriptsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                scriptsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                if (sup != null)
                {
                    var supBorder = new Border
                    {
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, -fontSize * 0.25, 0, 0),
                        Child = sup
                    };
                    Grid.SetRow(supBorder, 0);
                    scriptsGrid.Children.Add(supBorder);
                }

                if (sub != null)
                {
                    var subBorder = new Border
                    {
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, fontSize * 0.15, 0, 0),
                        Child = sub
                    };
                    Grid.SetRow(subBorder, 1);
                    scriptsGrid.Children.Add(subBorder);
                }

                Grid.SetColumn(scriptsGrid, 1);
                grid.Children.Add(scriptsGrid);

                return grid;
            }
        }
    }
}
