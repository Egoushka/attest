using System;
using System.IO;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// docs/COVERAGE.md is generated from the registry, so the table cannot drift from the code.
    /// When it is stale this fails and says how to regenerate it.
    /// </summary>
    public class CoverageDocTests
    {
        [Fact]
        public void CoverageDocMatchesTheRegistry()
        {
            string path = Path.Combine(RepoRoot.Find(), "docs", "COVERAGE.md");
            string expected = CoverageDoc.Render();

            if (Environment.GetEnvironmentVariable("ATTEST_UPDATE_COVERAGE") == "1")
            {
                File.WriteAllText(path, expected, new System.Text.UTF8Encoding(false));
                return;
            }

            Assert.True(File.Exists(path), "docs/COVERAGE.md is missing. Regenerate it: ATTEST_UPDATE_COVERAGE=1 dotnet test --filter CoverageDocTests");
            string actual = File.ReadAllText(path).Replace("\r\n", "\n");
            Assert.True(
                actual == expected,
                "docs/COVERAGE.md is stale: it no longer matches the registry (sources, coverage, revision or known gaps changed). "
                + "Regenerate it with: ATTEST_UPDATE_COVERAGE=1 dotnet test --filter CoverageDocTests, then commit the file.");
        }
    }

    internal static class RepoRoot
    {
        internal static string Find()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Attest.sln")))
            {
                directory = directory.Parent;
            }

            Assert.True(directory != null, "Attest.sln is not above " + AppContext.BaseDirectory + ".");
            return directory.FullName;
        }
    }
}
