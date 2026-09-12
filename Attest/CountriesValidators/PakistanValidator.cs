using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class PakistanValidator : IdValidationAbstract
    {

        public PakistanValidator()
        {
            CountryCode = nameof(Country.PK);
        }

        /// <summary>
        /// The FBR issues a National Tax Number to companies, but publishes no format or check
        /// rule for it, so there is no rule to apply here. CountryValidator answers the caller
        /// with Invalid("Not supported").
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Validate CNIC (Computerized National Identity Card)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();

            // 13 digits, first digit is the province code 1-7, last digit is the
            // gender digit and is never 0.
            // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.pk.cnic
            var isValid = Regex.IsMatch(id, "^[1-7][0-9]{11}[1-9]{1}$");
            if (isValid)
            {
                return ValidationResult.Success();
            }
            else
            {
                return ValidationResult.Invalid("Invalid format");
            }
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            throw new NotSupportedException();
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^\\d{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
