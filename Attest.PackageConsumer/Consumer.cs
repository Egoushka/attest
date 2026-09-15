using System.ComponentModel.DataAnnotations;
using Attest;
using Attest.DataAnnotations;

namespace Attest.PackageConsumer
{
    /// <summary>
    /// Touches both packages the way a consumer does, so the compile fails rather than passes
    /// silently if an asset or a dependency is missing from the netstandard2.0 graph.
    /// </summary>
    internal sealed class Consumer
    {
        [VAT(Country.BE)]
        public string Vat { get; set; }

        [ZipCode(Country.BE)]
        public string PostalCode { get; set; }

        internal static bool IsValid(string value)
        {
            var validator = new CountryValidator();

            Attest.ValidationResult result = validator.ValidateVAT(value, Country.BE);
            IdentifierResult byKind = validator.Validate(value, Country.BE, IdentifierKind.Business);

            var context = new ValidationContext(new Consumer());
            bool viaAttribute = new VATAttribute(Country.BE).GetValidationResult(value, context) == null;

            return result.IsValid && byKind.IsValid && viaAttribute;
        }
    }
}
