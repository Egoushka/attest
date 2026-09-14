using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class UnitedArabEmiratesValidator : IdValidationAbstract
    {
        public UnitedArabEmiratesValidator()
        {
            CountryCode = nameof(Country.AE);
        }

        /// <summary>The kinds AE has no published rule for.</summary>
        internal override IdentifierKind UnsupportedKinds
        {
            get { return IdentifierKind.CompanyNumber | IdentifierKind.PersonalTaxCode | IdentifierKind.PostalCode | IdentifierKind.Vat; }
        }

        /*
         * 
         *     "784-1980-1234567-9",
    "123-1234-0123456-7",
    "784198012345679"
         */

        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^784[0-9]{4}[0-9]{7}[0-9]{1}$"))
            {
                return ValidationResult.InvalidFormat("xxx-xxxx-xxxxxxx-x");
            }

            return ValidationResult.Success();
        }

        public override ValidationResult ValidateEntity(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            return ValidationResult.Invalid("Not supported");
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            return ValidationResult.Invalid("Not supported");
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidationResult.Invalid("Not supported");
        }
    }
}
