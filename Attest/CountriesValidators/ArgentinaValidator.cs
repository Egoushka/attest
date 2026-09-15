
using System;
using System.Linq;
using System.Text.RegularExpressions;


namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Argentina.</summary>
    public class ArgentinaValidator : IdValidationAbstract
    {
        /// <summary>
        /// Valid CUIT taxpayer types: individuals, companies and international purposes.
        /// https://arthurdejong.org/nl/python-stdnum/doc/1.20/stdnum.ar.cuit
        /// </summary>
        private static readonly string[] _cuitTypes = new string[]
        {
            "20", "23", "24", "27", "30", "33", "34", "50", "51", "55"
        };

        /// <summary>Creates a validator for Argentina (AR).</summary>
        public ArgentinaValidator()
        {
            CountryCode = nameof(Country.AR);
        }

        /// <summary>
        /// Validate CUIT Number - Código Único de Identificación Tributaria
        /// </summary>
        /// <param name="cuit"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string cuit)
        {
            return ValidateCuit(cuit);
        }


        /// <summary>
        /// Validate CUIT
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateCuit(id);

        }

        /// <summary>
        /// Validate VAT/IVA
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateCuit(vatId);
        }



        private ValidationResult ValidateCuit(string cuit)
        {

            cuit = cuit.RemoveSpecialCharacthers();

            // [0-9] and not \d: in .NET \d also matches non-ASCII Unicode digits,
            // which int.Parse below rejects with a FormatException.
            if (!Regex.IsMatch(cuit, "^[0-9]{11}$"))
            {
                return ValidationResult.InvalidFormat("12345678901");
            }
            else if (!_cuitTypes.Contains(cuit.Substring(0, 2)))
            {
                return ValidationResult.Invalid("Invalid taxpayer type.");
            }
            else
            {
                int calculado = CalculateDigitCuit(cuit);
                int digito = int.Parse(cuit.Substring(10));
                bool isValid = calculado == digito;
                return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
            }
        }



        /// <summary>
        /// Validate DNI number (SSN for Argentina)
        /// </summary>
        /// <param name="dni"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string dni)
        {
            dni = dni.RemoveSpecialCharacthers();
            // 7 or 8 digits, no check digit. Numbers below 10.000.000 are older but still valid.
            // https://arthurdejong.org/nl/python-stdnum/doc/1.20/stdnum.ar.dni
            if (!Regex.IsMatch(dni, "^[0-9]{7,8}$"))
            {
                return ValidationResult.InvalidFormat("12345678");
            }
            return ValidationResult.Success();
        }

        private int CalculateDigitCuit(string cuit)
        {
            int[] mult = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            char[] nums = cuit.ToCharArray();
            int total = 0;
            for (int i = 0; i < mult.Length; i++)
            {
                total += int.Parse(nums[i].ToString()) * mult[i];
            }
            var resto = total % 11;
            return resto == 0 ? 0 : resto == 1 ? 9 : 11 - resto;
        }


        /// <summary>
        /// Validate Argentina CBU (Bank Account)
        /// </summary>
        /// <param name="cbu"></param>
        /// <returns></returns>
        public ValidationResult ValidateCBU(string cbu)
        {
            cbu = cbu.RemoveSpecialCharacthers();

            if (!Regex.IsMatch(cbu, "^[0-9]{22}$"))
            {
                return ValidationResult.InvalidFormat("1234567890123456789012");
            }

            string getChecksumDigit(string value)
            {
                int[] ponderador = new int[] { 3, 1, 7, 9 };
                var sum = 0;
                int j = 0;
                for (int i = value.Length - 1; i >= 0; --i)
                {
                    sum += (int.Parse(value[i].ToString()) * ponderador[j % 4]);
                    ++j;
                }

                return ((10 - sum % 10) % 10).ToString();
            }


            if (cbu[7].ToString() != getChecksumDigit(cbu.Substring(0, 7)))
            {
                return ValidationResult.InvalidChecksum();
            }
            if (cbu[21].ToString() != getChecksumDigit(cbu.Substring(8, 13)))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        /// <summary>Validates a postal code issued by Argentina.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^([0-9]{4}|[A-Za-z][0-9]{4}[A-Za-z]{3})$"))
            {
                return ValidationResult.InvalidFormat("NNNN OR ANNNNAAA");
            }
            return ValidationResult.Success();
        }
    }
}
