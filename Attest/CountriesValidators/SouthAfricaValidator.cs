using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by South Africa.</summary>
    public class SouthAfricaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for South Africa (ZA).</summary>
        public SouthAfricaValidator()
        {
            CountryCode = nameof(Country.ZA);
        }

        /// <summary>
        /// Validates a national identification number issued by South Africa: Social Number.
        /// </summary>
        public override ValidationResult ValidateNationalIdentity(string number)
        {
            number = number.RemoveSpecialCharacthers();

            if (!number.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("1234567890123");
            }
            else if (number.Length != 13)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!(number[10] == '0' || number[10] == '1'))
            {
                return ValidationResult.Invalid("The eleven digit must be 1 or 0");
            }
            else if (!HasValidDate(number))
            {
                return ValidationResult.InvalidDate();
            }

            return number.CheckLuhnDigit() ? ValidationResult.Success() : ValidationResult.InvalidChecksum();

        }

        private bool HasValidDate(string number)
        {
            try
            {
                //Only two digits are used for the year, so map it into the 100 year window ending today.
                //https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/za/idnr.py
                int year = int.Parse(number.Substring(0, 2)) + DateTime.Today.Year / 100 * 100;
                int month = int.Parse(number.Substring(2, 2));
                int day = int.Parse(number.Substring(4, 2));

                if (year > DateTime.Today.Year)
                {
                    year -= 100;
                }

                DateTime date = new DateTime(year, month, day);
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>Validates a company identifier issued by South Africa: VAT Code.</summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// Validates a natural person's tax code, which here is the same number
        /// <see cref="ValidateVAT"/> validates.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateVAT(ssn);
        }

        /// <summary>Validates a VAT number issued by South Africa: VAT Code.</summary>
        public override ValidationResult ValidateVAT(string number)
        {
            number = number.RemoveSpecialCharacthers();
            number = number?.StripPrefix("ZA");

            if (!Regex.IsMatch(number, @"^[01239][0-9]{9}$"))
            {
                return ValidationResult.InvalidFormat("012391239");
            }

            bool isValid = number.CheckLuhnDigit();
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>Validates a postal code issued by South Africa.</summary>
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
