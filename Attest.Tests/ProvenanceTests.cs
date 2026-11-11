using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// The provenance table (Attest/Registry/Provenance.tsv) is data; these tests are what keep it
    /// honest: every covered type cites an https source, no date is in the future, and every known-gap
    /// id names a real entry in KNOWN-ISSUES.md.
    /// </summary>
    public class ProvenanceTests
    {
        /// <summary>Covered types for which no authority source could be fetched; listed, not hidden. Each is named in the PR that added it.</summary>
        private static readonly HashSet<string> NoSourceAllowList = new HashSet<string>(StringComparer.Ordinal)
        {
            "cu.postalcode", "il.vat", "py.postalcode",
        };

        private static readonly string[] Slugs = { "nationalid", "taxcode", "company", "vat", "postalcode" };

        private static bool IsCovered(IdentifierType t)
        {
            return t.Rule.Coverage == Coverage.Full || t.Rule.Coverage == Coverage.FormatOnly;
        }

        [Fact]
        public void EveryCoveredTypeCitesAnHttpsSource()
        {
            var missing = TestTypes.Pairs
                .Where(t => IsCovered(t) && !NoSourceAllowList.Contains(t.Id))
                .Where(t => !t.Rule.Sources.Any(s => s.Url.StartsWith("https://", StringComparison.Ordinal)))
                .Select(t => t.Id).ToList();
            Assert.True(missing.Count == 0, "Covered types with no https source: " + string.Join(", ", missing));

            var stale = NoSourceAllowList.Where(id => TestTypes.Pairs.Any(t => t.Id == id && t.Rule.Sources.Count > 0)).ToList();
            Assert.True(stale.Count == 0, "Remove from the allow-list, they have sources now: " + string.Join(", ", stale));
        }

        [Fact]
        public void SourcesAreWellFormedAndNotDatedInTheFuture()
        {
            DateTime limit = DateTime.UtcNow.Date.AddDays(1);
            foreach (IdentifierType type in TestTypes.Pairs)
            {
                var urls = new HashSet<string>(StringComparer.Ordinal);
                foreach (Source s in type.Rule.Sources)
                {
                    Assert.True(Uri.IsWellFormedUriString(s.Url, UriKind.Absolute) || Uri.TryCreate(s.Url, UriKind.Absolute, out _), type.Id + ": bad url " + s.Url);
                    Assert.True(s.Url.StartsWith("https://", StringComparison.Ordinal) || s.Url.StartsWith("http://", StringComparison.Ordinal), type.Id + ": not http(s) " + s.Url);
                    Assert.False(string.IsNullOrWhiteSpace(s.Authority), type.Id + ": empty authority");
                    Assert.False(string.IsNullOrWhiteSpace(s.Title), type.Id + ": empty title");
                    Assert.True(s.Accessed < limit, type.Id + ": Accessed in the future for " + s.Url);
                    Assert.True(s.Accessed.Year >= 2020, type.Id + ": implausible Accessed for " + s.Url);
                    Assert.True(urls.Add(s.Url), type.Id + ": duplicate source " + s.Url);
                }

                if (type.Rule.LastVerified.HasValue)
                {
                    Assert.True(type.Rule.LastVerified.Value < limit, type.Id + ": LastVerified in the future");
                }
            }
        }

        [Fact]
        public void RevisionsArePositive()
        {
            Assert.All(TestTypes.Pairs, t => Assert.True(t.Rule.Revision >= 1, t.Id));
        }

        [Fact]
        public void IdsAreUniqueAndEveryCountryHasAllFiveKinds()
        {
            Assert.Equal(TestTypes.Pairs.Count, TestTypes.Pairs.Select(t => t.Id).Distinct().Count());
            foreach (var group in TestTypes.Pairs.GroupBy(t => t.Country))
            {
                var slugs = group.Select(t => t.Id.Substring(t.Id.IndexOf('.') + 1)).OrderBy(s => s, StringComparer.Ordinal).ToList();
                Assert.True(
                    slugs.SequenceEqual(Slugs.OrderBy(s => s, StringComparer.Ordinal)),
                    group.Key + " has kinds: " + string.Join(", ", slugs));
            }
        }

        [Fact]
        public void EveryRowInTheTableNamesARealType()
        {
            var known = new HashSet<string>(Identifiers.All.Select(t => t.Id), StringComparer.Ordinal);
            var unknown = Provenance.Ids.Where(id => !known.Contains(id)).ToList();
            Assert.True(unknown.Count == 0, "Provenance.tsv names types that do not exist: " + string.Join(", ", unknown));
        }

        [Fact]
        public void TheEmptyPairsAreResolvedOrStayNotCovered()
        {
            // Of the 25 pairs without a rule: 3 gained one, 4 are NotApplicable (the country issues
            // no such identifier), 18 remain NotCovered with a known gap or a published rule not yet written.
            Assert.Equal(18, TestTypes.Pairs.Count(t => t.Rule.Coverage == Coverage.NotCovered));
            Assert.Equal(
                new[] { "ae.postalcode", "hk.postalcode", "hk.vat", "us.vat" },
                TestTypes.Pairs.Where(t => t.Rule.Coverage == Coverage.NotApplicable).Select(t => t.Id).OrderBy(i => i, StringComparer.Ordinal).ToArray());
            Assert.All(
                TestTypes.Pairs.Where(t => t.Rule.Coverage == Coverage.NotApplicable),
                t => Assert.Equal(Verdict.NotCovered, t.Validate("1234567").Verdict));
            Assert.All(
                TestTypes.Pairs.Where(t => t.Rule.Coverage == Coverage.NotCovered),
                t => Assert.Equal(Verdict.NotCovered, t.Validate("1234567").Verdict));
            Assert.All(
                TestTypes.Pairs.Where(t => t.Rule.Coverage == Coverage.NotCovered),
                t => Assert.True(t.Rule.KnownGaps.Count > 0, t.Id + " is NotCovered and names no known gap."));
        }

        [Fact]
        public void TheCountriesAddedIn2Point0HaveFiveKindsAndEveryUnruledTypeNamesAGap()
        {
            Assert.Equal(13, TestTypes.NativePairs.Select(t => t.Country).Distinct().Count());
            foreach (var group in TestTypes.NativePairs.GroupBy(t => t.Country))
            {
                Assert.Equal(5, group.Count());
            }

            Assert.All(
                TestTypes.NativePairs.Where(t => t.Rule.Coverage == Coverage.NotCovered),
                t =>
                {
                    Assert.True(t.Rule.KnownGaps.Count > 0, t.Id);
                    Assert.Equal(Verdict.NotCovered, t.Validate("1234567").Verdict);
                });
            Assert.Equal(
                new[] { "ke.company", "ke.taxcode", "ke.vat", "ma.company", "mz.company", "mz.taxcode", "mz.vat", "vn.company", "vn.taxcode", "vn.vat" },
                TestTypes.NativePairs.Where(t => t.Rule.Coverage == Coverage.FormatOnly).Select(t => t.Id).OrderBy(i => i, StringComparer.Ordinal).ToArray());
            Assert.Equal("1234567890-123", Identifiers.Find("vn.company").Format("1234567890123"));
            Assert.True(Identifiers.Find("vn.company").Validate("1234567890-123").IsValid);
            Assert.False(Identifiers.Find("ke.vat").Validate("B123456789Z").IsValid);
            Assert.True(Identifiers.Find("ma.company").Validate("123456789 0001 12").IsValid);
            Assert.True(Identifiers.Find("mz.taxcode").Validate("123456789").IsValid);
            Assert.False(Identifiers.Find("mz.taxcode").Validate("12345678").IsValid);
        }

        [Fact]
        public void KnownGapsLinkToRealEntriesOfTheSameCountryAndEveryEntryIsLinked()
        {
            string text = File.ReadAllText(Path.Combine(RepoRoot.Find(), "KNOWN-ISSUES.md"));
            var entries = Regex.Matches(text, @"^- \*\*(KI-[A-Z0-9]{2,8}-[0-9]+)\*\*", RegexOptions.Multiline)
                .Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            Assert.True(entries.Count >= 30, "Found only " + entries.Count + " KNOWN-ISSUES.md entries with ids; the scan is broken.");
            Assert.Equal(entries.Count, entries.Distinct().Count());

            var linked = new HashSet<string>(StringComparer.Ordinal);
            foreach (IdentifierType type in Identifiers.All)
            {
                string prefix = "KI-" + (type.Country != null ? type.Country.ToString() : type.Id.ToUpperInvariant()) + "-";
                foreach (string gap in type.Rule.KnownGaps)
                {
                    Assert.True(entries.Contains(gap), type.Id + " links to " + gap + ", which is not in KNOWN-ISSUES.md");
                    Assert.True(
                        gap.StartsWith(prefix, StringComparison.Ordinal),
                        type.Id + " links to " + gap + ", an entry of another country");
                    linked.Add(gap);
                }
            }

            var orphans = entries.Where(e => !linked.Contains(e)).ToList();
            Assert.True(orphans.Count == 0, "KNOWN-ISSUES.md entries no type links to: " + string.Join(", ", orphans));
        }
    }
}
