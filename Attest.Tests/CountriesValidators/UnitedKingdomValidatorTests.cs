using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class UnitedKingdomValidatorTests
    {
        private readonly UnitedKingdomValidator _ukValidator;
        public UnitedKingdomValidatorTests()
        {
            _ukValidator = new UnitedKingdomValidator();
        }

        [Theory]
        [InlineData("9434765870", true)]
        [InlineData("9434765871", false)]
        [InlineData("943 476 5870", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("not a number", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("AB123456C", true)]
        [InlineData("TN50X", false)]
        [InlineData("20267565392", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("980780684", true)]
        [InlineData("802311781", false)]
        [InlineData("6640211", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("ABCDEFGHI", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("980780684", true)]
        [InlineData("802311781", false)]
        [InlineData("GB980780684", true)]
        [InlineData("GD001", true)]     // Government department, 000-499
        [InlineData("GD500", false)]
        [InlineData("HA500", true)]     // Health authority, 500-999
        [InlineData("HA499", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("GDABC", false)]
        [InlineData("ABCDEFGHI", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("EC1Y 8SY", true)]
        [InlineData("GIR0AA", true)]
        [InlineData("321311", false)]
        [InlineData("SW1A 1AA", true)]
        [InlineData("sw1a 1aa", true)]
        [InlineData("BFPO 1234", true)]
        [InlineData("EC1Y8SYXX", false)]
        [InlineData("EC1Y 8SY is not a postcode", false)]
        [InlineData("NOTAPOSTCODE SW1A 1AA GARBAGE", false)]
        [InlineData("GIR 0AA JUNK", false)]
        [InlineData("EC1A 1CC", false)]  // C is not allowed in the unit part
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ukValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
