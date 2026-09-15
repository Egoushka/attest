using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// AGENTS.md rule 6 names four constructs this library must not use: <c>\d</c>,
    /// <c>char.IsDigit</c>, <c>ToUpper()</c> and <c>StartsWith(string)</c>. Each reads the caller's
    /// culture or accepts Unicode digits that int.Parse rejects, and each has shipped here once --
    /// 38 ToUpper() call sites rejected the Dutch postcode 1234ij on a tr-TR thread, and the regex
    /// digit class let Arabic-Indic digits past a guard into a parse that threw.
    ///
    /// The rule was prose only. The other sweeps catch these defects through behaviour, one input
    /// at a time, so a new call site is caught only if someone thought to assert the value that
    /// exposes it; this reads the source instead and so cannot be outrun by a country nobody wrote
    /// a hostile row for. It found PeruValidator's EndsWith, which every existing sweep missed.
    ///
    /// Comments are stripped before the scan, because the reason a validator gives for using
    /// [0-9] rather than \d is written in the very characters this forbids.
    /// </summary>
    public class SourceConventionSweepTests
    {
        /// <summary>A construct that must not appear in shipped source, and what to write instead.</summary>
        private sealed class Banned
        {
            public Banned(string name, string pattern, string instead, Func<string, string, bool> permitted = null)
            {
                Name = name;
                Pattern = new Regex(pattern, RegexOptions.CultureInvariant);
                Instead = instead;
                Permitted = permitted ?? ((file, line) => false);
            }

            public string Name { get; }
            public Regex Pattern { get; }
            public string Instead { get; }

            /// <summary>The one place the construct is the right answer, by file and matched line.</summary>
            public Func<string, string, bool> Permitted { get; }
        }

        private static readonly Banned[] Rules =
        {
            new Banned(
                "ToUpper() / ToLower()",
                @"\.To(?:Upper|Lower)\(\s*\)",
                "ToUpperInvariant(), which does not fold i to the dotted I on a tr-TR thread"),

            new Banned(
                @"char.IsDigit / char.IsNumber",
                @"char\.Is(?:Digit|Number)\s*\(",
                "IsAsciiDigits() or IsAsciiDigit(), which match [0-9] and nothing else",
                // IdExtensions is where the sanctioned helpers live: RemoveSpecialCharacthers has to
                // recognise a non-ASCII digit in order to replace it with the sentinel.
                permitted: (file, line) => file == "IdExtensions.cs"),

            new Banned(
                @"\d in a pattern",
                @"\\d",
                "[0-9], because .NET's \\d also matches Arabic-Indic, Devanagari and fullwidth digits"),

            new Banned(
                "StartsWith / EndsWith without a StringComparison",
                @"\.(?:StartsWith|EndsWith)\s*\(",
                "StripPrefix(), or the same call with StringComparison.Ordinal",
                permitted: (file, line) => ArgumentsAt(line).Contains("StringComparison")),
        };

        public static IEnumerable<object[]> EveryShippedSourceFile()
        {
            foreach (string path in ShippedSourceFiles())
            {
                yield return new object[] { Relative(path) };
            }
        }

        [Theory]
        [MemberData(nameof(EveryShippedSourceFile))]
        public void NoFileUsesAConstructThatReadsTheCulture(string relativePath)
        {
            string[] lines = File.ReadAllLines(Path.Combine(RepositoryRoot(), relativePath));
            string fileName = Path.GetFileName(relativePath);
            var found = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string code = WithoutComments(lines[i]);

                foreach (Banned rule in Rules)
                {
                    foreach (Match match in rule.Pattern.Matches(code))
                    {
                        string tail = code.Substring(match.Index);
                        if (rule.Permitted(fileName, tail))
                        {
                            continue;
                        }

                        found.Add(string.Format(
                            "{0}({1}): {2} -- use {3}. See AGENTS.md rule 6.",
                            relativePath, i + 1, rule.Name, rule.Instead));
                    }
                }
            }

            Assert.True(found.Count == 0, string.Join(Environment.NewLine, found));
        }

        /// <summary>
        /// A check that cannot fail is worse than no check. If the scan ever stops finding the
        /// source tree -- a moved project, a renamed folder -- say so rather than reporting success
        /// over an empty set.
        /// </summary>
        [Fact]
        public void TheScanReachesTheWholeLibrary()
        {
            List<string> files = ShippedSourceFiles().ToList();

            Assert.True(
                files.Count >= 80,
                string.Format("The sweep found only {0} source files under {1}; it is broken.", files.Count, RepositoryRoot()));

            Assert.Contains(files, f => Path.GetFileName(f) == "PeruValidator.cs");
        }

        private static IEnumerable<string> ShippedSourceFiles()
        {
            string root = RepositoryRoot();

            foreach (string project in new[] { "Attest", "Attest.DataAnnotations" })
            {
                foreach (string path in Directory.GetFiles(Path.Combine(root, project), "*.cs", SearchOption.AllDirectories))
                {
                    // bin and obj hold generated assembly-info and the compiler's own scratch files.
                    if (path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) ||
                        path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                    {
                        continue;
                    }

                    yield return path;
                }
            }
        }

        private static string Relative(string absolute)
        {
            return absolute.Substring(RepositoryRoot().Length).TrimStart(Path.DirectorySeparatorChar);
        }

        private static string RepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Attest.sln")))
            {
                directory = directory.Parent;
            }

            Assert.True(directory != null, "Attest.sln is not above " + AppContext.BaseDirectory + "; the sweep cannot find the source.");
            return directory.FullName;
        }

        /// <summary>
        /// The line with its comment removed. A quote-aware scan rather than IndexOf("//"), because
        /// a // inside a string literal is code and a \d inside a comment is prose.
        /// </summary>
        private static string WithoutComments(string line)
        {
            var code = new StringBuilder(line.Length);
            bool inString = false;
            bool inChar = false;
            bool verbatim = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                char next = i + 1 < line.Length ? line[i + 1] : '\0';

                if (!inString && !inChar && c == '/' && next == '/')
                {
                    break;
                }

                if (inString)
                {
                    if (verbatim && c == '"' && next == '"')
                    {
                        code.Append(c).Append(next);
                        i++;
                        continue;
                    }

                    if (!verbatim && c == '\\')
                    {
                        code.Append(c).Append(next);
                        i++;
                        continue;
                    }

                    if (c == '"')
                    {
                        inString = false;
                        verbatim = false;
                    }
                }
                else if (inChar)
                {
                    if (c == '\\')
                    {
                        code.Append(c).Append(next);
                        i++;
                        continue;
                    }

                    if (c == '\'')
                    {
                        inChar = false;
                    }
                }
                else if (c == '"')
                {
                    inString = true;
                    verbatim = i > 0 && line[i - 1] == '@';
                }
                else if (c == '\'')
                {
                    inChar = true;
                }

                code.Append(c);
            }

            return code.ToString();
        }

        /// <summary>
        /// The argument list of the call the given text opens, to its balanced closing paren. Used
        /// so that one StringComparison on a line does not excuse a second call without one.
        /// </summary>
        private static string ArgumentsAt(string text)
        {
            int open = text.IndexOf('(');
            if (open < 0)
            {
                return string.Empty;
            }

            int depth = 0;

            for (int i = open; i < text.Length; i++)
            {
                if (text[i] == '(')
                {
                    depth++;
                }
                else if (text[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(open, i - open + 1);
                    }
                }
            }

            // Unbalanced: the call is split across lines. Return what there is, so a
            // StringComparison on a later line is not credited to this one.
            return text.Substring(open);
        }
    }
}
