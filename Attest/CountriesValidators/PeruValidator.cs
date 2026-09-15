using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class PeruValidator : IdValidationAbstract
    {

        public PeruValidator()
        {
            CountryCode = nameof(Country.PE);
        }


        private string CalculateChecksumNationalIdentity(string number)
        {
            int[] weights = new int[] { 3, 2, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(number[i]);
            }
            sum %= 11;
            return string.Format("{0}{1}", "65432110987"[sum], "KJIHGFEDCBA"[sum]);
        }

        /// <summary>
        /// Cédula Única de Identidad (CUI)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string number)
        {
            // python-stdnum upper cases before validating, so a lower case check letter
            // is accepted: https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/cui.py
            number = number.RemoveSpecialCharacthers().ToUpperInvariant();
            if (!(number.Length == 8 || number.Length == 9))
            {
                return ValidationResult.InvalidLength();
            }
            // [0-9] and not char.IsDigit: IsDigit also accepts non-ASCII Unicode digits.
            else if (!Regex.IsMatch(number.Substring(0, 8), "^[0-9]+$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }
            else if (number.Length > 8 && !CalculateChecksumNationalIdentity(number).Contains(number[number.Length - 1]))
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        // The first two digits of a RUC say who holds it, so a company number and a personal one
        // are told apart by them. SUNAT's own account of the structure, Section II of the OECD TIN
        // sheet it authored: "(a) Individuals identified with DNI: Prefix 10 + DNI + verification
        // digit. b) Individuals identified with another type of identity document: Prefix 15 +
        // random number + verification digit. c) Legal entities: Prefix 20 + random number +
        // verification digit."
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/peru-tin.pdf
        // "17" is not in that list, and python-stdnum accepts it as a RUC type without saying whose.
        // It sits with the natural persons: the Peruvian press and the RUC guides both describe 15
        // and 17 as the prefixes for an individual identified by a document other than the DNI, 17
        // being the persona natural no domiciliada. Nothing places it with legal entities, for which
        // SUNAT lists 20 alone. It was valid as both until now, which made every 17 ambiguous.
        // https://www.infobae.com/peru/2023/06/22/sunat-cuales-son-los-tipos-de-ruc-y-que-significan/
        // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/ruc.py
        private static readonly string[] _personTypes = new string[] { "10", "15", "17" };
        private static readonly string[] _entityTypes = new string[] { "20" };

        // IGV is filed under the RUC and Peru issues no separate VAT number, so every RUC is a
        // VAT identifier -- a sole trader's included. Delegating to ValidateEntity rejected the
        // personal forms, which is a false negative on a real registration.
        private static readonly string[] _vatTypes = new string[] { "10", "15", "17", "20" };

        /// <summary>
        /// RUC Peruvian company tax number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateRuc(id, _entityTypes);
        }

        private int CalculateChecksum(string number)
        {
            int[] weights = new int[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(number[i]);
            }
            sum %= 11;
            return (11 - sum).Mod(10);
        }

        /// <summary>
        /// RUC 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string number)
        {
            return ValidateRuc(number, _personTypes);
        }

        private ValidationResult ValidateRuc(string number, string[] allowedTypes)
        {
            number = number.RemoveSpecialCharacthers();
            if (number.Length != 11)
            {
                return ValidationResult.InvalidLength();
            }
            // [0-9] and not char.IsDigit: IsDigit also accepts non-ASCII Unicode digits.
            else if (!Regex.IsMatch(number, "^[0-9]{11}$"))
            {
                return ValidationResult.InvalidFormat("12345678901");
            }
            else if (!allowedTypes.Contains(number.Substring(0, 2)))
            {
                return ValidationResult.Invalid("Invalid taxpayer type");
            }
            // A char comparison, not EndsWith: String.EndsWith(string) folds with the thread's
            // culture, which is the defect CultureSweepTests exists for. The checksum is a single
            // digit (Mod(10)) and the format guard above has already fixed the length at 11.
            else if ((int)char.GetNumericValue(number[10]) != CalculateChecksum(number))
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Peru has no separate VAT number: IGV is filed under the RUC, so any RUC is accepted
        /// here, a natural person's included.
        /// </summary>
        /// <remarks>
        /// A personal RUC is therefore valid as a business identifier as well as a personal one,
        /// and <see cref="IdentifierResult.IsAmbiguous"/> says so. That is the country's doing
        /// rather than this library's: Peru issues one number for both roles and publishes no way
        /// to tell a VAT registration from a personal one.
        /// </remarks>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateRuc(vatId, _vatTypes);
        }

        /// <summary>Validates a postal code issued by Peru.</summary>
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
