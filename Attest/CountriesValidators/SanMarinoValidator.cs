using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class SanMarinoValidator : IdValidationAbstract
    {
        public SanMarinoValidator()
        {
            CountryCode = nameof(Country.SM);
        }

        private readonly int[] _notAvailableNumbers = new int[]
        {
             2, 4, 6, 7, 8, 9, 10, 11, 13, 16, 18, 19, 20, 21, 25, 26, 30, 32, 33, 35,
             36, 37, 38, 39, 40, 42, 45, 47, 49, 51, 52, 55, 56, 57, 58, 59, 61, 62,
             64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 79, 80, 81, 84, 85,
             87, 88, 91, 92, 94, 95, 96, 97, 99
        };



        /// <summary>
        /// COE (Codice operatore economico, San Marino national tax number).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }


        /// <summary>
        /// SSI (Social Security Number), the tax number of individuals: a block of up to 9 digits,
        /// whose first digits may be omitted when they are zeros.
        /// https://www.oecd.org/tax/automatic-exchange/crs-implementation-and-assistance/tax-identification-numbers/San-Marino-TIN.pdf
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            if (ValidateVAT(id).IsValid)
            {
                return ValidationResult.Success();
            }

            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^\d{1,9}$"))
            {
                return ValidationResult.InvalidFormat("123456789");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// COE (Codice operatore economico, San Marino national tax number)
        /// </summary>
        /// <param name="coe"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string coe)
        {
            coe = coe.RemoveSpecialCharacthers();
            coe = coe.StripPrefix("SM");

            if (coe.Length > 5 || coe.Length == 0)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!coe.All(char.IsDigit))
            {
                return ValidationResult.InvalidFormat("1234");
            }
            else if (coe.Length < 3 && !_notAvailableNumbers.Contains(int.Parse(coe)))
            {
                return ValidationResult.Invalid("Invalid code");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^4789\\d$"))
            {
                return ValidationResult.InvalidFormat("4789N");
            }
            return ValidationResult.Success();
        }
    }
}
