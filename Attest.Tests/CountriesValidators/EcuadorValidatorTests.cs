using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class EcuadorValidatorTests
    {
        private readonly EcuadorValidator _ecuadorValidator;

        public EcuadorValidatorTests()
        {
            _ecuadorValidator = new EcuadorValidator();
        }

        // CI (Cedula de identidad): ten digits, a province code of 01-24 (or 30/50), a third digit
        // below 6 and a Luhn style check digit folded modulo 10.
        // 171430710-3 and 1714307104 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.ec.ci.html
        [Theory]
        [InlineData("171430710-3", true)]
        [InlineData("1714307103", true)]
        [InlineData("1700000019", true)]    // Check digit computed with the same fold
        [InlineData("0950000018", true)]    // Province 09, third digit 5
        [InlineData("1714307104", false)]   // Wrong check digit
        [InlineData("2514307103", false)]   // Province 25 does not exist
        [InlineData("171430710", false)]    // Nine digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCi(string code, bool isValid)
        {
            Assert.Equal(isValid, _ecuadorValidator.ValidateCI(code).IsValid);
        }

        // RUC: thirteen digits. The third digit picks the taxpayer class - below 6 is a natural RUC
        // (a CI plus a three digit establishment), 6 is a public RUC (nine digits checked against
        // weights 3,2,7,6,5,4,3,2,1 plus a four digit establishment) and 9 is a juridical RUC (ten
        // digits checked against weights 4,3,2,7,6,5,4,3,2,1 plus a three digit establishment).
        // 1792060346-001 and 1763154690001 are from
        // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.ec.ruc.html
        [Theory]
        [InlineData("1792060346-001", true)]
        [InlineData("1792060346001", true)]
        [InlineData("1714307103001", true)]     // Natural RUC built on the CI above
        [InlineData("1790000001001", true)]     // Juridical, check sum computed over ten digits
        [InlineData("1760000070001", true)]     // Public, check sum computed over nine digits
        [InlineData("1763154690001", false)]    // Wrong check sum
        [InlineData("1714307104001", false)]    // Natural RUC over a CI with a wrong check digit
        [InlineData("1714307103000", false)]    // Establishment number 000
        [InlineData("1760000070000", false)]    // Public RUC with establishment number 0000
        [InlineData("1770000070001", false)]    // Third digit 7 is not a taxpayer class
        [InlineData("2592060346001", false)]    // Province 25 does not exist
        [InlineData("179206034601", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ecuadorValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("1792060346001", true)]
        [InlineData("1714307103001", true)]
        [InlineData("1763154690001", false)]    // Wrong check sum
        [InlineData("179206034601", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ecuadorValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("1792060346001", true)]
        [InlineData("1790000001001", true)]
        [InlineData("1763154690001", false)]    // Wrong check sum
        [InlineData("179206034601", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ecuadorValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("1792060346001", true)]
        [InlineData("1763154690001", false)]    // Wrong check sum
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _ecuadorValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("170150", true)]
        [InlineData("090150", true)]
        [InlineData("17015", false)]    // Five digits
        [InlineData("1701500", false)]  // Seven digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _ecuadorValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
