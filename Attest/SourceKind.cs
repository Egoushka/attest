namespace Attest
{
    /// <summary>What sort of document a <see cref="Source"/> is.</summary>
    public enum SourceKind
    {
        /// <summary>Published by the issuing authority.</summary>
        Authority,

        /// <summary>A law or regulation.</summary>
        Law,

        /// <summary>An OECD TIN sheet.</summary>
        Oecd,

        /// <summary>Another implementation of the same rule.</summary>
        ReferenceImplementation,

        /// <summary>Anything else.</summary>
        Secondary,
    }
}
