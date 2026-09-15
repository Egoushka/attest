namespace Attest
{
    /// <summary>
    /// What every country validator answers. The per-country classes derive from this, and README
    /// documents calling one of them directly -- <c>new BelgiumValidator().ValidateVAT(...)</c> --
    /// as a supported entry point alongside <see cref="CountryValidator"/>.
    /// </summary>
    /// <remarks>
    /// Every method here answers rather than throws, for any input at all: null, an empty string, a
    /// value in the wrong script, a value far too long. Twenty-two validators used to throw instead,
    /// and <c>ValidatorClassSweepTests</c> now proves that none does.
    /// </remarks>
    public abstract class IdValidationAbstract
    {

        /// <summary>The ISO 3166-1 alpha-2 code of the country whose rules this validator encodes.</summary>
        public string CountryCode { get; protected set; }

        /// <summary>
        /// The kinds this country has no rule for. <see cref="CountryValidator.Supports"/> reads it,
        /// so a validator that cannot answer for a kind says so here rather than by throwing.
        /// </summary>
        /// <remarks>
        /// Eleven validators used to signal this by throwing NotSupportedException, which the facade
        /// caught and turned into Invalid("Not supported"). That made the throw load-bearing: it was
        /// the only signal Supports had, so it could not be removed one validator at a time, and it
        /// escaped to anyone calling a validator class directly — which README documents as a
        /// supported entry point. Declaring it instead lets every Validate* method keep the
        /// never-throw contract.
        /// </remarks>
        internal virtual IdentifierKind UnsupportedKinds
        {
            get { return default(IdentifierKind); }
        }
        /// <summary>
        /// Validates a national identity number. Where the country issues one number for both
        /// purposes, this defers to <see cref="ValidateIndividualTaxCode"/>.
        /// </summary>
        public virtual ValidationResult ValidateNationalIdentity(string ssn)
        {
            return ValidateIndividualTaxCode(ssn);
        }

        /// <summary>Validates a natural person's tax code.</summary>
        public abstract ValidationResult ValidateIndividualTaxCode(string id);

        /// <summary>Validates a company or organisation identifier.</summary>
        public abstract ValidationResult ValidateEntity(string id);

        /// <summary>Validates a VAT registration number, with or without its country prefix.</summary>
        public abstract ValidationResult ValidateVAT(string vatId);

        /// <summary>Validates a postal code.</summary>
        public abstract ValidationResult ValidatePostalCode(string postalCode);
    }
}
