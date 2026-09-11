using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class AzerbaijanValidatorTests
    {
        private readonly AzerbaijanValidator _azerbaijanValidator;

        public AzerbaijanValidatorTests()
        {
            _azerbaijanValidator = new AzerbaijanValidator();
        }

        // The PIN (FIN) is the seven character code printed on the identity card, made of Latin
        // letters and digits.
        // https://www.e-gov.az/en/services/read/3243/1
        [Theory]
        [InlineData("5VBK5VR", true)]
        [InlineData("6ZZ5N6T", true)]
        [InlineData("1234567", true)]
        [InlineData("5VBK 5VR", true)]          // Same code with a separator
        [InlineData("5VBK5V", false)]           // Six characters
        [InlineData("5VBK5VRA", false)]         // Eight characters
        [InlineData("ЖЖЖЖЖЖЖ", false)]          // Not Latin letters
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _azerbaijanValidator.ValidateNationalIdentity(code).IsValid);
        }

        // The VÖEN is ten digits: two for the territorial unit, six for the serial number, a check
        // digit whose algorithm is not published and a taxpayer type digit.
        // https://lookuptax.com/docs/tax-identification-number/azerbaijan-tax-id-guide
        [Theory]
        [InlineData("1300380271", true)]
        [InlineData("9900003871", true)]
        [InlineData("1300 380 271", true)]      // Same number with separators
        [InlineData("130038027", false)]        // Nine digits
        [InlineData("13003802712", false)]      // Eleven digits
        [InlineData("AZ1300380271", false)]     // Letters are not a valid prefix
        [InlineData("130038027x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _azerbaijanValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("1300380271", true)]
        [InlineData("9900003871", true)]
        [InlineData("130038027", false)]        // Nine digits
        [InlineData("13003802712", false)]      // Eleven digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _azerbaijanValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("1300380271", true)]
        [InlineData("9900003871", true)]
        [InlineData("130038027", false)]        // Nine digits
        [InlineData("AZ1300380271", false)]     // Letters are not a valid prefix
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _azerbaijanValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("AZ1000", true)]            // Baku
        [InlineData("AZ 5000", true)]           // Ganja, with a separator
        [InlineData("az1000", true)]
        [InlineData("1000", false)]             // The AZ prefix is mandatory
        [InlineData("AZ100", false)]            // Three digits
        [InlineData("AZ10000", false)]          // Five digits
        [InlineData("BY1000", false)]           // Wrong prefix
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _azerbaijanValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
