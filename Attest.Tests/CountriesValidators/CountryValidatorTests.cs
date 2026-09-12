using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class CountryValidatorTests
    {
        private readonly CountryValidator _countryValidator;

        public CountryValidatorTests()
        {
            _countryValidator = new CountryValidator();
        }

        /// <summary>
        /// A registered country whose validator throws for a method it has no rule for must be
        /// answered exactly like an unregistered country (XX), not escape the facade.
        /// </summary>
        [Theory]
        [InlineData("123456789", Country.US)]   // UnitedStatesValidator.ValidateVAT throws NotSupportedException
        [InlineData("123456789", Country.AE)]
        [InlineData("123456789", Country.HK)]
        [InlineData("123456789", Country.XX)]   // no validator registered
        [InlineData(null, Country.US)]
        [InlineData("", Country.AE)]
        public void TestVatOfCountryWithoutVat(string code, Country country)
        {
            var result = _countryValidator.ValidateVAT(code, country);

            Assert.False(result.IsValid);
            Assert.Equal("Not supported", result.ErrorMessage);
        }

        [Theory]
        [InlineData("123456789", Country.AE)]
        [InlineData("123456789", Country.HK)]   // HongKongValidator.ValidateEntity throws NotImplementedException
        [InlineData("123456789", Country.XX)]
        [InlineData(null, Country.AE)]
        public void TestEntityOfCountryWithoutEntityCode(string code, Country country)
        {
            var result = _countryValidator.ValidateEntity(code, country);

            Assert.False(result.IsValid);
            Assert.Equal("Not supported", result.ErrorMessage);
        }

        /// <summary>
        /// Montenegro used to throw NotImplementedException from these three methods, so the facade
        /// answered "Not supported". It has rules now, and must answer on the merits instead.
        /// </summary>
        [Fact]
        public void MontenegroAnswersOnTheMeritsWhereItUsedToThrow()
        {
            Assert.True(_countryValidator.ValidateEntity("02655284", Country.ME).IsValid);
            Assert.True(_countryValidator.ValidateVAT("02655284", Country.ME).IsValid);

            var wrong = _countryValidator.ValidateEntity("02655285", Country.ME);
            Assert.False(wrong.IsValid);
            Assert.NotEqual("Not supported", wrong.ErrorMessage);
        }

        [Theory]
        [InlineData("123456789", Country.AE)]
        [InlineData("123456789", Country.XX)]
        [InlineData(null, Country.AE)]
        public void TestIndividualTaxCodeOfCountryWithoutOne(string code, Country country)
        {
            var result = _countryValidator.ValidateIndividualTaxCode(code, country);

            Assert.False(result.IsValid);
            Assert.Equal("Not supported", result.ErrorMessage);
        }

        [Theory]
        [InlineData("12345", Country.AE)]
        [InlineData("12345", Country.HK)]
        [InlineData("12345", Country.XX)]
        [InlineData(null, Country.AE)]
        public void TestPostalCodeOfCountryWithoutOne(string code, Country country)
        {
            var result = _countryValidator.ValidateZIPCode(code, country);

            Assert.False(result.IsValid);
            Assert.Equal("Not supported", result.ErrorMessage);
        }

        /// <summary>
        /// CountryCode used to be static, so every validator constructed afterwards overwrote it
        /// process wide and the last country registered by CountryValidator (ZA) always won.
        /// </summary>
        [Fact]
        public void TestCountryCodeIsPerInstance()
        {
            var unitedStates = new UnitedStatesValidator();
            var germany = new GermanyValidator();

            Assert.Equal(nameof(Country.US), unitedStates.CountryCode);
            Assert.Equal(nameof(Country.DE), germany.CountryCode);
        }
    }
}
