#nullable enable
using System.Text;

namespace Attest
{
    /// <summary>What a rule says about one value: <see cref="Reason.None"/> is valid.</summary>
    internal readonly struct Outcome
    {
        internal Outcome(Reason reason, string? detail)
        {
            Reason = reason;
            Detail = detail;
        }

        internal Reason Reason { get; }

        internal string? Detail { get; }

        internal static Outcome Valid => new Outcome(Reason.None, null);
    }

    /// <summary>
    /// One identifier type's logic. A rule answers for a non-empty value within the input bound;
    /// the bounds, null handling and exception guard live in <see cref="IdentifierType"/>, so a
    /// rule cannot forget them.
    /// </summary>
    internal abstract class Rule
    {
        /// <summary>Provenance and coverage.</summary>
        internal abstract RuleInfo Info { get; }

        /// <summary>Whether Attest has an opinion at all; false makes the verdict NotCovered.</summary>
        internal virtual bool HasRule => Info.Coverage == Coverage.Full || Info.Coverage == Coverage.FormatOnly;

        /// <summary>Validates a value that is non-empty and within the bound.</summary>
        internal abstract Outcome Check(string value);

        /// <summary>Compact canonical form of a non-empty value; null when nothing is left.</summary>
        internal virtual string? Normalize(string value)
        {
            return DefaultNormalize(value);
        }

        /// <summary>Printed form of a value that is already valid; compact unless overridden.</summary>
        internal virtual string Format(string normalized)
        {
            return normalized;
        }

        /// <summary>
        /// Strips the separators every type documents -- '.', '-', space and '/' -- and upper-cases
        /// with the invariant culture. It removes nothing else, so a wrong character stays wrong.
        /// </summary>
        internal static string? DefaultNormalize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c == '.' || c == '-' || c == ' ' || c == '/')
                {
                    continue;
                }

                builder.Append(c);
            }

            return builder.Length == 0 ? null : builder.ToString().ToUpperInvariant();
        }
    }
}
