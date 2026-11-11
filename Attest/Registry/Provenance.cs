#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Attest
{
    /// <summary>
    /// The provenance of every identifier type, read from the embedded table Provenance.tsv
    /// (sources, known gaps, coverage, last verified, revision). It is data, not code: adding a
    /// source or bumping a revision is a one-line edit to that file, and the format is documented
    /// in its header.
    /// </summary>
    internal static class Provenance
    {
        private const string ResourceName = "Attest.Provenance.tsv";

        private static readonly Dictionary<string, Entry> Entries = Load();

        /// <summary>
        /// The provenance of one type. <paramref name="supported"/> says whether the 1.x validator
        /// has a rule for it, which decides the coverage when the table does not override it.
        /// </summary>
        internal static RuleInfo For(string id, bool supported)
        {
            Entry? entry;
            Entries.TryGetValue(id, out entry);
            Coverage coverage = entry != null && entry.Coverage.HasValue
                ? entry.Coverage.Value
                : (supported ? Coverage.Full : Coverage.NotCovered);

            if (entry == null)
            {
                return new RuleInfo(new Source[0], null, 1, new string[0], coverage);
            }

            return new RuleInfo(entry.Sources.ToArray(), entry.LastVerified, entry.Revision, entry.KnownGaps.ToArray(), coverage);
        }

        /// <summary>Ids that appear in the table, for the test that every row names a real type.</summary>
        internal static IEnumerable<string> Ids => Entries.Keys;

        private sealed class Entry
        {
            internal readonly List<Source> Sources = new List<Source>();
            internal readonly List<string> KnownGaps = new List<string>();
            internal Coverage? Coverage;
            internal DateTime? LastVerified;
            internal int Revision = 1;
        }

        private static Dictionary<string, Entry> Load()
        {
            var entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
            Assembly assembly = typeof(Provenance).GetTypeInfo().Assembly;
            using (Stream? stream = assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Embedded resource " + ResourceName + " is missing.");
                }

                using (var reader = new StreamReader(stream))
                {
                    string? line;
                    int number = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        number++;
                        if (line.Length == 0 || line[0] == '#')
                        {
                            continue;
                        }

                        try
                        {
                            Apply(entries, line.Split('\t'));
                        }
                        catch (Exception e) when (e is FormatException || e is IndexOutOfRangeException || e is ArgumentException)
                        {
                            throw new InvalidOperationException("Provenance.tsv line " + number + ": " + e.Message, e);
                        }
                    }
                }
            }

            return entries;
        }

        private static void Apply(Dictionary<string, Entry> entries, string[] f)
        {
            foreach (string id in ExpandIds(f[1]))
            {
                Entry? entry;
                if (!entries.TryGetValue(id, out entry))
                {
                    entry = new Entry();
                    entries.Add(id, entry);
                }

                switch (f[0])
                {
                    case "S":
                        entry.Sources.Add(new Source(
                            f[2],
                            f[3],
                            f[4],
                            ParseDate(f[5]),
                            (SourceKind)Enum.Parse(typeof(SourceKind), f[6])));
                        break;
                    case "G":
                        entry.KnownGaps.Add(f[2]);
                        break;
                    case "C":
                        entry.Coverage = (Coverage)Enum.Parse(typeof(Coverage), f[2]);
                        break;
                    case "V":
                        entry.LastVerified = ParseDate(f[2]);
                        break;
                    case "R":
                        entry.Revision = int.Parse(f[2], CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new FormatException("unknown row type '" + f[0] + "'");
                }
            }
        }

        /// <summary>"be:vat,company" becomes "be.vat" and "be.company"; an id without a colon ("iban") is an international type.</summary>
        private static IEnumerable<string> ExpandIds(string ids)
        {
            int colon = ids.IndexOf(':');
            if (colon < 0)
            {
                yield return ids;
                yield break;
            }

            string country = ids.Substring(0, colon);
            foreach (string kind in ids.Substring(colon + 1).Split(','))
            {
                yield return country + "." + kind;
            }
        }

        private static DateTime ParseDate(string value)
        {
            return DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}
