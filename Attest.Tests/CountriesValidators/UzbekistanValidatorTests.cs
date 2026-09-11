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
        // digit for century and gender, six for the date of birth, then the serial number and a
        // check digit whose algorithm is not published, so only the format is validated.
        // https://taxid.pro/docs/countries/uzbekistan
        [Theory]
        [InlineData("31120798702393", true)]    // Example published by taxid.pro
        [InlineData("41504658900162", true)]    // Female born 1965-04-15
        [InlineData("311 207 987 023 93", true)]// Same number with separators
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
        [InlineData("31120798702393", true)]
        [InlineData("41504658900162", true)]
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
