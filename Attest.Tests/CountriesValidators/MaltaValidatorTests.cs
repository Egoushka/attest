using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MaltaValidatorTests
    {
        private readonly MaltaValidator _maltaValidator;

        public MaltaValidatorTests()
        {
            _maltaValidator = new MaltaValidator();
        }

        [Theory]
        [InlineData("1234567M", true)]
        [InlineData("1234567G", true)]
        [InlineData("12345678", false)]      // 8 digits is not a Maltese TIN, the numeric form is 9 digits
        [InlineData("1893120105733", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _maltaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("1234567M", true)]
        [InlineData("1234567Z", true)]       // Gozitan, birth registered 1800-1899
        [InlineData("1234567H", true)]       // Gozitan, birth registered after 2000
        [InlineData("0000123G", true)]
        [InlineData("123M", true)]           // Leading zeros may be omitted in print
        [InlineData("123456789", true)]      // Taxpayer reference number of a non Maltese national
        [InlineData("12345678", false)]      // 8 digits, neither the id card nor the 9 digit form
        [InlineData("1234567X", false)]      // X is not one of M, G, A, P, L, H, B, Z
        [InlineData("1234567", false)]
        [InlineData("12M", false)]
        [InlineData("1893120105733", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _maltaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("123456789", true)]
        [InlineData("12345678", false)]
        [InlineData("1234567890", false)]
        [InlineData("1234567M", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _maltaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("11679112", true)]
        [InlineData("MT 1167-9112", true)]
        [InlineData("mt11679112", true)]
        [InlineData("Mt 1167-9112", true)]   // Prefix stripped regardless of case
        [InlineData("12345634", true)]       // 3+8+18+28+40+54 = 151, 37 - 151 % 37 = 34
        // 3+0+0+0+8+63 = 74, an exact multiple of 37, so the check value is 37 rather than 00.
        // https://vat-validator.readthedocs.io/en/latest/_modules/vat_validator/countries.html
        [InlineData("10001737", true)]
        [InlineData("11679113", false)]
        [InlineData("1167MT9112", false)]    // MT is only a prefix, not a separator
        [InlineData("01679112", false)]      // Must not start with zero
        [InlineData("1167911", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _maltaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("FRN 1913", true)]       // Malta Communications Authority, Floriana
        [InlineData("VLT 1117", true)]       // Valletta
        [InlineData("SLM 3010", true)]       // Sliema
        [InlineData("frn 1913", true)]       // Lowercase input is normalised
        [InlineData("slm3010", true)]
        [InlineData("VLT 05", false)]        // Pre 2007 format, withdrawn
        [InlineData("NXR 01", false)]
        [InlineData("VLT 117", false)]
        [InlineData("VCT 17531", false)]
        [InlineData("1913 FRN", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _maltaValidator.ValidatePostalCode(code).IsValid);
        }

    }
}
