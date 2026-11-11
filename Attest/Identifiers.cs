#nullable enable
using System;
using System.Collections.Generic;

namespace Attest
{
    /// <summary>
    /// Lookup and discovery of identifier types, and the operations that span several of them.
    /// Rules are immutable and stateless, so everything here is static and thread-safe.
    /// </summary>
    public static class Identifiers
    {
        /// <summary>Every identifier type Attest knows, including those it has no rule for.</summary>
        public static IReadOnlyList<IdentifierType> All => Registry.All;

        /// <summary>Finds a type by its id or an alias, ignoring case; null when there is none.</summary>
        /// <param name="id">An id such as "be.vat", or an alias.</param>
        public static IdentifierType? Find(string? id)
        {
            return Registry.Find(id);
        }

        /// <summary>Every type of one country; empty for a country Attest has no types for.</summary>
        /// <param name="country">The country.</param>
        public static IReadOnlyList<IdentifierType> For(Country country)
        {
            return Registry.For(country);
        }

        /// <summary>The types of one country whose kind is any of <paramref name="kinds"/>.</summary>
        /// <param name="country">The country.</param>
        /// <param name="kinds">The kinds to keep.</param>
        public static IReadOnlyList<IdentifierType> For(Country country, IdentifierKind kinds)
        {
            var result = new List<IdentifierType>();
            foreach (IdentifierType type in Registry.For(country))
            {
                if ((type.Kind & kinds) != 0)
                {
                    result.Add(type);
                }
            }

            return result;
        }

        /// <summary>
        /// Validates a value against the type with the given id or alias. An unknown id is
        /// <see cref="Verdict.NotCovered"/> with a null <see cref="IdentifierResult.Type"/>, not an
        /// exception.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="typeId">The id or alias of the type.</param>
        public static IdentifierResult Validate(string? value, string typeId)
        {
            IdentifierType? type = Registry.Find(typeId);
            if (type == null)
            {
                return new IdentifierResult(Verdict.NotCovered, Reason.None, "Unknown identifier type.", null, null);
            }

            return type.Validate(value);
        }

        /// <summary>
        /// Validates a value against every type of one country at once, so a caller can ask "is this
        /// any business identifier" without naming one. Replaces
        /// <c>CountryValidator.Validate(value, country, kinds)</c>.
        /// </summary>
        /// <param name="value">The identifier to validate.</param>
        /// <param name="country">The country that issued it.</param>
        /// <param name="kinds">
        /// The kinds to accept. <see cref="IdentifierKind.Any"/> (personal and business) is always
        /// evaluated as well, because that is how ambiguity is detected; a kind with no rule never
        /// matches.
        /// </param>
        public static MatchResult Match(string? value, Country country, IdentifierKind kinds = IdentifierKind.Any)
        {
            IdentifierKind evaluate = kinds | IdentifierKind.Any;
            var details = new Dictionary<IdentifierType, IdentifierResult>();
            IdentifierKind matched = default(IdentifierKind);

            foreach (IdentifierType type in Registry.For(country))
            {
                if ((type.Kind & evaluate) == 0)
                {
                    continue;
                }

                IdentifierResult result = type.Validate(value);
                details[type] = result;
                if (result.IsValid)
                {
                    matched |= type.Kind;
                }
            }

            return new MatchResult(kinds, matched, details);
        }

        /// <summary>
        /// Validates an EU-style VAT number with its country prefix ("BE0428759497"): the country
        /// comes from the value. Greece is "EL" in VIES and is accepted as well as "GR". A value
        /// with no recognisable prefix is <see cref="Reason.InvalidPrefix"/>.
        /// </summary>
        /// <param name="value">The prefixed VAT number.</param>
        public static IdentifierResult ValidateVat(string? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value))
            {
                return new IdentifierResult(Verdict.Invalid, Reason.Empty, "The value is empty.", null, null);
            }

            if (value.Length > IdentifierType.MaxInputLength)
            {
                return new IdentifierResult(Verdict.Invalid, Reason.TooLong, "The value is longer than " + IdentifierType.MaxInputLength + " characters.", null, null);
            }

            string compact = Rule.DefaultNormalize(value) ?? string.Empty;
            string prefix = compact.Length >= 2 ? compact.Substring(0, 2) : compact;
            if (prefix == "EL")
            {
                prefix = "GR";
            }

            IdentifierType? type = prefix.Length == 2 ? Registry.Find(prefix + ".vat") : null;
            if (type == null)
            {
                return new IdentifierResult(Verdict.Invalid, Reason.InvalidPrefix, "The value does not start with a known country prefix.", compact.Length == 0 ? null : compact, null);
            }

            return type.Validate(value);
        }
    }
}
