using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class BelgiumValidator : IdValidationAbstract
    {
        public BelgiumValidator()
        {
            CountryCode = nameof(Country.BE);
        }

        /// <summary>
        /// BTW, TVA, NWSt, ondernemingsnummer (Belgian enterprise number).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        private int ModFunction(long nr)
        {
            return (int)(97 - (nr % 97));
        }

        /// <summary>
        /// Rijksregisternummer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^\d{11}$"))
            {
                return ValidationResult.InvalidFormat("12345678901");
            }
            // Parsed as a number, not compared as text: check numbers below 10 are written with a
            // leading zero ("01"), which never equals the computed "1".
            var checkDigit = int.Parse(id.Substring(id.Length - 2));

            var nrToCheck = long.Parse(id.Substring(0, 9));

            if (ModFunction(nrToCheck) == checkDigit)
            {
                return ValidationResult.Success();
            }

            // People born from 2000 on get a 2 in front of the nine digits before the modulo
            nrToCheck = long.Parse('2' + id.Substring(0, 9));

            bool isValid = ModFunction(nrToCheck) == checkDigit;
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// BTW, TVA, NWSt, ondernemingsnummer (Belgian enterprise number).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string id)
        {
            id = id.RemoveSpecialCharacthers();
            id = id.StripPrefix("BE");

            if (id.Length == 9)
            {
                id = id.PadLeft(10, '0');
            }

            if (!Regex.IsMatch(id, @"^[0-1]?\d{9}$"))
            {
                return ValidationResult.InvalidFormat("1234567890");
            }


            var isValid = 97 - int.Parse(id.Substring(0, 8)) % 97 == int.Parse(id.Substring(8, 2));
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
