using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class MaltaValidator : IdValidationAbstract
    {

        public MaltaValidator()
        {
            CountryCode = nameof(Country.MT);
        }


        /// <summary>
        /// Taxpayer reference number issued by the IRD to entities resident in Malta: 9 digits.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/malta-tin.pdf
        /// </summary>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers().ToUpperInvariant();
            if (id.Length != 9)
            {
                return ValidationResult.InvalidLength();
            }
            if (!Regex.IsMatch(id, @"^[0-9]{9}$"))
            {
                return ValidationResult.InvalidFormat("123456789");
            }
            return ValidationResult.Success();
        }


        /// <summary>
        /// Maltese nationals use their identity card number as TIN: 7 digits plus one of M, G, A, P, L, H, B, Z.
        /// Individuals who are not Maltese nationals get a 9 digit taxpayer reference number instead.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/malta-tin.pdf
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers().ToUpperInvariant();
            if (Regex.IsMatch(id, @"^[0-9]{9}$"))
            {
                return ValidationResult.Success();
            }
            if (id.Length > 8 || id.Length < 4)
            {
                return ValidationResult.InvalidLength();
            }
            // The first four digits may be omitted when they are zero, but the stored form is always 8 characters.
            id = id.PadLeft(8, '0');
            if (!Regex.IsMatch(id, @"^[0-9]{7}[MGAPLHBZ]$"))
            {
                return ValidationResult.InvalidFormat("1234567M");
            }
            return ValidationResult.Success();
        }


        /// <summary>
        /// VAT Number (VAT). Eight digits, the last two of which are the check value
        /// 37 - (weighted sum of the first six mod 37). That value is a two digit number in
        /// 01-37, so when the weighted sum is an exact multiple of 37 the check digits are "37"
        /// and not "00" - vat-validator codes the same case explicitly as "(r == 0 and c7_c8 == 37)".
        /// python-stdnum instead weights all eight digits by 3,4,6,7,8,9,10,1 and asks for a
        /// multiple of 37, which is the same test loosened to also accept the check value plus 37
        /// or plus 74 (so "00" and "74" here, and 49 and 86 wherever 12 is correct).
        /// https://vat-validator.readthedocs.io/en/latest/_modules/vat_validator/countries.html
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers().ToUpperInvariant();
            vatId = vatId.StripPrefix("MT");
            if (!Regex.IsMatch(vatId, @"^[1-9][0-9]{7}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }

            int[] multipliers = { 3, 4, 6, 7, 8, 9 };
            var sum = vatId.Sum(multipliers);

            var checkDigit = 37 - sum % 37;

            return checkDigit == int.Parse(vatId.Substring(6, 2)) ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Seven character postcode: three letters and four digits (e.g. FRN1913). The pre 2007
        /// five character format (three letters and two digits) is no longer issued.
        /// https://www.mca.org.mt/sites/default/files/Postcodes%20Decision_1.pdf
        /// </summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers().ToUpperInvariant();
            if (!Regex.IsMatch(postalCode, @"^[A-Z]{3}[0-9]{4}$"))
            {
                return ValidationResult.InvalidFormat("AAANNNN OR (AAA NNNN)");
            }
            return ValidationResult.Success();
        }
    }
}
