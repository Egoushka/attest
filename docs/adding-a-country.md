# Adding or fixing a country

Every validator here is one class, one test file and two registrations. The mechanical part takes
twenty minutes. The part that matters is the first step: finding the rule and proving it. 197
defects were fixed in this repo across three repair waves, and almost all of them came from someone
encoding a rule they had not read, or copying a test number they had not checked.

Read [KNOWN-ISSUES.md](../KNOWN-ISSUES.md) before you start. If the country is already listed there,
the reason it was left alone is written down, and you need to beat that reason, not rediscover it.

## 1. Find the rule, and cite it

A rule goes in with a source, in an XML doc comment on the method, as a URL. No source, no rule.

Good sources, in the order they are trusted here:

- **The issuing authority.** SUNAT's own description of the RUC structure is quoted verbatim in
  `PeruValidator`; `ArmeniaValidator` cites law HO-288-N article 4 and decision N 1783-N by their
  arlis.am URLs; `AzerbaijanValidator` cites e-gov.az for the PIN's character set.
- **The OECD TIN sheet for that country** (`oecd.org/.../aeoi/<country>-tin.pdf`). Useful for
  structure and length; often explicit that a check digit exists without giving the algorithm.
- **python-stdnum.** Link the module file, e.g.
  `https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/pe/ruc.py`. Its doctests are
  published, checked examples — the only numbers in this repo you may copy without computing them.

Bad sources: SEO tax-lookup sites. `lookuptax` appears three times in KNOWN-ISSUES.md, every time as
a reason **not** to act. Azerbaijan's VÖEN taxpayer-type digit is still unenforced because that
constraint rests on lookuptax alone; Georgia's personal number has no check-digit validation because
lookuptax, a second guide and tin-check.com each describe the trailing digits differently. One
secondary source that nothing corroborates is not enough to reject a live number with.

If the sources contradict each other, take the widest reading and say why in a comment — see
`BoliviaValidator`, where three sources disagree on the maximum NIT length and the code takes the
widest so the change cannot reject anything any source endorses.

## 2. Write the validator

One file: `Attest/CountriesValidators/<Country>Validator.cs`, namespace `Attest.Countries`, deriving
from `IdValidationAbstract`, with the country code set in the constructor:

```csharp
public class BelgiumValidator : IdValidationAbstract
{
    public BelgiumValidator()
    {
        CountryCode = nameof(Country.BE);
    }
```

`Attest/IdValidationAbstract.cs` declares five methods:

| Method | What it means |
|---|---|
| `ValidateNationalIdentity` | Identity number of a natural person (Rijksregisternummer, kennitala, DNI/CUI). **Virtual**, and its base implementation calls `ValidateIndividualTaxCode`. |
| `ValidateIndividualTaxCode` | Tax number of a natural person. |
| `ValidateEntity` | Registration number of a company or other legal entity. |
| `ValidateVAT` | VAT registration number. |
| `ValidatePostalCode` | Postal code. Not part of `IdentifierKind.Any`. |

The four abstract methods must be implemented. **`ValidateNationalIdentity` is the trap.** Leaving it
unoverridden is a decision, not a default:

- Armenia did not override it, so the 10-digit public services number fell through to the 8-digit TIN
  rule: every real PSN was rejected and every company TIN was accepted as a personal identity number.
- Guatemala's `ValidateIndividualTaxCode` threw `NotSupportedException`, so the inherited
  `ValidateNationalIdentity(anything)` threw too.

Override it when the country issues a separate identity number. When the fallback is genuinely
correct, say so in the doc comment with the source — Nigeria's `ValidateEntity` was a bare one-line
delegation with no explanation, which reads like an unfinished stub and invites the next contributor
to "fix" it by inventing a discriminator that does not exist.

### The contract

Every public `Validate*` method returns a `ValidationResult` for **any** input — null, empty,
whitespace, punctuation, garbage, Unicode digits. It never throws. 22 of 87 validators used to;
`Attest.Tests/UnicodeDigitSweepTests.cs` sweeps all 87 countries against all five methods plus
`Validate`, and will fail your PR if the new one throws.

The first line of every method is the sanitiser:

```csharp
id = id.RemoveSpecialCharacthers();
```

`IdExtensions.RemoveSpecialCharacthers` (`Attest/IdExtensions.cs`) is an extension method, so null in
gives `""` out, which every format check then rejects. It keeps letters of **any** script — a
Belarusian UNP is written with Cyrillic look-alikes that `BelarusValidator` maps itself — keeps ASCII
digits, drops punctuation, and replaces a decimal digit outside 0-9 with `U+FFFD`, a character no
format check in this library accepts. Do not bypass it, and do not delete characters to make input
parse: dropping a digit validates the rest and turns a wrong number into a right one.

Two consequences:

- `\d` and `char.IsDigit` match every Unicode decimal digit; `int.Parse` accepts only ASCII. After
  `RemoveSpecialCharacthers` the `U+FFFD` substitution makes `^\d{11}$` safe, which is why
  `BelgiumValidator` can use it. **Anywhere you guard unsanitised input, or use
  `All(char.IsDigit)`, write `[0-9]`.** That mismatch is the whole "digits outside ASCII" section of
  CHANGELOG.md, and KNOWN-ISSUES.md still lists it for ~70 validators.
- Strip country prefixes with an anchored check, not `String.Replace`. Norway's `ValidateVAT` used
  unanchored `Replace` for "NO"/"MVA"; Belarus still strips "УНП" anywhere in the string and so
  accepts input python-stdnum rejects.

Return the specific result: `ValidationResult.InvalidFormat("12345678901")`, `InvalidLength()`,
`InvalidDate()`, `InvalidChecksum()`, or `Invalid("...")` with a message that says why. Peru's
`Invalid("Invalid")` told a caller nothing about a well-formed RUC being rejected for holder type.

## 3. Register it

Two places, both in `Attest/`:

1. `Country.cs` — the ISO 3166-1 alpha-2 member, with a doc comment giving the country name. The
   members are in alphabetical order by code and carry **no explicit values** (only `XX = 0`), so
   inserting one mid-list renumbers every member after it. Adding a code that sorts last is free;
   anything else changes the integer value of existing members.
2. `CountryValidator.cs` — one line in the `Load()` dictionary, in the same alphabetical order:

```csharp
{ Country.BE, new BelgiumValidator() },
```

That is all. The facade's five `Validate*` overloads, `Validate(value, country, kinds)`, `Supports`,
`SupportedCountries` and the `Attest.DataAnnotations` attributes all go through that dictionary; none
of them needs touching. Add a row to the supported-countries table in README.md in the same PR.

## 4. Write the test file

`Attest.Tests/CountriesValidators/<Country>ValidatorTests.cs`, namespace `Attest.Tests`. Read
`BelgiumValidatorTests.cs` for the shape and `PeruValidatorTests.cs` for the current bar.

The shape: a `readonly` validator field built in the constructor, then one `[Theory]` per method with
`[InlineData(code, isValid)]` rows — `TestNationalId`, `TestIndividualCode`, `TestCorrectEntityCode`,
`TestCorrectVatCode`, `TestPostalCode` — each asserting `Assert.Equal(isValid, ....IsValid)`.

Every theory carries the crash rows:

```csharp
[InlineData("abc", false)]
[InlineData("", false)]
[InlineData(null, false)]
```

73 of the 88 files in `Attest.Tests/CountriesValidators` already have them. `UnicodeDigitSweepTests`
covers the same ground centrally, but the per-country rows are what fails first and points at the
right file.

Every non-obvious row gets a comment saying what it is, in the style of Belgium's:

```csharp
[InlineData("85071501709", true)]     // Check number below 10, written as "09"
[InlineData("85.07.15-017.09", true)] // Same number as printed on the id card
[InlineData("90462200196", true)]     // BIS number for a non resident, month 06 + 40
```

and Peru's, where the source URL sits above the block it justifies.

### Test numbers are computed, never copied

This is the rule with the most scar tissue behind it. Thailand's tax-code tests used a Swedish
personnummer — the `+` separator is uniquely Swedish and nobody noticed for years. Cyprus's only
national-identity test was a Czech rodné číslo copied out of the Czech file. Both suites were green
and certified nothing.

A `true` row must be either a number whose check digit **you computed**, or a published example from
an authoritative source, cited in a comment next to it.

Worked example, Belgium. The rule is mod-97: the check number is `97 - (first nine digits mod 97)`,
written as the last **two** digits.

```
12060105317
  first nine   120601053
  mod 97       80
  97 - 80      17         -> printed check "17"   valid
```

Now the case that started this project:

```
85071501709
  first nine   850715017
  mod 97       88
  97 - 88      9          -> printed check "09"   valid
```

The old code compared the check as text, so a computed `1` never matched a printed `01`, and roughly
9% of all Belgian national numbers were rejected. `BelgiumValidator` now does
`int.Parse(id.Substring(id.Length - 2))` and the comment above it says why. For people born from 2000
on, a `2` is prefixed to the nine digits before the modulo — `05030900178` fails the first pass and
passes as `2050309001 mod 97 = 19`, `97 - 19 = 78`.

Include at least one `false` row that is the same number with the check digit changed by one
(`85071501708`). A suite of well-formed valid numbers proves the format check works and says nothing
about the checksum.

## 5. Never assert behaviour you believe is wrong

If the validator is wrong and you cannot fix it from a source, **leave the code, skip the test, and
add the finding to KNOWN-ISSUES.md**. A green test that locks in a bug is worse than no test: it tells
the next person the behaviour was intended.

What that failure looks like in this repo: Russia's tests asserted that Sberbank's and Gazprom's
company numbers were valid *personal* tax codes. Andorra had four rows asserting that a parapublic
entity and a public body were valid natural-person identifiers. Peru had three. Iceland had
`TestEntity("1207742209", true)` — a person's kennitala asserted valid as a company identifier. Every
one of those rows was written to make a suite green.

KNOWN-ISSUES entries are a bullet stating the gap and a `*Not fixed:*` line stating what you looked at
and why you stopped:

```markdown
## Armenia

- The 8th digit of the TIN is a check digit, but only the length is validated.
  - *Not fixed:* The SRC's OECD sheet states the check digit exists and is calculated from the first
    seven digits but gives no algorithm, and nothing published by the SRC, python-stdnum (no stdnum/am
    module) or any secondary guide supplies one. Inventing one would reject live numbers.
```

## 6. When there is no published check digit

Implement the format and the field ranges. **Do not invent a checksum.** A guessed algorithm rejects
real numbers, which is the worst failure this library has — it is invisible until someone's customer
cannot register.

Armenia is the pattern: the OECD sheet says the TIN's 8th digit is a check digit and gives no
algorithm; law HO-288-N and decision N 1783-N both say the public services number's specifying digit
is calculated by a procedure the authorised body defines and does not publish. So `ArmeniaValidator`
validates length, the day offset (11-41 men, 51-81 women), the century-shifted month, the serial range
and the "no 666" rule from article 4 part 3 — and the check digit not at all, with the reason in the
doc comment. Azerbaijan is the same: `ValidateEntity` is a 10-digit format check because the VÖEN's
9th-digit algorithm is unpublished and the 10th-digit taxpayer type has only lookuptax behind it.

Then add the gap to KNOWN-ISSUES.md, so the next person does not spend the afternoon you just spent.

## 7. If the number encodes holder type

When a country's number says whether its holder is a person or a company, the personal methods and the
business methods **must reject each other's numbers**. Four countries this library reported as
indistinguishable turned out to be bugs, all from a one-line `ValidateEntity` delegating to
`ValidateIndividualTaxCode`:

- **Peru** — RUC prefix 10 and 15 are natural persons, 20 is a legal entity.
- **Andorra** — the NRT's leading letter is the holder type, documented by the tax authority.
- **Iceland** — an organisation's kennitala adds 40 to the day field, so the ranges cannot overlap.
- **Thailand** — digits 1-3 are the issuing agency and digit 4 the taxpayer type.
- **Russia** — 10 digits for a company, 12 for a person, with different check digits. Both algorithms
  were present, behind one method that accepted either.

Pin it with a kind test, the way `PeruValidatorTests` does:

```csharp
[InlineData("10054148289", IdentifierKind.PersonalTaxCode)]
[InlineData("20512333797", IdentifierKind.CompanyNumber | IdentifierKind.Vat)]
public void TestKindIsUnambiguous(string code, IdentifierKind expectedMatch)
{
    IdentifierResult result = new CountryValidator().Validate(code, Country.PE);

    Assert.Equal(expectedMatch, result.Matched);
    Assert.False(result.IsAmbiguous);
}
```

Ambiguity is reported, not guessed. Where a country really does issue one number for both roles —
Armenia and Nigeria — both methods accept it, `IdentifierResult.IsAmbiguous` says so, and the doc
comment carries the source that says the number has no holder-type field ("TIN consists of 8 digits
... No meaning is given to the numbers", Armenia's OECD sheet). If you make a country unambiguous,
update the country list in the `IsAmbiguous` doc comment in `Attest/IdentifierResult.cs`.

## Before you open the PR

- [ ] Every rule in the validator has a source URL in a doc comment.
- [ ] Every `true` test number was computed by you, or is a cited published example.
- [ ] No test asserts behaviour you think is wrong; anything you could not fix is in KNOWN-ISSUES.md.
- [ ] Each theory has `null`, `""` and a garbage row, and one wrong-check-digit row.
- [ ] `RemoveSpecialCharacthers` is the first line of every method; `[0-9]` anywhere a parse follows
      an unsanitised guard.
- [ ] Registered in `Country.cs` and in `CountryValidator.Load()`, plus a README table row.
- [ ] The commit body names the country, what was wrong and what the caller-visible symptom was.
      `CHANGELOG.md` is written by the maintainer at release time -- do not edit it.
