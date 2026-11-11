using Attest.Countries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Attest
{
    /// <summary>
    /// The entry point: one object that dispatches to the rules of 87 countries. Construct it once
    /// and keep it -- the country table is built once per process, and nothing here holds state.
    /// </summary>
    /// <example>
    /// <code>
    /// var validator = new CountryValidator();
    /// ValidationResult result = validator.ValidateIndividualTaxCode("93051822361", Country.BE);
    /// </code>
    /// </example>
    [System.Obsolete("Use the static Identifiers class: Identifiers.Validate, Identifiers.Match and Identifiers.For. Removed in 3.0.")]
    public class CountryValidator : ICountryValidator
    {
        static readonly Dictionary<Country, IdValidationAbstract> _supportedCountries = Registry.Validators;

        static readonly IdentifierKind[] _allKinds = Registry.LegacyKinds;

        /// <summary>
        /// Whether this library has any rules for the country. A country it does not know reports
        /// every value invalid; ask <see cref="Supports"/> for a finer answer, kind by kind.
        /// </summary>
        public static bool IsCountrySupported(Country country)
        {
            return _supportedCountries.ContainsKey(country);
        }

        /// <summary>The alpha-2 code of every country this library has rules for.</summary>
        public static List<string> SupportedCountries
        {
            get
            {
                return _supportedCountries.Keys.Select(c => c.ToString()).ToList();
            }
        }

        /// <summary>Validates a natural person's tax code.</summary>
        /// <param name="ssn">The value to validate, with or without the separators it is printed with.</param>
        /// <param name="country">The country whose rule to apply.</param>
        /// <returns>Whether the value is valid, and if not, why.</returns>
        public ValidationResult ValidateIndividualTaxCode(string ssn, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateIndividualTaxCode(ssn);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        /// <summary>Validates a VAT registration number, with or without its country prefix.</summary>
        /// <param name="vat">The value to validate. A leading country prefix is stripped from the front only.</param>
        /// <param name="country">The country whose rule to apply.</param>
        /// <returns>Whether the value is valid, and if not, why.</returns>
        public ValidationResult ValidateVAT(string vat, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateVAT(vat);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        /// <summary>Validates a company or organisation identifier.</summary>
        /// <param name="vat">The value to validate, with or without the separators it is printed with.</param>
        /// <param name="country">The country whose rule to apply.</param>
        /// <returns>Whether the value is valid, and if not, why.</returns>
        public ValidationResult ValidateEntity(string vat, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateEntity(vat);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        /// <summary>Validates a national identity number.</summary>
        /// <param name="ssn">The value to validate, with or without the separators it is printed with.</param>
        /// <param name="country">The country whose rule to apply.</param>
        /// <returns>Whether the value is valid, and if not, why.</returns>
        public ValidationResult ValidateNationalIdentityCode(string ssn, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateNationalIdentity(ssn);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        /// <summary>Validates a postal code.</summary>
        /// <param name="zip">The value to validate, with or without the space it is printed with.</param>
        /// <param name="country">The country whose rule to apply.</param>
        /// <returns>Whether the value is valid, and if not, why.</returns>
        public ValidationResult ValidateZIPCode(string zip, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidatePostalCode(zip);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");
        }
        /// <summary>
        /// Validates a value against a category of identifiers rather than one named method, so a
        /// caller can ask "is this any business identifier for this country" without knowing which
        /// of the country's methods to call.
        /// </summary>
        /// <param name="value">The identifier to validate.</param>
        /// <param name="country">The country that issued it.</param>
        /// <param name="kinds">
        /// The kinds to accept. Defaults to <see cref="IdentifierKind.Any"/>, which covers personal
        /// and business identifiers but not postal codes.
        /// </param>
        /// <remarks>
        /// Every personal and business kind is evaluated regardless of what was requested, because
        /// that is the only way to know whether a match is ambiguous — see
        /// <see cref="LegacyIdentifierResult.IsAmbiguous"/>. Kinds a country has no rule for are reported
        /// as "Not supported" in <see cref="LegacyIdentifierResult.Details"/> and never match.
        /// </remarks>
        public LegacyIdentifierResult Validate(string value, Country country, IdentifierKind kinds = IdentifierKind.Any)
        {
            var evaluate = kinds | IdentifierKind.Any;
            var details = new Dictionary<IdentifierKind, ValidationResult>();
            var matched = default(IdentifierKind);

            foreach (var kind in _allKinds)
            {
                if ((evaluate & kind) == 0)
                {
                    continue;
                }

                var result = ValidateSingleKind(value, country, kind);
                details[kind] = result;
                if (result.IsValid)
                {
                    matched |= kind;
                }
            }

            return new LegacyIdentifierResult(kinds, matched, details);
        }

        /// <summary>
        /// Whether this country has a rule for the given kind. A kind with no rule always reports
        /// the value as invalid, which is not the same answer as "this value is wrong".
        /// </summary>
        /// <param name="country">The country to ask about.</param>
        /// <param name="kind">A single kind. Combinations report true only if every kind is supported.</param>
        public bool Supports(Country country, IdentifierKind kind)
        {
            if (!_supportedCountries.ContainsKey(country))
            {
                return false;
            }

            var validator = _supportedCountries[country];
            var asked = false;
            foreach (var single in _allKinds)
            {
                if ((kind & single) == 0)
                {
                    continue;
                }

                asked = true;
                if ((validator.UnsupportedKinds & single) != 0)
                {
                    return false;
                }
            }

            return asked;
        }

        private ValidationResult ValidateSingleKind(string value, Country country, IdentifierKind kind)
        {
            if (!_supportedCountries.ContainsKey(country))
            {
                return ValidationResult.Invalid("Not supported");
            }

            var validator = _supportedCountries[country];
            if ((validator.UnsupportedKinds & kind) != 0)
            {
                return ValidationResult.Invalid("Not supported");
            }

            return Registry.Run(validator, value, kind);
        }
    }
}
