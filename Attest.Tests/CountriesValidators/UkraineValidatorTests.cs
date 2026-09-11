using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class UkraineValidatorTests
    {
        private readonly UkraineValidator _ukraineValidator;

        public UkraineValidatorTests()
        {
            _ukraineValidator = new UkraineValidator();
        }

        // РНОКПП check digit, weights (-1, 5, 7, 9, 4, 6, 10, 5, 7), the sum modulo 11 modulo 10.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ua/rntrc.py
        [Theory]
        [InlineData("1759013776", true)]        // python-stdnum example
        [InlineData("2530414071", true)]        // python-stdnum example
        [InlineData("25 30 41 40 71", true)]    // Same number with separators
        [InlineData("3305412340", true)]        // Born 1990-07-01, check digit computed
        [InlineData("3141156784", true)]        // Born 1985-12-31, check digit computed
        [InlineData("9000000002", true)]        // The negative first weight makes the sum -9 here
        [InlineData("1759013770", false)]       // Wrong check digit
        [InlineData("2530414070", false)]       // Wrong check digit
        [InlineData("3305412341", false)]       // Wrong check digit
        [InlineData("175901377", false)]        // Nine digits
        [InlineData("17590137761", false)]      // Eleven digits
        [InlineData("175901377x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukraineValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // ЄДРПОУ check digit.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ua/edrpou.py
        [Theory]
        [InlineData("32855961", true)]          // python-stdnum example
        [InlineData("14360570", true)]          // PrivatBank
        [InlineData("00032129", true)]          // National Bank of Ukraine
        [InlineData("21560766", true)]
        [InlineData("32 855 961", true)]        // Same number with separators
        [InlineData("30000005", true)]          // First pass gives 10, so the raised weights decide
        [InlineData("32855968", false)]         // Wrong check digit
        [InlineData("14360571", false)]         // Wrong check digit
        [InlineData("00032120", false)]         // Wrong check digit
        [InlineData("3285596", false)]          // Seven digits
        [InlineData("328559610", false)]        // Nine digits
        [InlineData("3285596x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukraineValidator.ValidateEntity(code).IsValid);
        }

        // The ІПН of a VAT payer is twelve digits; no check digit algorithm is published for it,
        // so only the format is validated.
        [Theory]
        [InlineData("123456789012", true)]
        [InlineData("143605704081", true)]
        [InlineData("123 456 789 012", true)]   // Same number with separators
        [InlineData("12345678901", false)]      // Eleven digits
        [InlineData("1234567890123", false)]    // Thirteen digits
        [InlineData("12345678901x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukraineValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("01001", true)]             // Kyiv
        [InlineData("79000", true)]             // Lviv
        [InlineData("01 001", true)]
        [InlineData("0100", false)]             // Four digits
        [InlineData("010011", false)]           // Six digits
        [InlineData("A1001", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukraineValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
