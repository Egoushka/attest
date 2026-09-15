using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Kazakhstan.</summary>
    public class KazahstanValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Kazakhstan (KZ).</summary>
        public KazahstanValidator()
        {
            CountryCode = nameof(Country.KZ);
        }

        /// <summary>
        /// BIN БСН – бизнес-сәйкестендіру нөмірі
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{12}$"))
            {
                return ValidationResult.InvalidFormat("123456789012");
            }
            return ValidateCheckDigit(id);
        }

        /// <summary>
        /// PIN
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^[0-9]{12}$"))
            {
                return ValidationResult.InvalidFormat("123456789012");
            }

            // The first six digits are range-checked only and the 7th digit (century and sex) is
            // not checked at all: постановление Правительства РК № 853 of 26.08.2013 struck the birth date, the
            // century and the sex out of the Правила формирования идентификационного номера, so an issued IIN
            // need not agree with any of them - "их несовпадение с датой рождения не является ошибкой" - and
            // 0 is in any case a documented value of the 7th digit, for foreign nationals. A real
            // calendar check on the first six digits, or a 0-6 range check on the 7th, would reject
            // numbers the state does issue.
            // https://ru.wikipedia.org/wiki/%D0%98%D0%BD%D0%B4%D0%B8%D0%B2%D0%B8%D0%B4%D1%83%D0%B0%D0%BB%D1%8C%D0%BD%D1%8B%D0%B9_%D0%B8%D0%B4%D0%B5%D0%BD%D1%82%D0%B8%D1%84%D0%B8%D0%BA%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D1%8B%D0%B9_%D0%BD%D0%BE%D0%BC%D0%B5%D1%80
            try
            {
                int month = int.Parse(ssn.Substring(2, 2));
                int day = int.Parse(ssn.Substring(4, 2));
                if (month > 12 || month < 1 || day < 1 || day > 31)
                {
                    return ValidationResult.InvalidDate();
                }
            }
            catch
            {
                return ValidationResult.InvalidDate();
            }

            return ValidateCheckDigit(ssn);
        }

        /// <summary>
        /// IIN and BIN share the same check digit: the weighted sum of the first
        /// eleven digits modulo 11. A remainder of 10 is recalculated with the
        /// alternative weights; when that is also 10 the number is never issued,
        /// so it stays invalid.
        /// https://ru.wikipedia.org/wiki/%D0%98%D0%BD%D0%B4%D0%B8%D0%B2%D0%B8%D0%B4%D1%83%D0%B0%D0%BB%D1%8C%D0%BD%D1%8B%D0%B9_%D0%B8%D0%B4%D0%B5%D0%BD%D1%82%D0%B8%D1%84%D0%B8%D0%BA%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D1%8B%D0%B9_%D0%BD%D0%BE%D0%BC%D0%B5%D1%80
        /// </summary>
        private ValidationResult ValidateCheckDigit(string id)
        {
            int[] multipliers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            int[] multipliersDoubleCheck = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 1, 2 };

            var checkDigit = id.Sum(multipliers) % 11;
            if (checkDigit == 10)
            {
                checkDigit = id.Sum(multipliersDoubleCheck) % 11;
            }

            var isValid = checkDigit == id[11].ToInt();
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// BIN 
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

        /// <summary>Validates a postal code issued by Kazakhstan.</summary>
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
