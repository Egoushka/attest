#nullable enable
using System;
using System.Collections.Generic;

namespace Attest
{
    /// <summary>
    /// Native Parse by type id: what a valid, normalized value encodes (date of birth, sex, region).
    /// An entry exists only where an authority publication describing the encoding is cited beside
    /// it. A decoder runs only on a value the rule accepted, so it does not re-validate.
    /// </summary>
    internal static class ParseForms
    {
        private static readonly Dictionary<string, Func<string, ParsedParts?>> ById = Build();

        internal static Func<string, ParsedParts?>? For(string id)
        {
            Func<string, ParsedParts?>? form;
            return ById.TryGetValue(id, out form) ? form : null;
        }

        private static ParsedParts Of(DateTime? birth, Sex? sex, params string[] parts)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                dictionary[parts[i]] = parts[i + 1];
            }

            return new ParsedParts(birth, sex, dictionary);
        }

        private static int Num(string v, int start, int length)
        {
            return int.Parse(v.Substring(start, length), System.Globalization.CultureInfo.InvariantCulture);
        }

        private static DateTime? Date(int year, int month, int day)
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private static Dictionary<string, Func<string, ParsedParts?>> Build()
        {
            var forms = new Dictionary<string, Func<string, ParsedParts?>>(StringComparer.Ordinal);
            // Bulgarian EGN: YYMMDD, then three digits whose last (the 9th digit overall) is even for
            // a man and odd for a woman, then a check digit. Month +20 is a birth up to 1899, +40 from
            // 2000, none for 1900-1999. GRAO, Naredba RD-02-20-9, Art. 104.
            // https://www.grao.bg/
            Func<string, ParsedParts?> egn = v =>
            {
                int month = Num(v, 2, 2);
                int century = month > 40 ? 2000 : month > 20 ? 1800 : 1900;
                month = month > 40 ? month - 40 : month > 20 ? month - 20 : month;
                return Of(Date(century + Num(v, 0, 2), month, Num(v, 4, 2)), (v[8] - '0') % 2 == 0 ? Sex.Male : Sex.Female);
            };
            forms["bg.nationalid"] = egn;
            forms["bg.taxcode"] = egn;

            // Swedish personnummer: YYMMDD, a three-digit birth number whose last digit (the 9th digit)
            // is odd for a man, then a control digit. Skatteverket. The century is not in the ten-digit
            // form (the "-" becomes "+" from the year a person turns 100, and Normalize drops it), so
            // the date is read from the twelve-digit form only.
            // https://www.skatteverket.se/privat/folkbokforing/personnummer.4.3810a01c150939e893f18c29.html
            Func<string, ParsedParts?> se = value =>
            {
                string v = value.Replace("+", "");
                int offset = v.Length == 12 ? 2 : 0;
                DateTime? birth = v.Length == 12 ? Date(Num(v, 0, 4), Num(v, 4, 2), Num(v, 6, 2)) : null;
                return Of(birth, (v[offset + 8] - '0') % 2 == 1 ? Sex.Male : Sex.Female);
            };
            forms["se.nationalid"] = se;
            forms["se.taxcode"] = se;

            // Norwegian fodselsnummer: DDMMYY, a three-digit individual number whose last digit (the 9th
            // digit) is odd for a man, then two control digits. Skatteetaten. The century rule is not on
            // the pages read (KI-NO-3), so no birth date is decoded.
            // https://www.skatteetaten.no/en/person/foreign/norwegian-id-number-and-d-number/
            Func<string, ParsedParts?> no = v => Of(null, (v[8] - '0') % 2 == 1 ? Sex.Male : Sex.Female);
            forms["no.nationalid"] = no;
            forms["no.taxcode"] = no;

            // Danish CPR number: DDMMYY, a four-digit serial whose last digit (the 10th digit) is even
            // for a woman and odd for a man. cpr.dk. Digits 5-7 select the century, but the page read
            // does not give the table (KI-DK-2), so no birth date is decoded.
            // https://cpr.dk/cpr-systemet/opbygning-af-cpr-nummeret
            Func<string, ParsedParts?> dk = v => Of(null, (v[9] - '0') % 2 == 0 ? Sex.Female : Sex.Male);
            forms["dk.nationalid"] = dk;
            forms["dk.taxcode"] = dk;


            // Polish PESEL: RRMMDDPPPPK. The century is added to the month: +80 for 1800-1899, +0 for
            // 1900-1999, +20 for 2000-2099, +40 for 2100-2199, +60 for 2200-2299. The last digit of the
            // four-digit serial (the 10th digit) is even for a woman and odd for a man.
            // https://www.gov.pl/web/gov/czym-jest-numer-pesel
            forms["pl.nationalid"] = v =>
            {
                int month = Num(v, 2, 2);
                int block = month / 20;
                int century = block == 4 ? 1800 : block == 0 ? 1900 : block == 1 ? 2000 : block == 2 ? 2100 : 2200;
                return Of(Date(century + Num(v, 0, 2), month % 20, Num(v, 4, 2)), (v[9] - '0') % 2 == 1 ? Sex.Male : Sex.Female);
            };
            forms["pl.taxcode"] = forms["pl.nationalid"];

            // Finnish henkilotunnus: DDMMYY, a separator, a three-digit individual number (odd for a
            // man, even for a woman), a check character. The separator gives the century: + for the
            // 1800s; - or Y X W V U for the 1900s; A B C D E F from 2000. Decree 690/2022 amending 128/2010 section 2.
            // https://data.finlex.fi/api/media/statute/17253/mainPdf/main.pdf
            Func<string, ParsedParts?> fi = v =>
            {
                char sign = v[6];
                int century = sign == '+' ? 1800 : (sign == '-' || "YXWVU".IndexOf(sign) >= 0) ? 1900 : "ABCDEF".IndexOf(sign) >= 0 ? 2000 : 0;
                DateTime? birth = century == 0 ? null : Date(century + Num(v, 4, 2), Num(v, 2, 2), Num(v, 0, 2));
                return Of(birth, (v[9] - '0') % 2 == 1 ? Sex.Male : Sex.Female);
            };
            forms["fi.nationalid"] = fi;
            forms["fi.taxcode"] = fi;

            return forms;
        }
    }
}
