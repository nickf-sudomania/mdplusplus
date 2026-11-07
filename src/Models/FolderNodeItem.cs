using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MDPlus.Core;

namespace MDPlus.Models
{
    public enum SidebarMode
    {
        Outline,
        Files
    }

    public class FolderNodeItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _fullPath = string.Empty;
        private string _relativePath = string.Empty;
        private bool _isDirectory;
        private int _depth;
        private bool _isExpanded = true;
        private bool _isActiveDocument;
        private DocumentFormat _format = DocumentFormat.Markdown;
        private List<FolderNodeItem> _children = new List<FolderNodeItem>();

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RelativePath
        {
            get => _relativePath;
            set
            {
                if (_relativePath != value)
                {
                    _relativePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set
            {
                if (_isDirectory != value)
                {
                    _isDirectory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconGlyph));
                    OnPropertyChanged(nameof(BadgeText));
                    OnPropertyChanged(nameof(ExpanderGlyph));
                }
            }
        }

        public int Depth
        {
            get => _depth;
            set
            {
                if (_depth != value)
                {
                    _depth = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IndentMargin));
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconGlyph));
                    OnPropertyChanged(nameof(ExpanderGlyph));
                }
            }
        }

        public bool IsActiveDocument
        {
            get => _isActiveDocument;
            set
            {
                if (_isActiveDocument != value)
                {
                    _isActiveDocument = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ItemFontWeight));
                }
            }
        }

        public DocumentFormat Format
        {
            get => _format;
            set
            {
                if (_format != value)
                {
                    _format = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BadgeText));
                }
            }
        }

        public string ExpanderGlyph => IsDirectory ? (IsExpanded ? "▾" : "▸") : string.Empty;

        public string IconGlyph => IsDirectory ? (IsExpanded ? "📂" : "📁") : "📄";

        public string BadgeText => IsDirectory ? "DIR" : DocumentFormatHelper.GetFormatBadge(Format);

        public Thickness IndentMargin => new Thickness(Depth * 14, 2, 4, 2);

        public FontWeight ItemFontWeight => IsDirectory || IsActiveDocument ? FontWeights.SemiBold : FontWeights.Normal;

        public List<FolderNodeItem> Children
        {
            get => _children;
            set
            {
                if (_children != value)
                {
                    _children = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
