using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class IcelandValidatorTests
    {
        private readonly IcelandValidator _icelandValidator;

        public IcelandValidatorTests()
        {
            _icelandValidator = new IcelandValidator();
        }

        [Theory]
        [InlineData("1207742209", true)]  // Born 1974-07-12, check digit 0 (weighted sum 110, divisible by 11)
        [InlineData("0311880409", true)]  // Born 1988-11-03, check digit 0
        [InlineData("1207740509", true)]  // Born 1974-07-12, check digit 0
        [InlineData("120774-2209", true)] // Same number as printed, with the separator
        [InlineData("0101904529", true)]  // Born 1990-01-01, check digit 2
        [InlineData("3103991169", true)]  // Born 1999-03-31, check digit 6
        [InlineData("0101904519", false)] // Wrong check digit, should be 2
        [InlineData("1207740009", false)] // Weighted sum mod 11 is 1, so no check digit can match
        [InlineData("3002901109", false)] // 30 February never exists
        [InlineData("1207742208", false)] // Century digit 8: only 9 and 0 are issued, and 8 decoded to 28yy
        [InlineData("1207742203", false)] // Century digit 3 decoded to 23yy, a year no register issues
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghij", false)]
        [InlineData("123", false)]
        [InlineData("12077422090", false)] // Too long
        [InlineData("4710080280", false)] // Landsbankinn hf., an organisation: day 47 is 07 + 40
        [InlineData("6204830369", false)] // JBT Marel ehf., an organisation: day 62 is 22 + 40
        [InlineData("4010080280", false)] // Day 40 decodes to registration day 0, which never exists
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _icelandValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        // An organisation kennitala carries its registration date with 40 added to the day, so the
        // day field runs 41-71 and a person's 01-31 can never be read as an organisation.
        // https://en.wikipedia.org/wiki/Icelandic_identification_number
        [InlineData("4710080280", true)]  // Landsbankinn hf., registered 2008-10-07
        [InlineData("6312051780", true)]  // Icelandair Group hf., registered 2005-12-23
        [InlineData("6204830369", true)]  // JBT Marel ehf., registered 1983-04-22
        [InlineData("6407070540", true)]  // Marel Iceland ehf., registered 2007-07-24
        [InlineData("471008-0280", true)] // Same number as printed, with the separator
        [InlineData("4101902309", true)]  // Registered 1990-01-01, century digit 9
        [InlineData("4710080279", false)] // Wrong check digit, should be 8
        [InlineData("7104902369", false)] // Day 71 decodes to 31 April, which never exists
        [InlineData("4010080280", false)] // Day 40 decodes to registration day 0, which never exists
        [InlineData("1207742209", false)] // A person's kennitala, not an organisation's
        [InlineData("3103991169", false)] // A person's kennitala, not an organisation's
        [InlineData("0101904519", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghij", false)]
        [InlineData("47100802800", false)] // Too long
        public void TestEntity(string code, bool isValid)
        {
            Assert.Equal(isValid, _icelandValidator.ValidateEntity(code).IsValid);
        }
    }
}
