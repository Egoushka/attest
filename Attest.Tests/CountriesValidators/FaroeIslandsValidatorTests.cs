using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class FaroeIslandsValidatorTests
    {
        private readonly FaroeIslandsValidator _faroeIslandsValidator;

        public FaroeIslandsValidatorTests()
        {
            _faroeIslandsValidator = new FaroeIslandsValidator();
        }

        // V-tal (vinnutal): six digits assigned by TAKS, no published check digit.
        // https://lookuptax.com/docs/tax-identification-number/faroe-islands-tax-id-guide
        [Theory]
        [InlineData("530007", true)]
        [InlineData("123 456", true)]
        [InlineData("12345", false)]     // 5 digits
        [InlineData("1234567", false)]   // 7 digits
        [InlineData("abcdef", false)]    // not numeric
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("530007", true)]
        [InlineData("123 456", true)]
        [InlineData("12345", false)]
        [InlineData("1234567", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidateVAT(code).IsValid);
        }

        // P-tal: nine digits, DDMMYY followed by a three digit serial, no published check digit.
        // https://www.norden.org/en/info-norden/civil-registration-number-faroe-islands-p-number
        [Theory]
        [InlineData("150785123", true)]
        [InlineData("010190-456", true)]
        [InlineData("15078512", false)]    // 8 digits
        [InlineData("1507851234", false)]  // 10 digits
        [InlineData("abcdefghi", false)]   // not numeric
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // Postal codes are three digits, FO-100 Torshavn to FO-970 Nordadalur.
        // https://en.wikipedia.org/wiki/Postal_codes_in_the_Faroe_Islands
        [Theory]
        [InlineData("100", true)]
        [InlineData("180", true)]
        [InlineData("12", false)]
        [InlineData("1000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
