using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class TaiwanValidator : IdValidationAbstract
    {
        public TaiwanValidator()
        {
            CountryCode = nameof(Country.TW);
        }
        /// <summary>
        /// Unified Business Number (統一編號): eight digits weighted 1,2,1,2,1,2,4,1, where the
        /// digits of every product are added together. Since 2023-04-01 the Ministry of Finance
        /// requires that total to be divisible by 5 instead of 10 to widen the pool of free
        /// numbers; when the seventh digit is 7 the total may also be one short of a multiple.
        /// https://www.mof.gov.tw/singlehtml/384fb3077bb349ea973e7fc6f13b6974?cntId=8d164b10f20042b9ab9864b51b20f0c2
        /// https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.tw.ubn.html
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, "^[0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNNNN");
            }

            int[] weights = { 1, 2, 1, 2, 1, 2, 4, 1 };
            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                int product = id[i].ToInt() * weights[i];
                sum += product / 10 + product % 10;
            }

            int remainder = sum % 5;
            bool isValid = remainder == 0 || (remainder == 4 && id[6] == '7');
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Validate ssn for locals and residents
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();

            if (Regex.IsMatch(ssn, "^[A-Z][12][0-9]{8}$"))
            {
                return ValidateLocalSSN(ssn);
            }
            else if (Regex.IsMatch(ssn, "^[A-Z][A-D89][0-9]{8}$"))
            {
                return ValidateResidentSSN(ssn);
            }
            return ValidationResult.Invalid("Invalid format");
        }

        /// <summary>
        /// Validate local SSN
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public ValidationResult ValidateLocalSSN(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, "^[A-Z][12][0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("A123456789");
            }

            int idLen = ssn.Length;
            string letters = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            int letterIndex = letters.IndexOf(ssn[0]);
            decimal weightedSum = Math.Floor((decimal)letterIndex / 10 + 1) + letterIndex * (idLen - 1);
            string idTail = ssn.Substring(1);

            int weight = idLen - 2;

            for (int i = 0, len = idTail.Length; i < len; i++)
            {
                string _char2 = idTail[i].ToString();
                weightedSum += int.Parse(_char2) * weight;
                weight--;
            }

            decimal remainder = (weightedSum + int.Parse(ssn.Substring(9))) % 10;
            bool isValid = remainder == 0;
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();

        }

        /// <summary>
        /// Validate ssn of a resident
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public ValidationResult ValidateResidentSSN(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, "^[A-Z][A-D89][0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("AB12345677");
            }

            int idLen = ssn.Length;

            string letters = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            int letterIndex = letters.IndexOf(ssn[0]);
            decimal weightedSum = Math.Floor((decimal)letterIndex / 10 + 1) + letterIndex * (idLen - 1);
            // Resident certificates issued from 2021-01-02 carry one letter and nine digits, the
            // second being 8 (male) or 9 (female) and entering the sum as a plain digit; the
            // pre-2021 form has a second letter A-D worth its position in the table above, which
            // is the units digit of its code (A=10 -> 0 ... D=13 -> 3).
            // https://www.cna.com.tw/news/asoc/202012160106.aspx (worked example A800000014)
            weightedSum += (ssn[1].IsAsciiDigit() ? ssn[1].ToInt() : letters.IndexOf(ssn[1])) * (idLen - 2);
            string idTail = ssn.Substring(2);

            int weight = idLen - 3;

            for (int i = 0, len = idTail.Length; i < len; i++)
            {
                string _char3 = idTail[i].ToString();
                weightedSum += int.Parse(_char3) * weight;
                weight--;
            }

            decimal remainder = (weightedSum + int.Parse(ssn.Substring(9))) % 10;
            bool isValid = remainder == 0;
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();

        }

        /// <summary>
        /// Business tax (VAT) is charged against the same Unified Business Number.
        /// https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.tw.ubn.html
        /// </summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            // The three digit district code stands on its own; Chunghwa Post extended the
            // delivery segment from two to three digits on 2020-03-03 and all three lengths
            // remain in use. https://www.cna.com.tw/news/firstnews/202003020346.aspx
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{3}([0-9]{2,3})?$"))
            {
                return ValidationResult.InvalidFormat("NNN, NNNNN or NNNNNN");
            }

            // The district code runs 100 (Taipei) to 983 (Hualien county, where zone 9 ends), so
            // 000-099 and 984-999 are not codes at all. The assigned set inside that range is full
            // of holes -- 368 of the 884 numbers are in use -- and those are deliberately not
            // encoded: districts merge and the list drifts, while the bounds do not.
            // https://en.wikipedia.org/wiki/Postal_codes_in_Taiwan
            int district = int.Parse(postalCode.Substring(0, 3));
            if (district < 100 || district > 983)
            {
                return ValidationResult.Invalid("No such postal district");
            }

            return ValidationResult.Success();
        }
    }
}
