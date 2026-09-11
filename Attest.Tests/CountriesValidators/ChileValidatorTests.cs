using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ChileValidatorTests
    {
        private readonly ChileValidator _chileValidator;

        public ChileValidatorTests()
        {
            _chileValidator = new ChileValidator();
        }

        [Theory]
        // The RUN on the identity card is the same number as the RUT.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/cl/rut.py
        [InlineData("76086428-5", true)]
        [InlineData("12531909-2", true)]
        [InlineData("12531909-3", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _chileValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        // python-stdnum doctests.
        [InlineData("76086428-5", true)]
        [InlineData("CL 12531909-2", true)]
        [InlineData("cl 12531909-2", true)]
        [InlineData("125319092", true)]
        // Check digit computed by hand: weights 2,3,4,5,6,7 cycling from the right, 11 - (sum mod 11).
        [InlineData("12345674", true)]
        // Same rule, and a remainder of 10 is written as K.
        [InlineData("12000008K", true)]
        [InlineData("12000008k", true)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("12531909-3", false)]
        // python-stdnum doctest: letter inside the body.
        [InlineData("76086A28-5", false)]
        [InlineData("12345678", false)]
        // Only a leading "CL" is stripped, so this is an 11 character string, not a RUT.
        [InlineData("76086CL4285", false)]
        [InlineData("1253190921", false)]
        [InlineData("1234567", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _chileValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        // Companies use the same RUT.
        [InlineData("76086428-5", true)]
        [InlineData("125319092", true)]
        [InlineData("12531909-3", false)]
        [InlineData("1253190921", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _chileValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("76086428-5", true)]
        [InlineData("CL 12531909-2", true)]
        [InlineData("12531909-3", false)]
        [InlineData("76086A28-5", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _chileValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        // Seven digits, Santiago Centro.
        [InlineData("8320000", true)]
        [InlineData("832-0000", true)]
        [InlineData("832000", false)]
        [InlineData("abcdefg", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _chileValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
