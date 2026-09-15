using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Guatemala.</summary>
    public class GuatemalaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Guatemala (GT).</summary>
        public GuatemalaValidator()
        {
            CountryCode = nameof(Country.GT);
        }

        /// <summary>The kinds GT has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.PersonalId | IdentifierKind.PersonalTaxCode; }
        }

        /// <summary>
        /// The check digit of a Guatemalan NIT, as a string, or an empty string for a null value.
        /// </summary>
        public string CalculateChecksum(string number)
        {
            if (number == null)
            {
                return string.Empty;
            }

            int sum = 0;

            char[] charArray = number.ToCharArray();
            Array.Reverse(charArray);
            number = new string(charArray);

            for (int i = 2; i <= number.Length + 1; i++)
            {
                sum = sum + (i * (int)char.GetNumericValue(number[i - 2]));
            }



            int c = (-sum).Mod(11);
            if (c == 10)
            {
                return "K";
            }
            else
            {
                return c.ToString();
            }

        }

        /// <summary>
        /// NIT (Número de Identificación Tributaria, Guatemala tax number)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            // stdnum's compact() upper cases the number and strips leading zeroes, so "576937-k"
            // and "00576937K" are the same NIT as "576937-K". Leading zeroes carry a weight in the
            // check sum but contribute nothing, so stripping them does not change the check digit.
            // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.gt.nit.html
            id = id.RemoveSpecialCharacthers().ToUpperInvariant().TrimStart('0');
            if (id.Length < 2 || id.Length > 12)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!id.Substring(0, id.Length - 1).IsAsciiDigits())
            {
                return ValidationResult.Invalid("Invalid format");
            }
            else if (id[id.Length - 1] != 'K' && !id[id.Length - 1].IsAsciiDigit())
            {
                return ValidationResult.Invalid("Invalid format");
            }
            else if (id[id.Length - 1].ToString() != CalculateChecksum(id.Substring(0, id.Length - 1)))
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();

        }

        /// <summary>
        /// Not supported: Guatemala has no separate personal tax code, and the NIT is validated by
        /// <see cref="ValidateEntity"/>.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidationResult.Invalid("Not supported");
        }

        /// <summary>
        /// NIT
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            // The VAT identifier is the NIT itself: two to twelve characters closed with a check
            // digit that may be K, not a fixed run of eight digits.
            // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.gt.nit.html
            return ValidateEntity(vatId);
        }

        /// <summary>Validates a postal code issued by Guatemala.</summary>
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
