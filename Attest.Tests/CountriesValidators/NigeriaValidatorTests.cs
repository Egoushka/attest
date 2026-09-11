using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class NigeriaValidatorTests
    {
        private readonly NigeriaValidator _nigeriaValidator;

        public NigeriaValidatorTests()
        {
            _nigeriaValidator = new NigeriaValidator();
        }

        // National Identification Number, eleven digits issued by NIMC. No check digit is
        // published, so only the length and the character set are validated.
        // https://nimc.gov.ng/faqs/
        [Theory]
        [InlineData("12345678901", true)]
        [InlineData("70123456789", true)]
        [InlineData("1234567890", false)]      // Ten digits, that is a TIN not a NIN
        [InlineData("123456789012", false)]    // Twelve digits
        [InlineData("1234567890A", false)]     // Letter in the last position
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Joint Tax Board TIN, ten digits, purely numeric and without a published check digit.
        // https://lookuptax.com/docs/tax-identification-number/nigeria-tax-id-guide
        [Theory]
        [InlineData("1234567890", true)]
        [InlineData("0102345678", true)]
        [InlineData("123456789", false)]       // Nine digits
        [InlineData("12345678901", false)]     // Eleven digits
        [InlineData("123456789A", false)]      // Letter in the last position
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("1234567890", true)]
        [InlineData("123456789", false)]       // Nine digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("1234567890", true)]
        [InlineData("123456789", false)]       // Nine digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateVAT(code).IsValid);
        }

        // Six digit NIPOST codes: 100001 is Ikeja HO in Lagos, 900001 is Garki HO in Abuja.
        // https://en.wikipedia.org/wiki/Postal_codes_in_Nigeria
        [Theory]
        [InlineData("100001", true)]
        [InlineData("900001", true)]
        [InlineData("10001", false)]           // Five digits
        [InlineData("1000011", false)]         // Seven digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
