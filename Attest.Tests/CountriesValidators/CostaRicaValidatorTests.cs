using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class CostaRicaValidatorTests
    {
        private readonly CostaRicaValidator _costaRicaValidator;

        public CostaRicaValidatorTests()
        {
            _costaRicaValidator = new CostaRicaValidator();
        }

        // CPJ (Cedula de Persona Juridica): ten digits, the first being the class (2-5) and the
        // next three the type of juridical person, which is constrained per class. No check digit.
        // 3-101-999999, 3-534-123559 and 4 000 042138 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.cr.cpj.html
        [Theory]
        [InlineData("3-101-999999", true)]
        [InlineData("4 000 042138", true)]
        [InlineData("2100123456", true)]        // Class 2, type 100
        [InlineData("5001123456", true)]        // Class 5, type 001
        [InlineData("CR3101999999", true)]      // Country prefix is stripped
        [InlineData("3-534-123559", false)]     // Class 3 with a type outside the published list
        [InlineData("6101999999", false)]       // Class 6 does not exist
        [InlineData("4001042138", false)]       // Class 4 only allows type 000
        [InlineData("310132541", false)]        // Nine digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _costaRicaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("3-101-999999", true)]
        [InlineData("4 000 042138", true)]
        [InlineData("3-534-123559", false)]
        [InlineData("6101999999", false)]
        [InlineData("310132541", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _costaRicaValidator.ValidateVAT(code).IsValid);
        }

        // CPF (Cedula de Persona Fisica): 0P-TTTT-AAAA, ten digits with a leading zero that is
        // usually omitted, leaving nine. No check digit.
        // 3-0455-0175, 30-1234-1234 and 701610395 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.cr.cpf.html
        [Theory]
        [InlineData("3-0455-0175", true)]
        [InlineData("0304550175", true)]        // Same number with the leading zero written out
        [InlineData("701610395", true)]
        [InlineData("30-1234-1234", false)]     // Ten digits not starting with zero
        [InlineData("12345678", false)]         // Eight digits
        [InlineData("030455017a", false)]       // Letters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCpf(string code, bool isValid)
        {
            Assert.Equal(isValid, _costaRicaValidator.ValidateCPF(code).IsValid);
        }

        // CR / DIMEX (Cedula de Residencia): eleven or twelve digits starting with 1.
        // 155812994816, 30123456789 and 122200569906 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.cr.cr.html
        [Theory]
        [InlineData("155812994816", true)]
        [InlineData("122200569906", true)]
        [InlineData("15581299481", true)]       // Eleven digit form
        [InlineData("30123456789", false)]      // Does not start with 1
        [InlineData("12345678", false)]         // Eight digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestResident(string code, bool isValid)
        {
            Assert.Equal(isValid, _costaRicaValidator.ValidateResident(code).IsValid);
        }

        // Accepts either a CPF or a Cedula de Residencia.
        [Theory]
        [InlineData("3-0455-0175", true)]
        [InlineData("701610395", true)]
        [InlineData("155812994816", true)]
        [InlineData("30-1234-1234", false)]     // Neither a CPF (bad leading digit) nor a CR (too short)
        [InlineData("30123456789", false)]      // Eleven digits not starting with 1
        [InlineData("12345678", false)]         // Eight digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _costaRicaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("3-0455-0175", true)]
        [InlineData("155812994816", true)]
        [InlineData("12345678", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _costaRicaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("10101", true)]
        [InlineData("1000", true)]
        [InlineData("123", false)]      // Three digits
        [InlineData("123456", false)]   // Six digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _costaRicaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
