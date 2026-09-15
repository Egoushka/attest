using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Monaco.</summary>
    public class MonacoValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Monaco (MC).</summary>
        public MonacoValidator()
        {
            CountryCode = nameof(Country.MC);
        }

        /// <summary>The kinds MC has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.PersonalId | IdentifierKind.PersonalTaxCode; }
        }

        /// <summary>
        /// Monaco has no company identifier rule here, so every value is reported invalid -- which is not a
        /// verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>
        /// Monaco has no personal tax code rule here, so every value is reported invalid -- which is not a
        /// verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>Validates a VAT number issued by Monaco: VAT Number.</summary>
        public override ValidationResult ValidateVAT(string number)
        {
            number = number.RemoveSpecialCharacthers();
            number = number.StripPrefix("FR").StripPrefix("MC");


            if (number.Length != 11)
            {
                return ValidationResult.InvalidLength();
            }
            else if (number.Substring(2, 3) != "000")
            {
                // Monaco numbers are issued as a French TVA whose SIREN part starts with "000".
                // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/fr/tva.py
                return ValidationResult.Invalid("Invalid Code");
            }

            return new FranceValidator().ValidateVAT(number);
        }

        /// <summary>Validates a postal code issued by Monaco.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^980[0-9]{2}$"))
            {
                return ValidationResult.InvalidFormat("980NN");
            }
            return ValidationResult.Success();
        }
    }
}
