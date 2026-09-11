using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class NewZealandValidatorTests
    {
        private readonly NewZealandValidator _newZealandValidator;

        public NewZealandValidatorTests()
        {
            _newZealandValidator = new NewZealandValidator();
        }

        [Theory]
        [InlineData("49091850", true)]      // Eight digits, check digit 0
        [InlineData("136410132", true)]     // Nine digits, check digit 2
        [InlineData("136410133", false)]    // Wrong check digit
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _newZealandValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("49091850", true)]
        [InlineData("49098576", true)]
        [InlineData("4909185-0", true)]     // Same number as printed, with the separator
        [InlineData("136410132", true)]     // Nine digits
        [InlineData("100000131", true)]     // Primary weights give 10, so the secondary weights decide
        [InlineData("49091851", false)]     // Wrong check digit
        [InlineData("136410133", false)]    // Wrong check digit
        [InlineData("100000132", false)]    // Wrong check digit on the secondary weights path
        [InlineData("10000000", false)]     // Below the lowest issued number
        [InlineData("150000000", false)]    // Above the highest issued number
        [InlineData("9125568", false)]      // Seven digits
        [InlineData("0123456789", false)]   // Ten digits, used to reach the checksum and throw
        [InlineData("4909185A", false)]     // Not all digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("!!!", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _newZealandValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("49091850", true)]
        [InlineData("136410132", true)]
        [InlineData("49091851", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _newZealandValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("49098576", true)]
        [InlineData("NZ 49-098-576", true)]
        [InlineData("136410133", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _newZealandValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("6011", true)]
        [InlineData("0110", true)]
        [InlineData("601", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _newZealandValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
