using CountryValidation.Countries;
using Xunit;

namespace CountryValidation.Tests
{
    public class BelgiumValidatorTests
    {
        private readonly BelgiumValidator _belgiumValidator;

        public BelgiumValidatorTests()
        {
            _belgiumValidator = new BelgiumValidator();
        }

        [Theory]
        [InlineData("12060105317", true)]
        [InlineData("36574261890", false)]
        [InlineData("36554266806", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _belgiumValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("12060105317", true)]
        [InlineData("36574261890", false)]
        [InlineData("36554266806", false)]
        [InlineData("93051822361", true)]     // Born 1993-05-18
        [InlineData("85071501709", true)]     // Check number below 10, written as "09"
        [InlineData("85.07.15-017.09", true)] // Same number as printed on the id card
        [InlineData("05030900178", true)]     // Born after 2000, nine digits prefixed with a 2
        [InlineData("05030907009", true)]     // Born after 2000, check number below 10
        [InlineData("90462200196", true)]     // BIS number for a non resident, month 06 + 40
        [InlineData("85071501708", false)]    // Wrong check number
        [InlineData("85071501700", false)]    // Check number 00 never occurs
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belgiumValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("0428759497", true)]
        [InlineData("BE403019261", true)]
        [InlineData("431150351", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belgiumValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("0428759497", true)]
        [InlineData("BE403019261", true)]
        [InlineData("BE 428759497", true)]
        [InlineData("431150351", false)]
        [InlineData("BE1000003682", true)]
        [InlineData("BE1000003681", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belgiumValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("4000 ", true)]
        [InlineData("1000", true)]
        [InlineData("32", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belgiumValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
