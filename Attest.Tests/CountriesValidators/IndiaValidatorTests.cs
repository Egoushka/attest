using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class IndiaValidatorTests
    {
        private readonly IndiaValidator _indiaValidator;

        public IndiaValidatorTests()
        {
            _indiaValidator = new IndiaValidator();
        }

        [Theory]
        [InlineData("234123412346", true)]      // stdnum reference Aadhaar
        [InlineData("234567890124", true)]      // Verhoeff check digit 4
        [InlineData("987654321096", true)]      // Verhoeff check digit 6
        [InlineData("2345 6789 0124", true)]    // Same number as printed on the card
        [InlineData("234567890123", false)]     // Wrong check digit
        [InlineData("123412341234", false)]     // Correct Verhoeff, but Aadhaar never starts with 1
        [InlineData("012345678901", false)]     // Aadhaar never starts with 0
        [InlineData("23456789012", false)]      // Too short
        [InlineData("0", false)]
        [InlineData("ABCDE1234F", false)]       // A PAN pasted into the Aadhaar field
        [InlineData("22AAAAA0000A1Z5", false)]  // A GSTIN pasted into the Aadhaar field
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("ABCPZ1234E", true)]
        [InlineData("ABCDE1234F", false)]   // D is not a card holder type
        [InlineData("ABCPZ1234", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("27123456789V", true)]
        [InlineData("27123456789C", true)]
        [InlineData("27123456789", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("110001", true)]
        [InlineData("560 001", true)]
        [InlineData("11000", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
