using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class UruguayValidator : IdValidationAbstract
    {
        public UruguayValidator()
        {
            CountryCode = nameof(Country.UY);
        }

        /// <summary>
        /// Validate RUT numbers
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string rut)
        {
            return ValidateVAT(rut);
        }

        /// <summary>
        /// Validate RUT numbers
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string rut)
        {
            return ValidateVAT(rut);
        }


        /// <summary>
        /// Validate RUT numbers
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string rut)
        {
            rut = rut.RemoveSpecialCharacthers().ToUpperInvariant();
            // Only a "UY" prefix is stripped, not every occurrence: "2110034UY20017" is not a RUT.
            rut = rut.StripPrefix("UY");

            if (rut.Length != 12)
            {
                return ValidationResult.InvalidLength();
            }
            // [0-9] and not char.IsDigit: IsDigit also accepts non-ASCII Unicode digits,
            // which the int.Parse below rejects with a FormatException.
            else if (!Regex.IsMatch(rut, "^[0-9]{12}$"))
            {
                return ValidationResult.InvalidFormat("012345678901");
            }
            // Registration numbers run 01 to 22: DGI's resolution of 14/10/2024 gives every
            // taxpayer registered from 21 October 2024 the prefix 22 regardless of department,
            // while earlier registrations keep their 01-21 prefixes.
            // https://www.gub.uy/direccion-general-impositiva/comunicacion/noticias/nueva-numeracion-del-rut
            else if (int.Parse(rut.Substring(0, 2)) < 1 || int.Parse(rut.Substring(0, 2)) > 22)
            {
                return ValidationResult.Invalid("Invalid code");
            }
            else if (rut.Substring(2, 6) == "000000")
            {
                return ValidationResult.Invalid("Invalid code");
            }
            else if (rut.Substring(8, 3) != "001")
            {
                return ValidationResult.Invalid("Invalid code");
            }
            else if ((int)char.GetNumericValue(rut[rut.Length - 1]) != CalculateChecksum(rut.Substring(0, rut.Length - 1)))
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        private int CalculateChecksum(string number)
        {
            int[] weights = new[] { 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int total = 0;

            for (int i = 0; i < number.Length; i++)
            {
                total = total + weights[i] * (int)char.GetNumericValue(number[i]);
            }

            return (-total).Mod(11);

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
