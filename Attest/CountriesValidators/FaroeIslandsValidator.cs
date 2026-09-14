using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class FaroeIslandsValidator : IdValidationAbstract
    {
        public FaroeIslandsValidator()
        {
            CountryCode = nameof(Country.FO);
        }


        /// <summary>
        /// V-number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{6}$"))
            {
                return ValidationResult.InvalidFormat("123 456");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// P-number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            // Section II of the Faroese TIN sheet gives the P number's format as "Ddmmyyxxx
            // (ddmmyy-xxx) 9 digits", so the first six digits are the date of birth: day 01-31
            // and month 01-12. The century is not published, so the year is not resolved and
            // 29 February is not checked against it.
            // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/faroe-islands-tin.pdf
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^(0[1-9]|[12][0-9]|3[01])(0[1-9]|1[0-2])[0-9]{5}$"))
            {
                return ValidationResult.InvalidFormat("ddmmyyxxx");
            }


            return ValidationResult.Success();

        }

        /// <summary>
        /// V-number
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            // Three digits running from 100 (Torshavn) to 970 (Sumba) with large unassigned gaps,
            // so only the impossible 000-099 block is rejected here.
            // https://da.wikipedia.org/wiki/Postnumre_p%C3%A5_F%C3%A6r%C3%B8erne
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[1-9][0-9]{2}$"))
            {
                return ValidationResult.InvalidFormat("NNN");
            }
            return ValidationResult.Success();
        }
    }
}
