using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class GermanyValidatorTests
    {
        private readonly GermanyValidator _germanyValidator;
        public GermanyValidatorTests()
        {
            _germanyValidator = new GermanyValidator();
        }

        [Theory]
        [InlineData("36 574 261 809 ", true)]
        [InlineData("36574261890", false)]
        [InlineData("36554266806", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghijk", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _germanyValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        [InlineData("36 574 261 809 ", true)]
        [InlineData("36574261890", false)]
        [InlineData("36554266806", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghijk", false)]
        // Repetition rule: within the first ten digits exactly one digit may repeat, twice or three
        // times, and three identical digits must not stand at directly consecutive positions.
        // https://de.wikipedia.org/wiki/Steuerliche_Identifikationsnummer
        // All four carry the check digit the ISO 7064 MOD 11,10 procedure produces, so only the
        // repetition rule decides the verdict.
        [InlineData("84090153608", true)]   // 0 three times, non-consecutive: allowed since 2016
        [InlineData("68109522236", false)]  // 2 three times, all three consecutive
        [InlineData("37406812415", false)]  // two digits repeated (4 and 1), only one may repeat
        [InlineData("81909378658", false)]  // two digits repeated (8 and 9)
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _germanyValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // The 13-digit ELSTER-Steuernummerformat carries the Bundesfinanzamtsnummer, so the
        // Prüfziffernverfahren is selectable and the Prüfziffer is checked. Every "true" row is an
        // example from Tabelle 4-1 of the official ELSTER specification "Prüfung der Steuer- und
        // Steueridentifikationsnummer" (Stand 02.09.2026); the "false" rows carry a check digit
        // that the Bundesland's procedure does not produce for those first twelve digits.
        // https://download.elster.de/download/schnittstellen/Pruefung_der_Steuer_und_Steueridentifikatsnummer.pdf
        [Theory]
        [InlineData("2866081508156", true)]  // Baden-Württemberg, 2er-Verfahren
        [InlineData("2866081508151", false)]
        [InlineData("2653081508158", true)]  // Hessen, 2er-Verfahren
        [InlineData("2653081508150", false)]
        [InlineData("2138081508154", true)]  // Schleswig-Holstein, 2er-Verfahren
        [InlineData("2138081508150", false)]
        [InlineData("9198081508152", true)]  // Bayern (München), 11er-Verfahren
        [InlineData("9198081508153", false)]
        [InlineData("9296081508153", true)]  // Bayern (Nürnberg), 11er-Verfahren
        [InlineData("3098081508157", true)]  // Brandenburg, 11er-Verfahren
        [InlineData("4098081508157", true)]  // Mecklenburg-Vorpommern, 11er-Verfahren
        [InlineData("3248081508156", true)]  // Sachsen, 11er-Verfahren
        [InlineData("3198081508152", true)]  // Sachsen-Anhalt, 11er-Verfahren
        [InlineData("4198081508152", true)]  // Thüringen, 11er-Verfahren
        [InlineData("1096081508187", true)]  // Saarland, 11er-Verfahren
        [InlineData("1096081508180", false)]
        [InlineData("2497012301233", true)]  // Bremen, 11er-Verfahren
        [InlineData("2497012301230", false)]
        [InlineData("2241081508154", true)]  // Hamburg, 11er-Verfahren
        [InlineData("2241081508150", false)]
        [InlineData("2388081508158", true)]  // Niedersachsen, 11er-Verfahren
        [InlineData("2388081508150", false)]
        [InlineData("1197081508154", true)]  // Berlin, 11er-Verfahren Berlin-B
        [InlineData("1197081508152", false)]
        [InlineData("5400081508159", true)]  // Nordrhein-Westfalen, 11er-Verfahren NRW
        [InlineData("5400081508150", false)]
        [InlineData("5500081508151", true)]  // Nordrhein-Westfalen, 11er-Verfahren NRW
        [InlineData("5600081508154", true)]  // Nordrhein-Westfalen, 11er-Verfahren NRW
        [InlineData("2799081508152", true)]  // Rheinland-Pfalz, modifiziertes 11er-Verfahren
        [InlineData("2799081508150", false)]
        [InlineData("2893081508152", true)]  // Baden-Württemberg, 2er-Verfahren
        public void TestElsterEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _germanyValidator.ValidateEntity(code).IsValid);
        }

        // Formal rules of section 5 of the same specification, on the 13-digit ELSTER format.
        [Theory]
        [InlineData("2866181508156", false)] // the 5th digit is 0 by definition
        [InlineData("2866000008156", false)] // Bezirksnummer 000
        [InlineData("2866099808156", false)] // Bezirksnummer 998
        [InlineData("9198009908150", false)] // bayerischer Programmierverbund, Bezirksnummer below 100
        [InlineData("5400081500009", false)] // Nordrhein-Westfalen, UUUP must be greater than 0009
        [InlineData("2900081508152", false)] // no Bundesland has Landesnummer 29
        public void TestElsterEntityCodeFormalRules(string code, bool isValid)
        {
            Assert.Equal(isValid, _germanyValidator.ValidateEntity(code).IsValid);
        }

        // The 10- or 11-digit form printed on a Bescheid carries no Bundesfinanzamtsnummer, so no
        // Prüfziffernverfahren can be selected for it (section 4) and no check digit is asserted
        // here. Only the land-independent Bezirksnummer rules of section 5 apply.
        [Theory]
        [InlineData("93815/08152", true)]
        [InlineData("151/815/08156", true)]
        [InlineData("66815/08156", true)]    // Baden-Württemberg, FFBBB/UUUUP
        [InlineData("97 123 01233", true)]   // Bremen, FF BBB UUUUP
        [InlineData("053 815 08158", true)]  // Hessen, 0FF BBB UUUUP
        [InlineData("400/8150/8159", true)]  // Nordrhein-Westfalen, FFF/BBBB/UUUP
        [InlineData("0000000000", false)]    // Bezirksnummer 000
        [InlineData("9999999999", false)]    // Bezirksnummer 999
        [InlineData("9399808152", false)]    // Bezirksnummer 998
        [InlineData("00000000000", false)]   // Bezirksnummer 000 under either 11-digit reading
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghij", false)]
        [InlineData("12345", false)]
        [InlineData("12345678901234", false)]
        public void TestBescheidEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _germanyValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("DE 136,695 976", true)]
        [InlineData("DE136695976", true)]
        [InlineData("136695978", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcdefghi", false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _germanyValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        [InlineData("26133", true)]
        [InlineData("53225", true)]
        [InlineData("32", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("abcde", false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _germanyValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
