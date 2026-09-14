using Attest.Countries;
using System;
using Attest;
using Xunit;

namespace Attest.Tests
{
    public class UnitedStatesValidatorTests
    {
        private readonly UnitedStatesValidator _unitedStatesValidator;
        public UnitedStatesValidatorTests()
        {
            _unitedStatesValidator = new UnitedStatesValidator();
        }

        [Theory]
        [InlineData("181-26-4874", true)]
        [InlineData("136-23-9624", true)]
        [InlineData("181-26-48741", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _unitedStatesValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("181-26-4874", true)]
        [InlineData("912903456", true)]
        [InlineData("20267565392", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _unitedStatesValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("91-1144442", true)]
        [InlineData("07-1144442", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _unitedStatesValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("20267565393")]
        public void TestCorrectVatCode(string code)
        {
            AssertNotSupported(_unitedStatesValidator.ValidateVAT(code));

        }

        [Theory]
        [InlineData("95014", true)]
        [InlineData("99999-9999", true)]
        [InlineData("950142", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _unitedStatesValidator.ValidatePostalCode(code).IsValid);
        }

        /// <summary>
        /// A kind the country has no rule for is answered, not thrown at. It used to throw
        /// NotSupportedException, which escaped to anyone calling the validator class directly.
        /// </summary>
        private static void AssertNotSupported(ValidationResult result)
        {
            Assert.False(result.IsValid);
            Assert.Equal("Not supported", result.ErrorMessage);
        }

    }
}
