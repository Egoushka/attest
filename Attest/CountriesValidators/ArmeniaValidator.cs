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
        /// TIN Number. The eight digit TIN is issued to entities and to individuals alike and has
        /// the same structure in both cases, so a company number cannot be told from a personal
        /// tax code. "TIN consists of 8 digits ... No meaning is given to the numbers."
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/armenia-tin.pdf
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateIndividualTaxCode(id);
        }

        /// <summary>
        /// Public services number (ՀԾՀ) - the ten digit number of a natural person, which is a
        /// different number from the eight digit TIN and is never issued to an entity.
        /// Composition per article 4 of the law on the public services number (HO-288-N,
        /// 30.11.2011, https://www.arlis.am/hy/acts/87872), which repeats the social security card
        /// number rule of government decision N 1783-N of 24.12.2003
        /// (https://www.arlis.am/hy/acts/36172): DDMMYYSSSC, where DD is the day of birth offset
        /// by the sex, MM the month offset by the century and C a check digit whose calculation
        /// the issuing body defines and does not publish.
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^[0-9]{10}$"))
            {
                return ValidationResult.InvalidFormat("1234567890");
            }

            // Digits 1-2 are the day of birth offset by 10 for men (11 is the 1st, 41 the 31st)
            // and by 50 for women (51 is the 1st, 81 the 31st).
            int day = int.Parse(ssn.Substring(0, 2));
            if ((day < 11 || day > 41) && (day < 51 || day > 81))
            {
                return ValidationResult.InvalidDate();
            }

            // Digits 3-4 are the month of birth shifted by 20 per century: 01-12 for the 20th,
            // 21-32 for the 21st, 41-52 for the 22nd, 61-72 for the 23rd and 81-92 for the 19th.
            // June is additionally coded 8 higher (14, 34, 54, 74, 94) so that the digit 6 never
            // lands here; the acts state the plain range as well, so both codings are accepted.
            int month = int.Parse(ssn.Substring(2, 2)) % 20;
            if ((month < 1 || month > 12) && month != 14)
            {
                return ValidationResult.InvalidDate();
            }

            // Digits 7-9 are a serial that runs 001-999, and the digit 6 may not occur three times
            // in a row anywhere in the number (article 4, part 3).
            if (ssn.Substring(6, 3) == "000" || ssn.Contains("666"))
            {
                return ValidationResult.Invalid("Invalid public services number");
            }

            return ValidationResult.Success();
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
            if (!Regex.IsMatch(ssn, @"^[0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// TIN Number
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateIndividualTaxCode(vatId);
        }


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
