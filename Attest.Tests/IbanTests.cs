using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using IbanNet;
using IbanNet.Registry;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// IBAN is Attest's own rule (zero dependencies). IbanNet is referenced by this test project
    /// only, as a differential oracle: its registry and ours must agree, country by country.
    /// </summary>
    public class IbanTests
    {
        private static readonly IdentifierType Iban = Identifiers.Find("iban");

        // MOD 97-10 written out independently of Attest.Algorithms: BigInteger on the rearranged digits.
        private static string WithCheckDigits(string country, string bban)
        {
            if (bban.Any(ch => !char.IsAsciiLetterOrDigit(ch)))
            {
                return country + "97" + bban; // no check digits can be computed over a symbol; both must reject
            }

            string rearranged = bban + country + "00";
            var digits = new StringBuilder();
            foreach (char c in rearranged)
            {
                digits.Append(c >= 'A' ? (c - 'A' + 10).ToString() : c.ToString());
            }

            int check = 98 - (int)(BigInteger.Parse(digits.ToString()) % 97);
            return country + check.ToString("00") + bban;
        }

        private static string Sub(string s, int index, char c)
        {
            return s.Substring(0, index) + c + s.Substring(index + 1);
        }

        [Fact]
        public void IsRegisteredAsAnInternationalBankAccountType()
        {
            Assert.NotNull(Iban);
            Assert.Null(Iban.Country);
            Assert.Equal(IdentifierKind.BankAccount, Iban.Kind);
            Assert.Equal(Coverage.Full, Iban.Coverage);
            Assert.DoesNotContain(Iban, Identifiers.For(Country.DE));
            Assert.Equal(Verdict.Valid, Identifiers.Validate("DE89 3704 0044 0532 0130 00", "iban").Verdict);
        }

        // The two examples every IBAN explanation prints; the check digits were recomputed with
        // BigInteger (above) in the corpus, not taken on trust.
        [Theory]
        [InlineData("DE89370400440532013000", "DE89 3704 0044 0532 0130 00")]
        [InlineData("de89 3704 0044 0532 0130 00", "DE89 3704 0044 0532 0130 00")]
        [InlineData("GB82WEST12345698765432", "GB82 WEST 1234 5698 7654 32")]
        public void ValidIbansNormalizeAndPrintInGroupsOfFour(string input, string printed)
        {
            IdentifierResult result = Iban.Validate(input);
            Assert.Equal(Verdict.Valid, result.Verdict);
            Assert.Equal(printed.Replace(" ", string.Empty), result.Normalized);
            Assert.Equal(printed, Iban.Format(input));
        }

        [Theory]
        [InlineData("DE89370400440532013001", Reason.InvalidChecksum)]
        [InlineData("DE8937040044053201300", Reason.InvalidLength)]
        [InlineData("DE893704004405320130000", Reason.InvalidLength)]
        [InlineData("XX89370400440532013000", Reason.InvalidPrefix)]
        [InlineData("8989370400440532013000", Reason.InvalidPrefix)]
        [InlineData("DE89-3704-0044-0532-0130-00", Reason.InvalidCharacters)]   // only the space separates
        [InlineData("DE89370400440532O13000", Reason.InvalidFormat)]            // letter O where a digit belongs
        [InlineData("DEAB370400440532013000", Reason.InvalidFormat)]            // check digits not digits
        [InlineData("DE00370400440532013000", Reason.InvalidChecksum)]          // check digits 00 never occur
        [InlineData("DE", Reason.InvalidPrefix)]
        [InlineData("DE89 3704 0044 0532 0130 00 ", Reason.InvalidCharacters)]
        public void RejectsWithTheReason(string input, Reason reason)
        {
            IdentifierResult result = Iban.Validate(input);
            Assert.Equal(Verdict.Invalid, result.Verdict);
            Assert.Equal(reason, result.Reason);
            Assert.Null(Iban.Format(input));
        }

        [Fact]
        public void NeverThrowsAndNormalizeIsIdempotent()
        {
            string[] inputs = { null, "", " ", "   ", "I", "\0", "DE89\t3704", new string('9', 200), "\uD800", "٣٤", "de89ı", "AD" + new string('0', 22) };
            foreach (string input in inputs)
            {
                IdentifierResult result = Iban.Validate(input);
                Assert.NotEqual(Verdict.NotCovered, result.Verdict);
                string once = Iban.Normalize(input);
                Assert.Equal(once, Iban.Normalize(once));
            }
        }

        [Fact]
        public void EveryCountryOfTheTableHasAnInternallyConsistentStructure()
        {
            foreach (string country in IbanRule.Countries)
            {
                IbanRule.Spec spec = IbanRule.SpecOf(country);
                Assert.Equal(spec.Length - 4, spec.Classes.Length);
                Assert.All(spec.Classes, c => Assert.Contains(c, "nac"));
            }
        }

        // --- differential tests against IbanNet (test-only dependency) ---

        private static readonly IbanValidator Oracle = new IbanValidator();

        /// <summary>Countries IbanNet's registry has that ours does not, and the reverse. Empty means agreement.</summary>
        [Fact]
        public void TheCountryListMatchesIbanNetsRegistry()
        {
            var theirs = new HashSet<string>(IbanRegistry.Default.Select(c => c.TwoLetterISORegionName), StringComparer.Ordinal);
            var ours = new HashSet<string>(IbanRule.Countries, StringComparer.Ordinal);

            var onlyOurs = ours.Except(theirs).OrderBy(c => c, StringComparer.Ordinal).ToList();
            var onlyTheirs = theirs.Except(ours).OrderBy(c => c, StringComparer.Ordinal).ToList();

            // RU: Wikipedia's table lists it (33 characters, 14n 15c) but IbanNet's registry does not yet
            // (KNOWN-ISSUES KI-IBAN-2). Any other difference is a defect in one of the two.
            onlyOurs.Remove("RU");

            Assert.True(
                onlyOurs.Count == 0 && onlyTheirs.Count == 0,
                "Only in Attest: " + string.Join(",", onlyOurs) + ". Only in IbanNet: " + string.Join(",", onlyTheirs) + ".");
        }

        [Fact]
        public void EveryRegistryExampleIsAcceptedWithTheSameLength()
        {
            var problems = new List<string>();
            foreach (IbanCountry country in IbanRegistry.Default)
            {
                string cc = country.TwoLetterISORegionName;
                string example = country.Iban.Example;
                if (IbanRule.SpecOf(cc) == null)
                {
                    continue; // reported by TheCountryListMatchesIbanNetsRegistry
                }

                IdentifierResult result = Iban.Validate(example);
                if (!result.IsValid)
                {
                    problems.Add(cc + " example " + example + " rejected: " + result.Reason + " " + result.Detail);
                }

                if (IbanRule.SpecOf(cc).Length != example.Length)
                {
                    problems.Add(cc + " length " + IbanRule.SpecOf(cc).Length + " but the registry example has " + example.Length);
                }
            }

            Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
        }

        /// <summary>
        /// For every country and every BBAN position, put a letter, a digit and a symbol there,
        /// recompute the check digits so only the structure can reject it, and require both
        /// implementations to give the same verdict. This is what exercises the character classes.
        /// </summary>
        [Fact]
        public void StructureVerdictsAgreeWithIbanNetOnEveryPosition()
        {
            var problems = new List<string>();
            int compared = 0;
            foreach (IbanCountry country in IbanRegistry.Default)
            {
                string cc = country.TwoLetterISORegionName;
                if (IbanRule.SpecOf(cc) == null)
                {
                    continue;
                }

                string bban = country.Iban.Example.Substring(4);
                foreach (char probe in new[] { 'A', 'Z', '0', '9', '*' })
                {
                    for (int i = 0; i < bban.Length; i++)
                    {
                        string candidate = WithCheckDigits(cc, Sub(bban, i, probe));
                        bool ours = Iban.Validate(candidate).IsValid;
                        bool theirs = Oracle.Validate(candidate).IsValid;
                        compared++;
                        if (ours != theirs)
                        {
                            problems.Add(cc + " " + candidate + ": Attest " + ours + ", IbanNet " + theirs);
                        }
                    }
                }

                // wrong lengths, with the check digits recomputed
                foreach (string changed in new[] { bban + "0", bban.Substring(0, bban.Length - 1) })
                {
                    string candidate = WithCheckDigits(cc, changed);
                    compared++;
                    if (Iban.Validate(candidate).IsValid != Oracle.Validate(candidate).IsValid)
                    {
                        problems.Add(cc + " length " + candidate.Length + ": disagreement");
                    }
                }
            }

            Assert.True(compared > 5000, "Only " + compared + " comparisons were made.");
            Assert.True(problems.Count == 0, problems.Count + " disagreements:" + Environment.NewLine + string.Join(Environment.NewLine, problems.Take(40)));
        }
    }
}
