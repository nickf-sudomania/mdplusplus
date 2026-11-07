using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MDPlus.Models;

namespace MDPlus.Core
{
    /// <summary>
    /// Provides fast directory scanning, tree flattening, file filtering, and
    /// combined multi-file document generation ("See It In One Go" folder view).
    /// </summary>
    public static class FolderService
    {
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".md", ".markdown", ".mdown", ".mkd",
            ".txt", ".log",
            ".csv", ".tsv",
            ".json",
            ".ini", ".cfg", ".yaml", ".yml", ".xml"
        };

        private static readonly HashSet<string> MarkdownExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".md", ".markdown", ".mdown", ".mkd"
        };

        private static readonly HashSet<string> ExcludedDirectoryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", ".hg", ".svn", ".vs", ".idea", ".agents", ".github",
            "node_modules", "bin", "obj", "packages", "TestResults", "__pycache__"
        };

        public static bool IsSupportedDocumentFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            string ext = Path.GetExtension(filePath);
            return !string.IsNullOrEmpty(ext) && SupportedExtensions.Contains(ext);
        }

        public static bool IsMarkdownFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            string ext = Path.GetExtension(filePath);
            return !string.IsNullOrEmpty(ext) && MarkdownExtensions.Contains(ext);
        }

        /// <summary>
        /// Scans a root directory and returns a hierarchical tree of FolderNodeItems.
        /// Empty subdirectories that contain no matching files are pruned automatically.
        /// </summary>
        public static List<FolderNodeItem> ScanFolderTree(
            string rootPath,
            bool markdownOnly = false,
            int maxDepth = 4,
            int maxEntries = 1000)
        {
            var result = new List<FolderNodeItem>();
            if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
            {
                return result;
            }

            string fullRoot;
            try
            {
                fullRoot = Path.GetFullPath(rootPath);
            }
            catch
            {
                return result;
            }

            int entryCount = 0;
            ScanDirectoryRecursive(fullRoot, fullRoot, 0, maxDepth, markdownOnly, maxEntries, ref entryCount, result);
            return result;
        }

        private static void ScanDirectoryRecursive(
            string rootPath,
            string currentDir,
            int depth,
            int maxDepth,
            bool markdownOnly,
            int maxEntries,
            ref int entryCount,
            List<FolderNodeItem> targetList)
        {
            if (entryCount >= maxEntries || depth > maxDepth)
            {
                return;
            }

            // 1. Subdirectories (if within maxDepth)
            if (depth < maxDepth)
            {
                string[] subDirs = Array.Empty<string>();
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch
                {
                    // Ignore inaccessible directories
                }

                Array.Sort(subDirs, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a), Path.GetFileName(b)));

                foreach (string subDir in subDirs)
                {
                    if (entryCount >= maxEntries) break;

                    string dirName = Path.GetFileName(subDir);
                    if (string.IsNullOrEmpty(dirName) || dirName.StartsWith(".", StringComparison.Ordinal) || ExcludedDirectoryNames.Contains(dirName))
                    {
                        continue;
                    }

                    try
                    {
                        var attr = File.GetAttributes(subDir);
                        if ((attr & FileAttributes.Hidden) != 0 || (attr & FileAttributes.System) != 0)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    var childNodes = new List<FolderNodeItem>();
                    ScanDirectoryRecursive(rootPath, subDir, depth + 1, maxDepth, markdownOnly, maxEntries, ref entryCount, childNodes);

                    // Only include directory node if it contains at least one supported file (directly or nested)
                    if (childNodes.Count > 0)
                    {
                        string relPath = GetSafeRelativePath(rootPath, subDir);
                        var dirNode = new FolderNodeItem
                        {
                            Name = dirName,
                            FullPath = subDir,
                            RelativePath = relPath,
                            IsDirectory = true,
                            Depth = depth,
                            IsExpanded = depth < 1, // Expand top-level subfolders by default
                            Children = childNodes
                        };
                        targetList.Add(dirNode);
                    }
                }
            }

            // 2. Files in currentDir
            string[] files = Array.Empty<string>();
            try
            {
                files = Directory.GetFiles(currentDir);
            }
            catch
            {
                // Ignore inaccessible files
            }

            Array.Sort(files, (a, b) => StringComparer.OrdinalIgnoreCase.Compare(Path.GetFileName(a), Path.GetFileName(b)));

            foreach (string file in files)
            {
                if (entryCount >= maxEntries) break;

                if (markdownOnly)
                {
                    if (!IsMarkdownFile(file)) continue;
                }
                else
                {
                    if (!IsSupportedDocumentFile(file)) continue;
                }

                string fileName = Path.GetFileName(file);
                if (string.IsNullOrEmpty(fileName) || fileName.StartsWith(".", StringComparison.Ordinal))
                {
                    continue;
                }

                string relPath = GetSafeRelativePath(rootPath, file);
                var format = DocumentFormatHelper.DetectFromPath(file);

                targetList.Add(new FolderNodeItem
                {
                    Name = fileName,
                    FullPath = file,
                    RelativePath = relPath,
                    IsDirectory = false,
                    Depth = depth,
                    Format = format
                });
                entryCount++;
            }
        }

        /// <summary>
        /// Flattens the hierarchical folder tree into a visible list for ListBox binding,
        /// respecting folder expansion state (when no filter is active) or matching searchFilter across all levels.
        /// </summary>
        public static List<FolderNodeItem> FlattenVisibleNodes(
            IEnumerable<FolderNodeItem>? rootNodes,
            string? searchFilter = null,
            string? activeFilePath = null)
        {
            var flat = new List<FolderNodeItem>();
            if (rootNodes == null) return flat;

            string filter = searchFilter?.Trim() ?? string.Empty;
            bool hasFilter = !string.IsNullOrEmpty(filter);

            foreach (var node in rootNodes)
            {
                FlattenNodeRecursive(node, filter, hasFilter, activeFilePath, flat);
            }

            return flat;
        }

        private static void FlattenNodeRecursive(
            FolderNodeItem node,
            string filter,
            bool hasFilter,
            string? activeFilePath,
            List<FolderNodeItem> target)
        {
            node.IsActiveDocument = !node.IsDirectory &&
                                    !string.IsNullOrEmpty(activeFilePath) &&
                                    string.Equals(node.FullPath, activeFilePath, StringComparison.OrdinalIgnoreCase);

            if (!hasFilter)
            {
                target.Add(node);
                if (node.IsDirectory && node.IsExpanded && node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        FlattenNodeRecursive(child, filter, false, activeFilePath, target);
                    }
                }
            }
            else
            {
                if (node.IsDirectory)
                {
                    var matchingChildren = new List<FolderNodeItem>();
                    if (node.Children != null)
                    {
                        foreach (var child in node.Children)
                        {
                            FlattenNodeRecursive(child, filter, true, activeFilePath, matchingChildren);
                        }
                    }

                    if (matchingChildren.Count > 0)
                    {
                        target.Add(node);
                        target.AddRange(matchingChildren);
                    }
                }
                else
                {
                    if (node.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        node.RelativePath.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        target.Add(node);
                    }
                }
            }
        }

        /// <summary>
        /// Collects all file paths in traversal order from a folder tree.
        /// </summary>
        public static List<string> GetAllFilePaths(IEnumerable<FolderNodeItem>? nodes)
        {
            var files = new List<string>();
            if (nodes == null) return files;

            foreach (var node in nodes)
            {
                CollectFilesRecursive(node, files);
            }

            return files;
        }

        private static void CollectFilesRecursive(FolderNodeItem node, List<string> files)
        {
            if (node.IsDirectory)
            {
                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        CollectFilesRecursive(child, files);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(node.FullPath))
            {
                files.Add(node.FullPath);
            }
        }

        /// <summary>
        /// Ensures that all ancestor directory nodes containing the specified file path are expanded.
        /// </summary>
        public static bool ExpandToFile(IEnumerable<FolderNodeItem>? nodes, string? filePath)
        {
            if (nodes == null || string.IsNullOrWhiteSpace(filePath)) return false;

            bool foundAny = false;
            foreach (var node in nodes)
            {
                if (ExpandNodeRecursive(node, filePath))
                {
                    foundAny = true;
                }
            }
            return foundAny;
        }

        private static bool ExpandNodeRecursive(FolderNodeItem node, string filePath)
        {
            if (!node.IsDirectory)
            {
                return string.Equals(node.FullPath, filePath, StringComparison.OrdinalIgnoreCase);
            }

            bool foundInChild = false;
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (ExpandNodeRecursive(child, filePath))
                    {
                        foundInChild = true;
                    }
                }
            }

            if (foundInChild)
            {
                node.IsExpanded = true;
            }
            return foundInChild;
        }

        /// <summary>
        /// Builds a single combined Markdown document ("See It In One Go") from all supported files in a folder.
        /// Each file is rendered under its own top-level heading so the Table of Contents outline allows instant
        /// navigation across all files and their internal sections.
        /// </summary>
        public static (string CombinedMarkdown, List<string> IncludedFiles) BuildCombinedFolderMarkdown(
            string folderPath,
            bool markdownOnly = false,
            bool includeSubfolders = true,
            int maxFiles = 100)
        {
            var includedFiles = new List<string>();
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return (string.Empty, includedFiles);
            }

            string fullFolder = Path.GetFullPath(folderPath);
            string folderName = Path.GetFileName(fullFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrEmpty(folderName))
            {
                folderName = fullFolder;
            }

            var tree = ScanFolderTree(fullFolder, markdownOnly: markdownOnly, maxDepth: includeSubfolders ? 3 : 0, maxEntries: maxFiles);
            var allFiles = GetAllFilePaths(tree).Take(maxFiles).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"# 📂 Folder View: {folderName}");
            sb.AppendLine();
            sb.AppendLine("> [!NOTE]");
            sb.AppendLine($"> **Folder Path:** `{fullFolder}`");
            sb.AppendLine($"> **Files Displayed Together:** {allFiles.Count} {(markdownOnly ? "(Markdown files)" : "(All supported files)")}");
            sb.AppendLine();

            if (allFiles.Count == 0)
            {
                sb.AppendLine("*No supported documents were found in this folder.*");
                return (sb.ToString(), includedFiles);
            }

            // Build quick-jump index table
            sb.AppendLine("## 📑 Folder Contents Overview");
            sb.AppendLine();
            sb.AppendLine("| # | File | Format | Size |");
            sb.AppendLine("| :--- | :--- | :---: | ---: |");

            for (int i = 0; i < allFiles.Count; i++)
            {
                string filePath = allFiles[i];
                string relPath = GetSafeRelativePath(fullFolder, filePath);
                var format = DocumentFormatHelper.DetectFromPath(filePath);
                string badge = DocumentFormatHelper.GetFormatBadge(format);
                string sizeText = GetFormattedFileSize(filePath);
                string headingText = $"📄 {relPath}";
                string anchor = GenerateHeadingAnchor(headingText);

                sb.AppendLine($"| {i + 1} | [{EscapeTablePipe(relPath)}](#{anchor}) | `{badge}` | {sizeText} |");
            }

            sb.AppendLine();

            // Append each file's content
            for (int i = 0; i < allFiles.Count; i++)
            {
                string filePath = allFiles[i];
                string relPath = GetSafeRelativePath(fullFolder, filePath);
                var format = DocumentFormatHelper.DetectFromPath(filePath);
                string formatName = DocumentFormatHelper.GetFormatDisplayName(format);

                string content;
                try
                {
                    content = File.ReadAllText(filePath);
                }
                catch (Exception ex)
                {
                    content = $"*Unable to read file: {ex.Message}*";
                }

                includedFiles.Add(filePath);

                sb.AppendLine("---");
                sb.AppendLine();
                sb.AppendLine($"# 📄 {relPath}");
                sb.AppendLine();
                sb.AppendLine($"*Path: `{relPath}` • Format: **{formatName}** • [Open Standalone File]({EscapeMarkdownLinkUrl(filePath)})*");
                sb.AppendLine();

                if (format == DocumentFormat.Markdown)
                {
                    sb.AppendLine(DemoteMarkdownHeadings(content));
                }
                else
                {
                    string fenceLang = GetFenceLanguageForFormat(format);
                    string fence = GetSafeCodeFence(content);
                    sb.AppendLine($"{fence}{fenceLang}");
                    sb.AppendLine(content.TrimEnd('\r', '\n'));
                    sb.AppendLine(fence);
                }

                sb.AppendLine();
            }

            return (sb.ToString(), includedFiles);
        }

        /// <summary>
        /// Demotes ATX headings (# -> ##, ## -> ###, up to ######) outside of fenced code blocks
        /// so each file's top-level # 📄 {RelativePath} header acts as the parent H1 in the Outline.
        /// Also strips leading YAML frontmatter blocks so mid-document --- delimiters aren't misparsed.
        /// </summary>
        internal static string DemoteMarkdownHeadings(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return string.Empty;

            string normalized = markdown.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] lines = normalized.Split('\n');
            var sb = new StringBuilder(markdown.Length + 64);

            int startLine = 0;
            // Strip leading YAML frontmatter if present
            if (lines.Length > 2 && lines[0].Trim() == "---")
            {
                for (int i = 1; i < Math.Min(lines.Length, 50); i++)
                {
                    string trimmed = lines[i].Trim();
                    if (trimmed == "---" || trimmed == "...")
                    {
                        startLine = i + 1;
                        break;
                    }
                }
            }

            bool inCodeFence = false;
            char fenceChar = '\0';
            int fenceLen = 0;

            for (int i = startLine; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmedStart = line.TrimStart();

                if (trimmedStart.StartsWith("```", StringComparison.Ordinal) ||
                    trimmedStart.StartsWith("~~~", StringComparison.Ordinal))
                {
                    char ch = trimmedStart[0];
                    int count = 0;
                    while (count < trimmedStart.Length && trimmedStart[count] == ch)
                    {
                        count++;
                    }

                    if (!inCodeFence)
                    {
                        inCodeFence = true;
                        fenceChar = ch;
                        fenceLen = count;
                    }
                    else if (ch == fenceChar && count >= fenceLen && string.IsNullOrWhiteSpace(trimmedStart.Substring(count)))
                    {
                        inCodeFence = false;
                    }

                    sb.AppendLine(line);
                    continue;
                }

                if (!inCodeFence && trimmedStart.StartsWith("#", StringComparison.Ordinal))
                {
                    int hashes = 0;
                    while (hashes < trimmedStart.Length && trimmedStart[hashes] == '#')
                    {
                        hashes++;
                    }

                    if (hashes >= 1 && hashes <= 6 && (hashes == trimmedStart.Length || trimmedStart[hashes] == ' ' || trimmedStart[hashes] == '\t'))
                    {
                        int newLevel = Math.Min(6, hashes + 1);
                        sb.Append(new string('#', newLevel));
                        sb.AppendLine(trimmedStart.Substring(hashes));
                        continue;
                    }
                }

                sb.AppendLine(line);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string GetFenceLanguageForFormat(DocumentFormat format) => format switch
        {
            DocumentFormat.Json => "json",
            DocumentFormat.Yaml => "yaml",
            DocumentFormat.Xml => "xml",
            DocumentFormat.Ini or DocumentFormat.Cfg => "ini",
            DocumentFormat.Csv or DocumentFormat.Tsv => "csv",
            _ => "text"
        };

        private static string GetSafeCodeFence(string content)
        {
            int maxBackticks = 2;
            int current = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '`')
                {
                    current++;
                    if (current > maxBackticks) maxBackticks = current;
                }
                else
                {
                    current = 0;
                }
            }
            return new string('`', Math.Max(3, maxBackticks + 1));
        }

        private static string GetSafeRelativePath(string rootPath, string fullPath)
        {
            try
            {
                return Path.GetRelativePath(rootPath, fullPath);
            }
            catch
            {
                return Path.GetFileName(fullPath);
            }
        }

        private static string GetFormattedFileSize(string filePath)
        {
            try
            {
                var info = new FileInfo(filePath);
                if (!info.Exists) return "0 B";
                long bytes = info.Length;
                if (bytes < 1024) return $"{bytes} B";
                if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F1} KB";
                return $"{(bytes / (1024.0 * 1024.0)):F1} MB";
            }
            catch
            {
                return "-";
            }
        }

        private static string EscapeTablePipe(string text)
        {
            return text.Replace("|", "\\|");
        }

        private static string EscapeMarkdownLinkUrl(string path)
        {
            return $"<{path.Replace('\\', '/')}>";
        }

        private static string GenerateHeadingAnchor(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in text.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    sb.Append('-');
                }
            }
            return sb.ToString().Trim('-');
        }
    }
}
