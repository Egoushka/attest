namespace Attest
{
    /// <summary>How much of what the issuing authority publishes a rule checks.</summary>
    public enum Coverage
    {
        /// <summary>Everything the authority publishes is checked.</summary>
        Full,

        /// <summary>Shape and field ranges; the check digit exists but its algorithm is not published.</summary>
        FormatOnly,

        /// <summary>The country issues no such identifier.</summary>
        NotApplicable,

        /// <summary>It exists, but nothing authoritative describes it.</summary>
        NotPublished,

        /// <summary>It exists and is documented; Attest has no rule yet.</summary>
        NotCovered,
    }
}
