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
        public string? SetupFileName { get; set; }
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
        /// Gets the currently executing application version as a clean semantic version string (e.g. "1.06").
        /// </summary>
        public static string GetCurrentVersion()
        {
            var assembly = typeof(UpdateService).Assembly;
            var infoVerAttr = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>(assembly);
            if (infoVerAttr != null && !string.IsNullOrWhiteSpace(infoVerAttr.InformationalVersion))
            {
                string infoVer = infoVerAttr.InformationalVersion.Trim();
                int plusIdx = infoVer.IndexOf('+');
                if (plusIdx > 0) infoVer = infoVer.Substring(0, plusIdx);
                return infoVer;
            }
            var ver = assembly.GetName().Version;
            if (ver == null) return "1.06";
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

            // Locate the first digit to strip tag prefixes like 'v', 'ver', 'release-', 'mdplus-v'
            int firstDigitIdx = -1;
            for (int i = 0; i < trimmed.Length; i++)
            {
                if (char.IsDigit(trimmed[i]))
                {
                    firstDigitIdx = i;
                    break;
                }
            }

            if (firstDigitIdx < 0) return null;

            string versionPart = trimmed.Substring(firstDigitIdx);

            // Remove prerelease or build metadata tags following the version digits (e.g., "-beta", "+build123")
            int separatorIdx = versionPart.IndexOfAny(new[] { '-', '+' });
            if (separatorIdx >= 0)
            {
                versionPart = versionPart.Substring(0, separatorIdx);
            }

            string[] parts = versionPart.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
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
        /// Performs path-bounded filename matching to avoid substring collisions, and falls back to
        /// release note labels (e.g. SHA-256: ...) or bare hashes.
        /// </summary>
        public static string? ExtractExpectedHash(string checksumsContent, string targetFileName = SetupFileName)
        {
            if (string.IsNullOrWhiteSpace(checksumsContent)) return null;

            var checksums = HashService.ParseChecksums(checksumsContent);

            // 1. Exact match on targetFileName
            if (checksums.TryGetValue(targetFileName, out var exactHash) && exactHash.Length == 64)
            {
                return exactHash;
            }

            // 2. Exact match on file name component of path (e.g. "dist/MDPlus-Setup.exe", "*MDPlus-Setup.exe")
            foreach (var kvp in checksums)
            {
                string cleanKey = kvp.Key.TrimStart('*', '?', ' ', '\t');
                string keyFileName = Path.GetFileName(cleanKey.Replace('/', '\\'));
                if (keyFileName.Equals(targetFileName, StringComparison.OrdinalIgnoreCase) && kvp.Value.Length == 64)
                {
                    return kvp.Value;
                }
            }

            // 2b. If targetFileName is or contains "MDPlus" and "Setup", match versioned setup assets (e.g. "MDPlus-v1.3.0-Setup.exe")
            string targetBaseName = Path.GetFileNameWithoutExtension(targetFileName);
            if (targetBaseName.Contains("MDPlus", StringComparison.OrdinalIgnoreCase) && targetBaseName.Contains("Setup", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var kvp in checksums)
                {
                    string cleanKey = kvp.Key.TrimStart('*', '?', ' ', '\t');
                    string keyFileName = Path.GetFileName(cleanKey.Replace('/', '\\'));
                    if (keyFileName.Contains("MDPlus", StringComparison.OrdinalIgnoreCase) &&
                        keyFileName.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase) &&
                        kvp.Value.Length == 64)
                    {
                        return kvp.Value;
                    }
                }
            }

            // 3. Line-by-line target file association matching (for Markdown lists, tables, bullets, and multi-asset release notes)
            string escapedTarget = Regex.Escape(targetFileName);
            string targetPattern = $@"(?<![\w\.-]){escapedTarget}(?![\w\.-])";
            string[] lines = checksumsContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (Regex.IsMatch(line, targetPattern, RegexOptions.IgnoreCase))
                {
                    // Check if this line contains a 64-hex SHA-256 hash
                    var lineMatch = Regex.Match(line, @"\b([a-fA-F0-9]{64})\b");
                    if (lineMatch.Success)
                    {
                        return lineMatch.Groups[1].Value.ToLowerInvariant();
                    }

                    // Scan subsequent lines (up to 4) until next section or hash is found
                    for (int k = i + 1; k < Math.Min(lines.Length, i + 5); k++)
                    {
                        string nextLine = lines[k].Trim();
                        if (string.IsNullOrEmpty(nextLine)) continue;

                        // Stop if entering another header or different binary asset
                        if (nextLine.StartsWith("#") || (k > i + 1 && (nextLine.EndsWith(".exe") || nextLine.EndsWith(".zip") || nextLine.EndsWith(".tar.gz") || nextLine.EndsWith(".msi"))))
                        {
                            break;
                        }

                        var nextLineMatch = Regex.Match(nextLine, @"\b([a-fA-F0-9]{64})\b");
                        if (nextLineMatch.Success)
                        {
                            return nextLineMatch.Groups[1].Value.ToLowerInvariant();
                        }
                    }
                }
            }

            // 4. Check for target-associated hash labels (e.g. "MDPlus-Setup.exe SHA-256", "sha256: MDPlus-Setup.exe")
            string targetLower = targetFileName.ToLowerInvariant();
            string targetBaseLower = targetBaseName.ToLowerInvariant();
            foreach (var kvp in checksums)
            {
                string cleanKey = kvp.Key.Trim().ToLowerInvariant();
                bool matchesTarget = cleanKey.Contains(targetLower) || cleanKey.Contains(targetBaseLower);
                if (matchesTarget && kvp.Value.Length == 64)
                {
                    return kvp.Value;
                }
            }

            // 5. If manifest contains exactly one entry, allow generic hash labels (e.g. "sha256", "sha-256", "checksum")
            if (checksums.Count == 1)
            {
                var single = System.Linq.Enumerable.First(checksums);
                string cleanKey = single.Key.Trim().ToLowerInvariant();
                if ((string.IsNullOrEmpty(cleanKey) || cleanKey == "sha256" || cleanKey == "sha-256" || cleanKey.Contains("sha256") || cleanKey.Contains("checksum")) &&
                    single.Value.Length == 64)
                {
                    return single.Value;
                }
            }

            // 6. Bare hash (single hash file)
            if (checksums.TryGetValue(string.Empty, out var bareHash) && bareHash.Length == 64)
            {
                return bareHash;
            }

            // 7. Fallback regex search for any standalone 64-hex string in the content
            var hashMatches = Regex.Matches(checksumsContent, @"\b([a-fA-F0-9]{64})\b");
            if (hashMatches.Count == 1)
            {
                return hashMatches[0].Groups[1].Value.ToLowerInvariant();
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
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // When /releases/latest returns 404, check if /releases list endpoint has any releases (e.g. prereleases)
                        if (_releasesApiUrl.EndsWith("/releases/latest", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                string listUrl = _releasesApiUrl.Substring(0, _releasesApiUrl.Length - 7); // strip "/latest"
                                using var listReq = new HttpRequestMessage(HttpMethod.Get, listUrl);
                                using var listResp = await _httpClient.SendAsync(listReq, cancellationToken).ConfigureAwait(false);
                                if (listResp.IsSuccessStatusCode)
                                {
                                    string listJson = await listResp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                                    using var listDoc = JsonDocument.Parse(listJson);
                                    if (listDoc.RootElement.ValueKind == JsonValueKind.Array && listDoc.RootElement.GetArrayLength() > 0)
                                    {
                                        return ParseReleaseJsonElement(listDoc.RootElement[0], currentVer);
                                    }
                                }
                            }
                            catch
                            {
                                // Fall through to up-to-date result
                            }
                        }

                        // When no releases have been published on GitHub yet, the current build is already up to date
                        return new UpdateCheckResult
                        {
                            IsSuccess = true,
                            IsUpdateAvailable = false,
                            CurrentVersion = currentVer,
                            LatestVersion = currentVer,
                            ReleaseUrl = "https://github.com/nickf-sudomania/mdplusplus/releases"
                        };
                    }

                    string statusMsg;
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                        (int)response.StatusCode == 429)
                    {
                        string resetDetail = string.Empty;
                        if (response.Headers.TryGetValues("x-ratelimit-reset", out var resetValues))
                        {
                            var first = System.Linq.Enumerable.FirstOrDefault(resetValues);
                            if (long.TryParse(first, out long epoch))
                            {
                                var resetUtc = DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime;
                                var minutesLeft = Math.Max(1, (int)Math.Ceiling((resetUtc - DateTime.UtcNow).TotalMinutes));
                                resetDetail = $" Window resets in approximately {minutesLeft} minute{(minutesLeft == 1 ? "" : "s")} ({resetUtc:HH:mm} UTC).";
                            }
                        }
                        else if (response.Headers.TryGetValues("Retry-After", out var retryValues))
                        {
                            var firstRetry = System.Linq.Enumerable.FirstOrDefault(retryValues);
                            if (int.TryParse(firstRetry, out int seconds))
                            {
                                var mins = Math.Max(1, (int)Math.Ceiling(seconds / 60.0));
                                resetDetail = $" Please retry in approximately {mins} minute{(mins == 1 ? "" : "s")}.";
                            }
                        }

                        statusMsg = $"GitHub API rate limit reached (60 requests/hour for unauthenticated checks).{resetDetail} Please try again later.";
                    }
                    else
                    {
                        statusMsg = $"GitHub API returned {(int)response.StatusCode} ({response.ReasonPhrase})";
                    }

                    return new UpdateCheckResult
                    {
                        IsSuccess = false,
                        CurrentVersion = currentVer,
                        ErrorMessage = statusMsg
                    };
                }

                string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(json);
                return ParseReleaseJsonElement(doc.RootElement, currentVer);
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
        /// Parses a GitHub Release JSON element (from /releases/latest or /releases array)
        /// into an UpdateCheckResult.
        /// </summary>
        public static UpdateCheckResult ParseReleaseJsonElement(JsonElement root, string currentVer)
        {
            try
            {
                string tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? string.Empty : string.Empty;
            string name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
            string body = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? string.Empty : string.Empty;
            string htmlUrl = root.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() ?? string.Empty : string.Empty;

            string? setupUrl = null;
            string? setupFileName = null;
            int bestSetupPriority = 0;

            string? checksumsUrl = null;
            int bestChecksumsPriority = 0;

            if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsProp.EnumerateArray())
                {
                    string assetName = asset.TryGetProperty("name", out var an) ? an.GetString() ?? string.Empty : string.Empty;
                    string dlUrl = asset.TryGetProperty("browser_download_url", out var du) ? du.GetString() ?? string.Empty : string.Empty;

                    int setupPriority = 0;
                    if (assetName.Equals(SetupFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        setupPriority = 4;
                    }
                    else if (assetName.StartsWith("MDPlus", StringComparison.OrdinalIgnoreCase) &&
                             assetName.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        setupPriority = 3;
                    }
                    else if (assetName.Contains("MDPlus", StringComparison.OrdinalIgnoreCase) &&
                             assetName.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        setupPriority = 2;
                    }
                    else if (assetName.EndsWith("Setup.exe", StringComparison.OrdinalIgnoreCase) ||
                             assetName.EndsWith("-setup.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        setupPriority = 1;
                    }

                    if (setupPriority > bestSetupPriority)
                    {
                        bestSetupPriority = setupPriority;
                        setupUrl = dlUrl;
                        setupFileName = assetName;
                    }

                    int checksumsPriority = 0;
                    if (assetName.Equals(ChecksumsFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        checksumsPriority = 4;
                    }
                    else if (assetName.StartsWith("MDPlus", StringComparison.OrdinalIgnoreCase) &&
                             (assetName.EndsWith("sums.txt", StringComparison.OrdinalIgnoreCase) ||
                              assetName.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase) ||
                              assetName.EndsWith(".checksums.sha256", StringComparison.OrdinalIgnoreCase)))
                    {
                        checksumsPriority = 3;
                    }
                    else if (assetName.Contains("MDPlus", StringComparison.OrdinalIgnoreCase) &&
                             (assetName.EndsWith("sums.txt", StringComparison.OrdinalIgnoreCase) ||
                              assetName.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase) ||
                              assetName.EndsWith(".checksums.sha256", StringComparison.OrdinalIgnoreCase)))
                    {
                        checksumsPriority = 2;
                    }
                    else if (assetName.EndsWith(".checksums.sha256", StringComparison.OrdinalIgnoreCase) ||
                             assetName.EndsWith(".sha256", StringComparison.OrdinalIgnoreCase) ||
                             assetName.EndsWith("sums.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        checksumsPriority = 1;
                    }

                    if (checksumsPriority > bestChecksumsPriority)
                    {
                        bestChecksumsPriority = checksumsPriority;
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
                SetupFileName = setupFileName,
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

            string targetFileName = updateInfo.SetupFileName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(targetFileName) && !string.IsNullOrEmpty(updateInfo.SetupDownloadUrl))
            {
                try { targetFileName = Path.GetFileName(new Uri(updateInfo.SetupDownloadUrl).LocalPath); } catch { }
            }
            if (string.IsNullOrWhiteSpace(targetFileName)) targetFileName = SetupFileName;

            string tempDir = Path.Combine(Path.GetTempPath(), "MDPlusUpdate");
            string destinationExe = Path.Combine(tempDir, targetFileName);

            try
            {
                // 1. Download checksum manifest or extract expected hash
                string? expectedHash = null;
                if (!string.IsNullOrEmpty(updateInfo.ChecksumsDownloadUrl))
                {
                    try
                    {
                        string checksumsContent = await _httpClient.GetStringAsync(updateInfo.ChecksumsDownloadUrl, cancellationToken).ConfigureAwait(false);
                        expectedHash = ExtractExpectedHash(checksumsContent, targetFileName);
                        if (string.IsNullOrEmpty(expectedHash) && !targetFileName.Equals(SetupFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            expectedHash = ExtractExpectedHash(checksumsContent, SetupFileName);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch
                    {
                        // Fall back to highlights
                    }
                }

                if (string.IsNullOrEmpty(expectedHash) && !string.IsNullOrEmpty(updateInfo.ReleaseHighlights))
                {
                    expectedHash = ExtractExpectedHash(updateInfo.ReleaseHighlights, targetFileName);
                    if (string.IsNullOrEmpty(expectedHash) && !targetFileName.Equals(SetupFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        expectedHash = ExtractExpectedHash(updateInfo.ReleaseHighlights, SetupFileName);
                    }
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
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                if (File.Exists(destinationExe))
                {
                    try
                    {
                        File.SetAttributes(destinationExe, FileAttributes.Normal);
                        File.Delete(destinationExe);
                    }
                    catch
                    {
                        // If file is locked or in use, create a collision-free unique executable name
                        string uniqueName = $"{Path.GetFileNameWithoutExtension(targetFileName)}_{Guid.NewGuid().ToString("N")[..8]}{Path.GetExtension(targetFileName)}";
                        destinationExe = Path.Combine(tempDir, uniqueName);
                    }
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

                        if (totalBytes > 0 && totalRead < totalBytes)
                        {
                            throw new IOException($"Download was incomplete: received {totalRead} of {totalBytes} bytes.");
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
            catch (OperationCanceledException)
            {
                try { if (File.Exists(destinationExe)) File.Delete(destinationExe); } catch { }
                throw;
            }
            catch (Exception ex)
            {
                try { if (File.Exists(destinationExe)) File.Delete(destinationExe); } catch { }
                return new UpdateInstallResult
                {
                    Success = false,
                    ErrorMessage = $"Download or verification error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Validates installer existence and creates a configured ProcessStartInfo instance.
        /// Configures process elevation via the "runas" verb on Windows NT so that updates to
        /// Program Files and system file associations have necessary administrative permissions.
        /// </summary>
        public static ProcessStartInfo CreateInstallerProcessStartInfo(string installerPath)
        {
            if (string.IsNullOrEmpty(installerPath) || !File.Exists(installerPath))
            {
                throw new FileNotFoundException("Installer not found: " + installerPath);
            }

            string workingDir = Path.GetDirectoryName(installerPath) ?? Path.GetTempPath();

            var startInfo = new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true,
                WorkingDirectory = workingDir
            };

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                startInfo.Verb = "runas";
            }

            return startInfo;
        }

        /// <summary>
        /// Attempts to start the installer process with UAC elevation, falling back to standard execution
        /// if elevation is blocked by policy. Returns true if the process successfully started,
        /// or false if the user cancelled the UAC elevation prompt (error 1223).
        /// </summary>
        public static bool TryLaunchInstaller(
            string installerPath,
            Action<ProcessStartInfo>? startProcess = null)
        {
            var startInfo = CreateInstallerProcessStartInfo(installerPath);

            try
            {
                if (startProcess != null)
                {
                    startProcess(startInfo);
                    return true;
                }
                else
                {
                    try
                    {
                        Process.Start(startInfo);
                        return true;
                    }
                    catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
                    {
                        // User cancelled the UAC elevation prompt (ERROR_CANCELLED = 1223).
                        return false;
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        // If execution failed due to elevation policy, fallback to standard execution without runas
                        try
                        {
                            var fallbackInfo = new ProcessStartInfo
                            {
                                FileName = installerPath,
                                UseShellExecute = true,
                                WorkingDirectory = Path.GetDirectoryName(installerPath) ?? Path.GetTempPath()
                            };
                            Process.Start(fallbackInfo);
                            return true;
                        }
                        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
                        {
                            return false;
                        }
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return false;
            }
        }

        /// <summary>
        /// Safely shuts down the WPF application and cleanly terminates the process.
        /// </summary>
        public static void ExitApplication()
        {
            if (Application.Current != null)
            {
                try
                {
                    if (Application.Current.Dispatcher.CheckAccess())
                    {
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
                    }
                }
                catch { }
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Safely launches the verified installer and cleanly closes MDPlus.
        /// Handles UAC cancellation (error 1223) safely without terminating the application.
        /// Optional delegates allow unit tests to verify process start and exit behaviors.
        /// </summary>
        public static void LaunchInstallerAndExit(
            string installerPath,
            Action<ProcessStartInfo>? startProcess = null,
            Action? exitApp = null)
        {
            bool launched = TryLaunchInstaller(installerPath, startProcess);
            if (!launched) return;

            if (exitApp != null)
            {
                exitApp();
                return;
            }

            ExitApplication();
        }
    }
}
