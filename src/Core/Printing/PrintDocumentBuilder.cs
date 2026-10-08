using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using MDPlus.Models;

namespace MDPlus.Core.Printing
{
    public sealed class PrintPreviewDocumentHandle : IDisposable
    {
        private bool _disposed;

        public Uri PackageUri { get; }
        public Package Package { get; }
        public MemoryStream Stream { get; }
        public XpsDocument XpsDoc { get; }
        public FixedDocumentSequence DocumentSequence { get; }
        public int PageCount => DocumentSequence.DocumentPaginator.PageCount;

        public PrintPreviewDocumentHandle(Uri packageUri, Package package, MemoryStream stream, XpsDocument xpsDoc, FixedDocumentSequence sequence)
        {
            PackageUri = packageUri;
            Package = package;
            Stream = stream;
            XpsDoc = xpsDoc;
            DocumentSequence = sequence;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { PackageStore.RemovePackage(PackageUri); } catch { }
            try { XpsDoc.Close(); } catch { }
            try { Package.Close(); } catch { }
            try { Stream.Dispose(); } catch { }
        }
    }

    public static class PrintDocumentBuilder
    {
        /// <summary>
        /// Builds an isolated, print-optimized FlowDocument from the given tab and print settings.
        /// </summary>
        public static FlowDocument BuildPrintFlowDocument(DocumentTabItem tab, PrintSettings settings, string? activeEditorText = null)
        {
            if (tab == null) throw new ArgumentNullException(nameof(tab));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            // Select palette: default to Light Paper (ink saver) or current theme
            var palette = settings.Theme == PrintThemeKind.CurrentTheme
                ? ThemeManager.Instance.CurrentPalette
                : ThemePalette.GitHubLight;

            FlowDocument flowDoc;

            if (tab.Format == DocumentFormat.Markdown)
            {
                MarkdownDocument mdDoc;
                if (!string.IsNullOrEmpty(activeEditorText))
                {
                    mdDoc = new MarkdownParser().Parse(activeEditorText);
                }
                else
                {
                    mdDoc = tab.Document;
                }

                var converter = new MarkdownToWpfConverter(tab.DirectoryName, palette, enableLatex: true, enableHtml: true);
                flowDoc = converter.Convert(mdDoc);
            }
            else if (tab.Format is DocumentFormat.Csv or DocumentFormat.Tsv)
            {
                string text = !string.IsNullOrEmpty(activeEditorText) ? activeEditorText : tab.RawMarkdown;
                flowDoc = CsvToFlowDocumentConverter.Convert(text, palette, tab.Format == DocumentFormat.Tsv);
            }
            else if (tab.Format == DocumentFormat.Json)
            {
                string text = !string.IsNullOrEmpty(activeEditorText) ? activeEditorText : tab.RawText;
                flowDoc = JsonToFlowDocumentConverter.Convert(text, palette);
            }
            else
            {
                string text = !string.IsNullOrEmpty(activeEditorText) ? activeEditorText : tab.RawText;
                flowDoc = PlainTextToFlowDocumentConverter.Convert(text, tab.Format, palette);
            }

            // Configure single-column page metrics
            var pageSize = settings.GetPhysicalPageSize();
            flowDoc.PageWidth = pageSize.Width;
            flowDoc.PageHeight = pageSize.Height;
            flowDoc.PagePadding = settings.GetContentPadding();
            flowDoc.ColumnWidth = double.PositiveInfinity; // Enforce single continuous column

            flowDoc.Background = palette.EditorBackgroundBrush;
            flowDoc.Foreground = palette.EditorForegroundBrush;

            return flowDoc;
        }

        /// <summary>
        /// Creates the appropriate DocumentPaginator, applying running headers and footers if enabled.
        /// </summary>
        public static DocumentPaginator CreatePrintPaginator(FlowDocument flowDoc, PrintSettings settings)
        {
            if (flowDoc == null) throw new ArgumentNullException(nameof(flowDoc));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            IDocumentPaginatorSource idp = flowDoc;
            var innerPaginator = idp.DocumentPaginator;
            innerPaginator.PageSize = settings.GetPhysicalPageSize();

            if (settings.IncludeHeadersAndFooters)
            {
                return new HeaderFooterDocumentPaginator(innerPaginator, settings);
            }

            return innerPaginator;
        }

        /// <summary>
        /// Writes the FlowDocument to an in-memory XPS package for 100% WYSIWYG preview in DocumentViewer.
        /// </summary>
        public static PrintPreviewDocumentHandle CreateInmemoryXpsDocument(FlowDocument flowDoc, PrintSettings settings)
        {
            if (flowDoc == null) throw new ArgumentNullException(nameof(flowDoc));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var paginator = CreatePrintPaginator(flowDoc, settings);

            var ms = new MemoryStream();
            var packageUri = new Uri($"memorystream://preview_{Guid.NewGuid():N}.xps");
            var package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
            PackageStore.AddPackage(packageUri, package);

            var xpsDoc = new XpsDocument(package, CompressionOption.NotCompressed, packageUri.AbsoluteUri);
            paginator.ComputePageCount();
            var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
            writer.Write(paginator);
            package.Flush();

            var sequence = xpsDoc.GetFixedDocumentSequence();
            return new PrintPreviewDocumentHandle(packageUri, package, ms, xpsDoc, sequence);
        }
    }
}
