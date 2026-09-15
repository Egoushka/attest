using Attest;
using Xunit;

namespace Attest.Tests
{
    public class CountryValidatorKindsTests
    {
        private readonly CountryValidator _validator;

        public CountryValidatorKindsTests()
        {
            _validator = new CountryValidator();
        }

        [Fact]
        public void PersonalNumberMatchesPersonButNotBusiness()
        {
            // Belgian national register number, check digits 61
            var asPerson = _validator.Validate("93051822361", Country.BE, IdentifierKind.Person);
            var asBusiness = _validator.Validate("93051822361", Country.BE, IdentifierKind.Business);

            Assert.True(asPerson.IsValid);
            Assert.False(asBusiness.IsValid);
            Assert.False(asPerson.IsAmbiguous);
        }

        [Fact]
        public void CompanyNumberMatchesBusinessButNotPerson()
        {
            var asBusiness = _validator.Validate("0428759497", Country.BE, IdentifierKind.Business);
            var asPerson = _validator.Validate("0428759497", Country.BE, IdentifierKind.Person);

            Assert.True(asBusiness.IsValid);
            Assert.False(asPerson.IsValid);
        }

        [Fact]
        public void AnyIsTheDefaultAndCoversBothCategories()
        {
            Assert.True(_validator.Validate("93051822361", Country.BE).IsValid);
            Assert.True(_validator.Validate("0428759497", Country.BE).IsValid);
        }

        [Fact]
        public void CountriesThatIssueOneNumberForBothReportAmbiguity()
        {
            // Armenia issues one 8 digit TIN to people and to companies alike, and the State
            // Revenue Committee states that no meaning is carried by the digits
            var result = _validator.Validate("02618169", Country.AM, IdentifierKind.Business);

            Assert.True(result.IsValid);
            Assert.True(result.IsAmbiguous);
            Assert.Equal(IdentifierKind.PersonalTaxCode, result.Matched & IdentifierKind.PersonalTaxCode);
        }

        /// <summary>
        /// README and <see cref="IdentifierResult.IsAmbiguous"/> both name Russia here, and both
        /// had to be narrowed once already: the ambiguity is the sole trader's, not every Russian
        /// number's. A ten digit ИНН belongs to a legal entity and nobody else, so it is business
        /// and unambiguous; the twelve digit form is a person's, and that same person files VAT
        /// under it. Nothing tested the distinction, which is why the prose drifted from it.
        /// </summary>
        [Fact]
        public void RussiaIsAmbiguousForTheSoleTraderAndNotForTheCompany()
        {
            var person = _validator.Validate("500100732259", Country.RU, IdentifierKind.Business);
            var company = _validator.Validate("7707083893", Country.RU, IdentifierKind.Business);

            Assert.True(person.IsValid);
            Assert.True(person.IsAmbiguous);

            Assert.True(company.IsValid);
            Assert.False(company.IsAmbiguous);
        }

        [Fact]
        public void PostalCodeIsOutsideAny()
        {
            Assert.False(_validator.Validate("1000", Country.BE).IsValid);
            Assert.True(_validator.Validate("1000", Country.BE, IdentifierKind.PostalCode).IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-an-identifier")]
        public void GarbageIsInvalidAndNeverThrows(string value)
        {
            var result = _validator.Validate(value, Country.BE);

            Assert.False(result.IsValid);
            Assert.False(result.IsAmbiguous);
        }

        [Fact]
        public void UnregisteredCountryIsInvalidRatherThanThrowing()
        {
            var result = _validator.Validate("93051822361", Country.XX);

            Assert.False(result.IsValid);
            Assert.False(_validator.Supports(Country.XX, IdentifierKind.Person));
        }

        [Fact]
        public void DetailsCarryEveryEvaluatedKind()
        {
            var result = _validator.Validate("93051822361", Country.BE, IdentifierKind.Person);

            Assert.Contains(IdentifierKind.PersonalId, result.Details.Keys);
            Assert.Contains(IdentifierKind.PersonalTaxCode, result.Details.Keys);
            Assert.Contains(IdentifierKind.CompanyNumber, result.Details.Keys);
            Assert.Contains(IdentifierKind.Vat, result.Details.Keys);
            Assert.DoesNotContain(IdentifierKind.PostalCode, result.Details.Keys);
            Assert.Equal(IdentifierKind.Person, result.Requested);
        }

        [Fact]
        public void SupportsDistinguishesNoRuleFromWrongValue()
        {
            Assert.True(_validator.Supports(Country.BE, IdentifierKind.Vat));
            Assert.False(_validator.Supports(Country.US, IdentifierKind.Vat));

            // A kind with no rule reports the value invalid, which is not a verdict on the value
            Assert.False(_validator.Validate("anything", Country.US, IdentifierKind.Vat).IsValid);
        }
    }
}
