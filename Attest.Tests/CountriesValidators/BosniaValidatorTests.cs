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

        // JIB, thirteen digits starting with 4 whose last digit is a modulus 11 check digit
        // over the preceding twelve (weights 7,6,5,4,3,2,7,6,5,4,3,2, a remainder of 10
        // written as 0). Structure: Pravilnik o dodjeljivanju identifikacionih brojeva i
        // poreznoj registraciji (Porezna uprava FBiH) clan 11, and Pravilnik o uslovima i
        // nacinu registracije i identifikacije poreskih obveznika ("Sl. glasnik RS" 4/13) clan 11.
        [Theory]
        [InlineData("4200344670009", true)]     // Raiffeisen BANK dd Bosna i Hercegovina
        [InlineData("4402955260002", true)]     // JP Autoputevi RS d.o.o. Banja Luka
        [InlineData("4400959000002", true)]     // Poste Srpske a.d. Banja Luka
        [InlineData("4200-344670009", true)]    // Same number with separators
        [InlineData("4200344670008", false)]    // Wrong check digit
        [InlineData("420034467000", false)]     // Twelve digits
        [InlineData("42003446700090", false)]   // Fourteen digits
        [InlineData("0101990170003", false)]    // A JMBG is not a JIB
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bosniaValidator.ValidateEntity(code).IsValid);
        }

        // The identifikacioni broj of an indirect tax payer is the JIB without its leading "4",
        // so twelve digits carrying the same check digit.
        [Theory]
        [InlineData("402955260002", true)]      // PDV broj of JIB 4402955260002
        [InlineData("200344670009", true)]      // PDV broj of JIB 4200344670009
        [InlineData("402955260003", false)]     // Wrong check digit
        [InlineData("4402955260002", false)]    // Thirteen digits, that is the JIB
        [InlineData("40295526000", false)]      // Eleven digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _bosniaValidator.ValidateVAT(code).IsValid);
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
