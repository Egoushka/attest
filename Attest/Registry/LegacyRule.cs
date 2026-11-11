#nullable enable
using System;
using System.Collections.Generic;

namespace Attest
{
    /// <summary>
    /// Exposes one method of a 1.x country validator as a rule. The 1.x classes remain the single
    /// implementation of the rule; this is the adapter the registry builds 435 of, until each is
    /// ported to a native rule.
    /// </summary>
    internal sealed class LegacyRule : Rule
    {
        private readonly IdValidationAbstract _validator;
        private readonly IdentifierKind _kind;

        internal LegacyRule(IdValidationAbstract validator, IdentifierKind kind, bool supported)
        {
            _validator = validator;
            _kind = kind;
            Info = new RuleInfo(
                new Source[0],
                null,
                1,
                new string[0],
                supported ? Coverage.Full : Coverage.NotCovered);
        }

        internal override RuleInfo Info { get; }

        internal override Outcome Check(string value)
        {
            ValidationResult result = Registry.Run(_validator, value, _kind);
            return result.IsValid ? Outcome.Valid : new Outcome(MapReason(result.ErrorMessage), result.ErrorMessage);
        }

        /// <summary>
        /// The 1.x result carries a message, not a code. The four factories' messages map reliably;
        /// anything else is a rule-specific rejection and is reported as a format failure.
        /// </summary>
        private static Reason MapReason(string? message)
        {
            if (message != null)
            {
                if (message.StartsWith("Invalid checksum", StringComparison.Ordinal))
                {
                    return Reason.InvalidChecksum;
                }

                if (message.StartsWith("Invalid length", StringComparison.Ordinal))
                {
                    return Reason.InvalidLength;
                }

                if (message.StartsWith("Invalid date", StringComparison.Ordinal))
                {
                    return Reason.InvalidDate;
                }
            }

            return Reason.InvalidFormat;
        }
    }
}
