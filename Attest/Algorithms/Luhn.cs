#nullable enable
namespace Attest.Algorithms
{
    /// <summary>
    /// The Luhn mod-10 check (ISO/IEC 7812-1, Annex B): from the right, double every second digit,
    /// subtract 9 from any result over 9, and the total must be a multiple of 10.
    /// </summary>
    internal static class Luhn
    {
        /// <summary>True when the string is ASCII digits only, at least two, and passes the check.</summary>
        internal static bool IsValid(string? value)
        {
            if (value == null || value.Length < 2)
            {
                return false;
            }

            return Sum(value, 0) % 10 == 0;
        }

        /// <summary>Computes the digit to append to <paramref name="payload"/>; false if it is not ASCII digits.</summary>
        internal static bool TryComputeCheckDigit(string? payload, out int check)
        {
            check = 0;
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            int sum = Sum(payload!, 1);
            if (sum < 0)
            {
                return false;
            }

            check = (10 - sum % 10) % 10;
            return true;
        }

        /// <summary>The Luhn sum, counting from the right; <paramref name="offset"/> 1 leaves room for a check digit. -1 on a non-digit.</summary>
        private static int Sum(string value, int offset)
        {
            int sum = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[value.Length - 1 - i];
                if (c < '0' || c > '9')
                {
                    return -1;
                }

                int d = c - '0';
                if ((i + offset) % 2 == 1)
                {
                    d *= 2;
                    if (d > 9)
                    {
                        d -= 9;
                    }
                }

                sum += d;
            }

            return sum;
        }
    }
}
