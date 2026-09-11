using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class SwitzerlandValidatorTests
    {
        private readonly SwitzerlandValidator _switzerlandValidator;

        public SwitzerlandValidatorTests()
        {
            _switzerlandValidator = new SwitzerlandValidator();
        }

        [Theory]
        [InlineData("7569217076985", true)]
        [InlineData("756.9217.0769.85", true)]
        [InlineData("756.9217.0769.84", false)]
        [InlineData("756.9217.0769.8", false)] // Digit short, used to throw IndexOutOfRangeException
        [InlineData("CHE123456789", false)]    // Letters, used to throw FormatException
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _switzerlandValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("7569217076985", true)]
        [InlineData("756.9217.0769.85", true)]
        [InlineData("756.9217.0769.84", false)]
        [InlineData("7561234567897", true)]        // EAN-13 check digit 7
        [InlineData("756.1234.5678.97", true)]     // Same number as printed on the AHV card
        [InlineData("7564890112340", true)]        // EAN-13 check digit 0
        [InlineData("7561234567891", false)]       // Wrong check digit
        [InlineData("756.9217.0769.8", false)]     // 12 digits, used to throw IndexOutOfRangeException
        [InlineData("7569217076985123456", false)] // Too long, used to be accepted
        [InlineData("0000000000000", false)]       // 13 digits but not the 756 prefix
        [InlineData("CHE123456789", false)]        // Letters, used to throw FormatException
        [InlineData("abc", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _switzerlandValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("CHE-100.155.212", true)]
        [InlineData("CHE100155212", true)]
        [InlineData("CHE-100.155.213", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _switzerlandValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("CHE-107.787.577 IVA", true)]
        [InlineData("CHE107787577IVA", true)]
        [InlineData("CHE-107.787.578 IVA", false)]
        [InlineData("che-107.787.577 iva", true)] // Same number in lower case
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _switzerlandValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("2544", true)]
        [InlineData("1211", true)]
        [InlineData("321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _switzerlandValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
