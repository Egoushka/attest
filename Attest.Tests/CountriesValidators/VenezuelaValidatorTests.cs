using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class VenezuelaValidatorTests
    {
        private readonly VenezuelaValidator _venezuelaValidator;

        public VenezuelaValidatorTests()
        {
            _venezuelaValidator = new VenezuelaValidator();
        }

        // RIF: a type letter (V, E, J, P or G) followed by nine digits. The check digit closes
        // 11 - (type value + sum over weights 3,2,7,6,5,4,3,2) mod 11, with 10 and 11 written as 0.
        // V-11470283-4 and V-11470283-3 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.ve.rif.html
        [Theory]
        [InlineData("V-11470283-4", true)]
        [InlineData("V114702834", true)]
        [InlineData("J305991685", true)]        // Company, check digit computed with the same weights
        [InlineData("E812345675", true)]        // Foreign natural person
        [InlineData("G200001062", true)]        // Government
        [InlineData("VEV114702834", true)]      // Country prefix is stripped
        [InlineData("V-11470283-3", false)]     // Wrong check digit
        [InlineData("J305991686", false)]       // Wrong check digit
        [InlineData("X114702834", false)]       // X is not a RIF type
        [InlineData("V11470283", false)]        // Nine characters
        [InlineData("V1147028345", false)]      // Eleven characters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _venezuelaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("J305991685", true)]
        [InlineData("G200001062", true)]
        [InlineData("J305991686", false)]       // Wrong check digit
        [InlineData("X114702834", false)]       // X is not a RIF type
        [InlineData("V11470283", false)]        // Nine characters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _venezuelaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("V114702834", true)]
        [InlineData("E812345675", true)]
        [InlineData("V-11470283-3", false)]     // Wrong check digit
        [InlineData("V11470283", false)]        // Nine characters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _venezuelaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("V114702834", true)]
        [InlineData("V-11470283-3", false)]     // Wrong check digit
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _venezuelaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Four digits, optionally followed by the letter that marks a sub area, as in 1010-A.
        [Theory]
        [InlineData("1010", true)]
        [InlineData("1010-A", true)]
        [InlineData("1010 A", true)]
        [InlineData("101", false)]      // Three digits
        [InlineData("10101", false)]    // Five digits
        [InlineData("1010AB", false)]   // Two trailing letters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _venezuelaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
