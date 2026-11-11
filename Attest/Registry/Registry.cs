#nullable enable
using Attest.Countries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Attest
{
    /// <summary>
    /// The table of every identifier type, built once. Today it is the 87 1.x validators exposed
    /// five kinds each; native rules register here as they are ported.
    /// </summary>
    internal static class Registry
    {
        /// <summary>The five 1.x kinds, in the order types are listed.</summary>
        internal static readonly IdentifierKind[] LegacyKinds =
        {
            IdentifierKind.PersonalId,
            IdentifierKind.PersonalTaxCode,
            IdentifierKind.CompanyNumber,
            IdentifierKind.Vat,
            IdentifierKind.PostalCode,
        };

        /// <summary>The 1.x validator of each country, which is the rule code of every adapted type.</summary>
        internal static readonly Dictionary<Country, IdValidationAbstract> Validators = LoadValidators();

        internal static readonly IReadOnlyList<IdentifierType> All;

        private static readonly Dictionary<string, IdentifierType> ById;
        private static readonly Dictionary<Country, List<IdentifierType>> ByCountry;

        static Registry()
        {
            var all = new List<IdentifierType>();
            ById = new Dictionary<string, IdentifierType>(StringComparer.OrdinalIgnoreCase);
            ByCountry = new Dictionary<Country, List<IdentifierType>>();

            foreach (KeyValuePair<Country, IdValidationAbstract> pair in Validators)
            {
                var forCountry = new List<IdentifierType>();
                string code = pair.Key.ToString().ToLowerInvariant();
                string country = CountryName(pair.Value);

                foreach (IdentifierKind kind in LegacyKinds)
                {
                    bool supported = (pair.Value.UnsupportedKinds & kind) == 0;
                    string slug, alias, description;
                    Describe(kind, out slug, out alias, out description);

                    string id = code + "." + slug;
                    var type = new IdentifierType(
                        id,
                        pair.Key,
                        kind,
                        country + " " + description,
                        country + " " + description,
                        new[] { code + "." + alias },
                        new LegacyRule(id, pair.Value, kind, supported));

                    Add(type);
                    all.Add(type);
                    forCountry.Add(type);
                }

                ByCountry[pair.Key] = forCountry;
            }

            // International types: no country, so they are in All and Find but in no For(country).
            var international = new List<IdentifierType>
            {
                new IdentifierType("iban", null, IdentifierKind.BankAccount, "International Bank Account Number", "IBAN", new string[0], new IbanRule(Provenance.For("iban", true))),
                Simple("issn", IdentifierKind.TradeItem, "International Standard Serial Number", "ISSN", new[] { '-', ' ' }, InternationalRules.Issn, InternationalRules.IssnFormat),
                Simple("isbn", IdentifierKind.TradeItem, "International Standard Book Number", "ISBN", new[] { '-', ' ' }, InternationalRules.Isbn, null),
                Simple("gtin", IdentifierKind.TradeItem, "Global Trade Item Number (EAN, UPC)", "GTIN", new[] { '-', ' ' }, InternationalRules.Gtin, null, "ean", "upc"),
                Simple("isin", IdentifierKind.Security, "International Securities Identification Number", "ISIN", new[] { '-', ' ' }, InternationalRules.Isin, null),
                Simple("lei", IdentifierKind.Security, "Legal Entity Identifier", "LEI", new[] { '-', ' ' }, InternationalRules.Lei, null),
                Simple("bic", IdentifierKind.BankAccount, "Business Identifier Code (SWIFT BIC)", "BIC", new[] { ' ' }, InternationalRules.Bic, null, "swift"),
            };
            foreach (IdentifierType type in international)
            {
                Add(type);
                all.Add(type);
            }

            All = all.AsReadOnly();
        }

        private static IdentifierType Simple(string id, IdentifierKind kind, string name, string local, char[] separators, Func<string, Outcome> check, Func<string, string>? format, params string[] aliases)
        {
            return new IdentifierType(id, null, kind, name, local, aliases, new SimpleRule(Provenance.For(id, true), separators, check, format));
        }

        internal static IdentifierType? Find(string? id)
        {
            if (id == null)
            {
                return null;
            }

            IdentifierType? type;
            return ById.TryGetValue(id.Trim(), out type) ? type : null;
        }

        internal static IReadOnlyList<IdentifierType> For(Country country)
        {
            List<IdentifierType>? types;
            return ByCountry.TryGetValue(country, out types) ? types : new List<IdentifierType>();
        }

        /// <summary>Calls the 1.x method that answers for one kind.</summary>
        internal static ValidationResult Run(IdValidationAbstract validator, string value, IdentifierKind kind)
        {
            switch (kind)
            {
                case IdentifierKind.PersonalId:
                    return validator.ValidateNationalIdentity(value);
                case IdentifierKind.PersonalTaxCode:
                    return validator.ValidateIndividualTaxCode(value);
                case IdentifierKind.CompanyNumber:
                    return validator.ValidateEntity(value);
                case IdentifierKind.Vat:
                    return validator.ValidateVAT(value);
                default:
                    return validator.ValidatePostalCode(value);
            }
        }

        private static void Add(IdentifierType type)
        {
            ById.Add(type.Id, type);
            foreach (string alias in type.Aliases)
            {
                ById.Add(alias, type);
            }
        }

        private static void Describe(IdentifierKind kind, out string slug, out string alias, out string description)
        {
            switch (kind)
            {
                case IdentifierKind.PersonalId:
                    slug = "nationalid"; alias = "nationalidentity"; description = "national identity number";
                    break;
                case IdentifierKind.PersonalTaxCode:
                    slug = "taxcode"; alias = "individualtaxcode"; description = "personal tax code";
                    break;
                case IdentifierKind.CompanyNumber:
                    slug = "company"; alias = "entity"; description = "company number";
                    break;
                case IdentifierKind.Vat:
                    slug = "vat"; alias = "vatin"; description = "VAT number";
                    break;
                default:
                    slug = "postalcode"; alias = "zipcode"; description = "postal code";
                    break;
            }
        }

        /// <summary>"UnitedStatesValidator" becomes "United States".</summary>
        private static string CountryName(IdValidationAbstract validator)
        {
            string name = validator.GetType().Name;
            if (name.EndsWith("Validator", StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - "Validator".Length);
            }

            if (name == "Kazahstan")
            {
                name = "Kazakhstan";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && name[i] >= 'A' && name[i] <= 'Z')
                {
                    builder.Append(' ');
                }

                builder.Append(name[i]);
            }

            return builder.ToString();
        }

        private static Dictionary<Country, IdValidationAbstract> LoadValidators()
        {
            return new Dictionary<Country, IdValidationAbstract>
            {
                { Country.AD, new AndorraValidator() },
                { Country.AE, new UnitedArabEmiratesValidator() },
                { Country.AL, new AlbaniaValidator() },
                { Country.AM, new ArmeniaValidator() },
                { Country.AR, new ArgentinaValidator() },
                { Country.AT, new AustriaValidator() },
                { Country.AU, new AustraliaValidator() },
                { Country.AZ, new AzerbaijanValidator() },
                { Country.BA, new BosniaValidator() },
                { Country.BE, new BelgiumValidator() },
                { Country.BG, new BulgariaValidator() },
                { Country.BH, new BahrainValidator() },
                { Country.BO, new BoliviaValidator() },
                { Country.BR, new BrazilValidator() },
                { Country.BY, new BelarusValidator() },
                { Country.CA, new CanadaValidator() },
                { Country.CH, new SwitzerlandValidator() },
                { Country.CL, new ChileValidator() },
                { Country.CN, new ChinaValidator() },
                { Country.CO, new ColombiaValidator()},
                { Country.CR, new CostaRicaValidator()},
                { Country.CU, new CubaValidator()},
                { Country.CY, new CyprusValidator()},
                { Country.CZ, new CzechValidator()},
                { Country.DE, new GermanyValidator() },
                { Country.DK, new DenmarkValidator() },
                { Country.DO, new DominicanRepublicValidator() },
                { Country.EC, new EcuadorValidator() },
                { Country.EE, new EstoniaValidator() },
                { Country.ES, new SpainValidator() },
                { Country.FI, new FinlandValidator() },
                { Country.FO, new FaroeIslandsValidator() },
                { Country.FR, new FranceValidator() },
                { Country.GB, new UnitedKingdomValidator() },
                { Country.GE, new GeorgiaValidator() },
                { Country.GR, new GreeceValidator() },
                { Country.GT, new GuatemalaValidator() },
                { Country.HK, new HongKongValidator() },
                { Country.HR, new CroatiaValidator() },
                { Country.HU, new HungaryValidator() },
                { Country.ID, new IndonesiaValidator() },
                { Country.IE, new IrelandValidator() },
                { Country.IL, new IsraelValidator() },
                { Country.IN, new IndiaValidator() },
                { Country.IS, new IcelandValidator() },
                { Country.IT, new ItalyValidator()},
                { Country.JP, new JapanValidator() },
                { Country.KR, new KoreaValidator() },
                { Country.KZ, new KazahstanValidator() },
                { Country.LT, new LithuaniaValidator() },
                { Country.LU, new LuxembourgValidator() },
                { Country.LV, new LatviaValidator() },
                { Country.MC, new MonacoValidator() },
                { Country.MD, new MoldovaValidator() },
                { Country.ME, new MontenegroValidator() },
                { Country.MK, new MacedoniaValidator() },
                { Country.MT, new MaltaValidator() },
                { Country.MU, new MauritiusValidator() },
                { Country.MX, new MexicoValidator() },
                { Country.MY, new MalaysiaValidator() },
                { Country.NG, new NigeriaValidator() },
                { Country.NL, new NetherlandsValidator() },
                { Country.NO, new NorwayValidator() },
                { Country.NZ, new NewZealandValidator() },
                { Country.PE, new PeruValidator() },
                { Country.PH, new PhilippinesValidator() },
                { Country.PK, new PakistanValidator() },
                { Country.PL, new PolandValidator() },
                { Country.PT, new PortugalValidator() },
                { Country.PY, new ParaguayValidator() },
                { Country.RO, new RomaniaValidator() },
                { Country.RS, new SerbiaValidator() },
                { Country.RU, new RussiaValidator() },
                { Country.SE, new SwedenValidator() },
                { Country.SI, new SloveniaValidator() },
                { Country.SK, new SlovakiaValidator() },
                { Country.SM, new SanMarinoValidator() },
                { Country.SV, new ElSalvadorValidator() },
                { Country.TH, new ThailandValidator() },
                { Country.TR, new TurkeyValidator() },
                { Country.TW, new TaiwanValidator() },
                { Country.UA, new UkraineValidator() },
                { Country.US, new UnitedStatesValidator() },
                { Country.UY, new UruguayValidator() },
                { Country.UZ, new UzbekistanValidator() },
                { Country.VE, new VenezuelaValidator() },
                { Country.ZA, new SouthAfricaValidator() }

            };
        }
    }
}
