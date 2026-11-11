#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Attest.Algorithms;

namespace Attest
{
    /// <summary>
    /// The International Bank Account Number (ISO 13616): country code, two check digits and a
    /// country-specific BBAN of fixed total length and character classes. Checked here: the
    /// registry length and BBAN structure of the country, and the MOD 97-10 check digits. The
    /// national check digits inside the BBAN (Belgian, Spanish, French, Italian, Polish and others)
    /// are not checked; see KNOWN-ISSUES.md KI-IBAN-1.
    /// </summary>
    internal sealed class IbanRule : Rule
    {
        /// <summary>
        /// country code, total length, BBAN structure (n digits, a upper-case letters, c digits or
        /// upper-case letters), as in the SWIFT IBAN Registry. Written from the registry's published
        /// structures and cross-checked against Wikipedia's table and, in the tests only, IbanNet.
        /// </summary>
        private static readonly string[] Table =
        {
            "AD:24:4n4n12c", "AE:23:3n16n", "AL:28:8n16c", "AT:20:5n11n", "AZ:28:4a20c", "BA:20:3n3n8n2n",
            "BE:16:3n7n2n", "BG:22:4a4n2n8c", "BH:22:4a14c", "BI:27:5n5n11n2n", "BR:29:8n5n10n1a1c", "BY:28:4c4n16c",
            "CH:21:5n12c", "CR:22:4n14n", "CY:28:3n5n16c", "CZ:24:4n6n10n", "DE:22:8n10n", "DJ:27:5n5n11n2n",
            "DK:18:4n9n1n", "DO:28:4c20n", "EE:20:2n2n11n1n", "EG:29:4n4n17n", "ES:24:4n4n1n1n10n", "FI:18:3n11n",
            "FK:18:2a12n", "FO:18:4n9n1n", "FR:27:5n5n11c2n", "GB:22:4a6n8n", "GE:22:2a16n", "GI:23:4a15c",
            "GL:18:4n9n1n", "GR:27:3n4n16c", "GT:28:4c20c", "HN:28:4a20n", "HR:21:7n10n", "HU:28:3n4n1n15n1n", "IE:22:4a6n8n",
            "IL:23:3n3n13n", "IQ:23:4a3n12n", "IS:26:4n2n6n10n", "IT:27:1a5n5n12c", "JO:30:4a4n18c", "KW:30:4a22c",
            "KZ:20:3n13c", "LB:28:4n20c", "LC:32:4a24c", "LI:21:5n12c", "LT:20:5n11n", "LU:20:3n13c",
            "LV:21:4a13c", "LY:25:3n3n15n", "MC:27:5n5n11c2n", "MD:24:2c18c", "ME:22:3n13n2n", "MK:19:3n10c2n",
            "MN:20:4n12n", "MR:27:5n5n11n2n", "MT:31:4a5n18c", "MU:30:4a2n2n12n3n3a", "NI:28:4a20n", "NL:18:4a10n",
            "NO:15:4n6n1n", "OM:23:3n16c", "PK:24:4a16c", "PL:28:8n16n", "PS:29:4a21c", "PT:25:4n4n11n2n",
            "QA:29:4a21c", "RO:24:4a16c", "RS:22:3n13n2n", "RU:33:9n5n15c", "SA:24:2n18c", "SC:31:4a2n2n16n3a",
            "SD:18:2n12n", "SE:24:3n16n1n", "SI:19:5n8n2n", "SK:24:4n6n10n", "SM:27:1a5n5n12c", "SO:23:4n3n12n",
            "ST:25:4n4n11n2n", "SV:28:4a20n", "TL:23:3n14n2n", "TN:24:2n3n13n2n", "TR:26:5n1n16c", "UA:29:6n19c",
            "VA:22:3n15n", "VG:24:4a16n", "XK:20:4n10n2n", "YE:30:4a4n18c",
        };

        private static readonly Dictionary<string, Spec> Specs = Parse();

        internal sealed class Spec
        {
            internal int Length;
            internal char[] Classes = new char[0]; // one class per BBAN position
        }

        internal IbanRule(RuleInfo info)
        {
            Info = info;
        }

        internal override RuleInfo Info { get; }

        internal override bool NativeNormalize => true;

        internal override bool NativeFormat => true;

        /// <summary>The country codes the table covers, for the tests.</summary>
        internal static IEnumerable<string> Countries => Specs.Keys;

        /// <summary>Total length and BBAN classes of a country, or null.</summary>
        internal static Spec? SpecOf(string country)
        {
            Spec? spec;
            return Specs.TryGetValue(country, out spec) ? spec : null;
        }

        /// <summary>Only the space is a documented separator of an IBAN; letters are upper-cased.</summary>
        internal override string? Normalize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c != ' ')
                {
                    builder.Append(c);
                }
            }

            return builder.Length == 0 ? null : builder.ToString().ToUpperInvariant();
        }

        /// <summary>Groups of four, the last one short: "DE89 3704 0044 0532 0130 00".</summary>
        internal override string Format(string normalized)
        {
            var builder = new StringBuilder(normalized.Length + normalized.Length / 4);
            for (int i = 0; i < normalized.Length; i += 4)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(normalized, i, Math.Min(4, normalized.Length - i));
            }

            return builder.ToString();
        }

        internal override Outcome Check(string value)
        {
            string? iban = Normalize(value);
            if (iban == null)
            {
                return new Outcome(Reason.Empty, "The value is empty.");
            }

            foreach (char c in iban)
            {
                if (!((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')))
                {
                    return new Outcome(Reason.InvalidCharacters, "An IBAN has only letters A-Z and digits 0-9 besides spaces.");
                }
            }

            if (iban.Length < 4 || !IsLetter(iban[0]) || !IsLetter(iban[1]))
            {
                return new Outcome(Reason.InvalidPrefix, "An IBAN starts with a two-letter country code.");
            }

            Spec? spec = SpecOf(iban.Substring(0, 2));
            if (spec == null)
            {
                return new Outcome(Reason.InvalidPrefix, "No IBAN registry entry for country " + iban.Substring(0, 2) + ".");
            }

            if (iban.Length != spec.Length)
            {
                return new Outcome(Reason.InvalidLength, "An IBAN of " + iban.Substring(0, 2) + " has " + spec.Length + " characters.");
            }

            if (!IsDigit(iban[2]) || !IsDigit(iban[3]))
            {
                return new Outcome(Reason.InvalidFormat, "The check digits are not two digits.");
            }

            for (int i = 0; i < spec.Classes.Length; i++)
            {
                char c = iban[4 + i];
                bool ok = spec.Classes[i] == 'n' ? IsDigit(c)
                    : spec.Classes[i] == 'a' ? IsLetter(c)
                    : true; // c: any of the characters already allowed
                if (!ok)
                {
                    return new Outcome(Reason.InvalidFormat, "Position " + (i + 5) + " must be " + (spec.Classes[i] == 'n' ? "a digit" : "a letter") + ".");
                }
            }

            // ISO 13616 check digits lie between 02 and 98; MOD 97-10 never produces 00, 01 or 99.
            string check = iban.Substring(2, 2);
            if (check == "00" || check == "01" || check == "99" || !Iso7064.IsValidMod97_10(iban.Substring(4) + iban.Substring(0, 4)))
            {
                return new Outcome(Reason.InvalidChecksum, "The IBAN check digits do not match.");
            }

            return Outcome.Valid;
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsLetter(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        private static Dictionary<string, Spec> Parse()
        {
            var specs = new Dictionary<string, Spec>(StringComparer.Ordinal);
            foreach (string row in Table)
            {
                string[] f = row.Split(':');
                var classes = new List<char>();
                int i = 0;
                string structure = f[2];
                while (i < structure.Length)
                {
                    int count = 0;
                    while (structure[i] >= '0' && structure[i] <= '9')
                    {
                        count = count * 10 + (structure[i] - '0');
                        i++;
                    }

                    char cls = structure[i++];
                    for (int k = 0; k < count; k++)
                    {
                        classes.Add(cls);
                    }
                }

                var spec = new Spec { Length = int.Parse(f[1], System.Globalization.CultureInfo.InvariantCulture), Classes = classes.ToArray() };
                if (spec.Classes.Length + 4 != spec.Length)
                {
                    throw new InvalidOperationException("IBAN table: " + f[0] + " structure does not add up to its length.");
                }

                specs.Add(f[0], spec);
            }

            return specs;
        }
    }
}
