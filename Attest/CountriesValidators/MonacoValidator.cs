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

        public override ValidationResult ValidateEntity(string id)
        {
            throw new NotSupportedException();
        }

        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            throw new NotSupportedException();
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
            if (!Regex.IsMatch(postalCode, "^980\\d{2}$"))
            {
                return ValidationResult.InvalidFormat("980NN");
            }
            return ValidationResult.Success();
        }
    }
}
