using System.Collections.Generic;
using System.Linq;

namespace Attest.Tests
{
    internal static class TestTypes
    {
        /// <summary>The 435 country x kind pairs; international types (IBAN ...) are tested on their own.</summary>
        internal static IReadOnlyList<IdentifierType> Pairs { get; } = Identifiers.All.Where(t => t.Country != null).ToList();

        /// <summary>The types with no country.</summary>
        internal static IReadOnlyList<IdentifierType> International { get; } = Identifiers.All.Where(t => t.Country == null).ToList();
    }
}
