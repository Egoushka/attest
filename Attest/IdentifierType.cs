#nullable enable
using System;
using System.Collections.Generic;

namespace Attest
{
    /// <summary>
    /// One kind of identifier, such as a Belgian VAT number: its stable id, what it is called, how
    /// much of it Attest checks, and the operations on a value of that type. Obtain one from
    /// <see cref="Identifiers"/>; instances are immutable and thread-safe.
    /// </summary>
    public sealed class IdentifierType
    {
        /// <summary>The longest input any type examines, in characters; longer is <see cref="Reason.TooLong"/>.</summary>
        internal const int MaxInputLength = 128;

        private readonly Rule _rule;

        /// <summary>The rule behind this type, for tests that inspect which operations it implements.</summary>
        internal Rule Implementation => _rule;

        internal IdentifierType(string id, Country? country, IdentifierKind kind, string name, string localName, IReadOnlyList<string> aliases, Rule rule)
        {
            Id = id;
            Country = country;
            Kind = kind;
            Name = name;
            LocalName = localName;
            Aliases = aliases;
            _rule = rule;
        }

        /// <summary>The stable public key: lower case, such as "be.vat"; never reused.</summary>
        public string Id { get; }

        /// <summary>The issuing country; null for international types.</summary>
        public Country? Country { get; }

        /// <summary>What sort of identifier this is.</summary>
        public IdentifierKind Kind { get; }

        /// <summary>The English name.</summary>
        public string Name { get; }

        /// <summary>The name in the issuing country's language.</summary>
        public string LocalName { get; }

        /// <summary>Other ids <see cref="Identifiers.Find"/> resolves to this type.</summary>
        public IReadOnlyList<string> Aliases { get; }

        /// <summary>How much of what the authority publishes the rule checks.</summary>
        public Coverage Coverage => _rule.Info.Coverage;

        /// <summary>The rule's provenance.</summary>
        public RuleInfo Rule => _rule.Info;

        /// <summary>
        /// Validates a value. Never throws, in any culture. Input over 128 characters is
        /// <see cref="Reason.TooLong"/> and null, empty or whitespace is <see cref="Reason.Empty"/>,
        /// both before any rule runs; a type without a rule answers <see cref="Verdict.NotCovered"/>.
        /// </summary>
        /// <param name="value">The value, with or without the separators it is printed with.</param>
        public IdentifierResult Validate(string? value)
        {
            if (!_rule.HasRule)
            {
                return new IdentifierResult(Verdict.NotCovered, Reason.None, "No rule for " + Id + ".", null, this);
            }

            if (value == null || string.IsNullOrWhiteSpace(value))
            {
                return new IdentifierResult(Verdict.Invalid, Reason.Empty, "The value is empty.", null, this);
            }

            if (value.Length > MaxInputLength)
            {
                return new IdentifierResult(Verdict.Invalid, Reason.TooLong, "The value is longer than " + MaxInputLength + " characters.", null, this);
            }

            string? normalized = SafeNormalize(value);
            Outcome outcome;
            try
            {
                outcome = _rule.Check(value);
            }
            catch (Exception e)
            {
                // A rule must not throw; if one does, the value is not a valid identifier and the
                // caller still gets an answer.
                outcome = new Outcome(Reason.InvalidFormat, e.GetType().Name);
            }

            return outcome.Reason == Reason.None
                ? new IdentifierResult(Verdict.Valid, Reason.None, null, normalized, this)
                : new IdentifierResult(Verdict.Invalid, outcome.Reason, outcome.Detail, normalized, this);
        }

        /// <summary>
        /// The compact canonical form: separators ('.', '-', space, '/') removed and letters upper
        /// cased invariantly. Null for null, empty, whitespace, over-long input or nothing left.
        /// Idempotent, and it never repairs a wrong character.
        /// </summary>
        /// <param name="value">The value to normalize.</param>
        public string? Normalize(string? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value) || value.Length > MaxInputLength)
            {
                return null;
            }

            return SafeNormalize(value);
        }

        /// <summary>
        /// The official printed form, or null when the value is not valid. Compact unless the type
        /// has a printed form of its own.
        /// </summary>
        /// <param name="value">The value to format.</param>
        public string? Format(string? value)
        {
            IdentifierResult result = Validate(value);
            return result.IsValid && result.Normalized != null ? SafeFormat(result.Normalized) : null;
        }

        /// <summary>Validates and decomposes a value. An invalid value yields no parts.</summary>
        /// <param name="value">The value to parse.</param>
        public ParseResult Parse(string? value)
        {
            IdentifierResult result = Validate(value);
            if (!result.IsValid || result.Normalized == null)
            {
                return new ParseResult(result, null, null, null, null);
            }

            ParsedParts? parsed;
            try
            {
                parsed = _rule.Parse(result.Normalized);
            }
            catch (Exception)
            {
                // A decoder must not throw; a value it cannot decode is still a valid value.
                parsed = null;
            }

            return parsed == null
                ? new ParseResult(result, null, null, null, null)
                : new ParseResult(result, parsed.BirthDate, parsed.Sex, null, parsed.Parts);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Id;
        }

        private string? SafeNormalize(string value)
        {
            try
            {
                return _rule.Normalize(value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string SafeFormat(string normalized)
        {
            try
            {
                return _rule.Format(normalized);
            }
            catch (Exception)
            {
                return normalized;
            }
        }
    }
}
