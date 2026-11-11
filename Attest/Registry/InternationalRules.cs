#nullable enable
using System;
using System.Text;
using Attest.Algorithms;

namespace Attest
{
    /// <summary>
    /// An international identifier that is one function: normalize (documented separators only),
    /// check, print. Each factory below cites the standard it implements.
    /// </summary>
    internal sealed class SimpleRule : Rule
    {
        private readonly Func<string, Outcome> _check;
        private readonly char[] _separators;
        private readonly Func<string, string>? _format;

        internal SimpleRule(RuleInfo info, char[] separators, Func<string, Outcome> check, Func<string, string>? format)
        {
            Info = info;
            _separators = separators;
            _check = check;
            _format = format;
        }

        internal override RuleInfo Info { get; }

        internal override bool NativeNormalize => true;

        internal override bool NativeFormat => _format != null;

        internal override string? Normalize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (Array.IndexOf(_separators, c) < 0)
                {
                    builder.Append(c);
                }
            }

            return builder.Length == 0 ? null : builder.ToString().ToUpperInvariant();
        }

        internal override string Format(string normalized)
        {
            return _format != null ? _format(normalized) : normalized;
        }

        internal override Outcome Check(string value)
        {
            string? n = Normalize(value);
            return n == null ? new Outcome(Reason.Empty, "The value is empty.") : _check(n);
        }
    }

    /// <summary>The checks of the international identifiers.</summary>
    internal static class InternationalRules
    {
        private static bool Digits(string s, int from, int count)
        {
            for (int i = from; i < from + count; i++)
            {
                if (s[i] < '0' || s[i] > '9')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllDigits(string s)
        {
            return s.Length > 0 && Digits(s, 0, s.Length);
        }

        private static bool Alnum(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z');
        }

        private static Outcome Bad(Reason reason, string detail)
        {
            return new Outcome(reason, detail);
        }

        /// <summary>ISSN: seven digits and a check digit, modulus 11 with weights 8..2, X for 10 (issn.org).</summary>
        internal static Outcome Issn(string v)
        {
            if (v.Length != 8)
            {
                return Bad(Reason.InvalidLength, "An ISSN has eight characters.");
            }

            if (!Digits(v, 0, 7) || !(Digits(v, 7, 1) || v[7] == 'X'))
            {
                return Bad(Reason.InvalidFormat, "Seven digits and a digit or X.");
            }

            int sum = 0;
            for (int i = 0; i < 7; i++)
            {
                sum += (v[i] - '0') * (8 - i);
            }

            int c = (11 - sum % 11) % 11;
            char expected = c == 10 ? 'X' : (char)('0' + c);
            return v[7] == expected ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The ISSN check digit does not match.");
        }

        internal static string IssnFormat(string v)
        {
            return v.Substring(0, 4) + "-" + v.Substring(4);
        }

        /// <summary>ISBN-10 (modulus 11, weights 10..1, X for 10) and ISBN-13 (EAN-13 check, prefix 978 or 979).</summary>
        internal static Outcome Isbn(string v)
        {
            if (v.Length == 10)
            {
                if (!Digits(v, 0, 9) || !(Digits(v, 9, 1) || v[9] == 'X'))
                {
                    return Bad(Reason.InvalidFormat, "Nine digits and a digit or X.");
                }

                int sum = 0;
                for (int i = 0; i < 10; i++)
                {
                    sum += (v[i] == 'X' ? 10 : v[i] - '0') * (10 - i);
                }

                return sum % 11 == 0 ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The ISBN-10 check digit does not match.");
            }

            if (v.Length == 13)
            {
                if (!AllDigits(v))
                {
                    return Bad(Reason.InvalidFormat, "An ISBN-13 is thirteen digits.");
                }

                if (!v.StartsWith("978", StringComparison.Ordinal) && !v.StartsWith("979", StringComparison.Ordinal))
                {
                    return Bad(Reason.InvalidPrefix, "An ISBN-13 starts with 978 or 979.");
                }

                return GtinSumOk(v) ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The ISBN-13 check digit does not match.");
            }

            return Bad(Reason.InvalidLength, "An ISBN has ten or thirteen characters.");
        }

        /// <summary>GTIN-8, -12, -13 and -14 (EAN, UPC): weights 3 and 1 from the digit before the check digit.</summary>
        internal static Outcome Gtin(string v)
        {
            if (!AllDigits(v))
            {
                return Bad(Reason.InvalidFormat, "A GTIN is digits only.");
            }

            if (v.Length != 8 && v.Length != 12 && v.Length != 13 && v.Length != 14)
            {
                return Bad(Reason.InvalidLength, "A GTIN has 8, 12, 13 or 14 digits.");
            }

            return GtinSumOk(v) ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The GTIN check digit does not match.");
        }

        private static bool GtinSumOk(string v)
        {
            int sum = 0;
            for (int i = 0; i < v.Length; i++)
            {
                int fromRight = v.Length - 1 - i; // 0 is the check digit
                sum += (v[i] - '0') * (fromRight % 2 == 0 ? 1 : 3);
            }

            return sum % 10 == 0;
        }

        /// <summary>ISIN (ISO 6166): two letters, nine alphanumerics, a Luhn check digit over the letters read as 10..35.</summary>
        internal static Outcome Isin(string v)
        {
            if (v.Length != 12)
            {
                return Bad(Reason.InvalidLength, "An ISIN has twelve characters.");
            }

            if (!(v[0] >= 'A' && v[0] <= 'Z') || !(v[1] >= 'A' && v[1] <= 'Z'))
            {
                return Bad(Reason.InvalidPrefix, "An ISIN starts with two letters.");
            }

            for (int i = 2; i < 11; i++)
            {
                if (!Alnum(v[i]))
                {
                    return Bad(Reason.InvalidFormat, "Alphanumerics A-Z and 0-9 only.");
                }
            }

            if (!Digits(v, 11, 1))
            {
                return Bad(Reason.InvalidFormat, "The last character is a digit.");
            }

            var digits = new StringBuilder();
            foreach (char c in v)
            {
                if (c >= 'A')
                {
                    digits.Append(c - 'A' + 10);
                }
                else
                {
                    digits.Append(c);
                }
            }

            return Luhn.IsValid(digits.ToString()) ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The ISIN check digit does not match.");
        }

        /// <summary>LEI (ISO 17442): twenty alphanumerics, the last two digits are MOD 97-10 check digits.</summary>
        internal static Outcome Lei(string v)
        {
            if (v.Length != 20)
            {
                return Bad(Reason.InvalidLength, "An LEI has twenty characters.");
            }

            for (int i = 0; i < 18; i++)
            {
                if (!Alnum(v[i]))
                {
                    return Bad(Reason.InvalidFormat, "Alphanumerics A-Z and 0-9 only.");
                }
            }

            if (!Digits(v, 18, 2))
            {
                return Bad(Reason.InvalidFormat, "The last two characters are digits.");
            }

            return Iso7064.IsValidMod97_10(v) ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The LEI check digits do not match.");
        }

        /// <summary>BIC (ISO 9362): four letters, two letters of country, two alphanumerics, optionally three more.</summary>
        internal static Outcome Bic(string v)
        {
            if (v.Length != 8 && v.Length != 11)
            {
                return Bad(Reason.InvalidLength, "A BIC has eight or eleven characters.");
            }

            for (int i = 0; i < v.Length; i++)
            {
                bool letters = i < 6;
                if (letters ? !(v[i] >= 'A' && v[i] <= 'Z') : !Alnum(v[i]))
                {
                    return Bad(Reason.InvalidFormat, "Position " + (i + 1) + (letters ? " must be a letter." : " must be a letter or digit."));
                }
            }

            return Outcome.Valid;
        }

        /// <summary>ISMN: thirteen digits, prefix 9790, EAN-13 style check with weights 1 and 3 (Library of Congress, U.S. ISMN Agency).</summary>
        internal static Outcome Ismn(string v)
        {
            if (v.Length != 13)
            {
                return Bad(Reason.InvalidLength, "An ISMN has thirteen digits.");
            }

            if (!AllDigits(v))
            {
                return Bad(Reason.InvalidFormat, "An ISMN is digits only.");
            }

            if (!v.StartsWith("9790", StringComparison.Ordinal))
            {
                return Bad(Reason.InvalidPrefix, "An ISMN starts with 979-0.");
            }

            return GtinSumOk(v) ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The ISMN check digit does not match.");
        }

        /// <summary>IMEI: fifteen digits, the last a Luhn check digit over the first fourteen (3GPP TS 23.003 clause 6.2.1 and annex B).</summary>
        internal static Outcome Imei(string v)
        {
            if (v.Length != 15)
            {
                return Bad(Reason.InvalidLength, "An IMEI has fifteen digits.");
            }

            if (!AllDigits(v))
            {
                return Bad(Reason.InvalidFormat, "An IMEI is digits only.");
            }

            return Luhn.IsValid(v) ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The IMEI check digit does not match.");
        }

        /// <summary>
        /// Freight container number (ISO 6346): three owner letters, U, J or Z, six digits and a check
        /// digit. Layout from BIC; the check is the ISO 6346 weighted sum (see KI-ISO6346-1).
        /// </summary>
        internal static Outcome Container(string v)
        {
            if (v.Length != 11)
            {
                return Bad(Reason.InvalidLength, "A container number has eleven characters.");
            }

            for (int i = 0; i < 3; i++)
            {
                if (v[i] < 'A' || v[i] > 'Z')
                {
                    return Bad(Reason.InvalidFormat, "The owner code is three letters.");
                }
            }

            if (v[3] != 'U' && v[3] != 'J' && v[3] != 'Z')
            {
                return Bad(Reason.InvalidFormat, "The equipment category is U, J or Z.");
            }

            if (!Digits(v, 4, 7))
            {
                return Bad(Reason.InvalidFormat, "The serial number and check digit are digits.");
            }

            int sum = 0;
            int weight = 1;
            for (int i = 0; i < 10; i++)
            {
                int value = i < 4 ? LetterValue(v[i]) : v[i] - '0';
                sum += value * weight;
                weight *= 2;
            }

            return sum % 11 % 10 == v[10] - '0' ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The container check digit does not match.");
        }

        // A is 10 and values that are multiples of 11 are skipped: A=10, B=12, ... K=21, L=23.
        private static int LetterValue(char letter)
        {
            int value = 10;
            for (char c = 'A'; c < letter; c++)
            {
                value++;
                if (value % 11 == 0)
                {
                    value++;
                }
            }

            return value;
        }

        private const string EicAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-";

        /// <summary>
        /// EIC (ENTSO-E Energy Identification Code): sixteen characters from 0-9, A-Z and '-', the
        /// last a check character. Each of the first fifteen is valued by its place in the alphabet
        /// above, weighted 16 down to 2, summed; the check is 36 - ((sum - 1) mod 37), written as a
        /// letter above 9. ENTSO-E EIC Data Exchange Implementation Guide v1.2, section 5.
        /// </summary>
        internal static Outcome Eic(string v)
        {
            if (v.Length != 16)
            {
                return Bad(Reason.InvalidLength, "An EIC has sixteen characters.");
            }

            int sum = 0;
            for (int i = 0; i < 16; i++)
            {
                int value = EicAlphabet.IndexOf(v[i]);
                if (value < 0)
                {
                    return Bad(Reason.InvalidFormat, "An EIC uses digits, capital letters and '-'.");
                }

                if (i < 15)
                {
                    sum += value * (16 - i);
                }
            }

            int check = 36 - (sum - 1) % 37;
            if (check == 36)
            {
                return Bad(Reason.InvalidChecksum, "An allocated EIC never has '-' as its check character.");
            }

            return EicAlphabet[check] == v[15] ? Outcome.Valid : Bad(Reason.InvalidChecksum, "The EIC check character does not match.");
        }

        /// <summary>NACE code: a division of two digits, optionally a group (three digits) or a class (four). The section letter is not part of the code (Eurostat KS-GQ-24-007).</summary>
        internal static Outcome Nace(string v)
        {
            if (v.Length < 2 || v.Length > 4)
            {
                return Bad(Reason.InvalidLength, "A NACE code has two, three or four digits.");
            }

            return AllDigits(v) ? Outcome.Valid : Bad(Reason.InvalidFormat, "A NACE code is digits only; the section letter is not part of it.");
        }

        internal static string NaceFormat(string v)
        {
            return v.Length > 2 ? v.Substring(0, 2) + "." + v.Substring(2) : v;
        }

        /// <summary>SEED excise number: two letters of country, then eleven alphanumerics, left-padded with zeros (EC DDS2 SEED help page).</summary>
        internal static Outcome Seed(string v)
        {
            if (v.Length != 13)
            {
                return Bad(Reason.InvalidLength, "A SEED excise number has thirteen characters.");
            }

            for (int i = 0; i < 13; i++)
            {
                bool ok = i < 2 ? v[i] >= 'A' && v[i] <= 'Z' : Alnum(v[i]);
                if (!ok)
                {
                    return Bad(Reason.InvalidFormat, "Two letters of country, then eleven letters or digits.");
                }
            }

            return Outcome.Valid;
        }

        /// <summary>OSS VAT number of the non-Union scheme (EU and nine characters) or the import scheme (IM and ten). Shape only.</summary>
        internal static Outcome Oss(string v)
        {
            bool union = v.StartsWith("EU", StringComparison.Ordinal);
            bool import = v.StartsWith("IM", StringComparison.Ordinal);
            if (!union && !import)
            {
                return Bad(Reason.InvalidPrefix, "An OSS number starts with EU or IM.");
            }

            if (v.Length != (union ? 11 : 12))
            {
                return Bad(Reason.InvalidLength, union ? "A non-Union OSS number has eleven characters." : "An import OSS number has twelve characters.");
            }

            for (int i = 2; i < v.Length; i++)
            {
                if (!Alnum(v[i]))
                {
                    return Bad(Reason.InvalidFormat, "Letters and digits after the prefix.");
                }
            }

            return Outcome.Valid;
        }
    }
}
