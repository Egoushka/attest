using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class KazahstanValidatorTests
    {
        private readonly KazahstanValidator _kazahstanValidator;

        public KazahstanValidatorTests()
        {
            _kazahstanValidator = new KazahstanValidator();
        }

        [Theory]
        [InlineData("900701300108", true)]
        [InlineData("851231401208", true)]
        [InlineData("900701300107", false)]
        [InlineData("KZ900701300108", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _kazahstanValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("900701300108", true)]     // Male born 1990-07-01, check digit from the first weight set
        [InlineData("851231401208", true)]     // Female born 1985-12-31
        [InlineData("050311600009", true)]     // Female born 2005-03-11
        [InlineData("770101499905", true)]     // Female born 1977-01-01
        [InlineData("000229501204", true)]     // Male born 2000-02-29
        [InlineData("900701301802", true)]     // First weight set gives 10, so the alternative set decides
        [InlineData("900701 300108", true)]    // Same number with a separator
        [InlineData("900701-300108", true)]
        [InlineData("900701300107", false)]    // Wrong check digit
        [InlineData("851231401200", false)]    // Wrong check digit
        [InlineData("900701301840", false)]    // Both weight sets give 10, such a number is never issued
        [InlineData("900701301841", false)]
        [InlineData("901301300108", false)]    // Month 13
        [InlineData("900700300108", false)]    // Day 00
        [InlineData("KZ900701300108", false)]  // Letters are not a valid prefix
        [InlineData("9007013001089", false)]   // Thirteen digits
        [InlineData("90070130010", false)]     // Eleven digits
        [InlineData("12345678901x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _kazahstanValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("230340123459", true)]     // Resident legal entity, head office, registered 2023-03
        [InlineData("120741000014", true)]     // Individual entrepreneur, branch, registered 2012-07
        [InlineData("051162005005", true)]     // Non resident, representation, registered 2005-11
        [InlineData("23 03 40 12345 9", true)] // Same number with separators
        [InlineData("230340123450", false)]    // Wrong check digit
        [InlineData("KZ230340123459", false)]  // Letters are not a valid prefix
        [InlineData("9999230340123459", false)]// Sixteen digits
        [InlineData("abcdefghijkl", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _kazahstanValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("230340123459", true)]
        [InlineData("120741000014", true)]
        [InlineData("230340123450", false)]
        [InlineData("KZ230340123459", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _kazahstanValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("050000", true)]
        [InlineData("010000", true)]
        [InlineData("050 000", true)]
        [InlineData("0500", false)]
        [InlineData("0500000", false)]
        [InlineData("A50000", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _kazahstanValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
