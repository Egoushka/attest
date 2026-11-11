#nullable enable
using System;
using System.Collections.Generic;

namespace Attest
{
    /// <summary>A verdict together with the parts decoded from the value, when it was valid.</summary>
    public readonly struct ParseResult
    {
        internal ParseResult(IdentifierResult result, DateTime? birthDate, Sex? sex, Country? issuingCountry, IReadOnlyDictionary<string, string>? parts)
        {
            Result = result;
            BirthDate = birthDate;
            Sex = sex;
            IssuingCountry = issuingCountry;
            _parts = parts;
        }

        private readonly IReadOnlyDictionary<string, string>? _parts;

        /// <summary>The verdict. Nothing below is filled in unless it is valid.</summary>
        public IdentifierResult Result { get; }

        /// <summary>The birth date, for personal ids that encode one.</summary>
        public DateTime? BirthDate { get; }

        /// <summary>Female or male, when the identifier encodes it.</summary>
        public Sex? Sex { get; }

        /// <summary>The country, for prefixed or multi-country types.</summary>
        public Country? IssuingCountry { get; }

        /// <summary>Type-specific parts such as "region", "serial" or "check". Empty for an invalid value.</summary>
        public IReadOnlyDictionary<string, string> Parts => _parts ?? EmptyParts;

        private static readonly IReadOnlyDictionary<string, string> EmptyParts = new Dictionary<string, string>();
    }
}
