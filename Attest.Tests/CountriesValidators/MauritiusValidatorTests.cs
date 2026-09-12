using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class MauritiusValidatorTests
    {
        private readonly MauritiusValidator _mauritiusValidator;

        public MauritiusValidatorTests()
        {
            _mauritiusValidator = new MauritiusValidator();
        }

        // National Identity Card number: the initial of the surname, DDMMYY of birth, a six digit
        // serial and a check character. The check character is the alphabet "0123456789A-Z"
        // indexed by (17 - sum) mod 17, where sum runs the first thirteen characters - the leading
        // letter included, valued by its position in that alphabet - against the weights 14..2.
        // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.mu.nid.html
        // Check characters below were computed with that algorithm.
        [Theory]
        [InlineData("A250690500360A", true)]
        [InlineData("M120385123456E", true)]
        [InlineData("S310178004567B", true)]
        [InlineData("A290200500360F", true)]    // 29 February, the century is unknown so it stands
        [InlineData("A3004905003609", true)]    // 30 April
        [InlineData("A250690500360B", false)]   // Wrong check character
        [InlineData("A250690500360", false)]    // Thirteen characters
        [InlineData("A2506905003601A", false)]  // Fifteen characters
        [InlineData("1250690500360A", false)]   // Leading character is not a letter
        [InlineData("A320690500360G", false)]   // Day 32, check character is correct
        [InlineData("A251390500360C", false)]   // Month 13, check character is correct
        [InlineData("A000690500360B", false)]   // Day 00, check character is correct
        [InlineData("A2500905003602", false)]   // Month 00, check character is correct
        [InlineData("A3102905003600", false)]   // 31 February, check character is correct
        [InlineData("A310490500360E", false)]   // 31 April, check character is correct
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _mauritiusValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        // The identity card number doubles as the national identifier, so the base class
        // delegation applies.
        [Theory]
        [InlineData("A250690500360A", true)]
        [InlineData("M120385123456E", true)]
        [InlineData("A250690500360B", false)]   // Wrong check character
        [InlineData("A250690500360", false)]    // Thirteen characters
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _mauritiusValidator.ValidateNationalIdentity(code).IsValid);
        }

        // Business Registration Number: an entity type letter (C company, I individual,
        // P partnership, F among others) or LLP/LP, then the two digit year of registration and
        // a serial. No check digit is published. Every accepted number below is a real BRN from
        // the MRA's quarterly register of VAT registered persons.
        // https://www.mra.mu/download/ListofVATRegPersons.pdf
        [Theory]
        [InlineData("C21179119", true)]         // A & A Property Developers Ltd
        [InlineData("I12003567", true)]         // sole trader
        [InlineData("P07002139", true)]         // A H M Jeewa & Cie
        [InlineData("F16000116", true)]         // Accurex & Associates, an F prefix
        [InlineData("LLP23000042", true)]       // Atwell (Mauritius) LLP
        [InlineData("LLP260060", true)]         // Chetty Reesaul & Partners LLP
        [InlineData("LP20000178", true)]        // limited partnership
        [InlineData("C2117911", false)]         // 7 digits after the letter
        [InlineData("C211791190", false)]       // 9 digits after the letter
        [InlineData("21179119", false)]         // no entity type letter
        [InlineData("CC2117911", false)]        // two letters
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestEntity(string code, bool isValid)
        {
            Assert.Equal(isValid, _mauritiusValidator.ValidateEntity(code).IsValid);
        }

        // VAT Registration Number: eight digits, no published check digit. The accepted numbers
        // below are real ones from the same MRA register; its 35994 entries are all eight digits
        // and start with 1, 2, 3, 4, 5, 7 or 8, so the leading digit carries no rule.
        // https://www.mra.mu/download/ListofVATRegPersons.pdf
        [Theory]
        [InlineData("27899124", true)]          // A & A Property Developers Ltd
        [InlineData("15209225", true)]          // leading digit 1
        [InlineData("31106191", true)]          // leading digit 3
        [InlineData("2789912", false)]          // 7 digits
        [InlineData("278991245", false)]        // 9 digits
        [InlineData("VAT27899124", false)]      // the VAT prefix is not part of the number
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestVat(string code, bool isValid)
        {
            Assert.Equal(isValid, _mauritiusValidator.ValidateVAT(code).IsValid);
        }
    }
}
