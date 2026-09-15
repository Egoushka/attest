using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class IndonesiaValidatorTests
    {
        private readonly IndonesiaValidator _indonesiaValidator;

        public IndonesiaValidatorTests()
        {
            _indonesiaValidator = new IndonesiaValidator();
        }

        // NPWP: 15 digits, the 9th digit is a Luhn check digit over the first 8. The second
        // digit is the taxpayer type: 0-3 for organisations, 4-9 for individuals. Since 2024 the
        // number is 16 digits: the same number with a leading 0, which moves the check digit to
        // the 10th position, or - for a citizen - the NIK.
        // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.id.npwp
        [Theory]
        [InlineData("013121660091000", true)]    // python-stdnum id.npwp example
        [InlineData("01.312.166.0-091.000", true)]
        [InlineData("016090524017000", true)]    // python-stdnum id.npwp example
        [InlineData("013000666091000", true)]    // python-stdnum id.npwp example
        [InlineData("013121660091", true)]       // 12 digits, the branch code defaults to 000
        [InlineData("0013121660091000", true)]   // 2024 form of the same number
        [InlineData("0016090524017000", true)]   // 2024 form of the same number
        [InlineData("013121661091000", false)]   // wrong check digit
        [InlineData("0013121661091000", false)]  // 2024 form, wrong check digit
        [InlineData("053121661091000", false)]   // taxpayer type 5 is an individual
        [InlineData("0053121661091000", false)]  // 2024 form, taxpayer type 5 is an individual
        [InlineData("3174012501900001", false)]  // a NIK identifies a person, not an organisation
        [InlineData("01312166009100", false)]    // 14 digits
        [InlineData("0131216600910001", false)]  // 16 digits, but the check digit is the 10th
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestEntity(string code, bool isValid)
        {
            Assert.Equal(isValid, _indonesiaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("053121661091000", true)]    // check digit 1 computed from the published rule
        [InlineData("041234568091000", true)]    // check digit 8 computed from the published rule
        [InlineData("079998886091000", true)]    // check digit 6 computed from the published rule
        [InlineData("0053121661091000", true)]   // 2024 form of the same number
        [InlineData("3174012501900001", true)]   // NIK, born 25-01-1990
        [InlineData("3174016501900001", true)]   // NIK of a woman: 40 is added to the day
        [InlineData("053121660091000", false)]   // wrong check digit
        [InlineData("0053121660091000", false)]  // 2024 form, wrong check digit
        [InlineData("013121660091000", false)]   // taxpayer type 1 is an organisation
        [InlineData("0013121660091000", false)]  // 2024 form, taxpayer type 1 is an organisation
        [InlineData("3174013201900001", false)]  // NIK with day 32
        [InlineData("3174012513900001", false)]  // NIK with month 13
        [InlineData("05312166109100", false)]    // 14 digits
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indonesiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("053121661091000", true)]
        [InlineData("3174012501900001", true)]
        [InlineData("053121660091000", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _indonesiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("013121660091000", true)]
        [InlineData("016090524017000", true)]
        [InlineData("0013121660091000", true)]   // 2024 form of the same number
        [InlineData("013121661091000", false)]   // wrong check digit
        // A PKP -- a taxpayer registered to collect VAT -- can be a sole proprietor, and since
        // 2024 an Indonesian individual's NPWP is their NIK. This row asserted the opposite,
        // which is what made every VAT registered sole proprietor invalid.
        [InlineData("3174012501900001", true)]   // A person's NIK, and a person can be a PKP
        [InlineData("046090528017000", true)]    // Taxpayer type 4, an individual entrepreneur
        [InlineData("046090524017000", false)]   // Same number, check digit of the type 1 form
        [InlineData("01312166009100", false)]    // 14 digits
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestVat(string code, bool isValid)
        {
            Assert.Equal(isValid, _indonesiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("10110", true)]              // Gambir, Central Jakarta
        [InlineData("40115", true)]              // Bandung
        [InlineData("1011", false)]              // 4 digits
        [InlineData("101101", false)]            // 6 digits
        [InlineData("abcde", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indonesiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
