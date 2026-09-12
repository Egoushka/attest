using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ElSalvadorValidatorTests
    {
        private readonly ElSalvadorValidator _elSalvadorValidator;

        public ElSalvadorValidatorTests()
        {
            _elSalvadorValidator = new ElSalvadorValidator();
        }

        // NIT: fourteen digits shaped MMMM-DDMMYY-SSS-C, starting with 0, 1 or 9. When the three
        // digit sequence is 100 or lower the check digit is sum(weights 14..2) % 11 % 10, otherwise
        // it is -sum(weights 2,7,6,5,4,3,2,7,6,5,4,3,2) mod 11 mod 10.
        // A leading "SV" country prefix is stripped before the number is checked.
        // 0614-050707-104-8, "SV 0614-050707-104-8" and 0614-050707-104-0 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.sv.nit.html
        [Theory]
        [InlineData("0614-050707-104-8", true)]
        [InlineData("06140507071048", true)]
        [InlineData("SV 0614-050707-104-8", true)]  // A leading "SV" country prefix is stripped
        [InlineData("sv 0614-050707-104-8", true)]
        [InlineData("12171105992011", true)]     // Sequence 201, check digit computed with the new weights
        [InlineData("06140507070990", true)]     // Sequence 099, check digit computed with the old weights
        [InlineData("99991234561000", true)]     // Sequence 100 still uses the old weights
        [InlineData("0614-050707-104-0", false)] // Wrong check digit
        [InlineData("12171105992010", false)]    // Wrong check digit
        [InlineData("26140507071048", false)]    // First digit must be 0, 1 or 9
        [InlineData("0614SV0507071048", false)]  // Only a leading "SV" is stripped
        [InlineData("0614050707104", false)]     // Thirteen digits
        [InlineData("0614050707104a", false)]    // Letter in the number
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _elSalvadorValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("06140507071048", true)]
        [InlineData("12171105992011", true)]
        [InlineData("06140507071040", false)]    // Wrong check digit
        [InlineData("26140507071048", false)]    // First digit must be 0, 1 or 9
        [InlineData("0614050707104", false)]     // Thirteen digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _elSalvadorValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("06140507071048", true)]
        [InlineData("06140507070990", true)]
        [InlineData("06140507071040", false)]    // Wrong check digit
        [InlineData("0614050707104", false)]     // Thirteen digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _elSalvadorValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("06140507071048", true)]
        [InlineData("06140507071040", false)]    // Wrong check digit
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _elSalvadorValidator.ValidateNationalIdentity(code).IsValid);
        }

        // El Salvador uses the single postal code 1101, as recorded in Google's address metadata
        // for SV (zip pattern "1101").
        [Theory]
        [InlineData("1101", true)]
        [InlineData("1101 ", true)]
        [InlineData("1102", false)]
        [InlineData("11011", false)]    // Five digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _elSalvadorValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
