using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MexicoValidatorTests
    {
        private readonly MexicoValidator _mexicoValidator;

        public MexicoValidatorTests()
        {
            _mexicoValidator = new MexicoValidator();
        }

        [Theory]
        [InlineData("BADD110313HCMLNS06", true)]
        [InlineData("BADD110313HCMLNS07", false)] // Wrong check digit
        [InlineData("BADD110313HCMLNS0", false)]  // Check digit missing
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("not a curp", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _mexicoValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("GODE561231GR8", true)]   // Personal RFC with homoclave
        [InlineData("GODE 561231 GR8", true)] // Same number as printed
        [InlineData("COMG600703", true)]      // Personal RFC without homoclave
        [InlineData("MELM000229A16", true)]   // Born 2000-02-29, the two digit year is a 2000s year
        [InlineData("GODE561231GR9", false)]  // Wrong check digit
        [InlineData("MELM001301A16", false)]  // Month 13
        [InlineData("MELM000230A16", false)]  // 30 February
        [InlineData("COJO600703", false)]     // Blacklisted name
        [InlineData("GODE56123", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("not an rfc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _mexicoValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("MAB9307148T4", true)]   // Company RFC
        [InlineData("MAB-930714-8T4", true)] // Same number as printed
        [InlineData("SOL000229A14", true)]   // Incorporated 2000-02-29
        [InlineData("AAA010101AAA", false)]  // Wrong check digit
        [InlineData("MAB9313148T4", false)]  // Month 13
        [InlineData("MAB9302308T4", false)]  // 30 February
        [InlineData("GODE561231GR8", false)] // Personal RFC, not a company one
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("not an rfc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _mexicoValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("MAB9307148T4", true)]
        [InlineData("MAB 930714 8T4", true)]
        [InlineData("AAA010101AAA", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _mexicoValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("01000", true)]
        [InlineData("97305 ", true)]
        [InlineData("1000", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _mexicoValidator.ValidatePostalCode(code).IsValid);
        }

        /// <summary>
        /// The date of a well formed RFC used to be read at the wrong offsets, so every RFC was
        /// rejected as "Invalid date" and the check digit was never reached. Asserting the message
        /// rather than just IsValid keeps that block reachable.
        /// </summary>
        [Fact]
        public void TestChecksumIsReachedForAWellFormedDate()
        {
            Assert.Equal("Invalid checksum.", _mexicoValidator.ValidateEntity("AAA010101AAA").ErrorMessage);
        }
    }
}
