using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class AndorraValidatorTests
    {
        private readonly AndorraValidator _andorraValidator;
        public AndorraValidatorTests()
        {
            _andorraValidator = new AndorraValidator();
        }

        [Theory]
        [InlineData("F-123456-Z", true)]   // Resident natural person, NIA preceded by "F"
        [InlineData("F059888N", true)]
        [InlineData("E-850123-K", true)]   // Non-resident natural person, numbered from 800000 up
        [InlineData("E123456Z", false)]    // Below 800000 an "E" number is a non-resident entity
        [InlineData("U-132950-X", false)]  // Parapublic entity, not a person
        [InlineData("D059888N", false)]    // Public body, not a person
        [InlineData("A123B", false)]
        [InlineData("2012345699", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _andorraValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("F-123456-Z", true)]
        [InlineData("F059888N", true)]
        [InlineData("E-850123-K", true)]
        [InlineData("E123456Z", false)]
        [InlineData("F700000Z", false)]    // Resident natural persons end at 699999
        [InlineData("U-132950-X", false)]
        [InlineData("D059888N", false)]
        [InlineData("A-759999-X", false)]  // Societat anònima, not a person
        [InlineData("L-700001-B", false)]  // Societat de responsabilitat limitada, not a person
        [InlineData("I-706193-G", false)]  // "I" is not an issued holder-type letter
        [InlineData("A123B", false)]
        [InlineData("2012345699", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _andorraValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("U-132950-X", true)]
        [InlineData("D059888N", true)]
        [InlineData("A-759999-X", true)]   // Societat anònima, 700000 to 799999
        [InlineData("L-700001-B", true)]   // Societat de responsabilitat limitada, same band
        [InlineData("C-012345-J", true)]   // Comunitat de béns
        [InlineData("E123456Z", true)]     // Non-resident legal entity, any number
        [InlineData("F-123456-Z", false)]  // Natural person, not an entity
        [InlineData("F059888N", false)]
        [InlineData("A-123456-X", false)]  // Outside the 700000 to 799999 band
        [InlineData("I-706193-G", false)]
        [InlineData("A123B", false)]
        [InlineData("2012345699", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _andorraValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("U-132950-X", true)]
        [InlineData("D059888N", true)]
        [InlineData("A-759999-X", true)]
        [InlineData("F059888N", true)]     // A natural person declares IGI under their own NRT
        [InlineData("E-850123-K", true)]
        [InlineData("A-123456-X", false)]
        [InlineData("I-706193-G", false)]
        [InlineData("A123B", false)]
        [InlineData("2012345699", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _andorraValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("AD500", true)]
        [InlineData("AD 500", true)]
        [InlineData("AD5000", false)]
        [InlineData("500", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _andorraValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
