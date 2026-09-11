using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class SloveniaValidatorTests
    {
        private readonly SloveniaValidator _sloveniaValidator;

        public SloveniaValidatorTests()
        {
            _sloveniaValidator = new SloveniaValidator();
        }

        [Theory]
        [InlineData("0101006500006", true)]
        [InlineData("0101006500007", false)]
        [InlineData("010100650000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghijklm", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _sloveniaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("50223054", true)]
        [InlineData("50223055", false)]
        [InlineData("09999990", false)]
        [InlineData("SI50223054", true)]
        // Check digits computed from the DDV rule: 11 - (weighted sum mod 11), weights
        // 8,7,6,5,4,3,2 over the first seven digits.
        [InlineData("77300246", true)]
        // 11 - (sum mod 11) == 10 does map to check digit 0, so these stay valid.
        [InlineData("68483040", true)]
        [InlineData("21114650", true)]
        [InlineData("11718510", true)]
        // 11 - (sum mod 11) == 11 (weighted sum divisible by 11) has no representable check
        // digit, so these numbers are unissuable and must be rejected.
        [InlineData("25031090", false)]
        [InlineData("24330140", false)]
        [InlineData("27850250", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("1234567", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sloveniaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("50223054", true)]
        [InlineData("50223055", false)]
        [InlineData("09999990", false)]
        [InlineData("SI50223054", true)]
        // Check digits computed from the DDV rule: 11 - (weighted sum mod 11), weights
        // 8,7,6,5,4,3,2 over the first seven digits.
        [InlineData("77300246", true)]
        // 11 - (sum mod 11) == 10 does map to check digit 0, so these stay valid.
        [InlineData("68483040", true)]
        [InlineData("21114650", true)]
        [InlineData("11718510", true)]
        // 11 - (sum mod 11) == 11 (weighted sum divisible by 11) has no representable check
        // digit, so these numbers are unissuable and must be rejected.
        [InlineData("25031090", false)]
        [InlineData("24330140", false)]
        [InlineData("27850250", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("1234567", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sloveniaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("50223054", true)]
        [InlineData("50223055", false)]
        [InlineData("09999990", false)]
        [InlineData("SI50223054", true)]
        // Check digits computed from the DDV rule: 11 - (weighted sum mod 11), weights
        // 8,7,6,5,4,3,2 over the first seven digits.
        [InlineData("77300246", true)]
        // 11 - (sum mod 11) == 10 does map to check digit 0, so these stay valid.
        [InlineData("68483040", true)]
        [InlineData("21114650", true)]
        [InlineData("11718510", true)]
        // 11 - (sum mod 11) == 11 (weighted sum divisible by 11) has no representable check
        // digit, so these numbers are unissuable and must be rejected.
        [InlineData("25031090", false)]
        [InlineData("24330140", false)]
        [InlineData("27850250", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("1234567", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sloveniaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("4000", true)]
        [InlineData("2500", true)]
        [InlineData("211212", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcd", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _sloveniaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
