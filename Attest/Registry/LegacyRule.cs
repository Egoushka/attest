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
        private readonly PrintedForm? _printed;
        private readonly Func<string, ParsedParts?>? _parse;

        internal LegacyRule(string id, IdValidationAbstract validator, IdentifierKind kind, bool supported)
        {
            _validator = validator;
            _kind = kind;
            Info = Provenance.For(id, supported);
            _printed = PrintedForms.For(id);
            _parse = ParseForms.For(id);
        }

        internal override RuleInfo Info { get; }

        internal override bool NativeNormalize => _printed?.Normalize != null;

        internal override bool NativeFormat => _printed?.Format != null;

        internal override bool NativeParse => _parse != null;

        internal override ParsedParts? Parse(string normalized)
        {
            return _parse?.Invoke(normalized);
        }

        internal override string? Normalize(string value)
        {
            return _printed?.Normalize != null ? _printed.Normalize(value) : base.Normalize(value);
        }

        internal override string Format(string normalized)
        {
            return _printed?.Format != null ? _printed.Format(normalized) : base.Format(normalized);
        }

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
