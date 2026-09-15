using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class BoliviaValidator : IdValidationAbstract
    {
        public BoliviaValidator()
        {
            CountryCode = nameof(Country.BO);
        }

        /// <summary>
        /// CI Number
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string ssn)
        {
            // The complemento is alphanumeric (Reglamento del Registro Unico de Identificacion
            // Personal, articulo 40, aprobado por Resolucion Administrativa SEGIP/DGE 632/2017),
            // so \w - which also matches "_" and non ASCII word characters - is too wide.
            //
            // Articulo 40 says "caracteres alfanumericos", plural and uncounted, and SEGIP issues
            // two character complements: 1A, 1B. Capping it at one rejected those, and a rejected
            // cedula is someone refused service. The cost of accepting two is that a ten digit
            // string now passes, because the hyphen is stripped before the check and a complemento
            // may itself be numeric, so nothing distinguishes 8 digits plus "12" from 10 digits.
            // Accepting a value nobody submits is the cheaper error of the two.
            ssn = ssn.RemoveSpecialCharacthers();
            if (string.IsNullOrWhiteSpace(ssn) || !Regex.IsMatch(ssn, "^[0-9]{5,8}[A-Za-z0-9]{0,2}$"))
            {
                return ValidationResult.InvalidFormat("1234567 or 1234567-1A");
            }
            return ValidationResult.Success();

        }

        /// <summary>
        /// Número de Identificación Tributaria
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            // Seven to thirteen digits. The SIN builds a natural person's NIT from the Cedula de
            // Identidad followed by a taxpayer type code (01, 02 or 04) and a random digit 1-9, so
            // the length tracks the CI rather than being fixed:
            // https://siatinfo.impuestos.gob.bo/index.php/requisitos-para-la-inscripcion/conceptos-generales/generacion-del-nit
            // No published source describes a NIT below seven or above thirteen digits, and no
            // check digit is published, so the length is all that can be checked.
            if (!Regex.IsMatch(id, @"^[0-9]{7,13}$"))
            {
                return ValidationResult.InvalidFormat("1234567890");
            }

            return ValidationResult.Success();
        }

        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            return ValidateEntity(id);
        }

        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateEntity(vatId);
        }

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
