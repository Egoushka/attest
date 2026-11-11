#nullable enable
namespace Attest.Algorithms
{
    /// <summary>
    /// The ISO/IEC 7064:2003 check character systems: the pure systems MOD 11-2, MOD 37-2 and
    /// MOD 97-10, and the hybrid system MOD 11,10. Each exposes the check character for a payload
    /// and a validity test over payload plus check.
    /// </summary>
    internal static class Iso7064
    {
        private const string Alphabet37 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ*";

        /// <summary>MOD 11-2: digits in, check is a digit or 'X' (for 10).</summary>
        internal static bool TryComputeMod11_2(string? payload, out char check)
        {
            check = '\0';
            int p;
            if (!Pure(payload, 11, 2, "0123456789", out p))
            {
                return false;
            }

            int c = (12 - p) % 11;
            check = c == 10 ? 'X' : (char)('0' + c);
            return true;
        }

        /// <summary>True when the last character is the MOD 11-2 check of the rest.</summary>
        internal static bool IsValidMod11_2(string? value)
        {
            char expected;
            return value != null && value.Length >= 2
                && TryComputeMod11_2(value.Substring(0, value.Length - 1), out expected)
                && value[value.Length - 1] == expected;
        }

        /// <summary>MOD 37-2: characters 0-9, A-Z and '*', check is one of the same 37.</summary>
        internal static bool TryComputeMod37_2(string? payload, out char check)
        {
            check = '\0';
            int p;
            if (!Pure(payload, 37, 2, Alphabet37, out p))
            {
                return false;
            }

            check = Alphabet37[(38 - p) % 37];
            return true;
        }

        /// <summary>True when the last character is the MOD 37-2 check of the rest.</summary>
        internal static bool IsValidMod37_2(string? value)
        {
            char expected;
            return value != null && value.Length >= 2
                && TryComputeMod37_2(value.Substring(0, value.Length - 1), out expected)
                && value[value.Length - 1] == expected;
        }

        /// <summary>Hybrid MOD 11,10: digits in, one decimal check digit out.</summary>
        internal static bool TryComputeMod11_10(string? payload, out int check)
        {
            check = 0;
            int p;
            if (!Hybrid11_10(payload, out p))
            {
                return false;
            }

            check = (11 - p) % 10;
            return true;
        }

        /// <summary>True when the last digit is the MOD 11,10 check of the rest.</summary>
        internal static bool IsValidMod11_10(string? value)
        {
            int expected;
            return value != null && value.Length >= 2
                && TryComputeMod11_10(value.Substring(0, value.Length - 1), out expected)
                && value[value.Length - 1] == (char)('0' + expected);
        }

        /// <summary>
        /// MOD 97-10: the two check digits to append to a payload of digits and, as in IBAN
        /// (ISO 13616), letters counted as A=10 to Z=35. 98 minus the remainder of payload*100.
        /// </summary>
        internal static bool TryComputeMod97_10(string? payload, out int check)
        {
            check = 0;
            int r;
            if (!Remainder97(payload, 2, out r))
            {
                return false;
            }

            check = 98 - r;
            return true;
        }

        /// <summary>True when the whole string, read as a number with letters as 10 to 35, leaves remainder 1 modulo 97.</summary>
        internal static bool IsValidMod97_10(string? value)
        {
            int r;
            return value != null && value.Length >= 3 && Remainder97(value, 0, out r) && r == 1;
        }

        private static bool Pure(string? payload, int modulus, int radix, string alphabet, out int p)
        {
            p = 0;
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            foreach (char c in payload!)
            {
                int v = alphabet.IndexOf(c);
                if (v < 0)
                {
                    return false;
                }

                p = (p + v) * radix % modulus;
            }

            return true;
        }

        private static bool Hybrid11_10(string? payload, out int p)
        {
            p = 10;
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            foreach (char c in payload!)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }

                int s = (p + (c - '0')) % 10;
                if (s == 0)
                {
                    s = 10;
                }

                p = s * 2 % 11;
            }

            return true;
        }

        private static bool Remainder97(string? value, int appendedZeros, out int remainder)
        {
            remainder = 0;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (char c in value!)
            {
                if (c >= '0' && c <= '9')
                {
                    remainder = (remainder * 10 + (c - '0')) % 97;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    remainder = (remainder * 100 + (c - 'A' + 10)) % 97;
                }
                else
                {
                    return false;
                }
            }

            for (int i = 0; i < appendedZeros; i++)
            {
                remainder = remainder * 10 % 97;
            }

            return true;
        }
    }
}
