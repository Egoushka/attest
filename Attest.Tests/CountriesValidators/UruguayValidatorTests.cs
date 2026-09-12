using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class UruguayValidatorTests
    {
        private readonly UruguayValidator _uruguayValidator;

        public UruguayValidatorTests()
        {
            _uruguayValidator = new UruguayValidator();
        }

        [Theory]
        // Falls through to the RUT check.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/uy/rut.py
        [InlineData("21-100342-001-7", true)]
        [InlineData("211406340011", true)]
        [InlineData("210303670014", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _uruguayValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        // python-stdnum doctests.
        [InlineData("21-100342-001-7", true)]
        [InlineData("211003420017", true)]
        [InlineData("UY 21 140634 001 1", true)]
        [InlineData("uy 21 140634 001 1", true)]
        [InlineData("211406340011", true)]
        // Registration number 22, assigned to every taxpayer registered from 21 October 2024 by
        // DGI's resolution of 14/10/2024.
        // https://www.gub.uy/direccion-general-impositiva/comunicacion/noticias/nueva-numeracion-del-rut
        [InlineData("221003420014", true)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("210303670014", false)]
        // Check digit changed from 7 to 8.
        [InlineData("211003420018", false)]
        // Positions 9 to 11 must be 001.
        [InlineData("211003420027", false)]
        // Registration numbers 00 and 23 are out of the 01 to 22 range; both check digits are correct.
        [InlineData("001003420017", false)]
        [InlineData("231003420011", false)]
        // Six zero sequence number.
        [InlineData("210000000017", false)]
        // python-stdnum doctest: too short.
        [InlineData("12345678", false)]
        // Only a leading "UY" is stripped, so this is a 14 character string, not a RUT.
        [InlineData("2110034UY20017", false)]
        // Eastern Arabic digits: char.IsDigit would accept them and int.Parse would then throw.
        [InlineData("\u0662\u0661\u0661\u0660\u0660\u0663\u0664\u0662\u0660\u0660\u0661\u0667", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _uruguayValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("211003420017", true)]
        [InlineData("211406340011", true)]
        [InlineData("221406340019", true)]
        [InlineData("210303670014", false)]
        [InlineData("12345678", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _uruguayValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("21-100342-001-7", true)]
        [InlineData("UY 21 140634 001 1", true)]
        [InlineData("210303670014", false)]
        [InlineData("211003420027", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _uruguayValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        // Five digits, Montevideo.
        [InlineData("11000", true)]
        [InlineData("1100", false)]
        [InlineData("110000", false)]
        [InlineData("abcde", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _uruguayValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
