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

        [Theory]
        [InlineData("26136907-2-13", false)]
        [InlineData("18509151239", true)]  // Born 1985-09-15
        [InlineData("18510151231", true)]  // Born 1985-10-15, October is a valid month
        [InlineData("27010010776", true)]  // Born 1970-10-01, female
        [InlineData("18510151232", false)] // Wrong check digit
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
