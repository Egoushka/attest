using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Georgia.</summary>
    public class GeorgiaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Georgia (GE).</summary>
        public GeorgiaValidator()
        {
            CountryCode = nameof(Country.GE);
        }

        /// <summary>
        /// Validates a company identifier issued by Georgia: Identification Number (sakidentifikatsio
        /// nomeri).
        /// </summary>
        public override ValidationResult ValidateEntity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^[0-9]{9}$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }
            return ValidationResult.Success();

        }

        /// <summary>Validates a natural person's tax code.</summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^([0-9]{9}|[0-9]{11})$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }
            return ValidationResult.Success();
        }

        /// <summary>Validates a VAT number issued by Georgia: VAT Number.</summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

        /// <summary>Validates a postal code issued by Georgia.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{4}$"))
            {
                return ValidationResult.InvalidFormat("NNNN");
            }
            return ValidationResult.Success();
        }
    }
}
