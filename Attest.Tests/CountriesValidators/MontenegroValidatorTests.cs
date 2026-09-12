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

        // PIB, eight digits whose last digit is a modulus 11 check digit over the
        // weights 8,7,6,5,4,3,2, a remainder of 10 written as 0.
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.me.pib.html
        [Theory]
        [InlineData("02655284", true)]          // python-stdnum doctest
        [InlineData("02 655 284", true)]        // Same number with separators
        // PIBs quoted in a report of the Drzavna revizorska institucija Crne Gore.
        [InlineData("02333643", true)]
        [InlineData("02013886", true)]
        [InlineData("02655283", false)]         // Wrong check digit
        [InlineData("0265528", false)]          // Seven digits
        [InlineData("026552840", false)]        // Nine digits
        // Eastern Arabic digits: .NET \d would match them and char.GetNumericValue
        // would then validate a number no register issued.
        [InlineData("٠٢٦٥٥٢٨٤", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _montenegroValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("02655284", true)]
        [InlineData("02333643", true)]
        [InlineData("02655283", false)]         // Wrong check digit
        [InlineData("0265528", false)]          // Seven digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _montenegroValidator.ValidateEntity(code).IsValid);
        }

        // Natural persons are registered with the tax authority under their JMB.
        [Theory]
        [InlineData("0101990210005", true)]
        [InlineData("1505978254568", true)]
        [InlineData("0101990210006", false)]    // Wrong check digit
        [InlineData("0101990330000", false)]    // Valid check digit, region 33 is Croatia
        [InlineData("010199021000", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _montenegroValidator.ValidateIndividualTaxCode(code).IsValid);
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
