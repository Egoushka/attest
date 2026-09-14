using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class MalaysiaValidator : IdValidationAbstract
    {
        public MalaysiaValidator()
        {
            CountryCode = nameof(Country.MY);
        }

        /// <summary>
        ///   Nombor Cukai Pendapatan (ITN)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();

            // Non-individual TIN codes and lengths per IRBM/OECD "Information on Tax Identification Numbers - Malaysia"
            // (updated March 2023): https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/malaysia-tin.pdf
            // Effective 2 January 2023 a trailing "0" was appended, so 10 or 11 digits follow the code.
            if (!Regex.IsMatch(id, @"^(CS|FA|PT|TA|TC|TN|TR|TP|LE|C|D|E|F|J)[0-9]{10,11}$"))
            {
                return ValidationResult.InvalidFormat("C20880050010");
            }
            return ValidationResult.Success();

        }


        /// <summary>
        /// Nombor Cukai Pendapatan (ITN)  
        /// </summary>
        /// <param name="itn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string itn)
        {
            itn = itn.RemoveSpecialCharacthers();

            // Individual TIN: the SG (non-business) and OG (business) codes were converted to IG on
            // 2 January 2023, the digits unchanged; 9 to 11 digits follow the code (IG115002000,
            // IG4040080091, IG56003500070). Same source as ValidateEntity.
            if (!Regex.IsMatch(itn, @"^(IG|SG|OG)[0-9]{9,11}$"))
            {
                return ValidationResult.InvalidFormat("IG56003500070");
            }
            return ValidationResult.Success();

        }

        /// <summary>
        ///  Nombor Cukai Pendapatan (ITN)
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
