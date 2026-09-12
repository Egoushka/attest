using Attest.Countries;
using System;
using Xunit;

namespace Attest.Tests
{
    public class PakistanValidatorTests
    {
        private readonly PakistanValidator _pakistanValidator;

        public PakistanValidatorTests()
        {
            _pakistanValidator = new PakistanValidator();
        }

        // CNIC: 13 digits, the first is the province code 1-7 and the last is the gender digit,
        // which is never 0. There is no check digit.
        // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.pk.cnic
        [Theory]
        [InlineData("34201-0891231-8", true)]    // python-stdnum pk.cnic example
        [InlineData("42201-0397640-8", true)]    // python-stdnum pk.cnic example
        [InlineData("3420108912318", true)]      // same number without separators
        [InlineData("54201-0891231-8", true)]    // province code 5, Balochistan
        [InlineData("74201-0891231-8", true)]    // province code 7, Gilgit-Baltistan
        [InlineData("84201-0891231-8", false)]   // province code 8 does not exist
        [InlineData("04201-0891231-8", false)]   // province code 0 does not exist
        [InlineData("34201-0891231-0", false)]   // gender digit is never 0
        [InlineData("3420A0891231-8", false)]    // a letter survives separator stripping
        [InlineData("34201-089123-8", false)]    // 12 digits
        [InlineData("34201-08912311-8", false)]  // 14 digits
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _pakistanValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("34201-0891231-8", true)]
        [InlineData("84201-0891231-8", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _pakistanValidator.ValidateNationalIdentity(code).IsValid);
        }

        // The FBR publishes no format or check rule for the National Tax Number, so this
        // validator has no rule for entity or VAT codes; CountryValidator turns the throw into
        // Invalid("Not supported"), and CountryValidator.Supports reads it as "no rule".
        [Theory]
        [InlineData("3420108912318")]
        [InlineData(null)]
        public void TestEntityIsNotSupported(string code)
        {
            Assert.Throws<NotSupportedException>(() => _pakistanValidator.ValidateEntity(code));
        }

        [Theory]
        [InlineData("3420108912318")]
        [InlineData(null)]
        public void TestVatIsNotSupported(string code)
        {
            Assert.Throws<NotSupportedException>(() => _pakistanValidator.ValidateVAT(code));
        }

        [Theory]
        [InlineData("44000", true)]              // Islamabad
        [InlineData("74200", true)]              // Karachi
        [InlineData("4400", false)]              // 4 digits
        [InlineData("440000", false)]            // 6 digits
        [InlineData("abcde", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _pakistanValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
