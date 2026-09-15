using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Venezuela.</summary>
    public class VenezuelaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Venezuela (VE).</summary>
        public VenezuelaValidator()
        {
            CountryCode = nameof(Country.VE);
        }
        /// <summary>
        /// Validates a company identifier issued by Venezuela: Registro de Informacion Fiscal (RIF).
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// Validates a natural person's tax code, which here is the same number
        /// <see cref="ValidateVAT"/> validates.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// Registro de Informacion Fiscal (RIF) 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string id)
        {
            id = id.RemoveSpecialCharacthers();
            id = id?.StripPrefix("VE");

            if (id.Length != 10)
            {
                return ValidationResult.Invalid("Invalid length");
            }
            else if (!Regex.IsMatch(id, "^[VEJPG][0-9]{9}$"))
            {
                return ValidationResult.InvalidFormat("[VEJPG]123456789");
            }

            Dictionary<char, int> types = new Dictionary<char, int>
            {
                { 'V',4},//natural person born in Venezuela
                { 'E', 8},//foreign natural person
                { 'J', 12},//company
                { 'P',16},//passport
                { 'G', 20}//government
            };

            var sum = types[id[0]];
            var weight = new int[] { 3, 2, 7, 6, 5, 4, 3, 2 };

            for (var i = 0; i < 8; i++)
            {
                sum += int.Parse(id[i + 1].ToString()) * weight[i];
            }

            sum = 11 - sum % 11;
            if (sum == 11 || sum == 10)
            {
                sum = 0;
            }

            bool isValid = sum.ToString() == id.Substring(9, 1);
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>Validates a postal code issued by Venezuela.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            // Four digits plus the optional letter that marks a sub area, as in 1010-A. The
            // separator is already gone by this point, so the pattern must not ask for one.
            if (!Regex.IsMatch(postalCode, "^[0-9]{4}[a-zA-Z]?$"))
            {
                return ValidationResult.InvalidFormat("NNNN or NNNN A");
            }
            return ValidationResult.Success();
        }
    }
}
