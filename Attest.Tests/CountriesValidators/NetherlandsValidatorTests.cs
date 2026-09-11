using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class NetherlandsValidatorTests
    {
        private readonly NetherlandsValidator _netherlandsValidator;

        public NetherlandsValidatorTests()
        {
            _netherlandsValidator = new NetherlandsValidator();
        }

        [Theory]
        [InlineData("111222333", true)]
        [InlineData("941331490", true)]
        [InlineData("101222331", true)]
        [InlineData("9413.31.490", true)]
        [InlineData("941331491", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("notanumber", false)]
        [InlineData("101234567890123", false)] // More digits than an int holds
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _netherlandsValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("111222333", true)]
        [InlineData("941331490", true)]
        [InlineData("101222331", false)]
        [InlineData("notanumber", false)]
        [InlineData("9413.31.490", true)]
        [InlineData("941331491", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _netherlandsValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("004495445B01", true)]
        [InlineData("123456789B90", false)]
        [InlineData("NL000099998B57", true)] // btw-identificatienummer, business.gov.nl example
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _netherlandsValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("004495445B01", true)]      // Legacy BSN derived btw-nummer
        [InlineData("123456789B90", false)]
        [InlineData("NL000099998B57", true)]    // btw-identificatienummer, business.gov.nl example
        [InlineData("NL002455799B11", true)]    // Issued since 2020-01-01, MOD 97-10 only
        [InlineData("123456789B13", true)]      // MOD 97-10 check digits over "NL" + number
        [InlineData("000099998B58", false)]     // Wrong check digits
        [InlineData("100000060B01", false)]     // Mod 11 remainder 10, no valid check digit exists
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("garbage", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _netherlandsValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("1234 AB", true)]
        [InlineData("2490 AA", true)]
        [InlineData("1321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _netherlandsValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
