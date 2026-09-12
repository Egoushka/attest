using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MacedoniaValidatorTests
    {
        private readonly MacedoniaValidator _macedoniaValidator;

        public MacedoniaValidatorTests()
        {
            _macedoniaValidator = new MacedoniaValidator();
        }

        // JMBG check digit: m = 11 - ((7(a+g)+6(b+h)+5(c+i)+4(d+j)+3(e+k)+2(f+l)) mod 11),
        // m of 10 or 11 becomes 0. Region digits 41-49 are North Macedonia.
        // https://en.wikipedia.org/wiki/Unique_Master_Citizen_Number
        [Theory]
        [InlineData("0101990410004", true)]     // Born 1990-01-01, region 41, check digit computed
        [InlineData("0403981447899", true)]     // Born 1981-03-04, region 44
        [InlineData("0101990 41-0004", true)]   // Same number with separators
        [InlineData("0101990410005", false)]    // Wrong check digit
        [InlineData("0101990330000", false)]    // Valid check digit, region 33 is Croatia
        [InlineData("0113990410004", false)]    // Month 13 is not a date
        [InlineData("010199041000", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _macedoniaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // ЕДБ, thirteen digits with a modulus 11 check digit, optionally prefixed with MK.
        // Numbers taken from https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.mk.edb.html
        [Theory]
        [InlineData("4030000375897", true)]
        [InlineData("4020990116747", true)]
        [InlineData("MK4057009501106", true)]
        [InlineData("mk4057009501106", true)]
        [InlineData("МК 4020990116747", true)]  // Cyrillic prefix, python-stdnum doctest
        [InlineData("4030000375890", false)]    // Wrong check digit
        // Eastern Arabic digits: .NET \d would match them and char.GetNumericValue would
        // then read them as the check digit of a number no register issued.
        [InlineData("٤٠٣٠٠٠٠٣٧٥٨٩٧", false)]
        [InlineData("403000037589", false)]     // Twelve digits
        [InlineData("MK40300003758970", false)] // Fourteen digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _macedoniaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("4030000375897", true)]
        [InlineData("MK4057009501106", true)]
        [InlineData("4030000375890", false)]    // Wrong check digit
        [InlineData("403000037589", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _macedoniaValidator.ValidateEntity(code).IsValid);
        }

        // The EMBG of a Macedonian citizen is also his tax number, so this is the JMBG rule.
        // https://www.ujp.gov.mk/files/attachment/0000/0154/Pravilnik_za_postapkata_nacinot_i_rokovite_za_dodeluvanje_na_edinstven_danocen_broj_161_09__od_31.12.2009.pdf
        [Theory]
        [InlineData("0101990410004", true)]
        [InlineData("0403981447899", true)]
        [InlineData("0101990410005", false)]    // Wrong check digit
        [InlineData("0101990330000", false)]    // Valid check digit, region 33 is Croatia
        [InlineData("010199041000", false)]     // Twelve digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _macedoniaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("1000", true)]              // Skopje
        [InlineData("7000", true)]              // Bitola
        [InlineData("100", false)]
        [InlineData("10000", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _macedoniaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
