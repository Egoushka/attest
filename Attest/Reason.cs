namespace Attest
{
    /// <summary>
    /// Why a value was rejected. Members are only ever added; a rule reports the first reason, in
    /// declaration order, that applies. No member means "unknown": see <see cref="Verdict.NotCovered"/>.
    /// </summary>
    public enum Reason
    {
        /// <summary>No failure: the value is valid, or there was no rule to apply.</summary>
        None,

        /// <summary>The value is null, empty or whitespace.</summary>
        Empty,

        /// <summary>The value is over the input bound and was rejected before any parsing.</summary>
        TooLong,

        /// <summary>A character that cannot occur in this type.</summary>
        InvalidCharacters,

        /// <summary>The wrong number of characters.</summary>
        InvalidLength,

        /// <summary>The right characters and length, in the wrong shape.</summary>
        InvalidFormat,

        /// <summary>The check digit does not match.</summary>
        InvalidChecksum,

        /// <summary>An embedded date does not exist.</summary>
        InvalidDate,

        /// <summary>A field outside its published range: region, month, century, type letter.</summary>
        InvalidComponent,

        /// <summary>A wrong or missing country prefix.</summary>
        InvalidPrefix,
    }
}
