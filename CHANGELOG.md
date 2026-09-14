# Changelog

## 1.0.0 (unreleased)

First release under the name Attest, descended from CountryValidator 1.1.3.



### Added

- The git tag is now the single source of the version. `Directory.Build.props` holds the shared
  package metadata and a `1.0.0-dev` placeholder; the release workflow overrides it with the version
  read from the tag, so the two packages cannot drift apart or disagree with what was tagged. The
  workflow refuses to publish a version that `CHANGELOG.md` does not document, and uses that section
  as the release notes. Publishing authenticates through nuget.org trusted publishing, so there is no
  long-lived API key in the repository at all. Packages now carry the README, XML documentation, and symbol packages for
  debugging into them.


- Continuous integration: build, test and pack on every push and pull request, plus a release
  workflow that publishes to NuGet when a `v*` tag is pushed, authenticating through trusted
  publishing rather than a stored API key.

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

### Fixed: digits outside ASCII

.NET's regex digit class and `char.IsDigit` match every Unicode decimal digit, while `int.Parse`
accepts only ASCII. A validator that guarded with one and parsed with the other threw
`FormatException` on input no identifier scheme allows. The input sanitiser now keeps letters of any
script — a Belarusian UNP is written in Cyrillic — and replaces a decimal digit outside 0-9 with a
character that no format check accepts. Dropping such a digit instead would have been worse: it
would validate the remaining digits and turn a wrong number into a right one.

A sweep of all 87 countries against every public method found what was left, and fixed it: Estonia,
the United States, Poland and Bolivia validated unsanitised input; Croatia threw
`ArgumentNullException` from `ValidateVAT` by explicit design, against the library's own contract;
Greece, Sweden and the United Kingdom preserved null through `?.` before dereferencing it. The sweep
is now a test, so no country can regress into throwing on null, garbage or non-ASCII digits.

### Fixed in the third repair wave

Settles which countries genuinely cannot tell a personal identifier from a business one, and closes
the tractable half of the known-issues list. 65 defects fixed, 364 test cases added.

Four countries the library reported as indistinguishable turned out to be defects rather than facts:

- **Russia** issues a 10-digit ИНН to companies and a 12-digit one to people, with different check
  digits. Both algorithms were present, but behind one method that accepted either, so every company
  number also validated as a person's tax code. Four existing test rows asserted Sberbank's and
  Gazprom's numbers were valid personal tax codes.
- **Peru**'s RUC carries a holder-type prefix (10 and 15 natural persons, 20 legal entities).
- **Andorra**'s NRT carries a holder-type letter, which the Andorran tax authority documents openly.
- **Iceland**'s kennitala adds 40 to the day field for an organisation, so the ranges cannot overlap.
- **Thailand** encodes the issuing agency in digits 1-3 and the taxpayer type in digit 4.

Armenia and Nigeria really do issue one number for both roles, so `IsAmbiguous` still reports them,
as it does for a Russian sole trader filing VAT under his personal number.

- **Andorra**
  - ValidateEntity delegated to ValidateIndividualTaxCode, so every NRT validated as both a personal and a company identifier despite the leading letter encoding holder type
  - No lower bound on E numbers, so a non-resident legal entity's E number below 800000 validated as a natural person
  - Four existing test rows asserted the buggy behaviour: U-132950-X and D059888N (a parapublic entity and a public body) were asserted valid as ValidateNationalIdentity and ValidateIndividualTaxCode
- **Armenia**
  - ValidateNationalIdentity was not overridden, so IdentifierKind.PersonalId fell through to the 8-digit TIN: every real 10-digit public services number was rejected and every company TIN was accepted…
  - Nothing recorded, in code or tests, that the ValidateEntity -> ValidateIndividualTaxCode delegation is deliberate rather than an oversight.
- **Bolivia**
  - ValidateEntity used an unbounded `^\d{10,}$`: a 30-digit string passed and every 7-9 digit legacy NIT was rejected.
  - ValidateNationalIdentity's complemento pattern `\w?` accepted an underscore (and non-ASCII word characters).
- **Bosnia**
  - ValidateEntity threw NotSupportedException, so Bosnia had no company-number validation despite the JIB being a real 13-digit identifier.
  - ValidateVAT threw NotSupportedException.
- **Brazil**
  - All ten repeated-digit CPFs (00000000000 .. 99999999999) satisfy both mod-11 check digits; only the all-zero case was rejected, so 11111111111, 22222222222, ... were reported valid.
  - Alphanumeric CNPJ unsupported: the format guard was ^[0-9]{14}$ and DigitChecksum used int.Parse, so every CNPJ containing a letter returned InvalidFormat.
- **Cyprus**
  - ValidatePostalCode accepted "0000" — the Cypriot range is 1000-9999, allocated by district (Nicosia 1000-2999 … Kyrenia 9000-9999), so no code begins with 0.
- **Ecuador**
  - ValidateEntity missed stdnum's two RUC fallbacks: a third digit of 6 never fell back to natural-RUC validation when the public check sum failed, and a third digit of 9 never tried the public check…
  - ValidateCI rejected a third digit of 6 (`Char.GetNumericValue(number[2]) > 5`).
- **ElSalvador**
  - The documented "SV" country prefix was not stripped, so stdnum's own valid input "SV 0614-050707-104-8" was rejected as InvalidLength (RemoveSpecialCharacthers keeps letters).
- **FaroeIslands**
  - ValidateIndividualTaxCode checked only ^\d{9}$, so the DDMMYY part was never validated: "999999999", "000000000", "320785123" (day 32) and "151385123" (month 13) were all accepted as P-numbers.
  - ValidatePostalCode accepted any three digits, including "000" and "099".
- **Guatemala**
  - ValidateEntity's compaction lacked stdnum's `.upper()` and `.lstrip('0')`, so "576937-k" and "00576937K" were rejected.
  - ValidateIndividualTaxCode threw NotSupportedException, and the inherited IdValidationAbstract.ValidateNationalIdentity delegates to it, so ValidateNationalIdentity(anything) threw instead of…
- **Iceland**
  - ValidateEntity delegated verbatim to ValidateIndividualTaxCode, so (a) every real company kennitala was rejected — day 41-71 blew up DateTime(year, month, day) and returned InvalidDate — and (b)…
  - Test row TestEntity("1207742209", true) asserted that a person's kennitala is a valid company identifier.
- **Indonesia**
  - Only the legacy 15-digit NPWP was supported; the 2024 16-digit NPWP was rejected as InvalidFormat.
  - The taxpayer-type error message said "Second digit must be between 0-3", which is wrong for the 16-digit form where the type digit is the third character.
- **Kazakhstan**
  - KNOWN-ISSUES: the 7th digit (century/sex, documented values 0-6) is never validated.
  - Two BIN rows in KazahstanValidatorTests.cs carried comments that contradict each other and the published BIN structure: 120741000014 (5th digit 4) was labelled "Individual entrepreneur" and…
- **Korea**
  - ValidateIndividualTaxCode rejected any RRN whose birth date was less than 17 years ago, so every minor's RRN was reported InvalidDate.
  - The place-of-birth component (digits 8-9) was not checked, so 97-99 were accepted.
- **Macedonia**
  - ValidateIndividualTaxCode threw NotImplementedException; the earlier wave left it because it could not tell whether the method means the EDB or the EMBG.
  - ValidateVAT rejected the Cyrillic prefix form (МК4020990116747), which is one of python-stdnum's own doctests, and its ^\d{13}$ guard matched non-ASCII Unicode digits that char.GetNumericValue then…
- **Mauritius**
  - ValidateDate rejected only day > 31 and month > 12, so day 00, month 00, 31 February and 31 April all passed; the parsed year was assigned but never used (CS0219).
  - ValidateEntity threw NotImplementedException.
  - ValidateVAT threw NotImplementedException.
- **Moldova**
  - ValidateIndividualTaxCode (IDNP) and the inherited ValidateNationalIdentity checked only ^\d{13}$, so e.g. 9999999999999 passed.
- **Montenegro**
  - ValidateVAT threw NotImplementedException for every input including null.
  - ValidateEntity threw NotImplementedException.
  - ValidateIndividualTaxCode threw NotImplementedException.
- **Nigeria**
  - ValidateIndividualTaxCode (and, through delegation, ValidateEntity and ValidateVAT) accepted only the 10-digit JTB TIN.
  - ValidateEntity delegated to ValidateIndividualTaxCode as a bare one-liner with no explanation, which reads like an unfinished stub and invites a future wave to 'fix' it by inventing a discriminator.
  - ValidateVAT delegated to the TIN rule under a comment reading "JBT TIN" — a typo, and no source for why VAT and TIN share a number.
- **Norway**
  - ValidateVAT stripped "NO"/"no"/"MVA"/"mva" with unanchored String.Replace.
- **Pakistan**
  - No defect found - reported for completeness.
- **Peru**
  - ValidateEntity delegated to ValidateIndividualTaxCode, so every valid RUC validated as both a personal and a company identifier and CountryValidator.Validate reported IsAmbiguous for all of them.
  - ValidationResult.Invalid("Invalid") gave no indication why a well-formed RUC was rejected — now the message a caller sees when the holder type is wrong for the method called.
  - Three existing test rows asserted the bug: TestIndividualCode accepted the company RUC 20512333797, TestCorrectEntityCode and TestCorrectVatCode accepted the personal RUC 10054148289.
  - Nothing pinned the caller-visible symptom: IdentifierResult.IsAmbiguous on the new CountryValidator.Validate API.
- **Philippines**
  - ValidateEntity and ValidateIndividualTaxCode required exactly 12 digits, rejecting the bare 9-digit BIR TIN and the 14-digit form BIR returns now print.
  - ValidateVAT required a literal trailing V, so a VAT-registered taxpayer's plain TIN failed VAT validation while passing entity validation.
- **Portugal**
  - ValidatePostalCode accepted "0000000" — the leading digit is one of nine postal regions, 1 Lisboa through 9 Madeira/Açores; there is no 0 range.
  - ValidateCartaoCidadao had no format guard: only a length-12 check plus the implicit 'first 9 are digits' from CalculateSum.
- **Russia**
  - ValidateIndividualTaxCode accepted the 10-digit legal-entity ИНН, so every company number was also reported as a natural person's tax code and CountryValidator.Validate flagged IsAmbiguous for it.
  - KNOWN-ISSUES: ValidateIndividualTaxCode returned ErrorMessage "Invalid length" when a 10-digit ИНН failed its check digit, because the if/else chain fell through to the `else if (id.Length != 12)`…
  - ValidateVAT delegated to ValidateIndividualTaxCode and would have broken for 10-digit company VAT numbers once that method was narrowed.
- **Taiwan**
  - ValidateEntity threw NotImplementedException; the 8-digit Unified Business Number (統一編號) has a published checksum.
  - ValidateVAT threw NotSupportedException.
  - The resident branch matched only the pre-2021 ARC form ^[A-Z][A-D][0-9]{8}$, so every certificate issued since 2021-01-02 was rejected outright.
  - ValidatePostalCode required exactly 5 digits, rejecting the bare 3-digit district code and the 3+3 form.
- **Thailand**
  - ValidateEntity delegated to ValidateIndividualTaxCode, so every personal number validated as a company number and every company number validated as a personal one; the library reported IsAmbiguous…
  - A number with a valid check digit but an unissued agency prefix (e.g. 009x, 097x) was accepted by every method.
  - ValidateVAT delegated to ValidateIndividualTaxCode.
  - Existing test rows asserted incorrect behaviour: TestNationalId asserted 0105-515-004-336, 0107537001510 and 0107537001706 were valid *national identity* numbers, and TestIndividualCode asserted…
- **Turkey**
  - ValidatePostalCode accepted any five digits, including "00000" and "99999".
  - ValidateVAT stripped "TR" with an unanchored String.Replace, removing those letters from anywhere in the string.
- **Ukraine**
  - None — the assignment brief said ValidateEntity 'just delegates to the 12-digit VAT method', but that is stale: a prior wave already implemented the 8-digit ЄДРПОУ and the 10-digit РНОКПП check…
- **Uruguay**
  - Registration-number range was 01-21, so a real RUT beginning with 22 was rejected.
- **Uzbekistan**
  - ValidateIndividualTaxCode (and ValidateNationalIdentity through it) accepted any 14 digits; the 14th digit of a PINFL is a check digit.

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
