using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Attest.DataAnnotations
{
    /// <summary>
    /// When applied to a <see cref="string" /> property or parameter, validates that a valid TIN is provided.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class PersonTINAttribute : ValidationAttribute
    {
        public PersonTINAttribute(Country countryCode)
        {
            if (!Enum.IsDefined(typeof(Country), countryCode))
            {
                throw new ArgumentNullException(nameof(countryCode));
            }

            CountryCode = countryCode;
        }

        public Country CountryCode { get; set; }

        protected override System.ComponentModel.DataAnnotations.ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return System.ComponentModel.DataAnnotations.ValidationResult.Success;
            }

            if (!(value is string vat))
            {
                // base.IsValid(object) is not implemented by ValidationAttribute, so
                // deferring to it threw NotImplementedException. A value that is not a
                // string is not a valid code, and saying so beats throwing.
                return new System.ComponentModel.DataAnnotations.ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    validationContext.MemberName != null ? new[] { validationContext.MemberName } : null);
            }

            CountryValidator taxValidator = new CountryValidator();
            ValidationResult result = taxValidator.ValidateIndividualTaxCode(vat, CountryCode);
            if (result.IsValid)
            {
                return System.ComponentModel.DataAnnotations.ValidationResult.Success;
            }

            validationContext.Items["Error"] = result.ErrorMessage;

            IEnumerable<string> memberNames = null;
            if (validationContext.MemberName != null)
            {
                memberNames = new[] { validationContext.MemberName };
            }

            return new System.ComponentModel.DataAnnotations.ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
        }

    }
}
