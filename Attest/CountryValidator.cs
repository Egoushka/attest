using Attest.Countries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Attest
{
    public class CountryValidator : ICountryValidator
    {
        static readonly Dictionary<Country, IdValidationAbstract> _supportedCountries;

        static readonly IdentifierKind[] _allKinds =
        {
            IdentifierKind.PersonalId,
            IdentifierKind.PersonalTaxCode,
            IdentifierKind.CompanyNumber,
            IdentifierKind.Vat,
            IdentifierKind.PostalCode,
        };

        static CountryValidator()
        {
            _supportedCountries = Load();
        }

        public static bool IsCountrySupported(Country country)
        {
            return _supportedCountries.ContainsKey(country);
        }

        public static List<string> SupportedCountries
        {
            get
            {
                return _supportedCountries.Keys.Select(c => c.ToString()).ToList();
            }
        }


        private static Dictionary<Country, IdValidationAbstract> Load()
        {
            Dictionary<Country, IdValidationAbstract> ssnCountries = new Dictionary<Country, IdValidationAbstract>
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

            return ssnCountries;
        }

        public ValidationResult ValidateIndividualTaxCode(string ssn, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateIndividualTaxCode(ssn);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        public ValidationResult ValidateVAT(string vat, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateVAT(vat);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        public ValidationResult ValidateEntity(string vat, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateEntity(vat);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        public ValidationResult ValidateNationalIdentityCode(string ssn, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidateNationalIdentity(ssn);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");

        }

        public ValidationResult ValidateZIPCode(string zip, Country country)
        {
            if (_supportedCountries.ContainsKey(country))
            {
                try
                {
                    return _supportedCountries[country].ValidatePostalCode(zip);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    // Some validators throw for a method they have no rule for; answer the
                    // same way an unregistered country is answered instead of escaping.
                    return ValidationResult.Invalid("Not supported");
                }
            }
            return ValidationResult.Invalid("Not supported");
        }
        /// <summary>
        /// Validates a value against a category of identifiers rather than one named method, so a
        /// caller can ask "is this any business identifier for this country" without knowing which
        /// of the country's methods to call.
        /// </summary>
        /// <param name="value">The identifier to validate.</param>
        /// <param name="country">The country that issued it.</param>
        /// <param name="kinds">
        /// The kinds to accept. Defaults to <see cref="IdentifierKind.Any"/>, which covers personal
        /// and business identifiers but not postal codes.
        /// </param>
        /// <remarks>
        /// Every personal and business kind is evaluated regardless of what was requested, because
        /// that is the only way to know whether a match is ambiguous — see
        /// <see cref="IdentifierResult.IsAmbiguous"/>. Kinds a country has no rule for are reported
        /// as "Not supported" in <see cref="IdentifierResult.Details"/> and never match.
        /// </remarks>
        public IdentifierResult Validate(string value, Country country, IdentifierKind kinds = IdentifierKind.Any)
        {
            var evaluate = kinds | IdentifierKind.Any;
            var details = new Dictionary<IdentifierKind, ValidationResult>();
            var matched = default(IdentifierKind);

            foreach (var kind in _allKinds)
            {
                if ((evaluate & kind) == 0)
                {
                    continue;
                }

                var result = ValidateSingleKind(value, country, kind);
                details[kind] = result;
                if (result.IsValid)
                {
                    matched |= kind;
                }
            }

            return new IdentifierResult(kinds, matched, details);
        }

        /// <summary>
        /// Whether this country has a rule for the given kind. A kind with no rule always reports
        /// the value as invalid, which is not the same answer as "this value is wrong".
        /// </summary>
        /// <param name="country">The country to ask about.</param>
        /// <param name="kind">A single kind. Combinations report true only if every kind is supported.</param>
        public bool Supports(Country country, IdentifierKind kind)
        {
            if (!_supportedCountries.ContainsKey(country))
            {
                return false;
            }

            var asked = false;
            foreach (var single in _allKinds)
            {
                if ((kind & single) == 0)
                {
                    continue;
                }

                asked = true;
                try
                {
                    // The validators that have no rule for a kind throw before looking at the
                    // input, so any value answers the question.
                    ValidateOnValidator(_supportedCountries[country], string.Empty, single);
                }
                catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
                {
                    return false;
                }
            }

            return asked;
        }

        private ValidationResult ValidateSingleKind(string value, Country country, IdentifierKind kind)
        {
            if (!_supportedCountries.ContainsKey(country))
            {
                return ValidationResult.Invalid("Not supported");
            }

            try
            {
                return ValidateOnValidator(_supportedCountries[country], value, kind);
            }
            catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)
            {
                return ValidationResult.Invalid("Not supported");
            }
        }

        private static ValidationResult ValidateOnValidator(IdValidationAbstract validator, string value, IdentifierKind kind)
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
    }
}
