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
        [InlineData("1234567_", true)]  // "_" is punctuation, stripped like any separator before the check
        // Articulo 40 says "caracteres alfanumericos", and SEGIP issues two character complements.
        // This row asserted the opposite, which rejected every cedula carrying one.
        [InlineData("4567890AB", true)] // Two complemento characters, as SEGIP issues
        [InlineData("1234567-1A", true)] // The printed form, hyphen stripped before the check
        [InlineData("4567890ABC", false)] // Three, which no source describes
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
        // code is seven to thirteen digits. The SIN builds a natural person's NIT from the Cedula
        // de Identidad plus a three digit suffix, so the length is not fixed; the bounds here are
        // the widest any published source describes, and no source puts a NIT outside them.
        // https://siatinfo.impuestos.gob.bo/index.php/requisitos-para-la-inscripcion/conceptos-generales/generacion-del-nit
        [Theory]
        [InlineData("1023456789", true)]
        [InlineData("4567890123", true)]
        [InlineData("1234567", true)]       // Seven digits, the shortest length any source reports
        [InlineData("123456789", true)]     // Nine digit legacy NIT
        [InlineData("123456", false)]       // Six digits, below every published length
        [InlineData("12345678901234", false)]   // Fourteen digits, above every published length
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
