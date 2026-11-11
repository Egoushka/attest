#nullable enable
namespace Attest.Algorithms
{
    /// <summary>
    /// H. Michael Damm's quasigroup check digit (2004). Catches every single-digit error and every
    /// adjacent transposition, with a single table and no permutation step.
    /// </summary>
    internal static class Damm
    {
        // A weakly totally anti-symmetric quasigroup of order 10 with zero on the diagonal.
        private static readonly int[,] Table =
        {
            { 0, 3, 1, 7, 5, 9, 8, 6, 4, 2 },
            { 7, 0, 9, 2, 1, 5, 4, 8, 6, 3 },
            { 4, 2, 0, 6, 8, 7, 1, 3, 5, 9 },
            { 1, 7, 5, 0, 9, 8, 3, 4, 2, 6 },
            { 6, 1, 2, 3, 0, 4, 5, 9, 7, 8 },
            { 3, 6, 7, 4, 2, 0, 9, 5, 8, 1 },
            { 5, 8, 6, 9, 7, 2, 0, 1, 3, 4 },
            { 8, 9, 4, 5, 3, 6, 2, 0, 1, 7 },
            { 9, 4, 3, 8, 6, 1, 7, 2, 0, 5 },
            { 2, 5, 8, 1, 4, 3, 6, 7, 9, 0 },
        };

        /// <summary>True when the string is ASCII digits only, at least two, and the check digit matches.</summary>
        internal static bool IsValid(string? value)
        {
            int interim;
            return value != null && value.Length >= 2 && Interim(value, out interim) && interim == 0;
        }

        /// <summary>Computes the digit to append to <paramref name="payload"/>; false if it is not ASCII digits.</summary>
        internal static bool TryComputeCheckDigit(string? payload, out int check)
        {
            return Interim(payload, out check) && !string.IsNullOrEmpty(payload);
        }

        private static bool Interim(string? value, out int interim)
        {
            interim = 0;
            if (value == null)
            {
                return false;
            }

            foreach (char c in value)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }

                interim = Table[interim, c - '0'];
            }

            return true;
        }
    }
}
