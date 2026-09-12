using System;
using System.Collections.Generic;
using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// .NET's regex digit class and char.IsDigit match every Unicode decimal digit while int.Parse
    /// accepts only ASCII, so a validator that guards with one and parses with the other throws on
    /// input that looks numeric. This sweeps every country and every public method to prove that
    /// none of them does, and that null and garbage are answered rather than thrown at.
    /// </summary>
    public class UnicodeDigitSweepTests
    {
        private readonly CountryValidator _validator = new CountryValidator();

        public static IEnumerable<object[]> EveryCountry()
        {
            foreach (Country country in Enum.GetValues(typeof(Country)))
            {
                yield return new object[] { country };
            }
        }

        [Theory]
        [MemberData(nameof(EveryCountry))]
        public void NoCountryThrowsOnDigitsOutsideAscii(Country country)
        {
            var inputs = new[]
            {
                "\u0662\u0660\u0662\u0666\u0667\u0665\u0666\u0665\u0663\u0669\u0663", // Arabic-Indic
                "\u0967\u0968\u0969\u096a\u096b\u096c\u096d\u096e\u096f\u0966",       // Devanagari
                "\uff11\uff12\uff13\uff14\uff15\uff16\uff17\uff18\uff19\uff10",       // fullwidth
                "1234\u0665" + "6789",                                                        // mixed with ASCII
                "\u0be7\u0be8\u0be9\u0bea\u0beb\u0bec\u0bed\u0bee\u0bef",             // Tamil
            };

            foreach (var input in inputs)
            {
                // The point of the assertion is that none of these throws. A false verdict is the
                // right answer: no identifier scheme here is written in non-ASCII digits.
                Assert.False(_validator.ValidateIndividualTaxCode(input, country).IsValid);
                Assert.False(_validator.ValidateEntity(input, country).IsValid);
                Assert.False(_validator.ValidateVAT(input, country).IsValid);
                Assert.False(_validator.ValidateNationalIdentityCode(input, country).IsValid);
                Assert.False(_validator.ValidateZIPCode(input, country).IsValid);
                Assert.False(_validator.Validate(input, country).IsValid);
            }
        }

        [Theory]
        [MemberData(nameof(EveryCountry))]
        public void NoCountryThrowsOnNullOrGarbage(Country country)
        {
            foreach (var input in new[] { null, "", "   ", "---", "abc" })
            {
                Assert.False(_validator.ValidateIndividualTaxCode(input, country).IsValid);
                Assert.False(_validator.ValidateEntity(input, country).IsValid);
                Assert.False(_validator.ValidateVAT(input, country).IsValid);
                Assert.False(_validator.ValidateNationalIdentityCode(input, country).IsValid);
                Assert.False(_validator.ValidateZIPCode(input, country).IsValid);
            }
        }
    }
}
