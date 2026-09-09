using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MDPlus.Models;

namespace MDPlus.Core
{
    public class UpdateCheckResult
    {
        public bool IsSuccess { get; set; }
        public bool IsUpdateAvailable { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
        public string ReleaseHighlights { get; set; } = string.Empty;
        public string ReleaseUrl { get; set; } = string.Empty;
        public string? SetupDownloadUrl { get; set; }
        public string? ChecksumsDownloadUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UpdateInstallResult
    {
        public bool Success { get; set; }
        public string? InstallerPath { get; set; }
        public string? ExpectedHash { get; set; }
        public string? ActualHash { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Lightweight, non-blocking service for checking official GitHub releases,
    /// verifying cryptographic SHA-256 installer signatures, and launching the updater.
    /// </summary>
    public class UpdateService
    {
        public const string DefaultReleasesApiUrl = "https://api.github.com/repos/nickf-sudomania/mdplusplus/releases/latest";
        public const string SetupFileName = "MDPlus-Setup.exe";
        public const string ChecksumsFileName = "SHA256SUMS.txt";

        private readonly HttpClient _httpClient;
        private readonly string _releasesApiUrl;

        public UpdateService(HttpClient? httpClient = null, string? releasesApiUrl = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _releasesApiUrl = releasesApiUrl ?? DefaultReleasesApiUrl;

            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MDPlus-Updater");
            }
            if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            }
        }

        /// <summary>
        /// Gets the currently executing application version as a clean semantic version string (e.g. "1.0.0").
        /// </summary>
        public static string GetCurrentVersion()
        {
            var ver = typeof(UpdateService).Assembly.GetName().Version;
            if (ver == null) return "1.0.0";
            return $"{ver.Major}.{ver.Minor}.{Math.Max(0, ver.Build)}";
        }

        /// <summary>
        /// Determines whether a startup background update check should be performed
        /// based on user preference and debouncing (at most once every 24 hours).
        /// </summary>
        public static bool ShouldCheckOnStartup(AppSettings? settings, DateTime utcNow)
        {
            if (settings == null) return false;
            if (!settings.CheckForUpdatesOnStartup) return false;
            if (!settings.LastUpdateCheckUtc.HasValue) return true;

            var elapsed = utcNow - settings.LastUpdateCheckUtc.Value;
            // Allow check if negative (system clock skew) or >= 24 hours
            return elapsed < TimeSpan.Zero || elapsed >= TimeSpan.FromHours(24);
        }

        /// <summary>
        /// Parses a version string into an integer array, removing 'v' prefixes and build metadata.
        /// </summary>
        public static int[]? ParseVersionComponents(string? versionString)
        {
            if (string.IsNullOrWhiteSpace(versionString)) return null;

            string trimmed = versionString.Trim();
            if (trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(1).Trim();
            }

            // Remove prerelease or build metadata tags (e.g., "-beta", "+build123")
            int separatorIdx = trimmed.IndexOfAny(new[] { '-', '+' });
            if (separatorIdx >= 0)
            {
                trimmed = trimmed.Substring(0, separatorIdx);
            }

            string[] parts = trimmed.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;

            var result = new List<int>();
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int val) && val >= 0)
                {
                    result.Add(val);
                }
                else
                {
                    var match = Regex.Match(part, @"^\d+");
                    if (match.Success && int.TryParse(match.Value, out int extracted))
                    {
                        result.Add(extracted);
                    }
                    else
                    {
                        result.Add(0);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Compares two semantic version strings.
        /// Returns > 0 if versionA > versionB, < 0 if versionA < versionB, and 0 if equal.
        /// </summary>
        public static int CompareVersions(string? versionA, string? versionB)
        {
            var a = ParseVersionComponents(versionA);
            var b = ParseVersionComponents(versionB);

            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            int maxLen = Math.Max(a.Length, b.Length);
            for (int i = 0; i < maxLen; i++)
            {
                int valA = i < a.Length ? a[i] : 0;
                int valB = i < b.Length ? b[i] : 0;

                if (valA != valB)
                {
                    return valA.CompareTo(valB);
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns true if candidateVersion is strictly newer than currentVersion.
        /// </summary>
        public static bool IsNewerVersion(string? currentVersion, string? candidateVersion)
        {
            return CompareVersions(candidateVersion, currentVersion) > 0;
        }

        /// <summary>
        /// Extracts the expected SHA-256 hash for a given target filename from checksum manifest content.
        /// </summary>
        public static string? ExtractExpectedHash(string checksumsContent, string targetFileName = SetupFileName)
        {
            if (string.IsNullOrWhiteSpace(checksumsContent)) return null;

            var checksums = HashService.ParseChecksums(checksumsContent);

            if (checksums.TryGetValue(targetFileName, out var exactHash))
            {
                return exactHash;
            }

            foreach (var kvp in checksums)
            {
                if (kvp.Key.Equals(targetFileName, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.EndsWith(targetFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            if (checksums.TryGetValue(string.Empty, out var bareHash) && bareHash.Length == 64)
            {
                return bareHash;
            }

            return null;
        }

        /// <summary>
        /// Queries the GitHub releases API asynchronously and checks if a newer version is available.
        /// </summary>
        public async Task<UpdateCheckResult> CheckForUpdatesAsync(
            string? currentVersionOverride = null,
            CancellationToken cancellationToken = default)
        {
            string currentVer = currentVersionOverride ?? GetCurrentVersion();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, _releasesApiUrl);
                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return new UpdateCheckResult
                    {
                        IsSuccess = false,
                        CurrentVersion = currentVer,
                        ErrorMessage = $"GitHub API returned {(int)response.StatusCode} ({response.ReasonPhrase})"
                    };
                }

                string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? string.Empty : string.Empty;
                string name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
                string body = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? string.Empty : string.Empty;
                string htmlUrl = root.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() ?? string.Empty : string.Empty;

                string? setupUrl = null;
                string? checksumsUrl = null;

                if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var asset in assetsProp.EnumerateArray())
                    {
                        string assetName = asset.TryGetProperty("name", out var an) ? an.GetString() ?? string.Empty : string.Empty;
                        string dlUrl = asset.TryGetProperty("browser_download_url", out var du) ? du.GetString() ?? string.Empty : string.Empty;

                        if (assetName.Equals(SetupFileName, StringComparison.OrdinalIgnoreCase) ||
                            assetName.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            setupUrl = dlUrl;
                        }
                        else if (assetName.Equals(ChecksumsFileName, StringComparison.OrdinalIgnoreCase) ||
                                 assetName.EndsWith(".checksums.sha256", StringComparison.OrdinalIgnoreCase) ||
                                 assetName.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase))
                        {
                            checksumsUrl = dlUrl;
                        }
                    }
                }

                bool isNewer = IsNewerVersion(currentVer, tagName);

                return new UpdateCheckResult
                {
                    IsSuccess = true,
                    IsUpdateAvailable = isNewer,
                    CurrentVersion = currentVer,
                    LatestVersion = tagName,
                    ReleaseHighlights = !string.IsNullOrWhiteSpace(body) ? body : name,
                    ReleaseUrl = htmlUrl,
                    SetupDownloadUrl = setupUrl,
                    ChecksumsDownloadUrl = checksumsUrl
                };
            }
            catch (Exception ex)
            {
                return new UpdateCheckResult
                {
                    IsSuccess = false,
                    CurrentVersion = currentVer,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Downloads MDPlus-Setup.exe to a temporary location, cryptographically verifies its SHA-256 checksum
        /// against SHA256SUMS.txt, and returns the path to the verified executable.
        /// </summary>
        public async Task<UpdateInstallResult> DownloadAndVerifyUpdateAsync(
            UpdateCheckResult updateInfo,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (updateInfo == null || string.IsNullOrEmpty(updateInfo.SetupDownloadUrl))
            {
                return new UpdateInstallResult
                {
                    Success = false,
                    ErrorMessage = "No setup executable download URL available."
                };
            }

            try
            {
                // 1. Download checksum manifest or extract expected hash
                string? expectedHash = null;
                if (!string.IsNullOrEmpty(updateInfo.ChecksumsDownloadUrl))
                {
                    try
                    {
                        string checksumsContent = await _httpClient.GetStringAsync(updateInfo.ChecksumsDownloadUrl, cancellationToken).ConfigureAwait(false);
                        expectedHash = ExtractExpectedHash(checksumsContent, SetupFileName);
                    }
                    catch
                    {
                        // Fall back to highlights
                    }
                }

                if (string.IsNullOrEmpty(expectedHash) && !string.IsNullOrEmpty(updateInfo.ReleaseHighlights))
                {
                    expectedHash = ExtractExpectedHash(updateInfo.ReleaseHighlights, SetupFileName);
                }

                if (string.IsNullOrEmpty(expectedHash))
                {
                    return new UpdateInstallResult
                    {
                        Success = false,
                        ErrorMessage = "Could not find published cryptographic SHA-256 checksum (SHA256SUMS.txt) for installer verification."
                    };
                }

                // 2. Download installer into a temporary directory
                string tempDir = Path.Combine(Path.GetTempPath(), "MDPlusUpdate");
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                string destinationExe = Path.Combine(tempDir, SetupFileName);
                if (File.Exists(destinationExe))
                {
                    try { File.Delete(destinationExe); } catch { }
                }

                using (var response = await _httpClient.GetAsync(updateInfo.SetupDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    await using (var fileStream = new FileStream(destinationExe, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                    {
                        byte[] buffer = new byte[81920];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                            totalRead += bytesRead;

                            if (totalBytes > 0 && progress != null)
                            {
                                progress.Report(Math.Min(1.0, (double)totalRead / totalBytes));
                            }
                        }
                    }
                }

                // 3. Cryptographic Verification against published SHA-256 hash
                string actualHash = await HashService.ComputeSha256Async(destinationExe, cancellationToken).ConfigureAwait(false);
                bool isMatch = string.Equals(actualHash, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase);

                if (!isMatch)
                {
                    // For security, delete the untrusted/corrupted installer
                    try { File.Delete(destinationExe); } catch { }

                    return new UpdateInstallResult
                    {
                        Success = false,
                        ExpectedHash = expectedHash,
                        ActualHash = actualHash,
                        ErrorMessage = $"Cryptographic SHA-256 verification failed!\nExpected: {expectedHash}\nActual:   {actualHash}\nThe downloaded installer was rejected and discarded for security."
                    };
                }

                return new UpdateInstallResult
                {
                    Success = true,
                    InstallerPath = destinationExe,
                    ExpectedHash = expectedHash,
                    ActualHash = actualHash
                };
            }
            catch (Exception ex)
            {
                return new UpdateInstallResult
                {
                    Success = false,
                    ErrorMessage = $"Download or verification error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Safely launches the verified installer and closes MDPlus.
        /// </summary>
        public static void LaunchInstallerAndExit(string installerPath)
        {
            if (string.IsNullOrEmpty(installerPath) || !File.Exists(installerPath))
            {
                throw new FileNotFoundException("Installer not found: " + installerPath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            };
            Process.Start(startInfo);

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}
