using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class NorwayValidatorTests
    {
        private readonly NorwayValidator _norwayValidator;

        public NorwayValidatorTests()
        {
            _norwayValidator = new NorwayValidator();
        }

        // Fodselsnummer: 11 digits, DDMMYY + 3 individual digits + 2 mod-11 check digits.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/no/fodselsnummer.py
        [Theory]
        [InlineData("15108695088", true)]   // python-stdnum doctest
        [InlineData("151086 95088", true)]  // same number with a separator
        [InlineData("15078512323", true)]   // born 1985-07-15, individual 123, check digits computed from the mod-11 rule
        [InlineData("13047230157", true)]   // born 1972-04-13, individual 301
        [InlineData("55078512317", true)]   // D-number for a non resident, day 15 + 40
        [InlineData("15108695077", false)]  // wrong first check digit (python-stdnum doctest)
        [InlineData("80078512354", false)]  // FH-number (day >= 80), carries no birth date by design
        [InlineData("15138512301", false)]  // month 13 does not exist
        [InlineData("1510869508", false)]   // 10 digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _norwayValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // Organisasjonsnummer: 9 digits, weights 3-2-7-6-5-4-3-2 mod 11.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/no/orgnr.py
        [Theory]
        [InlineData("988077917", true)]     // python-stdnum doctest
        [InlineData("976760271", true)]     // check digit computed from the mod-11 rule
        [InlineData("988077918", false)]    // wrong check digit (python-stdnum doctest)
        [InlineData("12345678", false)]     // 8 digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _norwayValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("988077917", true)]
        [InlineData("NO 988 077 917 MVA", true)]
        [InlineData("812345672", true)]     // check digit computed from the mod-11 rule
        [InlineData("988077918", false)]    // wrong check digit
        [InlineData("98807791", false)]     // 8 digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _norwayValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("0010", true)]
        [InlineData("5003", true)]
        [InlineData("123", false)]
        [InlineData("12345", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _norwayValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
