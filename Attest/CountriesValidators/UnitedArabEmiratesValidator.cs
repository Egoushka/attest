using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by United Arab Emirates.</summary>
    public class UnitedArabEmiratesValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for United Arab Emirates (AE).</summary>
        public UnitedArabEmiratesValidator()
        {
            CountryCode = nameof(Country.AE);
        }

        /// <summary>The kinds AE has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.PersonalTaxCode | IdentifierKind.PostalCode | IdentifierKind.Vat; }
        }

        /*
         * 
         *     "784-1980-1234567-9",
    "123-1234-0123456-7",
    "784198012345679"
         */

        /// <summary>Validates a national identification number issued by United Arab Emirates.</summary>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^784[0-9]{4}[0-9]{7}[0-9]{1}$"))
            {
                return ValidationResult.InvalidFormat("xxx-xxxx-xxxxxxx-x");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// United Arab Emirates has no company identifier rule here, so every value is reported invalid --
        /// which is not a verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>
        /// United Arab Emirates has no personal tax code rule here, so every value is reported invalid --
        /// which is not a verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>
        /// United Arab Emirates has no postal code rule here, so every value is reported invalid -- which
        /// is not a verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>
        /// United Arab Emirates has no VAT number rule here, so every value is reported invalid -- which is
        /// not a verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidationResult.Invalid("Not supported");
        }
    }
}
