using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MDPlus.Controls
{
    public class FindEventArgs : EventArgs
    {
        public string SearchText { get; set; } = string.Empty;
        public bool MatchCase { get; set; }
        public bool Forward { get; set; }
    }

    public partial class FindBar : UserControl
    {
        public event EventHandler<FindEventArgs>? FindRequested;
        public event EventHandler? Closed;

        public FindBar()
        {
            InitializeComponent();
        }

        public void FocusSearchBox()
        {
            FindTextBox.Focus();
            FindTextBox.SelectAll();
        }

        public void SetMatchCount(int current, int total)
        {
            if (string.IsNullOrEmpty(FindTextBox.Text))
            {
                MatchCountText.Text = string.Empty;
            }
            else if (total == 0)
            {
                MatchCountText.Text = "No matches";
                MatchCountText.Foreground = new SolidColorBrush(Color.FromRgb(248, 81, 73));
            }
            else
            {
                MatchCountText.Text = $"{current} of {total}";
                MatchCountText.Foreground = new SolidColorBrush(Color.FromRgb(139, 148, 158));
            }
        }

        public void ApplyTheme(bool isDark)
        {
            if (isDark)
            {
                OuterBorder.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));
                OuterBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                FindTextBox.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                FindTextBox.Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                FindTextBox.BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                PreviousButton.Background = new SolidColorBrush(Color.FromRgb(50, 50, 52));
                PreviousButton.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                PreviousButton.BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                NextButton.Background = new SolidColorBrush(Color.FromRgb(50, 50, 52));
                NextButton.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                NextButton.BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                MatchCaseCheckBox.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                CloseButton.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));
            }
            else
            {
                OuterBorder.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                OuterBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                FindTextBox.Background = Brushes.White;
                FindTextBox.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                FindTextBox.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                PreviousButton.Background = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                PreviousButton.Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                PreviousButton.BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                NextButton.Background = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                NextButton.Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                NextButton.BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                MatchCaseCheckBox.Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                CloseButton.Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            }
        }

        private void TriggerFind(bool forward)
        {
            if (string.IsNullOrEmpty(FindTextBox.Text)) return;

            FindRequested?.Invoke(this, new FindEventArgs
            {
                SearchText = FindTextBox.Text,
                MatchCase = MatchCaseCheckBox.IsChecked == true,
                Forward = forward
            });
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerFind(true);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerFind(false);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void FindTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TriggerFind(true);
        }

        private void FindTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool forward = (Keyboard.Modifiers & ModifierKeys.Shift) == 0;
                TriggerFind(forward);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Visibility = Visibility.Collapsed;
                Closed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void MatchCase_Changed(object sender, RoutedEventArgs e)
        {
            TriggerFind(true);
        }
    }
}
