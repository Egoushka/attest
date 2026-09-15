using Attest;
using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class PeruValidatorTests
    {
        private readonly PeruValidator _peruValidator;

        public PeruValidatorTests()
        {
            _peruValidator = new PeruValidator();
        }

        [Theory]
        // CUI printed on the DNI: eight digits and an optional check digit or check letter.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/cui.py
        [InlineData("10117410", true)]
        [InlineData("10117410-2", true)]
        [InlineData("101174102", true)]
        // Letter form of the same check value: weights 3,2,7,6,5,4,3,2 mod 11 gives index 4,
        // where 65432110987 holds '2' and KJIHGFEDCBA holds 'G'.
        [InlineData("10117410G", true)]
        [InlineData("10117410g", true)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("10117410-3", false)]
        [InlineData("101174109", false)]
        [InlineData("1011741", false)]
        [InlineData("1011741A", false)]
        [InlineData("1011741012", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        // RUC of a natural person. The first two digits carry the holder type: SUNAT's own
        // description of the structure is "(a) Individuals identified with DNI: Prefix 10 + DNI +
        // verification digit. b) Individuals identified with another type of identity document:
        // Prefix 15 + random number + verification digit. c) Legal entities: Prefix 20 + random
        // number + verification digit."
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/peru-tin.pdf
        // 10054148289 is python-stdnum's doctest for to_dni(), which yields DNI 05414828.
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/ruc.py
        [InlineData("10054148289", true)]
        // Prefix 15, same eight digit body, check digit recomputed with weights 5,4,3,2,7,6,5,4,3,2.
        [InlineData("15054148281", true)]
        // Prefix 17 is absent from SUNAT's list, and the RUC guides describe it as the persona
        // natural no domiciliada -- an individual, not a company.
        [InlineData("17054148283", true)]
        // Prefix 20 is a legal entity, not a natural person. Both are real numbers with a correct
        // check digit, so only the taxpayer type tells them apart.
        [InlineData("20512333797", false)]
        [InlineData("20054148284", false)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("10054148288", false)]
        // 30 is not one of the 10, 15, 17, 20 taxpayer types.
        [InlineData("30512333797", false)]
        [InlineData("1005414828", false)]
        [InlineData("100541482891", false)]
        [InlineData("1005414828A", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        // Companies use the same RUC format, under taxpayer type 20.
        [InlineData("20512333797", true)]
        [InlineData("20054148284", true)]
        // Type 17 is a natural person, so it is not a company number. It used to be accepted as
        // both, which made every 17 ambiguous.
        [InlineData("17054148283", false)]
        // Types 10 and 15 belong to natural persons, so a company number is never one of them.
        [InlineData("10054148289", false)]
        [InlineData("15054148281", false)]
        // python-stdnum doctest: wrong check digit.
        [InlineData("20512333798", false)]
        [InlineData("30512333797", false)]
        [InlineData("2051233379", false)]
        [InlineData("205123337971", false)]
        [InlineData("2051233379A", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        // Peru has no separate VAT number: IGV is filed under the RUC, so every RUC is one --
        // a sole trader's included. These rows asserted the opposite, which rejected a real
        // registration because of who held it.
        [InlineData("20512333797", true)]
        [InlineData("10054148289", true)]
        [InlineData("15054148281", true)]
        [InlineData("17054148283", true)]
        [InlineData("20512333798", false)]
        [InlineData("2051233379", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        // The taxpayer type keeps the company number disjoint from the personal one, so no value
        // is valid as both a CompanyNumber and a PersonalTaxCode. VAT is the exception and it is
        // Peru's, not this library's: IGV is filed under whichever RUC the taxpayer holds, so a
        // sole trader's personal RUC is a VAT identifier too, and IsAmbiguous says so rather than
        // this library picking a side. Type 17 is a natural person and no longer both.
        [InlineData("10054148289", IdentifierKind.PersonalTaxCode | IdentifierKind.Vat, true)]
        [InlineData("17054148283", IdentifierKind.PersonalTaxCode | IdentifierKind.Vat, true)]
        [InlineData("20512333797", IdentifierKind.CompanyNumber | IdentifierKind.Vat, false)]
        public void TestKindMatchesTheTaxpayerType(string code, IdentifierKind expectedMatch, bool ambiguous)
        {
            IdentifierResult result = new CountryValidator().Validate(code, Country.PE);

            Assert.Equal(expectedMatch, result.Matched);
            Assert.Equal(ambiguous, result.IsAmbiguous);
        }

        [Theory]
        // Five digits, Lima.
        [InlineData("15001", true)]
        [InlineData("1500", false)]
        [InlineData("150011", false)]
        [InlineData("abcde", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _peruValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
