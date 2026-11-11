using Attest.Algorithms;
using Xunit;

namespace Attest.Tests.Algorithms
{
    /// <summary>
    /// The shared check-digit schemes. Every vector is either the scheme's own published example or
    /// was worked out by hand from the definition in the standard, in a separate script, and not
    /// read from any other implementation. Several tests are properties of the scheme instead of
    /// vectors, so a wrong table fails them without anybody having to type a wrong answer.
    /// </summary>
    public class CheckDigitAlgorithmTests
    {
        [Theory]
        // 79927398713 is the worked example in ISO/IEC 7812-1 Annex B: payload 7992739871, check 3.
        [InlineData("7992739871", 3)]
        // Hand computed: 1 doubled is 2, so the check is 10 - 2 = 8.
        [InlineData("1", 8)]
        [InlineData("0", 0)]
        // Hand computed: the doubled-digit sum of 123456781234567 is 60, already a multiple of 10, so 0.
        [InlineData("123456781234567", 0)]
        public void LuhnComputesTheCheckDigit(string payload, int expected)
        {
            Assert.True(Luhn.TryComputeCheckDigit(payload, out int check));
            Assert.Equal(expected, check);
            Assert.True(Luhn.IsValid(payload + expected));
        }

        [Theory]
        [InlineData("79927398713", true)]
        [InlineData("79927398710", false)]
        [InlineData("79927398731", false)]
        [InlineData("0", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        // Arabic-Indic digits are digits to char.IsDigit and to nothing in this library.
        [InlineData("٧٩٩٢٧٣٩٨٧١٣", false)]
        public void LuhnValidates(string value, bool expected)
        {
            Assert.Equal(expected, Luhn.IsValid(value));
        }

        [Fact]
        public void LuhnRefusesNonDigitPayload()
        {
            Assert.False(Luhn.TryComputeCheckDigit("12a4", out _));
            Assert.False(Luhn.TryComputeCheckDigit("", out _));
        }

        [Theory]
        // Wikipedia's worked examples for Verhoeff: 236 -> 2363, 12345 -> 123451, 142857 -> 1428570.
        [InlineData("236", 3)]
        [InlineData("12345", 1)]
        [InlineData("142857", 0)]
        public void VerhoeffComputesTheCheckDigit(string payload, int expected)
        {
            Assert.True(Verhoeff.TryComputeCheckDigit(payload, out int check));
            Assert.Equal(expected, check);
            Assert.True(Verhoeff.IsValid(payload + expected));
            Assert.False(Verhoeff.IsValid(payload + ((expected + 1) % 10)));
        }

        [Theory]
        // Damm's own examples: 572 -> 5724, 43881234567 -> 438812345679.
        [InlineData("572", 4)]
        [InlineData("43881234567", 9)]
        public void DammComputesTheCheckDigit(string payload, int expected)
        {
            Assert.True(Damm.TryComputeCheckDigit(payload, out int check));
            Assert.Equal(expected, check);
            Assert.True(Damm.IsValid(payload + expected));
        }

        [Fact]
        public void VerhoeffAndDammCatchEverySingleDigitErrorAndAdjacentTransposition()
        {
            for (int n = 0; n < 1000; n++)
            {
                string payload = n.ToString("D3");
                foreach (var scheme in new (System.Func<string, (bool ok, int check)> compute, System.Func<string, bool> valid)[]
                {
                    (p => (Verhoeff.TryComputeCheckDigit(p, out int c), c), Verhoeff.IsValid),
                    (p => (Damm.TryComputeCheckDigit(p, out int c), c), Damm.IsValid),
                })
                {
                    var (ok, check) = scheme.compute(payload);
                    Assert.True(ok);
                    string full = payload + check;
                    Assert.True(scheme.valid(full));

                    for (int i = 0; i < full.Length; i++)
                    {
                        for (char d = '0'; d <= '9'; d++)
                        {
                            if (d == full[i])
                            {
                                continue;
                            }

                            char[] changed = full.ToCharArray();
                            changed[i] = d;
                            Assert.False(scheme.valid(new string(changed)), full + " with position " + i + " set to " + d);
                        }
                    }

                    for (int i = 0; i + 1 < full.Length; i++)
                    {
                        if (full[i] == full[i + 1])
                        {
                            continue;
                        }

                        char[] swapped = full.ToCharArray();
                        (swapped[i], swapped[i + 1]) = (swapped[i + 1], swapped[i]);
                        Assert.False(scheme.valid(new string(swapped)), full + " swapped at " + i);
                    }
                }
            }
        }

        [Theory]
        // ORCID iDs are MOD 11-2; these are ORCID's published examples (check digits 7 and X).
        [InlineData("000000021825009", '7')]
        [InlineData("000000021694233", 'X')]
        // Hand computed from the definition: ((p + d) * 2) mod 11, check = (12 - p) mod 11.
        [InlineData("079", 'X')]
        [InlineData("1234567", '2')]
        [InlineData("98765432", '8')]
        public void Mod11_2(string payload, char expected)
        {
            Assert.True(Iso7064.TryComputeMod11_2(payload, out char check));
            Assert.Equal(expected, check);
            Assert.True(Iso7064.IsValidMod11_2(payload + expected));
            Assert.False(Iso7064.IsValidMod11_2(payload + (expected == '0' ? '1' : '0')));
        }

        [Theory]
        [InlineData("0794", 5)]
        [InlineData("123456789", 7)]
        [InlineData("5555", 1)]
        [InlineData("1", 9)]
        public void Mod11_10(string payload, int expected)
        {
            Assert.True(Iso7064.TryComputeMod11_10(payload, out int check));
            Assert.Equal(expected, check);
            Assert.True(Iso7064.IsValidMod11_10(payload + expected));
            Assert.False(Iso7064.IsValidMod11_10(payload + ((expected + 1) % 10)));
        }

        [Theory]
        [InlineData("ABC123", 'G')]
        [InlineData("0", '1')]
        [InlineData("X9Y8Z7", 'L')]
        [InlineData("HELLO", '6')]
        public void Mod37_2(string payload, char expected)
        {
            Assert.True(Iso7064.TryComputeMod37_2(payload, out char check));
            Assert.Equal(expected, check);
            Assert.True(Iso7064.IsValidMod37_2(payload + expected));
        }

        [Fact]
        public void Mod37_2AcceptsTheStarCheckCharacter()
        {
            // Search for a payload whose check is '*', the 37th symbol, rather than assert a guess.
            for (int n = 0; n < 200; n++)
            {
                string payload = n.ToString();
                Assert.True(Iso7064.TryComputeMod37_2(payload, out char check));
                Assert.True(Iso7064.IsValidMod37_2(payload + check));
                if (check == '*')
                {
                    return;
                }
            }

            Assert.Fail("No payload under 200 has the check character '*'.");
        }

        [Theory]
        // The IBAN of ISO 13616's own example, GB82 WEST 1234 5698 7654 32, rearranged: the payload
        // is the BBAN, then the country letters, and the check is the 82 in the example.
        [InlineData("WEST12345698765432GB", 82)]
        // Hand computed: 1234567890 * 100 mod 97 = 6, so 98 - 6 = 92; and 1 * 100 mod 97 = 3, so 95.
        [InlineData("1234567890", 92)]
        [InlineData("1", 95)]
        public void Mod97_10(string payload, int expected)
        {
            Assert.True(Iso7064.TryComputeMod97_10(payload, out int check));
            Assert.Equal(expected, check);
            Assert.True(Iso7064.IsValidMod97_10(payload + expected.ToString("D2")));
        }

        [Theory]
        [InlineData("WEST12345698765432GB82", true)]
        [InlineData("WEST12345698765432GB83", false)]
        [InlineData("west12345698765432gb82", false)]
        [InlineData("12", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Mod97_10Validates(string value, bool expected)
        {
            Assert.Equal(expected, Iso7064.IsValidMod97_10(value));
        }

        [Fact]
        public void Iso7064RefusesCharactersOutsideTheAlphabet()
        {
            Assert.False(Iso7064.TryComputeMod11_2("12a", out _));
            Assert.False(Iso7064.TryComputeMod11_10("12a", out _));
            Assert.False(Iso7064.TryComputeMod37_2("12a", out _));
            Assert.False(Iso7064.TryComputeMod97_10("12-4", out _));
            Assert.False(Iso7064.TryComputeMod11_2("١٢", out _));
        }

        [Fact]
        public void WeightedSumMultipliesByPosition()
        {
            // 1*4 + 2*3 + 3*2 + 4*1 = 20.
            Assert.True(Weighted.TrySum("1234", new[] { 4, 3, 2, 1 }, out int sum));
            Assert.Equal(20, sum);
            Assert.True(Weighted.TryMod("1234", new[] { 4, 3, 2, 1 }, 11, out int rem));
            Assert.Equal(9, rem);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("12a4")]
        [InlineData("١٢٣٤")]
        [InlineData(null)]
        public void WeightedSumRefusesShortOrNonAsciiInput(string value)
        {
            Assert.False(Weighted.TrySum(value, new[] { 4, 3, 2, 1 }, out _));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(7, 7)]
        [InlineData(18, 9)]
        [InlineData(99, 18)]
        public void DigitSum(int n, int expected)
        {
            Assert.Equal(expected, Weighted.DigitSum(n));
        }
    }
}
