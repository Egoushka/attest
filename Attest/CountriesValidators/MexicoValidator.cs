using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class MexicoValidator : IdValidationAbstract
    {
        public MexicoValidator()
        {
            CountryCode = nameof(Country.MX);
        }

        private static int DigitVerification(string curp17)
        {
            var dictionary = "0123456789ABCDEFGHIJKLMNÑOPQRSTUVWXYZ";
            var sum = 0;

            for (var i = 0; i < 17; i++)
            {
                sum = sum + dictionary.IndexOf(curp17[i]) * (18 - i);
            }

            int checkDigit = 10 - sum % 10;
            if (checkDigit == 10)
                return 0;

            return checkDigit;
        }


        /// <summary>
        /// Mexico Unique Population Registry Code - CURP
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            var match = Regex.Match(ssn, @"^([A-Z][AEIOUX][A-Z]{2}[0-9]{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12][0-9]|3[01])[HM](?:AS|B[CS]|C[CLMSH]|D[FG]|G[TR]|HG|JC|M[CNS]|N[ETL]|OC|PL|Q[TR]|S[PLR]|T[CSL]|VZ|YN|ZS)[B-DF-HJ-NP-TV-Z]{3}[A-Z0-9])([0-9])$");

            if (!match.Success)
            {
                return ValidationResult.Invalid("Invalid format");
            }

            if (match.Groups[2].Value != DigitVerification(match.Groups[1].Value).ToString())
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// RFC (Registro Federal de Contribuyentes, Mexican tax number)
        /// </summary>
        /// <param name="rfc"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string rfc)
        {
            rfc = rfc.RemoveSpecialCharacthers();

            if (rfc.Length == 12) //# number assigned to company
            {
                if (!Regex.IsMatch(rfc, "^[A-Z&Ñ]{3}[0-9]{6}[0-9A-Z]{3}$"))
                {
                    return ValidationResult.Invalid("Invalid format");
                }
                else if (!HasValidDate(rfc.Substring(3, 6)))
                {
                    return ValidationResult.InvalidDate();
                }
            }
            else
            {
                return ValidationResult.InvalidLength();
            }

            if (!Regex.IsMatch(rfc.Substring(rfc.Length - 3), @"^[1-9A-V][1-9A-Z][0-9A]$"))
            {
                return ValidationResult.Invalid("Invalid");
            }
            else if (rfc[rfc.Length - 1] != CalculateChecksum(rfc.Substring(0, rfc.Length - 1)))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        private char CalculateChecksum(string number)
        {
            string alphabet = "0123456789ABCDEFGHIJKLMN&OPQRSTUVWXYZ Ñ";
            number = ("   " + number);
            number = number.Substring(number.Length - 12);


            int sum = 0;
            for (int i = 0; i < number.Length; i++)
            {
                sum += alphabet.IndexOf(number[i]) * (13 - i);
            }

            return alphabet[(11 - sum).Mod(11)];
        }
        /// <summary>
        /// Checks the six digit YYMMDD component of an RFC. The two digit year is resolved
        /// against the 2000s, as python-stdnum's mx.rfc does: the century only changes the
        /// answer for 29 February of a century year, and 2000 is a leap year while 1900 is not.
        /// https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/mx/rfc.py
        /// </summary>
        private bool HasValidDate(string yymmdd)
        {
            return DateTime.TryParseExact("20" + yymmdd, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        /// <summary>
        /// RFC (Registro Federal de Contribuyentes, Mexican tax number)
        /// </summary>
        /// <param name="rfc"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string rfc)
        {
            string[] name_blacklist = new string[] {
                "BUEI", "BUEY", "CACA", "CACO", "CAGA", "CAGO", "CAKA", "CAKO", "COGE",
                "COJA", "COJE", "COJI", "COJO", "CULO", "FETO", "GUEY", "JOTO", "KACA",
                "KACO", "KAGA", "KAGO", "KAKA", "KOGE", "KOJO", "KULO", "MAME", "MAMO",
                "MEAR", "MEAS", "MEON", "MION", "MOCO", "MULA", "PEDA", "PEDO", "PENE",
                "PUTA", "PUTO", "QULO", "RATA", "RUIN"
            };

            rfc = rfc.RemoveSpecialCharacthers();



            if (rfc.Length == 10 || rfc.Length == 13)//# number assigned to person
            {
                if (!Regex.IsMatch(rfc, @"^[A-Z&Ñ]{4}[0-9]{6}[0-9A-Z]{0,3}$"))
                {
                    return ValidationResult.Invalid("Invalid format");
                }
                else if (name_blacklist.Contains(rfc.Substring(0, 4)))
                {
                    return ValidationResult.Invalid("Name is blacklisted");
                }
                else if (!HasValidDate(rfc.Substring(4, 6)))
                {
                    return ValidationResult.InvalidDate();
                }
            }
            else
            {
                return ValidationResult.InvalidLength();
            }

            if (rfc.Length >= 12)
            {
                if (!Regex.IsMatch(rfc.Substring(rfc.Length - 3), @"^[1-9A-V][1-9A-Z][0-9A]$"))
                {
                    return ValidationResult.Invalid("Invalid");
                }
                else if (rfc[rfc.Length - 1] != CalculateChecksum(rfc.Substring(0, rfc.Length - 1)))
                {
                    return ValidationResult.InvalidChecksum();
                }
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
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
