using System;
using System.Globalization;
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
            return ValidateNpwp(ssn, individual: false);
        }

        /// <summary>
        /// NPWP 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            return ValidateNpwp(id, individual: true);
        }

        /// <summary>
        /// NPWP. Until 2024 it was 15 digits: 2 digits of taxpayer type, 6 identifying the
        /// taxpayer, a Luhn check digit over the first 8, 3 digits for the local tax office and
        /// 3 for the branch. Since 2024 it is 16 digits, either the legacy number with a leading
        /// 0 - which moves the check digit to the 10th position - or, for an Indonesian citizen,
        /// the NIK itself.
        /// "T" denotes taxpayer type code (0 = government treasury [bendahara pemerintah],
        /// 1-3 = company/organization [badan], 4/6 = invidual entrepreneur [pengusaha
        /// perorangan], 5 = civil servants [pegawai negeri, PNS], 7-9 = individual employee
        /// [pegawai perorangan]).
        /// https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.id.npwp
        /// </summary>
        private static ValidationResult ValidateNpwp(string npwp, bool individual)
        {
            npwp = npwp.RemoveSpecialCharacthers();
            if (npwp.Length == 12)
            {
                npwp += "000";
            }

            var taxpayerTypes = individual ? "456789" : "0123";
            var typeError = individual
                ? "Taxpayer type must be between 4-9"
                : "Taxpayer type must be between 0-3";

            int typeIndex;
            int checkedLength;
            if (Regex.IsMatch(npwp, "^[0-9]{15}$"))
            {
                typeIndex = 1;
                checkedLength = 9;
            }
            else if (Regex.IsMatch(npwp, "^0[0-9]{15}$"))
            {
                typeIndex = 2;
                checkedLength = 10;
            }
            else if (Regex.IsMatch(npwp, "^[0-9]{16}$"))
            {
                // A NIK belongs to a person, so it is never an organisation's NPWP.
                return individual ? ValidateNik(npwp) : ValidationResult.Invalid(typeError);
            }
            else
            {
                return ValidationResult.InvalidFormat("ST.sss.sss.C-OOO.BBB");
            }

            if (taxpayerTypes.IndexOf(npwp[typeIndex]) < 0)
            {
                return ValidationResult.Invalid(typeError);
            }
            return npwp.Substring(0, checkedLength).CheckLuhnDigit()
                ? ValidationResult.Success()
                : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// NIK (Nomor Induk Kependudukan), PPRRSSDDMMYYXXXX: 6 digits of registration place,
        /// then the birth date as DDMMYY with 40 added to the day for women, then a 4 digit
        /// sequence number. There is no check digit. The registration place is not verified
        /// here: python-stdnum checks it against a full province/regency/district table, which
        /// this library does not carry.
        /// The two digit year is resolved against the 2000s, as MexicoValidator does for the
        /// RFC: the century only changes the answer for 29 February of a century year, and 2000
        /// is a leap year while 1900 is not.
        /// https://arthurdejong.org/python-stdnum/doc/2.1/stdnum.id.nik
        /// </summary>
        private static ValidationResult ValidateNik(string nik)
        {
            var day = int.Parse(nik.Substring(6, 2), CultureInfo.InvariantCulture);
            if (day > 40)
            {
                day -= 40;
            }

            var date = "20" + nik.Substring(10, 2) + nik.Substring(8, 2) + day.ToString("00", CultureInfo.InvariantCulture);
            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return ValidationResult.InvalidDate();
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
            if (!Regex.IsMatch(postalCode, "^[0-9]{5}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
