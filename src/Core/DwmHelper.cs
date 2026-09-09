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
            return ApplyTitleBarTheme(hwnd, palette.IsDark, palette.MenuBackgroundColor, palette.MenuForegroundColor);
        }

        /// <summary>
        /// Applies title bar styling using explicit dark mode flag, caption color, and text color.
        /// Gracefully falls back on older Windows builds where attributes 35/36 or 20 are unsupported.
        /// </summary>
        public static bool ApplyTitleBarTheme(IntPtr hwnd, bool isDark, Color captionColor, Color textColor)
        {
            if (hwnd == IntPtr.Zero) return false;

            try
            {
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

                return true;
            }
            catch
            {
                // Graceful fallback for non-Windows platforms or missing dwmapi.dll
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
    }
}
