using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Uzbekistan.</summary>
    public class UzbekistanValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Uzbekistan (UZ).</summary>
        public UzbekistanValidator()
        {
            CountryCode = nameof(Country.UZ);
        }

        /// <summary>The kinds UZ has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.Vat; }
        }

        /// <summary>
        /// Uzbekistan has no company identifier rule here, so every value is reported invalid -- which is
        /// not a verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>
        /// PINFL, the personal identification number of a natural person
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{14}$"))
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

        /// <summary>Validates a postal code issued by Uzbekistan.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{6}$"))
            {
                return ValidationResult.InvalidFormat("NNN NNN");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Uzbekistan has no VAT number rule here, so every value is reported invalid -- which is not a
        /// verdict on the value. Ask <see cref="CountryValidator.Supports"/> first.
        /// </summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidationResult.Invalid("Not supported");
        }
    }
}
