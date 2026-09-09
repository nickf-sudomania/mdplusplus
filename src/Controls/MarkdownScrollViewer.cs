using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MDPlus.Controls
{
    public class MarkdownScrollViewer : FlowDocumentScrollViewer
    {
        private readonly List<TextRange> _highlightRanges = new List<TextRange>();
        private int _currentMatchIndex = -1;
        private string _lastSearchText = string.Empty;
        private bool _lastMatchCase = false;

        public MarkdownScrollViewer()
        {
            IsToolBarVisible = false;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        private ScrollViewer? _internalScrollViewer;

        private ScrollViewer? InternalScrollViewer
        {
            get
            {
                if (_internalScrollViewer != null) return _internalScrollViewer;
                ApplyTemplate();
                _internalScrollViewer = Template?.FindName("PART_ContentHost", this) as ScrollViewer ?? FindVisualChild<ScrollViewer>(this);
                return _internalScrollViewer;
            }
        }

        public double VerticalOffset => InternalScrollViewer?.VerticalOffset ?? 0.0;
        public double ExtentHeight => InternalScrollViewer?.ExtentHeight ?? 0.0;
        public double ViewportHeight => InternalScrollViewer?.ViewportHeight ?? 0.0;

        public void ScrollToVerticalOffset(double offset)
        {
            InternalScrollViewer?.ScrollToVerticalOffset(offset);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _internalScrollViewer = Template?.FindName("PART_ContentHost", this) as ScrollViewer ?? FindVisualChild<ScrollViewer>(this);
        }

        private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DocumentProperty)
            {
                ClearHighlights();
            }
        }

        public bool ScrollToAnchor(string anchor)
        {
            if (Document == null || string.IsNullOrEmpty(anchor)) return false;

            return FindAndScrollToAnchor(Document.Blocks, anchor);
        }

        private bool FindAndScrollToAnchor(BlockCollection blocks, string anchor)
        {
            foreach (var block in blocks)
            {
                if (block is Paragraph p && p.Tag is string tagStr && tagStr.Equals(anchor, StringComparison.OrdinalIgnoreCase))
                {
                    p.BringIntoView();
                    return true;
                }

                if (block is Section s)
                {
                    if (FindAndScrollToAnchor(s.Blocks, anchor)) return true;
                }

                if (block is List l)
                {
                    foreach (var item in l.ListItems)
                    {
                        if (FindAndScrollToAnchor(item.Blocks, anchor)) return true;
                    }
                }
            }
            return false;
        }

        public (int current, int total) SearchText(string searchText, bool matchCase, bool forward)
        {
            if (Document == null || string.IsNullOrWhiteSpace(searchText))
            {
                ClearHighlights();
                return (0, 0);
            }

            // If query or options changed or no highlights yet, rebuild matches
            if (_highlightRanges.Count == 0 || !string.Equals(_lastSearchText, searchText, StringComparison.Ordinal) || _lastMatchCase != matchCase)
            {
                _lastSearchText = searchText;
                _lastMatchCase = matchCase;
                FindAllMatches(searchText, matchCase);
            }

            if (_highlightRanges.Count == 0)
            {
                return (0, 0);
            }

            if (forward)
            {
                _currentMatchIndex++;
                if (_currentMatchIndex >= _highlightRanges.Count) _currentMatchIndex = 0;
            }
            else
            {
                _currentMatchIndex--;
                if (_currentMatchIndex < 0) _currentMatchIndex = _highlightRanges.Count - 1;
            }

            // Scroll current match into view and focus selection
            var range = _highlightRanges[_currentMatchIndex];
            Selection?.Select(range.Start, range.End);

            if (range.Start.Parent is FrameworkContentElement elem)
            {
                elem.BringIntoView();
            }

            return (_currentMatchIndex + 1, _highlightRanges.Count);
        }

        public void ClearHighlights()
        {
            _highlightRanges.Clear();
            _currentMatchIndex = -1;
            _lastSearchText = string.Empty;
            if (Document?.ContentStart != null && Selection != null)
            {
                Selection.Select(Document.ContentStart, Document.ContentStart);
            }
        }

        private void FindAllMatches(string searchText, bool matchCase)
        {
            _highlightRanges.Clear();
            _currentMatchIndex = -1;
            if (Document == null) return;

            var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            TextPointer position = Document.ContentStart;

            while (position != null && position.CompareTo(Document.ContentEnd) < 0)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);
                    int indexInRun = 0;

                    while (indexInRun < textRun.Length)
                    {
                        int matchIndex = textRun.IndexOf(searchText, indexInRun, comparison);
                        if (matchIndex == -1) break;

                        TextPointer start = position.GetPositionAtOffset(matchIndex);
                        TextPointer end = position.GetPositionAtOffset(matchIndex + searchText.Length);

                        if (start != null && end != null)
                        {
                            _highlightRanges.Add(new TextRange(start, end));
                        }

                        indexInRun = matchIndex + Math.Max(1, searchText.Length);
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
        }
    }
}
