using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class IndiaValidatorTests
    {
        private readonly IndiaValidator _indiaValidator;

        public IndiaValidatorTests()
        {
            _indiaValidator = new IndiaValidator();
        }

        [Theory]
        [InlineData("234123412346", true)]      // stdnum reference Aadhaar
        [InlineData("234567890124", true)]      // Verhoeff check digit 4
        [InlineData("987654321096", true)]      // Verhoeff check digit 6
        [InlineData("2345 6789 0124", true)]    // Same number as printed on the card
        [InlineData("234567890123", false)]     // Wrong check digit
        [InlineData("123412341234", false)]     // Correct Verhoeff, but Aadhaar never starts with 1
        [InlineData("012345678901", false)]     // Aadhaar never starts with 0
        [InlineData("23456789012", false)]      // Too short
        [InlineData("0", false)]
        [InlineData("ABCDE1234F", false)]       // A PAN pasted into the Aadhaar field
        [InlineData("22AAAAA0000A1Z5", false)]  // A GSTIN pasted into the Aadhaar field
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("ABCPZ1234E", true)]
        [InlineData("ABCDE1234F", false)]   // D is not a card holder type
        [InlineData("ABCPZ1234", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // GST replaced the state VAT and CST regimes on 1 July 2017, so an Indian VAT number issued
        // since then is a GSTIN. Upstream issue #24 asked for the format and it was never added:
        // every GSTIN in circulation was reported as an invalid VAT number.
        // Each value asserted valid here was checked twice, against the base 36 Luhn rule computed
        // by hand and against python-stdnum's in_.gstin.
        [Theory]
        [InlineData("27AAPFU0939F1ZV", true)]      // python-stdnum's own example, check character V
        [InlineData("27 AAPFU 0939 F1ZV", true)]   // Same number with separators
        [InlineData("27aapfu0939f1zv", true)]      // Lower case: folded with the invariant culture
        [InlineData("29AAGCB7383J1Z4", true)]      // Karnataka, company PAN, check character 4
        [InlineData("07AAACB2894G1ZP", true)]      // Delhi, check character P
        [InlineData("09AAACH7409R1ZZ", true)]      // Check character Z, which is also the 14th
        [InlineData("27AAPFU0939F1ZO", false)]     // Wrong check character
        [InlineData("27AAPFU0939F1AA", false)]     // 14th character is not Z
        [InlineData("27AAPFU0939F0ZV", false)]     // Registration number 0 is never issued
        [InlineData("27AAPDU0939F1ZV", false)]     // D is not a PAN holder type
        // Check characters computed with the base 36 Luhn rule, so each row fails on the state
        // code alone rather than on the checksum.
        [InlineData("99AAACH7409R1ZQ", true)]       // 99, Centre Jurisdiction: an OIDAR registration
        [InlineData("97AAACH7409R1ZU", true)]       // 97, Other Territory
        [InlineData("39AAPFU0939F1ZQ", false)]      // State code 39: the list ends at 38 (Ladakh)
        [InlineData("00AAPFU0939F1ZB", false)]      // State code 00
        [InlineData("98AAPFU0939F1ZM", false)]      // 98 is not assigned; 97 and 99 are
        [InlineData("22AAAAA0000A1Z5", false)]     // The documentation placeholder, not a real GSTIN
        [InlineData("27AAPFU0939F1Z", false)]      // Fourteen characters
        [InlineData("369296450896540", false)]     // Fifteen digits, no PAN
        [InlineData("27123456789V", true)]         // The pre-GST TIN stays valid for stored records
        [InlineData("27123456789C", true)]
        [InlineData("27123456789", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("110001", true)]
        [InlineData("560 001", true)]
        [InlineData("11000", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _indiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
