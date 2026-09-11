using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MontenegroValidatorTests
    {
        private readonly MontenegroValidator _montenegroValidator;

        public MontenegroValidatorTests()
        {
            _montenegroValidator = new MontenegroValidator();
        }

        // JMBG check digit: m = 11 - ((7(a+g)+6(b+h)+5(c+i)+4(d+j)+3(e+k)+2(f+l)) mod 11),
        // m of 10 or 11 becomes 0. Region digits 20-29 are Montenegro.
        // https://en.wikipedia.org/wiki/Unique_Master_Citizen_Number
        [Theory]
        [InlineData("0101990210005", true)]     // Born 1990-01-01, region 21, check digit computed
        [InlineData("1505978254568", true)]     // Born 1978-05-15, region 25
        [InlineData("0101990 21-0005", true)]   // Same number with separators
        [InlineData("0101990210006", false)]    // Wrong check digit
        [InlineData("0101990330000", false)]    // Valid check digit, region 33 is Croatia
        [InlineData("0113990210005", false)]    // Month 13 is not a date
        [InlineData("010199021000", false)]     // Twelve digits
        [InlineData("01019902100055", false)]   // Fourteen digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _montenegroValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("81000", true)]             // Podgorica
        [InlineData("85310", true)]             // Budva
        [InlineData("8100", false)]
        [InlineData("810000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _montenegroValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
