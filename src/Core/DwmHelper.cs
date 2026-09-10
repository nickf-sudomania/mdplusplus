using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MDPlus.Core
{
    /// <summary>
    /// Helper for Windows Desktop Window Manager (DWM) interop to dynamically theme
    /// the window title bar and caption controls matching the active theme.
    /// </summary>
    public static class DwmHelper
    {
        // DWM attribute constants
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_BORDER_COLOR = 34;
        public const int DWMWA_CAPTION_COLOR = 35;
        public const int DWMWA_TEXT_COLOR = 36;
        public const uint DWMWA_COLOR_DEFAULT = 0xFFFFFFFF;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// Converts a WPF Color into a Win32 COLORREF integer (0x00BBGGRR).
        /// </summary>
        public static int ColorToColorRef(Color color)
        {
            return color.R | (color.G << 8) | (color.B << 16);
        }

        /// <summary>
        /// Converts RGB byte values into a Win32 COLORREF integer (0x00BBGGRR).
        /// </summary>
        public static int ColorToColorRef(byte r, byte g, byte b)
        {
            return r | (g << 8) | (b << 16);
        }

        /// <summary>
        /// Applies the theme's title bar attributes to the specified window handle.
        /// Enables or disables immersive dark mode and applies caption/text colors.
        /// </summary>
        public static bool ApplyTitleBarTheme(IntPtr hwnd, ThemePalette palette)
        {
            if (palette == null || hwnd == IntPtr.Zero) return false;
            return ApplyTitleBarTheme(hwnd, palette.IsDark, palette.MenuBackgroundColor, palette.MenuForegroundColor, palette.BorderColor);
        }

        /// <summary>
        /// Applies title bar styling using explicit dark mode flag, caption color, text color, and optional border color.
        /// Gracefully falls back on older Windows builds where attributes 34/35/36 or 20 are unsupported.
        /// </summary>
        public static bool ApplyTitleBarTheme(IntPtr hwnd, bool isDark, Color captionColor, Color textColor, Color? borderColor = null)
        {
            if (hwnd == IntPtr.Zero) return false;

            try
            {
                // Accessibility: If Windows High Contrast mode is active, preserve the OS high-contrast accessibility scheme
                try
                {
                    if (SystemParameters.HighContrast)
                    {
                        return ResetTitleBarTheme(hwnd);
                    }
                }
                catch
                {
                    // Fall through if SystemParameters is unavailable
                }

                // 1. Immersive dark mode (attribute 20 for Win10 2004+ and Win11; attribute 19 fallback for Win10 1809-1909)
                int useDarkMode = isDark ? 1 : 0;
                int hr = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
                if (hr != 0)
                {
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDarkMode, sizeof(int));
                }

                // 2. Caption color (attribute 35 - supported on Windows 11 build 22000+)
                int captionColorRef = ColorToColorRef(captionColor);
                DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref captionColorRef, sizeof(int));

                // 3. Caption text color (attribute 36 - supported on Windows 11 build 22000+)
                int textColorRef = ColorToColorRef(textColor);
                DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref textColorRef, sizeof(int));

                // 4. Window border color (attribute 34 - supported on Windows 11 build 22000+)
                if (borderColor.HasValue)
                {
                    int borderColorRef = ColorToColorRef(borderColor.Value);
                    DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref borderColorRef, sizeof(int));
                }

                return true;
            }
            catch
            {
                // Graceful fallback for non-Windows platforms or missing dwmapi.dll
                return false;
            }
        }

        /// <summary>
        /// Resets title bar DWM attributes back to Windows default OS caption styling.
        /// Gracefully handles Win10 1809-1909 attribute 19 and Win11 attributes.
        /// </summary>
        public static bool ResetTitleBarTheme(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;

            try
            {
                int defaultMode = 0;
                int hr = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref defaultMode, sizeof(int));
                if (hr != 0)
                {
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref defaultMode, sizeof(int));
                }

                int defaultColor = unchecked((int)DWMWA_COLOR_DEFAULT);
                DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref defaultColor, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref defaultColor, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref defaultColor, sizeof(int));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Applies the theme's title bar attributes to a WPF Window.
        /// </summary>
        public static bool ApplyTitleBarTheme(Window window, ThemePalette palette)
        {
            if (window == null || palette == null) return false;

            try
            {
                if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
                {
                    return window.Dispatcher.Invoke(() => ApplyTitleBarTheme(window, palette));
                }

                var helper = new WindowInteropHelper(window);
                IntPtr hwnd = helper.Handle;
                if (hwnd == IntPtr.Zero)
                {
                    try
                    {
                        hwnd = helper.EnsureHandle();
                    }
                    catch
                    {
                        // Window handle not yet available
                    }
                }

                if (hwnd == IntPtr.Zero) return false;
                return ApplyTitleBarTheme(hwnd, palette);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Resets title bar DWM attributes of a WPF Window back to OS defaults.
        /// </summary>
        public static bool ResetTitleBarTheme(Window window)
        {
            if (window == null) return false;

            try
            {
                if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
                {
                    return window.Dispatcher.Invoke(() => ResetTitleBarTheme(window));
                }

                var helper = new WindowInteropHelper(window);
                IntPtr hwnd = helper.Handle;
                if (hwnd == IntPtr.Zero)
                {
                    try
                    {
                        hwnd = helper.EnsureHandle();
                    }
                    catch
                    {
                        // Window handle not yet available
                    }
                }

                if (hwnd == IntPtr.Zero) return false;
                return ResetTitleBarTheme(hwnd);
            }
            catch
            {
                return false;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        /// <summary>
        /// Restores and activates a window, bringing it reliably to the foreground.
        /// </summary>
        public static void BringWindowToForeground(Window window)
        {
            if (window == null) return;
            try
            {
                if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
                {
                    window.Dispatcher.Invoke(() => BringWindowToForeground(window));
                    return;
                }

                var helper = new WindowInteropHelper(window);
                IntPtr hwnd = helper.Handle;
                if (hwnd != IntPtr.Zero)
                {
                    if (IsIconic(hwnd) || window.WindowState == WindowState.Minimized)
                    {
                        ShowWindow(hwnd, SW_RESTORE);
                        window.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        ShowWindow(hwnd, SW_SHOW);
                    }
                    SetForegroundWindow(hwnd);
                }

                if (window.Visibility != Visibility.Visible)
                {
                    window.Show();
                }
                window.Activate();
                window.Topmost = true;
                window.Topmost = false;
                window.Focus();
            }
            catch
            {
                try
                {
                    if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
                    window.Activate();
                }
                catch { }
            }
        }
    }
}
