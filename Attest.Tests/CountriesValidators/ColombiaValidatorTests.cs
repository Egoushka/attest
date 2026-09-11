using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ColombiaValidatorTests
    {
        private readonly ColombiaValidator _colombiaValidator;

        public ColombiaValidatorTests()
        {
            _colombiaValidator = new ColombiaValidator();
        }

        [Theory]
        // Individuals are identified by the same NIT.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/co/nit.py
        [InlineData("213.123.432-1", true)]
        [InlineData("8909039388", true)]
        [InlineData("2131234325", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _colombiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        // python-stdnum doctest.
        [InlineData("213.123.432-1", true)]
        [InlineData("2131234321", true)]
        // Check digit computed with weights 3,7,13,17,19,23,29,37,41 from the right, mod 11.
        [InlineData("8909039388", true)]
        // Shortest accepted number: seven digit body plus its check digit.
        [InlineData("90012344", true)]
        [InlineData("co2131234321", true)]
        // Only a leading "CO" is stripped, so this still contains letters.
        [InlineData("213CO1234321", false)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("2131234325", false)]
        [InlineData("213123432A", false)]
        [InlineData("1234567", false)]
        [InlineData("21312343210000000", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _colombiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("2131234321", true)]
        [InlineData("8909039388", true)]
        [InlineData("2131234325", false)]
        [InlineData("1234567", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _colombiaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("213.123.432-1", true)]
        [InlineData("8909039388", true)]
        [InlineData("2131234325", false)]
        [InlineData("213123432A", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _colombiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        // Six digits, Bogota.
        [InlineData("110111", true)]
        [InlineData("11011", false)]
        [InlineData("1101111", false)]
        [InlineData("abcdef", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _colombiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
