using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// The test-vector corpus (DESIGN.md 6.2): data files, one per identifier type, traced to an
    /// origin. These tests are the harness; the rows live in Attest.Tests/corpus.
    /// </summary>
    public class CorpusTests
    {
        [Fact]
        public void EveryRowIsWellFormedAndItsOriginIsAllowed()
        {
            var problems = new List<string>();
            int rows = 0;
            foreach (string path in Corpus.Files())
            {
                int revision;
                foreach (CorpusRow row in Corpus.Read(path, problems, out revision))
                {
                    rows++;
                    string problem = Corpus.OriginProblem(row);
                    if (problem != null)
                    {
                        problems.Add(problem);
                    }

                    if (row.Verdict == Verdict.Valid && (row.Reason != Reason.None || row.Normalized == "-" || row.Formatted == "-"))
                    {
                        problems.Add(row.Where + " a valid row has no reason and states both the normalized and the printed form.");
                    }

                    if (row.Verdict == Verdict.NotCovered && row.Reason != Reason.None)
                    {
                        problems.Add(row.Where + " a notcovered row has no reason.");
                    }

                    if (row.Verdict == Verdict.Invalid && row.Reason == Reason.None)
                    {
                        problems.Add(row.Where + " an invalid row must name its Reason.");
                    }
                }
            }

            Assert.True(rows > 0, "No corpus rows were found; the loader is broken.");
            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        }

        [Fact]
        public void EveryRowIsWhatTheLibrarySays()
        {
            var problems = new List<string>();
            foreach (string path in Corpus.Files())
            {
                string id = Path.GetFileNameWithoutExtension(path);
                IdentifierType type = Identifiers.Find(id);
                if (type == null)
                {
                    continue; // reported by NoFileNamesAnUnknownType
                }

                int revision;
                foreach (CorpusRow row in Corpus.Read(path, problems, out revision))
                {
                    IdentifierResult result = type.Validate(row.Value);
                    if (result.Verdict != row.Verdict)
                    {
                        problems.Add(row.Where + " \"" + row.Value + "\": expected " + row.Verdict + ", got " + result.Verdict + " (" + result.Reason + ").");
                        continue;
                    }

                    if (result.Reason != row.Reason)
                    {
                        problems.Add(row.Where + " \"" + row.Value + "\": expected reason " + row.Reason + ", got " + result.Reason + ".");
                    }

                    if (row.Verdict == Verdict.Valid)
                    {
                        if (type.Normalize(row.Value) != row.Normalized)
                        {
                            problems.Add(row.Where + " \"" + row.Value + "\": expected normalized \"" + row.Normalized + "\", got \"" + type.Normalize(row.Value) + "\".");
                        }

                        if (type.Format(row.Value) != row.Formatted)
                        {
                            problems.Add(row.Where + " \"" + row.Value + "\": expected printed \"" + row.Formatted + "\", got \"" + type.Format(row.Value) + "\".");
                        }
                    }
                    else
                    {
                        if (row.Normalized != "-" && type.Normalize(row.Value) != row.Normalized)
                        {
                            problems.Add(row.Where + " \"" + row.Value + "\": expected normalized \"" + row.Normalized + "\", got \"" + type.Normalize(row.Value) + "\".");
                        }

                        if (type.Format(row.Value) != null)
                        {
                            problems.Add(row.Where + " \"" + row.Value + "\": Format of a rejected value must be null.");
                        }
                    }
                }
            }

            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        }

        [Fact]
        public void NoFileNamesAnUnknownTypeOrIsStaleOrOneSided()
        {
            var problems = new List<string>();
            foreach (string path in Corpus.Files())
            {
                string id = Path.GetFileNameWithoutExtension(path);
                IdentifierType type = Identifiers.Find(id);
                if (type == null || type.Id != id)
                {
                    problems.Add(id + ".tsv names no registered type id.");
                    continue;
                }

                int revision;
                List<CorpusRow> rows = Corpus.Read(path, problems, out revision);
                if (revision != type.Rule.Revision)
                {
                    problems.Add(id + ".tsv says revision=" + revision + " but the rule is at revision " + type.Rule.Revision
                        + ". A revision bump means a verdict can change: review the rows, then update the header.");
                }

                if (type.Rule.Coverage == Coverage.Full || type.Rule.Coverage == Coverage.FormatOnly)
                {
                    if (!rows.Any(r => r.Verdict == Verdict.Valid) || !rows.Any(r => r.Verdict == Verdict.Invalid))
                    {
                        problems.Add(id + ".tsv needs at least one valid and one invalid row.");
                    }
                }
                else if (rows.Any(r => r.Verdict != Verdict.NotCovered))
                {
                    problems.Add(id + " has no rule (" + type.Rule.Coverage + "); its rows must all be notcovered.");
                }
            }

            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        }

        /// <summary>
        /// Migration is country by country (DESIGN.md 9). Until it is done the types without a file
        /// are listed in corpus/_pending.txt. The list can only shrink: a type that gains a file
        /// must leave it, and a new type must come with a file. Regenerate:
        /// ATTEST_UPDATE_CORPUS_PENDING=1 dotnet test --filter CorpusTests.
        /// </summary>
        [Fact]
        public void TheListOfTypesWithoutACorpusFileIsExact()
        {
            var have = new HashSet<string>(Corpus.Files().Select(Path.GetFileNameWithoutExtension), StringComparer.Ordinal);
            string expected = string.Join("\n", Identifiers.All
                .Where(t => t.Rule.Coverage == Coverage.Full || t.Rule.Coverage == Coverage.FormatOnly)
                .Select(t => t.Id)
                .Where(id => !have.Contains(id))
                .OrderBy(id => id, StringComparer.Ordinal)) + "\n";

            string path = Path.Combine(Corpus.Directory, Corpus.PendingFile);
            if (Environment.GetEnvironmentVariable("ATTEST_UPDATE_CORPUS_PENDING") == "1")
            {
                File.WriteAllText(path, expected);
            }

            Assert.True(File.Exists(path), "corpus/_pending.txt is missing. Regenerate: ATTEST_UPDATE_CORPUS_PENDING=1 dotnet test --filter CorpusTests");
            Assert.True(
                File.ReadAllText(path).Replace("\r\n", "\n") == expected,
                "corpus/_pending.txt does not match the types that lack a corpus file. Regenerate: ATTEST_UPDATE_CORPUS_PENDING=1 dotnet test --filter CorpusTests");
        }
    }
}
