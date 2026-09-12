using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class NigeriaValidatorTests
    {
        private readonly NigeriaValidator _nigeriaValidator;

        public NigeriaValidatorTests()
        {
            _nigeriaValidator = new NigeriaValidator();
        }

        // National Identification Number, eleven digits issued by NIMC. No check digit is
        // published, so only the length and the character set are validated.
        // https://nimc.gov.ng/faqs/
        [Theory]
        [InlineData("12345678901", true)]
        [InlineData("70123456789", true)]
        [InlineData("1234567890", false)]      // Ten digits, that is a TIN not a NIN
        [InlineData("123456789012", false)]    // Twelve digits
        [InlineData("1234567890A", false)]     // Letter in the last position
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Three TIN forms are in circulation and none of them has a published check digit: the
        // ten-digit JTB TIN, the twelve-digit FIRS TIN written 12345678-0001, and the thirteen
        // digit Tax ID that replaced both on 1 January 2026 under the Nigeria Tax Administration
        // Act 2025. The eleven-digit NIN is not among them, so a NIN is still rejected here.
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/nigeria-tin.pdf
        // https://fctirs.gov.ng/nigerian-tax-id-portal-goes-live/
        [Theory]
        [InlineData("1234567890", true)]       // JTB TIN
        [InlineData("0102345678", true)]
        [InlineData("12345678-0001", true)]    // FIRS TIN, hyphen stripped before matching
        [InlineData("123456780001", true)]     // The same FIRS TIN already written without it
        [InlineData("12345678-0002", true)]    // Second office of the same taxpayer
        [InlineData("1234567890123", true)]    // Tax ID under the 2025 Act
        [InlineData("123456789", false)]       // Nine digits
        [InlineData("12345678901", false)]     // Eleven digits, that is a NIN
        [InlineData("12345678-001", false)]    // Eleven digits once the hyphen is stripped
        [InlineData("12345678901234", false)]  // Fourteen digits
        [InlineData("123456789A", false)]      // Letter in the last position
        [InlineData("12345678-000A", false)]   // The letter survives stripping the hyphen
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // Companies hold the same numbers, so the rule is the same one. See
        // TheTinDoesNotRecordWhetherTheHolderIsAPersonOrACompany below for why that is correct.
        [Theory]
        [InlineData("1234567890", true)]       // JTB TIN, issued to corporate taxpayers too
        [InlineData("12345678-0001", true)]    // FIRS TIN
        [InlineData("1234567890123", true)]    // Corporate Tax ID under the 2025 Act
        [InlineData("123456789", false)]       // Nine digits
        [InlineData("12345678901", false)]     // Eleven digits, that is a NIN
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateEntity(code).IsValid);
        }

        // "Nigeria does not issue separate TIN for different taxes", so the VAT registration
        // number is the TIN.
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/nigeria-tin.pdf
        [Theory]
        [InlineData("1234567890", true)]
        [InlineData("12345678-0001", true)]
        [InlineData("1234567890123", true)]
        [InlineData("123456789", false)]       // Nine digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidateVAT(code).IsValid);
        }

        // Six digit NIPOST codes: 100001 is Ikeja HO in Lagos, 900001 is Garki HO in Abuja.
        // https://en.wikipedia.org/wiki/Postal_codes_in_Nigeria
        [Theory]
        [InlineData("100001", true)]
        [InlineData("900001", true)]
        [InlineData("10001", false)]           // Five digits
        [InlineData("1000011", false)]         // Seven digits
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _nigeriaValidator.ValidatePostalCode(code).IsValid);
        }

        /// <summary>
        /// The ambiguity here is a fact about Nigeria, not a gap in the validator, so it is pinned
        /// rather than fixed. Nigeria's own TIN profile at the OECD splits the two legacy formats
        /// by issuing authority and not by holder type: the ten-digit JTB TIN goes to employed
        /// individuals and to registered corporate taxpayers, and the twelve-digit FIRS TIN goes
        /// to corporate entities and also to armed forces members, police officers and diplomats.
        /// The thirteen-digit Tax ID that superseded both is one format generated from the
        /// holder's NIN or CAC number, carrying neither. Nothing in any of the three says which
        /// side of the line the holder is on, so a caller has to decide what an ambiguous match is
        /// worth. Only the eleven-digit NIN is unambiguously personal.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/nigeria-tin.pdf
        /// </summary>
        [Fact]
        public void TheTinDoesNotRecordWhetherTheHolderIsAPersonOrACompany()
        {
            var validator = new CountryValidator();

            foreach (var tin in new[] { "1234567890", "12345678-0001", "1234567890123" })
            {
                var result = validator.Validate(tin, Country.NG, IdentifierKind.Business);
                Assert.True(result.IsValid);
                Assert.True(result.IsAmbiguous);
                Assert.True(validator.Validate(tin, Country.NG, IdentifierKind.Person).IsValid);
            }

            var nin = validator.Validate("12345678901", Country.NG, IdentifierKind.Person);
            Assert.True(nin.IsValid);
            Assert.False(nin.IsAmbiguous);
        }
    }
}
