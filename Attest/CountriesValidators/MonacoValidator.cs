using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class MonacoValidator : IdValidationAbstract
    {
        public MonacoValidator()
        {
            CountryCode = nameof(Country.MC);
        }

        /// <summary>The kinds MC has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.PersonalId | IdentifierKind.PersonalTaxCode; }
        }

        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidationResult.Invalid("Not supported");
        }

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
