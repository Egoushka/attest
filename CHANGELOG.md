# Changelog

## 1.0.0 (unreleased)

First release under the name Attest, descended from CountryValidator 1.1.3.



### Fixed

- **Belgium** — national register numbers whose check number is below 10 were rejected. The check
  number was compared as text, so a computed `1` never matched the `01` printed in the number.
  Roughly 9% of all Belgian numbers were affected. Upstream issues
  [#10](https://github.com/anghelvalentin/CountryValidator/issues/10) and
  [#22](https://github.com/anghelvalentin/CountryValidator/issues/22).
- **Thailand** — `ValidateIndividualTaxCode` returned success for any input that was not exactly ten
  digits, and rejected the ten digit numbers it did recognise. Thai tax identification numbers are
  13 digits since 1 February 2012, so it now validates the 13 digit number and its check digit.
  Test data for the individual, entity and VAT cases was Swedish, and has been replaced.
- **Thailand** — `ValidateNationalIdentity` accepted 13 characters of any kind; it now requires digits.

### Fixed in the first repair wave

An audit of all 87 validators found that 22 of them threw instead of returning a result, and that
several could never return a correct verdict at all. 52 defects were fixed across 22 countries and
the core types, and 529 test cases were added (586 tests before, 1111 after). Every fix was
reproduced against the published rule for that country before being changed.

- **Bulgaria**
  - ValidateEntity(null) threw NullReferenceException at `vat.Length` (BulgariaValidator.cs:20) because ValidateEntity was the only public method that never sanitized its input.
  - BgForeignerPhysicalPerson threw IndexOutOfRangeException for any input shorter than 10 characters: IdExtensions.Sum bounds its loop on multipliers.Length (9) rather than input.Length, then line 137 reads vat[9].
  - BgMiscellaneousVatNumber had the identical unguarded shape (9 multipliers plus vat[9] at line 156) and was the second half of the `||` on the same two crash paths.
- **Canada**
  - ValidateIndividualTaxCode (SIN) had its format guard inverted: `if (Regex.IsMatch(sin, @"^\d{9}$")) return ValidationResult.InvalidFormat(...)`.
- **Czechia and Slovakia**
  - IsDayAndMonthValid called DateTime.DaysInMonth(year, month - 1) with a 1-based month.
  - ValidateIndividualTaxCode threw an unhandled FormatException on 10-character alphanumeric garbage.
  - CzechValidator.ValidatePostalCode accepted a 4-digit code (no such Czech PSC), rejected punctuated correct codes because it normalised with Trim() instead of RemoveSpecialCharacthers(), and threw NullReferenceException on null…
- **Finland**
  - HETU century separator regex accepted only '-', '+' and 'A'.
  - ValidateIndividualTaxCode(null) threw ArgumentNullException from Regex.IsMatch instead of returning a ValidationResult.
  - Y-tunnus (ValidateVAT/ValidateEntity) treated a weighted-sum remainder of 1 as check digit 0.
- **France**
  - ValidateVAT never verified the 2-character TVA key: for any number whose SIREN does not start with "000" (i.e. everything except Monaco), line 151 returned ValidateEntity(...) early, so the key was accepted unchecked as long as…
  - The two checksum branches were swapped: `else if (!number.All(char.IsDigit))` guarded the all-numeric formula `int.Parse(number[0..2]) == long.Parse(number[2..] + "12") % 97`, while the alphanumeric formula sat in the `else`.
  - In the alphanumeric branch the mod-11 test `(long.Parse(number[2..]) + 1 + check / 11) % 11 != (check % 11)` was nested inside the letter-first sub-branch only, so a new-style number starting with a digit (e.g. 4Z123456782)…
  - ValidateVAT indexed number[0] and called number.Substring(2) before any length check, so ValidateVAT("") and ValidateVAT("FR") threw IndexOutOfRangeException / ArgumentOutOfRangeException.
  - Only the first key character was checked against the alphabet; the second was unvalidated, so a key containing the excluded letters I or O in second position (e.g.
- **Hong Kong**
  - Regex.IsMatch arguments swapped at HongKongValidator.cs:32 — the pattern text was passed as the input and string.Empty as the pattern, so IsMatch always returned true, the negation was always false, and the format branch was…
  - Lowercase HKIDs were mishandled. getLetterValue on line 19 computes `letter[0] - 55`, which yields 42 for 'a' instead of 10 for 'A', and line 40 compares the check character against the literal "A".
- **Hungary**
  - HungaryValidator.CheckSum summed `(int)value[i]` — the UTF-16 code unit (48 + digit) — instead of the digit value.
  - ValidateNationalIdentity's format regex had the month alternation `(0[1-9]|1[12])`, which matches 01-09, 11 and 12 but omits 10 — every személyi azonosító for an October birth was rejected as "Invalid format".
- **Iceland**
  - IcelandValidator.ValidateIndividualTaxCode computed the expected kennitala check digit as `11 - sum % 11` without reducing mod 11.
- **India**
  - ValidateNationalIdentity (Aadhaar) fed unguarded input into StringToReversedIntArray, whose int.Parse per character throws FormatException on any letter (upstream issue #24).
  - Empty string and null validated as a correct Aadhaar.
  - No leading-digit rule. '123412341234' has a correct Verhoeff check digit and was accepted, but Aadhaar numbers never start with 0 or 1.
- **Ireland**
  - ValidateEntity (line 48) and ValidateIndividualTaxCode (line 86) mapped the mod-23 checksum to a letter with `checksum + 64`, which yields '@' (ASCII 64) at index 0 instead of 'W'.
  - ValidateVAT's format regex `^(\d{7}[A-W])|([7-9][A-Z\*\+)]\d{5}[A-W])|(\d{7}[A-W][AH])$` is an unanchored three-way alternation: `^` binds only to branch 1, `$` only to branch 3, branch 2 has neither.
  - ValidateVAT threw ArgumentNullException on null input (Regex.IsMatch(null, ...)), violating the rule that every public Validate* must return a ValidationResult for any input.
- **Latvia**
  - ValidateIndividualTaxCode checksum: `checkSum = (1 - checkSum) % 11; checkSum += (checkSum < -1) ? 11 : 0;` uses C# truncating remainder on a negative dividend.
- **Lithuania**
  - 9-digit PVM (VAT) second pass assigned the raw weighted sum to checkDigit instead of sum % 11, so every VAT number needing the second pass (~1 in 11) was rejected as InvalidChecksum.
  - ValidateVAT (and therefore ValidateEntity, which delegates to it) threw ArgumentNullException on null input — Regex.IsMatch(null, ...) was the first statement.
- **Malaysia**
  - ValidateEntity and ValidateIndividualTaxCode had the format test inverted (`if (Regex.IsMatch(...)) return Invalid`), so every well-formed TIN was rejected and every garbage string — including null, "" and "I am not an ID" —…
  - Bare negation would have created a new false-reject class: the entity pattern `^(CS|D|E|F|FA|PT|TA|TC|TN|TR|TP|TJ|LE)\d{10}$` omits `C` (companies, the most common prefix) and `J`, contains `TJ` which is not an IRBM code, and…
  - The individual pattern `^(SG|OG)\d{10}[01]$` predates the 2023 reform: it lacks the current `IG` prefix entirely and forces exactly 11 digits ending in 0 or 1, which rejects two of the three officially published examples.
- **Mexico**
  - HasValidDate read the RFC date component at Substring(2,2)/(4,2)/(6,2), which is wrong for both of its two call sites, so it always threw internally and returned false.
  - The hand-rolled parse also used `new DateTime(1900 + year, ...)` inside a try/catch used as control flow. 1900 is not a leap year and 2000 is, so "000229" — a real RFC date, 29 Feb 2000 — is still wrongly rejected even with the…
  - Upstream issue #19 ("Unreachable code Mexico") points at ValidateEntity's `if (rfc.Length >= 12)` guard.
- **Netherlands**
  - ValidateVAT only implemented the legacy BSN-derived mod-11 btw-nummer, so post-2020 btw-identificatienummers issued to sole proprietorships were rejected (government example NL000099998B57 and python-stdnum's NL002455799B11 both…
  - `if (checkDigit > 9) checkDigit = 0;` rewrote mod-11 remainder 10 (which means "no valid check digit exists") to 0, creating false positives: 100000060B01 validated even though 100000060 is not a valid BSN and NL100000060B01…
  - ValidateNationalIdentity threw FormatException on "" and null and OverflowException on long digit strings, via ValidateOnderwijsnummer's `int.Parse(number) <= 0` (reached whenever ValidateIndividualTaxCode fails).
- **New Zealand**
  - The length guard `if (!(ird.Length != 8 || ird.Length != 9))` is a tautology that never fires (no integer is both 8 and 9), so the InvalidLength branch was dead code and all four public methods reached `long.Parse(ird)` unguarded.
- **Paraguay**
  - CalculateChecksum computed `-(s.Mod(11)).Mod(10)` — C# unary minus binds after the `.Mod(11)` call, so the negation was applied to the wrong side of the modulus.
  - ValidateVAT rejected any RUC shorter than 6 characters as InvalidLength.
- **San Marino**
  - ValidateIndividualTaxCode returned Success on the fallback branch when the input did NOT match the digit pattern (inverted branch).
- **South Africa**
  - HasValidDate returned false on the success path as well as in the catch, so no South African ID could ever validate — ValidateNationalIdentity always returned "Invalid date" and the Luhn check on line 35 was unreachable.
  - The two-digit year was passed straight to `new DateTime(year, month, day)`, so "80" meant year 80 AD.
- **Switzerland**
  - ValidateIndividualTaxCode (and ValidateNationalIdentity, which IdValidationAbstract delegates to it) had no format guard: GetCheckDigit indexed ssn[0..11] with Int32.Parse and line 50 then read id[12].
  - ValidateVAT ran a case-sensitive regex against un-uppercased input, so "che-107.787.577 iva" was returned as InvalidFormat while the identical "CHE-107.787.577 IVA" validated.
- **United Kingdom**
  - ValidateVAT indexed and parsed before any guard: ValidateVAT(null) -> NullReferenceException at vatId.Replace (the `?.` on the line above preserved null); ValidateVAT("") / ("A") -> ArgumentOutOfRangeException at…
  - Postcode regex was a top-level alternation with no enclosing group, so `^` bound only to the GIR branch and `$` only to the BFPO branch.
  - ValidateNHS(null) threw NullReferenceException on ssn.Length, because `ssn?.RemoveSpecialCharacthers()` kept the null.
- **Core types and DataAnnotations**
  - CompanyTINAttribute's constructor called CountryValidator.IsCountrySupported(CountryCode) — the not-yet-assigned auto-property, always default(Country) == Country.XX, the one enum member absent from CountryValidator.Load().
  - IdValidationAbstract.CountryCode was `public static string`, written by all 87 validator constructors.
  - The facade returned ValidationResult.Invalid("Not supported") for an unregistered country but let NotSupportedException/NotImplementedException escape for 35 of 435 (registered country, method) pairs — a supported country crashed…

### Changed

- Target frameworks are `netstandard2.0` and `net8.0`. `netstandard2.1` and `net48` were dropped.
- Tests run on `net9.0` with xunit 2.9.2.
- Renamed to Attest. Package ids are `Attest` and `Attest.DataAnnotations`, the root namespace is
  `Attest` (was `CountryValidation`), and the country validators live in `Attest.Countries`.
  Migrating from CountryValidator is a package swap plus a `using` change.
- Dropped the upstream package icon, which is the original project's branding.
