using System;
using System.Linq;
using System.Text.RegularExpressions;


namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Chile.</summary>
    public class ChileValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Chile (CL).</summary>
        public ChileValidator()
        {
            CountryCode = nameof(Country.CL);
        }


        /// <summary>
        /// Validate  National Tax Number (RUN/RUT)  
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string number)
        {
            return ValidateVAT(number);
        }

        /// <summary>
        /// Validate  National Tax Number (RUN/RUT)  
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string number)
        {
            return ValidateVAT(number);
        }



        /// <summary>
        /// Validate  National Tax Number (RUN/RUT)  
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string number)
        {
            number = number.RemoveSpecialCharacthers();
            // python-stdnum upper cases before validating, so a lower case "k" check digit
            // is accepted: https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/cl/rut.py
            // Only a "CL" prefix is stripped, not every occurrence: "76086CL4285" is not a RUT.
            number = number.ToUpperInvariant();
            number = number.StripPrefix("CL");

            if (!(number.Length == 8 || number.Length == 9))
            {
                return ValidationResult.InvalidLength();
            }
            // [0-9] and not char.IsDigit: IsDigit also accepts non-ASCII Unicode digits.
            else if (!Regex.IsMatch(number.Substring(0, number.Length - 1), "^[0-9]+$"))
            {
                return ValidationResult.InvalidFormat("12345678 or 123456789");
            }

            if (number[number.Length - 1] != CalculateChecksum(number))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();

        }

        private int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        private char CalculateChecksum(string number)
        {
            int s = 0;

            char[] array = number.Substring(0, number.Length - 1).ToCharArray();
            Array.Reverse(array);
            number = new string(array);


            for (int i = 0; i < number.Length; i++)
            {
                s = s + ((int)char.GetNumericValue(number[i])) * (4 + Mod(5 - i, 6));
            }

            return "0123456789K"[s % 11];
        }

        /// <summary>Validates a postal code issued by Chile.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{7}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNN or NNN-NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
