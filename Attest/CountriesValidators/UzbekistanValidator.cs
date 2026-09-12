using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class UzbekistanValidator : IdValidationAbstract
    {
        public UzbekistanValidator()
        {
            CountryCode = nameof(Country.UZ);
        }

        public override ValidationResult ValidateEntity(string id)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// PINFL, the personal identification number of a natural person
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^\d{14}$"))
            {
                return ValidationResult.InvalidFormat("12345678901234");
            }

            // The fourteenth digit closes the first thirteen: "контрольная цифра (позиция 14)
            // рассчитывается по модулю 10 с постоянно повторяющейся весовой функцией 731 731",
            // Cabinet of Ministers regulation no. 177 of 12.04.2022, chapter 2, group 5.
            // https://lex.uz/ru/docs/5955669
            int[] weights = { 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1, 7 };

            return id.Sum(weights) % 10 == id[13].ToInt()
                ? ValidationResult.Success()
                : ValidationResult.InvalidChecksum();
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^\\d{6}$"))
            {
                return ValidationResult.InvalidFormat("NNN NNN");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            throw new NotSupportedException();
        }
    }
}
