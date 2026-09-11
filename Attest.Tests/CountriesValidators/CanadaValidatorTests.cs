using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class CanadaValidatorTests
    {
        private readonly CanadaValidator _canadaValidator;

        public CanadaValidatorTests()
        {
            _canadaValidator = new CanadaValidator();
        }

        // SIN: 9 digits + Luhn check digit, see https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ca/sin.py
        [Theory]
        [InlineData("123456782", true)]
        [InlineData("123-456-782", true)]   // Same number as printed on the card
        [InlineData("193456787", true)]
        [InlineData("123456789", false)]    // Wrong check digit
        [InlineData("999999999", false)]    // Wrong check digit
        [InlineData("12345678", false)]     // Too short
        [InlineData("1234567821", false)]   // Too long
        [InlineData("12345678Z", false)]    // Not all digits
        [InlineData("5", false)]            // Single digit, trivially satisfied the old check digit arithmetic
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _canadaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("812345676", true)]
        [InlineData("823456785", true)]
        [InlineData("812345677", false)]    // Wrong check digit
        [InlineData("123456782", false)]    // Does not start with 8
        [InlineData("81234567", false)]     // Too short
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _canadaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("812345676", true)]
        [InlineData("812345677", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _canadaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("K1A0B1", true)]
        [InlineData("K1A 0B1", true)]
        [InlineData("D1A0B1", false)]       // D is never used in the first position
        [InlineData("12345", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _canadaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
