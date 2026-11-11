using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Bahrain.</summary>
    [System.Obsolete("Use Identifiers.For(Country) and IdentifierType.Validate. Removed in 3.0.")]
    public class BahrainValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Bahrain (BH).</summary>
        public BahrainValidator()
        {
            CountryCode = nameof(Country.BH);
        }

        /// <summary>The kinds BH has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber; }
        }

        /// <summary>
        /// Bahrain has no company identifier rule here, so every value is reported invalid -- which is not
        /// a verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>Validates a natural person's tax code.</summary>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{9}$"))
            {
                return ValidationResult.Invalid("123456789");
            }
            return ValidationResult.Success();
        }

        /// <summary>Validates a postal code issued by Bahrain.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{3,4}$"))
            {
                return ValidationResult.InvalidFormat("NNN or NNNN");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// VAT account number issued by the National Bureau for Revenue (NBR): a private number of 15
        /// digits, issued with the VAT registration certificate. No structure or check digit is
        /// published, so the length is all that is checked (KNOWN-ISSUES KI-BH-3).
        /// https://www.nbr.gov.bh/vat_faqs/2
        /// </summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(vatId, "^[0-9]{15}$"))
            {
                return ValidationResult.InvalidFormat("15 digits");
            }
            return ValidationResult.Success();
        }
    }
}
