using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class IcelandValidator : IdValidationAbstract
    {
        public IcelandValidator()
        {
            CountryCode = nameof(Country.IS);
        }
        /// <summary>
        /// Kennitala issued to an organisation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateKennitala(id, isOrganisation: true);
        }


        /// <summary>
        /// Kennitala
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string value)
        {
            return ValidateKennitala(value, isOrganisation: false);
        }

        /// <summary>
        /// A kennitala encodes the holder type in its day field: an individual carries the day of
        /// birth (01-31), while an organisation carries its day of registration with 40 added to it
        /// (41-71). The two ranges cannot overlap, so the number itself says which one it belongs to.
        /// https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/is_/kennitala.py
        /// </summary>
        private ValidationResult ValidateKennitala(string value, bool isOrganisation)
        {
            value = value.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(value, "^[0-9]{6}[0-9]{4}$"))
            {
                return ValidationResult.InvalidFormat("Invalid format");
            }
            try
            {
                var day = int.Parse(value.Substring(0, 2));
                var month = int.Parse(value.Substring(2, 2));
                var year = int.Parse(value.Substring(4, 2));
                var century = (int)char.GetNumericValue(value[9]);

                if (isOrganisation)
                {
                    if (day < 41 || day > 71)
                    {
                        return ValidationResult.Invalid("The code does not belong to an organisation");
                    }
                    day -= 40;
                }
                else if (day > 31)
                {
                    return ValidationResult.Invalid("The code does not belong to an individual");
                }

                year = (century == 9) ? (1900 + year) : ((20 + century) * 100 + year);
                DateTime date = new DateTime(year, month, day);
            }
            catch
            {
                return ValidationResult.InvalidDate();
            }

            var sum = 0;
            var weight = new int[] { 3, 2, 7, 6, 5, 4, 3, 2 };
            for (var i = 0; i < 8; i++)
            {
                sum += (int)char.GetNumericValue(value[i]) * weight[i];
            }
            // Check digit is (11 - weighted sum mod 11) mod 11; a remainder of 0 yields check digit 0,
            // and a remainder of 1 yields 10, which never matches a single digit.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/is_/kennitala.py
            sum = (11 - sum % 11) % 11;
            return sum == (int)char.GetNumericValue(value[8]) ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }


        /// <summary>
        /// Virdisaukaskattsnumer (VSK)  
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            vatId = vatId.StripPrefix("IS");

            if (Regex.IsMatch(vatId, @"^[0-9]{5,6}$"))
            {
                return ValidationResult.Success();
            }
            return ValidationResult.Invalid("The VAT code should have 5 or 6 digits  ");
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{3}$"))
            {
                return ValidationResult.InvalidFormat("NNN");
            }
            return ValidationResult.Success();
        }
    }
}
