using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class TurkeyValidatorTests
    {
        private readonly TurkeyValidator _turkeyValidator;

        public TurkeyValidatorTests()
        {
            _turkeyValidator = new TurkeyValidator();
        }

        // T.C. Kimlik No: 11 digits, never starting with 0, two trailing check digits.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/tr/tckimlik.py
        [Theory]
        [InlineData("17291716060", true)]   // python-stdnum doctest
        [InlineData("172 917 160 60", true)]
        [InlineData("12345678950", true)]   // check digits computed from the published rule
        [InlineData("98765432150", true)]
        [InlineData("17291716050", false)]  // wrong check digits (python-stdnum doctest)
        [InlineData("07291716092", false)]  // leading zero (python-stdnum doctest)
        [InlineData("1729171606", false)]   // 10 digits (python-stdnum doctest)
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _turkeyValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // VKN (Vergi Kimlik Numarasi): 10 digits with a trailing check digit.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/tr/vkn.py
        [Theory]
        [InlineData("4540536920", true)]    // python-stdnum doctest
        [InlineData("1234567890", true)]    // check digit computed from the published rule
        [InlineData("4540536921", false)]   // wrong check digit (python-stdnum doctest)
        [InlineData("454053692", false)]    // 9 digits (python-stdnum doctest)
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _turkeyValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("4540536920", true)]
        [InlineData("TR 4540536920", true)]
        [InlineData("tr4540536920", true)]
        [InlineData("9876543217", true)]    // check digit computed from the published rule
        [InlineData("4540536921", false)]
        [InlineData("45405369201", false)]  // 11 digits
        [InlineData("1TR234567890", false)] // TR is only a country prefix, not a separator
        [InlineData("12TR34567890", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _turkeyValidator.ValidateVAT(code).IsValid);
        }

        // Five digits opening with the licence plate code of one of the 81 provinces, 01 to 81.
        // https://en.wikipedia.org/wiki/Postal_codes_in_Turkey
        // https://en.wikipedia.org/wiki/Provinces_of_Turkey
        [Theory]
        [InlineData("34000", true)]
        [InlineData("06010", true)]
        [InlineData("01010", true)]     // Province 01, Adana
        [InlineData("81000", true)]     // Province 81, Duzce, created in 1999
        [InlineData("00000", false)]    // 00 is not a province code
        [InlineData("99999", false)]    // 99 is not a province code
        [InlineData("82000", false)]    // There is no 82nd province
        [InlineData("123", false)]
        [InlineData("340000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _turkeyValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
