using System;
using System.Text.RegularExpressions;

namespace CountryValidation.Countries
{
    public class ThailandValidator : IdValidationAbstract
    {
        public ThailandValidator()
        {
            CountryCode = nameof(Country.TH);
        }

        /// <summary>
        /// Validate Thailand citizen number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^\d{13}$"))
            {
                return ValidationResult.InvalidLength();
            }

            var sum = 0;
            for (var i = 0; i < 12; i++)
            {
                sum += (int)Char.GetNumericValue(ssn[i]) * (13 - i);
            }

            return (11 - sum % 11).Mod(10) == (int)char.GetNumericValue(ssn[12]) ? ValidationResult.Success() : ValidationResult.InvalidChecksum();

        }

        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateIndividualTaxCode(id);
        }

        /// <summary>
        /// Thai tax identification numbers are 13 digits since 1 February 2012. Individuals use the
        /// identification number issued by the Ministry of Interior, juristic persons the registration
        /// number issued by the Ministry of Commerce; both carry the same check digit.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/thailand-tin.pdf
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateNationalIdentity(ssn);
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateIndividualTaxCode(vatId);
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
