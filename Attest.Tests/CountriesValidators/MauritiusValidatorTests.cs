using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MauritiusValidatorTests
    {
        private readonly MauritiusValidator _mauritiusValidator;

        public MauritiusValidatorTests()
        {
            _mauritiusValidator = new MauritiusValidator();
        }

        // National Identity Card number: the initial of the surname, DDMMYY of birth, a six digit
        // serial and a check character. The check character is the alphabet "0123456789A-Z"
        // indexed by (17 - sum) mod 17, where sum runs the first thirteen characters - the leading
        // letter included, valued by its position in that alphabet - against the weights 14..2.
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.mu.nid.html
        // Check characters below were computed with that algorithm.
        [Theory]
        [InlineData("A250690500360A", true)]
        [InlineData("M120385123456E", true)]
        [InlineData("S310178004567B", true)]
        [InlineData("A250690500360B", false)]   // Wrong check character
        [InlineData("A250690500360", false)]    // Thirteen characters
        [InlineData("A2506905003601A", false)]  // Fifteen characters
        [InlineData("1250690500360A", false)]   // Leading character is not a letter
        [InlineData("A320690500360G", false)]   // Day 32, check character is correct
        [InlineData("A251390500360C", false)]   // Month 13, check character is correct
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _mauritiusValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // The identity card number doubles as the national identifier, so the base class
        // delegation applies.
        [Theory]
        [InlineData("A250690500360A", true)]
        [InlineData("M120385123456E", true)]
        [InlineData("A250690500360B", false)]   // Wrong check character
        [InlineData("A250690500360", false)]    // Thirteen characters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _mauritiusValidator.ValidateNationalIdentity(code).IsValid);
        }
    }
}
