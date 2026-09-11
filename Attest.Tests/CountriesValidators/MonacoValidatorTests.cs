using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MonacoValidatorTests
    {
        private readonly MonacoValidator _monacoValidator;

        public MonacoValidatorTests()
        {
            _monacoValidator = new MonacoValidator();
        }

        // Monegasque companies are issued a French TVA number whose SIREN part starts with "000";
        // the two leading digits are the key, int(key) == int(siren + "12") % 97.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/fr/tva.py
        [Theory]
        [InlineData("53000004605", true)]    // SIREN 000004605, key 53 computed from the mod-97 rule
        [InlineData("FR53000004605", true)]
        [InlineData("47000076965", true)]    // SIREN 000076965, key 47
        [InlineData("53000004606", false)]   // key no longer matches the SIREN
        [InlineData("40303265045", false)]   // valid French TVA, but the SIREN does not start with "000"
        [InlineData("5300000460", false)]    // 10 characters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _monacoValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("98000", true)]
        [InlineData("98012", true)]
        [InlineData("75008", false)]   // a French postal code
        [InlineData("9800", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _monacoValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
