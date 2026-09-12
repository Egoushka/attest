using Attest.Countries;
using System;
using Xunit;

namespace Attest.Tests
{
    public class KoreaValidatorTests
    {
        private readonly KoreaValidator _koreaValidator;

        public KoreaValidatorTests()
        {
            _koreaValidator = new KoreaValidator();
        }

        // Resident Registration Number: YYMMDD-SXXXXXC, the last digit is a check digit over
        // the first 12 weighted 2,3,4,5,6,7,8,9,2,3,4,5 with (11 - sum mod 11) mod 10.
        // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.kr.rrn
        [Theory]
        [InlineData("9710139019902", true)]     // python-stdnum kr.rrn example
        [InlineData("971013-9019902", true)]    // same number in presentation format
        [InlineData("8507151012340", true)]     // check digit 0 computed from the published rule
        [InlineData("6812152609879", true)]     // check digit 9 computed from the published rule
        [InlineData("0102033012341", true)]     // check digit 1 computed from the published rule
        [InlineData("1503223012340", true)]     // born 2015: an RRN is issued at birth registration
        [InlineData("9710139969900", true)]     // place of birth 96, the highest accepted value
        [InlineData("9710139019903", false)]    // python-stdnum kr.rrn wrong check digit example
        [InlineData("8507151012341", false)]    // wrong check digit
        [InlineData("9713139019909", false)]    // month 13, check digit is correct
        [InlineData("9912313012346", false)]    // birth date 2099-12-31 is in the future
        [InlineData("9710139979908", false)]    // place of birth 97 is above the documented maximum
        [InlineData("971013901990", false)]     // 12 digits
        [InlineData("97101390199022", false)]   // 14 digits
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _koreaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("9710139019902", true)]
        [InlineData("9710139019903", false)]
        [InlineData("abc", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _koreaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Korea issues a separate business registration number and has no VAT number, so this
        // validator has no rule for either; CountryValidator turns the throw into
        // Invalid("Not supported"), and CountryValidator.Supports reads it as "no rule".
        [Theory]
        [InlineData("1234567890")]
        [InlineData(null)]
        public void TestEntityIsNotSupported(string code)
        {
            Assert.Throws<NotSupportedException>(() => _koreaValidator.ValidateEntity(code));
        }

        [Theory]
        [InlineData("1234567890")]
        [InlineData(null)]
        public void TestVatIsNotSupported(string code)
        {
            Assert.Throws<NotSupportedException>(() => _koreaValidator.ValidateVAT(code));
        }

        [Theory]
        [InlineData("03187", true)]             // Jongno-gu, Seoul
        [InlineData("06236", true)]             // Gangnam-gu, Seoul
        [InlineData("1234", false)]             // 4 digits
        [InlineData("123456", false)]           // 6 digits
        [InlineData("abcde", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _koreaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
