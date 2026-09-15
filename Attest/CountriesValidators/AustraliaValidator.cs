using System.Linq;
using System.Text.RegularExpressions;


namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Australia.</summary>
    public class AustraliaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Australia (AU).</summary>
        public AustraliaValidator()
        {
            CountryCode = nameof(Country.AU);
        }

        /// <summary>
        /// Validate ABN, ACN or TFN
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {

            if (ValidateABN(id).IsValid)
            {
                return ValidationResult.Success();
            }
            else if (ValidateACN(id).IsValid)
            {
                return ValidationResult.Success();
            }
            else if (ValidateTFN(id).IsValid)
            {
                return ValidationResult.Success();

            }
            return ValidationResult.Invalid("Invalid");
        }

        /// <summary>
        /// Validate TFN
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            return ValidateTFN(id);
        }

        /// <summary>
        /// Validate ABN  
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string number)
        {
            return ValidateABN(number);
        }


        /// <summary>
        /// Validates a TFN (Tax File Number), the eight or nine digit number the ATO issues to a
        /// person or an entity.
        /// </summary>
        public ValidationResult ValidateTFN(string number)
        {
            number = number.RemoveSpecialCharacthers();

            if (!number.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("12345678 or 123456789");
            }
            if (!(number.Length == 8 || number.Length == 9))
            {
                return ValidationResult.InvalidLength();
            }
            if (CheckSumTFN(number) != 0)
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();

        }

        private int CheckSumTFN(string number)
        {
            int[] weights = new int[] { 1, 4, 3, 7, 5, 8, 6, 9, 10 };
            int sum = 0;
            for (int i = 0; i < number.Length; i++)
            {
                sum += ((int)char.GetNumericValue(number[i])) * weights[i];
            }

            return sum % 11;
        }

        /// <summary>
        /// Validates an ABN (Australian Business Number): eleven digits whose weighted sum is a
        /// multiple of 89 once 1 is subtracted from the first.
        /// </summary>
        public ValidationResult ValidateABN(string number)
        {
            number = number.RemoveSpecialCharacthers();
            if (number?.Length != 11)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!number.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("12345678901");
            }

            return IsValidABNChecksum(number) ?
            ValidationResult.Success()
                : ValidationResult.InvalidChecksum();

        }


        /// <summary>
        /// Validates an ACN (Australian Company Number), the nine digit number ASIC issues to a
        /// company. An ABN for that company embeds it.
        /// </summary>
        public ValidationResult ValidateACN(string number)
        {
            number = number.RemoveSpecialCharacthers();
            if (number.Length != 9)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!number.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("123456789");
            }
            if (CalculateACNChecksum(number) != number[number.Length - 1])
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        private char CalculateACNChecksum(string number)
        {
            int sum = 0;

            for (int i = 0; i < number.Length - 1; i++)
            {
                sum += ((int)char.GetNumericValue(number[i])) * (8 - i);
            }

            int rem = 10 - sum % 10;
            if (rem == 10)
            {
                return '0';
            }
            return rem.ToString()[0];
        }

        private bool IsValidABNChecksum(string number)
        {
            int s = 0;
            int[] weights = new int[] { 10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
            for (int i = 0; i < number.Length; i++)
            {
                if (i == 0)
                {
                    s += weights[i] * ((int)char.GetNumericValue(number[i]) - 1);
                }
                else
                {
                    s += weights[i] * ((int)char.GetNumericValue(number[i]));
                }
            }

            return s % 89 == 0;
        }

        /// <summary>Validates a postal code issued by Australia.</summary>
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
