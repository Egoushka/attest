using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Attest.Tests
{
    /// <summary>
    /// Normalize and Format are only safe if they never turn a valid value into an invalid one.
    /// Dropping a '-' is wrong where the dash carries meaning (a Finnish century sign), and a
    /// printed form that the type itself rejects is worse than no printed form. This reads every
    /// row of every country test class that the type accepts as written, and holds the three
    /// properties DESIGN.md section 3.3 promises against it:
    /// the normalized value is valid, Normalize is idempotent, and the printed form
    /// normalizes back to the same value and prints the same.
    /// </summary>
    public class NormalizeFormatSweepTests
    {
        private static IEnumerable<(IdentifierType Type, string Value)> AcceptedRows()
        {
            foreach (Type testClass in typeof(NormalizeFormatSweepTests).Assembly.GetTypes()
                .Where(t => t.Name.EndsWith("ValidatorTests", StringComparison.Ordinal)))
            {
                string name = testClass.Name.Substring(0, testClass.Name.Length - "ValidatorTests".Length);
                Country? country = Registry.Validators
                    .Where(p => p.Value.GetType().Name == name + "Validator")
                    .Select(p => (Country?)p.Key)
                    .FirstOrDefault();
                if (country == null)
                {
                    continue;
                }

                foreach (MethodInfo method in testClass.GetMethods())
                {
                    foreach (DataAttribute data in method.GetCustomAttributes<InlineDataAttribute>())
                    {
                        foreach (object[] row in data.GetData(method))
                        {
                            if (row == null || row.Length == 0 || !(row[0] is string value) || !row.Skip(1).Any(a => a is bool b && b))
                            {
                                continue;
                            }

                            foreach (IdentifierType type in Registry.For(country.Value))
                            {
                                if (type.Validate(value).IsValid)
                                {
                                    yield return (type, value);
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public void NormalizingAValidValueKeepsItValidAndPrintingItRoundTrips()
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var failures = new List<string>();
            int rows = 0;

            foreach ((IdentifierType type, string value) in AcceptedRows())
            {
                if (!seen.Add(type.Id + "|" + value))
                {
                    continue;
                }

                rows++;
                string normalized = type.Normalize(value);
                if (normalized == null || !type.Validate(normalized).IsValid)
                {
                    failures.Add(type.Id + " Normalize(\"" + value + "\") = \"" + normalized + "\" is not valid");
                    continue;
                }

                if (type.Normalize(normalized) != normalized)
                {
                    failures.Add(type.Id + " Normalize is not idempotent on \"" + value + "\"");
                }

                string printed = type.Format(value);
                if (printed == null)
                {
                    failures.Add(type.Id + " Format(\"" + value + "\") is null for a valid value");
                    continue;
                }

                if (type.Format(normalized) != printed)
                {
                    failures.Add(type.Id + " Format differs between \"" + value + "\" and its normalized form");
                }

                if (!type.Validate(printed).IsValid)
                {
                    failures.Add(type.Id + " printed form \"" + printed + "\" is not valid");
                }
                else if (type.Normalize(printed) != normalized)
                {
                    failures.Add(type.Id + " printed form \"" + printed + "\" does not normalize back to \"" + normalized + "\"");
                }

                if (type.Format(printed) != printed)
                {
                    failures.Add(type.Id + " Format is not idempotent on \"" + printed + "\"");
                }
            }

            Assert.True(rows > 500, "Only " + rows + " accepted rows were found; the scan is broken.");
            Assert.True(failures.Count == 0, failures.Count + " failures of " + rows + " rows:" + Environment.NewLine + string.Join(Environment.NewLine, failures.Take(60)));
        }


        // Printed forms. The ABN and Eircode examples are the ones the authority prints; the NINO layout is HMRC's (its own QQ123456A example is not a valid number); the
        // CNPJ and the Belgian number are values the country tests already prove valid.
        [Theory]
        [InlineData("au.vat", "51824753556", "51 824 753 556")]
        [InlineData("au.company", "51 824 753 556", "51 824 753 556")]
        [InlineData("br.company", "16.727.230/0001-97", "16.727.230/0001-97")]
        [InlineData("br.company", "16727230000197", "16.727.230/0001-97")]
        [InlineData("be.company", "0428759497", "0428.759.497")]
        [InlineData("gb.nationalid", "AB123456C", "AB 12 34 56 C")]
        [InlineData("gb.postalcode", "sw1a1aa", "SW1A 1AA")]
        [InlineData("ie.postalcode", "A65F4E2", "A65 F4E2")]
        [InlineData("us.postalcode", "99999 9999", "99999-9999")]
        public void FormatPrintsTheAuthoritysForm(string id, string input, string expected)
        {
            Assert.Equal(expected, Identifiers.Find(id).Format(input));
        }

        [Theory]
        [InlineData("be.vat", "BE0428759497")]       // no printed form of its own: compact
        [InlineData("gb.taxcode", "AB123456C")]      // KI-GB-1
        public void TypesWithoutAPrintedFormStayCompact(string id, string input)
        {
            Assert.Equal(input, Identifiers.Find(id).Format(input));
        }

        [Fact]
        public void NormalizeKeepsWhatCarriesMeaning()
        {
            Assert.Equal("131052-308T", Identifiers.Find("fi.nationalid").Normalize("131052-308t"));
            Assert.Equal("122119m", Identifiers.Find("at.company").Normalize("FN 122119m"));
            Assert.Equal("122119M", Identifiers.Find("at.company").Normalize("fn 122119M"));
            Assert.False(Identifiers.Find("at.company").Validate("122119M").IsValid);
            Assert.Equal("FN 122119m", Identifiers.Find("at.company").Format("122119m"));
            Assert.Equal("99999-9999", Identifiers.Find("us.postalcode").Normalize("99999 9999"));
            Assert.Equal("999999999", Identifiers.Find("us.postalcode").Normalize("999999999"));
        }
    }
}
