using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class IcelandValidatorTests
    {
        private readonly IcelandValidator _icelandValidator;

        public IcelandValidatorTests()
        {
            _icelandValidator = new IcelandValidator();
        }

        [Theory]
        [InlineData("1207742209", true)]  // Born 1974-07-12, check digit 0 (weighted sum 110, divisible by 11)
        [InlineData("0311880409", true)]  // Born 1988-11-03, check digit 0
        [InlineData("1207740509", true)]  // Born 1974-07-12, check digit 0
        [InlineData("120774-2209", true)] // Same number as printed, with the separator
        [InlineData("0101904529", true)]  // Born 1990-01-01, check digit 2
        [InlineData("3103991169", true)]  // Born 1999-03-31, check digit 6
        [InlineData("0101904519", false)] // Wrong check digit, should be 2
        [InlineData("1207740009", false)] // Weighted sum mod 11 is 1, so no check digit can match
        [InlineData("3002901109", false)] // 30 February never exists
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghij", false)]
        [InlineData("123", false)]
        [InlineData("12077422090", false)] // Too long
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _icelandValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("1207742209", true)]
        [InlineData("0101904519", false)]
        [InlineData(null, false)]
        public void TestEntity(string code, bool isValid)
        {
            Assert.Equal(isValid, _icelandValidator.ValidateEntity(code).IsValid);
        }
    }
}
