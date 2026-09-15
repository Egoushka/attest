using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Italy.</summary>
    public class ItalyValidator : IdValidationAbstract
    {
        private static readonly string OmocodeChars = "LMNPQRSTUV";
        private static readonly int[] ControlCodeArray = new[] { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23 };
        private static readonly Regex CheckRegex = new Regex(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$");

        /// <summary>Creates a validator for Italy (IT).</summary>
        public ItalyValidator()
        {
            CountryCode = nameof(Country.IT);
        }

        /// <summary>Validates a natural person's tax code.</summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();

            if (string.IsNullOrEmpty(ssn) || ssn.Length < 16)
            {
                return ValidationResult.Invalid("Invalid length. The code must have 16 characters");
            }

            ssn = Normalize(ssn, false);
            if (!CheckRegex.Match(ssn).Success)
            {
                string nonOmocodeFC = ReplaceOmocodeChars(ssn);
                if (!CheckRegex.Match(nonOmocodeFC).Success)
                {
                    return ValidationResult.InvalidFormat("RCCMNL83S18D969H");
                }
            }
            bool isValid = ssn[15] == GetControlChar(ssn.Substring(0, 15));
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();

        }

        private string ReplaceOmocodeChars(string fc)
        {
            char[] fcChars = fc.ToCharArray();
            int[] pos = new[] { 6, 7, 9, 10, 12, 13, 14 };
            foreach (int i in pos) if (!Char.IsNumber(fcChars[i])) fcChars[i] = OmocodeChars.IndexOf(fcChars[i]).ToString()[0];
            return new string(fcChars);
        }

        private char GetControlChar(string f15)
        {
            int tot = 0;
            byte[] arrCode = Encoding.UTF8.GetBytes(f15.ToUpperInvariant());
            for (int i = 0; i < f15.Length; i++)
            {
                if ((i + 1) % 2 == 0) tot += (char.IsLetter(f15, i))
                    ? arrCode[i] - (byte)'A'
                    : arrCode[i] - (byte)'0';
                else tot += (char.IsLetter(f15, i))
                    ? ControlCodeArray[(arrCode[i] - (byte)'A')]
                    : ControlCodeArray[(arrCode[i] - (byte)'0')];
            }
            tot %= 26;
            char l = (char)(tot + 'A');
            return l;
        }

        private string Normalize(string s, bool normalizeDiacritics)
        {
            if (String.IsNullOrEmpty(s)) return s;
            s = s.Trim().ToUpperInvariant();
            if (normalizeDiacritics)
            {
                string src = "ÀÈÉÌÒÙàèéìòù";
                string rep = "AEEIOUAEEIOU";
                for (int i = 0; i < src.Length; i++) s = s.Replace(src[i], rep[i]);
                return s;
            }
            return s;
        }

        /// <summary>Validates a company identifier issued by Italy.</summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// VAT - Partita IVA  
        /// </summary>
        /// <param name="vat"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vat)
        {
            vat = vat.RemoveSpecialCharacthers();
            vat = vat.StripPrefix("IT");

            if (!Regex.IsMatch(vat, @"^[0-9]{11}$"))
            {
                return ValidationResult.InvalidFormat("12345678901");
            }

            int[] Multipliers = { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };

            if (int.TryParse(vat.Substring(0, 7), out int res))
            {
                if (res == 0)
                {
                    return ValidationResult.Invalid("Invalid format");
                }
            }
            else
            {
                return ValidationResult.Invalid("Invalid format");
            }

            var temp = int.Parse(vat.Substring(7, 3));

            if ((temp < 1 || temp > 201) && temp != 999 && temp != 888)
            {
                return ValidationResult.Invalid("Invalid");
            }

            var index = 0;
            var sum = 0;
            foreach (var m in Multipliers)
            {
                temp = vat[index++].ToInt() * m;
                sum += temp > 9
                    ? (int)Math.Floor(temp / 10D) + temp % 10
                    : temp;
            }

            var checkDigit = 10 - sum % 10;

            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[10].ToInt();
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>Validates a postal code issued by Italy.</summary>
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
