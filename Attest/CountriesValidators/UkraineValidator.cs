using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Ukraine.</summary>
    public class UkraineValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Ukraine (UA).</summary>
        public UkraineValidator()
        {
            CountryCode = nameof(Country.UA);
        }

        /// <summary>
        /// ЄДРПОУ (EDRPOU), the registry code of a legal entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }

            // The weights are shifted by one position for the 3xxxxxxx..5xxxxxxx range, and a
            // remainder of 10 makes the sum be taken again with every weight raised by two.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ua/edrpou.py
            int[] multipliers = id[0] >= '3' && id[0] <= '5'
                ? new[] { 7, 1, 2, 3, 4, 5, 6 }
                : new[] { 1, 2, 3, 4, 5, 6, 7 };

            var checkDigit = id.Sum(multipliers) % 11;
            if (checkDigit > 9)
            {
                for (var index = 0; index < multipliers.Length; index++)
                {
                    multipliers[index] += 2;
                }
                checkDigit = id.Sum(multipliers) % 11 % 10;
            }

            return checkDigit == id[7].ToInt()
                ? ValidationResult.Success()
                : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// РНОКПП (RNTRC), the registration number of an individual taxpayer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^[0-9]{10}$"))
            {
                return ValidationResult.InvalidFormat("1234567890");
            }

            // The first weight is negative, so the weighted sum has to be brought back into
            // 0..10 before the check digit is read off it.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ua/rntrc.py
            int[] multipliers = { -1, 5, 7, 9, 4, 6, 10, 5, 7 };
            var checkDigit = id.Sum(multipliers).Mod(11) % 10;

            return checkDigit == id[9].ToInt()
                ? ValidationResult.Success()
                : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// ІПН, the number of a VAT payer
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            // Twelve digits: for a legal entity the first seven are the ЄДРПОУ without its own
            // check digit, then two for the oblast and two for the district; for an individual the
            // first ten are the РНОКПП. The twelfth digit is a check digit whose algorithm the tax
            // authority sets but does not publish, so only the format is validated.
            // https://uk.wikipedia.org/wiki/Індивідуальний_податковий_номер_платника_ПДВ
            vatId = vatId.RemoveSpecialCharacthers();

            // A sole trader registered for VAT before 09.03.2020 holds a ten digit IPN -- their
            // RNOKPP -- and keeps it until the VAT registration is cancelled. Order 30 of the
            // Ministry of Finance, of 29.01.2020, made the number twelve digits for registrations
            // from that date on; it did not renumber the ones already issued. Rejecting the ten
            // digit form was a false negative on a live registration, and unlike the twelve digit
            // form its check digit is published, so this path validates more rather than less.
            // https://obcity.gov.ua/2020/04/01/individualniy-podatkoviy-nomer-platnika-pdv-u-fop/
            if (vatId.Length == 10)
            {
                return ValidateIndividualTaxCode(vatId);
            }

            if (!Regex.IsMatch(vatId, @"^[0-9]{12}$"))
            {
                return ValidationResult.InvalidFormat("123456789012 or 1234567890");
            }

            return ValidationResult.Success();
        }

        /// <summary>Validates a postal code issued by Ukraine.</summary>
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
