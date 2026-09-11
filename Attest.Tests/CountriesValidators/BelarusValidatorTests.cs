using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class BelarusValidatorTests
    {
        private readonly BelarusValidator _belarusValidator;

        public BelarusValidatorTests()
        {
            _belarusValidator = new BelarusValidator();
        }

        [Theory]
        [InlineData("MA1953684", true)]
        [InlineData("200988541", true)]
        [InlineData("MA1953685", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _belarusValidator.ValidateNationalIdentity(code).IsValid);
        }

        // An individual holds the letter form of the UNP, a sole trader the numeric one.
        [Theory]
        [InlineData("MA1953684", true)]      // python-stdnum by.unp doctest
        [InlineData("УНП MA1953684", true)]  // python-stdnum by.unp doctest, with the УНП prefix
        [InlineData("МА1953684", true)]      // Cyrillic М and А, the look-alikes of Latin M and A
        [InlineData("НА1002009", true)]      // Cyrillic Н is Latin H, not N
        [InlineData("ma1953684", true)]
        [InlineData("AB1234563", true)]
        [InlineData("CT7770007", true)]
        [InlineData("EK0001238", true)]
        [InlineData("200988541", true)]      // Numeric form, issued to sole traders
        [InlineData("MA1953685", false)]     // Wrong check digit
        [InlineData("AD1953684", false)]     // D is not one of ABCEHKMOPT
        [InlineData("A11953684", false)]     // The first two characters are letters or digits, never mixed
        [InlineData("MA195368A", false)]     // The last seven characters must be digits
        [InlineData("MA195368", false)]      // Too short
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("garbage!!", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belarusValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // Organisations and sole traders are numbered with 9 digits.
        [Theory]
        [InlineData("200988541", true)]      // python-stdnum by.unp doctest
        [InlineData("100988545", true)]
        [InlineData("700887128", true)]
        [InlineData("191000773", true)]
        [InlineData("491100022", true)]
        [InlineData("УНП 200988541", true)]
        [InlineData("200 988 541", true)]
        [InlineData("200988542", false)]     // python-stdnum by.unp doctest, wrong check digit
        [InlineData("MA1953684", false)]     // Valid UNP, but the letter form belongs to an individual
        [InlineData("МА1953684", false)]
        [InlineData("800988541", false)]     // Region 8 does not exist
        [InlineData("000988541", false)]     // Region 0 does not exist
        [InlineData("100000010", false)]     // Weighted sum is 10 mod 11, so no check digit exists
        [InlineData("20098854", false)]      // Too short
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("!!!", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belarusValidator.ValidateEntity(code).IsValid);
        }

        // The UNP doubles as the VAT number, in either form.
        [Theory]
        [InlineData("200988541", true)]
        [InlineData("MA1953684", true)]
        [InlineData("УНП MA1953684", true)]
        [InlineData("UNP 200988541", true)]
        [InlineData("200 988 541", true)]
        [InlineData("ma1953684", true)]
        [InlineData("МА1953684", true)]
        [InlineData("200988542", false)]     // Wrong check digit
        [InlineData("AB1234567", false)]     // Wrong check digit, AB1234563 is the valid one
        [InlineData("900988541", false)]     // Region 9 does not exist
        [InlineData("A11953684", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belarusValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("220030", true)]         // Minsk
        [InlineData("220 030", true)]
        [InlineData("22003", false)]
        [InlineData("2200300", false)]
        [InlineData("ABCDEF", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _belarusValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
