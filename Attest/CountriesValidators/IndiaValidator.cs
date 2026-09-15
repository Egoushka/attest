using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Attest.Countries
{
    public class IndiaValidator : IdValidationAbstract
    {

        public IndiaValidator()
        {
            CountryCode = nameof(Country.IN);
        }

        readonly int[,] d = new int[,]
        {
            {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
            {1, 2, 3, 4, 0, 6, 7, 8, 9, 5},
            {2, 3, 4, 0, 1, 7, 8, 9, 5, 6},
            {3, 4, 0, 1, 2, 8, 9, 5, 6, 7},
            {4, 0, 1, 2, 3, 9, 5, 6, 7, 8},
            {5, 9, 8, 7, 6, 0, 4, 3, 2, 1},
            {6, 5, 9, 8, 7, 1, 0, 4, 3, 2},
            {7, 6, 5, 9, 8, 2, 1, 0, 4, 3},
            {8, 7, 6, 5, 9, 3, 2, 1, 0, 4},
            {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
        };

        readonly int[,] p = new int[,]
       {
            {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
            {1, 5, 7, 6, 2, 8, 3, 0, 9, 4},
            {5, 8, 0, 3, 7, 9, 6, 1, 4, 2},
            {8, 9, 1, 6, 0, 4, 3, 5, 2, 7},
            {9, 4, 5, 3, 1, 2, 6, 8, 7, 0},
            {4, 2, 8, 6, 5, 7, 3, 9, 0, 1},
            {2, 7, 9, 3, 8, 0, 6, 4, 1, 5},
            {7, 0, 4, 6, 9, 1, 3, 2, 5, 8}
       };
        readonly int[] inv = { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

        /// <summary>
        /// Aadhaar
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public override ValidationResult ValidateNationalIdentity(string num)
        {
            num = num.RemoveSpecialCharacthers();
            // Aadhaar is 12 digits, never starts with 0 or 1, and ends with a Verhoeff check digit.
            // https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.in_.aadhaar.html
            if (!Regex.IsMatch(num, @"^[2-9][0-9]{11}$"))
            {
                return ValidationResult.InvalidFormat("234123412346");
            }
            int c = 0;
            int[] myArray = StringToReversedIntArray(num);

            for (int i = 0; i < myArray.Length; i++)
            {
                c = d[c, p[(i % 8), myArray[i]]];
            }

            bool isValid = c == 0;
            return isValid ? ValidationResult.Success() : ValidationResult.InvalidChecksum();
        }

        private int[] StringToReversedIntArray(string num)
        {
            int[] myArray = new int[num.Length];

            for (int i = 0; i < num.Length; i++)
            {
                myArray[i] = int.Parse(num.Substring(i, 1));
            }

            Array.Reverse(myArray);

            return myArray;

        }

        /// <summary>
        /// PAN (Permanent Account Number, Indian income tax identifier)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ValidationResult ValidateEntity(string id)
        {
            id = id.RemoveSpecialCharacthers();
            if (id.Length != 10)
            {
                return ValidationResult.InvalidLength();
            }
            else if (!Regex.IsMatch(id, "^[A-Z]{5}[0-9]{4}[A-Z]$"))
            {
                return ValidationResult.InvalidFormat("AAAAA1234A");
            }
            else if (!HasValidCardType(id))
            {
                return ValidationResult.Invalid("Invalid card type");
            }
            // The four digit serial runs from 0001: AAAAA0000A is the placeholder the documentation
            // is written with and not a number anyone holds.
            // https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/in_/pan.py
            else if (id.Substring(5, 4) == "0000")
            {
                return ValidationResult.Invalid("Invalid serial number");
            }
            return ValidationResult.Success();
        }


        private bool HasValidCardType(string number)
        {

            string[] card_holder_types = new string[]{
                "A",// 'Association of Persons (AOP)'
                "B",//Body of Individuals (BOI)'
                "C",//Company'
                "F",//Firm'
                "G",//Government'
                "H",//HUF (Hindu Undivided Family)'
                "L",//Local Authority'
                "J",//Artificial Juridical Person'
                "P",//Individual
                "T",//Trust (AOP)
                "K",//Krish (Trust Krish)
            };


            return card_holder_types.Contains(number.Substring(3, 1));
        }


        /// <summary>
        /// GSTIN (Goods and Services Tax identification number), and the state VAT / CST TIN it
        /// replaced.
        /// </summary>
        /// <remarks>
        /// GST replaced the state VAT and CST regimes on 1 July 2017, so a number issued to an
        /// Indian business since then is a GSTIN and the eleven digit TIN is historic. Both are
        /// accepted: the TIN is what a record predating the change holds, and rejecting it would
        /// be a false negative on data that was correct when it was stored.
        ///
        /// A GSTIN is a two digit state code, the holder's ten character PAN, a registration
        /// number for that PAN within the state, the letter Z, and a check character over the
        /// first fourteen computed with the Luhn algorithm in base 36.
        /// https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/in_/gstin.py
        /// </remarks>
        /// <param name="vatId">A GSTIN, or a pre-GST VAT/CST TIN.</param>
        /// <returns>Whether the number is well formed and its check character is right.</returns>
        public override ValidationResult ValidateVAT(string vatId)
        {
            vatId = vatId.RemoveSpecialCharacthers().ToUpperInvariant();

            if (vatId.Length == GstinLength)
            {
                return ValidateGstin(vatId);
            }

            // [0-9] and not \d: in .NET \d also matches non-ASCII Unicode digits.
            if (!Regex.IsMatch(vatId, "^[0-9]{11}[CV]$"))
            {
                return ValidationResult.InvalidFormat("27AAPFU0939F1ZV or 12345678901C");
            }
            return ValidationResult.Success();
        }

        private const int GstinLength = 15;

        /// <summary>The digits and letters a GSTIN check character is computed over, in value order.</summary>
        private const string Base36Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private ValidationResult ValidateGstin(string gstin)
        {
            if (!Regex.IsMatch(gstin, "^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][0-9A-Z]{3}$"))
            {
                return ValidationResult.InvalidFormat("27AAPFU0939F1ZV");
            }

            // State codes run 01 to 38 -- 38 is Ladakh, split from Jammu and Kashmir in 2019 --
            // plus 97 for Other Territory and 99 for Centre Jurisdiction, which is where an OIDAR
            // registration sits. Encoded as a range rather than as python-stdnum's table, whose
            // list stops at 37 and so rejects every Ladakh GSTIN.
            int state = int.Parse(gstin.Substring(0, 2));
            if ((state < 1 || state > 38) && state != 97 && state != 99)
            {
                return ValidationResult.Invalid("Invalid state code");
            }

            // The 13th character counts the registrations held against one PAN in one state, so it
            // starts at 1; the 14th is Z for every registration python-stdnum accepts.
            if (gstin[12] == '0' || gstin[13] != 'Z')
            {
                return ValidationResult.Invalid("Invalid registration number");
            }

            // The PAN is embedded whole, holder type and all, so the rule already written for it
            // answers here rather than being restated.
            ValidationResult pan = ValidateEntity(gstin.Substring(2, 10));
            if (!pan.IsValid)
            {
                return pan;
            }

            return gstin[14] == Base36CheckCharacter(gstin.Substring(0, 14))
                ? ValidationResult.Success()
                : ValidationResult.InvalidChecksum();
        }

        /// <summary>
        /// The Luhn check character over <paramref name="value"/> in base 36: every character
        /// counts as its position in the alphabet, weights alternate 1 and 2 from the left, and
        /// each product is folded by adding its quotient and remainder over 36.
        /// </summary>
        private static char Base36CheckCharacter(string value)
        {
            int sum = 0;

            for (int i = 0; i < value.Length; i++)
            {
                int product = Base36Alphabet.IndexOf(value[i]) * ((i % 2 == 0) ? 1 : 2);
                sum += (product / 36) + (product % 36);
            }

            return Base36Alphabet[(36 - (sum % 36)) % 36];
        }


        /// <summary>
        /// PAN (Permanent Account Number, Indian income tax identifier)
        /// </summary>
        /// <param name="pan"></param>
        /// <returns></returns>
        public override ValidationResult ValidateIndividualTaxCode(string pan)
        {
            return ValidateEntity(pan);
        }

        public override ValidationResult ValidatePostalCode(string postalCode)
        {
            postalCode = postalCode.RemoveSpecialCharacthers();
            if (!Regex.IsMatch(postalCode, "^[0-9]{6}$"))
            {
                return ValidationResult.InvalidFormat("NNNNNN or NNN NNN");
            }
            return ValidationResult.Success();
        }
    }
}
