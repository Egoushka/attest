using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ArgentinaValidatorTests
    {
        private readonly ArgentinaValidator _argentinaValidator;
        public ArgentinaValidatorTests()
        {
            _argentinaValidator = new ArgentinaValidator();
        }

        [Theory]
        [InlineData("20.123.456 ", true)]
        [InlineData("20123456", true)]
        // DNI below 10.000.000 has only 7 digits and is still in circulation.
        [InlineData("1234567", true)]
        [InlineData("123456", false)]
        [InlineData("2012345699", false)]
        [InlineData("1234567A", false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _argentinaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("20-05536168-2", true)]
        [InlineData("20267565393", true)]
        // Check digit calculated with weights 5,4,3,2,7,6,5,4,3,2 (mod 11).
        [InlineData("27230938607", true)]
        [InlineData("20267565392", false)]
        // Valid check digit but 99 is not an issued taxpayer type.
        [InlineData("99267565399", false)]
        // Eastern Arabic digits: .NET \d would match them and int.Parse would then throw.
        [InlineData("٢٠٢٦٧٥٦٥٣٩٣", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _argentinaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("20267565393", true)]
        [InlineData("20055361682", true)]
        [InlineData("30500010912", true)]
        [InlineData("2026756A393", false)]
        [InlineData("99267565399", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _argentinaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("20267565393", true)]
        [InlineData("20055361682", true)]
        [InlineData("2026756A393", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _argentinaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("2850590940090418135201", true)]
        [InlineData("28505909 40090418135201", true)]
        // First check digit (position 8) wrong.
        [InlineData("2810590940090418135201", false)]
        // Second check digit (position 22) wrong.
        [InlineData("2850590940090418135200", false)]
        [InlineData("285059094009041813520", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCbu(string code, bool isValid)
        {
            Assert.Equal(isValid, _argentinaValidator.ValidateCBU(code).IsValid);
        }

        [Theory]
        [InlineData("1425", true)]
        [InlineData("C1425DKE", true)]
        [InlineData("C1425 DKE", true)]
        [InlineData("c1425dke", true)]
        // The alternation used to be unanchored: anything starting with 4 digits passed.
        [InlineData("1234XYZQWERTY", false)]
        [InlineData("1425ABCD", false)]
        [InlineData("12345", false)]
        [InlineData("C1425DK", false)]
        [InlineData("ABCD", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _argentinaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
