#nullable enable
using System;
using System.Collections.Generic;

namespace Attest
{
    /// <summary>Machine-readable provenance of one identifier type's rule.</summary>
    public sealed class RuleInfo
    {
        internal RuleInfo(IReadOnlyList<Source> sources, DateTime? lastVerified, int revision, IReadOnlyList<string> knownGaps, Coverage coverage)
        {
            Sources = sources;
            LastVerified = lastVerified;
            Revision = revision;
            KnownGaps = knownGaps;
            Coverage = coverage;
        }

        /// <summary>The documents the rule was written from. Empty for rules not yet annotated.</summary>
        public IReadOnlyList<Source> Sources { get; }

        /// <summary>When a human or agent last re-read the source and the rule matched; null if never.</summary>
        public DateTime? LastVerified { get; }

        /// <summary>Bumped whenever any verdict for this type can change.</summary>
        public int Revision { get; }

        /// <summary>Ids into KNOWN-ISSUES.md.</summary>
        public IReadOnlyList<string> KnownGaps { get; }

        /// <summary>How much of what the authority publishes the rule checks.</summary>
        public Coverage Coverage { get; }
    }
}
