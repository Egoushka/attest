using Attest.Countries;
using Xunit;

namespace Attest.Tests
{
    public class BrazilValidatorTests
    {
        private readonly BrazilValidator _brazilValidator;

        public BrazilValidatorTests()
        {
            _brazilValidator = new BrazilValidator();
        }

        [Theory]
        // Falls through to the CPF check. https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/br/cpf.py
        [InlineData("39053344705", true)]
        [InlineData("390.533.447-05", true)]
        [InlineData("23100299900", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestNationalId(string code, bool isValid)
        {
            Assert.Equal(isValid, _brazilValidator.ValidateNationalIdentity(code).IsValid);
        }

        [Theory]
        // python-stdnum doctest for the CPF.
        [InlineData("39053344705", true)]
        [InlineData("390.533.447-05", true)]
        // Check digits computed with weights 10..2 and 11..2, both mod 11.
        [InlineData("111.444.777-35", true)]
        // python-stdnum doctest: wrong check digits.
        [InlineData("23100299900", false)]
        // Second check digit changed from 5 to 4.
        [InlineData("39053344704", false)]
        // All zeros satisfies both check digits but is not an issued CPF.
        [InlineData("00000000000", false)]
        [InlineData("3905334470", false)]
        [InlineData("390533447A5", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestIndividualCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _brazilValidator.ValidateIndividualTaxCode(code).IsValid);
        }

        [Theory]
        // python-stdnum doctest for the CNPJ.
        [InlineData("16.727.230/0001-97", true)]
        [InlineData("16727230000197", true)]
        // Check digits computed with weights 5,4,3,2,9..2 and 6,5,4,3,2,9..2, both mod 11.
        [InlineData("11222333000181", true)]
        // python-stdnum doctest: wrong check digits.
        [InlineData("16727230000198", false)]
        // All zeros satisfies both check digits but is not an issued CNPJ.
        [InlineData("00000000000000", false)]
        [InlineData("1672723000019", false)]
        [InlineData("1672723000019X", false)]
        // Eastern Arabic digits: .NET \d would match them and int.Parse would then throw.
        [InlineData("\u0661\u0666\u0667\u0662\u0667\u0662\u0663\u0660\u0660\u0660\u0660\u0661\u0669\u0667", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectEntityCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _brazilValidator.ValidateEntity(code).IsValid);
        }

        [Theory]
        // The VAT number is the CNPJ.
        [InlineData("16.727.230/0001-97", true)]
        [InlineData("11222333000181", true)]
        [InlineData("16727230000198", false)]
        [InlineData("1672723000019", false)]
        [InlineData("abc", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestCorrectVatCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _brazilValidator.ValidateVAT(code).IsValid);
        }

        [Theory]
        // CEP of Avenida Paulista, Sao Paulo.
        [InlineData("01310100", true)]
        [InlineData("01310-100", true)]
        [InlineData("1310100", false)]
        [InlineData("013101000", false)]
        [InlineData("abcdefgh", false)]
        // Eastern Arabic digits are not a CEP.
        [InlineData("\u0660\u0661\u0663\u0661\u0660\u0661\u0660\u0660", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TestPostalCode(string code, bool isValid)
        {
            Assert.Equal(isValid, _brazilValidator.ValidatePostalCode(code).IsValid);
        }
    }
}
