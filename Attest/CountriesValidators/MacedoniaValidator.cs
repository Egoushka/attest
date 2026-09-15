using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Macedonia.</summary>
    public class MacedoniaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Macedonia (MK).</summary>
        public MacedoniaValidator()
        {
            CountryCode = nameof(Country.MK);
        }

        /// <summary>Validates a national identification number issued by Macedonia.</summary>
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

            return 41 <= rr && rr <= 49 ? ValidationResult.Success() : ValidationResult.Invalid("Invalid Region. Macedonia region is between 41-49");
        }


        /// <summary>Validates a company identifier issued by Macedonia.</summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        /// <summary>
        /// The EMBG of a citizen of North Macedonia is also his tax number, so the individual
        /// tax code is validated as an EMBG (JMBG). Pravilnik za postapkata, nacinot i rokovite
        /// za dodeluvanje na edinstven danocen broj ("Sluzben vesnik na RM" br. 161/2009), clen 2(4).
        /// https://www.ujp.gov.mk/files/attachment/0000/0154/Pravilnik_za_postapkata_nacinot_i_rokovite_za_dodeluvanje_na_edinstven_danocen_broj_161_09__od_31.12.2009.pdf
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateNationalIdentity(ssn);
        }

        /// <summary>
        /// ЕДБ (Единствен Даночен Број)
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            // The prefix is written with either Latin or Cyrillic letters.
            vatId = vatId.StripPrefix("MK").StripPrefix("МК");

            // [0-9] and not \d: in .NET \d also matches non-ASCII Unicode digits, which
            // char.GetNumericValue below would happily read as a number no register issued.
            if (!Regex.IsMatch(vatId, "^[0-9]{13}$"))
            {
                return ValidationResult.InvalidFormat("MK1234567890123");
            }

            // The last digit is a modulus 11 check digit, a remainder of 10 is written as 0.
            // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.mk.edb.html
            if ((int)char.GetNumericValue(vatId[12]) != CalculateChecksum(vatId.Substring(0, 12)))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        private int CalculateChecksum(string number)
        {
            int[] weights = new int[] { 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(number[i]);
            }

            return (11 - sum % 11) % 11 % 10;
        }

        /// <summary>Validates a postal code issued by Macedonia.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{4}$"))
            {
                return ValidationResult.InvalidFormat("NNNN");
            }
            return ValidationResult.Success();
        }
    }
}
