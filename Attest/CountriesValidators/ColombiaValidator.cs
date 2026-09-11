
using System;
using System.Linq;
using System.Text.RegularExpressions;


namespace Attest.Countries
{
    public class ColombiaValidator : IdValidationAbstract
    {
        public ColombiaValidator()
        {
            CountryCode = nameof(Country.CO);
        }


        /// <summary>
        /// Validat RUT (Registro Unico Tributario)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// NIT (Número De Identificación Tributaria, Colombian identity code)
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateVAT(ssn);
        }

        public override ValidationResult ValidateVAT(string number)
        {
            number = number.RemoveSpecialCharacthers().ToUpperInvariant();
            // Only a "CO" prefix is stripped, not every occurrence: "213CO1234321" is not a NIT.
            if (number.StartsWith("CO"))
            {
                number = number.Substring(2);
            }
            if (!(8 <= number.Length && number.Length <= 16))
            {
                return ValidationResult.InvalidLength();
            }
            // [0-9] and not char.IsDigit: IsDigit also accepts non-ASCII Unicode digits.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/co/nit.py
            else if (!Regex.IsMatch(number, "^[0-9]+$"))
            {
                return ValidationResult.Invalid("Only digits are allowed");
            }
            if (CalculateChecksum(number.Substring(0, number.Length - 1)) != number[number.Length - 1])
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }


        private char CalculateChecksum(string number)
        {
            int s = 0;
            int[] weights = new int[] { 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };

            char[] charArray = number.ToCharArray();
            Array.Reverse(charArray);
            number = new string(charArray);

            for (int i = 0; i < number.Length; i++)
            {
                s = s + weights[i] * (int)char.GetNumericValue(number[i]);
            }

            s %= 11;

            return "01987654321"[s];
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{6}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNN");
            }
            return ValidationResult.Success();
        }

    }
}
