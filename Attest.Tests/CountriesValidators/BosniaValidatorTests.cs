using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class BosniaValidatorTests
    {
        private readonly BosniaValidator _bosniaValidator;

        public BosniaValidatorTests()
        {
            _bosniaValidator = new BosniaValidator();
        }

        // JMBG check digit: m = 11 - ((7(a+g)+6(b+h)+5(c+i)+4(d+j)+3(e+k)+2(f+l)) mod 11),
        // m of 10 or 11 becomes 0. Region digits 10-19 are Bosnia and Herzegovina.
        // https://en.wikipedia.org/wiki/Unique_Master_Citizen_Number
        [Theory]
        [InlineData("0101990170003", true)]     // Born 1990-01-01, region 17, check digit computed
        [InlineData("2312985151236", true)]     // Born 1985-12-23, region 15
        [InlineData("0107006100014", true)]     // Born 2006-07-01, three digit year below 800
        [InlineData("0101990 17-0003", true)]   // Same number with separators
        [InlineData("0101990170004", false)]    // Wrong check digit
        [InlineData("0101990330000", false)]    // Valid check digit, region 33 is Croatia
        [InlineData("0113990170003", false)]    // Month 13 is not a date
        [InlineData("010199017000", false)]     // Twelve digits
        [InlineData("01019901700034", false)]   // Fourteen digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _bosniaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("0101990170003", true)]
        [InlineData("2312985151236", true)]
        [InlineData("0101990170004", false)]    // Wrong check digit
        [InlineData("010199017000", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bosniaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("71000", true)]             // Sarajevo
        [InlineData("78000", true)]             // Banja Luka
        [InlineData("7100", false)]
        [InlineData("710000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bosniaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
