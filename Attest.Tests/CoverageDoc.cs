using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Attest;

namespace Attest.Tests
{
    /// <summary>Renders docs/COVERAGE.md from the registry. Nothing in it is typed by hand.</summary>
    internal static class CoverageDoc
    {
        private static readonly SourceKind[] Strong = { SourceKind.Authority, SourceKind.Law, SourceKind.Oecd };

        internal static string Render()
        {
            IReadOnlyList<IdentifierType> all = Registry.All;
            var sb = new StringBuilder();
            sb.Append("<!-- Generated from the registry by Attest.Tests.CoverageDocTests. Do not edit; run the tests with ATTEST_UPDATE_COVERAGE=1 to regenerate. -->\n");
            sb.Append("# Coverage\n\n");

            sb.Append("- Types: ").Append(all.Count).Append(" (").Append(all.Select(t => t.Country).Distinct().Count()).Append(" countries x 5 kinds)\n");
            sb.Append("- Coverage: ");
            sb.Append(string.Join(", ", Enum.GetValues(typeof(Coverage)).Cast<Coverage>()
                .Select(c => c + " " + all.Count(t => t.Rule.Coverage == c))));
            sb.Append("\n");
            sb.Append("- Last verified: null for ").Append(all.Count(t => t.Rule.LastVerified == null)).Append(" of ").Append(all.Count).Append("\n");
            sb.Append("- Sources: ").Append(all.Sum(t => t.Rule.Sources.Count)).Append(" citations; ")
              .Append(all.Count(t => t.Rule.Sources.Any(s => Strong.Contains(s.Kind)))).Append(" types cite an authority, law or OECD source, ")
              .Append(all.Count(t => t.Rule.Sources.Count > 0 && !t.Rule.Sources.Any(s => Strong.Contains(s.Kind)))).Append(" only a reference implementation or secondary source, ")
              .Append(all.Count(t => t.Rule.Sources.Count == 0)).Append(" none\n");
            sb.Append("- Native Normalize / Format / Parse: ")
              .Append(all.Count(t => Overrides(t, "Normalize"))).Append(" / ")
              .Append(all.Count(t => Overrides(t, "Format"))).Append(" / ")
              .Append(all.Count(t => Overrides(t, "Parse"))).Append("\n");
            sb.Append("- Types with known gaps: ").Append(all.Count(t => t.Rule.KnownGaps.Count > 0)).Append("\n\n");

            sb.Append("Sources are authority@host (kind: A authority, L law, O OECD, R reference implementation, S secondary); full URLs are in Attest/Registry/Provenance.tsv. N/F/P: a native Normalize / Format / Parse (y/-).\n\n");
            sb.Append("| Id | Name | Kind | Country | Coverage | Sources | Verified | Rev | N/F/P | Gaps |\n");
            sb.Append("|---|---|---|---|---|---|---|---|---|---|\n");

            foreach (IdentifierType type in all.OrderBy(t => t.Id, StringComparer.Ordinal))
            {
                RuleInfo info = type.Rule;
                sb.Append("| ").Append(type.Id)
                  .Append(" | ").Append(Cell(type.Name))
                  .Append(" | ").Append(type.Kind)
                  .Append(" | ").Append(type.Country)
                  .Append(" | ").Append(info.Coverage)
                  .Append(" | ").Append(Cell(string.Join("; ", info.Sources.Select(Describe))))
                  .Append(" | ").Append(info.LastVerified.HasValue ? info.LastVerified.Value.ToString("yyyy-MM-dd") : "null")
                  .Append(" | ").Append(info.Revision)
                  .Append(" | ").Append(Overrides(type, "Normalize") ? "y" : "-")
                  .Append(Overrides(type, "Format") ? "y" : "-")
                  .Append(Overrides(type, "Parse") ? "y" : "-")
                  .Append(" | ").Append(string.Join(", ", info.KnownGaps))
                  .Append(" |\n");
            }

            return sb.ToString();
        }

        private static string Describe(Source source)
        {
            string host = new Uri(source.Url).Host;
            if (host.StartsWith("www.", StringComparison.Ordinal))
            {
                host = host.Substring(4);
            }

            return source.Authority + "@" + host + " " + source.Kind.ToString().Substring(0, 1);
        }

        private static string Cell(string text)
        {
            return text.Replace("|", "/");
        }

        /// <summary>Whether the type's rule declares its own version of the operation.</summary>
        internal static bool Overrides(IdentifierType type, string method)
        {
            Rule rule = type.Implementation;
            return method == "Normalize" ? rule.NativeNormalize : method == "Format" ? rule.NativeFormat : rule.NativeParse;
        }
    }
}
