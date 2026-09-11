using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class CyprusValidator : IdValidationAbstract
    {
        public CyprusValidator()
        {
            CountryCode = nameof(Country.CY);
        }

        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            // Cypriot identity card number: 10 digits, no published check digit,
            // so only the format can be verified.
            // https://learn.microsoft.com/en-us/purview/sit-defn-cyprus-identity-card
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^\d{10}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNNNNN");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidateEntity(string id)
        {
            // Tax Identification Code / VAT number: 8 digits plus a mod 26 check letter.
            // Numbers starting with "12" are reserved and are never issued.
            // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.cy.vat.html
            id = id.RemoveSpecialCharacthers().ToUpper().Replace("CY", string.Empty);

            if (!Regex.IsMatch(id, @"^\d{8}[A-Z]$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNNNL");
            }

            if (id.StartsWith("12"))
            {
                return ValidationResult.Invalid("Numbers starting with 12 are reserved.");
            }

            var result = 0;
            for (var index = 0; index < 8; index++)
            {
                var temp = id[index].ToInt();

                if (index % 2 == 0)
                {
                    switch (temp)
                    {
                        case 0:
                            temp = 1;
                            break;
                        case 1:
                            temp = 0;
                            break;
                        case 2:
                            temp = 5;
                            break;
                        case 3:
                            temp = 7;
                            break;
                        case 4:
                            temp = 9;
                            break;
                        default:
                            temp = temp * 2 + 3;
                            break;
                    }
                }
                result += temp;
            }

            var checkDigit = result % 26;
            bool isValid = id[8] == (char)(checkDigit + 65);
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        public override ValidationResult ValidateIndividualTaxCode(string vatId)
        {
            return ValidateEntity(vatId);
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            // The VAT number is the Tax Identification Code prefixed with CY.
            return ValidateEntity(vatId);
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
    }
}
