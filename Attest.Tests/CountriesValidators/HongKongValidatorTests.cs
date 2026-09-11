using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class HongKongValidatorTests
    {
        private readonly HongKongValidator _hongKongValidator;

        public HongKongValidatorTests()
        {
            _hongKongValidator = new HongKongValidator();
        }

        [Theory]
        [InlineData("A1234563", true)]
        [InlineData("O1234561", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _hongKongValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("A1234563", true)]      // One letter prefix
        [InlineData("C6686689", true)]
        [InlineData("K0000191", true)]
        [InlineData("Z683365A", true)]      // Check character A, worth 10
        [InlineData("G123456A", true)]
        [InlineData("WX1234569", true)]     // Two letter prefix
        [InlineData("XG9876549", true)]
        [InlineData("A123456(3)", true)]    // Same number as printed on the card
        [InlineData("a1234563", true)]      // Lowercase, letter value read as 'A'
        [InlineData("z683365a", true)]      // Lowercase with check character A
        [InlineData("A1234564", false)]     // Wrong check character
        [InlineData("C6686680", false)]
        [InlineData("AB1234567", false)]
        [InlineData("00000006", false)]     // Checksum passes but there is no letter prefix
        [InlineData("O1234561", false)]     // O is never issued as a prefix letter
        [InlineData("I1234565", false)]     // Neither is I
        [InlineData("AB123456Z", false)]    // Check character other than a digit or A
        [InlineData("ABCDEFGH", false)]
        [InlineData("A12345678", false)]    // Seven digits
        [InlineData("A1234563!@#$", true)]  // Special characters are stripped
        [InlineData("!@#$%^&*", false)]
        [InlineData("A123456", false)]      // Too short
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _hongKongValidator.ValidateIndividualTaxCode(code).IsValid);
        }
    }
}
