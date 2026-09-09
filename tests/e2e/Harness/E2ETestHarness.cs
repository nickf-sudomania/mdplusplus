using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MDPlus.E2E.Harness
{
    public class TestCaseResult
    {
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
        public long DurationMs { get; set; }
    }

    public static class E2ETestHarness
    {
        public static int PassCount { get; private set; }
        public static int FailCount { get; private set; }
        public static int SkipCount { get; private set; }
        public static List<TestCaseResult> Results { get; } = new List<TestCaseResult>();

        public static string GetRepositoryRoot()
        {
            string? dir = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, "src", "MDPlus.csproj")))
                {
                    return dir;
                }
                dir = Directory.GetParent(dir)?.FullName;
            }
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));
        }

        public static void Reset()
        {
            PassCount = 0;
            FailCount = 0;
            SkipCount = 0;
            Results.Clear();
        }

        public static void RunTest(string tier, string name, Action testAction)
        {
            var sw = Stopwatch.StartNew();
            var result = new TestCaseResult { Tier = tier, Name = name };
            try
            {
                testAction();
                sw.Stop();
                result.Passed = true;
                result.DurationMs = sw.ElapsedMilliseconds;
                PassCount++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  [PASS] ");
                Console.ResetColor();
                Console.WriteLine($"{name} ({sw.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.Passed = false;
                result.DurationMs = sw.ElapsedMilliseconds;
                result.Message = ex.Message;
                FailCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  [FAIL] ");
                Console.ResetColor();
                Console.WriteLine($"{name} ({sw.ElapsedMilliseconds} ms)");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"         Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"         Inner: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }
            Results.Add(result);
        }

        #region Assertions

        public static void AssertEqual<T>(T expected, T actual, string message = "")
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new Exception($"Assertion failed: Expected [{expected}], but got [{actual}]. {message}");
            }
        }

        public static void AssertTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new Exception($"Assertion failed: Expected true, but was false. {message}");
            }
        }

        public static void AssertFalse(bool condition, string message = "")
        {
            if (condition)
            {
                throw new Exception($"Assertion failed: Expected false, but was true. {message}");
            }
        }

        public static void AssertNotNull(object? obj, string message = "")
        {
            if (obj == null)
            {
                throw new Exception($"Assertion failed: Expected non-null value, but was null. {message}");
            }
        }

        public static void AssertContains(string expectedSubstring, string actualString, string message = "")
        {
            if (actualString == null || !actualString.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Assertion failed: Expected string to contain [{expectedSubstring}], but received: [{(actualString?.Length > 100 ? actualString.Substring(0, 100) + "..." : actualString)}]. {message}");
            }
        }

        public static void AssertInRange(double actual, double min, double max, string message = "")
        {
            if (actual < min || actual > max)
            {
                throw new Exception($"Assertion failed: Value [{actual}] is out of expected range [{min}, {max}]. {message}");
            }
        }

        #endregion

        #region WCAG AA Contrast Calculations

        public static double CalculateRelativeLuminance(Color color)
        {
            double r = Linearize(color.R / 255.0);
            double g = Linearize(color.G / 255.0);
            double b = Linearize(color.B / 255.0);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;

            static double Linearize(double val) =>
                val <= 0.03928 ? val / 12.92 : Math.Pow((val + 0.055) / 1.055, 2.4);
        }

        public static double CalculateContrastRatio(Color c1, Color c2)
        {
            double l1 = CalculateRelativeLuminance(c1);
            double l2 = CalculateRelativeLuminance(c2);
            double lighter = Math.Max(l1, l2);
            double darker = Math.Min(l1, l2);
            return (lighter + 0.05) / (darker + 0.05);
        }

        #endregion

        #region ICO Binary Analysis

        public class IcoEntry
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int ColorCount { get; set; }
            public int Planes { get; set; }
            public int BitCount { get; set; }
            public int BytesInRes { get; set; }
            public int ImageOffset { get; set; }
        }

        public static List<IcoEntry> ParseIco(byte[] bytes)
        {
            var entries = new List<IcoEntry>();
            if (bytes.Length < 6) return entries;

            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);

            ushort reserved = br.ReadUInt16();
            ushort type = br.ReadUInt16();
            ushort count = br.ReadUInt16();

            if (reserved != 0 || type != 1)
            {
                throw new InvalidDataException("Invalid ICO header signature.");
            }

            for (int i = 0; i < count; i++)
            {
                byte w = br.ReadByte();
                byte h = br.ReadByte();
                byte colorCount = br.ReadByte();
                byte res = br.ReadByte();
                ushort planes = br.ReadUInt16();
                ushort bitCount = br.ReadUInt16();
                int bytesInRes = br.ReadInt32();
                int imageOffset = br.ReadInt32();

                entries.Add(new IcoEntry
                {
                    Width = w == 0 ? 256 : w,
                    Height = h == 0 ? 256 : h,
                    ColorCount = colorCount,
                    Planes = planes,
                    BitCount = bitCount,
                    BytesInRes = bytesInRes,
                    ImageOffset = imageOffset
                });
            }

            return entries;
        }

        #endregion

        #region Progressive Testability Helpers

        public static bool TrySerializeFlowDocument(FlowDocument doc, out string serializedMarkdown)
        {
            serializedMarkdown = string.Empty;
            try
            {
                var serializerType = typeof(MDPlus.MainWindow).Assembly.GetType("MDPlus.Core.MarkdownSerializer");
                if (serializerType == null) return false;

                var method = serializerType.GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static, new[] { typeof(FlowDocument) });
                if (method == null) return false;

                var result = method.Invoke(null, new object[] { doc });
                if (result is string s)
                {
                    serializedMarkdown = s;
                    return true;
                }
            }
            catch
            {
                // Serialization method failed or not ready
            }
            return false;
        }

        public static bool TryGetThemePresets(out Array? presetValues, out Type? presetType)
        {
            presetValues = null;
            presetType = typeof(MDPlus.MainWindow).Assembly.GetType("MDPlus.Core.ThemePreset");
            if (presetType != null && presetType.IsEnum)
            {
                presetValues = Enum.GetValues(presetType);
                return true;
            }
            return false;
        }

        #endregion
    }
}
