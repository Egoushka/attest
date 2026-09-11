using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class JapanValidatorTests
    {
        private readonly JapanValidator _japanValidator;

        public JapanValidatorTests()
        {
            _japanValidator = new JapanValidator();
        }

        // Corporate Number (hojin bango): 13 digits, the leading digit is the check digit,
        // computed as 9 - (sum of the remaining 12 digits weighted 1,2,1,2... from the right mod 9).
        // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.jp.cn
        [Theory]
        [InlineData("5835678256246", true)]      // python-stdnum jp.cn example
        [InlineData("5-8356-7825-6246", true)]   // same number in presentation format
        [InlineData("1180301018771", true)]      // check digit 1 computed from the published rule
        [InlineData("4010401032645", true)]      // check digit 4 computed from the published rule
        [InlineData("2835678256246", false)]     // python-stdnum jp.cn wrong check digit example
        [InlineData("1180301018772", false)]     // wrong check digit
        [InlineData("583567825624", false)]      // 12 digits
        [InlineData("58356782562461", false)]    // 14 digits
        [InlineData("abcdefghijklm", false)]     // 13 characters, none of them a digit
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestEntity(string code, bool isValid)
        {
            Assert.Equal(isValid, _japanValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("5835678256246", true)]
        [InlineData("2835678256246", false)]
        [InlineData("583567825624", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestVat(string code, bool isValid)
        {
            Assert.Equal(isValid, _japanValidator.ValidateVAT(code).IsValid);
        }

        // My Number (kojin bango): 12 digits, the last one is the check digit.
        // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.jp.in_
        [Theory]
        [InlineData("621498320257", true)]       // python-stdnum jp.in_ example
        [InlineData("6214 9832 0257", true)]     // same number in presentation format
        [InlineData("123456789018", true)]       // check digit 8 computed from the published rule
        [InlineData("621498320258", false)]      // python-stdnum jp.in_ wrong check digit example
        [InlineData("123456789010", false)]      // wrong check digit
        [InlineData("62149832025", false)]       // 11 digits
        [InlineData("6214983202570", false)]     // 13 digits
        [InlineData("62149832025X", false)]      // python-stdnum jp.in_ non digit example
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _japanValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("621498320257", true)]
        [InlineData("621498320258", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _japanValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("1000001", true)]            // Chiyoda, Tokyo
        [InlineData("100-0001", true)]
        [InlineData("6068501", true)]            // Sakyo, Kyoto
        [InlineData("100000", false)]            // 6 digits
        [InlineData("10000012", false)]          // 8 digits
        [InlineData("abcdefg", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _japanValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
