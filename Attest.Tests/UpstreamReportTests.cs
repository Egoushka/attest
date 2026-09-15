using Attest;
using Xunit;

namespace Attest.Tests
{
    /// <summary>
    /// The reason this library exists is that CountryValidator stopped answering its bug reports.
    /// Ten of the twelve issues open upstream are defect reports; the other two ask whether the
    /// project is maintained and how the four Validate methods differ. README makes a claim about
    /// those ten, and a claim in a README is worth what it is tested at, so each one is asserted
    /// here with the value its reporter actually wrote and a link to the report.
    ///
    /// These rows duplicate coverage that already exists country by country, on purpose: the
    /// country files are organised around the rule, and this one is organised around the promise.
    /// A row that moves here is a promise that has stopped being true.
    ///
    /// https://github.com/anghelvalentin/CountryValidator/issues
    /// </summary>
    public class UpstreamReportTests
    {
        private readonly CountryValidator _validator = new CountryValidator();

        public enum Kind
        {
            IndividualTaxCode,
            NationalIdentity,
            Entity,
            Vat,
        }

        [Theory]
        // #9 -- the Netherlands replaced the btw-identificatienummer in 2020. The value is the
        // example on the Dutch government's own page, which upstream rejects.
        [InlineData(9, Country.NL, Kind.Vat, "NL000099998B57", true)]

        // #10 -- a check number below ten is written "09", and comparing it to int 9 as a string
        // rejected it. Roughly 9% of Belgian numbers were affected.
        [InlineData(10, Country.BE, Kind.NationalIdentity, "85071501709", true)]
        // The number the reporter pasted is not itself valid: 97 - 960917369 % 97 is 36 under the
        // 1900s reading and 65 under the 2000s one, never the 09 it carries, and python-stdnum's
        // be.nn rejects it too. Their diagnosis was right and their example was not.
        [InlineData(10, Country.BE, Kind.NationalIdentity, "96091736909", false)]

        // #13 -- the Swiss TVA number as it is printed, which upstream could not parse at all.
        [InlineData(13, Country.CH, Kind.Vat, "CHE-116.281.710 TVA", true)]
        [InlineData(13, Country.CH, Kind.Vat, "CHE-116.281.710 MWST", true)]
        [InlineData(13, Country.CH, Kind.Vat, "CHE116281710", true)]

        // #14 -- decree 690/2022 added century separators to the Finnish personal identity code
        // with effect from 1 January 2023. Y is one of the five new 1900s characters.
        [InlineData(14, Country.FI, Kind.NationalIdentity, "010594Y9032", true)]

        // #15 -- Belgian enterprise numbers began to be issued starting with 1 rather than 0.
        [InlineData(15, Country.BE, Kind.Vat, "BE1000003682", true)]

        // #19 -- a guard made the twelve character company RFC unreachable, so no company RFC
        // could validate.
        [InlineData(19, Country.MX, Kind.Entity, "MAB9307148T4", true)]

        // #21 -- reported as three countries at once, and all three were broken. README used to
        // credit this report for Paraguay alone.
        [InlineData(21, Country.CL, Kind.Entity, "76086428-5", true)]
        [InlineData(21, Country.PY, Kind.Entity, "80028061-0", true)]
        [InlineData(21, Country.UY, Kind.Entity, "211003420017", true)]

        // #22 -- the same Belgian defect as #10, reported two years later with a number of its own.
        [InlineData(22, Country.BE, Kind.NationalIdentity, "65.06.17 210.08", true)]

        // #24 -- the title is the Aadhaar validator throwing on a letter, and the body asks for the
        // GSTIN format. Both halves: the letter is answered rather than thrown on, and the GSTIN
        // that replaced the VAT TIN in 2017 validates. python-stdnum's own example.
        [InlineData(24, Country.IN, Kind.NationalIdentity, "2345678901ab", false)]
        [InlineData(24, Country.IN, Kind.Vat, "27AAPFU0939F1ZV", true)]

        // #27 -- the French VAT number was capped at nine characters. The reporter's value is the
        // one the European Commission's VIES service returns as valid.
        [InlineData(27, Country.FR, Kind.Vat, "68900296724", true)]
        public void TheReportIsAnswered(int upstreamIssue, Country country, Kind kind, string value, bool expected)
        {
            ValidationResult result = Validate(country, kind, value);

            Assert.True(
                expected == result.IsValid,
                string.Format(
                    "Upstream issue #{0}: {1} {2} \"{3}\" is {4}, expected {5}. {6}",
                    upstreamIssue, country, kind, value, result.IsValid, expected, result.ErrorMessage));
        }

        private ValidationResult Validate(Country country, Kind kind, string value)
        {
            switch (kind)
            {
                case Kind.IndividualTaxCode:
                    return _validator.ValidateIndividualTaxCode(value, country);
                case Kind.NationalIdentity:
                    return _validator.ValidateNationalIdentityCode(value, country);
                case Kind.Entity:
                    return _validator.ValidateEntity(value, country);
                default:
                    return _validator.ValidateVAT(value, country);
            }
        }
    }
}
