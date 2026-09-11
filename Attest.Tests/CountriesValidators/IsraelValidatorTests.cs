using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class IsraelValidatorTests
    {
        private readonly IsraelValidator _israelValidator;

        public IsraelValidatorTests()
        {
            _israelValidator = new IsraelValidator();
        }

        // Mispar Zehut: nine digits with a Luhn check digit.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/il/idnr.py
        [Theory]
        [InlineData("039337423", true)]   // python-stdnum doctest ('3933742-3')
        [InlineData("123456782", true)]   // check digit computed from the Luhn rule
        [InlineData("039337422", false)]  // wrong check digit (python-stdnum doctest)
        [InlineData("39337423", false)]   // 8 digits, the validator does not pad
        [InlineData("23456789a", false)]  // a letter scores -1 and used to reach a total of 40
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _israelValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // Company number (H.P.): nine digits starting with 5, Luhn check digit.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/il/hp.py
        [Theory]
        [InlineData("516179157", true)]    // python-stdnum doctest
        [InlineData("5161 79157", true)]
        [InlineData("587654328", true)]    // check digit computed from the Luhn rule
        [InlineData("516179150", false)]   // wrong check digit (python-stdnum doctest)
        [InlineData("416179157", false)]   // does not start with 5 (python-stdnum doctest)
        [InlineData("0516179157", false)]  // 10 digits
        [InlineData("050000009", false)]   // nine digits, but the first one is not 5
        [InlineData("59", false)]          // used to be zero padded to 000000059 and accepted
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _israelValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("516179157", true)]
        [InlineData("587654328", true)]
        [InlineData("516179150", false)]
        [InlineData("416179157", false)]
        [InlineData("59", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _israelValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("9103401", true)]
        [InlineData("6100000", true)]
        [InlineData("12345", false)]
        [InlineData("91034011", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _israelValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
