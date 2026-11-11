using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Attest.Tests
{
    /// <summary>One line of a corpus file: a value and everything the library must say about it.</summary>
    internal sealed class CorpusRow
    {
        internal string TypeId = "";
        internal int Line;
        internal string Value = "";
        internal Verdict Verdict;
        internal Reason Reason;
        internal string Normalized = "-";
        internal string Formatted = "-";
        internal string Origin = "";
        internal string Note = "";

        internal string Where => TypeId + ".tsv:" + Line;
    }

    /// <summary>
    /// Reads Attest.Tests/corpus/&lt;id&gt;.tsv (format: Attest.Tests/corpus/README.md). Problems are
    /// collected, not thrown, so one run reports every malformed line.
    /// </summary>
    internal static class Corpus
    {
        internal const string PendingFile = "_pending.txt";

        internal static string Directory => Path.Combine(RepoRoot.Find(), "Attest.Tests", "corpus");

        internal static IEnumerable<string> Files()
        {
            return System.IO.Directory.GetFiles(Directory, "*.tsv").OrderBy(f => f, StringComparer.Ordinal);
        }

        internal static List<CorpusRow> Read(string path, List<string> problems, out int revision)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            var rows = new List<CorpusRow>();
            revision = 0;
            string[] lines = File.ReadAllText(path, new UTF8Encoding(false, true)).Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int number = i + 1;
                if (line.IndexOf('\r') >= 0)
                {
                    problems.Add(id + ".tsv:" + number + " has a CR; files are LF.");
                    line = line.TrimEnd('\r');
                }

                if (line.Length == 0)
                {
                    continue;
                }

                if (line[0] == '#')
                {
                    string header = line.TrimStart('#', ' ');
                    if (header.StartsWith(id + " ", StringComparison.Ordinal) && header.Contains("revision="))
                    {
                        string n = header.Substring(header.IndexOf("revision=", StringComparison.Ordinal) + "revision=".Length).Split(' ')[0];
                        int.TryParse(n, NumberStyles.None, CultureInfo.InvariantCulture, out revision);
                    }

                    continue;
                }

                string[] f = line.Split('\t');
                if (f.Length != 7)
                {
                    problems.Add(id + ".tsv:" + number + " has " + f.Length + " fields, expected 7.");
                    continue;
                }

                Verdict verdict;
                Reason reason;
                if (!Enum.TryParse(f[1], true, out verdict) || f[1] != f[1].ToLowerInvariant())
                {
                    problems.Add(id + ".tsv:" + number + " verdict \"" + f[1] + "\" is not valid, invalid or notcovered.");
                    continue;
                }

                if (f[2] == "-")
                {
                    reason = Reason.None;
                }
                else if (!Enum.TryParse(f[2], false, out reason) || reason == Reason.None)
                {
                    problems.Add(id + ".tsv:" + number + " reason \"" + f[2] + "\" is not a Reason member or -.");
                    continue;
                }

                rows.Add(new CorpusRow
                {
                    TypeId = id,
                    Line = number,
                    Value = Unescape(f[0]),
                    Verdict = verdict,
                    Reason = reason,
                    Normalized = f[3],
                    Formatted = f[4],
                    Origin = f[5],
                    Note = f[6],
                });
            }

            return rows;
        }

        /// <summary>Origin vocabulary (DESIGN.md 6.2); returns a problem or null.</summary>
        internal static string OriginProblem(CorpusRow row)
        {
            bool published = row.Origin.StartsWith("published:https://", StringComparison.Ordinal) && row.Origin.Length > "published:https://".Length;
            bool computed = row.Origin.StartsWith("computed:", StringComparison.Ordinal) && row.Origin.Length > "computed:".Length;
            bool synthetic = row.Origin == "synthetic";

            if (!published && !computed && !synthetic)
            {
                return row.Where + " origin \"" + row.Origin + "\" is not published:<https url>, computed:<method> or synthetic.";
            }

            if (row.Verdict == Verdict.Valid && synthetic)
            {
                return row.Where + " is valid with origin synthetic; a valid row must be published or computed.";
            }

            return null;
        }

        /// <summary>Values may carry tab, newline or leading/trailing space as \t, \n, \s.</summary>
        private static string Unescape(string field)
        {
            return field.Replace("\\t", "\t").Replace("\\n", "\n").Replace("\\s", " ").Replace("\\\\", "\\");
        }
    }
}
