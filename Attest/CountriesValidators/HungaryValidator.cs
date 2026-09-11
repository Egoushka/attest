using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class HungaryValidator : IdValidationAbstract
    {
        public HungaryValidator()
        {
            CountryCode = nameof(Country.HU);
        }

        /// <summary>
        /// Szemelyi Szam Ellenorzese
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, "^[1-8][0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])[0-9]{3}[0-9]$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }
            var yearPrefix = "19";
            if (ssn[0] == '3' || ssn[0] == '4')
            {
                yearPrefix = "20";
            }

            bool reversedWeights;
            try
            {
                var year = int.Parse(yearPrefix + ssn.Substring(1, 2));
                var month = int.Parse(ssn.Substring(3, 2));
                var day = int.Parse(ssn.Substring(5, 2));
                DateTime date = new DateTime(year, month, day);
                if (date > DateTime.Now)
                {
                    return ValidationResult.InvalidDate();
                }
                // The check digit weights were reversed (10..1 instead of 1..10) for births
                // from 1997-01-01, so that swapping the last two digits is detected.
                // A leading 7 or 8 means an 18xx birth year, which always keeps the old weights.
                // https://hu.wikipedia.org/wiki/Szem%C3%A9lyi_azonos%C3%ADt%C3%B3
                reversedWeights = ssn[0] != '7' && ssn[0] != '8' && date >= new DateTime(1997, 1, 1);
            }
            catch
            {
                return ValidationResult.InvalidDate();
            }
            return (int)char.GetNumericValue(ssn[ssn.Length - 1]) == CheckSum(ssn, reversedWeights) ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Cegjegyzekszam Ellenorzese
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^(?:[01][0-9]|20)(?:[01][0-9]|2[0-3])[0-9]{6}$"))
            {
                return ValidationResult.Invalid("Invalid code");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Adoazonosito jel Ellenorzese
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string code)
        {
            code = code.RemoveSpecialCharacthers();

            if (!Regex.IsMatch(code, @"^8[2-5][0-9]{4}[0-9]{3}[0-9]$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }

            return (int)char.GetNumericValue(code[code.Length - 1]) == CheckSum(code) ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        private int CheckSum(string value, bool reversedWeights = false)
        {
            int sum = 0;
            for (int i = 0; i < value.Length - 1; i++)
            {
                var weight = reversedWeights ? value.Length - 1 - i : i + 1;
                sum += (int)char.GetNumericValue(value[i]) * weight;
            }

            return (sum % 11);
        }


        /// <summary>
        ///  Kozossegi Adoszam (ANUM)
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            vatId = vatId.Replace("hu", string.Empty).Replace("HU", string.Empty);
            if (!Regex.IsMatch(vatId, @"^\d{8}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }
            int[] multipliers = { 9, 7, 3, 1, 9, 7, 3 };
            var sum = vatId.Sum(multipliers);

            var checkDigit = 10 - sum % 10;

            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            bool isValid = checkDigit == vatId[7].ToInt();
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
