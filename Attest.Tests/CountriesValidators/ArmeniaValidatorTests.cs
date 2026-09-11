using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ArmeniaValidatorTests
    {
        private readonly ArmeniaValidator _armeniaValidator;

        public ArmeniaValidatorTests()
        {
            _armeniaValidator = new ArmeniaValidator();
        }

        // The ՀՎՀՀ (TIN) is issued to both individuals and entities and is exactly eight digits;
        // the eighth is a check digit whose algorithm is not published, so only the format is
        // validated.
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/armenia-tin.pdf
        [Theory]
        [InlineData("02618169", true)]
        [InlineData("00021341", true)]
        [InlineData("026 181 69", true)]        // Same number with separators
        [InlineData("0261816", false)]          // Seven digits
        [InlineData("026181690", false)]        // Nine digits
        [InlineData("0261816x", false)]
        [InlineData("AM02618169", false)]       // Letters are not a valid prefix
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("02618169", true)]
        [InlineData("00021341", true)]
        [InlineData("0261816", false)]          // Seven digits
        [InlineData("026181690", false)]        // Nine digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("02618169", true)]
        [InlineData("00021341", true)]
        [InlineData("0261816", false)]          // Seven digits
        [InlineData("026181690", false)]        // Nine digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("0010", true)]              // Yerevan
        [InlineData("3101", true)]              // Gyumri
        [InlineData("0010 ", true)]
        [InlineData("001", false)]              // Three digits
        [InlineData("00100", false)]            // Five digits
        [InlineData("A010", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
