using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class CubaValidatorTests
    {
        private readonly CubaValidator _cubaValidator;

        public CubaValidatorTests()
        {
            _cubaValidator = new CubaValidator();
        }

        // NI (Numero de identidad): eleven digits, the first six being the date of birth as YYMMDD
        // and the seventh selecting the century (9 => 18xx, 0-5 => 19xx, otherwise 20xx).
        // 91021027775, 72062506561, 85020291531, 9102102777A and 02023061531 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.cu.ni.html
        [Theory]
        [InlineData("91021027775", true)]       // 1991-02-10
        [InlineData("72062506561", true)]       // 1972-06-25
        [InlineData("85020291531", true)]       // 1885-02-02, century digit 9
        [InlineData("00022965311", true)]       // 2000-02-29, a leap year under century digit 6
        [InlineData("02023061531", false)]      // February 30th does not exist
        [InlineData("91001027775", false)]      // Month 00
        [InlineData("9102102777A", false)]      // Letter in the number
        [InlineData("9102102777", false)]       // Ten digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _cubaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("91021027775", true)]
        [InlineData("72062506561", true)]
        [InlineData("02023061531", false)]      // February 30th does not exist
        [InlineData("9102102777", false)]       // Ten digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _cubaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("10400", true)]
        [InlineData("53100", true)]
        [InlineData("1040", false)]     // Four digits
        [InlineData("104000", false)]   // Six digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _cubaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
