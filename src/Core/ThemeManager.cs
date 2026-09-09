using System;
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

        private AppThemeMode _mode = AppThemeMode.System;
        public AppThemeMode Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    UpdateTheme();
                }
            }
        }

        public bool IsDark { get; private set; }

        public event EventHandler? ThemeChanged;

        private ThemeManager()
        {
            SystemEvents.UserPreferenceChanged += (s, e) =>
            {
                if (_mode == AppThemeMode.System)
                {
                    UpdateTheme();
                }
            };
            UpdateTheme();
        }

        public void UpdateTheme()
        {
            bool previous = IsDark;

            if (_mode == AppThemeMode.System)
            {
                IsDark = GetWindowsSystemIsDark();
            }
            else
            {
                IsDark = _mode == AppThemeMode.Dark;
            }

            ThemeChanged?.Invoke(this, EventArgs.Empty);
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

        public SolidColorBrush WindowBackground => IsDark ? DarkWindowBg : LightWindowBg;
        public SolidColorBrush DocumentBackground => IsDark ? DarkDocBg : LightDocBg;
        public SolidColorBrush SidebarBackground => IsDark ? DarkSidebarBg : LightSidebarBg;
        public SolidColorBrush MenuBackground => IsDark ? DarkMenuBg : LightMenuBg;
        public SolidColorBrush StatusBarBackground => IsDark ? DarkStatusBg : LightStatusBg;
        public SolidColorBrush StatusBarForeground => IsDark ? DarkStatusFg : LightStatusFg;
        public SolidColorBrush Foreground => IsDark ? DarkFg : LightFg;
        public SolidColorBrush BorderBrush => IsDark ? DarkBorder : LightBorder;
        public SolidColorBrush AccentBrush => IsDark ? DarkAccent : LightAccent;
        public SolidColorBrush TabActiveBackground => IsDark ? DarkTabActive : LightTabActive;
        public SolidColorBrush TabInactiveBackground => IsDark ? DarkTabInactive : LightTabInactive;
    }
}
