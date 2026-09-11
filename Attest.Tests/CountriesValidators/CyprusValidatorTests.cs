using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class CyprusValidatorTests
    {
        private readonly CyprusValidator _cyprusValidator;

        public CyprusValidatorTests()
        {
            _cyprusValidator = new CyprusValidator();
        }

        // The Cypriot identity card number is 10 digits with no published check digit,
        // so only the format is asserted here.
        // https://learn.microsoft.com/en-us/purview/sit-defn-cyprus-identity-card
        [Theory]
        [InlineData("1234567890", true)]
        [InlineData("0000000001", true)]
        [InlineData("123456789", false)]
        [InlineData("12345678901", false)]
        [InlineData("123456789A", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _cyprusValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Check letters computed with the published mod 26 algorithm.
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.cy.vat.html
        [Theory]
        [InlineData("00123123T", true)]
        [InlineData("00123123A", false)]
        [InlineData("90000000Y", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _cyprusValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("10259033P", true)]
        [InlineData("10259033Z", false)]
        // Issued from 60000000 onwards since the March 2023 move to the Tax For All platform.
        [InlineData("60000000S", true)]
        [InlineData("60000000A", false)]
        // Numbers starting with 12 are reserved, even when the check letter is correct.
        [InlineData("12345678F", false)]
        [InlineData("1025903P", false)]
        [InlineData("102590331P", false)]
        [InlineData("10259033", false)]
        [InlineData("ABCDEFGHI", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _cyprusValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("10259033P", true)]
        [InlineData("CY-10259033P", true)]
        [InlineData("CY-10259033P ", true)]
        [InlineData("cy10259033p", true)]
        [InlineData("10259033Z", false)]
        [InlineData("CY-10259033Z", false)]
        [InlineData("CY12345678F", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _cyprusValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("2008", true)]
        [InlineData("3004 ", true)]
        [InlineData("1", false)]
        [InlineData("12345", false)]
        [InlineData("ABCD", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _cyprusValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
