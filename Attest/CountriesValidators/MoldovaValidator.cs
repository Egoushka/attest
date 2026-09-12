using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class MoldovaValidator : IdValidationAbstract
    {
        public MoldovaValidator()
        {
            CountryCode = nameof(Country.MD);
        }

        public override ValidationResult ValidateEntity(string number)
        {
            number = number.RemoveSpecialCharacthers();
            if (!number.All(char.IsDigit))
            {
                return ValidationResult.InvalidFormat("1234567890123");
            }
            else if (number.Length != 13)
            {
                return ValidationResult.InvalidLength();
            }
            else if ((int)char.GetNumericValue((number[number.Length - 1])) != CalculateChecksum(number.Substring(0, number.Length - 1)))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// IDNP (Identification Number of Person)
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^\d{13}$"))
            {
                return ValidationResult.InvalidFormat("1234567890123");
            }
            // The IDNP is the same thirteen digit state identifier as the IDNO; only the leading
            // registry digit differs (1 legal entity, 2 natural person, 3 vehicle), so it closes
            // with the same check digit over the weights 7,3,1 repeated, modulus 10.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/md/idno.py
            // https://github.com/iAsig/idnx-validator/blob/main/src/index.ts
            else if (ssn[ssn.Length - 1].ToInt() != CalculateChecksum(ssn.Substring(0, ssn.Length - 1)))
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Validate VAT code (Nr. de Inregistrare TVA)
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            // Seven digits, optionally prefixed with MD, for example MD9234564.
            // https://www.vatify.eu/moldova-vat-number.html
            vatId = vatId.RemoveSpecialCharacthers().ToUpper().Replace("MD", string.Empty);
            if (!Regex.IsMatch(vatId, @"^\d{7}$"))
            {
                return ValidationResult.InvalidFormat("1234567");
            }
            return ValidationResult.Success();

        }

        private int CalculateChecksum(string number)
        {
            number = number.RemoveSpecialCharacthers();
            int[] weights = new int[] { 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1 };

            int sum = 0;
            for (int i = 0; i < number.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(number[i]);
            }

            return sum % 10;
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[Mm][Dd][-]{0,1}\\d{4}$"))
            {
                return ValidationResult.InvalidFormat("CCNNNN CC-NNNN");
            }
            return ValidationResult.Success();
        }
    }
}
