using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class DominicanRepublicValidatorTests
    {
        private readonly DominicanRepublicValidator _dominicanRepublicValidator;

        public DominicanRepublicValidatorTests()
        {
            _dominicanRepublicValidator = new DominicanRepublicValidator();
        }

        // Cedula: 11 digits with a Luhn check digit.
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.do.cedula
        [Theory]
        [InlineData("00113918205", true)]      // stdnum reference number
        [InlineData("22400022111", true)]      // stdnum reference number
        [InlineData("224-0002211-1", true)]    // same number as printed on the card
        [InlineData("00000021249", true)]      // whitelisted, does not satisfy Luhn
        [InlineData("00113918204", false)]     // wrong check digit
        [InlineData("22400022112", false)]     // wrong check digit
        [InlineData("0011391820A", false)]     // not all digits
        [InlineData("001139182", false)]       // too short
        [InlineData("001139182050", false)]    // too long
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("garbage", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _dominicanRepublicValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("00113918205", true)]
        [InlineData("22400022111", true)]
        [InlineData("224-0002211-1", true)]
        [InlineData("00000021249", true)]      // whitelisted, does not satisfy Luhn
        [InlineData("00113918204", false)]     // wrong check digit
        [InlineData("22400022112", false)]     // wrong check digit
        [InlineData("0011391820A", false)]     // not all digits
        [InlineData("001139182", false)]       // too short
        [InlineData("001139182050", false)]    // too long
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("garbage", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _dominicanRepublicValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("101850043", true)]
        [InlineData("131246796", true)]
        [InlineData("101850042", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _dominicanRepublicValidator.ValidateEntity(code).IsValid);
        }

        // RNC: 9 digits, check digit from weights 7 9 8 6 5 4 3 2 mod 11.
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.do.rnc
        [Theory]
        [InlineData("101850043", true)]        // stdnum reference number
        [InlineData("131246796", true)]        // stdnum reference number
        [InlineData("1-01-85004-3", true)]     // same number as printed
        [InlineData("131123457", true)]        // check digit computed from the published weights
        [InlineData("401122332", true)]        // check digit computed from the published weights
        [InlineData("401007374", true)]        // whitelisted, does not satisfy the checksum
        [InlineData("101850042", false)]       // wrong check digit
        [InlineData("131246795", false)]       // wrong check digit
        [InlineData("1018A0043", false)]       // not all digits
        [InlineData("10185004", false)]        // too short
        [InlineData("1018500430", false)]      // too long
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("garbage", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _dominicanRepublicValidator.ValidateVAT(code).IsValid);
        }

        // NCF: 13 chars "E" + e-CF type, 11 chars "B" + NCF type, 19 chars "A"/"P" with the
        // NCF type at offset 9. https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.do.ncf
        [Theory]
        [InlineData("E310000000005", true)]           // e-CF format, in use since 2019-04-08
        [InlineData("E320000000005", true)]           // final consumer
        [InlineData("E460000000005", true)]           // export
        [InlineData("E470000000005", true)]           // payments abroad
        [InlineData("E350000000005", false)]          // 35 is not an e-CF document type
        [InlineData("E990000000005", false)]          // 99 is not an e-CF document type
        [InlineData("Z310000000005", false)]          // 13 chars must start with E
        [InlineData("E31000000000A", false)]          // not all digits after the prefix
        [InlineData("B0100000005", true)]             // NCF format, in use since 2018-05-01
        [InlineData("B0200000005", true)]             // final consumer
        [InlineData("B1700000005", true)]             // payments abroad
        [InlineData("B0000000005", false)]            // 00 is not an NCF document type
        [InlineData("B0500000005", false)]            // 05 is not an NCF document type
        [InlineData("B9900000005", false)]            // 99 is not an NCF document type
        [InlineData("Z0100000005", false)]            // 11 chars must start with B
        [InlineData("B010000000A", false)]            // not all digits after the prefix
        [InlineData("A020010210100000005", true)]     // format used before 2018-05-01
        [InlineData("P020010210100000005", true)]     // P series
        [InlineData("A020010219900000005", false)]    // 99 is not an NCF document type
        [InlineData("Z020010210100000005", false)]    // 19 chars must start with A or P
        [InlineData("B01000000051", false)]           // no NCF is 12 characters long
        // stdnum's compact() upper-cases the number, so the series letter is not case-sensitive.
        [InlineData("e310000000005", true)]           // lowercase e-CF series letter
        [InlineData("b0100000005", true)]             // lowercase B series letter
        [InlineData("a020010210100000005", true)]     // lowercase A series letter
        [InlineData("p020010210100000005", true)]     // lowercase P series letter
        [InlineData("z0100000005", false)]            // Z is not a series letter in either case
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("not a receipt", false)]
        [InlineData(null, false)]
        public void TestNCF(string code, bool isValid)
        {
            Assert.Equal(isValid, _dominicanRepublicValidator.ValidateNCF(code).IsValid);
        }

        [Theory]
        [InlineData("10101", true)]     // Santo Domingo, Distrito Nacional
        [InlineData("51000", true)]     // Santiago de los Caballeros
        [InlineData("1010", false)]     // too short
        [InlineData("101011", false)]   // too long
        [InlineData("abcde", false)]    // not digits
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _dominicanRepublicValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
