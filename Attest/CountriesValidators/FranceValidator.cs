using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class FranceValidator : IdValidationAbstract
    {
        readonly string _alphabet = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        public FranceValidator()
        {
            CountryCode = nameof(Country.FR);
        }


        /// <summary>
        /// SIREN (a French company identification number)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string number)
        {
            number = number.RemoveSpecialCharacthers().ToUpperInvariant().StripPrefix("FR");
            if (!number.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("123456789");
            }
            else if (number.Length != 9)
            {
                return ValidationResult.InvalidLength();
            }
            return number.CheckLuhnDigit() ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// NIF (Numéro d'Immatriculation Fiscale, French tax identification number).
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string number)
        {
            number = number.RemoveSpecialCharacthers().ToUpperInvariant().StripPrefix("FR");
            if (!number.IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("1234567890123");
            }
            else if (number.Length != 13)
            {
                return ValidationResult.InvalidLength();
            }
            return ValidationResult.Success();
        }

        /// <summary>
        /// NIR (French personal identification number).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string value)
        {
            value = value.RemoveSpecialCharacthers();
            if (value.Length != 15)
            {
                return ValidationResult.Invalid("Invalid length");
            }
            var pattern = @"^([1278])([0-9]{2})(0[1-9]|1[0-2]|20)([0-9]{2}|2[AB])([0-9]{3})([0-9]{3})([0-9]{2})$";
            var match = Regex.Match(value, pattern);
            if (!match.Success)
            {
                return ValidationResult.Invalid("Invalid format");
            }

            string gender = match.Groups[1].Value;
            string year = match.Groups[2].Value;
            string month = match.Groups[3].Value;
            string department = match.Groups[4].Value;
            string city = match.Groups[5].Value;
            int cityNumber = int.Parse(city);

            var certificate = match.Groups[6].Value;
            var key = match.Groups[7].Value;
            int keyNumber = int.Parse(key);

            if (certificate == "000" || int.Parse(key) * 1 > 97)
            {
                return ValidationResult.Invalid("Invalid certificate");
            }

            if (department == "2A")
            {
                department = "19";
            }
            else if (department == "2B")
            {
                department = "18";
            }
            else if (department == "97")
            {
                department += city[0];
                if (int.Parse(department) < 970 || int.Parse(department) >= 989)
                {
                    return ValidationResult.Invalid("Invalid department");
                }

                city = city.Substring(1);
                cityNumber = int.Parse(city);

                if (cityNumber < 1 || cityNumber > 90)
                {
                    return ValidationResult.Invalid("Invalid city");
                }
            }
            else if (cityNumber < 1 || cityNumber > 990)
            {
                return ValidationResult.Invalid("Invalid city");
            }

            string insee = $"{gender}{year}{month}{department.Replace("A", "0").Replace("B", "0")}{city}{certificate}";
            long inseeNumber = long.Parse(insee);

            if ((97 - (inseeNumber) % 97) != keyNumber)
            {
                return ValidationResult.InvalidChecksum();
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Taxe sur la Valeur Ajoutee (TVA)  
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public override ValidationResult ValidateVAT(string number)
        {
            number = number.RemoveSpecialCharacthers().ToUpperInvariant().StripPrefix("FR");
            if (number.Length != 11)
            {
                return ValidationResult.InvalidLength();
            }
            else if (_alphabet.IndexOf(number[0]) == -1 || _alphabet.IndexOf(number[1]) == -1)
            {
                return ValidationResult.Invalid("Invalid format");
            }
            else if (!number.Substring(2).IsAsciiDigits())
            {
                return ValidationResult.InvalidFormat("A1234567890");
            }

            if (number.Substring(2, 3) != "000")
            {
                // Numbers from Monaco start with "000" and are a valid TVA but not a valid SIREN.
                var siren = ValidateEntity(number.Substring(2));
                if (!siren.IsValid)
                {
                    return siren;
                }
            }

            // Key algorithm: https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/fr/tva.py
            if (number.IsAsciiDigits())
            {
                if (int.Parse(number.Substring(0, 2)) != (long.Parse(number.Substring(2) + "12") % 97))
                {
                    return ValidationResult.InvalidChecksum();
                }
            }
            else
            {
                int check = 0;
                if (number[0].IsAsciiDigit())
                {
                    check =
                        (_alphabet.IndexOf(number[0]) * 24) +
                        _alphabet.IndexOf(number[1]) - 10;
                }
                else
                {
                    check = (
                        _alphabet.IndexOf(number[0]) * 34 +
                        _alphabet.IndexOf(number[1]) - 100);
                }
                if ((long.Parse(number.Substring(2)) + 1 + check / 11) % 11 != (check % 11))
                {
                    return ValidationResult.InvalidChecksum();
                }
            }
            return ValidationResult.Success();
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
