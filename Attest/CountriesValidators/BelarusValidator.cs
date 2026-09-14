using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class BelarusValidator : IdValidationAbstract
    {
        // УНП (UNP) structure, see https://arthurdejong.org/git/python-stdnum/tree/stdnum/by/unp.py
        // and https://be.wikipedia.org/wiki/Уліковы_нумар_плацельшчыка :
        // 9 characters, either 9 digits (organisations and sole traders) or two Latin letters
        // followed by 7 digits (individuals). The first character is a region identifier and the
        // last one is a check digit.
        private const string LetterPositions = "ABCEHKMOPT";
        private const string RegionCodes = "1234567ABCEHKM";

        // The Cyrillic letters that are printed on Belarusian documents map onto their Latin
        // look-alikes, not onto their transliterations (В is B, not V; Н is H, not N; Р is P,
        // not R; С is C, not S), so IdExtensions.Translit() cannot be used here.
        private const string CyrillicLetters = "АВЕКМНОРСТ";
        private const string LatinLetters = "ABEKMHOPCT";

        public BelarusValidator()
        {
            CountryCode = nameof(Country.BY);
        }

        /// <summary>
        /// Payer's account number (UNP)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = Normalize(id);
            // Organisations and sole traders are numbered with 9 digits; only individuals get
            // the letter form.
            if (id.Length < 2 || !id.Substring(0, 2).IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("123456789");
            }
            return ValidateUNP(id);
        }

        /// <summary>
        /// Payer's account number (UNP)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string number)
        {
            // An individual holds the letter form, a sole trader the numeric one, so both are
            // accepted here.
            return ValidateUNP(Normalize(number));
        }

        /// <summary>
        ///  Payer's account number (UNP)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string number)
        {
            // The UNP doubles as the VAT number, in either form.
            return ValidateUNP(Normalize(number));
        }

        private ValidationResult ValidateUNP(string number)
        {
            if (number.Length != 9)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!number.Substring(2).IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("AA1234567");
            }
            else if (!number.Substring(0, 2).IsAsciiDigits()
                && !number.Substring(0, 2).All(c => LetterPositions.IndexOf(c) >= 0))
            {
                return ValidationResult.InvalidFormat("AA1234567");
            }
            else if (RegionCodes.IndexOf(number[0]) < 0)
            {
                return ValidationResult.Invalid("Invalid region code");
            }
            else if (CalculatChecksum(number) != number.Substring(number.Length - 1))
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        private string CalculatChecksum(string number)
        {
            number = number.ToUpperInvariant();
            string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int[] weights = new int[] { 29, 23, 19, 17, 13, 7, 5, 3 };
            if (!number.IsAsciiDigits())
            {
                number = string.Format("{0}{1}{2}", number[0], LetterPositions.IndexOf(number[1]), number.Substring(2));
            }
            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * alphabet.IndexOf(number[i]);
            }
            sum %= 11;
            if (sum > 9)
            {
                // No check digit exists for this base number, so it can never be valid.
                return string.Empty;
            }
            return sum.ToString();

        }

        private static string Normalize(string number)
        {
            number = (number ?? string.Empty)
                .ToUpperInvariant().StripPrefix("УНП").StripPrefix("UNP")
                .RemoveSpecialCharacthers();

            char[] characters = number.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                int index = CyrillicLetters.IndexOf(characters[i]);
                if (index >= 0)
                {
                    characters[i] = LatinLetters[index];
                }
            }
            return new string(characters);
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{6}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
