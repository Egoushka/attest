using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class PeruValidatorTests
    {
        private readonly PeruValidator _peruValidator;

        public PeruValidatorTests()
        {
            _peruValidator = new PeruValidator();
        }

        [Theory]
        // CUI printed on the DNI: eight digits and an optional check digit or check letter.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/cui.py
        [InlineData("10117410", true)]
        [InlineData("10117410-2", true)]
        [InlineData("101174102", true)]
        // Letter form of the same check value: weights 3,2,7,6,5,4,3,2 mod 11 gives index 4,
        // where 65432110987 holds '2' and KJIHGFEDCBA holds 'G'.
        [InlineData("10117410G", true)]
        [InlineData("10117410g", true)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("10117410-3", false)]
        [InlineData("101174109", false)]
        [InlineData("1011741", false)]
        [InlineData("1011741A", false)]
        [InlineData("1011741012", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        // RUC. https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/ruc.py
        [InlineData("20512333797", true)]
        [InlineData("10054148289", true)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("20512333798", false)]
        // 30 is not one of the 10, 15, 17, 20 taxpayer types.
        [InlineData("30512333797", false)]
        [InlineData("2051233379", false)]
        [InlineData("205123337971", false)]
        [InlineData("2051233379A", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        // Companies use the same RUC.
        [InlineData("20512333797", true)]
        [InlineData("10054148289", true)]
        [InlineData("20512333798", false)]
        [InlineData("30512333797", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("20512333797", true)]
        [InlineData("10054148289", true)]
        [InlineData("20512333798", false)]
        [InlineData("2051233379", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        // Five digits, Lima.
        [InlineData("15001", true)]
        [InlineData("1500", false)]
        [InlineData("150011", false)]
        [InlineData("abcde", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
