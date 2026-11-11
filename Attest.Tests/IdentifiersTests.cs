using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>The 2.0 entry points: lookup, verdicts, reasons, bounds, normalization and Match.</summary>
    public class IdentifiersTests
    {
        private static readonly string[] Samples =
        {
            "0428759497", "93051822361", "02618169", "500100732259", "7707083893", "1000",
            "1234AB", "A12345678", "123456789", "12345678901", "BE0428759497", "1234ij", "SW1A 1AA",
        };

        private static readonly IdentifierKind[] LegacyKinds =
        {
            IdentifierKind.PersonalId, IdentifierKind.PersonalTaxCode, IdentifierKind.CompanyNumber,
            IdentifierKind.Vat, IdentifierKind.PostalCode,
        };

        [Fact]
        public void EveryOneDotXPairIsRegistered()
        {
            Assert.Equal(87 * 5, Identifiers.All.Count);
            Assert.Equal(Identifiers.All.Count, Identifiers.All.Select(t => t.Id).Distinct().Count());
            Assert.All(Identifiers.All, t => Assert.Equal(t.Id.ToLowerInvariant(), t.Id));
        }

        [Fact]
        public void EveryIdAndAliasResolvesToItsType()
        {
            foreach (IdentifierType type in Identifiers.All)
            {
                Assert.Same(type, Identifiers.Find(type.Id));
                Assert.Same(type, Identifiers.Find(type.Id.ToUpperInvariant()));
                Assert.NotEmpty(type.Aliases);
                Assert.All(type.Aliases, alias => Assert.Same(type, Identifiers.Find(alias)));
            }
        }

        [Fact]
        public void FindAnswersNullForWhatItDoesNotKnow()
        {
            Assert.Null(Identifiers.Find(null));
            Assert.Null(Identifiers.Find(""));
            Assert.Null(Identifiers.Find("be.nonsense"));
        }

        [Fact]
        public void BelgianTypesCarryTheirShape()
        {
            IdentifierType vat = Identifiers.Find("be.vat");

            Assert.Equal(Country.BE, vat.Country);
            Assert.Equal(IdentifierKind.Vat, vat.Kind);
            Assert.Equal(Coverage.Full, vat.Coverage);
            Assert.Same(vat.Rule, vat.Rule);
            Assert.Same(vat, Identifiers.Find("BE.VATIN"));
            Assert.Equal(new[] { "be.nationalid", "be.taxcode", "be.company", "be.vat", "be.postalcode" },
                Identifiers.For(Country.BE).Select(t => t.Id));
        }

        [Fact]
        public void ForFiltersByKindAndIsEmptyForAnUnknownCountry()
        {
            Assert.Equal(2, Identifiers.For(Country.BE, IdentifierKind.Business).Count);
            Assert.Single(Identifiers.For(Country.BE, IdentifierKind.PostalCode));
            Assert.Empty(Identifiers.For(Country.XX));
        }

        [Fact]
        public void ValidatorForAnUnknownIdIsNotCoveredAndDoesNotThrow()
        {
            IdentifierResult result = Identifiers.Validate("0428759497", "zz.nothing");

            Assert.Equal(Verdict.NotCovered, result.Verdict);
            Assert.Null(result.Type);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void NotCoveredIsNotInvalidAndMatchesSupports()
        {
            var legacy = new CountryValidator();
            int notCovered = 0;

            foreach (IdentifierType type in Identifiers.All)
            {
                bool supported = legacy.Supports(type.Country.Value, type.Kind);
                IdentifierResult result = type.Validate("0428759497");

                if (supported)
                {
                    Assert.True(type.Coverage == Coverage.Full || type.Coverage == Coverage.FormatOnly);
                    Assert.NotEqual(Verdict.NotCovered, result.Verdict);
                }
                else
                {
                    notCovered++;
                    Assert.True(type.Coverage == Coverage.NotCovered || type.Coverage == Coverage.NotApplicable);
                    Assert.Equal(Verdict.NotCovered, result.Verdict);
                    Assert.Equal(Reason.None, result.Reason);
                    Assert.False(result.IsValid);
                }
            }

            // 25 pairs had no rule in 1.x; three now have one (bh.vat, gt.nationalid, mu.postalcode).
            Assert.Equal(22, notCovered);
        }

        [Fact]
        public void VerdictsMatchTheOneDotXValidatorsForEverySample()
        {
            var legacy = new CountryValidator();

            foreach (IdentifierType type in Identifiers.All.Where(t => t.Coverage == Coverage.Full))
            {
                foreach (string sample in Samples)
                {
                    bool expected = legacy.Validate(sample, type.Country.Value, IdentifierKind.Any | IdentifierKind.PostalCode)
                        .Details[type.Kind].IsValid;
                    IdentifierResult result = type.Validate(sample);

                    Assert.True(expected == result.IsValid, type.Id + " " + sample);
                    Assert.Equal(expected, result.Verdict == Verdict.Valid);
                    Assert.Equal(expected, result.Reason == Reason.None);
                }
            }
        }

        [Fact]
        public void ReasonsAreMappedFromTheOneDotXMessages()
        {
            IdentifierType rrn = Identifiers.Find("be.taxcode");

            Assert.Equal(Reason.InvalidFormat, rrn.Validate("1234").Reason);
            // The Belgian register number 93051822361 with its check digits changed.
            Assert.Equal(Reason.InvalidChecksum, rrn.Validate("93051822362").Reason);
            Assert.Equal(Verdict.Invalid, rrn.Validate("93051822362").Verdict);
            Assert.Equal(Verdict.Valid, rrn.Validate("93051822361").Verdict);
            Assert.NotNull(rrn.Validate("93051822362").Detail);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t　")]
        public void EmptyInputIsEmpty(string value)
        {
            foreach (IdentifierType type in Identifiers.All.Where(t => t.Coverage == Coverage.Full))
            {
                IdentifierResult result = type.Validate(value);

                Assert.Equal(Verdict.Invalid, result.Verdict);
                Assert.Equal(Reason.Empty, result.Reason);
            }
        }

        [Fact]
        public void InputOverTheBoundIsTooLongBeforeAnyRuleRuns()
        {
            string atBound = new string('1', 128);
            string over = new string('1', 129);

            foreach (IdentifierType type in Identifiers.All.Where(t => t.Coverage == Coverage.Full))
            {
                Assert.Equal(Reason.TooLong, type.Validate(over).Reason);
                Assert.NotEqual(Reason.TooLong, type.Validate(atBound).Reason);
                Assert.Null(type.Normalize(over));
                Assert.Null(type.Format(over));
            }

            Assert.Equal(Reason.TooLong, Identifiers.ValidateVat("BE" + over).Reason);
        }

        [Fact]
        public void NormalizeStripsOnlyTheDocumentedSeparatorsAndUpperCases()
        {
            IdentifierType type = Identifiers.Find("gb.postalcode");

            Assert.Equal("SW1A1AA", type.Normalize("sw1a 1aa"));
            Assert.Equal("ABC123", type.Normalize("a.b-c/1 2 3"));
            // A tab is not a documented separator, so it is not repaired away.
            Assert.Equal("AB\tC", type.Normalize("ab\tc"));
            Assert.Null(type.Normalize("- . /"));
            Assert.Null(type.Normalize(null));
            Assert.Null(type.Normalize("  "));
        }

        [Fact]
        public void NormalizeIsIdempotentAndFormatIsCompactOrNull()
        {
            string[] inputs = { "be 0428.759-497", "1234ij", "sw1a 1aa", "x.y-z/1", "١٢", "ß-ß" };

            foreach (IdentifierType type in Identifiers.All)
            {
                foreach (string input in inputs)
                {
                    string once = type.Normalize(input);
                    Assert.Equal(once, type.Normalize(once));
                }
            }

            IdentifierType vat = Identifiers.Find("be.vat");
            Assert.Equal("BE0428759497", vat.Format("be 0428.759.497"));
            Assert.Equal(vat.Format("BE0428759497"), vat.Format(vat.Normalize("be 0428.759.497")));
            Assert.Null(vat.Format("BE0428759498"));
            Assert.Equal("BE0428759497", vat.Validate("be 0428.759.497").Normalized);
        }

        [Fact]
        public void ParseReturnsTheVerdictAndNoPartsForNow()
        {
            IdentifierType vat = Identifiers.Find("be.vat");

            ParseResult good = vat.Parse("BE0428759497");
            ParseResult bad = vat.Parse("BE0428759498");

            Assert.True(good.Result.IsValid);
            Assert.False(bad.Result.IsValid);
            Assert.Empty(bad.Parts);
            Assert.Null(bad.BirthDate);
            Assert.Null(bad.Sex);
            Assert.Null(bad.IssuingCountry);
            Assert.Empty(default(ParseResult).Parts);
        }

        [Theory]
        [InlineData("BE0428759497", true, "be.vat")]
        [InlineData("be 0428.759.497", true, "be.vat")]
        [InlineData("BE0428759498", false, "be.vat")]
        public void ValidateVatTakesTheCountryFromThePrefix(string value, bool valid, string typeId)
        {
            IdentifierResult result = Identifiers.ValidateVat(value);

            Assert.Equal(valid, result.IsValid);
            Assert.Equal(typeId, result.Type.Id);
        }

        [Fact]
        public void ValidateVatRoutesGreeceFromEL()
        {
            IdentifierResult result = Identifiers.ValidateVat("EL094259216");

            Assert.Equal("gr.vat", result.Type.Id);
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("0428759497")]
        [InlineData("QQ0428759497")]
        [InlineData("1")]
        public void ValidateVatWithoutAKnownPrefixIsInvalidPrefix(string value)
        {
            IdentifierResult result = Identifiers.ValidateVat(value);

            Assert.Equal(Verdict.Invalid, result.Verdict);
            Assert.Equal(Reason.InvalidPrefix, result.Reason);
            Assert.Null(result.Type);
        }

        [Fact]
        public void ValidateVatOnNullIsEmpty()
        {
            Assert.Equal(Reason.Empty, Identifiers.ValidateVat(null).Reason);
            Assert.Equal(Reason.Empty, Identifiers.ValidateVat("  ").Reason);
        }

        [Fact]
        public void MatchSeparatesPersonFromBusiness()
        {
            MatchResult asPerson = Identifiers.Match("93051822361", Country.BE, IdentifierKind.Person);
            MatchResult asBusiness = Identifiers.Match("93051822361", Country.BE, IdentifierKind.Business);

            Assert.True(asPerson.IsValid);
            Assert.False(asBusiness.IsValid);
            Assert.False(asPerson.IsAmbiguous);
            Assert.Equal(4, asPerson.Details.Count);
            Assert.Equal(IdentifierKind.Person, asPerson.Requested);
            Assert.False(Identifiers.Match("1000", Country.BE).IsValid);
            Assert.True(Identifiers.Match("1000", Country.BE, IdentifierKind.PostalCode).IsValid);
            Assert.False(Identifiers.Match("1", Country.XX).IsValid);
        }

        [Theory]
        // Armenia and Nigeria issue one number to people and companies; Russia's twelve digit INN is
        // a sole trader's. Values taken from the 1.x kinds tests, which cite their sources.
        [InlineData("02618169", Country.AM)]
        [InlineData("500100732259", Country.RU)]
        [InlineData("7707083893", Country.RU)]
        [InlineData("12345678901", Country.NG)]
        [InlineData("1234567", Country.AM)]
        [InlineData("garbage", Country.NG)]
        public void MatchAgreesWithTheOneDotXValidateIncludingAmbiguity(string value, Country country)
        {
            var legacy = new CountryValidator();

            foreach (IdentifierKind kinds in new[] { IdentifierKind.Any, IdentifierKind.Business, IdentifierKind.Person, IdentifierKind.PostalCode })
            {
                var expected = legacy.Validate(value, country, kinds);
                MatchResult actual = Identifiers.Match(value, country, kinds);

                Assert.Equal(expected.Matched, actual.Matched);
                Assert.Equal(expected.IsValid, actual.IsValid);
                Assert.Equal(expected.IsAmbiguous, actual.IsAmbiguous);
                Assert.Equal(expected.Details.Count, actual.Details.Count);
            }

            Assert.True(Identifiers.Match("02618169", Country.AM, IdentifierKind.Business).IsAmbiguous);
            Assert.True(Identifiers.Match("500100732259", Country.RU, IdentifierKind.Business).IsAmbiguous);
            Assert.False(Identifiers.Match("7707083893", Country.RU, IdentifierKind.Business).IsAmbiguous);
        }

        [Fact]
        public void TheDefaultStructHasNoTypeAndIsNotValid()
        {
            IdentifierResult none = default;

            Assert.False(none.IsValid);
            Assert.Null(none.Type);
        }
    }

    /// <summary>
    /// Nothing in the new surface throws, for any input, in any culture. The existing sweeps keep
    /// covering the 1.x classes; this one calls every registered type and every static entry point.
    /// </summary>
    [Collection(CultureCollection.Name)]
    public class IdentifierTypeSweepTests
    {
        private static readonly CultureInfo[] Cultures =
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("tr-TR"),
            new CultureInfo("az-AZ"),
        };

        public static IEnumerable<object[]> HostileInputs()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { "\t\r\n" };
            yield return new object[] { "　" };
            // Arabic-Indic, Devanagari and fullwidth digits.
            yield return new object[] { "١٢٣٤٥٦٧٨٩٠" };
            yield return new object[] { "१२३४५६७८९०" };
            yield return new object[] { "１２３４５６７８９０" };
            yield return new object[] { "abİıij-.//  " };
            yield return new object[] { "😀\uD83D" };
            yield return new object[] { new string('1', 200) };
            yield return new object[] { new string('A', 200) };
            yield return new object[] { "BE" + new string('0', 198) };
            yield return new object[] { "0428759497" };
            yield return new object[] { "BE0428759497" };
            yield return new object[] { "1234ij" };
        }

        [Theory]
        [MemberData(nameof(HostileInputs))]
        public void NoRegisteredTypeThrowsInAnyCulture(string value)
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            CultureInfo originalUi = CultureInfo.CurrentUICulture;
            try
            {
                foreach (CultureInfo culture in Cultures)
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;

                    foreach (IdentifierType type in Identifiers.All)
                    {
                        IdentifierResult result = type.Validate(value);
                        string normalized = type.Normalize(value);
                        string formatted = type.Format(value);
                        ParseResult parsed = type.Parse(value);

                        Assert.Equal(result.Verdict, parsed.Result.Verdict);
                        Assert.Equal(result.Reason, parsed.Result.Reason);
                        Assert.True(formatted == null || result.IsValid);
                        Assert.Equal(result.Verdict == Verdict.Valid, result.Reason == Reason.None && result.Verdict != Verdict.NotCovered);
                        Assert.Equal(Identifiers.Validate(value, type.Id).Verdict, result.Verdict);
                    }

                    foreach (Country country in new[] { Country.BE, Country.AM, Country.RU, Country.NG, Country.XX })
                    {
                        Identifiers.Match(value, country);
                        Identifiers.Match(value, country, IdentifierKind.PostalCode);
                    }

                    Identifiers.ValidateVat(value);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
                Thread.CurrentThread.CurrentUICulture = originalUi;
            }
        }

        [Fact]
        public void NormalizeIsTheSameInEveryCulture()
        {
            string[] inputs = { "1234ij", "iv1 1aa", "ijsselstein", "i-i.i/i" };
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                foreach (string input in inputs)
                {
                    var seen = new HashSet<string>();
                    foreach (CultureInfo culture in Cultures)
                    {
                        Thread.CurrentThread.CurrentCulture = culture;
                        foreach (IdentifierType type in Identifiers.All.Take(40))
                        {
                            seen.Add(type.Normalize(input));
                        }
                    }

                    Assert.Single(seen);
                    Assert.DoesNotContain('İ', seen.Single());
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }
    }
}
