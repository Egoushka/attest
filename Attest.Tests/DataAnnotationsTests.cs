using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Attest;
using Attest.DataAnnotations;
using Xunit;
using DataAnnotationsResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Attest.Tests
{
    /// <summary>
    /// The Attest.DataAnnotations package shipped three versions with no tests of any kind. It is a
    /// thin wrapper, but the thin part is where its defects were: every attribute recorded its
    /// failure reason under a fixed key with <c>IDictionary.Add</c>, so a second failed validation
    /// against the same context threw ArgumentException.
    /// </summary>
    public class DataAnnotationsTests
    {
        private class Model
        {
            [PersonTIN(Country.BE)]
            public string TaxCode { get; set; }

            [VAT(Country.BE)]
            public string Vat { get; set; }

            [ZipCode(Country.BE)]
            public string PostalCode { get; set; }
        }

        [Fact]
        public void AValidValuePasses()
        {
            var model = new Model { TaxCode = "93051822361", Vat = "BE0428759497", PostalCode = "1000" };

            Assert.True(Validator.TryValidateObject(model, Context(model), new List<DataAnnotationsResult>(), true));
        }

        [Fact]
        public void AnInvalidValueFailsOnTheAnnotatedMember()
        {
            var model = new Model { TaxCode = "not-a-number" };
            var results = new List<DataAnnotationsResult>();

            Assert.False(Validator.TryValidateObject(model, Context(model), results, true));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Model.TaxCode)));
        }

        /// <summary>
        /// Null is the absence of a value, which is <c>[Required]</c>'s job rather than this
        /// attribute's. Validating it here would make every optional annotated property mandatory.
        /// </summary>
        [Fact]
        public void NullIsLeftToRequired()
        {
            var model = new Model();

            Assert.True(Validator.TryValidateObject(model, Context(model), new List<DataAnnotationsResult>(), true));
        }

        /// <summary>
        /// The regression this file exists for. Every attribute wrote its reason with
        /// <c>validationContext.Items.Add("Error", ...)</c>, and Add throws on a key that is already
        /// present, so reusing one context threw instead of validating.
        /// </summary>
        [Fact]
        public void AReusedValidationContextDoesNotThrow()
        {
            var model = new Model { TaxCode = "not-a-number" };
            var context = new ValidationContext(model) { MemberName = nameof(Model.TaxCode) };
            var results = new List<DataAnnotationsResult>();

            Validator.TryValidateProperty(model.TaxCode, context, results);
            Validator.TryValidateProperty(model.TaxCode, context, results);

            Assert.Equal(2, results.Count);
        }

        [Theory]
        [InlineData("0428759497", true)]
        [InlineData("BE0428759497", true)]
        [InlineData("0428759498", false)]
        public void TheAttributeAgreesWithTheValidatorItWraps(string value, bool expected)
        {
            var attribute = new VATAttribute(Country.BE);
            var model = new Model();

            bool viaAttribute = attribute.GetValidationResult(value, Context(model)) == DataAnnotationsResult.Success;
            bool viaValidator = new CountryValidator().ValidateVAT(value, Country.BE).IsValid;

            Assert.Equal(expected, viaAttribute);
            Assert.Equal(viaValidator, viaAttribute);
        }

        [Fact]
        public void EveryAttributeRejectsAnUndefinedCountry()
        {
            // Not ArgumentNullException, which is what it used to throw, but the constructor does
            // still refuse a value that is not a member of the enum.
            Assert.ThrowsAny<System.ArgumentException>(() => new PersonTINAttribute((Country)9999));
            Assert.ThrowsAny<System.ArgumentException>(() => new CompanyTINAttribute((Country)9999));
            Assert.ThrowsAny<System.ArgumentException>(() => new SSNAttribute((Country)9999));
            Assert.ThrowsAny<System.ArgumentException>(() => new VATAttribute((Country)9999));
            Assert.ThrowsAny<System.ArgumentException>(() => new ZipCodeAttribute((Country)9999));
        }

        [Fact]
        public void ANonStringValueIsNotClaimedValid()
        {
            var attribute = new VATAttribute(Country.BE);

            Assert.NotEqual(DataAnnotationsResult.Success, attribute.GetValidationResult(42, Context(new Model())));
        }

        private static ValidationContext Context(object instance)
        {
            return new ValidationContext(instance);
        }
    }
}
