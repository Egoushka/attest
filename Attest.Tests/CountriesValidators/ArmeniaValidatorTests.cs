using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ArmeniaValidatorTests
    {
        private readonly ArmeniaValidator _armeniaValidator;

        public ArmeniaValidatorTests()
        {
            _armeniaValidator = new ArmeniaValidator();
        }

        // The ՀՎՀՀ (TIN) is issued to both individuals and entities and is exactly eight digits;
        // the eighth is a check digit whose algorithm is not published, so only the format is
        // validated.
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/armenia-tin.pdf
        [Theory]
        [InlineData("02618169", true)]
        [InlineData("00021341", true)]
        [InlineData("026 181 69", true)]        // Same number with separators
        [InlineData("1101850123", false)]       // Public services number, not a TIN
        [InlineData("0261816", false)]          // Seven digits
        [InlineData("026181690", false)]        // Nine digits
        [InlineData("0261816x", false)]
        [InlineData("AM02618169", false)]       // Letters are not a valid prefix
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // The same TIN identifies an entity, so an entity cannot be told apart from an individual
        // by its number - only the ten digit public services number is personal.
        [Theory]
        [InlineData("02618169", true)]
        [InlineData("00021341", true)]
        [InlineData("1101850123", false)]       // Public services number, never issued to an entity
        [InlineData("0261816", false)]          // Seven digits
        [InlineData("026181690", false)]        // Nine digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("02618169", true)]
        [InlineData("00021341", true)]
        [InlineData("1101850123", false)]       // Public services number, not a VAT number
        [InlineData("0261816", false)]          // Seven digits
        [InlineData("026181690", false)]        // Nine digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidateVAT(code).IsValid);
        }

        // The public services number is the ten digit identifier of a natural person and is a
        // different number from the eight digit TIN: DD is the day of birth plus 10 for men and
        // plus 50 for women, MM the month plus 20 per century (June coded 8 higher still), YY the
        // year, SSS a serial 001-999 and C an unpublished check digit. The digit 6 may not occur
        // three times in a row. Article 4 of law HO-288-N of 30.11.2011,
        // https://www.arlis.am/hy/acts/87872, repeating government decision N 1783-N of
        // 24.12.2003, https://www.arlis.am/hy/acts/36172. The values below are constructed, not
        // numbers of real people.
        [Theory]
        [InlineData("1101850123", true)]        // Man born on 1 January 1985
        [InlineData("8103904567", true)]        // Woman born on 31 March 1990
        [InlineData("1114700018", true)]        // June 1970: month coded 14, not 06
        [InlineData("1534050012", true)]        // June 2005: 21st century, month coded 34
        [InlineData("5192950010", true)]        // December 1895: 19th century, month coded 92
        [InlineData("11 01 85 0123", true)]     // Same number with separators
        [InlineData("02618169", false)]         // TIN, which is not a public services number
        [InlineData("0101850123", false)]       // Day pair 01: below the male range
        [InlineData("4501850123", false)]       // Day pair 45: between the male and female ranges
        [InlineData("9101850123", false)]       // Day pair 91: above the female range
        [InlineData("1113850123", false)]       // Month pair 13
        [InlineData("1120850123", false)]       // Month pair 20
        [InlineData("1101850003", false)]       // Serial 000, the serial runs 001-999
        [InlineData("1101856661", false)]       // Three consecutive sixes
        [InlineData("110185012", false)]        // Nine digits
        [InlineData("11018501234", false)]      // Eleven digits
        [InlineData("110185012x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalIdentity(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("0010", true)]              // Yerevan
        [InlineData("3101", true)]              // Gyumri
        [InlineData("0010 ", true)]
        [InlineData("001", false)]              // Three digits
        [InlineData("00100", false)]            // Five digits
        [InlineData("A010", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _armeniaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
