using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class CzechValidatorTests
    {
        private readonly CzechValidator _czechValidator;

        public CzechValidatorTests()
        {
            _czechValidator = new CzechValidator();
        }

        [Theory]
        [InlineData("7103192745", true)]
        [InlineData("991231123", false)]
        [InlineData("1103492745", false)]
        [InlineData("590312123", false)]
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
            Assert.Equal(isValid, _czechValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("7103192745", true)]
        [InlineData("991231123", false)]
        [InlineData("1103492745", false)]
        [InlineData("590312123", false)]
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
            Assert.Equal(isValid, _czechValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("25123891", true)]
        [InlineData("7103192745", true)]
        [InlineData("CZ 25123891", true)]
        [InlineData("640903926", true)]
        [InlineData("590312123", true)]
        [InlineData("25123890", false)]
        [InlineData("1103492745", false)]
        [InlineData("991231123", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _czechValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("25123891", true)]
        [InlineData("7103192745", true)]
        [InlineData("CZ 25123891", true)]
        [InlineData("640903926", true)]
        [InlineData("590312123", true)]
        [InlineData("25123890", false)]
        [InlineData("1103492745", false)]
        [InlineData("991231123", false)]

        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _czechValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("100 00", true)]
        [InlineData("22599", true)]
        [InlineData("321", false)]
        [InlineData("110-00", true)]         // Punctuated but correct, Trim() alone left the dash in place
        [InlineData("1234", false)]          // Four digits is not a Czech PSC
        [InlineData(null, false)]            // Trim() on null threw NullReferenceException
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _czechValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
