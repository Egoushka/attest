#nullable enable
namespace Attest.Algorithms
{
    /// <summary>
    /// Weighted digit sums, the shape most national check digits share: multiply each digit by the
    /// weight at its position, add, and reduce modulo a constant.
    /// </summary>
    internal static class Weighted
    {
        /// <summary>
        /// The sum of digit[i] * weights[i] over the first <paramref name="weights"/>.Length
        /// characters. False when the value is shorter than the weights or a character is not an
        /// ASCII digit, so a caller cannot read a Unicode digit as a number.
        /// </summary>
        internal static bool TrySum(string? digits, int[] weights, out int sum)
        {
            sum = 0;
            if (digits == null || digits.Length < weights.Length)
            {
                return false;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                char c = digits[i];
                if (c < '0' || c > '9')
                {
                    return false;
                }

                sum += (c - '0') * weights[i];
            }

            return true;
        }

        /// <summary>The weighted sum reduced modulo <paramref name="modulus"/>.</summary>
        internal static bool TryMod(string? digits, int[] weights, int modulus, out int remainder)
        {
            int sum;
            remainder = 0;
            if (!TrySum(digits, weights, out sum))
            {
                return false;
            }

            remainder = sum % modulus;
            return true;
        }

        /// <summary>The sum of the decimal digits of <paramref name="n"/>, for schemes that add the digits of each product.</summary>
        internal static int DigitSum(int n)
        {
            int sum = 0;
            for (; n > 0; n /= 10)
            {
                sum += n % 10;
            }

            return sum;
        }
    }
}
