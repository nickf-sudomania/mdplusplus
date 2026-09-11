using System;
using System.IO;
using MDPlus.Models;

namespace MDPlus.Core
{
    /// <summary>
    /// Utility methods for detecting document format, badges, display names, and file dialog filters.
    /// </summary>
    public static class DocumentFormatHelper
    {
        public static DocumentFormat DetectFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return DocumentFormat.Markdown;
            }

            string ext = Path.GetExtension(filePath);
            return DetectFromExtension(ext);
        }

        public static DocumentFormat DetectFromExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return DocumentFormat.PlainText;
            }

            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = "." + extension;
            }

            extension = extension.ToLowerInvariant();

            return extension switch
            {
                ".md" or ".markdown" or ".mdown" or ".mkd" => DocumentFormat.Markdown,
                ".txt" => DocumentFormat.PlainText,
                ".log" => DocumentFormat.Log,
                ".csv" => DocumentFormat.Csv,
                ".tsv" => DocumentFormat.Tsv,
                ".json" => DocumentFormat.Json,
                ".ini" => DocumentFormat.Ini,
                ".cfg" => DocumentFormat.Cfg,
                ".yaml" or ".yml" => DocumentFormat.Yaml,
                ".xml" => DocumentFormat.Xml,
                _ => DocumentFormat.PlainText
            };
        }

        public static string GetFormatDisplayName(DocumentFormat format) => format switch
        {
            DocumentFormat.Markdown => "Markdown Document",
            DocumentFormat.PlainText => "Plain Text",
            DocumentFormat.Log => "Log File",
            DocumentFormat.Csv => "CSV Document",
            DocumentFormat.Tsv => "TSV Document",
            DocumentFormat.Json => "JSON Document",
            DocumentFormat.Ini => "INI Configuration",
            DocumentFormat.Cfg => "Configuration File",
            DocumentFormat.Yaml => "YAML Document",
            DocumentFormat.Xml => "XML Document",
            _ => "Text Document"
        };

        public static string GetFormatBadge(DocumentFormat format) => format switch
        {
            DocumentFormat.Markdown => "MD",
            DocumentFormat.PlainText => "TXT",
            DocumentFormat.Log => "LOG",
            DocumentFormat.Csv => "CSV",
            DocumentFormat.Tsv => "TSV",
            DocumentFormat.Json => "JSON",
            DocumentFormat.Ini => "INI",
            DocumentFormat.Cfg => "CFG",
            DocumentFormat.Yaml => "YAML",
            DocumentFormat.Xml => "XML",
            _ => "TXT"
        };

        public static string GetDefaultExtension(DocumentFormat format) => format switch
        {
            DocumentFormat.Markdown => ".md",
            DocumentFormat.PlainText => ".txt",
            DocumentFormat.Log => ".log",
            DocumentFormat.Csv => ".csv",
            DocumentFormat.Tsv => ".tsv",
            DocumentFormat.Json => ".json",
            DocumentFormat.Ini => ".ini",
            DocumentFormat.Cfg => ".cfg",
            DocumentFormat.Yaml => ".yaml",
            DocumentFormat.Xml => ".xml",
            _ => ".txt"
        };

        public static string GetOpenFileDialogFilter() =>
            "All Supported Files (*.md;*.txt;*.log;*.csv;*.tsv;*.json;*.ini;*.cfg;*.yaml;*.yml;*.xml)|*.md;*.markdown;*.mdown;*.mkd;*.txt;*.log;*.csv;*.tsv;*.json;*.ini;*.cfg;*.yaml;*.yml;*.xml|" +
            "Markdown Files (*.md;*.markdown;*.mdown;*.mkd)|*.md;*.markdown;*.mdown;*.mkd|" +
            "Text & Log Files (*.txt;*.log)|*.txt;*.log|" +
            "Tabular Data (*.csv;*.tsv)|*.csv;*.tsv|" +
            "JSON Files (*.json)|*.json|" +
            "Configuration Files (*.ini;*.cfg;*.yaml;*.yml;*.xml)|*.ini;*.cfg;*.yaml;*.yml;*.xml|" +
            "All Files (*.*)|*.*";

        public static string GetSaveFileDialogFilter(DocumentFormat format) => format switch
        {
            DocumentFormat.Csv => "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            DocumentFormat.Tsv => "TSV Files (*.tsv)|*.tsv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            DocumentFormat.Json => "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            DocumentFormat.Log => "Log Files (*.log)|*.log|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            DocumentFormat.Ini => "INI Files (*.ini)|*.ini|Configuration Files (*.cfg;*.ini)|*.cfg;*.ini|All Files (*.*)|*.*",
            DocumentFormat.Cfg => "Configuration Files (*.cfg)|*.cfg|INI Files (*.ini)|*.ini|All Files (*.*)|*.*",
            DocumentFormat.Yaml => "YAML Files (*.yaml;*.yml)|*.yaml;*.yml|All Files (*.*)|*.*",
            DocumentFormat.Xml => "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            DocumentFormat.PlainText => "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            _ => "Markdown Files (*.md)|*.md|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        public static bool IsTabular(DocumentFormat format) =>
            format == DocumentFormat.Csv || format == DocumentFormat.Tsv;

        public static bool IsConfig(DocumentFormat format) =>
            format is DocumentFormat.Ini or DocumentFormat.Cfg or DocumentFormat.Yaml or DocumentFormat.Xml;

        /// <summary>
        /// Serializes a FlowDocument back into raw text based on the specified DocumentFormat.
        /// Dispatches to MarkdownSerializer, CsvSerializer, JsonToFlowDocumentConverter, or PlainTextToFlowDocumentConverter.
        /// </summary>
        public static string SerializeFlowDocument(System.Windows.Documents.FlowDocument? doc, DocumentFormat format, string? lineEnding = null)
        {
            if (doc == null) return string.Empty;

            return format switch
            {
                DocumentFormat.Markdown => MarkdownSerializer.Serialize(doc),
                DocumentFormat.Csv => CsvSerializer.Serialize(doc, ',', lineEnding),
                DocumentFormat.Tsv => CsvSerializer.Serialize(doc, '\t', lineEnding),
                DocumentFormat.Json => JsonToFlowDocumentConverter.Serialize(doc, lineEnding),
                _ => PlainTextToFlowDocumentConverter.Serialize(doc, format, lineEnding)
            };
        }
    }
}
