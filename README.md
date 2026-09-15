# Attest

[![CI](https://github.com/Egoushka/attest/actions/workflows/ci.yml/badge.svg)](https://github.com/Egoushka/attest/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Attest.svg)](https://www.nuget.org/packages/Attest)

Validates national identification numbers, tax identification numbers, VAT codes and postal codes
for 87 countries.

Attest started as a fork of [CountryValidator](https://github.com/anghelvalentin/CountryValidator)
by Anghel Valentin, which has had no release since 1.1.3. The country coverage comes from that
project; 197 defects in it have been fixed since, each against the rule the country actually
publishes. See [NOTICE](NOTICE) for attribution and [CHANGELOG.md](CHANGELOG.md) for the detail.

## Install

```
dotnet add package Attest
dotnet add package Attest.DataAnnotations
```

Targets `netstandard2.0` and `net8.0`. The newest minor is the supported line — fixes ship forward
rather than being backported — and `netstandard2.0` stays until keeping it costs something.

A verdict change is never a patch here, so a minor release can change the answer for numbers you have
already stored. [CHANGELOG.md](CHANGELOG.md) says which, in both directions, for every release.

**Coming from CountryValidator?** [MIGRATION.md](MIGRATION.md) lists every verdict that changes,
in both directions. Read the section on numbers that used to be accepted and now are not before
you upgrade anything that stores what it validated.

## What is different from upstream

The short version of three repair waves, all of it verified by running the code rather than reading
it:

- **Hungary** accepted none of 181,677 valid tax ids. Its checksum summed UTF-16 code units instead
  of digit values, which shifted every result by a constant.
- **22 of 87 validators threw** instead of returning a result — on `null`, on short input, on a
  letter where a digit belonged. A sweep over every country and every method now proves none does.
- **Mexico and South Africa** could never validate anything: their date helpers always returned
  false. **Malaysia, Canada and San Marino** had inverted guards, so they accepted everything,
  including the empty string, and rejected the real formats.
- **Test data was fiction.** Thailand's tax-code tests used a Swedish personnummer; Cyprus's only
  national-id test was a Czech rodné číslo copied from the Czech file. Both passed for years.
- **Every bug report left open upstream is answered.** Ten of the twelve open issues report a
  defect — the other two ask whether the project is maintained and what the four `Validate` methods
  mean. The ten are the Netherlands' post-2020 btw-identificatienummer, Belgium's check number below
  ten (reported twice), Belgian enterprise numbers starting with 1, Finland's 2023 HETU separators,
  the printed Swiss TVA number, Mexico's unreachable company RFC, Chile, Paraguay and Uruguay, the
  Indian Aadhaar validator throwing on a letter and the GSTIN that replaced the VAT TIN in 2017, and
  the French VAT number capped at nine characters. Each is asserted with its reporter's own value in
  [UpstreamReportTests.cs](Attest.Tests/UpstreamReportTests.cs).

The test suite went from 586 cases to 4,212, and every validator now has one. What is still weak is
written down rather than hidden: [KNOWN-ISSUES.md](KNOWN-ISSUES.md) lists 33 entries by country: 19
still-open gaps, almost all of them check digits no authority publishes, and 14 the repair waves investigated and closed as decided rather than pending.

## Use

```csharp
using Attest;

var validator = new CountryValidator();

ValidationResult result = validator.ValidateIndividualTaxCode("93051822361", Country.BE);
if (!result.IsValid)
{
    Console.WriteLine(result.ErrorMessage);
}
```

### Asking by category

When you do not want to name a method — "is this any business identifier for this country?" — ask by
kind:

```csharp
using Attest;

var validator = new CountryValidator();

IdentifierResult result = validator.Validate(value, Country.BE, IdentifierKind.Business);

if (result.IsValid && !result.IsAmbiguous)
{
    // definitely a company number or a VAT number
}
```

`IdentifierKind` is a flags enum: `PersonalId`, `PersonalTaxCode`, `CompanyNumber`, `Vat`,
`PostalCode`, plus the combinations `Person`, `Business` and `Any` (the default, which covers
personal and business identifiers but not postal codes).

Two things the result tells you that a plain boolean cannot:

- **`IsAmbiguous`** — a few countries issue one number that serves as both a personal and a business
  identifier. Armenia (the 8 digit ՀՎՀՀ, whose digits carry no meaning by the State Revenue
  Committee's own account) and Nigeria issue literally one number for both. In Russia, Peru,
  Indonesia and Ukraine the overlap is narrower: a sole trader files VAT under their personal
  number, so that number is a VAT identifier as well as a personal one. There, a value asked about
  as a business identifier can be valid and indistinguishable at the same time, and the flag says so
  instead of guessing.
- **`Supports(country, kind)`** — whether the country has a rule for that kind at all. A kind with no
  rule reports every value invalid, which is not a verdict on the value. 25 of the 435 country/kind
  pairs in this library have no rule.

`Matched` lists every kind the value is valid as, and `Details` carries the individual
`ValidationResult` for each kind that was evaluated.

Each country also has its own validator class in `Attest.Countries`, if you want to skip the
dispatch:

```csharp
using Attest.Countries;

bool valid = new BelgiumValidator().ValidateVAT("BE0428759497").IsValid;
```

### Using Data Annotations

```csharp
[HttpPost]
public IActionResult ValidateSSN([Required, SSNAttribute(Country.US)] string ssn)
{
    if (!ModelState.IsValid)
    {
        return BadRequest();
    }

    return Ok();
}
```

## Supported countries
|   Supported Country  | Alpha Code 2 |                        National Identification Number Name                        |                                   VAT Code                                  | Entity code                                            | Postal Code        |
|:--------------------:|:------------:|:---------------------------------------------------------------------------------:|:---------------------------------------------------------------------------:|--------------------------------------------------------|--------------------|
| Andorra              | AD           | NRT (Número de Registre Tributari, Andorra tax number)                            | NRT (Número de Registre Tributari, Andorra tax number)                      | NRT (Número de Registre Tributari, Andorra tax number) | :heavy_check_mark: |
| United Arab Emirates | AE           |                                                                                   | :x:                                                                         | :x:                                                    | :x:                |
| Albania              | AL           | Identity Number - Numri i Identitetit (NID)                                       | NIPT (Numri i Identifikimit për Personin e Tatueshëm, Albanian VAT number). | NIPT                                                   | :heavy_check_mark: |
| Armenia              | AM           | TIN Number                                                                        | TIN Number                                                                  | TIN Number                                             | :heavy_check_mark: |
| Argentina            | AR           | DNI number                                                                        | VAT/IVA/CUIT                                                                | CUIT                                                   | :heavy_check_mark: |
| Austria              | AT           | Versicherungsnummer (VNR, SVNR, VSNR)                                             | UID (Umsatzsteuer-Identifikationsnummer)                                    |                                                        | :heavy_check_mark: |
| Australia            | AU           | TFN                                                                               | ABN                                                                         | ABN/ACN/TFN                                            | :heavy_check_mark: |
| Azerbaijan           | AZ           | PIN - Personal Identification Number                                              | VÖEN/TIN Number                                                             | VÖEN/TIN Number                                        | :heavy_check_mark: |
| Bosnia               | BA           | Unique Master Citizen Number JMBG                                                 |                                                                             | JIB (Jedinstveni identifikacioni broj)                 | :heavy_check_mark: |
| Belgium              | BE           | Rijksregisternummer                                                               | BTW, TVA, NWSt, ondernemingsnummer (Belgian enterprise number).             |                                                        | :heavy_check_mark: |
| Bulgaria             | BG           | Edinen grazhdanski nomer (EGN)                                                    | Идентификационен номер по ДДС                                               |                                                        | :heavy_check_mark: |
| Bahrain              | BH           | Social security number                                                            | :x:                                                                         | :x:                                                    | :heavy_check_mark: |
| Bolivia              | BO           | CI Number                                                                         | Número de Identificación Tributaria                                         | Número de Identificación Tributaria                    | :heavy_check_mark: |
| Brazil               | BR           | CPF                                                                               | Brazil Cadastro Nacional da Pessoa Juridica (CNPJ)                          | Brazil Cadastro Nacional da Pessoa Juridica (CNPJ)     | :heavy_check_mark: |
| Belarus              | BY           | Payer's account number (UNP)                                                      | Payer's account number (UNP)                                                | Payer's account number (UNP)                           | :heavy_check_mark: |
| Canada               | CA           | SIN Number                                                                        | Business Number                                                             | Business Number                                        | :heavy_check_mark: |
| Chile                | CL           | National Tax Number (RUN/RUT)                                                     | National Tax Number (RUN/RUT)                                               | National Tax Number (RUN/RUT)                          | :heavy_check_mark: |
| China                | CN           | Social Number (15 digits and 18 digits)                                           | :x:                                                                         | Business Number                                                    | :heavy_check_mark: |
| Colombia             | CO           | NIT (Número De Identificación Tributaria, Colombian identity code)                | VAT                                                                         | RUT (Registro Unico Tributario)                        | :heavy_check_mark: |
| Costa Rica           | CR           | CPF (Cédula de Persona Física,physical person ID number)/CR(Cédula de Residencia) | CPJ                                                                         | CPJ                                                    | :heavy_check_mark: |
| Cuba                 | CU           | NI (Número de identidad)                                                          | :x:                                                                         | :x:                                                    | :heavy_check_mark: |
| Croatia              | HR           | OIB (Osobni identifikacijski broj, Croatian identification number)                | OIB (Osobni identifikacijski broj, Croatian identification number)          | OIB (Osobni identifikacijski broj)                     | :heavy_check_mark: |
| Cyprus               | CY           | Identity Number                                                                   | ΦΠΑ                                                                         |                                                        | :heavy_check_mark: |
| Czechia              | CZ           | Rodné Císlo (RČ)                                                                  | Danove Identifikacni Cislo (DIC/VAT)                                        |                                                        | :heavy_check_mark: |
| Denmark              | DK           | Det Centrale Personregister (CPR)                                                 | Momsregistreringsnummer (CVR/VAT)                                           |                                                        | :heavy_check_mark: |
| Dominican Republic   | DO           | Cedula                                                                            | RNC (Registro Nacional del Contribuyente)                                   | RNC (Registro Nacional del Contribuyente)              | :heavy_check_mark: |
| Ecuador              | EC           | Registro Unico de Contribuyentes (RUC)                                            | Registro Unico de Contribuyentes (RUC)                                      | Registro Unico de Contribuyentes (RUC)                 | :heavy_check_mark: |
| Estonia              | EE           | Personal Id - Isikukood                                                           | Kaibemaksukohuslase (KMKR)                                                  | Registrikood (Estonian organisation registration code) | :heavy_check_mark: |
| Finland              | FI           | Henkilötunnus (HETU)                                                              | Arvonlisaveronumero (ALV)                                                   | Arvonlisaveronumero (ALV)                              | :heavy_check_mark: |
| Faroe Islands        | FO           | P-number                                                                          | V-number                                                                    | V-number                                               | :heavy_check_mark: |
| France               | FR           | INSEE/NIR (French personal identification number)                                 | Taxe sur la Valeur Ajoutee (TVA)                                            | SIREN                                                  | :heavy_check_mark: |
| Georgia              | GE           | Personal Number (piradi nomeri)                                                   | VAT Number                                                                  | Identification Number (sakidentifikatsio nomeri)       | :heavy_check_mark: |
| Germany              | DE           | Steueridentifikationsnummer                                                       | Umsatzsteur Identifikationnummer (VAT)                                      | Steuernummer                                           | :heavy_check_mark: |
| Greece               | GR           | AMKA (Αριθμός Μητρώου Κοινωνικής Ασφάλισης, Greek social security number)         | VAT Number (FPA)                                                            | VAT Number (FPA)                                       | :heavy_check_mark: |
| Great Britain        | GB           | NINO or NHS                                                                       | VAT Reg No                                                                  | Value added tax registration number                    | :heavy_check_mark: |
| Guatemala            | GT           | :x:                                                                               | NIT                                                                         | NIT (Número de Identificación Tributaria)              | :heavy_check_mark: |
| Hong Kong            | HK           | Social number                                                                     | :x:                                                                         | :x:                                                    | :x:                |
| Hungary              | HU           | Szemelyi Szam Ellenorzese                                                         | Kozossegi Adoszam (ANUM)                                                    | Cegjegyzekszam Ellenorzese                             | :heavy_check_mark: |
| Indonesia            | ID           | NPWP                                                                              | NPWP - Nomor Pokok Wajib Pajak                                              | NPWP                                                   | :heavy_check_mark: |
| Ireland              | IE           | PPS No (Personal Public Service Number, Irish personal number).                   | Irish Tax Reference Number (VAT)                                            |                                                        | :heavy_check_mark: |
| Israel               | IL           |                                                                                   |                                                                             |                                                        | :heavy_check_mark: |
| India                | IN           | PAN (Permanent Account Number)                                                    | GSTIN (VAT TIN / CST TIN before 2017)                                       | PAN (Permanent Account Number)                         | :heavy_check_mark: |
| Iceland              | IS           | Kennitala                                                                         | Virdisaukaskattsnumer (VSK)                                                 | Kennitala                                              | :heavy_check_mark: |
| Italy                | IT           | Codice fiscale - Fiscal Code                                                      | Partita IVA                                                                 |                                                        | :heavy_check_mark: |
| Japan                | JP           | Japan My Number                                                                   | Corporate Number (hōjin bangō)                                              | CN hōjin bangō, Japanese Corporate Number              | :heavy_check_mark: |
| Korea                | KR           | Resident Registration Number (RRN)                                                | :x:                                                                         | :x:                                                    | :heavy_check_mark: |
| Kazakhstan           | KZ           | PIN                                                                               | BIN                                                                         | BIN БСН – бизнес-сәйкестендіру нөмірі                  | :heavy_check_mark: |
| Latvia               | LV           | Personal Code - Personas kods                                                     | PVN (Pievienotās vērtības nodokļa, Latvian VAT number)                      | PVN (Pievienotās vērtības nodokļa, Latvian VAT number) | :heavy_check_mark: |
| Lithuania            | LT           | Personal Code - Asmens kodas                                                      | PVM (Pridėtinės vertės mokestis mokėtojo kodas, Lithuanian VAT number)      |                                                        | :heavy_check_mark: |
| Luxembourg           | LU           | Personal identification code (PIC)                                                | TVA (taxe sur la valeur ajoutée, Luxembourgian VAT number)                  |                                                        | :heavy_check_mark: |
| Malta                | MT           | Identity Card Number                                                              | VAT Number                                                                  |                                                        | :heavy_check_mark: |
| Monaco               | MC           | :x:                                                                               | VAT Number                                                                  | :x:                                                    | :heavy_check_mark: |
| Mexico               | MX           | CURP (Clave Única de Registro de Población)                                       | RFC (Registro Federal de Contribuyentes)                                    | RFC (Registro Federal de Contribuyentes)               | :heavy_check_mark: |
| Malaysia             | MY           | NRIC (National Registration Identity Card number)                                 | ITN (Income Tax Number)                                                     | ITN (Income Tax Number)                                | :heavy_check_mark: |
| Moldova              | MD           | IDNP (Identification Number of Person)                                            | Validate VAT code (Nr. de Inregistrare TVA)                                 |                                                        | :heavy_check_mark: |
| Montenegro           | ME           |                                                                                   |                                                                             |                                                        | :heavy_check_mark: |
| Macedonia            | MK           |                                                                                   | Vat Number                                                                  |                                                        | :heavy_check_mark: |
| Mauritius            | MU           | ID number (Mauritian national identifier)                                         |                                                                             |                                                        | :x:                |
| Netherlands          | NL           | Burgerservicenummer (BSN) - Citizen Service Number or Onderwijsnummer             | Omzetbelastingnummer (BTW)                                                  |                                                        | :heavy_check_mark: |
| Nigeria              | NG           | NIN (National Identification Number)                                              | TIN (Tax Identification Number)                                             | TIN (Tax Identification Number)                        | :heavy_check_mark: |
| Norway               | NO           |                                                                                   |                                                                             |                                                        | :heavy_check_mark: |
| New Zealand          | NZ           |                                                                                   |                                                                             |                                                        | :heavy_check_mark: |
| Peru                 | PE           | RUC                                                                               | RUC Peruvian company tax number                                             | RUC Peruvian company tax number                        | :heavy_check_mark: |
| Philippines          | PH           |                                                                                   |                                                                             |                                                        | :heavy_check_mark: |
| Pakistan             | PK           | CNIC (Computerized National Identity Card)                                        | :x:                                                                         | :x:                                                    | :heavy_check_mark: |
| Poland               | PL           | Polish National Identification Number (PESEL)                                     | Numer Identyfikacji Podatkowej (NIP)                                        |                                                        | :heavy_check_mark: |
| Portugal             | PT           | Número de identificação civil - NIC                                               | Numero de Identificacao Fiscal (NIF)                                        |                                                        | :heavy_check_mark: |
| Paraguay             | PY           | Registro Unico de Contribuyentes (RUC)                                            | Registro Unico de Contribuyentes (RUC)                                      | Registro Unico de Contribuyentes (RUC)                 | :heavy_check_mark: |
| Romania              | RO           | Cod Numeric Personal - Personal Numerical Code (CNP)                              | Cod fiscal TVA                                                              | Cod fiscal                                             | :heavy_check_mark: |
| Serbia               | RS           | Unique Master Citizen Number JMBG                                                 |                                                                             |                                                        | :heavy_check_mark: |
| Russia               | RU           | Taxpayer Personal Identification Number (INN)                                     | VAT Number                                                                  | VAT Number                                             | :heavy_check_mark: |
| Slovakia             | SK           | Rodné Císlo (RČ)                                                                  | Identifikačné číslo pre daň z pridanej hodnoty (IČ DPH)                     |                                                        | :heavy_check_mark: |
| Slovenia             | SI           | Unique Master Citizen Number JMBG                                                 | Identifikacijska številka za DDV                                            |                                                        | :heavy_check_mark: |
| San Marino           | SM           | COE (Codice operatore economico, San Marino national tax number)                  | COE (Codice operatore economico, San Marino national tax number)            | COE (Codice operatore economico)                       | :heavy_check_mark: |
| El Salvador          | SV           | NIT (Número de Identificación Tributaria, El Salvador tax number)                 |                                                                             |                                                        | :heavy_check_mark: |
| Thailand             | TH           | Thailand citizen number                                                           |                                                                             |                                                        | :heavy_check_mark: |
| Turkey               | TR           | T.C. Kimlik No. (Turkish personal identification number)                          | VKN (Vergi Kimlik Numarası, Turkish tax identification number)              | VKN (Vergi Kimlik Numarası)                            | :heavy_check_mark: |
| Taiwan               | TW           | SSN                                                                               | Unified Business Number (統一編號)                                              | Unified Business Number (統一編號)                         | :heavy_check_mark: |
| Spain                | ES           | DNI/NIF/NIE                                                                       | NIF / CIF                                                                   | NIF / CIF                                              | :heavy_check_mark: |
| Switzerland          | CH           | AHV (Sozialversicherungsnummer)                                                   | VAT, MWST, TVA, IVA, TPV (Mehrwertsteuernummer, the Swiss VAT number).      | UID Unternehmens-Identifikationsnummer                 | :heavy_check_mark: |
| Sweden               | SE           | Personnummer - Personal Identity Number                                           | VAT-nummer or momsnummer                                                    | Orgnr (Organisationsnummer, Swedish company number)    | :heavy_check_mark: |
| United States        | US           | Social Security Number                                                            | :x:                                                                         | EIN                                                    | :heavy_check_mark: |
| Ukraine              | UA           | Social Number                                                                     | VAT                                                                         | VAT                                                    | :heavy_check_mark: |
| Uzbekistan           | UZ           | PINFL (Personal Identification Number of a Physical Person)                       | :x:                                                                         | :x:                                                    | :heavy_check_mark: |
| Uruguay              | UY           | RUT numbers                                                                       | RUT numbers                                                                 | RUT numbers                                            | :heavy_check_mark: |
| Venezuela            | VE           | Registro de Informacion Fiscal (RIF)                                              | Registro de Informacion Fiscal (RIF)                                        | Registro de Informacion Fiscal (RIF)                   | :heavy_check_mark: |
| South Africa         | ZA           | Social Number                                                                     | VAT Code                                                                    | VAT Code                                               | :heavy_check_mark: |


<a href="https://buymeacoffee.com/egoushka" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" style="height: 51px !important;width: 217px !important;" ></a>

## Contributing

[CONTRIBUTING.md](CONTRIBUTING.md) has the rules this project runs on — every one of them exists
because a validator here was wrong for years without anyone noticing. The short version: cite a
published source for any rule you change, compute your own test numbers rather than copying them,
and never write a test that asserts behaviour you believe is wrong.

[docs/adding-a-country.md](docs/adding-a-country.md) walks through adding or fixing a country.

[KNOWN-ISSUES.md](KNOWN-ISSUES.md) is the backlog: weaknesses found and deliberately left alone,
each with the reason. Most are one validator and its test file, which makes them the natural place
to start.

Security reports go through [SECURITY.md](SECURITY.md). Conduct: [Contributor Covenant](CODE_OF_CONDUCT.md).

Upgrading from CountryValidator: [MIGRATION.md](MIGRATION.md).

## License

Apache License, Version 2.0 — see [LICENSE](LICENSE).

Copyright 2020 Anghel Valentin (original work)
Copyright 2026 Yehor Hrabovskyi (changes)

## Special thanks
[Python Stdnum](https://github.com/arthurdejong/python-stdnum)