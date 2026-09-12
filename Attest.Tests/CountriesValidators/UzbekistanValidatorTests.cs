using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class UzbekistanValidatorTests
    {
        private readonly UzbekistanValidator _uzbekistanValidator;

        public UzbekistanValidatorTests()
        {
            _uzbekistanValidator = new UzbekistanValidator();
        }

        // Since 2021 an individual is identified by the fourteen digit PINFL instead of a TIN: one
        // digit for century and gender, six for the date of birth, three for the district, three
        // for the serial number and a check digit over the first thirteen, weights 7,3,1 repeated,
        // modulus 10. 31210932040247 and 40201902050010 are the two worked examples of chapter 2,
        // group 5 of Cabinet of Ministers regulation no. 177 of 12.04.2022.
        // https://lex.uz/ru/docs/5955669
        [Theory]
        [InlineData("31210932040247", true)]    // Male born 1993-10-12, official example 1
        [InlineData("40201902050010", true)]    // Female born 1990-01-02, official example 2
        [InlineData("41504658900165", true)]    // Female born 1965-04-15, check digit computed
        [InlineData("312 109 320 402 47", true)]// Same number with separators
        [InlineData("31210932040240", false)]   // Wrong check digit
        [InlineData("40201902050011", false)]   // Wrong check digit
        [InlineData("31120798702393", false)]   // Fourteen digits, but the check digit has to be 2
        [InlineData("3112079870239", false)]    // Thirteen digits
        [InlineData("311207987023931", false)]  // Fifteen digits
        [InlineData("311207987", false)]        // The nine digit TIN of a legal entity is not a PINFL
        [InlineData("3112079870239x", false)]
        [InlineData("UZ31120798702393", false)] // Letters are not a valid prefix
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _uzbekistanValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("31210932040247", true)]
        [InlineData("40201902050010", true)]
        [InlineData("31210932040240", false)]   // Wrong check digit
        [InlineData("3112079870239", false)]    // Thirteen digits
        [InlineData("311207987023931", false)]  // Fifteen digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _uzbekistanValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("100000", true)]            // Tashkent
        [InlineData("140100", true)]            // Samarkand
        [InlineData("100 000", true)]
        [InlineData("10000", false)]            // Five digits
        [InlineData("1000000", false)]          // Seven digits
        [InlineData("A00000", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _uzbekistanValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
