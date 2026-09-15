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
                // Alphanumeric CNPJ (issued from July 2026): every character contributes its
                // ASCII code minus 48, so '0'-'9' keep their value and 'A'-'Z' become 17-42.
                // https://www.gov.br/receitafederal/pt-br/centrais-de-conteudo/publicacoes/documentos-tecnicos/cnpj/manual-dv-cnpj.pdf
                sum += (numbers[i] - '0') * index;

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
            // Both reference implementations upper-case before mapping characters to values.
            id = id.RemoveSpecialCharacthers().ToUpperInvariant();
            // From July 2026 the twelve positions before the check digits may hold A-Z as well
            // as 0-9; the two check digits stay numeric and existing numeric CNPJs are unchanged.
            // [0-9] and not \d: in .NET \d also matches non-ASCII Unicode digits, which are
            // not CNPJ characters but would pass a \d guard and then be scored as garbage.
            // A CNPJ whose first twelve characters are zero has valid check digits but is not issued.
            // https://www.gov.br/receitafederal/pt-br/centrais-de-conteudo/publicacoes/documentos-tecnicos/cnpj/manual-dv-cnpj.pdf
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/br/cnpj.py
            if (!Regex.IsMatch(id, @"^[0-9A-Z]{12}[0-9]{2}$") || id.StartsWith("000000000000", StringComparison.Ordinal))
            {
                // The hint has to show the alphanumeric shape: a digits-only example told a caller
                // holding 12.ABC.345/01DE-35 that its letters were the defect. This is Receita's
                // own published example, and the row asserting it valid is in the test file.
                return ValidationResult.InvalidFormat("12ABC34501DE35");
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

            // All ten repeated-digit CPFs ("00000000000".."99999999999") satisfy both check
            // digits arithmetically, but none was ever issued; Brazilian implementations treat
            // them as reserved numbers and reject them. python-stdnum rejects only the all-zero
            // case (int(number) <= 0), which is why the checksum alone is not enough here.
            // https://github.com/brazilian-utils/brazilian-utils/blob/main/src/is-valid-cpf/constants.ts
            // https://github.com/alvarofpp/validate-docbr/blob/master/validate_docbr/CPF.py
            if (!Regex.IsMatch(cpf, regex) || cpf.All(c => c == cpf[0]))
            {
                return ValidationResult.InvalidFormat("12345678901");

            }

            var strCPF = new string(cpf.Where(c => c.IsAsciiDigit()).ToArray());
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
