using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MDPlus.Core.Printing
{
    /// <summary>
    /// DocumentPaginator decorator that slices an inner paginator to a specific user page range.
    /// </summary>
    public class PageRangeDocumentPaginator : DocumentPaginator
    {
        private readonly DocumentPaginator _inner;
        private readonly int _startPage; // 0-based
        private readonly int _endPage;   // 0-based inclusive
        private readonly int _count;

        public PageRangeDocumentPaginator(DocumentPaginator inner, PageRange pageRange)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            
            int max = Math.Max(1, _inner.PageCount);
            _startPage = Math.Clamp(pageRange.PageFrom - 1, 0, max - 1);
            _endPage = Math.Clamp(pageRange.PageTo - 1, _startPage, max - 1);
            _count = (_endPage - _startPage) + 1;
        }

        public override bool IsPageCountValid => _inner.IsPageCountValid;

        public override int PageCount => _count;

        public override Size PageSize
        {
            get => _inner.PageSize;
            set => _inner.PageSize = value;
        }

        public override IDocumentPaginatorSource Source => _inner.Source;

        public override DocumentPage GetPage(int pageNumber)
        {
            int mappedPage = _startPage + pageNumber;
            return _inner.GetPage(mappedPage);
        }
    }
}
