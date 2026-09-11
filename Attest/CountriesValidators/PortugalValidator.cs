using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class PortugalValidator : IdValidationAbstract
    {
        public PortugalValidator()
        {
            CountryCode = nameof(Country.PT);
        }

        readonly Dictionary<char, int> chars = new Dictionary<char, int>{
            { '0', 0},
            {'1', 1},
            {'2', 2},
            {'3', 3},
            {'4', 4},
            {'5', 5},
            {'6', 6},
            {'7', 7},
            {'8', 8},
            {'9', 9},
            {'A', 10},
            {'B', 11},
            {'C', 12},
            {'D', 13},
            {'E', 14},
            {'F', 15},
            {'G', 16},
            {'H', 17},
            {'I', 18},
            {'J', 19},
            {'K', 20},
            {'L', 21},
            {'M', 22},
            {'N', 23},
            {'O', 24},
            {'P', 25},
            {'Q', 26},
            {'R', 27},
            {'S', 28},
            {'T', 29},
            {'U', 30},
            {'V', 31},
            {'W', 32},
            {'X', 33},
            {'Y', 34},
            {'Z', 35}
        };

        /// <summary>
        /// Número de identificação civil - NIC
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            ValidationResult validation = ValidateCartaoCidadao(ssn);
            if (validation.IsValid)
            {
                return validation;
            }
            else
            {
                return ValidateBilhetedeIdentidade(ssn);
            }
        }

        // The leading digits of a NIF identify the kind of taxpayer it was issued to:
        //  1, 2, 3     natural persons (the 3 range opened in June 2019)
        //  45          natural persons, non-resident citizens
        //  8           empresario em nome individual (sole trader, range no longer issued)
        //  5           pessoa colectiva registered with the Registo Nacional de Pessoas Colectivas
        //  6           central, regional or local public administration bodies
        //  7           herancas indivisas, investment funds, non-resident collectives, official attributions
        //  9           irregular collective bodies, condominiums, non-residents without permanent establishment
        // 0, and 4 not followed by 5, are not issued at all.
        // https://pt.wikipedia.org/wiki/N%C3%BAmero_de_identifica%C3%A7%C3%A3o_fiscal
        private const string IndividualPrefixes = "^([123]|45|8)";
        private const string EntityPrefixes = "^[5679]";
        private const string AnyPrefix = "^([12356789]|45)";

        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateNif(id, EntityPrefixes, "Invalid code. This is not a company nif.");
        }

        public override ValidationResult ValidateIndividualTaxCode(string code)
        {
            return ValidateNif(code, IndividualPrefixes, "Invalid code. This is not a personal nif.");
        }

        private ValidationResult ValidateNif(string nif, string allowedPrefixes, string prefixError)
        {
            nif = nif.RemoveSpecialCharacthers();
            nif = nif.Replace("PT", string.Empty).Replace("pt", string.Empty);
            int[] multipliers = { 9, 8, 7, 6, 5, 4, 3, 2 };

            if (!Regex.IsMatch(nif, @"^\d{9}$"))
            {
                return ValidationResult.InvalidFormat("123456789");

            }
            else if (!Regex.IsMatch(nif, allowedPrefixes))
            {
                return ValidationResult.Invalid(prefixError);
            }

            var sum = nif.Sum(multipliers);

            var checkDigit = 11 - sum % 11;

            if (checkDigit > 9)
            {
                checkDigit = 0;
            }
            bool isValid = checkDigit == nif[8].ToInt();
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        public int CheckSum(string value)
        {
            var sum = 0;

            for (var i = 0; i < value.Length; i++)
            {
                sum += value[i] * (value.Length + 1 - i);
            }

            var mod = sum % 11;
            return ((mod == 0 || mod == 1) ? 0 : 11 - mod);
        }

        /// <summary>
        /// Bilhete de Identidade
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ValidationResult ValidateBilhetedeIdentidade(string value)
        {
            value = value.RemoveSpecialCharacthers();

            if (value?.Length != 9)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!value.All(char.IsDigit))
            {
                return ValidationResult.InvalidFormat("123456789");
            }


            return CheckSum(value.Substring(0, 8)) == (int)char.GetNumericValue(value[8])
                ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Cartao do Cidadao
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ValidationResult ValidateCartaoCidadao(string value)
        {
            value = value.RemoveSpecialCharacthers();
            if (value?.Length != 12)
            {
                return ValidationResult.InvalidLength();
            }


            return CalculateSum(value) == 0 ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        private int CalculateSum(string value)
        {
            var sum = 0;

            for (var i = value.Length - 1; i >= 0; i--)
            {
                int d;
                try
                {
                    d = chars[value[i]];
                }
                catch
                {
                    return -1;
                }
                if (i < 9 && d > 9)
                {
                    return -1;
                }

                if (i % 2 == 0)
                {
                    d *= 2;

                    if (d > 9)
                    {
                        d -= 9;
                    }
                }

                sum += d;
            }

            return sum % 10;
        }

        /// <summary>
        /// Numero de Identificacao Fiscal (NIF). The Portuguese VAT number is the taxpayer's
        /// own NIF, so sole traders and other natural persons registered for IVA carry a
        /// 1, 2 or 3 prefixed VAT number just as companies carry a 5 prefixed one.
        /// https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.pt.nif.html
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateNif(vatId, AnyPrefix, "Invalid code. This is not a nif.");
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^\\d{7}$"))
            {
                return ValidationResult.InvalidFormat("NNNN-NNN");
            }
            return ValidationResult.Success();
        }
    }
}
