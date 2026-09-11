using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class BoliviaValidatorTests
    {
        private readonly BoliviaValidator _boliviaValidator;

        public BoliviaValidatorTests()
        {
            _boliviaValidator = new BoliviaValidator();
        }

        // Cedula de Identidad: five to eight digits, optionally followed by the single character
        // "complemento" that SEGIP adds when two people share a number. There is no check digit,
        // so only the format is validated.
        // https://www.segip.gob.bo/ (cedula de identidad, complemento alfanumerico)
        [Theory]
        [InlineData("1234567", true)]
        [InlineData("12345678", true)]
        [InlineData("4567890A", true)]  // Seven digits plus a complemento letter
        [InlineData("1234", false)]     // Four digits, below the minimum of five
        [InlineData("abcdefg", false)]  // Letters only
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _boliviaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // NIT. Bolivia publishes no check digit for the NIT, so the validator only checks that the
        // code is digits. Only cases that are wrong under every published description of the NIT
        // are asserted here; see the audit notes for the disputed length bounds.
        [Theory]
        [InlineData("1023456789", true)]
        [InlineData("4567890123", true)]
        [InlineData("1", false)]            // A single digit is far below any published NIT length
        [InlineData("12345678ab", false)]   // Letters are not allowed
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _boliviaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("1023456789", true)]
        [InlineData("4567890123", true)]
        [InlineData("1", false)]
        [InlineData("12345678ab", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _boliviaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("1023456789", true)]
        [InlineData("4567890123", true)]
        [InlineData("1", false)]
        [InlineData("12345678ab", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _boliviaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("0201", true)]
        [InlineData("1234", true)]
        [InlineData("123", false)]      // Three digits
        [InlineData("abcd", false)]     // Letters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _boliviaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
