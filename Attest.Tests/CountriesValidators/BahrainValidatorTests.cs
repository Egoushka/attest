using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class BahrainValidatorTests
    {
        private readonly BahrainValidator _bahrainValidator;

        public BahrainValidatorTests()
        {
            _bahrainValidator = new BahrainValidator();
        }

        // CPR (Personal Number), nine digits shaped YYMMNNNNC: year and month of birth, a serial
        // and a check digit. The check digit algorithm is not published by the iGA, so the
        // validator - and therefore this test - only covers the format.
        // https://en.wikipedia.org/wiki/National_identification_number#Bahrain
        [Theory]
        [InlineData("900412345", true)]
        [InlineData("021031234", true)]
        [InlineData("900-412-345", true)]    // Separators are stripped before matching
        [InlineData("12345678", false)]      // Eight digits
        [InlineData("1234567890", false)]    // Ten digits
        [InlineData("90041234A", false)]     // Letter in the check position
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bahrainValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // The CPR doubles as the national identifier, so the base class delegation applies.
        [Theory]
        [InlineData("900412345", true)]
        [InlineData("021031234", true)]
        [InlineData("12345678", false)]      // Eight digits
        [InlineData("90041234A", false)]     // Letter in the check position
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _bahrainValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Three or four digit block numbers, per the UPU addressing template for Bahrain.
        // https://www.upu.int/UPU/media/upu/PostalEntitiesFiles/addressingUnit/bhrEn.pdf
        [Theory]
        [InlineData("317", true)]
        [InlineData("1216", true)]
        [InlineData(" 428 ", true)]          // Whitespace is stripped before matching
        [InlineData("12", false)]            // Two digits
        [InlineData("12345", false)]         // Five digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bahrainValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
