using System.Text.RegularExpressions;
using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// The shared normaliser every validator calls on its first line. 276 call sites depend on the
    /// behaviour asserted here, and a change to it moves verdicts in all 87 countries at once, so it
    /// is pinned directly rather than observed through whichever country happens to notice.
    ///
    /// The contract is stated in CONTRIBUTING.md rule 5 and in IdExtensions.RemoveSpecialCharacthers:
    /// letters of any script survive, ASCII digits survive, punctuation and separators are dropped,
    /// and a decimal digit outside 0-9 becomes a sentinel no format check accepts.
    /// </summary>
    public class IdExtensionsTests
    {
        // The sentinel RemoveSpecialCharacthers substitutes for a non-ASCII decimal digit.
        private const string Sentinel = "�";

        [Theory]
        [InlineData(null, "")]                       // null in, empty out: every format check then rejects it
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("85.07.15-017.09", "85071501709")]   // the number as printed on a Belgian id card
        [InlineData("BE 0428 759 497", "BE0428759497")]
        [InlineData("---", "")]
        [InlineData("12/34-56 78", "12345678")]
        public void SeparatorsAndPunctuationAreDropped(string input, string expected)
        {
            Assert.Equal(expected, input.RemoveSpecialCharacthers());
        }

        [Theory]
        // Letters survive whatever their script. A Belarusian UNP is printed with Cyrillic
        // look-alikes that BelarusValidator maps itself, so stripping non-Latin letters here would
        // reject every genuine one.
        [InlineData("АВЕКМНОРСТ")]                    // Cyrillic look-alikes, not Latin
        [InlineData("ΦΠΑ")]                           // Greek
        [InlineData("abcXYZ")]
        public void LettersOfAnyScriptSurvive(string input)
        {
            Assert.Equal(input, input.RemoveSpecialCharacthers());
        }

        [Theory]
        [InlineData("١٢٣٤٥٦٧٨٩")]                     // Arabic-Indic
        [InlineData("۱۲۳۴۵۶۷۸۹")]                     // Extended Arabic-Indic
        [InlineData("१२३४५६७८९")]                     // Devanagari
        [InlineData("１２３４５６７８９")]                     // Fullwidth
        [InlineData("๑๒๓๔๕๖๗๘๙")]                     // Thai
        public void DecimalDigitsOutsideAsciiBecomeTheSentinelRatherThanDisappearing(string input)
        {
            string actual = input.RemoveSpecialCharacthers();

            // Dropping them would validate the digits that remain and turn a wrong number into a
            // right one, so the length must be preserved and every character replaced.
            Assert.Equal(input.Length, actual.Length);
            Assert.Equal(new string('�', input.Length), actual);
        }

        [Fact]
        public void TheSentinelIsAcceptedByNoDigitClassTheValidatorsUse()
        {
            // This is what makes the \d-before-parse sites in the validators safe: the sentinel
            // reaches the format check, and no digit class matches it, so nothing reaches int.Parse.
            Assert.DoesNotMatch(@"\d", Sentinel);
            Assert.DoesNotMatch("[0-9]", Sentinel);
            Assert.False(char.IsDigit(Sentinel[0]));
            Assert.False(char.IsLetterOrDigit(Sentinel[0]));
        }

        [Fact]
        public void AsciiDigitsMixedWithNonAsciiKeepTheirPositions()
        {
            Assert.Equal("12" + Sentinel + "45", "12٣45".RemoveSpecialCharacthers());
        }

        [Theory]
        [InlineData("79927398713", true)]             // the canonical Luhn example
        [InlineData("79927398710", false)]
        [InlineData("79927398711", false)]
        public void CheckLuhnDigitVerifiesTheTrailingDigit(string input, bool expected)
        {
            Assert.Equal(expected, input.CheckLuhnDigit());
        }

        [Theory]
        [InlineData(-1, 11, 10)]                      // the reason Mod exists: % alone returns -1 here
        [InlineData(-13, 11, 9)]
        [InlineData(13, 11, 2)]
        [InlineData(0, 11, 0)]
        public void ModIsNonNegativeWhereTheRemainderOperatorIsNot(int value, int modulus, int expected)
        {
            Assert.Equal(expected, value.Mod(modulus));
        }

        [Theory]
        [InlineData('0', 0)]
        [InlineData('7', 7)]
        [InlineData('9', 9)]
        public void ToIntReadsAnAsciiDigitsValue(char input, int expected)
        {
            Assert.Equal(expected, input.ToInt());
        }

        [Fact]
        public void SumMultipliesEachDigitByItsWeight()
        {
            // 1*3 + 2*1 + 3*2 = 11
            Assert.Equal(11, "123".Sum(new[] { 3, 1, 2 }));
        }

        [Theory]
        [InlineData("0123456789", 4, "456789")]
        [InlineData("0123456789", 0, "0123456789")]
        public void SliceTakesTheRemainderFromAnIndex(string input, int start, string expected)
        {
            Assert.Equal(expected, input.Slice(start));
        }

        [Theory]
        [InlineData("0123456789", 2, 3, "234")]
        public void SliceTakesALengthFromAnIndex(string input, int start, int length, string expected)
        {
            Assert.Equal(expected, input.Slice(start, length));
        }
    }
}
