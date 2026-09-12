using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class FaroeIslandsValidatorTests
    {
        private readonly FaroeIslandsValidator _faroeIslandsValidator;

        public FaroeIslandsValidatorTests()
        {
            _faroeIslandsValidator = new FaroeIslandsValidator();
        }

        // V-tal (vinnutal): six digits assigned by TAKS, no published check digit.
        // https://lookuptax.com/docs/tax-identification-number/faroe-islands-tax-id-guide
        [Theory]
        [InlineData("530007", true)]
        [InlineData("123 456", true)]
        [InlineData("12345", false)]     // 5 digits
        [InlineData("1234567", false)]   // 7 digits
        [InlineData("abcdef", false)]    // not numeric
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("530007", true)]
        [InlineData("123 456", true)]
        [InlineData("12345", false)]
        [InlineData("1234567", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidateVAT(code).IsValid);
        }

        // P-tal: nine digits, DDMMYY followed by a three digit serial. Section II of the Faroese
        // TIN sheet gives the format as "Ddmmyyxxx (ddmmyy-xxx) 9 digits", so the leading six
        // digits are a date of birth. The century is not published, so the year is not resolved
        // and the rows below do not depend on one.
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/faroe-islands-tin.pdf
        // https://www.norden.org/en/info-norden/civil-registration-number-faroe-islands-p-number
        [Theory]
        [InlineData("150785123", true)]
        [InlineData("010190-456", true)]
        [InlineData("311299001", true)]    // 31 December, the last day of a long month
        [InlineData("999999999", false)]   // day 99 and month 99 are not a date
        [InlineData("000000000", false)]   // day 00 and month 00 are not a date
        [InlineData("320785123", false)]   // no month has a 32nd day
        [InlineData("151385123", false)]   // month 13 does not exist
        [InlineData("15078512", false)]    // 8 digits
        [InlineData("1507851234", false)]  // 10 digits
        [InlineData("abcdefghi", false)]   // not numeric
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // Postal codes are three digits running from 100 (Torshavn) to 970 (Sumba), with large
        // unassigned gaps, so only the 000-099 block is rejected on range.
        // https://da.wikipedia.org/wiki/Postnumre_p%C3%A5_F%C3%A6r%C3%B8erne
        [Theory]
        [InlineData("100", true)]      // Lowest assigned code, Torshavn
        [InlineData("180", true)]
        [InlineData("970", true)]      // Highest assigned code
        [InlineData("000", false)]     // No code begins with a zero
        [InlineData("099", false)]
        [InlineData("12", false)]
        [InlineData("1000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _faroeIslandsValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
