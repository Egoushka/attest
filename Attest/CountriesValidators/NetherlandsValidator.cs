using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Netherlands.</summary>
    public class NetherlandsValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Netherlands (NL).</summary>
        public NetherlandsValidator()
        {
            CountryCode = nameof(Country.NL);
        }
        /// <summary>Validates a company identifier issued by Netherlands.</summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// Validates a national identification number issued by Netherlands: Burgerservicenummer (BSN) -
        /// Citizen Service Number or Onderwijsnummer.
        /// </summary>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ValidationResult result = ValidateIndividualTaxCode(ssn);
            if (result.IsValid)
            {
                return result;
            }
            else
            {
                if (ValidateOnderwijsnummer(ssn).IsValid)
                {
                    return ValidationResult.Success();
                }
                return result;
            }
        }

        private int CheckSum(string number)
        {
            int sum = 0;
            for (int i = 0; i < number.Length - 1; i++)
            {
                sum += (9 - i) * (int)char.GetNumericValue(number[i]);
            }

            return (sum - (int)char.GetNumericValue(number[number.Length - 1])).Mod(11);
        }

        /// <summary>
        /// Burgerservicenummer (BSN) - Citizen Service Number
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string number)
        {
            number = number.RemoveSpecialCharacthers();
            if (!(number.IsAsciiDigits() || !int.TryParse(number, out var parsedNum) || parsedNum <= 0 ))
            {
                return ValidationResult.Invalid("Invalid format. Only digits are allowed");
            }
            else if (number.Length != 9)
            {
                return ValidationResult.InvalidLength();
            }
            else if (CheckSum(number) != 0)
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Onderwijsnummer (the Dutch student identification number for students without BSN).
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ValidationResult ValidateOnderwijsnummer(string number)
        {
            number = number.RemoveSpecialCharacthers();

            if (!number.IsAsciiDigits() || !int.TryParse(number, out var parsedNum) || parsedNum <= 0)
            {
                return ValidationResult.InvalidFormat("1034.56.789");
            }
            else if (!number.StartsWith("10", StringComparison.Ordinal))
            {
                return ValidationResult.InvalidFormat("1034.56.789");
            }
            else if (number.Length != 9)
            {
                return ValidationResult.InvalidLength();
            }
            else if (Checksum(number) != 5)
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        private int Checksum(string number)
        {
            int sum = 0;
            for (int i = 0; i < number.Length - 1; i++)
            {
                sum += (9 - i) * (int)Char.GetNumericValue(number[i]);
            }
            return (sum - (int)Char.GetNumericValue(number[number.Length - 1])).Mod(11);
        }

        /// <summary>
        /// Omzetbelastingnummer (BTW)  
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {

            vatId = vatId.RemoveSpecialCharacthers();
            vatId = vatId?.StripPrefix("NL");

            if (!Regex.IsMatch(vatId, @"^[0-9]{9}B[0-9]{2}$"))
            {
                return ValidationResult.Invalid("Invalid format");
            }

            int[] multipliers = { 9, 8, 7, 6, 5, 4, 3, 2 };
            var sum = vatId.Sum(multipliers);

            // Two checksums are in use: the legacy BSN derived mod 11 btw-nummer, and since
            // 2020-01-01 the btw-identificatienummer issued to sole proprietorships, which carries
            // ISO 7064 MOD 97-10 over "NL" + the number instead.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/nl/btw.py
            // "2321" is NL and "11" is B under the A..Z -> 10..35 mapping used by MOD 97-10.
            bool isValid = sum % 11 == vatId[8].ToInt()
                || long.Parse("2321" + vatId.Substring(0, 9) + "11" + vatId.Substring(10)) % 97 == 1;

            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>Validates a postal code issued by Netherlands.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers().ToUpperInvariant();
            if (!Regex.IsMatch(postalCode, "^[0-9]{4}[A-Z]{2}$"))
            {
                return ValidationResult.InvalidFormat("NNNN WW");
            }
            return ValidationResult.Success();
        }
    }
}
