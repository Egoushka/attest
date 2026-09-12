using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class PhilippinesValidatorTests
    {
        private readonly PhilippinesValidator _philippinesValidator;

        public PhilippinesValidatorTests()
        {
            _philippinesValidator = new PhilippinesValidator();
        }

        // BIR TIN: a nine digit core number, optionally followed by a branch code - three digits
        // (000 for individuals and head offices) or the five digits BIR returns now print: "The
        // last 5 digits of the 14-digit TIN refers to the branch code", BIR Form No. 1701-MS
        // guidelines, August 2024. V (VAT registered) or N (non VAT) may follow. The BIR publishes
        // no check digit, so the validator only checks the shape.
        // https://bir-cdn.bir.gov.ph/BIR/pdf/1701-MS%20Guide%20August%202024%20ENCS_Final.pdf
        [Theory]
        [InlineData("123456789000", true)]
        [InlineData("123-456-789-000", true)]
        [InlineData("123456789000V", true)]
        [InlineData("123456789000N", true)]
        [InlineData("123456789", true)]          // the core number, no branch code
        [InlineData("123-456-789", true)]
        [InlineData("123456789V", true)]
        [InlineData("12345678900000", true)]     // 14 digits: five digit branch code
        [InlineData("12345678900", false)]       // 11 digits
        [InlineData("1234567890", false)]        // 10 digits
        [InlineData("1234567890000", false)]     // 13 digits
        [InlineData("123456789000X", false)]     // suffix is neither V nor N
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestEntity(string code, bool isValid)
        {
            Assert.Equal(isValid, _philippinesValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("123456789000", true)]
        [InlineData("123-456-789-000", true)]
        [InlineData("123456789000N", true)]
        [InlineData("123456789", true)]          // the core number, no branch code
        [InlineData("12345678900", false)]       // 11 digits
        [InlineData("1234567890", false)]        // 10 digits
        [InlineData("123456789000X", false)]     // suffix is neither V nor N
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _philippinesValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("123456789000", true)]
        [InlineData("123456789", true)]
        [InlineData("12345678900", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _philippinesValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("123456789000V", true)]
        [InlineData("123-456-789-000V", true)]
        [InlineData("123456789000v", true)]      // lower case suffix
        [InlineData("123456789000", true)]       // the V marker is a form convention, not the number
        [InlineData("123456789", true)]          // the core number, no branch code
        [InlineData("123456789000N", false)]     // non VAT suffix
        [InlineData("123456789N", false)]        // non VAT suffix on the core number
        [InlineData("12345678900V", false)]      // 11 digits
        [InlineData("1234567890", false)]        // 10 digits
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestVat(string code, bool isValid)
        {
            Assert.Equal(isValid, _philippinesValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("1000", true)]               // Manila
        [InlineData("4104", true)]               // Cavite
        [InlineData("100", false)]               // 3 digits
        [InlineData("10000", false)]             // 5 digits
        [InlineData("abcd", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _philippinesValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
