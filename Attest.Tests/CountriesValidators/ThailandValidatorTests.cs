using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class ThailandValidatorTests
    {
        private readonly ThailandValidator _thailandValidator;

        public ThailandValidatorTests()
        {
            _thailandValidator = new ThailandValidator();
        }

        [Theory]
        [InlineData("3100600445635", true)]
        [InlineData("1-2345-45678-78-1", true)]
        [InlineData("5100600445631", true)]     // Category 5, missed by the census
        [InlineData("8100600445636", true)]     // Category 8, naturalised or permanent resident
        [InlineData("1234545678789", false)]    // Wrong check digit
        [InlineData("0107537001510", false)]    // Juristic person registered with the DBD
        [InlineData("0105-515-004-336", false)] // Juristic person registered with the DBD
        [InlineData("0993000133978", false)]    // Juristic person registered with the Revenue Department
        [InlineData("0991000001239", false)]    // Foreign individual, has a TIN but no citizen number
        [InlineData(null, false)]
        [InlineData("", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("3100600445635", true)]
        [InlineData("1-2345-45678-78-1", true)]
        [InlineData("0991000001239", true)]     // Revenue Department number for a foreign individual
        [InlineData("0107537001510", false)]    // Juristic person, not an individual
        [InlineData("0105-515-004-336", false)] // Juristic person, not an individual
        [InlineData("0994000617721", false)]    // Income payer, not an individual
        [InlineData("0107537001511", false)]    // Wrong check digit
        [InlineData("8112289874", false)]       // Ten digit format, withdrawn in 2012
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("0107537001706", true)]
        [InlineData("0107-537-001-706", true)]
        [InlineData("0105536112014", true)]
        [InlineData("0993000133978", true)]  // Revenue Department, category 3, juristic person
        [InlineData("0994000617721", true)]  // Revenue Department, category 4, income payer
        [InlineData("0992000000128", true)]  // Revenue Department, category 2, body of persons
        [InlineData("0107537001707", false)] // Wrong check digit
        [InlineData("3100600445635", false)] // Citizen number, not a juristic person
        [InlineData("0991000001239", false)] // Revenue Department, category 1, foreign individual
        [InlineData("0099000000122", false)] // 009 is issued by no agency
        [InlineData("0971000000127", false)] // 097 is issued by no agency
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("0105515004336", true)]
        [InlineData("3100600445635", true)]   // A sole trader registers on the personal number
        [InlineData("0991000001239", true)]
        [InlineData("123456789101", false)]   // Twelve digits
        [InlineData("0107537001511", false)]  // Wrong check digit
        [InlineData("0099000000122", false)]  // 009 is issued by no agency
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("11455", true)]
        [InlineData("21321", true)]
        [InlineData("321", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _thailandValidator.ValidatePostalCode(code).IsValid);
        }

    }
}
