using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class SouthAfricaValidatorTests
    {
        private readonly SouthAfricaValidator _southAfricaValidator;

        public SouthAfricaValidatorTests()
        {
            _southAfricaValidator = new SouthAfricaValidator();
        }

        [Theory]
        [InlineData("7503305044089", true)]      // Born 1975-03-30, citizen
        [InlineData("750330 5044 08 9", true)]   // Same number as printed on the id book
        [InlineData("8001015009087", true)]      // Born 1980-01-01, citizen
        [InlineData("9001155019184", true)]      // Born 1990-01-15, permanent resident
        [InlineData("0002295000083", true)]      // Born 2000-02-29, the century must be mapped to 2000
        [InlineData("8001015009088", false)]     // Wrong check digit
        [InlineData("9902295000085", false)]     // 1999 is not a leap year
        [InlineData("8013015009082", false)]     // Month 13
        [InlineData("8001015009285", false)]     // The eleven digit is neither 1 nor 0
        [InlineData("800101500908", false)]      // Only 12 digits
        [InlineData("80010150090AB", false)]     // Not all digits
        [InlineData("garbage", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _southAfricaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("0001339050", true)]
        [InlineData("0001339051", false)]
        [InlineData("4001339054", false)]  // Must start with 0, 1, 2, 3 or 9
        [InlineData("garbage", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _southAfricaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("0001339050", true)]
        [InlineData("ZA0001339050", true)]
        [InlineData("0001339051", false)]
        [InlineData("garbage", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _southAfricaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("0001", true)]
        [InlineData("8000 ", true)]
        [InlineData("800", false)]
        [InlineData("garbage", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _southAfricaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
