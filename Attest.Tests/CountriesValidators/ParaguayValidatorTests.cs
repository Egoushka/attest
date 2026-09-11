using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ParaguayValidatorTests
    {
        private readonly ParaguayValidator _paraguayValidator;

        public ParaguayValidatorTests()
        {
            _paraguayValidator = new ParaguayValidator();
        }

        [Theory]
        [InlineData("80028061-0", true)]  // Legal entity, check digit 0
        [InlineData("800000358", true)]   // Same number as printed, without separator
        [InlineData("9991603", true)]     // Resident, 7 digits
        [InlineData("2660-3", true)]      // Short RUC, 5 digits
        [InlineData("800532492", false)]  // Wrong check digit, correct one is 0
        [InlineData("800280611", false)]  // Wrong check digit
        [InlineData("80123456789", false)]// Too long
        [InlineData("ABC123456", false)]  // Not numeric
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _paraguayValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("80028061-0", true)]
        [InlineData("800532492", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _paraguayValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("9991603", true)]
        [InlineData("2660-3", true)]
        [InlineData("800532492", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _paraguayValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("1209", true)]
        [InlineData("110", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _paraguayValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
