using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class FinlandValidatorTests
    {
        private readonly FinlandValidator _finlandValidator;

        public FinlandValidatorTests()
        {
            _finlandValidator = new FinlandValidator();
        }

        [Theory]
        [InlineData("311280-888Y", true)]
        [InlineData("131052-308T", true)]
        [InlineData("131052-308U", false)]
        [InlineData("310252-308Y", false)]
        [InlineData("010594Y9032", true)] // New 1900s separator
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _finlandValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("311280-888Y", true)]
        [InlineData("131052-308T", true)]
        [InlineData("131052-308U", false)]
        [InlineData("310252-308Y", false)]
        [InlineData("311280+888Y", true)]  // Born in the 1800s
        [InlineData("010594Y9032", true)]  // Separators added by decree 690/2022, born in the 1900s
        [InlineData("020594X903P", true)]
        [InlineData("030594W903B", true)]
        [InlineData("040594V9030", true)]
        [InlineData("050594U903M", true)]
        [InlineData("010516B903X", true)]  // Separators added by decree 690/2022, born in the 2000s
        [InlineData("020516C903K", true)]
        [InlineData("030516D9037", true)]
        [InlineData("040516E903V", true)]
        [InlineData("050516F903H", true)]
        [InlineData("010594Y9033", false)] // Wrong control character
        [InlineData("010594Z9032", false)] // Z is not a century separator
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("not a hetu", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _finlandValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("2077474-0", true)]
        [InlineData("2077474-1", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _finlandValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("20774740", true)]
        [InlineData("20774741", false)]
        [InlineData("10000080", false)] // Weighted sum leaves remainder 1, no check digit exists
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _finlandValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("22150", true)]
        [InlineData("22430", true)]
        [InlineData("2133213", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _finlandValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
