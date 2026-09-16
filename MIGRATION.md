# Migrating from CountryValidator

CountryValidator 1.1.3 shipped in November 2021 and has had no release since. Three repair waves
have fixed 197 defects in it. Before the mechanics, what you are running today:

- **Hungary rejected every valid tax id.** Not most. All 36,363,636 of them. The checksum summed
  UTF-16 code units instead of digit values, adding 48 x (1+...+9) = 2160, which is 4 mod 11, so the
  check digit it computed was always the correct one plus 4 and could never match.
- **Mexico and South Africa could not validate anything either.** Both date helpers returned false on
  the success path, so every RFC and every South African ID was rejected.
- **Thailand, San Marino and Malaysia accepted almost anything**, the empty string included, and
  rejected the real formats. If you stored what those validators approved, it is not what you think.
- **22 of the 87 validators threw** rather than returning a result — on `null`, on short input, on a
  letter where a digit belonged.
- **Every bug report still open upstream is answered here**, each asserted with its reporter's own
  value in [UpstreamReportTests.cs](Attest.Tests/UpstreamReportTests.cs).

The test suite went from 586 cases to 4,212. What is still weak is written down rather than hidden,
in [KNOWN-ISSUES.md](KNOWN-ISSUES.md).

## The swap

1. `dotnet remove package CountryValidator` and `dotnet add package Attest` (likewise
   `CountryValidator.DataAnnotations` to `Attest.DataAnnotations`).
2. Find and replace `CountryValidation` with `Attest` across the solution. Class and method names are
   unchanged, so nothing else moves.
3. Delete the `try`/`catch` blocks you wrapped around validator calls. Nothing throws now, so they
   are unreachable.

That is the whole mechanical change, and for most callers it is the whole change. **What it does not
cover is that a repaired validator gives a different answer.** Numbers your users could not enter
will start working, and numbers you accepted and stored may stop validating. If you store what you
validated, read [Before you ship](#before-you-ship) below; if you only validate at the edge of a
form, you are done.

This page is about what changes. [CHANGELOG.md](CHANGELOG.md) is the exhaustive record, entry by
entry, with the source for each rule.

The baseline is tagged, so you can always see the whole difference yourself:

```
git diff -M upstream-1.1.3..v1.1.0 -- \
  CountryValidator/CountriesValidators/NetherlandsValidator.cs \
  Attest/CountriesValidators/NetherlandsValidator.cs
```

The fork renamed the source directory, so both paths are needed and `-M` is what pairs them up.

## The package and namespace names

| CountryValidator 1.1.3 | Attest |
|---|---|
| `CountryValidator` package | `Attest` |
| `CountryValidator.DataAnnotations` package | `Attest.DataAnnotations` |
| `namespace CountryValidation` | `namespace Attest` |
| `namespace CountryValidation.Countries` | `namespace Attest.Countries` |
| `netstandard2.0`, `netstandard2.1`, `net48` | `netstandard2.0`, `net8.0` |

Two things behave differently and will not show up as compile errors:

- **`IdValidationAbstract.CountryCode` was `static`.** All 87 validator constructors wrote to it, so
  it held whichever validator was constructed last — process-wide shared mutable state that read
  `"ZA"` once the facade had loaded. It is an instance property now. If you read it, you were
  reading a bug.
- **`[CompanyTIN]` never ran.** Its constructor checked its own unassigned property and threw every
  time, and `Validator.TryValidateObject` silently drops an attribute that throws, so a property
  annotated with it validated unconditionally. It works now, which means properties you thought were
  being validated start being validated for the first time.

## Before you ship

Two sets of verdicts move. The first costs you nothing; the second is the one to check
against what you have stored.

### Numbers that were rejected and now pass

Nothing to do here — these are users who could not get through your forms before.

| Country | What was wrong |
|---|---|
| Hungary | The tax-code checksum summed UTF-16 code units instead of digit values, adding a constant 2160, which is 4 mod 11. It accepted **none** of the 36,363,636 valid numbers, and provably could not. Personal IDs for October births were rejected by a regex missing month `10`, and for anyone born from 1997 by the reversed check weights |
| Belgium | Check numbers `01`–`09` were compared as text, so a computed `1` never matched `01`. About 9% of all Belgian numbers |
| Mexico | `HasValidDate` read the date at the wrong offsets and always threw internally, so **every** RFC was rejected |
| South Africa | Its date helper returned false on the success path too, so **every** ID was rejected |
| Belarus | Both company methods demanded two leading letters. No UNP has two leading letters, so every genuine one was rejected |
| Canada | The SIN format guard was inverted: every correctly formed SIN rejected, only malformed input reached the checksum |
| Malaysia | Format tests inverted, and the patterns predated the 2023 reform |
| Ireland | The mod-23 checksum mapped 0 to `@` instead of `W`. The same number validated as a VAT number and failed as a PPS number |
| Iceland, Lithuania, Latvia | mod-11 normalisation errors, each rejecting roughly 1 in 11 valid numbers |
| Czechia, Slovakia | `DaysInMonth(year, month - 1)` threw on January births and mis-bounded every other month |
| Netherlands | The post-2020 btw-identificatienummer for sole proprietorships was rejected; the government's own example failed |
| Finland | The HETU century separators added by decree in 2023 (`Y X W V U`, `B C D E F`) were not accepted |
| Indonesia | Only the legacy 15-digit NPWP was supported, not the 16-digit form in use since 2024 |
| Korea | Any RRN whose birth date was under 17 years ago was rejected. Korean RRNs are issued at birth |
| Hong Kong | Lowercase HKIDs were mishandled |
| Argentina | Seven-digit DNIs, which are still in circulation, were rejected |

### Numbers that were accepted and now fail

**Read this section before upgrading.** If you stored numbers that only passed because of a defect,
re-validating them will now flag them. The worst cases are validators that accepted nearly anything.

| Country | What was wrong |
|---|---|
| Thailand | `ValidateIndividualTaxCode` returned **success for any input that was not exactly ten digits** — including the empty string — and rejected the ten-digit numbers it did recognise |
| San Marino | Same shape: any string that was not exactly 9 digits was reported valid |
| Malaysia | All four public methods returned success for any input, including `""` |
| India | An empty string had a correct Verhoeff checksum and validated as an Aadhaar number |
| Kazakhstan | No checksum was verified at all: any 12 digits passed |
| Germany | Any 10- or 11-digit string was a valid Steuernummer; the check digit it captured was never verified |
| Macedonia | Any 13 digits passed as a VAT number |
| Hong Kong | The format guard passed the pattern as the input to `Regex.IsMatch`, so it was dead and matched everything |
| New Zealand | The length guard was a tautology that never fired |
| Poland, Slovenia | Numbers their specifications declare unissuable were accepted (a mod-11 remainder of 10 rewritten to 0) |
| Portugal | Individuals and companies were told apart by a single leading digit, so company NIFs validated as personal ones and the reverse |
| Russia | A 10-digit company INN validated as a personal tax code. The two are different numbers with different check digits |
| Peru, Andorra, Iceland, Thailand | The holder type is encoded in the number — a type prefix, a type letter, +40 on the day field — and the company and personal methods now reject each other's numbers |
| Brazil | `00000000000` and other all-same-digit CPFs satisfy the check digits arithmetically and were accepted |
| United Kingdom, Argentina, Ireland, Kazakhstan | Unanchored regex alternation meant any string *containing* a valid code passed. `"NOTAPOSTCODE SW1A1AA"` was a valid UK postcode |
| Bolivia | `^\d{10,}$` had no upper bound, so a 30-digit string passed |
| Cyprus | The TIC format allowed only some leading digits and did not reject the reserved `12` prefix |
| All | A decimal digit outside ASCII 0-9 (Arabic-Indic, Devanagari, fullwidth) either threw or slipped through. `.NET`'s `\d` matches them; `int.Parse` does not. They are now rejected |

## Methods that no longer throw

22 of 87 validators threw instead of returning a `ValidationResult` — on `null`, on short input, on a
letter where a digit belonged. Croatia threw `ArgumentNullException` by explicit design. That is
fixed everywhere, and a sweep across all 87 countries and all five methods now enforces it, so:

**Any `try`/`catch` you wrapped around these calls is now dead code.** Check `IsValid` instead.

Eleven validators used to throw `NotSupportedException` for a kind their country has no rule for,
which escaped if you called the validator class directly rather than through `CountryValidator`.
Since 1.1.0 nothing throws: those kinds are declared, and both the facade and the validator class
return `Invalid("Not supported")`.

## What you gain

- `Validate(value, country, IdentifierKind)` validates against a category — `Person`, `Business`,
  `Any` — instead of you picking one of five methods. `IdentifierResult.IsAmbiguous` tells you when
  a country issues one number for both roles, which Armenia and Nigeria genuinely do.
- `Supports(country, kind)` distinguishes "this country has no rule for this" from "this value is
  wrong". 25 of the 435 country/kind pairs have no rule.
- [KNOWN-ISSUES.md](KNOWN-ISSUES.md): 33 entries across 26 countries, each with the reason. 19 are
  still-open gaps, almost all of them check digits no authority publishes; 14 were investigated and
  closed.

## Suggested upgrade path

1. Swap the package and the namespace, and delete the `try`/`catch` blocks that are now unreachable.
2. Run your existing validation over the identifiers you have already stored, with both versions,
   and diff the verdicts. The set that changes is small and knowable, and much better found by a
   query than by a support ticket.
3. Decide what to do about the numbers in the "now fail" column before you ship, not after.
