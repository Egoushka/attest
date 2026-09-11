using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MalaysiaValidatorTests
    {
        private readonly MalaysiaValidator _malaysiaValidator;

        public MalaysiaValidatorTests()
        {
            _malaysiaValidator = new MalaysiaValidator();
        }

        [Theory]
        [InlineData("IG115002000", true)]      // IRBM/OECD published example, 9 digits
        [InlineData("IG4040080091", true)]     // IRBM/OECD published example, 10 digits
        [InlineData("IG56003500070", true)]    // IRBM/OECD published example, 11 digits
        [InlineData("SG115002000", true)]      // Legacy prefix, converted to IG on 2 January 2023
        [InlineData("OG56003500070", true)]    // Legacy prefix
        [InlineData("IG-5600350-0070", true)]  // Same number with separators
        [InlineData("IG12345678", false)]      // Too few digits
        [InlineData("IG123456789012", false)]  // Too many digits
        [InlineData("XG115002000", false)]     // Unknown prefix
        [InlineData("C20880050010", false)]    // Non-individual code
        [InlineData("I am not an ID", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _malaysiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("IG56003500070", true)]
        [InlineData("I am not an ID", false)]  // Used to be accepted, the format test was inverted
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _malaysiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("C20880050010", true)]     // Company, IRBM/OECD published example
        [InlineData("D4800990020", true)]      // Partnership, IRBM/OECD published example
        [InlineData("E91005500060", true)]     // Employer, IRBM/OECD published example
        [InlineData("F10234567090", true)]     // Association, IRBM/OECD published example
        [InlineData("CS1234567890", true)]     // Cooperative society
        [InlineData("PT12345678090", true)]    // Limited liability partnership
        [InlineData("LE12345678090", true)]    // Labuan entity
        [InlineData("J12345678090", true)]     // Hindu joint family
        [InlineData("C 2088005001 0", true)]   // Same number with separators
        [InlineData("TJ1234567890", false)]    // Not an IRBM code, the code for joint families is J
        [InlineData("X20880050010", false)]    // Unknown code
        [InlineData("20880050010", false)]     // No code
        [InlineData("C208800500", false)]      // Too few digits
        [InlineData("C208800500100", false)]   // Too many digits
        [InlineData("C2088005001A", false)]    // Non digit in the numeric part
        [InlineData("hello", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _malaysiaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("C20880050010", true)]
        [InlineData("not-a-number", false)]    // Used to be accepted, the format test was inverted
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _malaysiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("50450", true)]
        [InlineData("88000", true)]
        [InlineData("1234", false)]
        [InlineData("5045A", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _malaysiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
