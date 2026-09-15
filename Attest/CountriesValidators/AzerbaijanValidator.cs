using System.Text.RegularExpressions;


namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Azerbaijan.</summary>
    public class AzerbaijanValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Azerbaijan (AZ).</summary>
        public AzerbaijanValidator()
        {
            CountryCode = nameof(Country.AZ);
        }

        /// <summary>
        /// PIN - Personal Identification Number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();

            // The PIN is the seven character code printed on the identity card and read from the
            // machine readable zone, so it is made of Latin letters and digits only. \w would also
            // accept letters of any other script.
            // https://www.e-gov.az/en/services/read/3243/1
            if (!Regex.IsMatch(ssn, @"^[A-Za-z0-9]{7}$"))
            {
                return ValidationResult.InvalidFormat("5VBK5VR");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// VÖEN/TIN Number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{10}$"))
            {
                return ValidationResult.InvalidFormat("1234567890");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// VÖEN/TIN Number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateEntity(ssn);
        }

        /// <summary>
        /// VÖEN/TIN Number
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

        /// <summary>Validates a postal code issued by Azerbaijan.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[Aa][Zz][0-9]{4}$"))
            {
                return ValidationResult.InvalidFormat("CCNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
