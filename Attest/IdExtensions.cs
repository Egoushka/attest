using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attest
{
    public static class IdExtensions
    {
        public static IEnumerable<int> ToDigitEnumerable(this int number)
        {
            IList<int> digits = new List<int>();

            while (number > 0)
            {
                digits.Add(number % 10);
                number = number / 10;
            }

            //digits are currently backwards, reverse the order
            return digits.Reverse();
        }

        /// <summary>
        /// Strips separators and punctuation, keeping letters and ASCII digits. Null becomes an
        /// empty string, which every validator then rejects on format or length.
        /// </summary>
        /// <remarks>
        /// Digits outside 0-9 are dropped rather than kept. .NET's regex digit class and
        /// char.IsDigit both match every Unicode decimal digit - Arabic-Indic, Devanagari,
        /// fullwidth - while int.Parse accepts only ASCII, so a validator that guarded with one and
        /// parsed with the other threw FormatException on input no identifier scheme allows.
        /// Letters are kept whatever their script: a Belarusian UNP is written with Cyrillic
        /// characters that BelarusValidator maps to their Latin look-alikes.
        /// </remarks>
        /// <summary>
        /// Stands in for a decimal digit outside 0-9. Matches no format check in this library:
        /// not a letter, not a digit, outside every character class the validators use.
        /// </summary>
        private const char NotAnIdentifierCharacter = '\uFFFD';

        public static string RemoveSpecialCharacthers(this string ssn)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < ssn?.Length; i++)
            {
                if (char.IsLetter(ssn[i]) || (ssn[i] >= '0' && ssn[i] <= '9'))
                {
                    sb.Append(ssn[i]);
                }
                else if (char.IsDigit(ssn[i]))
                {
                    // A digit outside 0-9. Kept as a character no format check accepts, rather
                    // than dropped: dropping it would validate the remaining digits and turn a
                    // wrong number into a right one.
                    sb.Append(NotAnIdentifierCharacter);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Whether every character is an ASCII digit. An empty string is vacuously true, which is
        /// what <c>All(char.IsDigit)</c> returned; a null value is false rather than a throw.
        /// </summary>
        /// <remarks>
        /// char.IsDigit matches every Unicode decimal digit — Arabic-Indic, Devanagari, fullwidth —
        /// while int.Parse accepts only ASCII. A guard written with one and a parse written with the
        /// other either throws or, where char.GetNumericValue follows instead, silently validates a
        /// number no register ever issued.
        /// </remarks>
        internal static bool IsAsciiDigits(this string value)
        {
            if (value == null)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!value[i].IsAsciiDigit())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Whether the character is one of 0-9, and not merely a Unicode decimal digit.</summary>
        internal static bool IsAsciiDigit(this char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Removes <paramref name="prefix"/> from the start of the value, and only from the start.
        /// </summary>
        /// <remarks>
        /// A country code is a prefix. Stripping it with String.Replace removed it wherever it
        /// appeared, so "76086CL4285" normalised to the Chilean RUT 76086428-5 and validated: a
        /// number nobody was ever issued, accepted because two of its characters spelled the
        /// country. The comparison is ordinal so that it does not depend on the thread culture,
        /// and case-insensitive because a prefix is written either way on an invoice.
        /// </remarks>
        internal static string StripPrefix(this string value, string prefix)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? value.Substring(prefix.Length)
                : value;
        }

        public static int ToInt(this char c)
        {
            return Convert.ToInt32(c) - Convert.ToInt32('0');
        }



        public static int Sum(this string input, int[] multipliers, int start = 0)
        {
            var sum = 0;

            for (var index = start; index < multipliers.Length; index++)
            {
                var digit = multipliers[index];
                sum += input[index].ToInt() * digit;
            }

            return sum;
        }

        public static string Slice(this string input, int startIndex)
        {
            return input.Substring(startIndex);
        }

        public static string Slice(this string input, int startIndex, int length)
        {
            return input.Substring(startIndex, length);
        }

        public static bool CheckLuhnDigit(this string stringDigits)
        {
            int lastDigit = (int)Char.GetNumericValue(stringDigits[stringDigits.Length - 1]);
            stringDigits = stringDigits.Substring(0, stringDigits.Length - 1);
            var digits = stringDigits.Select(c => (int)Char.GetNumericValue(c)).ToList();
            int[] results = { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 };
            var i = 0;
            var lengthMod = digits.Count % 2;
            return lastDigit == (digits.Sum(d => i++ % 2 == lengthMod ? d : results[d]) * 9) % 10;
        }

        public static string Translit(this string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }
            return str;
        }

        public static int Mod(this int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
