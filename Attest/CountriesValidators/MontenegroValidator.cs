using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class MontenegroValidator : IdValidationAbstract
    {
        public MontenegroValidator()
        {
            CountryCode = nameof(Country.ME);
        }

        public override ValidationResult ValidateNationalIdentity(string value)
        {
            value = value.RemoveSpecialCharacthers();

            if (!Regex.IsMatch(value, @"^[0-9]{13}$"))
            {
                return ValidationResult.InvalidFormat("1234567890123");
            }

            try
            {
                int day = int.Parse(value.Substring(0, 2));
                int month = int.Parse(value.Substring(2, 2));
                int year = int.Parse(value.Substring(4, 3));


                if (year >= 800)
                {
                    year = 1000 + year;
                }
                else
                {
                    year = 2000 + year;
                }
                DateTime date = new DateTime(year, month, day);
            }
            catch
            {
                return ValidationResult.InvalidDate();
            }


            int rr = int.Parse(value.Substring(7, 2));
            int k = int.Parse(value.Substring(12, 1));

            // Validate checksum
            var sum = 0;
            for (var i = 0; i < 6; i++)
            {
                sum += (7 - i) * ((int)char.GetNumericValue(value[i]) + (int)char.GetNumericValue(value[i + 6]));
            }
            sum = 11 - sum % 11;
            if (sum == 10 || sum == 11)
            {
                sum = 0;
            }
            if (sum != k)
            {
                return ValidationResult.InvalidChecksum();
            }

            // Validate political region
            // rr is the political region of birth, which can be in ranges:
            // 10-19: Bosnia and Herzegovina
            // 20-29: Montenegro
            // 30-39: Croatia (not used anymore)
            // 41-49: Macedonia
            // 50-59: Slovenia (only 50 is used)
            // 70-79: Central Serbia
            // 80-89: Serbian province of Vojvodina
            // 90-99: Kosovo

            return 20 <= rr && rr <= 29 ? ValidationResult.Success() : ValidationResult.Invalid("Invalid Region. Montenegro region is between 20-29");
        }


        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// For natural persons the tax number assigned by the Poreska uprava is based on the
        /// JMB, so the individual tax code is validated as a Montenegrin JMB.
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateNationalIdentity(ssn);
        }

        /// <summary>
        /// PIB (Poreski Identifikacioni Broj)
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();

            // [0-9] and not \d: in .NET \d also matches non-ASCII Unicode digits.
            if (!Regex.IsMatch(vatId, "^[0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }

            // The last digit is a modulus 11 check digit over the weights 8,7,6,5,4,3,2,
            // a remainder of 10 is written as 0.
            // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.me.pib.html
            int[] weights = new int[] { 8, 7, 6, 5, 4, 3, 2 };

            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(vatId[i]);
            }

            if ((11 - sum % 11) % 11 % 10 != (int)char.GetNumericValue(vatId[7]))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
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
