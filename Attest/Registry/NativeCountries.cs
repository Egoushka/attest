#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Attest
{
    /// <summary>
    /// A type of a country that has no 1.x validator: its rule is native, or absent. Absent means
    /// Coverage is NotCovered (or NotApplicable) from the provenance table and the verdict is NotCovered.
    /// </summary>
    internal sealed class NativeRule : Rule
    {
        private readonly Func<string, Outcome>? _check;
        private readonly Func<string, string>? _format;

        internal NativeRule(RuleInfo info, Func<string, Outcome>? check, Func<string, string>? format)
        {
            Info = info;
            _check = check;
            _format = format;
        }

        internal override RuleInfo Info { get; }

        internal override bool NativeNormalize => true;

        internal override bool NativeFormat => _format != null;

        internal override string Format(string normalized)
        {
            return _format != null ? _format(normalized) : normalized;
        }

        internal override Outcome Check(string value)
        {
            string? n = Normalize(value);
            if (n == null || _check == null)
            {
                return new Outcome(Reason.Empty, "The value is empty.");
            }

            return _check(n);
        }
    }

    /// <summary>The countries added in 2.0, which have no 1.x validator.</summary>
    internal static class NativeCountries
    {
        internal static readonly Country[] Countries =
        {
            Country.DZ, Country.EG, Country.GH, Country.GN, Country.KE, Country.LI, Country.MA,
            Country.MZ, Country.OM, Country.SG, Country.SN, Country.TN, Country.VN,
        };

        internal static string Name(Country country)
        {
            switch (country)
            {
                case Country.DZ: return "Algeria";
                case Country.EG: return "Egypt";
                case Country.GH: return "Ghana";
                case Country.GN: return "Guinea";
                case Country.KE: return "Kenya";
                case Country.LI: return "Liechtenstein";
                case Country.MA: return "Morocco";
                case Country.MZ: return "Mozambique";
                case Country.OM: return "Oman";
                case Country.SG: return "Singapore";
                case Country.SN: return "Senegal";
                case Country.TN: return "Tunisia";
                default: return "Vietnam";
            }
        }

        /// <summary>The check and printed form for one type id, or null when there is no rule.</summary>
        internal static void RuleFor(string id, out Func<string, Outcome>? check, out Func<string, string>? format)
        {
            check = null;
            format = null;
            switch (id)
            {
                case "ke.taxcode":
                case "ke.company":
                case "ke.vat":
                    check = KenyaPin;
                    break;
                case "vn.taxcode":
                case "vn.company":
                case "vn.vat":
                    check = VietnamTaxCode;
                    format = v => v.Length == 13 ? v.Substring(0, 10) + "-" + v.Substring(10) : v;
                    break;
                case "ma.company":
                    check = MoroccoIce;
                    break;
            }
        }

        /// <summary>
        /// Kenya KRA PIN: a letter A (individual) or P (company), nine digits, a letter -- eleven
        /// characters. No check letter algorithm is published. The three kinds share the PIN, so
        /// A and P are accepted for each (KRA's eTIMS validation rules do not tie a letter to a use).
        /// https://www.kra.go.ke/images/publications/Revised-Electronic-Tax-Register-Specifications-V2.0_14July2021.pdf
        /// </summary>
        private static Outcome KenyaPin(string v)
        {
            if (v.Length != 11)
            {
                return new Outcome(Reason.InvalidLength, "A KRA PIN has eleven characters.");
            }

            if ((v[0] != 'A' && v[0] != 'P') || !Regex.IsMatch(v.Substring(1, 9), "^[0-9]{9}$") || v[10] < 'A' || v[10] > 'Z')
            {
                return new Outcome(Reason.InvalidFormat, "A or P, nine digits, a letter.");
            }

            return Outcome.Valid;
        }

        /// <summary>
        /// Vietnam mã số thuế: ten digits, or ten digits, a hyphen and three more for a dependent unit
        /// (circular 105/2020/TT-BTC: N1N2 N3..N9 N10 - N11N12N13). The tenth digit is a check digit
        /// whose algorithm was not found, so only the shape is checked (KI-VN-1).
        /// </summary>
        private static Outcome VietnamTaxCode(string v)
        {
            if (v.Length != 10 && v.Length != 13)
            {
                return new Outcome(Reason.InvalidLength, "A tax code has ten or thirteen digits.");
            }

            return Regex.IsMatch(v, "^[0-9]+$") ? Outcome.Valid : new Outcome(Reason.InvalidFormat, "Digits only.");
        }

        /// <summary>
        /// Morocco ICE (identifiant commun de l'entreprise): fifteen digits, which the DGI forms print
        /// as groups of 9, 4 and 2. The meaning of the groups and the check were not found (KI-MA-1).
        /// https://www.tax.gov.ma/wps/wcm/connect/1bcc62a5-2b0e-425c-98a2-8a5b801811d9/aac_139b_16i.pdf
        /// </summary>
        private static Outcome MoroccoIce(string v)
        {
            if (v.Length != 15)
            {
                return new Outcome(Reason.InvalidLength, "An ICE has fifteen digits.");
            }

            return Regex.IsMatch(v, "^[0-9]+$") ? Outcome.Valid : new Outcome(Reason.InvalidFormat, "Digits only.");
        }
    }
}
