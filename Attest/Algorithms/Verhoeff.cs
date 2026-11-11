#nullable enable
namespace Attest.Algorithms
{
    /// <summary>
    /// Verhoeff's dihedral-group check digit (1969). Catches every single-digit error and every
    /// adjacent transposition.
    /// </summary>
    internal static class Verhoeff
    {
        // Multiplication table of the dihedral group D5.
        private static readonly int[,] D =
        {
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            { 1, 2, 3, 4, 0, 6, 7, 8, 9, 5 },
            { 2, 3, 4, 0, 1, 7, 8, 9, 5, 6 },
            { 3, 4, 0, 1, 2, 8, 9, 5, 6, 7 },
            { 4, 0, 1, 2, 3, 9, 5, 6, 7, 8 },
            { 5, 9, 8, 7, 6, 0, 4, 3, 2, 1 },
            { 6, 5, 9, 8, 7, 1, 0, 4, 3, 2 },
            { 7, 6, 5, 9, 8, 2, 1, 0, 4, 3 },
            { 8, 7, 6, 5, 9, 3, 2, 1, 0, 4 },
            { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 },
        };

        // Permutation table: row 1 is applied once per position, row n is row 1 applied n times.
        private static readonly int[,] P =
        {
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            { 1, 5, 7, 6, 2, 8, 3, 0, 9, 4 },
            { 5, 8, 0, 3, 7, 9, 6, 1, 4, 2 },
            { 8, 9, 1, 6, 0, 4, 3, 5, 2, 7 },
            { 9, 4, 5, 3, 1, 2, 6, 8, 7, 0 },
            { 4, 2, 8, 6, 5, 7, 3, 9, 0, 1 },
            { 2, 7, 9, 3, 8, 0, 6, 4, 1, 5 },
            { 7, 0, 4, 6, 9, 1, 3, 2, 5, 8 },
        };

        private static readonly int[] Inv = { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

        /// <summary>True when the string is ASCII digits only, at least two, and the check digit matches.</summary>
        internal static bool IsValid(string? value)
        {
            if (value == null || value.Length < 2)
            {
                return false;
            }

            int c;
            return Checksum(value, 0, out c) && c == 0;
        }

        /// <summary>Computes the digit to append to <paramref name="payload"/>; false if it is not ASCII digits.</summary>
        internal static bool TryComputeCheckDigit(string? payload, out int check)
        {
            check = 0;
            int c;
            if (string.IsNullOrEmpty(payload) || !Checksum(payload!, 1, out c))
            {
                return false;
            }

            check = Inv[c];
            return true;
        }

        private static bool Checksum(string value, int offset, out int c)
        {
            c = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[value.Length - 1 - i];
                if (ch < '0' || ch > '9')
                {
                    return false;
                }

                c = D[c, P[(i + offset) % 8, ch - '0']];
            }

            return true;
        }
    }
}
