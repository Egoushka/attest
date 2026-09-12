using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class TaiwanValidatorTests
    {
        private readonly TaiwanValidator _taiwanValidator;

        public TaiwanValidatorTests()
        {
            _taiwanValidator = new TaiwanValidator();
        }

        // National id: letter + [12] + 8 digits. The letter maps to two digits (A=10 ... Z=33,
        // I=34, O=35) weighted 1 and 9, the following digits are weighted 8..1 and the check
        // digit 1; the total must be a multiple of 10.
        // https://en.wikipedia.org/wiki/National_identification_card_(Taiwan)
        [Theory]
        [InlineData("A123456789", true)]        // textbook example, weighted sum 130
        [InlineData("A223344553", true)]        // check digit 3 computed from the published rule
        [InlineData("F131425364", true)]        // check digit 4 computed from the published rule
        [InlineData("L203040508", true)]        // check digit 8 computed from the published rule
        [InlineData("AB12345677", true)]        // resident certificate, check digit 7 computed
        [InlineData("FC22334450", true)]        // resident certificate, check digit 0 computed
        [InlineData("A800000014", true)]        // post-2021 resident certificate, official example
        [InlineData("A912345673", true)]        // post-2021 resident certificate, check digit computed
        [InlineData("A800000015", false)]       // post-2021 form, wrong check digit
        [InlineData("A123456788", false)]       // wrong check digit
        [InlineData("AB12345678", false)]       // wrong check digit
        [InlineData("A323456789", false)]       // third character of the gender digit is not 1 or 2
        [InlineData("A12345678", false)]        // 9 characters
        [InlineData("A1234567890", false)]      // 11 characters
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("A123456789", true)]
        [InlineData("A123456788", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("A123456789", true)]
        [InlineData("L203040508", true)]
        [InlineData("A123456788", false)]       // wrong check digit
        [InlineData("AB12345677", false)]       // resident certificate, not a local id
        [InlineData("A800000014", false)]       // post-2021 resident certificate, not a local id
        [InlineData("A12345678", false)]        // 9 characters
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestLocalSSN(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateLocalSSN(code).IsValid);
        }

        // Resident certificates issued from 2021-01-02 carry one letter and nine digits, the
        // second being 8 (male) or 9 (female); it enters the checksum as a plain digit, so the
        // arithmetic is the national id's. A800000014 is the example the announcement gives.
        // https://www.cna.com.tw/news/asoc/202012160106.aspx
        [Theory]
        [InlineData("AB12345677", true)]
        [InlineData("AD00011124", true)]        // check digit 4 computed from the published rule
        [InlineData("A800000014", true)]        // post-2021 form, official example
        [InlineData("F801122336", true)]        // post-2021 form, check digit 6 computed
        [InlineData("A912345673", true)]        // post-2021 form, female gender digit
        [InlineData("A800000015", false)]       // post-2021 form, wrong check digit
        [InlineData("A700000014", false)]       // second character 7 is neither a letter A-D nor 8/9
        [InlineData("AB12345678", false)]       // wrong check digit
        [InlineData("A123456789", false)]       // local id, not a resident certificate
        [InlineData("AE12345677", false)]       // second letter outside A-D
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestResidentSSN(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateResidentSSN(code).IsValid);
        }

        // Unified Business Number: eight digits weighted 1,2,1,2,1,2,4,1 with the digits of each
        // product added together. The Ministry of Finance relaxed the rule on 2023-04-01 so the
        // total only has to be divisible by 5, and when the seventh digit is 7 the total may also
        // be one short of a multiple.
        // https://www.mof.gov.tw/singlehtml/384fb3077bb349ea973e7fc6f13b6974?cntId=8d164b10f20042b9ab9864b51b20f0c2
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.tw.ubn.html
        [Theory]
        [InlineData("22099131", true)]          // TSMC, sum 30
        [InlineData("04541302", true)]          // Foxconn, sum 30
        [InlineData("20000019", true)]          // sum 15: divisible by 5 only, valid since 2023-04-01
        [InlineData("12345675", true)]          // seventh digit 7, sum 39, valid one short of 40
        [InlineData("22099130", false)]         // wrong check digit, sum 29
        [InlineData("24681357", false)]         // sum 39 but the seventh digit is 5, so no exception
        [InlineData("2209913", false)]          // 7 digits
        [InlineData("220991311", false)]        // 9 digits
        [InlineData("2209913A", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestEntity(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateEntity(code).IsValid);
        }

        // Business tax is charged against the same Unified Business Number.
        [Theory]
        [InlineData("22099131", true)]
        [InlineData("22099130", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestVat(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("10058", true)]             // Zhongzheng, Taipei
        [InlineData("40341", true)]             // West district, Taichung
        [InlineData("100", true)]               // bare district code, Zhongzheng
        [InlineData("100091", true)]            // 3+3 form, in use since 2020-03-03
        [InlineData("1234", false)]             // 4 digits
        [InlineData("1234567", false)]          // 7 digits
        [InlineData("10", false)]               // 2 digits
        [InlineData("abcde", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _taiwanValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
