using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class PolandValidatorTests
    {
        private readonly PolandValidator _polandValidator;

        public PolandValidatorTests()
        {
            _polandValidator = new PolandValidator();
        }

        [Theory]
        [InlineData("83010411457", true)]
        [InlineData("87123116221", true)]
        [InlineData("39100413824", false)]
        [InlineData("36032806768", false)]
        [InlineData("04271113861", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghijk", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _polandValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("83010411457", true)]
        [InlineData("87123116221", true)]
        [InlineData("39100413824", false)]
        [InlineData("36032806768", false)]
        [InlineData("04271113861", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghijk", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _polandValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("192598184", true)]
        [InlineData("123456785", true)]
        [InlineData("12345678512347", true)]
        [InlineData("12345678512348", false)]
        [InlineData("192598183", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghi", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _polandValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("8567346215", true)]
        [InlineData("8567346216", false)]
        [InlineData("PL8567346215", true)]
        // Check digits computed from the NIP rule: weights 6,5,7,2,3,4,5,6,7 over the first
        // nine digits, remainder mod 11 must equal the tenth digit.
        [InlineData("7877893282", true)]
        [InlineData("7921742183", true)]
        // Weighted sum mod 11 == 10, which no single digit can represent. The NIP spec has no
        // 10 -> 0 fallback, so these numbers are unissuable and must be rejected.
        [InlineData("3170669070", false)]
        [InlineData("8436469740", false)]
        [InlineData("8212378990", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("123456789", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _polandValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("00-950", true)]
        [InlineData("05470", true)]
        [InlineData("213213", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("ab-cde", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _polandValidator.ValidatePostalCode(code).IsValid);
        }

    }
}
