using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Luxembourg.</summary>
    public class LuxembourgValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Luxembourg (LU).</summary>
        public LuxembourgValidator()
        {
            CountryCode = nameof(Country.LU);
        }

        /// <summary>Validates a company identifier issued by Luxembourg.</summary>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateVAT(id);
        }

        readonly int[,] _d = {
             { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
             { 1, 2, 3, 4, 0, 6, 7, 8, 9, 5},
             {2, 3, 4, 0, 1, 7, 8, 9, 5, 6},
             {3, 4, 0, 1, 2, 8, 9, 5, 6, 7},
             {4, 0, 1, 2, 3, 9, 5, 6, 7, 8},
             {5, 9, 8, 7, 6, 0, 4, 3, 2, 1},
             {6, 5, 9, 8, 7, 1, 0, 4, 3, 2},
             {7, 6, 5, 9, 8, 2, 1, 0, 4, 3},
             {8, 7, 6, 5, 9, 3, 2, 1, 0, 4},
             {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
        };

        readonly int[,] _p = {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        {1, 5, 7, 6, 2, 8, 3, 0, 9, 4},
        {5, 8, 0, 3, 7, 9, 6, 1, 4, 2},
        {8, 9, 1, 6, 0, 4, 3, 5, 2, 7},
        {9, 4, 5, 3, 1, 2, 6, 8, 7, 0},
        {4, 2, 8, 6, 5, 7, 3, 9, 0, 1},
        {2, 7, 9, 3, 8, 0, 6, 4, 1, 5},
        {7, 0, 4, 6, 9, 1, 3, 2, 5, 8}
    };


        /// <summary>
        /// Validates the eleven digit national number of a Luxembourg resident.
        /// </summary>
        public ValidationResult ValidateResident(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();
            if (ssn?.Length != 11)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!ssn.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("12345678901");
            }

            int[] mutlipliers = new int[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < mutlipliers.Length; i++)
            {
                sum += (int)char.GetNumericValue(ssn[i]) * mutlipliers[i];
            }
            int check = 11 - sum % 11;
            return (int)char.GetNumericValue(ssn[ssn.Length - 1]) == check ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string ssn)
        {
            ValidationResult validationResult = ValidateNaturalPersons(ssn);
            if (validationResult.IsValid)
            {
                return validationResult;
            }
            else if (ValidateResident(ssn).IsValid)
            {
                return ValidationResult.Success();
            }
            return validationResult;
        }

        /// <summary>
        /// Validates the thirteen digit number Luxembourg issues to a natural person, which opens
        /// with the date of birth.
        /// </summary>
        public ValidationResult ValidateNaturalPersons(string ssn)
        {
            ssn = ssn.RemoveSpecialCharacthers();

            if (ssn.Length != 13)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!Regex.IsMatch(ssn, "(1[89]|20)[0-9]{2}(0[1-9]|1[012])(0[1-9]|[1-2][0-9]|3[0-1])[0-9]{5}"))
            {
                return ValidationResult.Invalid("Invalid format");
            }

            try
            {
                var year = int.Parse(ssn.Substring(0, 4));
                var month = int.Parse(ssn.Substring(4, 2));
                var day = int.Parse(ssn.Substring(6, 2));
                DateTime date = new DateTime(year, month, day);
                if (date > DateTime.Now)
                {
                    return ValidationResult.InvalidDate();
                }
            }
            catch
            {
                return ValidationResult.InvalidDate();
            }

            int sum = 0;
            for (int i = 0; i < ssn.Length - 1; i++)
            {
                if (i % 2 != 0)
                {
                    sum += (int)char.GetNumericValue(ssn[i]);
                }
                else
                {
                    sum += ((int)char.GetNumericValue(ssn[i]) * 2).ToString().ToCharArray().Sum(c => c - '0');
                }
            }

            if (sum % 10 != 0)
            {
                return ValidationResult.InvalidChecksum();
            }

            List<int> listNumbers = new List<int>();
            for (var i = 12; i >= 0; i--)
            {
                if (i != 11)
                {
                    listNumbers.Add((int)char.GetNumericValue(ssn[i]));
                }
            }
            var check = 0;
            for (var j = 0; j < listNumbers.Count; j++)
            {
                var item = listNumbers[j];
                var p = _p[j % 8, item];
                check = _d[check, p];
            }
            return check == 0 ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// TVA (taxe sur la valeur ajoutée, Luxembourgian VAT number)
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers();
            vatId = vatId.StripPrefix("LU");

            if (!Regex.IsMatch(vatId, @"^[0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }

            var isValid = int.Parse(vatId.Substring(0, 6)) % 89 == int.Parse(vatId.Substring(6, 2));
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>Validates a postal code issued by Luxembourg.</summary>
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
