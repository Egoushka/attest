using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// A verdict must not depend on the culture of the thread that asked for it.
    ///
    /// String.ToUpper() folds case with the current culture, and in Turkish and Azeri a lowercase
    /// 'i' uppercases to the dotted 'İ' rather than 'I'. Every format check in this library is
    /// written with [A-Z], so a validator that normalised with ToUpper() rejected identifiers on a
    /// tr-TR thread that it accepted everywhere else. The Dutch postcode 1234ij -- an ordinary one,
    /// IJ being a Dutch digraph -- and the Inverness postcode IV1 1AA were both real casualties.
    ///
    /// 38 call sites across 24 validators used ToUpper(); they use ToUpperInvariant() now. This
    /// sweep is what stops the next one coming back, because the defect is invisible on a machine
    /// whose culture is anything else.
    ///
    /// The tests set the thread culture, so they run in their own non-parallel collection.
    /// </summary>
    [Collection(CultureCollection.Name)]
    public class CultureSweepTests
    {
        private readonly CountryValidator _validator = new CountryValidator();

        /// <summary>
        /// The cultures that fold 'i' to something other than 'I'. Invariant is the baseline every
        /// other verdict is compared against.
        /// </summary>
        private static readonly CultureInfo[] Cultures =
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("tr-TR"),
            new CultureInfo("az-AZ"),
        };

        public enum Kind
        {
            NationalIdentity,
            IndividualTaxCode,
            Entity,
            Vat,
            PostalCode,
        }

        /// <summary>
        /// Values written in lower case that are valid, taken from the per-country test files and
        /// re-checked here. A value only exercises the casing path if it has a lower-case letter in
        /// it, so an all-digit identifier proves nothing and none is listed.
        /// </summary>
        public static IEnumerable<object[]> ValidLowerCaseValues()
        {
            // The four that were actually wrong before ToUpperInvariant: each carries a lower-case
            // 'i' in the part the format check reads.
            yield return new object[] { Country.NL, Kind.PostalCode, "1234ij" };
            yield return new object[] { Country.GB, Kind.PostalCode, "iv11aa" };
            yield return new object[] { Country.MT, Kind.PostalCode, "imd1234" };
            yield return new object[] { Country.SI, Kind.PostalCode, "si-1000" };

            // The rest of the validators that fold case, so the sweep covers the pattern and not
            // only the four inputs that happened to expose it.
            yield return new object[] { Country.LT, Kind.PostalCode, "lt-01100" };
            yield return new object[] { Country.LV, Kind.PostalCode, "lv-1000" };
            yield return new object[] { Country.HK, Kind.NationalIdentity, "a1234563" };
            yield return new object[] { Country.MT, Kind.NationalIdentity, "1234567m" };
            yield return new object[] { Country.IT, Kind.IndividualTaxCode, "rccmnl83s18d969h" };
            yield return new object[] { Country.PH, Kind.IndividualTaxCode, "123456789000v" };
            yield return new object[] { Country.TR, Kind.Vat, "tr4540536920" };
            yield return new object[] { Country.NO, Kind.Vat, "no988077917" };
            yield return new object[] { Country.MD, Kind.Vat, "md9234564" };
            yield return new object[] { Country.RS, Kind.Vat, "rs101134702" };
        }

        [Theory]
        [MemberData(nameof(ValidLowerCaseValues))]
        public void AValidLowerCaseValueStaysValidInEveryCulture(Country country, Kind kind, string value)
        {
            foreach (var culture in Cultures)
            {
                Assert.True(
                    WithCulture(culture, () => Validate(value, country, kind)),
                    $"{country} rejected \"{value}\" under {Describe(culture)}.");
            }
        }

        public static IEnumerable<object[]> EveryCountry()
        {
            foreach (Country country in Enum.GetValues(typeof(Country)))
            {
                yield return new object[] { country };
            }
        }

        [Theory]
        [MemberData(nameof(EveryCountry))]
        public void NoCountryAnswersDifferentlyUnderATurkicCulture(Country country)
        {
            // Mixed-case probes rather than realistic identifiers: the assertion is that the answer
            // does not move, whatever it is, so an invalid value is as useful here as a valid one.
            var inputs = new[]
            {
                "i", "I", "ii", "1234ij", "iv11aa", "si-1000", "iiii1111", "ISTANBUL", "istanbul",
                "che-107.787.577 iva", "mi123456i", "1i2i3i4i5i",
            };

            foreach (var input in inputs)
            {
                foreach (Kind kind in Enum.GetValues(typeof(Kind)))
                {
                    bool baseline = WithCulture(CultureInfo.InvariantCulture, () => Validate(input, country, kind));

                    for (int i = 1; i < Cultures.Length; i++)
                    {
                        CultureInfo culture = Cultures[i];
                        Assert.True(
                            baseline == WithCulture(culture, () => Validate(input, country, kind)),
                            $"{country}.{kind} answered differently for \"{input}\" under {Describe(culture)}.");
                    }
                }
            }
        }

        private bool Validate(string value, Country country, Kind kind)
        {
            switch (kind)
            {
                case Kind.NationalIdentity:
                    return _validator.ValidateNationalIdentityCode(value, country).IsValid;
                case Kind.IndividualTaxCode:
                    return _validator.ValidateIndividualTaxCode(value, country).IsValid;
                case Kind.Entity:
                    return _validator.ValidateEntity(value, country).IsValid;
                case Kind.Vat:
                    return _validator.ValidateVAT(value, country).IsValid;
                default:
                    return _validator.ValidateZIPCode(value, country).IsValid;
            }
        }

        private static bool WithCulture(CultureInfo culture, Func<bool> validate)
        {
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = culture;
                return validate();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        private static string Describe(CultureInfo culture)
        {
            return culture.Equals(CultureInfo.InvariantCulture) ? "the invariant culture" : culture.Name;
        }
    }

    /// <summary>
    /// These tests write to Thread.CurrentThread.CurrentCulture, so they do not share a thread with
    /// anything else while they run.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class CultureCollection
    {
        public const string Name = "culture";
    }
}
