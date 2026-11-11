using System.Collections.Generic;
using System.Linq;

namespace Attest.Tests
{
    internal static class TestTypes
    {
        /// <summary>The 435 country x kind pairs of the 87 countries with a 1.x validator; international types (IBAN ...) are tested on their own.</summary>
        internal static IReadOnlyList<IdentifierType> Pairs { get; } = Identifiers.All.Where(t => t.Country != null && Registry.Validators.ContainsKey(t.Country.Value)).ToList();

        /// <summary>The types of the countries added in 2.0, which have no 1.x validator.</summary>
        internal static IReadOnlyList<IdentifierType> NativePairs { get; } = Identifiers.All.Where(t => t.Country != null && !Registry.Validators.ContainsKey(t.Country.Value)).ToList();

        /// <summary>The types with no country.</summary>
        internal static IReadOnlyList<IdentifierType> International { get; } = Identifiers.All.Where(t => t.Country == null).ToList();
    }
}
