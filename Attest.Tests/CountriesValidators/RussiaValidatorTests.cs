using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class RussiaValidatorTests
    {
        private readonly RussiaValidator _russiaValidator;

        public RussiaValidatorTests()
        {
            _russiaValidator = new RussiaValidator();
        }

        // СНИЛС check number: the first nine digits are weighted 9..1, the sum modulo 101 is the
        // check number and a remainder of 100 or 101 is written as 00.
        // https://insur-portal.ru/pension/kontrolnoe-chislo-snils
        [Theory]
        [InlineData("11223344595", true)]       // 112-233-445 95, the example from the Pension Fund
        [InlineData("112-233-445 95", true)]    // Same number as printed on the card
        [InlineData("15657325992", true)]       // Weighted sum 92, below 100
        [InlineData("08765430300", true)]       // Weighted sum 202, 202 % 101 = 0, written as 00
        [InlineData("11223344594", false)]      // Wrong check number
        [InlineData("08765430301", false)]      // Wrong check number
        [InlineData("1122334459", false)]       // Ten digits
        [InlineData("112233445956", false)]     // Twelve digits
        [InlineData("1122334459x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidateNationalIdentity(code).IsValid);
        }

        // ИНН check digits, see https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ru/inn.py
        [Theory]
        [InlineData("7707083893", true)]        // Sberbank
        [InlineData("7736207543", true)]        // Gazprom
        [InlineData("1234567894", true)]        // python-stdnum example
        [InlineData("7707 083 893", true)]      // Same number with separators
        [InlineData("123456789047", true)]      // python-stdnum example of the 12 digit personal ИНН
        [InlineData("500100732259", true)]      // Personal ИНН, both check digits computed
        [InlineData("1234567895", false)]       // Wrong check digit
        [InlineData("123456789037", false)]     // Wrong check digits on the personal form
        [InlineData("123456789040", false)]     // Second check digit wrong
        [InlineData("770708389", false)]        // Nine digits
        [InlineData("77070838931", false)]      // Eleven digits
        [InlineData("770708389x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        [InlineData("7707083893", true)]        // Sberbank
        [InlineData("7736207543", true)]        // Gazprom
        [InlineData("7707 083 893", true)]      // Same number with separators
        [InlineData("1234567895", false)]       // Wrong check digit
        [InlineData("123456789047", false)]     // The 12 digit form belongs to a person, not a company
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        [InlineData("7707083893", true)]
        [InlineData("RU7707083893", true)]
        [InlineData("RU 7736207543", true)]
        [InlineData("RU123456789047", true)]    // Personal ИНН of a self employed VAT payer
        [InlineData("RU1234567895", false)]     // Wrong check digit
        [InlineData("RU770708389", false)]      // Nine digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidateVAT(code).IsValid);
        }

        // БИК: the last three digits are the conditional number of the credit organisation and run
        // from 050 to 999, while 000 to 002 are reserved for divisions of the Bank of Russia.
        // https://assistentus.ru/vedenie-biznesa/chto-takoe-bik-banka/
        [Theory]
        [InlineData("044525225", true)]         // Sberbank
        [InlineData("044525593", true)]         // Alfa-Bank
        [InlineData("044525000", true)]         // Main directorate of the Bank of Russia, Moscow
        [InlineData("044 525 225", true)]       // Same number with separators
        [InlineData("044525049", false)]        // 049 falls in the reserved gap 003..049
        [InlineData("044525003", false)]        // First value of the reserved gap
        [InlineData("04452522", false)]         // Eight digits
        [InlineData("0445252251", false)]       // Ten digits
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestBIK(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidateBIK(code).IsValid);
        }

        // ОГРН: the 13th digit is the last digit of the first twelve digits modulo 11.
        // https://glavkniga.ru/situations/k505650
        [Theory]
        [InlineData("1037739010891", true)]
        [InlineData("1027700132195", true)]     // Sberbank
        [InlineData("1147746683479", true)]
        [InlineData("1027700132195 ", true)]    // Trailing separator
        [InlineData("1037739010890", false)]    // Wrong check digit
        [InlineData("1027700132191", false)]    // Wrong check digit
        [InlineData("103773901089", false)]     // Twelve digits
        [InlineData("10377390108912", false)]   // Fourteen digits
        [InlineData("103773901089x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestOGRN(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidateOGRN(code).IsValid);
        }

        // ОГРНИП: the 15th digit is the last digit of the first fourteen digits modulo 13.
        // https://glavkniga.ru/situations/k505650
        [Theory]
        [InlineData("304500116000157", true)]
        [InlineData("315774600002237", true)]   // Check digit computed from the rule above
        [InlineData("304500116000150", false)]  // Wrong check digit
        [InlineData("315774600002230", false)]  // Wrong check digit
        [InlineData("30450011600015", false)]   // Fourteen digits
        [InlineData("3045001160001570", false)] // Sixteen digits
        [InlineData("30450011600015x", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestOGRNIP(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidateOGRNIP(code).IsValid);
        }

        [Theory]
        [InlineData("101000", true)]            // Moscow
        [InlineData("620014", true)]            // Yekaterinburg
        [InlineData("101 000", true)]
        [InlineData("10100", false)]            // Five digits
        [InlineData("1010000", false)]          // Seven digits
        [InlineData("A01000", false)]
        [InlineData("abc", false)]
        [InlineData("   ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _russiaValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
