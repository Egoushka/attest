using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class BrazilValidator : IdValidationAbstract
    {
        public BrazilValidator()
        {
            CountryCode = nameof(Country.BR);
        }
        private int DigitChecksum(string numbers)
        {
            int index = 2;

            char[] charArray = numbers.ToCharArray();
            Array.Reverse(charArray);
            numbers = new string(charArray);

            int sum = 0;

            for (int i = 0; i < numbers.Length; i++)
            {
                sum += int.Parse(numbers[i].ToString()) * index;

                index = index == 9 ? 2 : index + 1;
            }

            var mod = sum % 11;

            return mod < 2 ? 0 : 11 - mod;
        }

        /// <summary>
        /// Validate Brazil Cadastro Nacional da Pessoa Juridica (CNPJ)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            // [0-9] and not \d: in .NET \d also matches non-ASCII Unicode digits,
            // which int.Parse in DigitChecksum rejects with a FormatException.
            // A CNPJ whose first twelve digits are zero has valid check digits but is not issued.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/br/cnpj.py
            if (!Regex.IsMatch(id, @"^[0-9]{14}$") || id.StartsWith("000000000000"))
            {
                return ValidationResult.InvalidFormat("12345678901234");
            }

            var registration = id.Substring(0, 12);
            registration += DigitChecksum(registration);
            registration += DigitChecksum(registration);


            bool isValid = registration.Substring(registration.Length - 2) == id.Substring(registration.Length - 2);
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// Validate Brazil CPF
        /// </summary>
        /// <param name="cpf"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string cpf)
        {
            cpf = cpf.RemoveSpecialCharacthers();
            var regex = @"^[0-9]{11}$";

            // A CPF of all zeros satisfies both check digits; python-stdnum rejects it explicitly.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/br/cpf.py
            if (!Regex.IsMatch(cpf, regex) || cpf == "00000000000")
            {
                return ValidationResult.InvalidFormat("12345678901");

            }

            var strCPF = new string(cpf.Where(c => char.IsDigit(c)).ToArray());
            int sum = 0;

            for (var i = 1; i <= 9; i++)
            {
                sum = sum + int.Parse(strCPF.Substring(i - 1, 1)) * (11 - i);
            }
            int rest = sum * 10 % 11;

            if ((rest == 10) || (rest == 11))
                rest = 0;

            if (rest != int.Parse(strCPF.Substring(9, 1)))
            {
                return ValidationResult.InvalidChecksum();
            }

            sum = 0;
            for (int i = 1; i <= 10; i++)
                sum = sum + int.Parse(strCPF.Substring(i - 1, 1)) * (12 - i);

            rest = (sum * 10) % 11;

            if ((rest == 10) || (rest == 11))
                rest = 0;
            if (rest != int.Parse(strCPF.Substring(10, 1)))
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();

        }

        /// <summary>
        /// Validate Brazil Cadastro Nacional da Pessoa Juridica (CNPJ)
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
            if (!Regex.IsMatch(postalCode, "^[0-9]{8}$"))
            {
                return ValidationResult.InvalidFormat("NNNNN-NNN");
            }
            return ValidationResult.Success();
        }
    }
}
