using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class BulgariaValidatorTests
    {
        private readonly BulgariaValidator _bulgariaValidator;

        public BulgariaValidatorTests()
        {
            _bulgariaValidator = new BulgariaValidator();
        }

        [Theory]
        [InlineData("7523169263", true)]
        [InlineData("8032056031", true)]
        [InlineData("803205 603 1", true)]
        [InlineData("8001010008", true)]
        [InlineData("8019010008", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _bulgariaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("7523169263", true)]
        [InlineData("8032056031", true)]
        [InlineData("803205 603 1", true)]
        [InlineData("8001010008", true)]
        [InlineData("8019010008", true)]      // Month 19 is not a valid EGN date, but the 10 digit BULSTAT checksum accepts it
        [InlineData("5260181599", true)]      // Personal number of a foreigner (PNF), weights 21,19,17,13,11,9,7,3,1
        [InlineData("5260181598", false)]     // Same PNF with a wrong check digit
        [InlineData("123456789", false)]      // Nine digits, used to throw IndexOutOfRangeException
        [InlineData("12345", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bulgariaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("8001010008", true)]
        [InlineData("175074752", true)]       // Nine digit legal entity code
        [InlineData("175074751", false)]      // Wrong check digit
        [InlineData("5260181599", true)]      // Ten digit PNF
        [InlineData("12345678", false)]       // Eight digits, used to throw IndexOutOfRangeException
        [InlineData("123", false)]
        [InlineData("", false)]
        [InlineData(null, false)]             // Used to throw NullReferenceException
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bulgariaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("BG 175 074 752", true)]
        [InlineData("175074752", true)]
        [InlineData("175074751", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bulgariaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData(" 1000", true)]
        [InlineData("1700", true)]
        [InlineData("17001", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bulgariaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
