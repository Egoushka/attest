using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Bosnia.</summary>
    public class BosniaValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Bosnia (BA).</summary>
        public BosniaValidator()
        {
            CountryCode = nameof(Country.BA);
        }


        /// <summary>
        /// Unique Master Citizen Number JMBG
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

            return 10 <= rr && rr <= 19 ? ValidationResult.Success() : ValidationResult.Invalid("Invalid Region. Bosnia and Herzegovina region is between 10-19");
        }


        /// <summary>
        /// JIB (Jedinstveni identifikacioni broj)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();

            // Thirteen digits, the first of which is the "4" common to every JIB issued by the
            // entity tax administrations and by the Uprava za indirektno oporezivanje.
            // Pravilnik o dodjeljivanju identifikacionih brojeva i poreznoj registraciji
            // (Porezna uprava FBiH), clan 11 and 14:
            // https://www.pufbih.ba/v1/public/upload/zakoni/96237-pravilnik-o-dodjeljivanju-id-brojeva.pdf
            // Pravilnik o uslovima i nacinu registracije i identifikacije poreskih obveznika
            // ("Sluzbeni glasnik RS" broj 4/13), clan 11 to 13.
            if (!Regex.IsMatch(id, "^4[0-9]{12}$"))
            {
                return ValidationResult.InvalidFormat("4123456789012");
            }

            return ValidateJibChecksum(id);
        }

        /// <summary>
        /// Validates a natural person's tax code, which here is the same number
        /// <see cref="ValidateNationalIdentity"/> validates.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return ValidateNationalIdentity(ssn);
        }

        /// <summary>
        /// Identifikacioni broj of an indirect tax payer: the thirteen digit JIB with its
        /// leading "4" removed, so twelve digits. Pravilnik o registraciji i upisu u Jedinstveni
        /// registar obveznika indirektnih poreza ("Sluzbeni glasnik BiH" broj 51/12), clan 19 and 21:
        /// https://www.uino.gov.ba/portal/wp-content/uploads/PROPISI/2_Porezi/1_PDV/2_Pravilnici/B/B-2-Pravilnik-o-registraciji-i-upisu-u-Jedinstveni-registar-obveznika-indirektnih-poreza-Sluzbeni-glasnik-BiH-broj-5112b.pdf
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();

            if (!Regex.IsMatch(vatId, "^[0-9]{12}$"))
            {
                return ValidationResult.InvalidFormat("123456789012");
            }

            // Only the leading digit is dropped, so the check digit is still the one calculated
            // over the thirteen digit JIB.
            return ValidateJibChecksum("4" + vatId);
        }

        private ValidationResult ValidateJibChecksum(string jib)
        {
            // Both rulebooks say the thirteenth digit is a check digit "po modulu 11" over the
            // preceding twelve but neither prints the weights. The weights 7,6,5,4,3,2 twice,
            // with a remainder of 10 written as 0, hold for 29318 of the 29319 JIBs in the
            // Kanton Sarajevo taxpayer list published by the Porezna uprava FBiH
            // (https://pufbih.ba/v1/public/upload/files/Kanton%20Sarajevo%202.pdf) and for every
            // Republika Srpska JIB checked against it. It is the same calculation North Macedonia
            // uses for the EDB.
            int[] weights = new int[] { 7, 6, 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

            int sum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i] * (int)char.GetNumericValue(jib[i]);
            }

            if ((11 - sum % 11) % 11 % 10 != (int)char.GetNumericValue(jib[12]))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        /// <summary>Validates a postal code issued by Bosnia.</summary>
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
