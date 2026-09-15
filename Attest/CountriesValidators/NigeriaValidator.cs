using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Nigeria.</summary>
    public class NigeriaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Nigeria (NG).</summary>
        public NigeriaValidator()
        {
            CountryCode = nameof(Country.NG);
        }

        /// <summary>
        /// Nigerian National Identification Number (NIN)
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^[0-9]{11}$"))
            {
                return ValidationResult.InvalidFormat("12345678901");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Nigerian company tax number. Deliberately the same rule as
        /// <see cref="ValidateIndividualTaxCode"/>, because no Nigerian tax identifier records
        /// whether its holder is a person or a company. The TIN profile Nigeria filed with the
        /// OECD splits the two legacy formats by issuing authority rather than by holder type:
        /// the 10-digit JTB TIN goes to employed individuals, and also to the registered
        /// corporate taxpayers who apply for one with a CAC number, while the 12-digit FIRS TIN
        /// goes to corporate entities and also to members of the armed forces, the police and
        /// diplomats. The 13-digit Tax ID that replaced both on 1 January 2026 is a single
        /// format generated from the holder's NIN or CAC number, neither of which it carries.
        /// A value valid here is therefore also valid as a personal tax code, and the
        /// IsAmbiguous flag on the result of CountryValidator.Validate reports that honestly.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/nigeria-tin.pdf
        /// https://fctirs.gov.ng/nigerian-tax-id-portal-goes-live/
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateIndividualTaxCode(id);
        }

        /// <summary>
        /// Nigerian TIN, in any of the three forms in circulation. None of them has a published
        /// check digit, so only the length and the character set are validated.
        /// Ten digits is the JTB TIN. Twelve digits is the FIRS TIN, written as eight digits, a
        /// hyphen and a four-digit office number (12345678-0001) before the hyphen is stripped.
        /// Thirteen digits is the Tax ID introduced by the Nigeria Tax Administration Act 2025,
        /// which replaced both from 1 January 2026 and is retrieved from taxid.nrs.gov.ng.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/nigeria-tin.pdf
        /// https://fctirs.gov.ng/nigerian-tax-id-portal-goes-live/
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(ssn, @"^([0-9]{10}|[0-9]{12}|[0-9]{13})$"))
            {
                return ValidationResult.InvalidFormat("1234567890, 12345678-0001 or 1234567890123");
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// The VAT registration number is the TIN: "Nigeria does not issue separate TIN for
        /// different taxes", and the TIN is what appears on VAT certificates and invoices.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/nigeria-tin.pdf
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateIndividualTaxCode(vatId);
        }


        /// <summary>Validates a postal code issued by Nigeria.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{6}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
