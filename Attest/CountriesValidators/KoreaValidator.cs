using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class KoreaValidator : IdValidationAbstract
    {
        public KoreaValidator()
        {
            CountryCode = nameof(Country.KR);
        }

        /// <summary>The kinds KR has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.Vat; }
        }
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");

        }

        /// <summary>
        /// Validate  Resident Registration Number (RRN)
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();

            if (!Regex.IsMatch(ssn, "^[0-9]{13}$"))
            {
                return ValidationResult.InvalidFormat("1234567890123");
            }

            string dateString;
            string sDigit; // parse the date into 'YYYYMMDD' according to 'S' digit

            sDigit = ssn.Substring(6, 1);
            string yearPrefix;
            switch (sDigit)
            {
                case "1":
                case "2":
                case "5":
                case "6":
                    yearPrefix = "19";
                    break;

                case "3":
                case "4":
                case "7":
                case "8":
                    yearPrefix = "20";
                    break;

                default:
                    yearPrefix = "18";
                    break;
            }


            dateString = yearPrefix + ssn.Substring(0, 6);
            // The RRN is issued at birth registration, not at 17, so any past birth date is
            // acceptable; python-stdnum kr.rrn rejects only dates in the future (allow_future=False).
            // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.kr.rrn
            if (DateTime.TryParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datetime))
            {
                if (datetime > DateTime.Now)
                {
                    return ValidationResult.InvalidDate();
                }
                else if (datetime < new DateTime(1860, 1, 1))
                {
                    return ValidationResult.InvalidDate();
                }
            }
            else
            {
                return ValidationResult.InvalidDate();
            }

            // Digits 8 and 9 are the place of birth registration; python-stdnum kr.rrn rejects
            // anything above 96 as an invalid component.
            if (int.Parse(ssn.Substring(7, 2)) > 96)
            {
                return ValidationResult.Invalid("Invalid place of birth");
            }

            string char6; int index;
            int remainder; int weightedSum;

            int[] weight = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 2, 3, 4, 5, 0 }; // add 0 for check digit

            weightedSum = 0;
            index = 0;

            for (int i = 0, len = ssn.Length; i < len; i++)
            {
                char6 = ssn[i].ToString();
                weightedSum += int.Parse(char6) * weight[index];
                index++;
            }

            remainder = (11 - weightedSum % 11) % 10 - +int.Parse(ssn.Substring(12));
            var isValid = remainder == 0;
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidationResult.Invalid("Not supported");
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
