using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Hungary.</summary>
    public class HungaryValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Hungary (HU).</summary>
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
            // The leading digit carries the century as well as the sex: 1, 2, 5 and 6 mean a 19xx
            // birth year, 7 and 8 an 18xx one, and 3 and 4 are ambiguous between 18xx and 20xx.
            // https://hu.wikipedia.org/wiki/Szem%C3%A9lyi_azonos%C3%ADt%C3%B3
            var yearPrefix = "19";
            if (ssn[0] == '3' || ssn[0] == '4')
            {
                yearPrefix = "20";
            }
            else if (ssn[0] == '7' || ssn[0] == '8')
            {
                yearPrefix = "18";
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
                    if (yearPrefix != "20")
                    {
                        return ValidationResult.InvalidDate();
                    }

                    // A leading 3 or 4 reads as 18xx or 20xx; once the 20xx reading is in the
                    // future, 18xx is the only one left. When both readings are in the past the
                    // number is genuinely ambiguous and 20xx is kept, since nobody born in the
                    // 1800s is alive to hold the other one.
                    date = new DateTime(year - 200, month, day);
                }
                // The check digit weights were reversed (10..1 instead of 1..10) for births
                // from 1997-01-01, so that swapping the last two digits is detected.
                reversedWeights = date >= new DateTime(1997, 1, 1);
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
            // BB-FF-NNNNNN: BB is the registering court, 01 (Budapest) to 20, and FF is the
            // cegforma, 01 (vallalat) to 23. Neither has a 00.
            // https://hu.wikipedia.org/wiki/C%C3%A9gjegyz%C3%A9ksz%C3%A1m
            if (!Regex.IsMatch(id, @"^(?:0[1-9]|1[0-9]|20)(?:0[1-9]|1[0-9]|2[0-3])[0-9]{6}$"))
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
            vatId = vatId.StripPrefix("HU");
            if (!Regex.IsMatch(vatId, @"^[0-9]{8}$"))
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

        /// <summary>Validates a postal code issued by Hungary.</summary>
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
