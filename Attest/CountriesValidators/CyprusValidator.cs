using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Cyprus.</summary>
    public class CyprusValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Cyprus (CY).</summary>
        public CyprusValidator()
        {
            CountryCode = nameof(Country.CY);
        }

        /// <summary>Validates a national identification number issued by Cyprus: Identity Number.</summary>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            // Cypriot identity card number: 10 digits, no published check digit,
            // so only the format can be verified.
            // https://learn.microsoft.com/en-us/purview/sit-defn-cyprus-identity-card
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^[0-9]{10}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNNNNN");
            }
            return ValidationResult.Success();
        }

        /// <summary>Validates a company identifier issued by Cyprus.</summary>
        public override ValidationResult ValidateEntity(string id)
        {
            // Tax Identification Code / VAT number: 8 digits plus a mod 26 check letter.
            // Numbers starting with "12" are reserved and are never issued.
            // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.cy.vat.html
            id = id.RemoveSpecialCharacthers().ToUpperInvariant().StripPrefix("CY");

            if (!Regex.IsMatch(id, @"^[0-9]{8}[A-Z]$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNNNL");
            }

            if (id.StartsWith("12", StringComparison.Ordinal))
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

        /// <summary>
        /// Validates a natural person's tax code, which here is the same number
        /// <see cref="ValidateEntity"/> validates.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string vatId)
        {
            return ValidateEntity(vatId);
        }

        /// <summary>Validates a VAT number issued by Cyprus: ΦΠΑ.</summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            // The VAT number is the Tax Identification Code prefixed with CY.
            return ValidateEntity(vatId);
        }

        /// <summary>Validates a postal code issued by Cyprus.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            // Four digits running from 1000 to 9999, allocated by district: Nicosia 1000-2999,
            // Limassol 3000-4999, Famagusta 5000-5999, Larnaca 6000-7999, Paphos 8000-8999 and
            // Kyrenia 9000-9999. No code begins with a zero.
            // https://en.wikipedia.org/wiki/Postal_codes_in_Cyprus
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[1-9][0-9]{3}$"))
            {
                return ValidationResult.InvalidFormat("NNNN");
            }
            return ValidationResult.Success();
        }
    }
}
