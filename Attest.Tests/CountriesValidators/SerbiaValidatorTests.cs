using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class SerbiaValidatorTests
    {
        private readonly SerbiaValidator _serbiaValidator;

        public SerbiaValidatorTests()
        {
            _serbiaValidator = new SerbiaValidator();
        }

        // JMBG check digit: m = 11 - ((7(a+g)+6(b+h)+5(c+i)+4(d+j)+3(e+k)+2(f+l)) mod 11),
        // m of 10 or 11 becomes 0. Region digits 70-79 are central Serbia, 80-89 Vojvodina.
        // https://en.wikipedia.org/wiki/Unique_Master_Citizen_Number
        [Theory]
        [InlineData("0101990710008", true)]     // Born 1990-01-01, region 71, check digit computed
        [InlineData("2904985843213", true)]     // Born 1985-04-29, region 84
        [InlineData("0101990 71-0008", true)]   // Same number with separators
        [InlineData("0101990710009", false)]    // Wrong check digit
        [InlineData("0101990330000", false)]    // Valid check digit, region 33 is Croatia
        [InlineData("0113990710008", false)]    // Month 13 is not a date
        [InlineData("010199071000", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _serbiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("0101990710008", true)]
        [InlineData("2904985843213", true)]
        [InlineData("0101990710009", false)]    // Wrong check digit
        [InlineData("010199071000", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _serbiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // PIB, nine digits closed with an ISO 7064 MOD 11,10 check digit.
        // 101134702 and 101134703 are from https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.rs.pib.html
        [Theory]
        [InlineData("101134702", true)]
        [InlineData("101111114", true)]         // Check digit computed with MOD 11,10
        [InlineData("RS101134702", true)]
        [InlineData("101134703", false)]        // Wrong check digit
        [InlineData("10113470", false)]         // Eight digits
        [InlineData("1011347020", false)]       // Ten digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _serbiaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("101134702", true)]
        [InlineData("101111114", true)]
        [InlineData("101134703", false)]        // Wrong check digit
        [InlineData("10113470", false)]         // Eight digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _serbiaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("11000", true)]             // Belgrade
        [InlineData("21000", true)]             // Novi Sad
        [InlineData("1100", false)]
        [InlineData("110000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _serbiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
