using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class TaiwanValidatorTests
    {
        private readonly TaiwanValidator _taiwanValidator;

        public TaiwanValidatorTests()
        {
            _taiwanValidator = new TaiwanValidator();
        }

        // National id: letter + [12] + 8 digits. The letter maps to two digits (A=10 ... Z=33,
        // I=34, O=35) weighted 1 and 9, the following digits are weighted 8..1 and the check
        // digit 1; the total must be a multiple of 10.
        // https://en.wikipedia.org/wiki/National_identification_card_(Taiwan)
        [Theory]
        [InlineData("A123456789", true)]        // textbook example, weighted sum 130
        [InlineData("A223344553", true)]        // check digit 3 computed from the published rule
        [InlineData("F131425364", true)]        // check digit 4 computed from the published rule
        [InlineData("L203040508", true)]        // check digit 8 computed from the published rule
        [InlineData("AB12345677", true)]        // resident certificate, check digit 7 computed
        [InlineData("FC22334450", true)]        // resident certificate, check digit 0 computed
        [InlineData("A123456788", false)]       // wrong check digit
        [InlineData("AB12345678", false)]       // wrong check digit
        [InlineData("A323456789", false)]       // third character of the gender digit is not 1 or 2
        [InlineData("A12345678", false)]        // 9 characters
        [InlineData("A1234567890", false)]      // 11 characters
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("A123456789", true)]
        [InlineData("A123456788", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("A123456789", true)]
        [InlineData("L203040508", true)]
        [InlineData("A123456788", false)]       // wrong check digit
        [InlineData("AB12345677", false)]       // resident certificate, not a local id
        [InlineData("A12345678", false)]        // 9 characters
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestLocalSSN(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateLocalSSN(code).IsValid);
        }

        [Theory]
        [InlineData("AB12345677", true)]
        [InlineData("AD00011124", true)]        // check digit 4 computed from the published rule
        [InlineData("AB12345678", false)]       // wrong check digit
        [InlineData("A123456789", false)]       // local id, not a resident certificate
        [InlineData("AE12345677", false)]       // second letter outside A-D
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestResidentSSN(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateResidentSSN(code).IsValid);
        }

        [Theory]
        [InlineData("10058", true)]             // Zhongzheng, Taipei
        [InlineData("40341", true)]             // West district, Taichung
        [InlineData("1234", false)]             // 4 digits
        [InlineData("123456", false)]           // 6 digits
        [InlineData("abcde", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
