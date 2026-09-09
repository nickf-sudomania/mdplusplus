using System;
using System.Diagnostics;
using MDPlus.E2E.Harness;
using MDPlus.E2E.Tiers;

namespace MDPlus.E2E
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("        MDPlus Opaque-Box E2E Test Suite          ");
            Console.WriteLine("==================================================");
            Console.WriteLine($"Environment: .NET {Environment.Version} on {Environment.OSVersion}");
            Console.WriteLine($"Timestamp:   {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            Console.WriteLine();

            var totalSw = Stopwatch.StartNew();
            E2ETestHarness.Reset();

            try
            {
                // Run Tier 1: Feature Coverage
                Tier1_FeatureCoverage.RunAll();

                // Run Tier 2: Boundary & Corner Cases
                Tier2_BoundaryCornerCases.RunAll();

                // Run Tier 3: Cross-Feature Combinations
                Tier3_CrossFeatureCombinations.RunAll();

                // Run Tier 4: Real-World Application Scenarios
                Tier4_RealWorldScenarios.RunAll();

                // Run Tier 5: Adversarial Hardening (Challenger)
                Tier5_AdversarialHardening.RunAll();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[FATAL ERROR IN TEST HARNESS] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return 1;
            }

            totalSw.Stop();

            Console.WriteLine("\n==================================================");
            Console.WriteLine("                  E2E Test Summary                ");
            Console.WriteLine("==================================================");
            Console.WriteLine($"Total Tests:   {E2ETestHarness.Results.Count}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Passed:        {E2ETestHarness.PassCount}");
            Console.ResetColor();

            if (E2ETestHarness.FailCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed:        {E2ETestHarness.FailCount}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"Failed:        0");
            }

            Console.WriteLine($"Total Elapsed: {totalSw.ElapsedMilliseconds} ms");
            Console.WriteLine("==================================================");

            if (E2ETestHarness.FailCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("RESULT: SUCCESS - All E2E test cases passed cleanly.");
                Console.ResetColor();
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"RESULT: FAILURE - {E2ETestHarness.FailCount} test case(s) failed.");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
