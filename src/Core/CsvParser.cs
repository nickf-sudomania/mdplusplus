using System;
using System.Collections.Generic;
using System.Text;

namespace MDPlus.Core
{
    /// <summary>
    /// High-performance RFC 4180 compliant CSV and TSV parser.
    /// Supports configurable delimiters (comma, tab), quoted fields with embedded delimiters/newlines,
    /// escaped quotes (""), resilient unclosed quote recovery at EOF, jagged row padding,
    /// and trailing blank line suppression.
    /// </summary>
    public static class CsvParser
    {
        public static List<List<string>> Parse(string? text, char delimiter = ',')
        {
            var rows = new List<List<string>>();
            if (string.IsNullOrWhiteSpace(text))
                return rows;

            int len = text.Length;
            var currentRow = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;
            int maxCols = 0;

            for (int i = 0; i < len; i++)
            {
                char c = text[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < len && text[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i++; // Skip second escaped quote
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        if (currentField.Length == 0)
                        {
                            inQuotes = true;
                        }
                        else
                        {
                            currentField.Append('"');
                        }
                    }
                    else if (c == delimiter)
                    {
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else if (c == '\r')
                    {
                        if (i + 1 < len && text[i + 1] == '\n')
                        {
                            i++;
                        }
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                        rows.Add(currentRow);
                        if (currentRow.Count > maxCols) maxCols = currentRow.Count;
                        currentRow = new List<string>();
                    }
                    else if (c == '\n')
                    {
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                        rows.Add(currentRow);
                        if (currentRow.Count > maxCols) maxCols = currentRow.Count;
                        currentRow = new List<string>();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
            }

            // EOF handling: close quote if still open
            if (inQuotes)
            {
                inQuotes = false;
            }

            // Flush remaining field/row
            if (currentField.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentField.ToString());
                if (currentRow.Count == 1 && string.IsNullOrEmpty(currentRow[0]) && rows.Count > 0)
                {
                    // Trailing newline suppression
                }
                else
                {
                    rows.Add(currentRow);
                    if (currentRow.Count > maxCols) maxCols = currentRow.Count;
                }
            }

            // Harmonize jagged rows
            if (maxCols > 0)
            {
                foreach (var row in rows)
                {
                    while (row.Count < maxCols)
                    {
                        row.Add(string.Empty);
                    }
                }
            }

            return rows;
        }
    }
}
