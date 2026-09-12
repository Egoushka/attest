using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class MauritiusValidator : IdValidationAbstract
    {
        public MauritiusValidator()
        {
            CountryCode = nameof(Country.MU);
        }

        /// <summary>
        /// Business Registration Number issued by the Corporate and Business Registration
        /// Department: a letter for the entity type (C company, I individual, P partnership, and
        /// F among others), or LLP/LP for a partnership, followed by the two digit year of
        /// registration and the serial. No check digit is published.
        /// Lengths and prefixes taken from the MRA register of VAT registered persons, which
        /// prints the BRN of every registered business: all 35953 of them are a single letter
        /// and eight digits, or LLP/LP and six to eight digits.
        /// https://www.mra.mu/download/ListofVATRegPersons.pdf
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, "^([A-Z][0-9]{8}|(LLP|LP)[0-9]{6,8})$"))
            {
                return ValidationResult.InvalidFormat("C21179119");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// ID number (Mauritian national identifier)
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string number)
        {
            number = number.RemoveSpecialCharacthers();
            if (number.Length != 14)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!Regex.IsMatch(number, "^[A-Z][0-9]+[0-9A-Z]$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }
            else if (CalculateChecksum(number.Substring(0, 13)) != number[number.Length - 1])
            {
                return ValidationResult.InvalidChecksum();
            }
            else if (!ValidateDate(number))
            {
                return ValidationResult.InvalidDate();
            }
            return ValidationResult.Success();
        }


        private bool ValidateDate(string number)
        {
            try
            {
                int day = int.Parse(number.Substring(1, 2));
                int month = int.Parse(number.Substring(3, 2));

                if (month < 1 || month > 12)
                {
                    return false;
                }

                // 29 February is accepted in every year: the card carries a two digit year with
                // no century, so the leap year cannot be told. python-stdnum assumes 2000+ here
                // and flags the assumption in its own source.
                // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.mu.nid.html
                int[] daysInMonth = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

                return day >= 1 && day <= daysInMonth[month - 1];
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Weights 14 down to 2 over the first thirteen characters, modulus 17. The leading
        /// character is the initial of the surname, so its value is its position in the alphabet
        /// below - char.GetNumericValue returns -1 for letters and skews the whole sum.
        /// https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.mu.nid.html
        /// </summary>
        public char CalculateChecksum(string number)
        {
            string _alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int sum = 0;
            for (int i = 0; i < number.Length; i++)
            {
                sum += (14 - i) * _alphabet.IndexOf(number[i]);
            }

            sum = (17 - sum).Mod(17);

            return _alphabet[sum];
        }
        /// <summary>
        /// VAT Registration Number allocated by the Mauritius Revenue Authority: eight digits,
        /// no published check digit. All 35994 numbers in the MRA's own quarterly register of
        /// VAT registered persons are eight digits, with leading digits 1, 2, 3, 4, 5, 7 and 8
        /// all occurring, so the first digit is not constrained.
        /// https://www.mra.mu/download/ListofVATRegPersons.pdf
        /// </summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(vatId, "^[0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNNN");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Mauritius has postal codes, but the library carries no rule for them. The throw is the
        /// sentinel CountryValidator.Supports reads to tell 'no rule' from 'invalid value'.
        /// </summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            throw new NotSupportedException();
        }
    }
}
