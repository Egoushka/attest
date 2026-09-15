using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// The country table is the first thing an evaluating consumer reads, and it ships inside both
    /// packages. It has drifted twice: five countries were registered, tested and missing from it,
    /// and eight cells contradicted the code -- Taiwan's Unified Business Number was marked as not
    /// supported while its validator and its tests were both there, Mauritius claimed a postal code
    /// rule it has never had.
    ///
    /// A shell step in ci.yml used to check that every registered country had a row. This checks
    /// that and the reverse, and that every ":x:" means what it says, which is what actually went
    /// wrong. Prose can drift from code; a table of claims about code should not have to.
    /// </summary>
    public class ReadmeTableTests
    {
        private static readonly Regex Row = new Regex(@"^\|(?<cells>.*)\|\s*$", RegexOptions.Compiled);

        /// <summary>The column of the table each kind is claimed in.</summary>
        private static readonly (int Column, IdentifierKind Kind, string Name)[] Columns =
        {
            (2, IdentifierKind.PersonalId, "National Identification Number Name"),
            (3, IdentifierKind.Vat, "VAT Code"),
            (4, IdentifierKind.CompanyNumber, "Entity code"),
            (5, IdentifierKind.PostalCode, "Postal Code"),
        };

        private const string NotSupported = ":x:";

        [Fact]
        public void EveryRegisteredCountryHasARowAndEveryRowIsARegisteredCountry()
        {
            Dictionary<string, string[]> rows = TableRows();
            var registered = RegisteredCountries().Select(c => c.ToString()).ToList();

            Assert.True(registered.Count >= 50, "Only " + registered.Count + " countries are registered; the scan is broken.");

            var missing = registered.Where(c => !rows.ContainsKey(c)).ToList();
            Assert.True(missing.Count == 0, "Registered but absent from the README table: " + string.Join(", ", missing));

            var extra = rows.Keys.Where(c => !registered.Contains(c)).ToList();
            Assert.True(extra.Count == 0, "In the README table but not registered: " + string.Join(", ", extra));
        }

        [Fact]
        public void TheTableMarksExactlyTheKindsWithNoRule()
        {
            var validator = new CountryValidator();
            Dictionary<string, string[]> rows = TableRows();
            var wrong = new List<string>();

            foreach (Country country in RegisteredCountries())
            {
                string[] cells = rows[country.ToString()];

                foreach ((int column, IdentifierKind kind, string name) in Columns)
                {
                    bool claimsNone = cells[column] == NotSupported;
                    bool hasNone = !validator.Supports(country, kind);

                    if (claimsNone != hasNone)
                    {
                        wrong.Add(string.Format(
                            "{0} {1}: the table says {2}, the code says {3}.",
                            country, name,
                            claimsNone ? "no rule" : "a rule (\"" + cells[column] + "\")",
                            hasNone ? "no rule" : "a rule"));
                    }
                }
            }

            Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
        }

        private static IEnumerable<Country> RegisteredCountries()
        {
            return Enum.GetValues(typeof(Country))
                .Cast<Country>()
                .Where(CountryValidator.IsCountrySupported)
                .OrderBy(c => c.ToString());
        }

        /// <summary>The table keyed by alpha-2 code, each value the row's cells, trimmed.</summary>
        private static Dictionary<string, string[]> TableRows()
        {
            var rows = new Dictionary<string, string[]>(StringComparer.Ordinal);

            foreach (string line in File.ReadAllLines(Path.Combine(RepositoryRoot(), "README.md")))
            {
                Match match = Row.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                string[] cells = match.Groups["cells"].Value.Split('|').Select(c => c.Trim()).ToArray();
                if (cells.Length >= 6 && Regex.IsMatch(cells[1], "^[A-Z]{2}$"))
                {
                    rows[cells[1]] = cells;
                }
            }

            Assert.True(rows.Count >= 50, "Found only " + rows.Count + " rows in the README table; the parser is broken.");
            return rows;
        }

        private static string RepositoryRoot()
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
