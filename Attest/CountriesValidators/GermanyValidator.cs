using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class GermanyValidator : IdValidationAbstract
    {
        // Steuernummer rules, formats and factors below follow the official ELSTER specification
        // "Prüfung der Steuer- und Steueridentifikationsnummer", Bayerisches Landesamt für Steuern,
        // Stand 02.09.2026, sections 4 to 7:
        // https://download.elster.de/download/schnittstellen/Pruefung_der_Steuer_und_Steueridentifikatsnummer.pdf

        // Landesspezifische Faktoren für das 11er-Verfahren (section 6.6.3), applied to the first
        // twelve digits of the 13-digit ELSTER format.
        readonly int[] bayerischeFaktoren = new int[] { 0, 5, 4, 3, 0, 2, 7, 6, 5, 4, 3, 2 };
        readonly int[] bremenHamburgFaktoren = new int[] { 0, 0, 4, 3, 0, 2, 7, 6, 5, 4, 3, 2 };
        readonly int[] niedersachsenFaktoren = new int[] { 0, 0, 2, 9, 0, 8, 7, 6, 5, 4, 3, 2 };
        readonly int[] berlinAFaktoren = new int[] { 0, 0, 0, 0, 0, 7, 6, 5, 8, 4, 3, 2 };
        readonly int[] berlinBFaktoren = new int[] { 0, 0, 2, 9, 0, 8, 7, 6, 5, 4, 3, 2 };
        readonly int[] nordrheinWestfalenFaktoren = new int[] { 0, 3, 2, 1, 0, 7, 6, 5, 4, 3, 2, 1 };

        // Faktoren des modifizierten 11er-Verfahrens (section 6.4, Rheinland-Pfalz).
        readonly int[] rheinlandPfalzFaktoren = new int[] { 0, 0, 1, 2, 0, 1, 2, 1, 2, 1, 2, 1 };

        // Summanden und Faktoren des 2er-Verfahrens (section 6.5).
        readonly int[] zweierSummanden = new int[] { 0, 0, 9, 8, 0, 7, 6, 5, 4, 3, 2, 1 };
        readonly int[] zweierFaktoren = new int[] { 0, 0, 512, 256, 0, 128, 64, 32, 16, 8, 4, 2 };

        // Landesnummer (leading digits of the Bundesfinanzamtsnummer) to the factors of its
        // 11er-Verfahren. Bayern and Nordrhein-Westfalen have a one-digit Landesnummer, the rest two.
        readonly Dictionary<string, int[]> regions;

        // Section 6.3: Baden-Württemberg, Hessen and Schleswig-Holstein use the 2er-Verfahren.
        readonly HashSet<string> zweierverfahrenLaender = new HashSet<string> { "28", "26", "21" };

        // Section 5: in the Länder of the bayerischer Programmierverbund the Bezirksnummer is
        // never below 100.
        readonly HashSet<string> bayerischerProgrammierverbund =
            new HashSet<string> { "9", "30", "40", "10", "32", "31", "41" };

        public GermanyValidator()
        {
            CountryCode = nameof(Country.DE);

            regions = new Dictionary<string, int[]>
            {
                { "9", bayerischeFaktoren },              // Bayern
                { "30", bayerischeFaktoren },             // Brandenburg
                { "40", bayerischeFaktoren },             // Mecklenburg-Vorpommern
                { "10", bayerischeFaktoren },             // Saarland
                { "32", bayerischeFaktoren },             // Sachsen
                { "31", bayerischeFaktoren },             // Sachsen-Anhalt
                { "41", bayerischeFaktoren },             // Thüringen
                { "24", bremenHamburgFaktoren },          // Bremen
                { "22", bremenHamburgFaktoren },          // Hamburg
                { "23", niedersachsenFaktoren }           // Niedersachsen
            };
        }

        /// <summary>
        /// Steuernummer. Accepts the 13-digit ELSTER-Steuernummerformat, whose leading digits carry
        /// the Bundesfinanzamtsnummer and therefore select a Prüfziffernverfahren, and the 10- or
        /// 11-digit form printed on a Bescheid. The Prüfziffer is verified only for the ELSTER form:
        /// the procedure and its factors are landesspezifisch and a bare Bescheid number carries no
        /// Bundesfinanzamtsnummer to select them (section 4), so only the land-independent formal
        /// rules of section 5 can be applied to it.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (string.IsNullOrEmpty(id) || !id.All(char.IsDigit))
            {
                return ValidationResult.Invalid("Invalid format. Only numbers are allowed");
            }
            else if (id.Length == 13)
            {
                return ValidateElsterSteuernummer(id);
            }
            else if (id.Length == 10 || id.Length == 11)
            {
                return ValidateBescheidSteuernummer(id);
            }

            return ValidationResult.InvalidLength();
        }

        private ValidationResult ValidateElsterSteuernummer(string id)
        {
            // Section 5: the 5th digit of the ELSTER format is 0 by definition, in every Bundesland.
            if (id[4] != '0')
            {
                return ValidationResult.Invalid("Invalid code. The 5th digit must be 0.");
            }

            string land = id[0] == '9' || id[0] == '5'
                ? id.Slice(0, 1)
                : id.Slice(0, 2);

            var formalResult = ValidateBezirksnummer(id, land);
            if (formalResult != null)
            {
                return formalResult;
            }

            var pruefziffern = CalculatePruefziffern(id, land);
            if (pruefziffern == null)
            {
                return ValidationResult.Invalid("Invalid code. Unknown Bundesland.");
            }

            // Section 6.1: a Steuernummer whose procedure yields a multi-digit Prüfziffer is never
            // issued, so a candidate of 10 or more can never match.
            bool isValid = pruefziffern.Any(p => p < 10 && p == id[12].ToInt());
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Steuernummer as printed on a Bescheid: FFBBB/UUUUP, FF/BBB/UUUUP and 0FF BBB UUUUP all
        /// place the Bezirksnummer immediately before the trailing five digits, so section 5's
        /// Bezirksnummer rule can be applied without knowing the Bundesland.
        /// </summary>
        private ValidationResult ValidateBescheidSteuernummer(string id)
        {
            string bezirksnummer = id.Slice(id.Length - 8, 3);
            bool isValid = !IsReservedBezirksnummer(bezirksnummer);

            // An 11-digit number is ambiguous: it may also be the Nordrhein-Westfalen shape
            // FFF/BBBB/UUUP, whose Bezirksnummer is four digits. Reject only when the number is
            // invalid under every reading its length allows.
            if (!isValid && id.Length == 11)
            {
                string nrwBezirksnummer = id.Slice(3, 4);
                isValid = nrwBezirksnummer != "0000"
                    && nrwBezirksnummer != "0998"
                    && nrwBezirksnummer != "0999"
                    && int.Parse(id.Slice(7, 4)) > 9;
            }

            return isValid ? ValidationResult.Success() : ValidationResult.Invalid(InvalidBezirksnummer);
        }

        private ValidationResult ValidateBezirksnummer(string id, string land)
        {
            // Section 5, Nordrhein-Westfalen: FFFF 0 BBBB UUU P.
            if (land == "5")
            {
                string nrwBezirksnummer = id.Slice(5, 4);
                if (nrwBezirksnummer == "0000" || nrwBezirksnummer == "0998" || nrwBezirksnummer == "0999")
                {
                    return ValidationResult.Invalid(InvalidBezirksnummer);
                }
                else if (int.Parse(id.Slice(9, 4)) <= 9)
                {
                    return ValidationResult.Invalid("Invalid code. The Unterscheidungsnummer and Prüfziffer must be greater than 0009.");
                }

                return null;
            }

            // Every other Bundesland: FFFF 0 BBB UUUU P.
            string bezirksnummer = id.Slice(5, 3);
            if (IsReservedBezirksnummer(bezirksnummer))
            {
                return ValidationResult.Invalid(InvalidBezirksnummer);
            }
            else if (bayerischerProgrammierverbund.Contains(land) && int.Parse(bezirksnummer) < 100)
            {
                return ValidationResult.Invalid("Invalid code. The Bezirksnummer must not be below 100 in this Bundesland.");
            }

            return null;
        }

        private const string InvalidBezirksnummer = "Invalid code. Invalid Bezirksnummer.";

        private bool IsReservedBezirksnummer(string bezirksnummer)
        {
            return bezirksnummer == "000" || bezirksnummer == "998" || bezirksnummer == "999";
        }

        /// <summary>
        /// The Prüfziffer(n) the Bundesland's procedure produces for the first twelve digits, or
        /// null when the Landesnummer is not one of the sixteen Länder. Berlin returns two: it runs
        /// Berlin-A and Berlin-B, which differ only in their multipliers, and which one applies
        /// depends on the Finanzamt and the Bezirk (section 7.2).
        /// </summary>
        private int[] CalculatePruefziffern(string id, string land)
        {
            if (land == "5")
            {
                return new int[] { ElferverfahrenNrw(id) };
            }
            else if (land == "27")
            {
                return new int[] { ModifiziertesElferverfahren(id) };
            }
            else if (land == "11")
            {
                return new int[] { Elferverfahren(id, berlinAFaktoren), Elferverfahren(id, berlinBFaktoren) };
            }
            else if (zweierverfahrenLaender.Contains(land))
            {
                return new int[] { Zweierverfahren(id) };
            }

            int[] faktoren;
            return regions.TryGetValue(land, out faktoren)
                ? new int[] { Elferverfahren(id, faktoren) }
                : null;
        }

        /// <summary>
        /// Section 6.6: weighted sum of the first twelve digits; the Prüfziffer is the distance to
        /// the next multiple of 11, or 0 when the sum is already divisible by 11.
        /// </summary>
        private int Elferverfahren(string id, int[] faktoren)
        {
            int rest = id.Sum(faktoren) % 11;
            return rest == 0 ? 0 : 11 - rest;
        }

        /// <summary>
        /// Section 6.6, Nordrhein-Westfalen variant: the Prüfziffer is the remainder itself.
        /// </summary>
        private int ElferverfahrenNrw(string id)
        {
            return id.Sum(nordrheinWestfalenFaktoren) % 11;
        }

        /// <summary>
        /// Section 6.4: a two-digit product collapses to its last digit plus one; the Prüfziffer is
        /// the distance to the next higher number divisible by 10. A sum already divisible by 10
        /// yields 10, which section 6.1 rules out as a Prüfziffer, so such a number is not issued.
        /// </summary>
        private int ModifiziertesElferverfahren(string id)
        {
            int sum = 0;
            for (int i = 0; i < rheinlandPfalzFaktoren.Length; i++)
            {
                int product = id[i].ToInt() * rheinlandPfalzFaktoren[i];
                sum += product > 9 ? (product % 10) + 1 : product;
            }

            return 10 - (sum % 10);
        }

        /// <summary>
        /// Section 6.5: add the positional Summand and keep the last digit, multiply by the
        /// positional Faktor, reduce each product to a single digit by repeated Quersumme, and sum.
        /// The Prüfziffer is the distance to the next higher number divisible by 10, or 0 when the
        /// sum already ends in 0.
        /// </summary>
        private int Zweierverfahren(string id)
        {
            int sum = 0;
            for (int i = 0; i < zweierFaktoren.Length; i++)
            {
                int summe = (id[i].ToInt() + zweierSummanden[i]) % 10;
                sum += Quersumme(summe * zweierFaktoren[i]);
            }

            return sum % 10 == 0 ? 0 : 10 - (sum % 10);
        }

        private int Quersumme(int value)
        {
            while (value > 9)
            {
                int sum = 0;
                while (value > 0)
                {
                    sum += value % 10;
                    value = value / 10;
                }

                value = sum;
            }

            return value;
        }

        /// <summary>
        /// Validate German Tax Id Steueridentifikationsnummer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^\d{11}$"))
            {
                return ValidationResult.InvalidFormat("01234567890");
            }

            else if (id[0] == '0')
            {
                return ValidationResult.Invalid("Invalid format. The first digit must never be 0.");
            }

            char[] digits = id.ToCharArray();
            string first10Digits = id.Slice(0, 10);

            // Within the first ten digits exactly one digit occurs twice or three times and every
            // other digit occurs at most once; since 2016 the threefold occurrence is allowed, but
            // the three must not stand at directly consecutive positions ("bei drei gleichen Ziffern
            // duerfen nur zwei unmittelbar hintereinander stehen, nicht jedoch alle drei").
            // https://de.wikipedia.org/wiki/Steuerliche_Identifikationsnummer
            // https://download.elster.de/download/schnittstellen/Pruefung_der_Steuer_und_Steueridentifikatsnummer.pdf
            var repeated = first10Digits.GroupBy(x => x)
                  .Where(g => g.Count() > 1)
                  .Select(g => new { Value = g.Key, Count = g.Count() }).ToList();

            if (repeated.Count != 1 || repeated[0].Count > 3)
            {
                return ValidationResult.Invalid("Invalid");
            }
            else if (repeated[0].Count == 3
                && first10Digits.Contains(new string(repeated[0].Value, 3)))
            {
                return ValidationResult.Invalid("Invalid");
            }
            int sum = 0;
            int product = 10;
            for (int i = 0; i <= 9; i++)
            {
                sum = (int)(Char.GetNumericValue(digits[i]) + product) % 10;
                if (sum == 0)
                {
                    sum = 10;
                }
                product = (sum * 2) % 11;
            }
            int checksum = 11 - product;
            if (checksum == 10)
            {
                checksum = 0;
            }

            bool isValid = int.Parse(id[10].ToString()) == checksum;
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Umsatzsteur Identifikationnummer (VAT)
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            vatId = vatId.Replace("DE", string.Empty).Replace("de", string.Empty);
            if (!Regex.IsMatch(vatId, @"^[1-9]\d{8}$"))
            {
                return ValidationResult.InvalidFormat("123456789");
            }

            var product = 10;
            for (var index = 0; index < 8; index++)
            {
                var sum = (vatId[index].ToInt() + product) % 10;
                if (sum == 0)
                {
                    sum = 10;
                }

                product = 2 * sum % 11;
            }

            var val = 11 - product;
            var checkDigit = val == 10
                ? 0
                : val;

            bool isValid = checkDigit == vatId[8].ToInt();
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^\\d{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
