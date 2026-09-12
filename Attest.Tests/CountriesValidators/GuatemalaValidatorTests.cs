using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class GuatemalaValidatorTests
    {
        private readonly GuatemalaValidator _guatemalaValidator;

        public GuatemalaValidatorTests()
        {
            _guatemalaValidator = new GuatemalaValidator();
        }

        // NIT: two to twelve characters, all digits except the last, which is the check digit and
        // may be the letter K. The check digit is -sum(position weights 2,3,4,... from the right)
        // modulo 11, with 10 written as K. The number is upper cased and leading zeroes are
        // stripped before it is checked; leading zeroes carry a weight but contribute nothing.
        // 576937-K, 7108-0, 8977112-0 and 39525503 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.gt.nit.html
        [Theory]
        [InlineData("576937-K", true)]
        [InlineData("576937-k", true)]           // Lower case check digit
        [InlineData("00576937K", true)]          // Leading zeroes are stripped
        [InlineData("7108-0", true)]
        [InlineData("39525503", true)]
        [InlineData("12345679", true)]           // Check digit computed with the same weights
        [InlineData("8977112-0", false)]         // Wrong check digit, should be 5
        [InlineData("0K", false)]                // One character once the leading zero is stripped
        [InlineData("12345678", false)]          // Wrong check digit, should be 9
        [InlineData("1234567890123", false)]     // Thirteen characters
        [InlineData("5", false)]                 // One character
        [InlineData("57693AK", false)]           // Letter outside the check digit position
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _guatemalaValidator.ValidateEntity(code).IsValid);
        }

        // The VAT identifier is the NIT.
        [Theory]
        [InlineData("576937-K", true)]
        [InlineData("39525503", true)]
        [InlineData("8977112-0", false)]         // Wrong check digit
        [InlineData("12345678", false)]          // Wrong check digit
        [InlineData("1234567890123", false)]     // Thirteen characters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _guatemalaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("576937", "K")]
        [InlineData("7108", "0")]
        [InlineData("3952550", "3")]
        [InlineData("8977112", "5")]
        public void TestChecksum(string code, string checkDigit)
        {
            Assert.Equal(checkDigit, _guatemalaValidator.CalculateChecksum(code));
        }

        // Guatemala has no personal tax code separate from the NIT, so the individual code is
        // reported unsupported rather than throwing.
        [Theory]
        [InlineData("576937-K")]
        [InlineData("39525503")]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        public void TestIndividualCodeIsNotSupported(string code)
        {
            Assert.False(_guatemalaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // ValidateNationalIdentity delegates to the individual code, so it reports the same result
        // instead of propagating an exception.
        [Theory]
        [InlineData("576937-K")]
        [InlineData("39525503")]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        public void TestNationalIdIsNotSupported(string code)
        {
            Assert.False(_guatemalaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("01001", true)]
        [InlineData("09001", true)]
        [InlineData("0100", false)]     // Four digits
        [InlineData("010010", false)]   // Six digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _guatemalaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
