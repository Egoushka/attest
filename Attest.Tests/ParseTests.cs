using System;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// Parse decodes only what a cited authority page states. Each row below was decoded by hand from
    /// the rule the source gives, not by running Parse.
    /// </summary>
    public class ParseTests
    {
        [Theory]
        [InlineData("pl.nationalid", "83010411457", 1983, 1, 4, Sex.Male)]
        [InlineData("pl.nationalid", "00222900016", 2000, 2, 29, Sex.Male)]
        [InlineData("bg.nationalid", "8001010008", 1980, 1, 1, Sex.Male)]
        [InlineData("bg.nationalid", "7523169263", 1875, 3, 16, Sex.Male)]
        [InlineData("fi.nationalid", "131052-308T", 1952, 10, 13, Sex.Female)]
        [InlineData("fi.nationalid", "010594Y9032", 1994, 5, 1, Sex.Male)]
        public void DecodesDateAndSex(string id, string value, int year, int month, int day, Sex sex)
        {
            ParseResult parsed = Identifiers.Find(id).Parse(value);

            Assert.True(parsed.Result.IsValid);
            Assert.Equal(new DateTime(year, month, day), parsed.BirthDate);
            Assert.Equal(sex, parsed.Sex);
        }

        [Theory]
        [InlineData("se.nationalid", "8112289874", Sex.Male)]
        [InlineData("se.taxcode", "8112289874", Sex.Male)]
        [InlineData("no.nationalid", "15078512323", Sex.Male)]
        [InlineData("dk.taxcode", "2110625629", Sex.Male)]
        [InlineData("se.taxcode", "811228+9874", Sex.Male)]
        public void DecodesSexWhereTheCenturyIsNotPublished(string id, string value, Sex sex)
        {
            ParseResult parsed = Identifiers.Find(id).Parse(value);

            Assert.True(parsed.Result.IsValid);
            Assert.Null(parsed.BirthDate);
            Assert.Equal(sex, parsed.Sex);
        }

        [Fact]
        public void AnInvalidValueDecodesNothing()
        {
            ParseResult parsed = Identifiers.Find("pl.nationalid").Parse("83010411458");

            Assert.False(parsed.Result.IsValid);
            Assert.Null(parsed.BirthDate);
            Assert.Null(parsed.Sex);
            Assert.Empty(parsed.Parts);
        }

        [Fact]
        public void ParsingAnyValueWithAnyTypeNeverThrowsAndAnInvalidValueDecodesNothing()
        {
            foreach (IdentifierType type in Identifiers.All)
            {
                foreach (string value in new[] { "", "0", "83010411457", "8112289874", "131052-308T", "198112289874", "811228+9874", "99999999999", "ABCDEFGHIJK" })
                {
                    ParseResult parsed = type.Parse(value);
                    if (!parsed.Result.IsValid)
                    {
                        Assert.Null(parsed.BirthDate);
                        Assert.Null(parsed.Sex);
                    }
                }
            }
        }
    }
}
