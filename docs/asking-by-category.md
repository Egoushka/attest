# Asking by category

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
