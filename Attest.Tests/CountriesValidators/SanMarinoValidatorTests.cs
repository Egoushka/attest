using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class SanMarinoValidatorTests
    {
        private readonly SanMarinoValidator _sanMarinoValidator;

        public SanMarinoValidatorTests()
        {
            _sanMarinoValidator = new SanMarinoValidator();
        }

        [Theory]
        [InlineData("999999999", true)]   // SSI number, 9 digits (OECD TIN sheet example)
        [InlineData("123456789", true)]
        [InlineData("12345678", true)]    // Leading zero omitted
        [InlineData("SM99999", true)]     // A COE is accepted as a tax code as well
        [InlineData("1234567890", false)] // Too long for an SSI number
        [InlineData("12345678A", false)]
        [InlineData("hello world", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sanMarinoValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("999999999", true)]
        [InlineData("12345678A", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _sanMarinoValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("SM24165", true)]
        [InlineData("24165", true)]
        [InlineData("99", true)]      // Registered number with less than 3 digits
        [InlineData("1", false)]      // Not a registered number with less than 3 digits
        [InlineData("123456", false)] // More than 5 digits
        [InlineData("12A", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sanMarinoValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("SM24165", true)]
        [InlineData("123456", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sanMarinoValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("47890", true)]
        [InlineData("47899", true)]
        [InlineData("12345", false)]
        [InlineData("4789", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sanMarinoValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
