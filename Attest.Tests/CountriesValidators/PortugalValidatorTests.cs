using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class PortugalValidatorTests
    {
        private readonly PortugalValidator _portugalValidator;

        public PortugalValidatorTests()
        {
            _portugalValidator = new PortugalValidator();
        }

        [Theory]
        [InlineData("900000007", true)]
        [InlineData("900000006", false)]
        [InlineData("000000000ZZ4", true)]
        [InlineData("000000000ZZ3", false)]
        // A Cartao de Cidadao is nine digits, a two character document version and a check digit,
        // and python-stdnum upper cases the input before matching ^\d*[A-Z0-9]{2}\d$ against it.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pt/cc.py
        [InlineData("000000000zz4", true)]   // Same card number in lowercase
        [InlineData("00000000000A", false)]  // The check digit must be a digit, not a letter
        [InlineData("00000000AZZ4", false)]  // The civil number part must be nine digits
        [InlineData("100000003", false)]  // Bilhete de Identidade with a wrong check digit
        [InlineData("12345678", false)]   // Neither 9 nor 12 characters
        [InlineData("abcdefghi", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _portugalValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Check digits computed from the NIF rule: the first eight digits are weighted
        // 9..2, summed, and the check digit is 11 - (sum % 11), or 0 when that exceeds 9.
        [Theory]
        [InlineData("100000002", true)]     // Natural person, range 1
        [InlineData("200000004", true)]     // Natural person, range 2
        [InlineData("300000006", true)]     // Natural person, range 3 (issued since June 2019)
        [InlineData("450000001", true)]     // Natural person, non-resident citizen (range 45)
        [InlineData("800000005", true)]     // Empresario em nome individual
        [InlineData("pt 100000002", true)]  // Country prefix and separators are ignored
        [InlineData("100000003", false)]    // Wrong check digit
        [InlineData("501964843", false)]    // Company
        [InlineData("600000001", false)]    // Public administration body
        [InlineData("700000003", false)]    // Heranca indivisa / fund
        [InlineData("900000007", false)]    // Irregular collective body
        [InlineData("400000008", false)]    // 4 is only issued as the 45 range
        [InlineData("000000000", false)]    // The 0 range is never issued
        [InlineData("12345678", false)]     // Too short
        [InlineData("1234567890", false)]   // Too long
        [InlineData("abcdefghi", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _portugalValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("501964843", true)]     // Pessoa colectiva registered with the RNPC
        [InlineData("512345678", true)]     // Pessoa colectiva
        [InlineData("600000001", true)]     // Public administration body
        [InlineData("700000003", true)]     // Heranca indivisa / fund / non-resident collective
        [InlineData("900000007", true)]     // Irregular collective body
        [InlineData("990000001", true)]     // Sociedade civil sem personalidade juridica
        [InlineData("PT501964843", true)]   // Country prefix is ignored
        [InlineData("401964843", false)]    // 4 is only issued as the 45 range
        [InlineData("900000006", false)]    // Wrong check digit
        [InlineData("501964842", false)]    // Wrong check digit
        [InlineData("100000002", false)]    // Natural person
        [InlineData("200000004", false)]    // Natural person
        [InlineData("300000006", false)]    // Natural person
        [InlineData("450000001", false)]    // Natural person, non-resident citizen
        [InlineData("800000005", false)]    // Empresario em nome individual
        [InlineData("000000000", false)]    // The 0 range is never issued
        [InlineData("50196484", false)]     // Too short
        [InlineData("5019648430", false)]   // Too long
        [InlineData("abcdefghi", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _portugalValidator.ValidateEntity(code).IsValid);
        }

        // The Portuguese VAT number is the taxpayer's own NIF, so both individual and
        // collective ranges are accepted here.
        [Theory]
        [InlineData("501964843", true)]
        [InlineData("PT501964843", true)]
        [InlineData("100000002", true)]     // Natural person registered for IVA
        [InlineData("800000005", true)]     // Empresario em nome individual
        [InlineData("900000007", true)]
        [InlineData("501964842", false)]    // Wrong check digit
        [InlineData("100000003", false)]    // Wrong check digit
        [InlineData("400000008", false)]    // 4 is only issued as the 45 range
        [InlineData("000000000", false)]    // The 0 range is never issued
        [InlineData("50196484", false)]     // Too short
        [InlineData("abcdefghi", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _portugalValidator.ValidateVAT(code).IsValid);
        }

        // The first digit is one of the nine postal regions, 1 Lisboa through 9 Madeira e Acores,
        // so there is no 0 range.
        // https://en.wikipedia.org/wiki/Postal_codes_in_Portugal
        [Theory]
        [InlineData("2725-079", true)]
        [InlineData("1208-148", true)]
        [InlineData("1000-205", true)]      // Region 1, Lisboa
        [InlineData("9000-100", true)]      // Region 9, Madeira e Acores
        [InlineData("0000000", false)]      // The 0 range is not a postal region
        [InlineData("2312321q", false)]
        [InlineData("1208148", true)]       // Separator is optional
        [InlineData("1208-14", false)]      // Too short
        [InlineData("1208-1489", false)]    // Too long
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _portugalValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
