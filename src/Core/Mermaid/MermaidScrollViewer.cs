using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MDPlus.Core.Mermaid
{
    /// <summary>
    /// Custom horizontal ScrollViewer for vector Mermaid flowcharts.
    /// Provides smooth horizontal panning with Shift+Wheel, while bypassing
    /// vertical wheel events when Shift is not pressed so they bubble
    /// seamlessly up to MarkdownScrollViewer for uninterrupted 60 FPS document scrolling.
    /// </summary>
    public sealed class MermaidScrollViewer : ScrollViewer
    {
        public MermaidScrollViewer()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            Focusable = false;
            BorderThickness = new Thickness(0);
            Background = Brushes.Transparent;
            Padding = new Thickness(0);
            CanContentScroll = false;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                double step = Math.Max(40.0, Math.Abs(e.Delta) * 0.5);
                if (e.Delta < 0)
                {
                    ScrollToHorizontalOffset(HorizontalOffset + step);
                }
                else if (e.Delta > 0)
                {
                    ScrollToHorizontalOffset(HorizontalOffset - step);
                }
                e.Handled = true;
                return;
            }

            // Intentionally bypass base.OnMouseWheel(e) when Shift is not pressed.
            // This leaves e.Handled = false so the vertical mouse wheel event bubbles up
            // to the parent RichTextBox / MarkdownScrollViewer for uninterrupted 60 FPS document scrolling.
        }
    }
}
