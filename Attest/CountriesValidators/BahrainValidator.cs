using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class BahrainValidator : IdValidationAbstract
    {
        public BahrainValidator()
        {
            CountryCode = nameof(Country.BH);
        }

        /// <summary>The kinds BH has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.Vat; }
        }

        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{9}$"))
            {
                return ValidationResult.Invalid("123456789");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{3,4}$"))
            {
                return ValidationResult.InvalidFormat("NNN or NNNN");
            }
            return ValidationResult.Success();
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidationResult.Invalid("Not supported");
        }
    }
}
