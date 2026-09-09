using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace MDPlus.Core
{
    public enum TokenType
    {
        Plain,
        Keyword,
        String,
        Comment,
        Number,
        Type,
        Operator,
        Function,
        Property
    }

    public class HighlightToken
    {
        public string Text { get; set; } = string.Empty;
        public TokenType Type { get; set; }
    }

    public static class SyntaxHighlighter
    {
        private static readonly HashSet<string> CStyleKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "void", "volatile", "while", "var", "async", "await", "record", "init",
            "fn", "let", "mut", "impl", "trait", "match", "pub", "crate", "package", "func"
        };

        private static readonly HashSet<string> PythonKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "and", "as", "assert", "async", "await", "break", "class", "continue", "def", "del",
            "elif", "else", "except", "False", "finally", "for", "from", "global", "if", "import",
            "in", "is", "lambda", "None", "nonlocal", "not", "or", "pass", "raise", "return",
            "True", "try", "while", "with", "yield"
        };

        private static readonly HashSet<string> JsKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "break", "case", "catch", "class", "const", "continue", "debugger", "default", "delete",
            "do", "else", "export", "extends", "finally", "for", "function", "if", "import", "in",
            "instanceof", "new", "return", "super", "switch", "this", "throw", "try", "typeof",
            "var", "void", "while", "with", "yield", "let", "static", "enum", "await", "async",
            "null", "undefined", "true", "false", "from", "of", "type", "interface"
        };

        private static readonly HashSet<string> SqlKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "FROM", "WHERE", "INSERT", "INTO", "UPDATE", "DELETE", "JOIN", "INNER", "LEFT",
            "RIGHT", "OUTER", "ON", "GROUP", "BY", "ORDER", "HAVING", "LIMIT", "OFFSET", "CREATE",
            "TABLE", "ALTER", "DROP", "INDEX", "VIEW", "AS", "AND", "OR", "NOT", "NULL", "IS",
            "LIKE", "IN", "BETWEEN", "EXISTS", "CASE", "WHEN", "THEN", "ELSE", "END", "UNION", "ALL"
        };

        private static readonly HashSet<string> ShellKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "if", "fi", "then", "else", "elif", "for", "while", "do", "done", "case", "esac",
            "function", "return", "exit", "echo", "cd", "ls", "pwd", "mkdir", "rm", "cp", "mv",
            "git", "dotnet", "npm", "cargo", "docker", "kubectl", "curl", "wget", "cat", "grep",
            "Set-Location", "Get-ChildItem", "Write-Host", "Write-Output", "param", "process"
        };

        public static List<HighlightToken> Highlight(string code, string language)
        {
            var tokens = new List<HighlightToken>();
            if (string.IsNullOrEmpty(code)) return tokens;

            string lang = language.ToLowerInvariant();

            switch (lang)
            {
                case "json":
                    return HighlightJson(code);

                case "xml":
                case "html":
                    return HighlightXml(code);

                case "python":
                case "py":
                    return HighlightKeywords(code, PythonKeywords, "#", true);

                case "js":
                case "javascript":
                case "ts":
                case "typescript":
                case "jsx":
                case "tsx":
                    return HighlightKeywords(code, JsKeywords, "//", false);

                case "sql":
                    return HighlightKeywords(code, SqlKeywords, "--", false);

                case "sh":
                case "bash":
                case "shell":
                case "zsh":
                case "ps1":
                case "powershell":
                    return HighlightKeywords(code, ShellKeywords, "#", false);

                case "cs":
                case "csharp":
                case "c":
                case "cpp":
                case "c++":
                case "rust":
                case "rs":
                case "go":
                case "java":
                case "kotlin":
                case "swift":
                default:
                    return HighlightKeywords(code, CStyleKeywords, "//", false);
            }
        }

        private static readonly SolidColorBrush DarkKeywordBrush = CreateFrozen(Color.FromRgb(255, 123, 114));
        private static readonly SolidColorBrush DarkStringBrush = CreateFrozen(Color.FromRgb(165, 214, 255));
        private static readonly SolidColorBrush DarkCommentBrush = CreateFrozen(Color.FromRgb(139, 148, 158));
        private static readonly SolidColorBrush DarkNumberBrush = CreateFrozen(Color.FromRgb(121, 192, 255));
        private static readonly SolidColorBrush DarkTypeBrush = CreateFrozen(Color.FromRgb(255, 166, 87));
        private static readonly SolidColorBrush DarkPropertyBrush = CreateFrozen(Color.FromRgb(126, 231, 135));
        private static readonly SolidColorBrush DarkPlainBrush = CreateFrozen(Color.FromRgb(230, 237, 243));

        private static readonly SolidColorBrush LightKeywordBrush = CreateFrozen(Color.FromRgb(207, 34, 46));
        private static readonly SolidColorBrush LightStringBrush = CreateFrozen(Color.FromRgb(10, 48, 105));
        private static readonly SolidColorBrush LightCommentBrush = CreateFrozen(Color.FromRgb(101, 109, 118));
        private static readonly SolidColorBrush LightNumberBrush = CreateFrozen(Color.FromRgb(5, 80, 174));
        private static readonly SolidColorBrush LightTypeBrush = CreateFrozen(Color.FromRgb(149, 56, 0));
        private static readonly SolidColorBrush LightPropertyBrush = CreateFrozen(Color.FromRgb(17, 99, 41));
        private static readonly SolidColorBrush LightPlainBrush = CreateFrozen(Color.FromRgb(36, 41, 47));

        private static SolidColorBrush CreateFrozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static bool StartsWithAt(string str, int index, string prefix)
        {
            if (index + prefix.Length > str.Length) return false;
            for (int p = 0; p < prefix.Length; p++)
            {
                if (str[index + p] != prefix[p]) return false;
            }
            return true;
        }

        private static List<HighlightToken> HighlightKeywords(string code, HashSet<string> keywords, string lineCommentPrefix, bool hashComment)
        {
            var tokens = new List<HighlightToken>();
            int i = 0;
            int len = code.Length;

            while (i < len)
            {
                // 1. Line Comment
                if (StartsWithAt(code, i, lineCommentPrefix))
                {
                    int end = code.IndexOf('\n', i);
                    if (end == -1) end = len;
                    tokens.Add(new HighlightToken { Text = code.Substring(i, end - i), Type = TokenType.Comment });
                    i = end;
                    continue;
                }

                // 2. Block Comment /* ... */ (if not hash comment)
                if (!hashComment && StartsWithAt(code, i, "/*"))
                {
                    int end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    if (end == -1) end = len;
                    else end += 2;
                    tokens.Add(new HighlightToken { Text = code.Substring(i, end - i), Type = TokenType.Comment });
                    i = end;
                    continue;
                }

                // 3. Strings: "..." or '...' or `...`
                char c = code[i];
                if (c == '"' || c == '\'' || c == '`')
                {
                    char quote = c;
                    int j = i + 1;
                    bool escape = false;
                    while (j < len)
                    {
                        if (escape)
                        {
                            escape = false;
                        }
                        else if (code[j] == '\\')
                        {
                            escape = true;
                        }
                        else if (code[j] == quote)
                        {
                            j++;
                            break;
                        }
                        else if (code[j] == '\n' && quote != '`')
                        {
                            break; // single-line string broken
                        }
                        j++;
                    }
                    tokens.Add(new HighlightToken { Text = code.Substring(i, j - i), Type = TokenType.String });
                    i = j;
                    continue;
                }

                // 4. Numbers
                if (char.IsDigit(c) && (i == 0 || !char.IsLetterOrDigit(code[i - 1])))
                {
                    int j = i;
                    while (j < len && (char.IsLetterOrDigit(code[j]) || code[j] == '.' || code[j] == '_'))
                    {
                        j++;
                    }
                    tokens.Add(new HighlightToken { Text = code.Substring(i, j - i), Type = TokenType.Number });
                    i = j;
                    continue;
                }

                // 5. Identifiers / Words
                if (char.IsLetter(c) || c == '_')
                {
                    int j = i;
                    while (j < len && (char.IsLetterOrDigit(code[j]) || code[j] == '_'))
                    {
                        j++;
                    }
                    string word = code.Substring(i, j - i);
                    if (keywords.Contains(word))
                    {
                        tokens.Add(new HighlightToken { Text = word, Type = TokenType.Keyword });
                    }
                    else if (char.IsUpper(word[0]) && word.Length > 1)
                    {
                        tokens.Add(new HighlightToken { Text = word, Type = TokenType.Type });
                    }
                    else
                    {
                        tokens.Add(new HighlightToken { Text = word, Type = TokenType.Plain });
                    }
                    i = j;
                    continue;
                }

                // 6. Whitespace / Punctuation / Operators
                tokens.Add(new HighlightToken { Text = c.ToString(), Type = TokenType.Plain });
                i++;
            }

            return tokens;
        }

        private static List<HighlightToken> HighlightJson(string code)
        {
            var tokens = new List<HighlightToken>();
            int i = 0;
            int len = code.Length;

            while (i < len)
            {
                char c = code[i];

                if (c == '"')
                {
                    int j = i + 1;
                    bool escape = false;
                    while (j < len)
                    {
                        if (escape) escape = false;
                        else if (code[j] == '\\') escape = true;
                        else if (code[j] == '"') { j++; break; }
                        j++;
                    }

                    string str = code.Substring(i, j - i);
                    // Peek if followed by : (property key)
                    int k = j;
                    while (k < len && char.IsWhiteSpace(code[k])) k++;
                    bool isKey = k < len && code[k] == ':';

                    tokens.Add(new HighlightToken { Text = str, Type = isKey ? TokenType.Property : TokenType.String });
                    i = j;
                    continue;
                }

                if (char.IsDigit(c) || c == '-')
                {
                    int j = i + 1;
                    while (j < len && (char.IsDigit(code[j]) || code[j] == '.' || code[j] == 'e' || code[j] == 'E' || code[j] == '+' || code[j] == '-'))
                    {
                        j++;
                    }
                    tokens.Add(new HighlightToken { Text = code.Substring(i, j - i), Type = TokenType.Number });
                    i = j;
                    continue;
                }

                if (char.IsLetter(c))
                {
                    int j = i;
                    while (j < len && char.IsLetter(code[j])) j++;
                    string word = code.Substring(i, j - i);
                    if (word == "true" || word == "false" || word == "null")
                    {
                        tokens.Add(new HighlightToken { Text = word, Type = TokenType.Keyword });
                    }
                    else
                    {
                        tokens.Add(new HighlightToken { Text = word, Type = TokenType.Plain });
                    }
                    i = j;
                    continue;
                }

                tokens.Add(new HighlightToken { Text = c.ToString(), Type = TokenType.Plain });
                i++;
            }

            return tokens;
        }

        private static List<HighlightToken> HighlightXml(string code)
        {
            var tokens = new List<HighlightToken>();
            int i = 0;
            int len = code.Length;

            while (i < len)
            {
                // Comment <!-- ... -->
                if (StartsWithAt(code, i, "<!--"))
                {
                    int end = code.IndexOf("-->", i + 4, StringComparison.Ordinal);
                    if (end == -1) end = len;
                    else end += 3;
                    tokens.Add(new HighlightToken { Text = code.Substring(i, end - i), Type = TokenType.Comment });
                    i = end;
                    continue;
                }

                // Tag < ... >
                if (code[i] == '<')
                {
                    int end = code.IndexOf('>', i + 1);
                    if (end == -1) end = len;
                    else end += 1;
                    string tag = code.Substring(i, end - i);
                    tokens.Add(new HighlightToken { Text = tag, Type = TokenType.Keyword });
                    i = end;
                    continue;
                }

                // Normal text
                int nextTag = code.IndexOf('<', i);
                if (nextTag == -1) nextTag = len;
                tokens.Add(new HighlightToken { Text = code.Substring(i, nextTag - i), Type = TokenType.Plain });
                i = nextTag;
            }

            return tokens;
        }

        public static SolidColorBrush GetTokenBrush(TokenType type, bool isDark)
        {
            if (isDark)
            {
                return type switch
                {
                    TokenType.Keyword => DarkKeywordBrush,
                    TokenType.String => DarkStringBrush,
                    TokenType.Comment => DarkCommentBrush,
                    TokenType.Number => DarkNumberBrush,
                    TokenType.Type => DarkTypeBrush,
                    TokenType.Property => DarkPropertyBrush,
                    _ => DarkPlainBrush
                };
            }
            else
            {
                return type switch
                {
                    TokenType.Keyword => LightKeywordBrush,
                    TokenType.String => LightStringBrush,
                    TokenType.Comment => LightCommentBrush,
                    TokenType.Number => LightNumberBrush,
                    TokenType.Type => LightTypeBrush,
                    TokenType.Property => LightPropertyBrush,
                    _ => LightPlainBrush
                };
            }
        }
    }
}
