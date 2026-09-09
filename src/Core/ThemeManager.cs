using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace MDPlus.Core
{
    public enum AppThemeMode
    {
        System,
        Light,
        Dark
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class ThemeManager
    {
        private static ThemeManager? _instance;
        public static ThemeManager Instance => _instance ??= new ThemeManager();

        private AppThemeMode _mode = AppThemeMode.Dark;
        private ThemePreset _currentPreset = ThemePreset.GitHubDark;
        private bool _useLegacyBrushes = false;

        public AppThemeMode Mode
        {
            get => _mode;
            set
            {
                if (_mode != value || !_useLegacyBrushes)
                {
                    _mode = value;
                    _useLegacyBrushes = true;
                    UpdateThemeFromMode();
                }
            }
        }

        public ThemePreset CurrentPreset
        {
            get => _currentPreset;
            set => SetPreset(value);
        }

        public ThemePalette CurrentPalette => ThemePalette.GetPalette(_currentPreset);

        public bool IsDark { get; private set; } = true;

        public event EventHandler? ThemeChanged;

        private ThemeManager()
        {
            SystemEvents.UserPreferenceChanged += (s, e) =>
            {
                if (_mode == AppThemeMode.System && _useLegacyBrushes)
                {
                    UpdateThemeFromMode();
                }
            };

            SetPreset(ThemePreset.GitHubDark);
        }

        public void SetPreset(ThemePreset preset)
        {
            _currentPreset = preset;
            _useLegacyBrushes = false;
            IsDark = CurrentPalette.IsDark;
            _mode = IsDark ? AppThemeMode.Dark : AppThemeMode.Light;

            UpdateApplicationResources(CurrentPalette);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CycleNextTheme()
        {
            int count = Enum.GetValues<ThemePreset>().Length;
            int next = ((int)_currentPreset + 1) % count;
            SetPreset((ThemePreset)next);
        }

        public void UpdateTheme()
        {
            if (_useLegacyBrushes)
            {
                UpdateThemeFromMode();
            }
            else
            {
                SetPreset(_currentPreset);
            }
        }

        private void UpdateThemeFromMode()
        {
            if (_mode == AppThemeMode.System)
            {
                IsDark = GetWindowsSystemIsDark();
            }
            else
            {
                IsDark = _mode == AppThemeMode.Dark;
            }

            _currentPreset = IsDark ? ThemePreset.GitHubDark : ThemePreset.GitHubLight;
            UpdateApplicationResources(CurrentPalette);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateApplicationResources(ThemePalette palette)
        {
            var app = Application.Current;
            if (app == null) return;

            app.Resources["MenuBackgroundBrush"] = palette.MenuBg;
            app.Resources["MenuForegroundBrush"] = palette.MenuFg;
            app.Resources["MenuHoverBackgroundBrush"] = palette.MenuHoverBg;
            app.Resources["MenuHoverForegroundBrush"] = palette.MenuHoverFg;
            app.Resources["MenuPopupBackgroundBrush"] = palette.MenuPopupBg;
            app.Resources["MenuPopupBorderBrush"] = palette.MenuPopupBorder;
            app.Resources["MenuBorderBrush"] = palette.MenuBorder;
            app.Resources["MenuSeparatorBrush"] = palette.MenuSeparator;

            app.Resources["WindowBackgroundBrush"] = palette.WindowBg;
            app.Resources["DocumentBackgroundBrush"] = palette.EditorBg;
            app.Resources["ForegroundBrush"] = palette.EditorFg;
            app.Resources["BorderBrush"] = palette.Border;
            app.Resources["MutedForegroundBrush"] = palette.MutedFg;
            app.Resources["AccentBrush"] = palette.Accent;
            app.Resources["StatusBarBackgroundBrush"] = palette.StatusBg;
            app.Resources["StatusBarForegroundBrush"] = palette.StatusFg;

            app.Resources["SidebarBackgroundBrush"] = palette.SidebarBg;
            app.Resources["HeadingForegroundBrush"] = palette.HeadingFg;
            app.Resources["SelectionBackgroundBrush"] = palette.SelectionBg;
            app.Resources["CodeBackgroundBrush"] = palette.CodeBg;
        }

        private bool GetWindowsSystemIsDark()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key != null)
                {
                    object? val = key.GetValue("AppsUseLightTheme");
                    if (val is int lightThemeInt)
                    {
                        return lightThemeInt == 0;
                    }
                }
            }
            catch
            {
                // Fallback to dark if cannot read registry
            }
            return false;
        }

        // Color Palettes (Cached and frozen for performance)
        private static readonly SolidColorBrush DarkWindowBg = CreateFrozen(Color.FromRgb(30, 30, 30));
        private static readonly SolidColorBrush LightWindowBg = CreateFrozen(Color.FromRgb(250, 250, 250));
        private static readonly SolidColorBrush DarkDocBg = CreateFrozen(Color.FromRgb(24, 24, 24));
        private static readonly SolidColorBrush LightDocBg = CreateFrozen(Color.FromRgb(255, 255, 255));
        private static readonly SolidColorBrush DarkSidebarBg = CreateFrozen(Color.FromRgb(37, 37, 38));
        private static readonly SolidColorBrush LightSidebarBg = CreateFrozen(Color.FromRgb(243, 243, 243));
        private static readonly SolidColorBrush DarkMenuBg = CreateFrozen(Color.FromRgb(45, 45, 48));
        private static readonly SolidColorBrush LightMenuBg = CreateFrozen(Color.FromRgb(245, 245, 245));
        private static readonly SolidColorBrush DarkStatusBg = CreateFrozen(Color.FromRgb(0, 122, 204));
        private static readonly SolidColorBrush LightStatusBg = CreateFrozen(Color.FromRgb(240, 240, 240));
        private static readonly SolidColorBrush DarkStatusFg = Brushes.White;
        private static readonly SolidColorBrush LightStatusFg = CreateFrozen(Color.FromRgb(50, 50, 50));
        private static readonly SolidColorBrush DarkFg = CreateFrozen(Color.FromRgb(220, 220, 220));
        private static readonly SolidColorBrush LightFg = CreateFrozen(Color.FromRgb(30, 30, 30));
        private static readonly SolidColorBrush DarkBorder = CreateFrozen(Color.FromRgb(60, 60, 60));
        private static readonly SolidColorBrush LightBorder = CreateFrozen(Color.FromRgb(220, 220, 220));
        private static readonly SolidColorBrush DarkAccent = CreateFrozen(Color.FromRgb(88, 166, 255));
        private static readonly SolidColorBrush LightAccent = CreateFrozen(Color.FromRgb(9, 105, 218));
        private static readonly SolidColorBrush DarkTabActive = CreateFrozen(Color.FromRgb(24, 24, 24));
        private static readonly SolidColorBrush LightTabActive = CreateFrozen(Color.FromRgb(255, 255, 255));
        private static readonly SolidColorBrush DarkTabInactive = CreateFrozen(Color.FromRgb(40, 40, 40));
        private static readonly SolidColorBrush LightTabInactive = CreateFrozen(Color.FromRgb(235, 235, 235));

        private static SolidColorBrush CreateFrozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public SolidColorBrush WindowBackground => _useLegacyBrushes ? (IsDark ? DarkWindowBg : LightWindowBg) : CurrentPalette.WindowBg;
        public SolidColorBrush DocumentBackground => _useLegacyBrushes ? (IsDark ? DarkDocBg : LightDocBg) : CurrentPalette.EditorBg;
        public SolidColorBrush SidebarBackground => _useLegacyBrushes ? (IsDark ? DarkSidebarBg : LightSidebarBg) : CurrentPalette.SidebarBg;
        public SolidColorBrush MenuBackground => _useLegacyBrushes ? (IsDark ? DarkMenuBg : LightMenuBg) : CurrentPalette.MenuBg;
        public SolidColorBrush StatusBarBackground => _useLegacyBrushes ? (IsDark ? DarkStatusBg : LightStatusBg) : CurrentPalette.StatusBg;
        public SolidColorBrush StatusBarForeground => _useLegacyBrushes ? (IsDark ? DarkStatusFg : LightStatusFg) : CurrentPalette.StatusFg;
        public SolidColorBrush Foreground => _useLegacyBrushes ? (IsDark ? DarkFg : LightFg) : CurrentPalette.EditorFg;
        public SolidColorBrush BorderBrush => _useLegacyBrushes ? (IsDark ? DarkBorder : LightBorder) : CurrentPalette.Border;
        public SolidColorBrush AccentBrush => _useLegacyBrushes ? (IsDark ? DarkAccent : LightAccent) : CurrentPalette.Accent;
        public SolidColorBrush TabActiveBackground => _useLegacyBrushes ? (IsDark ? DarkTabActive : LightTabActive) : CurrentPalette.TabActiveBg;
        public SolidColorBrush TabInactiveBackground => _useLegacyBrushes ? (IsDark ? DarkTabInactive : LightTabInactive) : CurrentPalette.TabInactiveBg;
        public SolidColorBrush HeadingForeground => CurrentPalette.HeadingFg;
        public SolidColorBrush SelectionBackground => CurrentPalette.SelectionBg;
        public SolidColorBrush CodeBackground => CurrentPalette.CodeBg;
    }
}
