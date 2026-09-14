namespace Attest
{
    public abstract class IdValidationAbstract
    {

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
        public virtual ValidationResult ValidateNationalIdentity(string ssn)
        {
            return ValidateIndividualTaxCode(ssn);
        }

        public abstract ValidationResult ValidateIndividualTaxCode(string id);
        public abstract ValidationResult ValidateEntity(string id);
        public abstract ValidationResult ValidateVAT(string vatId);
        public abstract ValidationResult ValidatePostalCode(string postalCode);
    }
}
