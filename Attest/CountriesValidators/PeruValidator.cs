using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class PeruValidator : IdValidationAbstract
    {

        public PeruValidator()
        {
            CountryCode = nameof(Country.PE);
        }


        private string CalculateChecksumNationalIdentity(string number)
        {
            int[] weights = new int[] { 3, 2, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(number[i]);
            }
            sum %= 11;
            return string.Format("{0}{1}", "65432110987"[sum], "KJIHGFEDCBA"[sum]);
        }

        /// <summary>
        /// Cédula Única de Identidad (CUI)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string number)
        {
            // python-stdnum upper cases before validating, so a lower case check letter
            // is accepted: https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/cui.py
            number = number.RemoveSpecialCharacthers().ToUpperInvariant();
            if (!(number.Length == 8 || number.Length == 9))
            {
                return ValidationResult.InvalidLength();
            }
            // [0-9] and not char.IsDigit: IsDigit also accepts non-ASCII Unicode digits.
            else if (!Regex.IsMatch(number.Substring(0, 8), "^[0-9]+$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }
            else if (number.Length > 8 && !CalculateChecksumNationalIdentity(number).Contains(number[number.Length - 1]))
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// RUC Peruvian company tax number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateIndividualTaxCode(id);
        }

        private int CalculateChecksum(string number)
        {
            int[] weights = new int[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(number[i]);
            }
            sum %= 11;
            return (11 - sum).Mod(10);
        }

        /// <summary>
        /// RUC 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string number)
        {
            number = number.RemoveSpecialCharacthers();
            string[] validNumbers = new string[] { "10", "15", "17", "20" };
            if (number.Length != 11)
            {
                return ValidationResult.InvalidLength();
            }
            // [0-9] and not char.IsDigit: IsDigit also accepts non-ASCII Unicode digits.
            else if (!Regex.IsMatch(number, "^[0-9]{11}$"))
            {
                return ValidationResult.InvalidFormat("12345678901");
            }
            else if (!validNumbers.Contains(number.Substring(0, 2)))
            {
                return ValidationResult.Invalid("Invalid");
            }
            else if (!number.EndsWith(CalculateChecksum(number).ToString()))
            {
                return ValidationResult.InvalidChecksum();
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
            if (!Regex.IsMatch(postalCode, "^[0-9]{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
