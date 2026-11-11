#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace Attest
{
    /// <summary>
    /// The printed form and the compact form of one identifier type, where it differs from the
    /// default of <see cref="Rule"/> (strip '.', '-', ' ' and '/', upper-case, print compact).
    /// </summary>
    internal sealed class PrintedForm
    {
        internal PrintedForm(Func<string, string?>? normalize, Func<string, string>? format)
        {
            Normalize = normalize;
            Format = format;
        }

        /// <summary>A normalize that replaces the default, or null.</summary>
        internal Func<string, string?>? Normalize { get; }

        /// <summary>Prints a valid, normalized value, or null for the compact form.</summary>
        internal Func<string, string>? Format { get; }
    }

    /// <summary>
    /// Native Normalize and Format by type id. An entry exists only where the authority's own
    /// publication shows the printed form (cited beside it) or where the default separator
    /// stripping would change the meaning of a value. Every other type prints compact.
    /// </summary>
    internal static class PrintedForms
    {
        private static readonly Dictionary<string, PrintedForm> ById = Build();

        internal static PrintedForm? For(string id)
        {
            PrintedForm? form;
            return ById.TryGetValue(id, out form) ? form : null;
        }

        private static Dictionary<string, PrintedForm> Build()
        {
            var forms = new Dictionary<string, PrintedForm>(StringComparer.Ordinal);

            // Australian Business Number, 2-3-3-3: "51 824 753 556".
            // https://abr.business.gov.au/Help/AbnFormat
            Func<string, string> abn = v => v.Length == 11 && IsDigits(v) ? Group(v, ' ', 2, 3, 3, 3) : v;
            forms["au.company"] = new PrintedForm(null, abn);
            forms["au.vat"] = new PrintedForm(null, abn);

            // Brazilian CNPJ, XX.XXX.XXX/XXXX-DD (the first twelve characters may be letters from 2026).
            // https://www.gov.br/receitafederal/pt-br/centrais-de-conteudo/publicacoes/perguntas-e-respostas/cnpj/cnpj-alfanumerico.pdf
            forms["br.company"] = new PrintedForm(null, v => v.Length == 14
                ? v.Substring(0, 2) + "." + v.Substring(2, 3) + "." + v.Substring(5, 3) + "/" + v.Substring(8, 4) + "-" + v.Substring(12, 2)
                : v);

            // Belgian enterprise number, 0xxx.xxx.xxx.
            // https://news.economie.fgov.be/228779-les-numeros-d-entreprise-passent-au-1/
            forms["be.company"] = new PrintedForm(null, v => v.Length == 10 && IsDigits(v) ? Group(v, '.', 4, 3, 3) : v);

            // UK National Insurance number, two letters, three pairs of digits, a letter: "QQ 12 34 56 A".
            // https://www.gov.uk/hmrc-internal-manuals/national-insurance-manual/nim39110
            // gb.taxcode prints compact: its 1.x rule does not strip spaces (KI-GB-1).
            forms["gb.nationalid"] = new PrintedForm(null, v => v.Length == 9 ? Group(v, ' ', 2, 2, 2, 2, 1) : v);

            // UK postcode: outward code, one space, inward code of three characters ("SW1A 1AA").
            // https://www.ons.gov.uk/methodology/geography/ukgeographies/postalgeography
            forms["gb.postalcode"] = new PrintedForm(null, v => v.Length >= 5 && v.Length <= 7 && !v.StartsWith("BFPO", StringComparison.Ordinal)
                ? v.Substring(0, v.Length - 3) + " " + v.Substring(v.Length - 3)
                : v);

            // Eircode: three-character routing key, a space, four-character unique identifier ("A65 F4E2").
            // https://www.eircode.ie/faqs
            forms["ie.postalcode"] = new PrintedForm(null, v => v.Length == 7 ? v.Substring(0, 3) + " " + v.Substring(3) : v);

            // Austrian Firmenbuchnummer: the number, then one lower-case check letter ("FN 122119m").
            // The letter's case is part of the number, so it is not upper-cased and the FN prefix is
            // dropped from the compact form (the rule accepts it either way).
            // https://www.justiz.gv.at/
            forms["at.company"] = new PrintedForm(
                v => KeepCaseStripping(v, "FN"),
                v => "FN " + v);

            // Finnish personal identity code DDMMYYCZZZQ: the sign that follows the year is the
            // century marker (+ 1800s, - 1900s, A 2000s, and the letters added in 2023), not a
            // separator, so it stays. Only spaces are removed.
            // https://www.finlex.fi/fi/lainsaadanto/saadoskokoelma/2022/690
            Func<string, string?> keepSign = v => v.Replace(" ", string.Empty).Length == 0 ? null : v.Replace(" ", string.Empty).ToUpperInvariant();
            forms["fi.nationalid"] = new PrintedForm(keepSign, null);
            forms["fi.taxcode"] = new PrintedForm(keepSign, null);

            // US ZIP+4 is NNNNN-NNNN; the hyphen is part of the code, and a space is accepted for it.
            // https://pe.usps.com/text/pub28/
            forms["us.postalcode"] = new PrintedForm(
                v =>
                {
                    string t = v.Trim();
                    if (t.Length == 10 && IsDigits(t.Substring(0, 5)) && (t[5] == ' ' || t[5] == '-') && IsDigits(t.Substring(6)))
                    {
                        return t.Substring(0, 5) + "-" + t.Substring(6);
                    }

                    return Rule.DefaultNormalize(v);
                },
                null);

            return forms;
        }

        private static string? KeepCaseStripping(string value, string prefix)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c != ' ' && c != '.' && c != '-' && c != '/')
                {
                    builder.Append(c);
                }
            }

            string s = builder.ToString();
            if (s.Length >= prefix.Length && string.Equals(s.Substring(0, prefix.Length), prefix, StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(prefix.Length);
            }

            return s.Length == 0 ? null : s;
        }

        private static bool IsDigits(string s)
        {
            foreach (char c in s)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return s.Length > 0;
        }

        private static string Group(string v, char separator, params int[] sizes)
        {
            var builder = new StringBuilder(v.Length + sizes.Length);
            int at = 0;
            for (int i = 0; i < sizes.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(separator);
                }

                builder.Append(v, at, sizes[i]);
                at += sizes[i];
            }

            return builder.ToString();
        }
    }
}
