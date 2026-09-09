using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MDPlus.Core
{
    public class ChecksumVerificationResult
    {
        public string FileName { get; set; } = string.Empty;
        public string ExpectedHash { get; set; } = string.Empty;
        public string ActualHash { get; set; } = string.Empty;
        public bool IsMatch { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provides cryptographic SHA-256 hash generation and verification,
    /// implementing Notepad++ release integrity standards to detect file tampering and corruption.
    /// </summary>
    public static class HashService
    {
        /// <summary>
        /// Computes the SHA-256 hash of a file on disk.
        /// </summary>
        public static string ComputeSha256(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}", filePath);
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return ComputeSha256(stream);
        }

        /// <summary>
        /// Computes the SHA-256 hash of a readable stream.
        /// </summary>
        public static string ComputeSha256(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(stream);
            return ConvertToHex(hashBytes);
        }

        /// <summary>
        /// Computes the SHA-256 hash of a byte array.
        /// </summary>
        public static string ComputeSha256(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            byte[] hashBytes = SHA256.HashData(data);
            return ConvertToHex(hashBytes);
        }

        /// <summary>
        /// Verifies whether the SHA-256 hash of a file matches an expected hash.
        /// Ignores whitespace and case differences.
        /// </summary>
        public static bool VerifyFileSha256(string filePath, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash) || !File.Exists(filePath))
            {
                return false;
            }

            string actual = ComputeSha256(filePath);
            return string.Equals(actual, NormalizeHash(expectedHash), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Formats a single entry in standard GNU/coreutils format: "{hash}  {fileName}".
        /// </summary>
        public static string FormatChecksumEntry(string fileName, string hash)
        {
            return $"{hash.ToLowerInvariant()}  {fileName}";
        }

        /// <summary>
        /// Parses a checksum file containing lines formatted as "{hash}  {fileName}" or "{hash} *{fileName}".
        /// </summary>
        public static Dictionary<string, string> ParseChecksums(string checksumsContent)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(checksumsContent))
            {
                return result;
            }

            string[] lines = checksumsContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                {
                    continue; // Skip comments and empty lines
                }

                // Standard format: <hash>  <filename> or <hash> *<filename>
                int spaceIndex = trimmed.IndexOf(' ');
                if (spaceIndex >= 32) // SHA-256 is 64 hex characters
                {
                    string hash = trimmed.Substring(0, spaceIndex).Trim();
                    string fileName = trimmed.Substring(spaceIndex + 1).Trim().TrimStart('*');

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        result[fileName] = hash.ToLowerInvariant();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Verifies all files specified in a checksums file (e.g. SHA256SUMS.txt) against their files on disk.
        /// Relative paths are resolved against the directory containing the checksums file.
        /// </summary>
        public static List<ChecksumVerificationResult> VerifyChecksumsFile(string checksumsFilePath)
        {
            var results = new List<ChecksumVerificationResult>();
            if (!File.Exists(checksumsFilePath))
            {
                throw new FileNotFoundException($"Checksum file not found: {checksumsFilePath}", checksumsFilePath);
            }

            string baseDir = Path.GetDirectoryName(checksumsFilePath) ?? string.Empty;
            string content = File.ReadAllText(checksumsFilePath);
            var entries = ParseChecksums(content);

            foreach (var kvp in entries)
            {
                string fileName = kvp.Key;
                string expected = kvp.Value;
                string resolvedPath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(baseDir, fileName);

                var itemResult = new ChecksumVerificationResult
                {
                    FileName = fileName,
                    ExpectedHash = expected
                };

                if (!File.Exists(resolvedPath))
                {
                    itemResult.IsMatch = false;
                    itemResult.ErrorMessage = "File not found on disk.";
                }
                else
                {
                    try
                    {
                        string actual = ComputeSha256(resolvedPath);
                        itemResult.ActualHash = actual;
                        itemResult.IsMatch = string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
                        if (!itemResult.IsMatch)
                        {
                            itemResult.ErrorMessage = "Checksum mismatch: file contents have been altered or corrupted.";
                        }
                    }
                    catch (Exception ex)
                    {
                        itemResult.IsMatch = false;
                        itemResult.ErrorMessage = $"Error reading file: {ex.Message}";
                    }
                }

                results.Add(itemResult);
            }

            return results;
        }

        public static string NormalizeHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash)) return string.Empty;
            return hash.Trim().ToLowerInvariant();
        }

        private static string ConvertToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
