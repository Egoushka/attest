using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class GeorgiaValidator : IdValidationAbstract
    {
        public GeorgiaValidator()
        {
            CountryCode = nameof(Country.GE);
        }

        public override ValidationResult ValidateEntity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^[0-9]{9}$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }
            return ValidationResult.Success();

        }

        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^([0-9]{9}|[0-9]{11})$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

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
