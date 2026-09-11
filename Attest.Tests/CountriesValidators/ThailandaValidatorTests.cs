using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ThailandaValidatorTests
    {
        private readonly ThailandValidator _thailandaValidator;

        public ThailandaValidatorTests()
        {
            _thailandaValidator = new ThailandValidator();
        }

        [Theory]
        [InlineData("0105-515-004-336", true)]
        [InlineData("0107537001510", true)]
        [InlineData("0107537001706", true)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("0107537001510", true)]
        [InlineData("0105-515-004-336", true)]
        [InlineData("0107537001511", false)] // Wrong check digit
        [InlineData("8112289874", false)]    // Ten digit format, withdrawn in 2012
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("0107537001706", true)]
        [InlineData("0107-537-001-706", true)]
        [InlineData("0107537001707", false)] // Wrong check digit
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("0105515004336", true)]
        [InlineData("123456789101", false)] // Twelve digits
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("11455", true)]
        [InlineData("21321", true)]
        [InlineData("321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandaValidator.ValidatePostalCode(code).IsValid);
        }

    }
}
