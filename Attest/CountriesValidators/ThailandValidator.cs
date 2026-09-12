using System;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class ThailandValidator : IdValidationAbstract
    {
        /// <summary>
        /// Who a thirteen digit Thai number was issued to. The number itself says so, because its
        /// first three digits name the issuing agency.
        /// </summary>
        [Flags]
        private enum Holder
        {
            None = 0,

            /// <summary>Personal number issued by the Department of Provincial Administration.</summary>
            Citizen = 1,

            /// <summary>Foreign individual or undivided estate, issued by the Revenue Department.</summary>
            ForeignIndividual = 2,

            /// <summary>Juristic person, body of persons or income payer.</summary>
            Business = 4,

            Individual = Citizen | ForeignIndividual,

            Any = Individual | Business
        }

        public ThailandValidator()
        {
            CountryCode = nameof(Country.TH);
        }

        /// <summary>
        /// Validate Thailand citizen number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            return Validate(ssn, Holder.Citizen);
        }

        /// <summary>
        /// A juristic person is registered with the Department of Business Development and is never
        /// given a personal number, so its registration number is rejected here just as a personal
        /// number is rejected by <see cref="ValidateEntity"/>. Besides the citizen number this also
        /// accepts the number the Revenue Department issues to a foreign individual or an undivided
        /// estate, which is an individual taxpayer without a citizen number.
        /// </summary>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            return Validate(ssn, Holder.Individual);
        }

        public override ValidationResult ValidateEntity(string id)
        {
            return Validate(id, Holder.Business);
        }

        /// <summary>
        /// VAT is registered against the taxpayer identification number itself, so a sole trader
        /// registers with a personal number and a company with its registration number. Both are
        /// accepted here.
        /// </summary>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return Validate(vatId, Holder.Any);
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

        /// <summary>
        /// Thai tax identification numbers are thirteen digits since 1 February 2012 and carry the
        /// same check digit whoever holds them, but they are not one undivided series: the first
        /// three digits are the issuing agency and with it the holder type.
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/thailand-tin.pdf
        /// </summary>
        private static ValidationResult Validate(string id, Holder accepted)
        {
            id = id.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(id, @"^\d{13}$"))
            {
                return ValidationResult.InvalidLength();
            }

            var sum = 0;
            for (var i = 0; i < 12; i++)
            {
                sum += (int)Char.GetNumericValue(id[i]) * (13 - i);
            }

            if ((11 - sum % 11).Mod(10) != (int)char.GetNumericValue(id[12]))
            {
                return ValidationResult.InvalidChecksum();
            }

            var holder = HolderOf(id);
            if (holder == Holder.None)
            {
                return ValidationResult.Invalid("Invalid code. Unknown issuing agency.");
            }

            if ((holder & accepted) != 0)
            {
                return ValidationResult.Success();
            }

            if (accepted == Holder.Business)
            {
                return ValidationResult.Invalid("Invalid code. The number is not issued to a juristic person.");
            }

            if (accepted == Holder.Citizen)
            {
                return ValidationResult.Invalid("Invalid code. The number is not a citizen identification number.");
            }

            return ValidationResult.Invalid("Invalid code. The number is not issued to an individual.");
        }

        /// <summary>
        /// Digits one to three are the issuing agency: 100-999 the Department of Provincial
        /// Administration, 010-096 the Department of Business Development (a zero followed by the
        /// province code of the registrar) and 099 the Revenue Department. In a Revenue Department
        /// number the fourth digit is the taxpayer category: 1 foreign individual or undivided
        /// estate, 2 body of persons that is not a juristic person, 3 juristic person, 4 income
        /// payer. Section II of
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/thailand-tin.pdf
        /// </summary>
        private static Holder HolderOf(string id)
        {
            var agency = int.Parse(id.Substring(0, 3));

            if (agency >= 100)
            {
                return Holder.Citizen;
            }

            if (agency >= 10 && agency <= 96)
            {
                return Holder.Business;
            }

            if (agency != 99)
            {
                return Holder.None;
            }

            switch (id[3])
            {
                case '1':
                    return Holder.ForeignIndividual;
                case '2':
                case '3':
                case '4':
                    return Holder.Business;
                default:
                    return Holder.None;
            }
        }
    }
}
