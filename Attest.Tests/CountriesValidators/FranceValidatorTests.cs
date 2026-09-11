using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class FranceValidatorTests
    {
        private readonly FranceValidator _franceValidator;

        public FranceValidatorTests()
        {
            _franceValidator = new FranceValidator();
        }

        [Theory]
        [InlineData("295109912611193", true)]
        [InlineData("253072B07300470", true)]
        [InlineData("253072A07300443", true)]
        [InlineData("295109912611199", false)]
        [InlineData("253072C07300443", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _franceValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("0701987765432", true)]
        [InlineData("07 01 987 765 432", true)]
        [InlineData("070198776543", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _franceValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("552 008 443", true)]
        [InlineData("404833048", true)]
        [InlineData("404833047", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _franceValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("Fr 40 303 265 045", true)]
        [InlineData("23334175221", true)]
        [InlineData("K7399859412", true)]
        [InlineData("84 323 140 391", false)]
        [InlineData("4Z123456782", true)]      // New style number starting with a digit
        [InlineData("K6303265045", true)]      // New style key for SIREN 303265045
        [InlineData("FR68900296724", true)]    // Key 68 = (12 + 3 * (900296724 % 97)) % 97
        [InlineData("42000191100", true)]      // Monaco, valid TVA but not a valid SIREN
        [InlineData("FR41303265045", false)]   // Valid SIREN, wrong key (correct key is 40)
        [InlineData("FR11900296724", false)]   // Valid SIREN, wrong key (correct key is 68)
        [InlineData("43000191100", false)]     // Monaco, wrong key
        [InlineData("KA303265045", false)]     // Valid SIREN, wrong new style key
        [InlineData("ZI334175221", false)]     // The letters of the key cannot be I or O
        [InlineData("IO334175221", false)]
        [InlineData("garbage", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _franceValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("33380", true)]
        [InlineData("34092", true)]
        [InlineData("3321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _franceValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
