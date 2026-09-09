using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace MDPlus.Core
{
    public enum ThemePreset
    {
        GitHubDark,
        GitHubLight,
        Nord,
        OneDark,
        Monokai
    }

    public class ThemePalette
    {
        public ThemePreset Preset { get; }
        public string Name { get; }
        public bool IsDark { get; }

        // Window & Editor
        public SolidColorBrush WindowBg { get; }
        public Color WindowBackgroundColor => WindowBg.Color;
        public SolidColorBrush WindowBackgroundBrush => WindowBg;

        public SolidColorBrush EditorBg { get; }
        public Color EditorBackgroundColor => EditorBg.Color;
        public SolidColorBrush EditorBackgroundBrush => EditorBg;

        public SolidColorBrush EditorFg { get; }
        public Color EditorForegroundColor => EditorFg.Color;
        public SolidColorBrush EditorForegroundBrush => EditorFg;

        public SolidColorBrush SidebarBg { get; }
        public Color SidebarBackgroundColor => SidebarBg.Color;
        public SolidColorBrush SidebarBackgroundBrush => SidebarBg;

        // Menu Bar & Dropdowns
        public SolidColorBrush MenuBg { get; }
        public Color MenuBackgroundColor => MenuBg.Color;
        public SolidColorBrush MenuBackgroundBrush => MenuBg;

        public SolidColorBrush MenuFg { get; }
        public Color MenuForegroundColor => MenuFg.Color;
        public SolidColorBrush MenuForegroundBrush => MenuFg;

        public SolidColorBrush MenuHoverBg { get; }
        public Color MenuHoverBackgroundColor => MenuHoverBg.Color;
        public SolidColorBrush MenuHoverBackgroundBrush => MenuHoverBg;

        public SolidColorBrush MenuHoverFg { get; }
        public Color MenuHoverForegroundColor => MenuHoverFg.Color;
        public SolidColorBrush MenuHoverForegroundBrush => MenuHoverFg;

        public SolidColorBrush MenuPopupBg { get; }
        public Color MenuPopupBackgroundColor => MenuPopupBg.Color;
        public SolidColorBrush MenuPopupBackgroundBrush => MenuPopupBg;

        public SolidColorBrush MenuPopupBorder { get; }
        public Color MenuPopupBorderColor => MenuPopupBorder.Color;
        public SolidColorBrush MenuPopupBorderBrush => MenuPopupBorder;

        public SolidColorBrush MenuBorder { get; }
        public Color MenuBorderColor => MenuBorder.Color;
        public SolidColorBrush MenuBorderBrush => MenuBorder;

        public SolidColorBrush MenuSeparator { get; }
        public Color MenuSeparatorColor => MenuSeparator.Color;
        public SolidColorBrush MenuSeparatorBrush => MenuSeparator;

        // Status Bar
        public SolidColorBrush StatusBg { get; }
        public Color StatusBarBackgroundColor => StatusBg.Color;
        public SolidColorBrush StatusBarBackgroundBrush => StatusBg;

        public SolidColorBrush StatusFg { get; }
        public Color StatusBarForegroundColor => StatusFg.Color;
        public SolidColorBrush StatusBarForegroundBrush => StatusFg;

        // Chrome & Accents
        public SolidColorBrush Border { get; }
        public Color BorderColor => Border.Color;
        public SolidColorBrush BorderBrush => Border;

        public SolidColorBrush MutedFg { get; }
        public Color MutedForegroundColor => MutedFg.Color;
        public SolidColorBrush MutedForegroundBrush => MutedFg;

        public SolidColorBrush Accent { get; }
        public Color AccentColor => Accent.Color;
        public SolidColorBrush AccentBrush => Accent;

        public SolidColorBrush SelectionBg { get; }
        public Color SelectionBackgroundColor => SelectionBg.Color;
        public SolidColorBrush SelectionBackgroundBrush => SelectionBg;

        // Document Blocks (Code & Tables)
        public SolidColorBrush CodeBg { get; }
        public Color CodeBackgroundColor => CodeBg.Color;
        public SolidColorBrush CodeBackgroundBrush => CodeBg;

        public SolidColorBrush CodeBorder { get; }
        public Color CodeBorderColor => CodeBorder.Color;
        public SolidColorBrush CodeBorderBrush => CodeBorder;

        public SolidColorBrush TableHeaderBg { get; }
        public Color TableHeaderBackgroundColor => TableHeaderBg.Color;
        public SolidColorBrush TableHeaderBackgroundBrush => TableHeaderBg;

        public SolidColorBrush TableAltRowBg { get; }
        public Color TableAltRowBackgroundColor => TableAltRowBg.Color;
        public SolidColorBrush TableAltRowBackgroundBrush => TableAltRowBg;

        public SolidColorBrush TableBorder { get; }
        public Color TableBorderColor => TableBorder.Color;
        public SolidColorBrush TableBorderBrush => TableBorder;

        public SolidColorBrush TabActiveBg { get; }
        public Color TabActiveBackgroundColor => TabActiveBg.Color;
        public SolidColorBrush TabActiveBackgroundBrush => TabActiveBg;

        public SolidColorBrush TabInactiveBg { get; }
        public Color TabInactiveBackgroundColor => TabInactiveBg.Color;
        public SolidColorBrush TabInactiveBackgroundBrush => TabInactiveBg;

        public SolidColorBrush HeadingFg { get; }
        public Color HeadingForegroundColor => HeadingFg.Color;
        public SolidColorBrush HeadingForegroundBrush => HeadingFg;

        // Syntax Highlighting Tokens
        public SolidColorBrush SyntaxKeyword { get; }
        public SolidColorBrush SyntaxString { get; }
        public SolidColorBrush SyntaxComment { get; }
        public SolidColorBrush SyntaxNumber { get; }
        public SolidColorBrush SyntaxType { get; }
        public SolidColorBrush SyntaxFunction { get; }
        public SolidColorBrush SyntaxProperty { get; }

        public ThemePalette(
            ThemePreset preset,
            string name,
            bool isDark,
            Color windowBg,
            Color editorBg,
            Color editorFg,
            Color sidebarBg,
            Color menuBg,
            Color menuFg,
            Color menuHoverBg,
            Color menuHoverFg,
            Color menuPopupBg,
            Color menuPopupBorder,
            Color menuBorder,
            Color menuSeparator,
            Color statusBg,
            Color statusFg,
            Color border,
            Color mutedFg,
            Color accent,
            Color selectionBg,
            Color codeBg,
            Color codeBorder,
            Color tableHeaderBg,
            Color tableAltRowBg,
            Color tableBorder,
            Color tabActiveBg,
            Color tabInactiveBg,
            Color headingFg,
            Color syntaxKeyword,
            Color syntaxString,
            Color syntaxComment,
            Color syntaxNumber,
            Color syntaxType,
            Color syntaxFunction,
            Color syntaxProperty)
        {
            Preset = preset;
            Name = name;
            IsDark = isDark;

            WindowBg = CreateFrozen(windowBg);
            EditorBg = CreateFrozen(editorBg);
            EditorFg = CreateFrozen(editorFg);
            SidebarBg = CreateFrozen(sidebarBg);

            MenuBg = CreateFrozen(menuBg);
            MenuFg = CreateFrozen(menuFg);
            MenuHoverBg = CreateFrozen(menuHoverBg);
            MenuHoverFg = CreateFrozen(menuHoverFg);
            MenuPopupBg = CreateFrozen(menuPopupBg);
            MenuPopupBorder = CreateFrozen(menuPopupBorder);
            MenuBorder = CreateFrozen(menuBorder);
            MenuSeparator = CreateFrozen(menuSeparator);

            StatusBg = CreateFrozen(statusBg);
            StatusFg = CreateFrozen(statusFg);

            Border = CreateFrozen(border);
            MutedFg = CreateFrozen(mutedFg);
            Accent = CreateFrozen(accent);
            SelectionBg = CreateFrozen(selectionBg);

            CodeBg = CreateFrozen(codeBg);
            CodeBorder = CreateFrozen(codeBorder);
            TableHeaderBg = CreateFrozen(tableHeaderBg);
            TableAltRowBg = CreateFrozen(tableAltRowBg);
            TableBorder = CreateFrozen(tableBorder);

            TabActiveBg = CreateFrozen(tabActiveBg);
            TabInactiveBg = CreateFrozen(tabInactiveBg);
            HeadingFg = CreateFrozen(headingFg);

            SyntaxKeyword = CreateFrozen(syntaxKeyword);
            SyntaxString = CreateFrozen(syntaxString);
            SyntaxComment = CreateFrozen(syntaxComment);
            SyntaxNumber = CreateFrozen(syntaxNumber);
            SyntaxType = CreateFrozen(syntaxType);
            SyntaxFunction = CreateFrozen(syntaxFunction);
            SyntaxProperty = CreateFrozen(syntaxProperty);
        }

        private static SolidColorBrush CreateFrozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static Color FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return Colors.Black;
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return Color.FromRgb(r, g, b);
            }
            if (hex.Length == 8)
            {
                byte a = Convert.ToByte(hex.Substring(0, 2), 16);
                byte r = Convert.ToByte(hex.Substring(2, 2), 16);
                byte g = Convert.ToByte(hex.Substring(4, 2), 16);
                byte b = Convert.ToByte(hex.Substring(6, 2), 16);
                return Color.FromArgb(a, r, g, b);
            }
            return Colors.Black;
        }

        public static ThemePalette GitHubDark { get; } = new ThemePalette(
            ThemePreset.GitHubDark,
            "GitHub Dark",
            isDark: true,
            windowBg: FromHex("#0D1117"),
            editorBg: FromHex("#0D1117"),
            editorFg: FromHex("#E6EDF3"),
            sidebarBg: FromHex("#161B22"),
            menuBg: FromHex("#161B22"),
            menuFg: FromHex("#E6EDF3"),
            menuHoverBg: FromHex("#30363D"),
            menuHoverFg: FromHex("#FFFFFF"),
            menuPopupBg: FromHex("#161B22"),
            menuPopupBorder: FromHex("#30363D"),
            menuBorder: FromHex("#30363D"),
            menuSeparator: FromHex("#30363D"),
            statusBg: FromHex("#1F6FEB"),
            statusFg: FromHex("#FFFFFF"),
            border: FromHex("#30363D"),
            mutedFg: FromHex("#8B949E"),
            accent: FromHex("#58A6FF"),
            selectionBg: FromHex("#264F78"),
            codeBg: FromHex("#161B22"),
            codeBorder: FromHex("#30363D"),
            tableHeaderBg: FromHex("#21262D"),
            tableAltRowBg: FromHex("#161B22"),
            tableBorder: FromHex("#30363D"),
            tabActiveBg: FromHex("#0D1117"),
            tabInactiveBg: FromHex("#161B22"),
            headingFg: FromHex("#F0F6FC"),
            syntaxKeyword: FromHex("#FF7B72"),
            syntaxString: FromHex("#A5D6FF"),
            syntaxComment: FromHex("#8B949E"),
            syntaxNumber: FromHex("#79C0FF"),
            syntaxType: FromHex("#FFA657"),
            syntaxFunction: FromHex("#D2A8FF"),
            syntaxProperty: FromHex("#7EE787")
        );

        public static ThemePalette GitHubLight { get; } = new ThemePalette(
            ThemePreset.GitHubLight,
            "GitHub Light",
            isDark: false,
            windowBg: FromHex("#FFFFFF"),
            editorBg: FromHex("#FFFFFF"),
            editorFg: FromHex("#24292F"),
            sidebarBg: FromHex("#F6F8FA"),
            menuBg: FromHex("#F6F8FA"),
            menuFg: FromHex("#24292F"),
            menuHoverBg: FromHex("#EAEEF2"),
            menuHoverFg: FromHex("#24292F"),
            menuPopupBg: FromHex("#FFFFFF"),
            menuPopupBorder: FromHex("#D0D7DE"),
            menuBorder: FromHex("#D0D7DE"),
            menuSeparator: FromHex("#D0D7DE"),
            statusBg: FromHex("#0969DA"),
            statusFg: FromHex("#FFFFFF"),
            border: FromHex("#D0D7DE"),
            mutedFg: FromHex("#57606A"),
            accent: FromHex("#0969DA"),
            selectionBg: FromHex("#B6E3FF"),
            codeBg: FromHex("#F6F8FA"),
            codeBorder: FromHex("#D0D7DE"),
            tableHeaderBg: FromHex("#F6F8FA"),
            tableAltRowBg: FromHex("#F6F8FA"),
            tableBorder: FromHex("#D0D7DE"),
            tabActiveBg: FromHex("#FFFFFF"),
            tabInactiveBg: FromHex("#F6F8FA"),
            headingFg: FromHex("#1F2328"),
            syntaxKeyword: FromHex("#CF222E"),
            syntaxString: FromHex("#0A3069"),
            syntaxComment: FromHex("#6E7781"),
            syntaxNumber: FromHex("#0550AE"),
            syntaxType: FromHex("#953800"),
            syntaxFunction: FromHex("#8250DF"),
            syntaxProperty: FromHex("#116329")
        );

        public static ThemePalette Nord { get; } = new ThemePalette(
            ThemePreset.Nord,
            "Nord",
            isDark: true,
            windowBg: FromHex("#2E3440"),
            editorBg: FromHex("#2E3440"),
            editorFg: FromHex("#ECEFF4"),
            sidebarBg: FromHex("#2E3440"),
            menuBg: FromHex("#3B4252"),
            menuFg: FromHex("#ECEFF4"),
            menuHoverBg: FromHex("#4C566A"),
            menuHoverFg: FromHex("#ECEFF4"),
            menuPopupBg: FromHex("#2E3440"),
            menuPopupBorder: FromHex("#4C566A"),
            menuBorder: FromHex("#4C566A"),
            menuSeparator: FromHex("#3B4252"),
            statusBg: FromHex("#3B4252"),
            statusFg: FromHex("#ECEFF4"),
            border: FromHex("#4C566A"),
            mutedFg: FromHex("#7B88A1"),
            accent: FromHex("#88C0D0"),
            selectionBg: FromHex("#434C5E"),
            codeBg: FromHex("#3B4252"),
            codeBorder: FromHex("#4C566A"),
            tableHeaderBg: FromHex("#434C5E"),
            tableAltRowBg: FromHex("#3B4252"),
            tableBorder: FromHex("#4C566A"),
            tabActiveBg: FromHex("#2E3440"),
            tabInactiveBg: FromHex("#3B4252"),
            headingFg: FromHex("#ECEFF4"),
            syntaxKeyword: FromHex("#81A1C1"),
            syntaxString: FromHex("#A3BE8C"),
            syntaxComment: FromHex("#616E88"),
            syntaxNumber: FromHex("#B48EAD"),
            syntaxType: FromHex("#8FBCBB"),
            syntaxFunction: FromHex("#88C0D0"),
            syntaxProperty: FromHex("#D8DEE9")
        );

        public static ThemePalette OneDark { get; } = new ThemePalette(
            ThemePreset.OneDark,
            "One Dark",
            isDark: true,
            windowBg: FromHex("#282C34"),
            editorBg: FromHex("#282C34"),
            editorFg: FromHex("#ABB2BF"),
            sidebarBg: FromHex("#21252B"),
            menuBg: FromHex("#21252B"),
            menuFg: FromHex("#ABB2BF"),
            menuHoverBg: FromHex("#3E4451"),
            menuHoverFg: FromHex("#FFFFFF"),
            menuPopupBg: FromHex("#21252B"),
            menuPopupBorder: FromHex("#181A1F"),
            menuBorder: FromHex("#3E4451"),
            menuSeparator: FromHex("#353B45"),
            statusBg: FromHex("#21252B"),
            statusFg: FromHex("#9DA5B4"),
            border: FromHex("#3E4451"),
            mutedFg: FromHex("#828997"),
            accent: FromHex("#61AFEF"),
            selectionBg: FromHex("#3E4451"),
            codeBg: FromHex("#21252B"),
            codeBorder: FromHex("#3E4451"),
            tableHeaderBg: FromHex("#2C313A"),
            tableAltRowBg: FromHex("#21252B"),
            tableBorder: FromHex("#3E4451"),
            tabActiveBg: FromHex("#282C34"),
            tabInactiveBg: FromHex("#21252B"),
            headingFg: FromHex("#E5E5E5"),
            syntaxKeyword: FromHex("#C678DD"),
            syntaxString: FromHex("#98C379"),
            syntaxComment: FromHex("#7F848E"),
            syntaxNumber: FromHex("#D19A66"),
            syntaxType: FromHex("#E5C07B"),
            syntaxFunction: FromHex("#61AFEF"),
            syntaxProperty: FromHex("#E06C75")
        );

        public static ThemePalette Monokai { get; } = new ThemePalette(
            ThemePreset.Monokai,
            "Monokai",
            isDark: true,
            windowBg: FromHex("#272822"),
            editorBg: FromHex("#272822"),
            editorFg: FromHex("#F8F8F2"),
            sidebarBg: FromHex("#1E1F1C"),
            menuBg: FromHex("#1E1F1C"),
            menuFg: FromHex("#F8F8F2"),
            menuHoverBg: FromHex("#3E3D32"),
            menuHoverFg: FromHex("#A6E22E"),
            menuPopupBg: FromHex("#1E1F1C"),
            menuPopupBorder: FromHex("#49483E"),
            menuBorder: FromHex("#49483E"),
            menuSeparator: FromHex("#3E3D32"),
            statusBg: FromHex("#1E1F1C"),
            statusFg: FromHex("#F8F8F2"),
            border: FromHex("#49483E"),
            mutedFg: FromHex("#8F8A74"),
            accent: FromHex("#A6E22E"),
            selectionBg: FromHex("#49483E"),
            codeBg: FromHex("#1E1F1C"),
            codeBorder: FromHex("#49483E"),
            tableHeaderBg: FromHex("#3E3D32"),
            tableAltRowBg: FromHex("#22231E"),
            tableBorder: FromHex("#49483E"),
            tabActiveBg: FromHex("#272822"),
            tabInactiveBg: FromHex("#1E1F1C"),
            headingFg: FromHex("#F8F8F2"),
            syntaxKeyword: FromHex("#F92672"),
            syntaxString: FromHex("#E6DB74"),
            syntaxComment: FromHex("#8F8A74"),
            syntaxNumber: FromHex("#AE81FF"),
            syntaxType: FromHex("#66D9EF"),
            syntaxFunction: FromHex("#A6E22E"),
            syntaxProperty: FromHex("#FD971F")
        );

        private static readonly Dictionary<ThemePreset, ThemePalette> Palettes = new Dictionary<ThemePreset, ThemePalette>
        {
            [ThemePreset.GitHubDark] = GitHubDark,
            [ThemePreset.GitHubLight] = GitHubLight,
            [ThemePreset.Nord] = Nord,
            [ThemePreset.OneDark] = OneDark,
            [ThemePreset.Monokai] = Monokai
        };

        public static ThemePalette GetPalette(ThemePreset preset)
        {
            return Palettes.TryGetValue(preset, out var palette) ? palette : GitHubDark;
        }

        public static double CalculateRelativeLuminance(Color color)
        {
            double r = SrgbToLinear(color.R / 255.0);
            double g = SrgbToLinear(color.G / 255.0);
            double b = SrgbToLinear(color.B / 255.0);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        private static double SrgbToLinear(double val)
        {
            return val <= 0.04045 ? val / 12.92 : Math.Pow((val + 0.055) / 1.055, 2.4);
        }

        public static double CalculateContrast(Color c1, Color c2)
        {
            double l1 = CalculateRelativeLuminance(c1);
            double l2 = CalculateRelativeLuminance(c2);
            double lighter = Math.Max(l1, l2);
            double darker = Math.Min(l1, l2);
            return (lighter + 0.05) / (darker + 0.05);
        }
    }
}
