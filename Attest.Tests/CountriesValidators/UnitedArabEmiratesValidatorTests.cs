using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class UnitedArabEmiratesValidatorTests
    {
        private readonly UnitedArabEmiratesValidator _unitedArabEmiratesValidator;

        public UnitedArabEmiratesValidatorTests()
        {
            _unitedArabEmiratesValidator = new UnitedArabEmiratesValidator();
        }

        // Emirates ID, fifteen digits shaped 784-YYYY-NNNNNNN-C: the 784 ISO 3166-1 numeric code
        // for the UAE, the year of birth, a serial and a check digit.
        // https://u.ae/en/information-and-services/visa-and-emirates-id/emirates-id/what-is-emirates-id
        // ICP publishes no check digit algorithm, so the validator only enforces the format; the
        // valid samples below nevertheless carry the Luhn check digit of their first fourteen
        // digits, so they stay valid if a checksum is ever added.
        [Theory]
        [InlineData("784198012345678", true)]
        [InlineData("784-1980-1234567-8", true)]  // Same number, as printed on the card
        [InlineData("784197612345674", true)]
        [InlineData("123-1234-0123456-7", false)] // Prefix is not 784
        [InlineData("78419801234567", false)]     // Fourteen digits
        [InlineData("7841980123456789", false)]   // Sixteen digits
        [InlineData("78419801234567X", false)]    // Letter in the check position
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _unitedArabEmiratesValidator.ValidateNationalIdentity(code).IsValid);
        }
    }
}
