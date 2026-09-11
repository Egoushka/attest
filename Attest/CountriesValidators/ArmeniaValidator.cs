using System.Text.RegularExpressions;


namespace Attest.Countries
{
    public class ArmeniaValidator : IdValidationAbstract
    {
        public ArmeniaValidator()
        {
            CountryCode = nameof(Country.AM);
        }

        /// <summary>
        /// TIN Number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateIndividualTaxCode(id);
        }

        /// <summary>
        /// TIN Number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            // The ՀՎՀՀ is eight digits: a seven digit serial number and a check digit whose
            // algorithm is not published.
            // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/armenia-tin.pdf
            if (!Regex.IsMatch(ssn, @"^\d{8}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// TIN Number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateIndividualTaxCode(vatId);
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
