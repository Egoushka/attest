using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class PhilippinesValidator : IdValidationAbstract
    {
        // The BIR TIN is a nine digit core number followed, where the taxpayer has branches, by a
        // branch code: three digits in the long established twelve digit form (000 for the head
        // office and for individuals) and five digits on current BIR returns - "The last 5 digits
        // of the 14-digit TIN refers to the branch code", Guidelines and Instructions for BIR Form
        // No. 1701-MS (August 2024).
        // https://bir-cdn.bir.gov.ph/BIR/pdf/1701-MS%20Guide%20August%202024%20ENCS_Final.pdf
        // The trailing V (VAT registered) or N (non VAT) is how the registration type is written
        // on BIR forms, not part of the number itself. The BIR publishes no check digit.
        private const string TinPattern = @"^[0-9]{9}([0-9]{3}|[0-9]{5})?";

        public PhilippinesValidator()
        {
            CountryCode = nameof(Country.PH);
        }

        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers().ToUpperInvariant();
            if (!Regex.IsMatch(id, TinPattern + "[VN]?$"))
            {
                return ValidationResult.InvalidFormat("123456789012");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers().ToUpperInvariant();
            if (!Regex.IsMatch(id, TinPattern + "[VN]?$"))
            {
                return ValidationResult.InvalidFormat("1234-5678901-2");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^\\d{4}$"))
            {
                return ValidationResult.InvalidFormat("NNNN");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers().ToUpperInvariant();
            // A number written with the non VAT marker is not a VAT number; without a marker it
            // is the plain TIN, which a VAT registered taxpayer uses unchanged.
            if (!Regex.IsMatch(vatId, TinPattern + "V?$"))
            {
                return ValidationResult.InvalidFormat("123456789012V");
            }

            return ValidationResult.Success();
        }
    }
}
