using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MoldovaValidatorTests
    {
        private readonly MoldovaValidator _moldovaValidator;

        public MoldovaValidatorTests()
        {
            _moldovaValidator = new MoldovaValidator();
        }

        // IDNO, thirteen digits closed with a check digit over the weights 7,3,1 repeated, modulus 10.
        // 1008600038413 and 1008600038412 are from
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.md.idno.html
        [Theory]
        [InlineData("1008600038413", true)]
        [InlineData("1003600003904", true)]     // Check digit computed with the same weights
        [InlineData("1008600038412", false)]    // Wrong check digit
        [InlineData("100860003841", false)]     // Twelve digits
        [InlineData("10086000384130", false)]   // Fourteen digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _moldovaValidator.ValidateEntity(code).IsValid);
        }

        // IDNP, thirteen digits shaped 2TTTXXXYYYYYK. K closes the number with the same check digit
        // as the IDNO: the two are one state identifier scheme, separated only by the leading
        // registry digit (1 legal entity, 2 natural person, 3 vehicle).
        // https://ro.wikipedia.org/wiki/Identificator_numeric_personal_(Moldova)
        // https://github.com/iAsig/idnx-validator/blob/main/src/index.ts
        [Theory]
        [InlineData("2003600000391", true)]
        [InlineData("2014001234568", true)]
        [InlineData("200 360 000 039 1", true)] // Same number with separators
        [InlineData("2003600000390", false)]    // Wrong check digit
        [InlineData("2014001234561", false)]    // Wrong check digit
        [InlineData("9999999999999", false)]    // Thirteen digits, but the check digit has to be 6
        [InlineData("200360000039", false)]     // Twelve digits
        [InlineData("20036000003910", false)]   // Fourteen digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _moldovaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("2003600000391", true)]
        [InlineData("2014001234568", true)]
        [InlineData("2003600000390", false)]    // Wrong check digit
        [InlineData("200360000039", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _moldovaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Seven digits, optionally prefixed with MD, for example MD9234564.
        // https://www.vatify.eu/moldova-vat-number.html
        [Theory]
        [InlineData("9234564", true)]
        [InlineData("MD9234564", true)]
        [InlineData("923456", false)]           // Six digits
        [InlineData("92345644", false)]         // Eight digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _moldovaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("MD-2001", true)]           // Chisinau
        [InlineData("MD3100", true)]            // Balti
        [InlineData("MD-20011", false)]         // Five digits
        [InlineData("AB-2001", false)]          // Wrong prefix
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _moldovaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
