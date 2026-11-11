#nullable enable
namespace Attest
{
    /// <summary>
    /// The answer to "is this value a well-formed identifier of this type". A struct, so a
    /// validation allocates nothing for the result itself. Never carries an exception: every input
    /// produces one.
    /// </summary>
    public readonly struct IdentifierResult
    {
        internal IdentifierResult(Verdict verdict, Reason reason, string? detail, string? normalized, IdentifierType? type)
        {
            Verdict = verdict;
            Reason = reason;
            Detail = detail;
            Normalized = normalized;
            Type = type;
        }

        /// <summary>Valid, Invalid, or NotCovered when Attest has no rule for the type.</summary>
        public Verdict Verdict { get; }

        /// <summary>True only when <see cref="Verdict"/> is <see cref="Attest.Verdict.Valid"/>; false for NotCovered too.</summary>
        public bool IsValid => Verdict == Verdict.Valid;

        /// <summary><see cref="Attest.Reason.None"/> when valid; why it failed otherwise.</summary>
        public Reason Reason { get; }

        /// <summary>English text for logs; never the thing to branch on.</summary>
        public string? Detail { get; }

        /// <summary>The compact form, when the value could be normalized.</summary>
        public string? Normalized { get; }

        /// <summary>
        /// The type the value was checked against. Null only when no type could be chosen: an
        /// unknown type id, or a VAT number whose prefix names no country.
        /// </summary>
        public IdentifierType? Type { get; }
    }
}
