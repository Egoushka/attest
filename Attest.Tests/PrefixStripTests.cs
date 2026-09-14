using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// A country code is a prefix. Stripping it with String.Replace removed it wherever it appeared,
    /// so two characters that happened to spell the country turned a number nobody was issued into a
    /// valid one: "85PL67346215" normalised to the Polish NIP 8567346215 and passed its checksum.
    /// 40 validators did this.
    ///
    /// Each row asserts the whole contract in both directions, because the fix is only correct if it
    /// keeps what should be kept: the bare number is valid, the printed form carrying its prefix is
    /// valid in either case, and the same letters anywhere else in the value are not a prefix.
    ///
    /// Every bare value here comes from that country's own test file and was re-checked.
    /// </summary>
    public class PrefixStripTests
    {
        private readonly CountryValidator _validator = new CountryValidator();

        public enum Kind
        {
            IndividualTaxCode,
            Entity,
            Vat,
            PostalCode,
        }

        [Theory]
        // VAT numbers, where the prefix is the ISO country code.
        [InlineData(Country.PL, Kind.Vat, "PL", "8567346215")]
        [InlineData(Country.DE, Kind.Vat, "DE", "136695976")]
        [InlineData(Country.BE, Kind.Vat, "BE", "0428759497")]
        [InlineData(Country.IT, Kind.Vat, "IT", "00743110157")]
        [InlineData(Country.PT, Kind.Vat, "PT", "501442600")]
        [InlineData(Country.RO, Kind.Vat, "RO", "18547290")]
        [InlineData(Country.SE, Kind.Vat, "SE", "556188840401")]
        [InlineData(Country.GB, Kind.Vat, "GB", "980780684")]
        [InlineData(Country.LU, Kind.Vat, "LU", "10000356")]
        [InlineData(Country.EE, Kind.Vat, "EE", "100931558")]
        [InlineData(Country.HU, Kind.Vat, "HU", "12892312")]
        [InlineData(Country.CZ, Kind.Vat, "CZ", "25123891")]
        [InlineData(Country.SI, Kind.Vat, "SI", "50223054")]
        [InlineData(Country.NL, Kind.Vat, "NL", "004495445B01")]
        [InlineData(Country.DK, Kind.Vat, "DK", "13585628")]
        [InlineData(Country.HR, Kind.Vat, "HR", "33392005961")]
        [InlineData(Country.RU, Kind.Vat, "RU", "7707083893")]
        [InlineData(Country.FI, Kind.Vat, "FI", "09853608")]
        [InlineData(Country.RS, Kind.Vat, "RS", "101134702")]
        [InlineData(Country.IS, Kind.Vat, "IS", "12345")]
        [InlineData(Country.SM, Kind.Vat, "SM", "24165")]
        [InlineData(Country.VE, Kind.Vat, "VE", "V114756110")]
        [InlineData(Country.NO, Kind.Vat, "NO", "988077917")]
        [InlineData(Country.TR, Kind.Vat, "TR", "4540536920")]
        [InlineData(Country.MT, Kind.Vat, "MT", "12345634")]
        [InlineData(Country.AD, Kind.Vat, "AD", "U132950X")]

        // Greece prints EL rather than GR on a VAT number, and the validator accepts both.
        [InlineData(Country.GR, Kind.Vat, "EL", "094259216")]

        // Prefixes that are not the country's own ISO code.
        [InlineData(Country.AT, Kind.Entity, "FN", "122119m")]        // Firmenbuchnummer
        [InlineData(Country.MC, Kind.Vat, "FR", "53000004605")]       // Monaco files under a French number
        [InlineData(Country.CH, Kind.Vat, "CH", "E107787577IVA")]     // the UID itself starts CHE
        [InlineData(Country.BY, Kind.Vat, "UNP", "MA1953684")]        // a label, not a country code

        // Identifiers other than VAT numbers.
        [InlineData(Country.CL, Kind.IndividualTaxCode, "CL", "76086428-5")]
        [InlineData(Country.SV, Kind.IndividualTaxCode, "SV", "0614-050707-104-8")]
        [InlineData(Country.CO, Kind.Entity, "CO", "2131234321")]
        [InlineData(Country.UY, Kind.Entity, "UY", "211003420017")]
        [InlineData(Country.NZ, Kind.Entity, "NZ", "49091850")]

        // Postal codes, which carry the prefix as often as they do not.
        [InlineData(Country.LV, Kind.PostalCode, "LV", "1000")]
        [InlineData(Country.LT, Kind.PostalCode, "LT", "01100")]
        [InlineData(Country.SI, Kind.PostalCode, "SI", "1000")]
        public void APrefixIsStrippedFromTheStartAndNowhereElse(Country country, Kind kind, string prefix, string bare)
        {
            Assert.True(Validate(bare, country, kind),
                $"{country}: the bare value \"{bare}\" should be valid, so the rest of this row proves nothing.");

            Assert.True(Validate(prefix + bare, country, kind),
                $"{country}: \"{prefix + bare}\" is how the number is printed and must stay valid.");

            Assert.True(Validate(prefix.ToLowerInvariant() + bare, country, kind),
                $"{country}: the prefix is written either way on an invoice.");

            // The letters somewhere other than the front. Before the fix these normalised to the
            // bare number and were accepted.
            Assert.False(Validate(Splice(bare, prefix), country, kind),
                $"{country}: \"{Splice(bare, prefix)}\" is not a number anyone was issued.");

            Assert.False(Validate(bare + prefix, country, kind),
                $"{country}: \"{bare + prefix}\" has the code as a suffix, which is not a prefix.");
        }

        /// <summary>Puts the prefix inside the value rather than in front of it.</summary>
        private static string Splice(string bare, string prefix)
        {
            return bare.Substring(0, 2) + prefix + bare.Substring(2);
        }

        private bool Validate(string value, Country country, Kind kind)
        {
            switch (kind)
            {
                case Kind.IndividualTaxCode:
                    return _validator.ValidateIndividualTaxCode(value, country).IsValid;
                case Kind.Entity:
                    return _validator.ValidateEntity(value, country).IsValid;
                case Kind.Vat:
                    return _validator.ValidateVAT(value, country).IsValid;
                default:
                    return _validator.ValidateZIPCode(value, country).IsValid;
            }
        }
    }
}
