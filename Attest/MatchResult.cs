using System.Collections.Generic;

namespace Attest
{
    /// <summary>
    /// The outcome of validating a value against several identifier types of one country at once.
    /// This is the 1.x <c>IdentifierResult</c>, renamed; <see cref="Identifiers.Match"/> produces it.
    /// </summary>
    public sealed class MatchResult
    {
        internal MatchResult(IdentifierKind requested, IdentifierKind matched, IReadOnlyDictionary<IdentifierType, IdentifierResult> details)
        {
            Requested = requested;
            Matched = matched;
            Details = details;
        }

        /// <summary>The kinds the caller asked about.</summary>
        public IdentifierKind Requested { get; }

        /// <summary>
        /// Every kind the value is valid as, including kinds outside <see cref="Requested"/>. Those
        /// are what <see cref="IsAmbiguous"/> is derived from.
        /// </summary>
        public IdentifierKind Matched { get; }

        /// <summary>The result of each type that was evaluated, whether or not it matched.</summary>
        public IReadOnlyDictionary<IdentifierType, IdentifierResult> Details { get; }

        /// <summary>True when the value is valid as at least one of the requested kinds.</summary>
        public bool IsValid => (Matched & Requested) != 0;

        /// <summary>
        /// True when the value is valid both as a personal and as a business identifier, which
        /// happens in countries that issue one number for both -- Armenia and Nigeria here -- and in
        /// Russia, Peru, Indonesia and Ukraine, where a sole trader files VAT under their personal
        /// number. The country cannot tell the two apart, so neither can this library: a caller who
        /// asked for one category only should decide for itself whether an ambiguous match is good
        /// enough.
        /// </summary>
        public bool IsAmbiguous => (Matched & IdentifierKind.Person) != 0 && (Matched & IdentifierKind.Business) != 0;
    }
}
