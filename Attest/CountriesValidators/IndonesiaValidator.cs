using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class IndonesiaValidator : IdValidationAbstract
    {

        public IndonesiaValidator()
        {
            CountryCode = nameof(Country.ID);
        }

        /// <summary>
        /// NPWP
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (ssn.Length == 12)
            {
                ssn += "000";
            }

            if (!Regex.IsMatch(ssn, @"^\d{15}$"))
            {
                return ValidationResult.InvalidFormat("ST.sss.sss.C-OOO.BBB");
            }
            else if (!Regex.IsMatch(ssn, @"^\d[0123]"))
            {
                return ValidationResult.Invalid("Second digit must be between 0-3");
            }
            // The check digit is the 9th digit and covers the first 8.
            // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.id.npwp
            else if (!ssn.Substring(0, 9).CheckLuhnDigit())
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// NPWP 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            /*
             *   "T" denotes taxpayer type code (0 = government treasury [bendahara
    pemerintah], 1-3 = company/organization [badan], 4/6 = invidual
    entrepreneur [pengusaha perorangan], 5 = civil servants [pegawai negeri,
    PNS], 7-9 = individual employee [pegawai perorangan]).
             */

            id = id.RemoveSpecialCharacthers();
            if (id.Length == 12)
            {
                id += "000";
            }

            if (!Regex.IsMatch(id, @"^\d{15}$"))
            {
                return ValidationResult.InvalidFormat("ST.sss.sss.C-OOO.BBB");
            }
            else if (!Regex.IsMatch(id, @"^\d[456789]"))
            {
                return ValidationResult.Invalid("Second digit must be between 4-9");
            }
            // The check digit is the 9th digit and covers the first 8.
            // https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.id.npwp
            else if (!id.Substring(0, 9).CheckLuhnDigit())
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// NPWP - Nomor Pokok Wajib Pajak  
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
            if (!Regex.IsMatch(postalCode, "^\\d{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
