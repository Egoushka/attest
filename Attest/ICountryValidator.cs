namespace Attest
{
    /// <summary>
    /// The questions this library answers about a value. <see cref="CountryValidator"/> is the
    /// implementation; the interface is here so a consumer can substitute its own in a test.
    /// </summary>
    public interface ICountryValidator
    {
        /// <summary>Validates a company or organisation identifier.</summary>
        ValidationResult ValidateEntity(string vat, Country country);

        /// <summary>Validates a natural person's tax code.</summary>
        ValidationResult ValidateIndividualTaxCode(string id, Country country);

        /// <summary>Validates a national identity number.</summary>
        ValidationResult ValidateNationalIdentityCode(string ssn, Country country);

        /// <summary>Validates a VAT registration number, with or without its country prefix.</summary>
        ValidationResult ValidateVAT(string vat, Country country);

        /// <summary>Validates a postal code.</summary>
        ValidationResult ValidateZIPCode(string zip, Country country);

        /// <summary>
        /// Asks by category rather than by method: whether the value is valid as any of
        /// <paramref name="kinds"/>, and which ones it matched. Use this when the question is "is
        /// this any business identifier for this country?" rather than a named format.
        /// </summary>
        IdentifierResult Validate(string value, Country country, IdentifierKind kinds = IdentifierKind.Any);

        /// <summary>
        /// Whether the country has a rule for that kind at all. A kind with no rule reports every
        /// value invalid, which is not a verdict on the value -- ask this first before reading one
        /// as a rejection.
        /// </summary>
        bool Supports(Country country, IdentifierKind kind);

    }
}
