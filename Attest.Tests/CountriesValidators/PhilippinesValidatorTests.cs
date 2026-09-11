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

        // BIR TIN: nine digits plus a three digit branch code (000 for individuals), optionally
        // followed by V (VAT registered) or N (non VAT). The BIR publishes no check digit, so
        // the validator only checks the shape.
        // https://www.bir.gov.ph/taxpayer-identification-number-tin
        [Theory]
        [InlineData("123456789000", true)]
        [InlineData("123-456-789-000", true)]
        [InlineData("123456789000V", true)]
        [InlineData("123456789000N", true)]
        [InlineData("12345678900", false)]       // 11 digits
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
        [InlineData("12345678900", false)]       // 11 digits
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
        [InlineData("123456789000N", false)]     // non VAT suffix
        [InlineData("12345678900V", false)]      // 11 digits
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
