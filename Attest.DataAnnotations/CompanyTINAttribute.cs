using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Attest.DataAnnotations
{
    /// <summary>
    /// When applied to a <see cref="string" /> property or parameter, validates that a valid Company Identification Code is provided.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class CompanyTINAttribute : ValidationAttribute
    {
        /// <summary>Validates a company or organisation identifier against <paramref name="countryCode"/>'s rule.</summary>
        /// <param name="countryCode">The country whose rule to apply.</param>
        /// <exception cref="System.ArgumentException">The country is not a defined <see cref="Country"/>.</exception>
        public CompanyTINAttribute(Country countryCode)
        {
            if (!Enum.IsDefined(typeof(Country), countryCode))
            {
                throw new ArgumentNullException(nameof(countryCode));
            }

            CountryCode = countryCode;
        }

        /// <summary>The country whose rule the annotated member is validated against.</summary>
        public Country CountryCode { get; set; }

        /// <summary>
        /// Defers to <see cref="CountryValidator.ValidateEntity"/>. Null passes, so that whether the
        /// member is optional stays <c>RequiredAttribute</c>'s decision; a value that is not a
        /// string fails rather than throwing; and the reason is recorded under
        /// <c>validationContext.Items["Error"]</c>, overwriting whatever was there so that a context
        /// used for a second value does not throw.
        /// </summary>
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
            ValidationResult result = taxValidator.ValidateEntity(vat, CountryCode);
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
