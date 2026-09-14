using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class SouthAfricaValidator : IdValidationAbstract
    {
        public SouthAfricaValidator()
        {
            CountryCode = nameof(Country.ZA);
        }

        public override ValidationResult ValidateNationalIdentity(string number)
        {
            number = number.RemoveSpecialCharacthers();

            if (!number.All(char.IsDigit))
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

        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateVAT(ssn);
        }

        public override ValidationResult ValidateVAT(string number)
        {
            number = number.RemoveSpecialCharacthers();
            number = number?.StripPrefix("ZA");

            if (!Regex.IsMatch(number, @"^[01239]\d{9}$"))
            {
                return ValidationResult.InvalidFormat("012391239");
            }

            bool isValid = number.CheckLuhnDigit();
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
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
