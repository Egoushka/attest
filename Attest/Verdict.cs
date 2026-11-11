namespace Attest
{
    /// <summary>
    /// The three possible answers to "is this value a well-formed identifier of this type". The
    /// zero value is <see cref="Invalid"/>, so an uninitialised <see cref="IdentifierResult"/>
    /// fails closed instead of reading as valid.
    /// </summary>
    public enum Verdict
    {
        /// <summary>The value fails a check; see <see cref="Reason"/>.</summary>
        Invalid = 0,

        /// <summary>The value satisfies every check the rule has.</summary>
        Valid = 1,

        /// <summary>
        /// Attest has no rule for this type, so it has no opinion. Not the same as
        /// <see cref="Invalid"/>, although <see cref="IdentifierResult.IsValid"/> is false for both.
        /// </summary>
        NotCovered = 2,
    }
}
