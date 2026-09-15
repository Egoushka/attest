using System.Linq;
using System.Text.RegularExpressions;


namespace Attest.Countries
{
    /// <summary>Validates the identifiers and postal codes issued by Andorra.</summary>
    public class AndorraValidator : IdValidationAbstract
    {
        // The first letter of an NRT states what kind of holder it belongs to, so a number can be
        // read as personal or as a company number but not, except for "E", as both.
        // F = resident natural person, E = non-resident (natural person or legal entity),
        // A = societat anònima, L = societat de responsabilitat limitada, C = comunitat de béns,
        // D = public body, G = tax group, O = collective investment scheme, P = association or
        // foundation, U = parapublic entity.
        // Section II (TIN structure) of the OECD TIN sheet for Andorra:
        // https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/andorra-tin.pdf
        // and the Departament de Tributs i de Fronteres FAQ "Puc saber la tipologia d'un client
        // únicament amb el seu NRT?": https://www.govern.ad/ca/l/4191585
        const string PersonLetters = "EF";
        const string EntityLetters = "ACDEGLOPU";
        const string AnyLetters = "ACDEFGLOPU";

        /// <summary>Creates a validator for Andorra (AD).</summary>
        public AndorraValidator()
        {
            CountryCode = nameof(Country.AD);
        }

        /// <summary>
        /// NRT (Número de Registre Tributari, Andorra tax number) of a company, a public body or
        /// another entity. Rejects the "F" numbers of natural persons.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            return ValidateNrt(id, EntityLetters, "Invalid format. First letter must be ACDEGLOPU for an entity");
        }

        /// <summary>
        /// NRT (Número de Registre Tributari, Andorra tax number) of a natural person: the holder's
        /// Número d'Identificació Administrativa preceded by "F" when resident and "E" when not.
        /// Rejects the numbers of companies and other entities.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// https://www.oecd.org/content/dam/oecd/en/topics/policy-issue-focus/aeoi/andorra-tin.pdf
        public override ValidationResult ValidateIndividualTaxCode(string id)
        {
            var result = ValidateNrt(id, PersonLetters, "Invalid format. First letter must be E or F for a natural person");
            if (!result.IsValid)
            {
                return result;
            }

            // "E" is shared: a non-resident natural person is numbered from 800000 up, while a
            // non-resident legal entity may hold any number, so below that floor an E number is
            // an entity rather than a person.
            id = Compact(id);
            if (id[0] == 'E' && int.Parse(id.Substring(1, 6)) < 800000)
            {
                return ValidationResult.Invalid("Invalid format.The number code cannot be lower than 800000");
            }

            return result;
        }

        /// <summary>
        /// NRT (Número de Registre Tributari, Andorra tax number) as quoted for IGI purposes. Any
        /// holder type can be registered for IGI — a natural person carrying out an economic
        /// activity declares under their own "F" number (the declaració censal H5001A offers
        /// "Persona física" as an obligat tributari) — so this accepts every NRT letter and is the
        /// one method that does not tell a person from a company.
        /// </summary>
        /// <param name="vatId"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            return ValidateNrt(vatId, AnyLetters, "Invalid format. First letter must be ACDEFGLOPU");
        }

        /// <summary>Validates a postal code issued by Andorra.</summary>
        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[Aa][Dd][0-9]{3}$"))
            {
                return ValidationResult.InvalidFormat("CCNNN");
            }
            return ValidationResult.Success();
        }

        private static string Compact(string id)
        {
            return id.RemoveSpecialCharacthers().StripPrefix("AD");
        }

        private static ValidationResult ValidateNrt(string id, string letters, string letterError)
        {
            id = Compact(id);

            if (id.Length != 8)
            {
                return ValidationResult.InvalidLength();
            }

            if (!char.IsLetter(id[0]) || !char.IsLetter(id[id.Length - 1]))
            {
                return ValidationResult.Invalid("Invalid format. First and last character must be letters");
            }
            else if (!id.Substring(1, 6).IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("F-123456-Z");
            }
            else if (letters.IndexOf(id[0]) < 0)
            {
                return ValidationResult.Invalid(letterError);
            }
            else if (id[0] == 'F' && int.Parse(id.Substring(1, 6)) > 699999)
            {
                return ValidationResult.Invalid("Invalid format.The number code cannot be higher than 699999");
            }
            if ((id[0] == 'A' || id[0] == 'L') && !(699999 < int.Parse(id.Substring(1, 6)) && int.Parse(id.Substring(1, 6)) < 800000))
            {
                return ValidationResult.Invalid("Invalid format.The number code must be between 699999 and 800000");
            }

            return ValidationResult.Success();
        }
    }
}
