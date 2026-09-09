using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using MDPlus.Core;

namespace MDPlus.Models
{
    public enum ViewDisplayMode
    {
        Rendered,
        Raw,
        Split
    }

    public class DocumentTabItem : INotifyPropertyChanged
    {
        private string _filePath = string.Empty;
        private string _title = "Untitled";
        private string _rawMarkdown = string.Empty;
        private MarkdownDocument _document = new MarkdownDocument();
        private FlowDocument? _flowDocument;
        private List<HeadingItem> _headings = new List<HeadingItem>();
        private ViewDisplayMode _viewMode = ViewDisplayMode.Rendered;
        private double _zoom = 100.0;
        private string _encodingName = "UTF-8";
        private string _lineEndingName = "CRLF";

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(DirectoryName));
                }
            }
        }

        public string FileName => string.IsNullOrEmpty(_filePath) ? _title : Path.GetFileName(_filePath);
        public string DirectoryName => string.IsNullOrEmpty(_filePath) ? string.Empty : Path.GetDirectoryName(_filePath) ?? string.Empty;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        public string RawMarkdown
        {
            get => _rawMarkdown;
            set
            {
                if (_rawMarkdown != value)
                {
                    _rawMarkdown = value;
                    OnPropertyChanged();
                }
            }
        }

        public MarkdownDocument Document
        {
            get => _document;
            set
            {
                if (_document != value)
                {
                    _document = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatsText));
                }
            }
        }

        public FlowDocument? FlowDocument
        {
            get => _flowDocument;
            set
            {
                if (_flowDocument != value)
                {
                    _flowDocument = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<HeadingItem> Headings
        {
            get => _headings;
            set
            {
                if (_headings != value)
                {
                    _headings = value;
                    OnPropertyChanged();
                }
            }
        }

        public ViewDisplayMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (_viewMode != value)
                {
                    _viewMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsRenderedVisible));
                    OnPropertyChanged(nameof(IsRawVisible));
                }
            }
        }

        public bool IsRenderedVisible => _viewMode == ViewDisplayMode.Rendered || _viewMode == ViewDisplayMode.Split;
        public bool IsRawVisible => _viewMode == ViewDisplayMode.Raw || _viewMode == ViewDisplayMode.Split;

        public double Zoom
        {
            get => _zoom;
            set
            {
                double clamped = Math.Clamp(value, 50.0, 300.0);
                if (Math.Abs(_zoom - clamped) > 0.01)
                {
                    _zoom = clamped;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ZoomText));
                }
            }
        }

        public string ZoomText => $"{(int)_zoom}%";

        public string EncodingName
        {
            get => _encodingName;
            set
            {
                if (_encodingName != value)
                {
                    _encodingName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LineEndingName
        {
            get => _lineEndingName;
            set
            {
                if (_lineEndingName != value)
                {
                    _lineEndingName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatsText => $"{Document.WordCount:N0} words • {Document.CharacterCount:N0} chars • {Document.ReadingTimeMinutes} min read";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
