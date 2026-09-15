using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Faroe Islands.</summary>
    public class FaroeIslandsValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Faroe Islands (FO).</summary>
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

            // The day has to exist in the month it names. That much needs no century: 31 April and
            // 30 February are impossible in every one of them. 29 February is the only day that
            // depends on the year, so it stays accepted -- rejecting it would need the century the
            // sheet does not publish, and a false reject is the expensive direction here.
            int day = int.Parse(ssn.Substring(0, 2));
            int month = int.Parse(ssn.Substring(2, 2));
            int[] daysInMonth = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            if (day > daysInMonth[month - 1])
            {
                return ValidationResult.InvalidDate();
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

        /// <summary>Validates a postal code issued by Faroe Islands.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            // Three digits running from 100 (Torshavn) to 970 (Sumba); the published list ends
            // there, so 971-999 are not codes. The gaps inside the range are not encoded: the
            // assigned set is about 120 numbers and it moves, while the bounds do not.
            // https://da.wikipedia.org/wiki/Postnumre_p%C3%A5_F%C3%A6r%C3%B8erne
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[1-9][0-9]{2}$"))
            {
                return ValidationResult.InvalidFormat("NNN");
            }

            if (int.Parse(postalCode) > 970)
            {
                return ValidationResult.Invalid("No such postal code");
            }

            return ValidationResult.Success();
        }
    }
}
