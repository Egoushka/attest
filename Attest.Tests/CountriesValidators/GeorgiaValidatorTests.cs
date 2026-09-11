using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class GeorgiaValidatorTests
    {
        private readonly GeorgiaValidator _georgiaValidator;

        public GeorgiaValidatorTests()
        {
            _georgiaValidator = new GeorgiaValidator();
        }

        // A natural person carries an eleven digit personal number, an individual entrepreneur or
        // a foreign individual a nine digit identification code. No check digit algorithm is
        // published for either, so only the format is validated.
        // https://lookuptax.com/docs/tax-identification-number/georgia-tax-id-guide
        [Theory]
        [InlineData("01024001234", true)]       // Eleven digits, personal number
        [InlineData("62001012345", true)]
        [InlineData("204854595", true)]         // Nine digits, identification code
        [InlineData("010 240 012 34", true)]    // Same number with separators
        [InlineData("0102400123", false)]       // Ten digits, neither form
        [InlineData("010240012345", false)]     // Twelve digits
        [InlineData("20485459", false)]         // Eight digits
        [InlineData("0102400123x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _georgiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("01024001234", true)]       // The personal number of a natural person
        [InlineData("62001012345", true)]
        [InlineData("0102400123", false)]       // Ten digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _georgiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // The identification code of a legal entity is nine digits, allocated by the Public
        // Registry; the first two digits encode the legal form.
        [Theory]
        [InlineData("204854595", true)]
        [InlineData("400054952", true)]
        [InlineData("204 854 595", true)]       // Same number with separators
        [InlineData("20485459", false)]         // Eight digits
        [InlineData("2048545951", false)]       // Ten digits
        [InlineData("01024001234", false)]      // The eleven digit personal number is not an entity code
        [InlineData("20485459x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _georgiaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("204854595", true)]
        [InlineData("400054952", true)]
        [InlineData("20485459", false)]         // Eight digits
        [InlineData("01024001234", false)]      // Eleven digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _georgiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("0105", true)]              // Tbilisi
        [InlineData("6000", true)]              // Batumi
        [InlineData("0105 ", true)]
        [InlineData("010", false)]              // Three digits
        [InlineData("01050", false)]            // Five digits
        [InlineData("A105", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _georgiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
