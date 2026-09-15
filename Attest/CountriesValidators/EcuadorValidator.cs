using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Ecuador.</summary>
    public class EcuadorValidator : IdValidationAbstract
    {
        /// <summary>Creates a validator for Ecuador (EC).</summary>
        public EcuadorValidator()
        {
            CountryCode = nameof(Country.EC);
        }


        private int Checksum(string number, int[] weights)
        {
            int sum = 0;
            for (int i = 0; i < number.Length; i++)
            {
                sum = sum + weights[i] * (int)char.GetNumericValue(number[i]);
            }

            return sum % 11;
        }

        /// <summary>
        /// Registro Unico de Contribuyentes (RUC)  
        /// </summary>
        /// <remarks>
        /// The third digit picks the taxpayer class, but two classes overlap: a third digit of 6
        /// falls back to the natural RUC when the public check sum fails, and a third digit of 9
        /// is tried as a public RUC before the juridical check sum is applied.
        /// https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.ec.ruc.html
        /// </remarks>
        /// <param name="ruc"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string ruc)
        {
            ruc = ruc.RemoveSpecialCharacthers();

            if (ruc.Length != 13)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!ruc.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("1234567890123");
            }
            else if ((int.Parse(ruc.Substring(0, 2)) < 1 || int.Parse(ruc.Substring(0, 2)) > 24) && !new string[] { "30", "50" }.Contains(ruc.Substring(0, 2)))
            {
                return ValidationResult.Invalid("Invalid province code");
            }
            else if (int.Parse(ruc.Substring(2, 1)) < 6) // 0..5 = natural RUC: CI plus establishment number
            {
                return ValidateNatural(ruc);
            }
            else if (ruc[2] == '6')   // 6 = public RUC, or a natural RUC when the public check sum fails
            {
                ValidationResult result = ValidatePublic(ruc);
                return result.IsValid ? result : ValidateNatural(ruc);
            }
            else if (ruc[2] == '9') // 9 = juridical RUC, but the public check sum is tried first
            {
                ValidationResult result = ValidatePublic(ruc);
                return result.IsValid ? result : ValidateJuridical(ruc);
            }
            else
            {
                return ValidationResult.Invalid("Third digit is wrong");
            }
        }

        /// <summary>
        /// Natural RUC: a CI followed by a three digit establishment number.
        /// </summary>
        private ValidationResult ValidateNatural(string ruc)
        {
            if (ruc.Substring(ruc.Length - 3) == "000")
            {
                return ValidationResult.Invalid("Invalid code");
            }
            return ValidateCI(ruc.Substring(0, 10));
        }

        /// <summary>
        /// Public RUC: nine digits checked against 3,2,7,6,5,4,3,2,1 plus a four digit
        /// establishment number.
        /// </summary>
        private ValidationResult ValidatePublic(string ruc)
        {
            if (ruc.Substring(ruc.Length - 4) == "0000")
            {
                return ValidationResult.Invalid("Invalid code");
            }
            else if (Checksum(ruc.Substring(0, 9), new int[] { 3, 2, 7, 6, 5, 4, 3, 2, 1 }) != 0)
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Juridical RUC: ten digits checked against 4,3,2,7,6,5,4,3,2,1 plus a three digit
        /// establishment number.
        /// </summary>
        private ValidationResult ValidateJuridical(string ruc)
        {
            if (ruc.Substring(ruc.Length - 3) == "000")
            {
                return ValidationResult.Invalid("Establishment Number Wrong");
            }
            if (Checksum(ruc.Substring(0, 10), new int[] { 4, 3, 2, 7, 6, 5, 4, 3, 2, 1 }) != 0)
            {
                return ValidationResult.InvalidChecksum();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// Registro Unico de Contribuyentes (RUC) 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            return ValidateEntity(id);
        }

        /// <summary>
        /// Registro Unico de Contribuyentes (RUC) 
        /// </summary>
        /// <param name="ruc"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string ruc)
        {
            return ValidateEntity(ruc);
        }


        /// <summary>
        /// The weighted sum an Ecuadorian identifier's check digit is derived from, or -1 for a
        /// null value.
        /// </summary>
        public int Checksum(string number)
        {
            if (number == null)
            {
                return -1;
            }

            int sum = 0;
            for (int i = 0; i < number.Length; i++)
            {
                int temp = (i % 2 == 0 ? 2 : 1) * (int)char.GetNumericValue(number[i]);
                if (temp > 9)
                {
                    temp -= 9;
                }

                sum = sum + temp;
            }
            return sum % 10;
        }

        /// <summary>
        /// CI (Cédula de identidad, Ecuadorian personal identity code)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public ValidationResult ValidateCI(string number)
        {
            number = number.RemoveSpecialCharacthers();
            if (number.Length != 10)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!number.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("1234567890");
            }
            else if ((int.Parse(number.Substring(0, 2)) < 1 || int.Parse(number.Substring(0, 2)) > 24) && !new string[] { "30", "50" }.Contains(number.Substring(0, 2)))
            {
                return ValidationResult.Invalid("Invalid province code");
            }
            // The tipo de cedula digit runs 0 to 6: 6 is what lets a RUC whose public check sum
            // fails fall back to the natural RUC above.
            // https://arthurdejong.org/nm/python-stdnum/doc/1.20/stdnum.ec.ci.html
            else if (Char.GetNumericValue(number[2]) > 6)
            {
                return ValidationResult.Invalid("Third digit is wrong");
            }
            else if (Checksum(number) != 0)
            {

                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        /// <summary>Validates a postal code issued by Ecuador.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{6}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNN");
            }
            return ValidationResult.Success();
        }
    }
}
