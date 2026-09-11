using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class LatviaValidatorTests
    {
        private readonly LatviaValidator _latviaValidator;

        public LatviaValidatorTests()
        {
            _latviaValidator = new LatviaValidator();
        }

        [Theory]
        [InlineData("161175-19997", true)]
        [InlineData("16117519997", true)]
        [InlineData("161375-19997", false)]
        [InlineData("16117510010", true)]  // Check digit 0, born 1975-11-16
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _latviaValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("161175-19997", true)]
        [InlineData("16117519997", true)]
        [InlineData("161375-19997", false)]
        [InlineData("16117510010", true)]   // Check digit 0, weighted sum leaves remainder 10 mod 11
        [InlineData("240688-10040", true)]  // Same remainder 10 case, born 1988-06-24
        [InlineData("31129910180", true)]   // Same remainder 10 case, born 1999-12-31
        [InlineData("05030220180", true)]   // Same remainder 10 case, century digit 2, born 2002-03-05
        [InlineData("24068810041", false)]  // Wrong check digit, should be 0
        [InlineData("29028110180", false)]  // 1981-02-29 is not a date
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("not a number", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _latviaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("40003521600", true)]
        [InlineData("16117519997", true)]
        [InlineData("40003521601", false)]
        [InlineData("161375197", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _latviaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("40003521600", true)]
        [InlineData("16117519997", true)]
        [InlineData("40003521601", false)]
        [InlineData("167519997", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _latviaValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("LV-1000", true)]
        [InlineData("LV1073", true)]
        [InlineData("1073", true)]
        [InlineData("LT-2321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _latviaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
