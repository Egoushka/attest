# Changelog

## 1.0.0 (unreleased)

First release under the name Attest, descended from CountryValidator 1.1.3.



### Added

- `CountryValidator.Validate(value, country, IdentifierKind kinds)` validates against a category of
  identifiers rather than one named method, so callers can ask for personal identifiers only,
  business identifiers only, or any of them, without knowing which of a country's five methods
  applies. Returns an `IdentifierResult` carrying every kind the value matched, the per-kind
  results, and `IsAmbiguous` — true where a country issues one number that is both a personal and a
  business identifier, which is the case in seven of the countries here.
- `CountryValidator.Supports(country, kind)` reports whether a country has a rule for a kind at all,
  which a caller cannot otherwise distinguish from a value being wrong. 33 of the 435 country/kind
  pairs have no rule.

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

### Fixed in the second repair wave

Covers the defects the audit left on the list, plus the 38 validators that had never had a single
test. 80 further defects were fixed and 1911 test cases added, taking the suite from 1111 to 3010.
Writing the first tests for a validator is what surfaced most of these — every country below had
zero coverage before, unless it appears in the first wave as well.

- **Argentina**
  - ValidatePostalCode regex "^\\d{4}|[A-Za-z]\\d{4}[a-zA-Z]{3}$" is a top-level alternation with no group, so the anchors bind to one branch each: any string STARTING with 4 digits was accepted regardless of what…
  - ValidateCuit guarded with @"^\d{11}$".
  - ValidateCuit accepted any two-digit prefix.
  - ValidateNationalIdentity required exactly 8 digits and additionally rejected anything below 10000000, so every 7-digit DNI (numbers issued below 10 million, still in circulation) was a false negative.…
- **Armenia**
  - The InvalidFormat message advertised "123456789" (nine digits) for a TIN the same method requires to be eight digits.
- **Azerbaijan**
  - ValidateNationalIdentity used ^\w{7}$, and .NET's \w matches letters of any script, so "ЖЖЖЖЖЖЖ" passed as a PIN.
- **Belarus**
  - ValidateEntity gated on Regex "^[AaBbCcEeHhKkMmOoPpTt]{2}", requiring two leading letters.
  - ValidateVAT carried the same two-leading-letters guard, so it rejected every numeric UNP.
  - ValidateIndividualTaxCode was the mirror image: `!number.Substring(0, 2).All(char.IsDigit)` rejected the two-letter form, which is exactly the form issued to individuals.
  - All three UNP methods threw NullReferenceException on null input: `id?.Replace(...)` left id null, then the `id.Translit()` extension dereferenced it inside its body.
  - ValidateUNP never checked that the first two characters were consistently digits or consistently letters, nor that a letter in position 1 was one of ABCEHKMOPT. "A11953684" and "AD1953684" reached…
  - Cyrillic input was run through IdExtensions.Translit(), which transliterates (В->V, Н->N, Р->R, С->S) instead of mapping to the Latin look-alikes the UNP actually uses (B, H, P, C).
- **Bolivia**
  - ValidateNationalIdentity called Regex.IsMatch(ssn, ...) with no guard, so null input threw ArgumentNullException instead of returning a ValidationResult.
- **Brazil**
  - ValidateEntity/ValidateVAT (CNPJ) guarded with @"^\d{14}$"; .NET \d matches non-ASCII Unicode digits (e.g.
  - CNPJ "00000000000000" has arithmetically correct check digits and was reported valid.
  - CPF "00000000000" passes both check digits and was reported valid.
  - ValidatePostalCode used "^\\d{8}$", so a CEP written in Arabic-Indic digits was reported valid.
- **Chile**
  - A lower case "k" check digit (remainder 10) was rejected: CalculateChecksum returns upper case 'K' and the input was never upper-cased, so "12000008k" failed the checksum.
  - Replace("CL", "") stripped "CL" from anywhere in the string, so "76086CL4285" was accepted as the valid RUT 76086428-5.
  - Body digit check used char.IsDigit, which accepts non-ASCII Unicode digits; postal code used "^\\d{7}$" with the same leniency.
- **Colombia**
  - A NIT of the wrong length returned ValidationResult.InvalidChecksum() — the wrong failure reason for a length failure.
  - Replace("CO", "") stripped "CO" from anywhere, so "213CO1234321" was accepted as the valid NIT 2131234321.
  - char.IsDigit accepted non-ASCII Unicode digits; postal code used "^\\d{6}$".
- **CostaRica**
  - ValidateCPF accepted any 10-digit number.
- **Cuba**
  - ValidateIndividualTaxCode dereferenced number.Length with no guard, so null threw NullReferenceException.
- **Cyprus**
  - National-ID test data was the Czech rodne cislo 7103192745, copied verbatim from CzechValidatorTests.cs (grep confirms the same literal in CzechValidatorTests.cs and SlovakiaValidatorTests.cs).
  - The TIC/VAT format guard ^([0-59]\d{7}[A-Z])$ restricts the first digit to 0-5 or 9, rejecting 6, 7 and 8.
  - The reserved '12' prefix was not rejected. python-stdnum raises InvalidComponent for any CY number beginning '12'.
  - Input was never upper-cased, although the code already tried to handle lowercase input by calling .Replace("cy", ...).
  - ValidateVAT duplicated ValidateEntity's regex and CY-stripping, then delegated to ValidateEntity anyway, so the format rule lived in two places and could drift.
- **Dominican-republic**
  - ValidateNCF: the document-type test for the 11-character B-series receipt was inverted relative to its two sibling branches — `else if (_ncf_document_types.Contains(number.Substring(1, 2)))` rejected every…
  - _ecf_document_types was missing the codes "46" (comprobante electrónico para exportaciones) and "47" (comprobante electrónico para pagos al exterior), so valid 13-character e-CF receipts of those two types…
- **Ecuador**
  - In the public-RUC branch of ValidateEntity the establishment guard read `ruc.Substring(ruc.Length - 4) == "000"` — a 4-character slice compared to a 3-character literal, so it could never be true.
- **Germany**
  - ValidateEntity accepted any 10- or 11-digit numeric string and never verified the Pruefziffer it captured.
  - The 10/11-digit Bescheid form was validated by unanchored per-Land regexes that could not reject anything.
  - Three permanently dead regexes.
  - TestCorrectEntityCode was three rows that all expected true, with no negative row, so it certified nothing -- the theory would still pass if ValidateEntity returned Success for every 10/11/13-digit input.
  - ValidateEntity had no explicit guard for empty input.
- **Guatemala**
  - ValidateVAT was a bare `^\d{8}$` format check while the method's own doc comment says NIT.
- **Hungary**
  - ValidateNationalIdentity computed the szemelyi azonosito check digit with weights 1..10 for every input.
- **Indonesia**
  - NPWP Luhn check computed over the wrong window: ValidateEntity and ValidateIndividualTaxCode both called ssn.Substring(0, 10).CheckLuhnDigit(), treating digit 10 (first digit of the tax-office code) as the…
  - ValidateEntity rejected the wrong taxpayer-type digit with the message "Second digit must be between 4-9" — that is the individual rule; the entity branch requires 0-3.
- **Israel**
  - ValidateIndividualTaxCode accepted non-digit input. char.GetNumericValue returns -1 for a letter, so a letter contributed -1 or -2 to the running total instead of being rejected; "23456789a" summed to exactly…
  - ValidateEntity/ValidateVAT accepted strings that are not company numbers: inputs shorter than 9 were zero-padded (so "59" became 000000059 and validated), and the prefix regex ^0*5\d+$ let leading zeros stand…
- **Japan**
  - ValidatePostalCode's InvalidFormat hint read "NNNNNNN or NNN-NNNNN" — eight N in the grouped form for a seven-digit code.
- **Kazakhstan**
  - Unanchored regex in ValidateEntity (line 20) and ValidateIndividualTaxCode (line 36): @"\d{12}$" checks "ends with 12 digits", not "is exactly 12 digits".
  - No checksum at all.
  - No test file existed for Kazakhstan.
- **Korea**
  - new DateTime(DateTime.Now.Year - 17, DateTime.Now.Month, DateTime.Now.Day) throws ArgumentOutOfRangeException whenever the call happens on 29 February and the year 17 years earlier is not a leap year (e.g.…
- **Macedonia**
  - ValidateVAT (and ValidateEntity, which delegates to it) accepted any 13 digits with no check-digit verification.
- **Malta**
  - ValidatePostalCode accepted 2 and 3 digit postcodes.
  - ValidatePostalCode was case-sensitive while its normalisation line (RemoveSpecialCharacthers, no ToUpper) never uppercased, unlike the other three methods in the same file.
  - ValidateVAT stripped the country prefix with .Replace("mt").Replace("MT") without uppercasing first, so mixed-case input was rejected.
  - ValidateVAT's Replace("MT", "") removed 'MT' anywhere in the string, not just as a prefix.
  - ValidateIndividualTaxCode's regex ^\d{7}[1-9MGAPLHBZ]$ mixed digits into the id-card suffix class, so any 8-digit number ending 1-9 was accepted as a TIN (e.g. '12345678'), while '12345670' was rejected - an…
  - ValidateEntity required 8 digits (left-padding shorter input with zeros).
- **Mauritius**
  - MauritiusValidator.CalculateChecksum valued the leading character with char.GetNumericValue, which returns -1 for letters.
- **Moldova**
  - ValidateVAT rejected the documented optional MD prefix: RemoveSpecialCharacthers keeps letters, so "MD9234564" reached the ^\d{7}$ regex intact and failed.
- **Monaco**
  - ValidateVAT threw ArgumentOutOfRangeException for null, "", "abc" or any input shorter than 5 characters: `number.Substring(2, 3)` ran before any length guard.
- **Norway**
  - ValidateVAT (and ValidateEntity, which delegates to it) threw ArgumentNullException on null input: `vatId?.RemoveSpecialCharacthers()` propagated null through the whole ?. chain and Regex.IsMatch(null, ...)…
- **Pakistan**
  - ValidateIndividualTaxCode called id.Trim() and matched "^[1-7][0-9]{4}-[0-9]{7}-[1-9]{1}$".
- **Peru**
  - ValidateNationalIdentity rejected a lower case check letter: CalculateChecksumNationalIdentity produces upper case (e.g. "2G") and the input was never upper-cased, so "10117410g" failed.
  - char.IsDigit accepted non-ASCII Unicode digits in both the CUI body and the RUC; postal code used "^\\d{5}$".
- **Poland**
  - ValidateVAT (NIP) mapped a mod-11 remainder of 10 to check digit 0, accepting numbers the NIP spec declares unissuable.
  - ValidateIndividualTaxCode (PESEL) dereferenced `pesel.Length` with no null guard, so ValidateIndividualTaxCode(null) and ValidateNationalIdentity(null) threw NullReferenceException instead of returning a…
- **Portugal**
  - ValidateIndividualTaxCode discriminated with Regex.IsMatch(code, "^[5]") only, so every non-5 range validated as an individual NIF.
  - ValidateEntity discriminated with Regex.IsMatch(id, "^[123]") only, the mirror image of the same bug: individual ranges 45 and 8, plus the never-issued 0 and 4x ranges, validated as company NIFs. python3…
  - ValidateVAT carried a third copy of the entity guard (^[123] -> "This is not a company nif"), rejecting every Portuguese sole trader's and individual's VAT number.
  - The same 20-line format-guard + mod-11 checksum block was duplicated verbatim in ValidateEntity, ValidateIndividualTaxCode and ValidateVAT, which is how the three guards drifted apart in the first place.
- **Russia**
  - ValidateEntity measured the raw string length (id?.Length != 10) before stripping separators, so a formatted INN such as "7707 083 893" was rejected with "Invalid length" while every other method in the class…
- **Slovenia**
  - ValidateVAT (DDV) collapsed both 10 and 11 to check digit 0.
- **Taiwan**
  - The public helpers ValidateLocalSSN and ValidateResidentSSN did no format check of their own — they were only safe when reached through ValidateIndividualTaxCode.
- **Turkey**
  - ValidateIndividualTaxCode threw on null (Enumerable.All on a null source -> ArgumentNullException) and on "" (`"".All(char.IsDigit)` is vacuously true, so execution reached `kimlik[0]` ->…
  - ValidateVAT (and ValidateEntity, which delegates to it) threw ArgumentNullException on null: `vatId?.RemoveSpecialCharacthers().ToUpper().Replace(...)` short-circuits the whole chain to null, and…
- **Ukraine**
  - ValidateIndividualTaxCode required 12 digits and validated no check digit.
  - ValidateEntity just called ValidateVAT (12 digits), so the ЄДРПОУ registry code that identifies every Ukrainian legal entity (8 digits, e.g.
- **Uruguay**
  - ValidateVAT guarded digits with rut.All(char.IsDigit), which passes for Arabic-Indic digits, and then called int.Parse(rut.Substring(0, 2)) — FormatException escapes the validator for a 12-character…
  - Replace("UY", "") stripped "UY" from anywhere in the string, letting a 14-character string collapse into a 12-digit RUT.
  - ValidatePostalCode used "^\\d{5}$".
- **VenezuelaAfrica**
  - ValidatePostalCode ran RemoveSpecialCharacthers (which deletes the separator) and then matched `^\d{4}(\s[a-zA-Z]{1})?$`, which demands a whitespace character before the optional letter.

A further 61 weaknesses were found and deliberately left alone, because the fix would have been a
rewrite or the published rule could not be sourced with confidence. They are written up in
[KNOWN-ISSUES.md](KNOWN-ISSUES.md) rather than half-fixed here. The largest recurring one: .NET's `\d` matches any
Unicode decimal digit while `int.Parse` accepts only ASCII, so a validator that guards with `\d` and
then parses will throw on Arabic-Indic or fullwidth digits. Argentina and Brazil are fixed; the
pattern still exists elsewhere.

### Changed

- Target frameworks are `netstandard2.0` and `net8.0`. `netstandard2.1` and `net48` were dropped.
- Tests run on `net9.0` with xunit 2.9.2.
- Renamed to Attest. Package ids are `Attest` and `Attest.DataAnnotations`, the root namespace is
  `Attest` (was `CountryValidation`), and the country validators live in `Attest.Countries`.
  Migrating from CountryValidator is a package swap plus a `using` change.
- Dropped the upstream package icon, which is the original project's branding.
