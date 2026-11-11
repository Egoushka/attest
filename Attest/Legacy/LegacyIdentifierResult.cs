using System;
using System.Collections.Generic;

namespace Attest
{
    /// <summary>
    /// The 1.x multi-kind result, with its original shape: what <see cref="CountryValidator.Validate"/>
    /// returns so that code written against 1.x still compiles.
    /// </summary>
    [Obsolete("Use Identifiers.Match, which returns MatchResult. Removed in 3.0.")]
    public sealed class LegacyIdentifierResult
    {
        internal LegacyIdentifierResult(IdentifierKind requested, IdentifierKind matched, IReadOnlyDictionary<IdentifierKind, ValidationResult> details)
        {
            Requested = requested;
            Matched = matched;
            Details = details;
        }

        /// <summary>The kinds the caller asked about.</summary>
        public IdentifierKind Requested { get; }

        /// <summary>Every kind the value is valid as, including kinds outside <see cref="Requested"/>.</summary>
        public IdentifierKind Matched { get; }

        /// <summary>The result of each kind that was evaluated, whether or not it matched.</summary>
        public IReadOnlyDictionary<IdentifierKind, ValidationResult> Details { get; }

        /// <summary>True when the value is valid as at least one of the requested kinds.</summary>
        public bool IsValid => (Matched & Requested) != 0;

        /// <summary>True when the value is valid both as a personal and as a business identifier.</summary>
        public bool IsAmbiguous => (Matched & IdentifierKind.Person) != 0 && (Matched & IdentifierKind.Business) != 0;
    }
}
