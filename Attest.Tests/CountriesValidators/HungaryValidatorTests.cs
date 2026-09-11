using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class HungaryValidatorTests
    {
        private readonly HungaryValidator _hungaryValidator;

        public HungaryValidatorTests()
        {
            _hungaryValidator = new HungaryValidator();
        }

        // Szemelyi azonosito: M YYMMDD SSS K. The check digit is sum(digit * weight) mod 11,
        // with weights 1..10 for births up to 1996-12-31 and reversed weights 10..1 from
        // 1997-01-01 onwards. https://hu.wikipedia.org/wiki/Szem%C3%A9lyi_azonos%C3%ADt%C3%B3
        [Theory]
        [InlineData("26136907-2-13", false)]
        [InlineData("18509151239", true)]  // Born 1985-09-15
        [InlineData("18510151231", true)]  // Born 1985-10-15, October is a valid month
        [InlineData("27010010776", true)]  // Born 1970-10-01, female
        [InlineData("18510151232", false)] // Wrong check digit
        [InlineData("29612311231", true)]  // Born 1996-12-31, last day of the 1..10 weights
        [InlineData("29701011231", true)]  // Born 1997-01-01, first day of the 10..1 weights
        [InlineData("19803120076", true)]  // Born 1998-03-12, serial 007, reversed weights
        [InlineData("40012253112", true)]  // Born 2000-12-25, female, leading 4 means 20xx
        [InlineData("19803120075", false)] // 1998 birth carrying the pre-1997 check digit
        [InlineData("18509151232", false)] // 1985 birth carrying the post-1996 check digit
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _hungaryValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Adoazonosito jel: 8 + days since 1867-01-01 + serial + check digit,
        // where the check digit is the sum of digit * position (1..9) modulo 11.
        // https://hu.wikipedia.org/wiki/Ad%C3%B3azonos%C3%ADt%C3%B3_jel
        [Theory]
        [InlineData("26136907-2-13", false)]
        [InlineData("8400001230", true)]  // Born 1976-07-08, serial 123
        [InlineData("8414071236", true)]  // Born 1980-05-15, serial 123
        [InlineData("8452584563", true)]  // Born 1990-11-30, serial 456
        [InlineData("8400001231", false)] // Wrong check digit
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _hungaryValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("13-09-189347", true)]
        [InlineData("01-10-042595", true)]
        [InlineData("10949621-2-44", false)]
        [InlineData("13-9-189347", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _hungaryValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("HU-12892312", true)]
        [InlineData("HU-12892313", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _hungaryValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("1037", true)]
        [InlineData("2380", true)]
        [InlineData("12321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _hungaryValidator.ValidatePostalCode(code).IsValid);
        }

    }
}
