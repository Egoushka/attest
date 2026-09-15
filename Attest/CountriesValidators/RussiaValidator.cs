using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class RussiaValidator : IdValidationAbstract
    {
        // ИНН. Russia issues two different numbers, not two formats of one: a legal entity gets a
        // 10 digit ИНН with a single check digit, a natural person a 12 digit one with two, and
        // neither length is ever issued to the other kind of holder. An individual entrepreneur
        // keeps his personal 12 digit number, so length alone tells person from company.
        // Приказ ФНС России от 29.06.2012 № ММВ-7-6/435@ fixed the two lengths and the order that
        // replaces it on 01.01.2026 (Приказ ФНС России от 26.06.2025 № ЕД-7-14/559@) keeps them.
        // Weights: https://www.kholenkov.ru/data-validation/inn/
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ru/inn.py
        private static readonly int[] _companyWeights = new int[] { 2, 4, 10, 3, 5, 9, 4, 6, 8 };
        private static readonly int[] _personalWeights1 = new int[] { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
        private static readonly int[] _personalWeights2 = new int[] { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

        public RussiaValidator()
        {
            CountryCode = nameof(Country.RU);
        }

        /// <summary>
        /// Validate the ИНН of a natural person, which is the 12 digit form. The 10 digit form
        /// belongs to a legal entity and is rejected here, see <see cref="ValidateEntity"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{12}$"))
            {
                return ValidationResult.InvalidFormat("123456789012");
            }

            return ValidatePersonalInn(id);
        }

        /// <summary>
        /// Validate the ИНН of a legal entity, which is the 10 digit form. A 12 digit personal
        /// number is rejected here.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{10}$"))
            {
                return ValidationResult.InvalidFormat("1234567890");
            }

            return ValidateCompanyInn(id);
        }

        /// <summary>
        /// Validate the ИНН a НДС return is filed under. Both forms qualify: organisations and
        /// individual entrepreneurs are both НДС payers (ст. 143 НК РФ) and an entrepreneur files
        /// under his own 12 digit personal ИНН, so this is the one place the two meet.
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            vatId = vatId.StripPrefix("RU");
            if (Regex.IsMatch(vatId, @"^[0-9]{10}$"))
            {
                return ValidateCompanyInn(vatId);
            }
            if (Regex.IsMatch(vatId, @"^[0-9]{12}$"))
            {
                return ValidatePersonalInn(vatId);
            }
            return ValidationResult.InvalidFormat("1234567890");
        }

        private static ValidationResult ValidateCompanyInn(string id)
        {
            bool isValid = int.Parse(id[9].ToString()) == CheckDigit(id, _companyWeights);
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        private static ValidationResult ValidatePersonalInn(string id)
        {
            bool isValid = int.Parse(id[10].ToString()) == CheckDigit(id, _personalWeights1)
                && int.Parse(id[11].ToString()) == CheckDigit(id, _personalWeights2);
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        private static int CheckDigit(string id, int[] weights)
        {
            int total = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                total += int.Parse(id[i].ToString()) * weights[i];
            }
            return total % 11 % 10;
        }

        /// <summary>
        /// Validate SNILS
        /// </summary>
        /// <param name="snils"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string snils)
        {
            snils = snils.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(snils, @"^[0-9]{11}$"))
            {
                return ValidationResult.InvalidFormat("12345678901");
            }
            var checkSum = int.Parse(snils.Substring(9));
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(snils[i].ToString()) * (9 - i);
            }

            bool isValid = (sum < 100 && sum == checkSum)
                || ((sum == 100 || sum == 101) && checkSum == 0)
                || (sum > 101 && (sum % 101 == checkSum || (sum % 101 == 100 && checkSum == 0)));

            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }



        /// <summary>
        /// Valiudate Beneficiary’s Bank BIK code
        /// </summary>
        /// <param name="bik"></param>
        /// <returns></returns>
        public ValidationResult ValidateBIK(string bik)
        {
            bik = bik.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(bik, @"^[0-9]{9}$"))
            {
                return ValidationResult.InvalidFormat("123456789");
            }

            var thirdPart = int.Parse(bik.Substring(6));
            if (thirdPart == 0 || thirdPart == 1 || thirdPart == 2)
            {
                return ValidationResult.Success();
            }
            // A range violation, not a failed checksum: the BIK carries no check digit at all, so
            // reporting one told the caller to look for a typo in a number that had none.
            bool isValid = thirdPart >= 50 && thirdPart < 1000;
            return isValid
                ? ValidationResult.Success()
                : ValidationResult.Invalid("Invalid code. The last three digits are outside the assigned range.");
        }

        /// <summary>
        /// Validate OGRN (Principle State Registration Number)
        /// </summary>
        /// <param name="ogrn"></param>
        /// <returns></returns>
        public ValidationResult ValidateOGRN(string ogrn)
        {
            ogrn = ogrn.RemoveSpecialCharacthers();
            if (!(Regex.IsMatch(ogrn, @"^[0-9]{13}$")))
            {
                // The hint showed nine digits for a thirteen digit number.
                return ValidationResult.InvalidFormat("1234567890123");
            }
            long checkSUm = long.Parse(ogrn.Substring(0, ogrn.Length - 1)) % 11;

            bool isValid = checkSUm % 10 == Char.GetNumericValue(ogrn[12]);
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Validate OGRNIP - Primary State Registration Number of an Individual Entrepreneur
        /// </summary>
        /// <param name="ogrnip"></param>
        /// <returns></returns>
        public ValidationResult ValidateOGRNIP(string ogrnip)
        {
            ogrnip = ogrnip.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ogrnip, @"^[0-9]{15}$"))
            {
                return ValidationResult.InvalidFormat("123456789012345");
            }
            ulong checksum = ulong.Parse(ogrnip.Substring(0, ogrnip.Length - 1)) % 13;
            bool isValid = checksum % 10 == ulong.Parse(ogrnip[14].ToString());
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
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
