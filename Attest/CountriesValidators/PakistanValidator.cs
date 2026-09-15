using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Pakistan.</summary>
    public class PakistanValidator : IdValidationAbstract
    {

        /// <summary>Creates a validator for Pakistan (PK).</summary>
        public PakistanValidator()
        {
            CountryCode = nameof(Country.PK);
        }

        /// <summary>The kinds PK has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.Vat; }
        }

        /// <summary>
        /// The FBR issues a National Tax Number to companies, but publishes no format or check
        /// rule for it, so there is no rule to apply here. CountryValidator answers the caller
        /// with Invalid("Not supported").
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>
        /// Validate CNIC (Computerized National Identity Card)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();

            // 13 digits, first digit is the province code 1-7, last digit is the
            // gender digit and is never 0.
            // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.pk.cnic
            var isValid = Regex.IsMatch(id, "^[1-7][0-9]{11}[1-9]{1}$");
            if (isValid)
            {
                return ValidationResult.Success();
            }
            else
            {
                return ValidationResult.Invalid("Invalid format");
            }
        }

        /// <summary>
        /// Pakistan has no VAT number rule here, so every value is reported invalid -- which is not a
        /// verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>Validates a postal code issued by Pakistan.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
