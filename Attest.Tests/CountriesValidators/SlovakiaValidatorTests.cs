using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class SlovakiaValidatorTests
    {
        private readonly SlovakiaValidator _slovakiaValidator;

        public SlovakiaValidatorTests()
        {
            _slovakiaValidator = new SlovakiaValidator();
        }

        [Theory]
        [InlineData("7103192745", true)]
        [InlineData("991231123", false)]
        [InlineData("7103192746", false)]
        [InlineData("1103492745", false)]
        [InlineData("7101011236", true)]     // 1 Jan 1971, male - January threw ArgumentOutOfRangeException (month - 1 == 0)
        [InlineData("8551010017", true)]     // 1 Jan 1985, female (month 01 + 50) - same crash
        [InlineData("7103302745", true)]     // 30 Mar 1971 - was checked against February's 28 days
        [InlineData("7112312746", true)]     // 31 Dec 1971 - was checked against November's 30 days
        [InlineData("7102300007", false)]    // 30 Feb 1971 - impossible date, was checked against January's 31 days
        [InlineData("7101011237", false)]    // Wrong check digit, 710101123 % 11 == 6
        [InlineData("123456abcd", false)]    // Garbage of valid length threw FormatException
        [InlineData("abcdefghij", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _slovakiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("7103192745", true)]
        [InlineData("991231123", false)]
        [InlineData("7103192746", false)]
        [InlineData("1103492745", false)]
        [InlineData("7101011236", true)]     // 1 Jan 1971, male - January threw ArgumentOutOfRangeException (month - 1 == 0)
        [InlineData("8551010017", true)]     // 1 Jan 1985, female (month 01 + 50) - same crash
        [InlineData("7103302745", true)]     // 30 Mar 1971 - was checked against February's 28 days
        [InlineData("7112312746", true)]     // 31 Dec 1971 - was checked against November's 30 days
        [InlineData("7102300007", false)]    // 30 Feb 1971 - impossible date, was checked against January's 31 days
        [InlineData("7101011237", false)]    // Wrong check digit, 710101123 % 11 == 6
        [InlineData("123456abcd", false)]    // Garbage of valid length threw FormatException
        [InlineData("abcdefghij", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _slovakiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("2022749619", true)]
        [InlineData("SK 202 274 96 19", true)]
        [InlineData("2022749618", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _slovakiaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("2022749619", true)]
        [InlineData("SK 202 274 96 19", true)]
        [InlineData("2022749618", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _slovakiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("010 01", true)]
        [InlineData("02314", true)]
        [InlineData("321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _slovakiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
