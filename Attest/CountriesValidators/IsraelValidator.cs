using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Israel.</summary>
    public class IsraelValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Israel (IL).</summary>
        public IsraelValidator()
        {
            CountryCode = nameof(Country.IL);
        }

        /// <summary>Validates a company identifier issued by Israel.</summary>
        public override ValidationResult ValidateEntity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            // A company number is always exactly nine digits starting with 5, so zero padding a
            // shorter string only ever produced numbers that cannot be issued.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/il/hp.py
            if (ssn?.Length != 9 || !ssn.IsAsciiDigits())
            {
                return ValidationResult.Invalid("Invalid length. The code must have 9 digits");
            }

            if (!Regex.IsMatch(ssn, @"^5[0-9]{8}$"))
            {
                return ValidationResult.Invalid("For companies the first digit must be 5");
            }
            else if (!ssn.CheckLuhnDigit())
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        /// <summary>Validates a natural person's tax code.</summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (ssn?.Length != 9)
            {
                return ValidationResult.Invalid("Invalid length. The code must have 9 digits");
            }
            else if (!ssn.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("123456789");
            }

            int counter = 0;
            for (int i = 0; i < 9; i++)
            {
                int incNum = (int)char.GetNumericValue(ssn[i]);
                incNum *= (i % 2) + 1;
                if (incNum > 9)
                {
                    incNum -= 9;
                }

                counter += incNum;
            }
            bool isValid = counter % 10 == 0;
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Company Number
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

        /// <summary>Validates a postal code issued by Israel.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{7}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
