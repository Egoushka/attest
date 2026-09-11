# Known weaknesses

Found during the audit and the two repair waves, and deliberately left alone: either the fix would
have been a rewrite, or the country's real rule could not be sourced with confidence. None of these
is a wrong verdict on a well-formed number — they are validators that check less than the country
publishes, or that are stricter than the spec in a narrow case.

Anyone picking one of these up: confirm the rule from an official source or python-stdnum first, and
add the test before the fix.

## Armenia

- The 8th digit of the ՀՎՀՀ is a check digit, but only the length is validated. Separately, ValidateNationalIdentity is not overridden, so it falls back to the 8-digit TIN, while an Armenian public services number is 10 digits.
  - *Not fixed:* The TIN check digit algorithm is not published in any source I could find (OECD sheet and lookuptax both state it exists without specifying it). Adding a public services number rule would be new behaviour, not a fix, so I wrote no national identity test for Armenia.

## Azerbaijan

- VÖEN validation is length-only: the 9th digit is documented as an algorithmic check digit and the 10th as a taxpayer type marker (1 legal person, 2 natural person), and neither is enforced.
  - *Not fixed:* The check digit algorithm is not published, and the type-digit constraint rests on a single secondary source (lookuptax), so enforcing it risks rejecting live numbers.

## Bahrain

- ValidateIndividualTaxCode checks only '9 digits'. The CPR's ninth digit is a check digit and is not verified.
  - *Not fixed:* The iGA does not publish the check digit algorithm and no authoritative source for it could be found; guessing one would reject valid CPRs.
- The YYMM birth prefix of the CPR is not validated, so '999912345' (month 99) is accepted as valid.
  - *Not fixed:* The documented format is YYMMNNNNC, but the same source notes a minority of citizens and residents hold personal numbers that do not follow it, so a month range check would reject real numbers. No test asserts either way.

## Belarus

- IdExtensions.Translit() is now dead code — BelarusValidator was its only caller in the whole repository (grep for "Translit()" across Attest/CountriesValidators returns only this file), and it no longer uses it. It is also wrong for any Cyrillic-script identifier that uses Latin look-alikes rather than transliteration.
  - *Not fixed:* IdExtensions.cs is on the forbidden shared-file list. Left untouched; flagging it so whoever owns that file can decide whether to delete it or correct it.

## Bolivia

- ValidateEntity uses `^\d{10,}$`: unbounded upper length (a 30-digit string passes) and it rejects the 7-9 digit legacy NITs.
  - *Not fixed:* python-stdnum has no Bolivia module (confirmed 404) and published sources contradict each other on the bounds — lookuptax says there is no fixed length (it tracks the CI), taxdo says 7-10, another guide says 7-12. No SIN spec found, so the correct bounds are unsourced. Tests assert only what is wrong under every source (letters, a single digit).
- ValidateNationalIdentity's `\w?` complemento accepts an underscore and a 9th digit (so "123456789" validates as CI+complemento).
  - *Not fixed:* The SEGIP complemento is 1-2 alphanumeric characters; no authoritative pattern found, and tightening to [A-Za-z]? would create false negatives for numeric complementos. No test asserts either behaviour.

## Bosnia

- ValidateEntity and ValidateVAT throw NotSupportedException, so Bosnia has no company-number validation at all despite the JIB being a real 13-digit identifier.
  - *Not fixed:* NotSupportedException is explicitly by design per the brief, and python-stdnum has no ba module to source the rule from. Not tested, not touched.

## Brazil

- Alphanumeric CNPJ is not supported. From July 2026 the CNPJ body may contain A-Z, and the check digits are computed over ord(c)-48 values (stdnum's br/cnpj.py already implements this). The validator accepts digits only, so every alphanumeric CNPJ is reported InvalidFormat.
  - *Not fixed:* Not a defect in the existing rule but a rule change: it touches the format guard, the checksum input mapping and the reference format string — a rewrite of DigitChecksum rather than a local fix, and it needs a decision on whether the library should accept both generations.
- Repeated-digit CPFs such as "11111111111" satisfy both check digits and are reported valid. Most Brazilian implementations reject all eleven repdigit CPFs.
  - *Not fixed:* python-stdnum (the cited authority) also accepts them — it only rejects the all-zero case, which I did fix. Sources disagree, so I neither changed the behaviour nor asserted it in a test.

## Chile, Colombia, Peru (and ~70 validators outside this assignment)

- Sibling `All(char.IsDigit)` guards elsewhere in the library accept non-ASCII Unicode digits (Arabic-Indic, Devanagari, fullwidth). Where such a guard is followed by int.Parse the validator throws instead of returning a ValidationResult; where it is followed by char.GetNumericValue it silently validates a number no registry issued.
  - *Not fixed:* Fixed inside my five files only. The root-cause fix is a shared IsAsciiDigits helper in Attest/IdExtensions.cs, which this assignment forbids touching, and the remaining call sites belong to other agents' countries.

## Cyprus

- ValidateNationalIdentity can only ever check the format. The 10-digit rule rests on Microsoft Purview's DLP definition; no Cypriot government source publishes the identity card number's length or any check digit, and Wikipedia's Cypriot identity card article documents only that a card number exists. A transposed digit is undetectable.
  - *Not fixed:* No published check rule exists to encode. The regex is unchanged; the tests assert format only and say so in a comment, rather than pretending to verify a checksum.
- Unresolved question about whether TICs starting with 6 use a different check-character algorithm. Several SEO-grade pages (lookuptax.com and its rewrites) claim the Tax For All migration introduced a 'revised check-character algorithm' for the 6xxxxxxx range.
  - *Not fixed:* No published specification of any revised algorithm exists, python-stdnum applies one algorithm to all CY numbers and has never been changed for this (commit history on stdnum/cy/vat.py: last substantive change 2019, then type hints in 2025), and the three 'examples' those pages print (99999999L, 10123456X, 60012345A) all fail the documented mod-26 algorithm, so they are placeholders, not real numbers. I followed python-stdnum, the source named in my instructions, and computed the 60000000S check letter from it myself. If Cyprus ever publishes a second algorithm the 6-range branch will need splitting out.
- ValidatePostalCode accepts "0000". Cyprus postal codes run 1000-9999 (Nicosia 1xxx, Famagusta 5xxx, Larnaca 6xxx/7xxx, Paphos 8xxx), so no code begins with 0.
  - *Not fixed:* Outside the assignment's scope and I did not read an authoritative Cyprus Post range specification, only secondary summaries. Tightening to ^[1-9]\d{3}$ on that basis would be a guess. Left the rule alone; added only null/empty/whitespace/garbage negatives.

## Dominican-republic

- ValidateNCF is case-sensitive on the series letter, where the published reference implementation is not. `RemoveSpecialCharacthers` preserves case, so `number[0] != 'E'`, `number[0] != 'B'` and `Regex.IsMatch(number, "^[AP]")` all reject a lowercase series letter: "b0100000005" and "e310000000005" return "Invalid code". python-stdnum's `compact()` does `clean(number, ' ').upper().strip()`, so it accepts them.
  - *Not fixed:* Outside the assigned defect, and the correct resolution is a repo-wide convention call rather than a local one — several other validators in this library are likewise case-sensitive and I was told to touch only the two Dominican Republic files, so adding a `.ToUpper()` here would make this file diverge from its neighbours on a point I cannot verify as intentional. Behaviour is unchanged and no test asserts either way on lowercase input. One-line fix if wanted: `number = number.RemoveSpecialCharacthers().ToUpper();` (cite https://raw.githubusercontent.com/arthurdejong/python-stdnum/master/stdnum/do/ncf.py `compact()`).
- The whitelists contain entries that are shorter than the format they whitelist, and because the whitelist is consulted before the length check they validate as-is: `_validCedula` holds two 10-digit entries ("0094662667", "0710208838") among 578 otherwise-11-digit entries, and `_validRnc` holds one 8-digit entry ("10233317") among 23 otherwise-9-digit entries. So `ValidateIndividualTaxCode("0094662667")` returns valid for a 10-character input.
  - *Not fixed:* Not a porting error — I diffed against upstream and these exact short entries are present verbatim in python-stdnum's own whitelists (cedula.py has 581 11-digit and 2 10-digit tokens; rnc.py has 27 9-digit and 1 8-digit), and stdnum checks the whitelist before the length check too. The C# is a faithful port; "fixing" it would silently diverge from the reference data on numbers the DGII apparently does issue. Reported for visibility only. No test asserts on these values.

## Ecuador

- ValidateEntity is missing the two fallbacks stdnum performs: third digit 6 should fall back to natural-RUC validation when the public check sum fails, and third digit 9 should try the public check sum before the juridical one. Real RUCs in those classes are rejected as InvalidChecksum.
  - *Not fixed:* Restructuring the if/else chain into try-public-then-fallback is a rewrite of the method, not a local fix. Test rows avoid the ambiguous overlap (all chosen numbers give the same verdict under both implementations).
- ValidateCI rejects a third digit of 6 (`Char.GetNumericValue(number[2]) > 5`); python-stdnum allows 0-6 (`number[2] > '6'`).
  - *Not fixed:* Ambiguous. The tipo-de-cedula digit is 0-5 for natural persons, and stdnum's 6 appears to exist only to support the RUC fallback above. Left alone and not tested.

## ElSalvador

- The "SV" country prefix is not stripped, so stdnum's documented valid input "SV 0614-050707-104-8" is rejected as InvalidLength (RemoveSpecialCharacthers keeps letters).
  - *Not fixed:* Presentation-layer leniency, not a correctness defect, and adding a prefix strip is a behaviour change no source demands. Low severity; left for the next wave.

## FaroeIslands

- ValidateIndividualTaxCode checks only ^\d{9}$, so the DDMMYY part is never validated: "999999999" and "000000000" are accepted as P-numbers.
  - *Not fixed:* The P-tal has no published check digit, and no source I could find states whether the date part is ever modified the way Danish/Norwegian D-numbers are (day+40), nor what the century rule is. A day<=31 / month<=12 guard would be guesswork about which special forms exist, so I left the code alone and wrote no test asserting the current behaviour for impossible dates.
- ValidatePostalCode accepts any three digits, including "000" and "999"; the assigned range runs from 100 (Torshavn) to about 970 (Nordadalur) with large gaps.
  - *Not fixed:* Encoding the assigned set would need a hard-coded list that exists nowhere else in this repo - all ~85 ValidatePostalCode methods are plain format regexes - and a range check would still accept unassigned codes. Consistency beats a half-correct tightening.

## Georgia

- The 11-digit personal number's last two digits are described as verification digits, but ValidateIndividualTaxCode checks nothing beyond the 9-or-11-digit format, so any 11-digit string passes.
  - *Not fixed:* No published algorithm; python-stdnum has no Georgia module. Tested as a format guard only, and the test comments say so.

## Germany

- ValidateIndividualTaxCode (Steueridentifikationsnummer) over-accepts on the digit-repetition rule. GermanyValidator.cs uses `if (counts.Count != 9 && counts.Count != 8) return Invalid;` over the first ten digits. counts.Count == 8 is satisfied both by one digit occurring three times (valid) and by two different digits each occurring twice (invalid per the official rule, which allows exactly one repeated digit). It also never checks that a digit occurring three times is non-consecutive. Confirmed in python3 against the official rule: of 400k generated numbers carrying a correct ISO 7064 MOD 11,10 check digit, the validator accepts numbers the official rule rejects -- concrete examples 37406812415 (digits 4 and 1 each twice), 95748906826, 16487059644, 20341768488.
  - *Not fixed:* Outside my assignment, which is ValidateEntity and its tests, and the audit has no confirmed finding for this method. The three existing TestIndividualCode rows all behave identically under the current and the official rule, so tightening it would be an unverified change to a method no one asked me to touch while other agents work the same tree. I wrote no test asserting the current loose behaviour.
- The audit's own corrected_fix proposes adding ("2477081508152", true) as a regression row for the dead Bremen regex. That number is invalid: Bremen uses the 11er-Verfahren with factors 0,0,4,3,0,2,7,6,5,4,3,2, and for the first twelve digits 247708150815 the weighted sum is 136, so the Pruefziffer is 11 - (136 mod 11) = 7, not 2. 2477081508157 would be valid.
  - *Not fixed:* Not a code bug -- a bad test number in the audit's suggested fix. I did not add the row. The dead-regex regression is covered instead by 2497012301233, the Bremen example published in Tabelle 4-1 of the ELSTER spec, whose check digit 3 I recomputed and confirmed.

## Guatemala

- ValidateEntity's compaction lacks stdnum's `.upper()` and `.lstrip('0')`, so the lowercase check digit "576937-k" and the zero-padded "00576937K" are rejected where stdnum accepts them.
  - *Not fixed:* Two behaviour changes in one method; leading zeros do not affect the check digit (they carry weight but contribute 0) so this is purely about accepted input shapes. No test asserts either direction.
- ValidateIndividualTaxCode throws NotSupportedException by design, but IdValidationAbstract.ValidateNationalIdentity delegates to it, so ValidateNationalIdentity("anything") throws rather than returning a ValidationResult.
  - *Not fixed:* Per the brief, NotSupportedException by design is left alone; the leak through the base class would need a change to the shared IdValidationAbstract.cs, which is off limits. Neither method is tested.

## Hungary

- Century derivation is wrong for leading digits 3, 4, 7 and 8. Per the source, 7/8 mean an 1800s birth year but the validator uses yearPrefix "19" for them; 3/4 mean 18xx OR 20xx but the validator always assumes 20xx, so a genuine 1800s code with leading 3/4 and YY > 26 is rejected as a future date, and one with YY <= 26 is checked with the reversed weights when the old weights apply.
  - *Not fixed:* Handling 3/4 correctly means accepting either checksum (the source says 'mindket algoritmusra szukseg lehet'), which weakens detection for the common 20xx case and changes accept/reject behaviour well outside the assigned defect. Everyone born in the 1800s is dead, so it is a dead-data path. I added the 7/8 exclusion only to avoid regressing those to the new weights; I did not otherwise touch the century logic.
- ValidateEntity's cegjegyzekszam regex `^(?:[01][0-9]|20)(?:[01][0-9]|2[0-3])[0-9]{6}$` accepts county code 00 and company-form code 00, neither of which exists (counties run 01-20, forms 01-23).
  - *Not fixed:* Outside the assigned defect, and I could not find an authoritative published list of the form codes to cite before tightening the ranges. No checksum exists for this number, so the regex is the only check -- a wrong tightening would silently reject real registrations.

## Indonesia

- Only the legacy 15-digit NPWP is supported. Since 2024 the NPWP is 16 digits: either a NIK, or a leading 0 followed by the old number, in which case the Luhn window is the first 10 digits. A current 16-digit NPWP is rejected as InvalidFormat.
  - *Not fixed:* Supporting it means a second length branch plus NIK validation (province/regency table and birth-date extraction) — a rewrite, not a local fix. The 16-digit case is asserted invalid in no test.

## Kazakhstan

- ValidateIndividualTaxCode only range-checks the birth date (month 1-12, day 1-31) at KazahstanValidator.cs lines 42-47, so impossible calendar dates such as 900231... (31 February) or 900431... pass the date gate. A real DateTime.TryParseExact on the YYMMDD prefix would catch these.
  - *Not fixed:* Outside the assigned defect and carries false-negative risk I could not resolve from an authoritative source: the 7th digit value 0 marks foreign nationals, whose IINs are reported to carry synthetic or placeholder date components, and I found no published statement that every issued IIN has a real calendar date in positions 1-6. Tightening it could reject legitimately issued numbers, so I left the code alone and wrote no test asserting either behaviour for impossible calendar dates.
- The 7th digit (century + sex code) is never validated. Documented values are 0-6 (0 foreign national, 1/2 male/female 1800s, 3/4 1900s, 5/6 2000s); 7, 8 and 9 are not documented as assigned, so 900701700108-shaped numbers with a correct check digit are accepted.
  - *Not fixed:* Not part of the assignment, and I found no authoritative statement that 7-9 are permanently unassigned rather than merely unused so far — only secondary sources listing 0-6. Adding the guard on that basis risks rejecting future or edge-case issuance, so I left it and wrote no test for that path.
- The try/catch around int.Parse in ValidateIndividualTaxCode (lines 40-52) is now unreachable defensive code: the anchored @"^\d{12}$" guard above it guarantees both substrings are two digits, so int.Parse cannot throw.
  - *Not fixed:* Harmless dead code, not a defect. Removing it is a cosmetic diff in a file other agents are not touching but that adds no correctness; left in place to keep the change minimal.

## Korea

- ValidateIndividualTaxCode rejects any RRN whose birth date is less than 17 years ago (maxDate = now - 17 years -> InvalidDate). Korean RRNs are assigned at birth registration, not at 17 — the resident card is what is issued at 17. python-stdnum kr.rrn only rejects future dates, and only when allow_future=False. So every RRN of a minor is currently reported invalid.
  - *Not fixed:* Relaxing it changes which inputs the library accepts, and the 17 is clearly deliberate (not a typo or an inverted comparison). Out of scope for a test-coverage pass. All five valid numbers in KoreaValidatorTests use birth dates old enough to clear the gate, so the suite stays green either way.
- No check on the place-of-birth component. python-stdnum rejects int(number[7:9]) > 96 as InvalidComponent; this validator accepts 97-99.
  - *Not fixed:* Missing strictness rather than a wrong result; adding it would reject numbers the library currently accepts. Reported for the next wave.

## Macedonia

- ValidateIndividualTaxCode throws NotImplementedException (same contract violation as Montenegro), even though ValidateNationalIdentity validates the JMBG right above it and could be delegated to.
  - *Not fixed:* Ambiguous which number the method is meant to carry: natural persons in North Macedonia hold a 13-digit ЕДБ (starting 40/50) as their tax number, which is not the JMBG, and I found no source pinning down which of the two this method should accept. Delegating to the wrong one would be worse than throwing.

## Malta

- ValidateVAT computes the check digits as `37 - sum % 37` and compares for exact equality. When sum % 37 == 0 this yields 37, so a number whose check digits are '00' is rejected and one whose check digits are '37' is accepted. python-stdnum instead requires sum(weights 3,4,6,7,8,9,10,1 over all 8 digits) % 37 == 0, which accepts '00' - and also accepts the congruent values 49 and 86, which Malta almost certainly never issues.
  - *Not fixed:* Published implementations disagree and I found no authoritative Maltese source settling which of '00' and '37' the IRD emits when the weighted sum is a multiple of 37. Every non-degenerate case agrees - I cross-checked the current code against python-stdnum on 11679112 (its own doctest), MT 1167-9112, 12345634 (check digits I computed), 11679113 and 01679112, all matching. Switching to the stdnum form would additionally start accepting check digits 49 and 86, which is strictly looser, so I left the arithmetic alone rather than trade a hypothetical false reject for two certain false accepts.
- Wikipedia's 'Postal codes in Malta' article claims some Maltese postcodes begin with two letters (TP for Tigne Point), which the new ^[A-Z]{3}\d{4}$ pattern would reject.
  - *Not fixed:* Not a bug - I checked it before tightening the pattern. Real Tigne Point addresses use SLM 3xxx codes (Ix-Xatt Ta' Tigne SLM 3010, Preti Court SLM 3190), and the MCA's own 2018 decision notice states the seven-character three-letters-four-digits format with no exceptions. The Wikipedia claim does not hold up, so I encoded the MCA format.

## Mauritius

- ValidateDate only rejects day > 31 and month > 12. Day 00, month 00 and impossible dates (31 February, 31 April) all pass, and the parsed year is assigned but never used (CS0219). python-stdnum builds a real date here and rejects all of them.
  - *Not fixed:* Rejecting day/month 00 is safe, but a full date check needs a century rule and stdnum's own source flags its 2000+ assumption as probably wrong; whether Mauritius issues placeholder 00 dates for unknown births is unsourced. Tightening could reject real cards, so the code was left alone and no test asserts the loose behaviour.
- ValidateEntity and ValidateVAT throw NotImplementedException, so they crash on every input including null instead of returning a ValidationResult.
  - *Not fixed:* Not a missing guard but a missing feature: it needs the Mauritian business registration number and VAT (VAT2xxxxxxx) rules, which is a new implementation, not a small local fix. Treated like the NotSupportedException placeholders and left untested.

## Moldova

- ValidateIndividualTaxCode (IDNP) and the inherited ValidateNationalIdentity check only ^\d{13}$. The IDNP is documented as 2TTTXXXYYYYYK where K is a check digit, and the leading 2 (natural person) is not enforced either, so e.g. 9999999999999 passes.
  - *Not fixed:* The check-digit formula is not published: ro.wikipedia names K a 'cifră de control' without giving it, python-stdnum has only md.idno (companies), and the Moldovan State Tax Service material reached via the OECD TIN annex does not reproduce the algorithm. Encoding a guessed rule would be worse than the gap. I wrote format-only tests for these two methods and deliberately chose IDNP samples that also satisfy the IDNO 7,3,1 mod-10 weights, so those assertions stay green if a future wave does add the checksum.

## Montenegro

- ValidateVAT, ValidateEntity and ValidateIndividualTaxCode all throw NotImplementedException, so they violate the 'every public Validate* returns a ValidationResult' contract for every input including null. The Montenegro PIB is a fully published format: 8 digits, check digit = (-sum(w*d)) % 11 % 10 over weights 8,7,6,5,4,3,2 (valid 02655284, invalid 02655283).
  - *Not fixed:* Implementing three unimplemented methods is new functionality, not a local defect fix, and the brief says leave by-design throws alone. The algorithm and test vectors are in https://arthurdejong.org/python-stdnum/doc/1.20/stdnum.me.pib.html — next wave can land it in ~12 lines.

## Nigeria

- ValidateIndividualTaxCode accepts only the 10-digit JTB TIN, and ValidateEntity/ValidateVAT delegate to it. The 12-digit FIRS TIN (NNNNNNNN-NNNN, e.g. 12345678-0001) is rejected once RemoveSpecialCharacthers strips the hyphen, and since January 2026 the reformed Tax ID is 13 digits for individuals with the CAC registration number as the corporate Tax ID — so ValidateEntity is validating an individual TIN for companies.
  - *Not fixed:* Which form the library should accept is a product decision, not a lookup: broadening the regex changes what ValidateEntity means and the 2026 reform is too recent to have a settled published format. The test file therefore asserts only the 10-digit JTB rule the code documents, and contains no case claiming a 12- or 13-digit TIN is invalid.

## Norway

- ValidateVAT strips "NO"/"no"/"MVA"/"mva" with unanchored String.Replace, so those letters are removed from anywhere in the string: "1NO23456789" is accepted as organisasjonsnummer 123456789. TurkeyValidator.ValidateVAT has the identical defect with "TR" ("1TR234567890" is accepted as 1234567890).
  - *Not fixed:* The same unanchored-prefix-strip pattern appears in roughly twenty validators across the library (France, Latvia, Lithuania, Slovenia and others). Fixing it in my two files alone would make the codebase inconsistent; it is a shared-style decision for a wave that can touch all of them at once. No test asserts the false-positive behaviour.

## OTHER COUNTRIES (codebase-wide pattern, outside my files)

- The \d-matches-Unicode-Nd + int.Parse-throws combination is not Argentina-specific. Any validator that guards with "^\\d{N}$" (or \d inside a longer pattern) and then calls int.Parse/Convert on the characters will throw FormatException on Eastern-Arabic, Devanagari or fullwidth digits, because RemoveSpecialCharacthers preserves them. Grep showed the "^\\d{4}$" form in at least Belgium, Mexico and Paraguay postal codes (harmless there — no parse follows), but every checksum validator using \d before a parse is a candidate crash.
  - *Not fixed:* Those files belong to other agents in this shared working tree; the hard rules forbid touching them. Suggest a sweep for `\d` in validators that parse digits afterwards, replacing the class with [0-9].

## Philippines

- ValidateEntity and ValidateIndividualTaxCode both require exactly 12 digits ("^\\d{12}[VN]?$"). The BIR TIN is a 9-digit core number; the 3-digit branch code is an optional suffix (000 for individuals). A bare 9-digit TIN is rejected.
  - *Not fixed:* python-stdnum has no ph module (issue #116 is still open), so I have no authoritative validator to check against, and widening the regex to "^\\d{9}(\\d{3})?[VN]?$" changes accepted values on my own reading of BIR prose. No test asserts either way for 9-digit input.
- ValidateVAT requires a literal trailing "V" ("^\\d{12}V$"). The V/N suffix is a presentation convention on BIR forms, not part of the number, so a VAT-registered taxpayer's plain 12-digit TIN fails VAT validation while passing entity validation.
  - *Not fixed:* Same reason — no authoritative source to confirm whether the suffix is meant to be mandatory here. Tested the documented behaviour only where it is unambiguous (12 digits + V accepted, + N rejected).

## Poland

- Dead code in CheckIfDayIsCorrect (PolandValidator.cs:179): `for (int j = 1 + i; j <= 7 + i ? j <= 7 + i : j <= 12 + i; j += 2)` — the ternary's two branches evaluate the same condition, so the intended 'long months' arm is unreachable, and the loop body only tests `PESELDay > 31`, which the guard on line 172 has already rejected. The loop can never return false.
  - *Not fixed:* It is provably inert, not a behavioural defect. I replicated the whole month/day/century path in python3 and compared it against datetime.date across all 1,000,000 combinations of yy 00-99, month 00-99 and day 00-99: zero mismatches. Deleting the loop would be a pure no-op cleanup, which is review surface without a fix, so I left it for the owner to decide.

## Portugal

- public int CheckSum(string value) multiplies the CHAR CODE by the weight, not the digit: sum += value[i] * (value.Length + 1 - i). It happens to be correct for the only call site because that always passes an 8-character substring: the constant offset is 48 * (9+8+7+6+5+4+3+2) = 48 * 44 = 2112, and 2112 % 11 == 0, so the ASCII bias cancels mod 11. For any other length it returns garbage, and CheckSum(null) throws NullReferenceException.
  - *Not fixed:* It is public API, it is not a Validate* method (so the never-throw rule as written does not reach it), and it produces the correct answer on the only path that reaches it from inside the class — ValidateBilhetedeIdentidade calls it with value.Substring(0, 8). Changing a public signature/behaviour is outside a NIF-prefix assignment and would be an unrequested API change. Verified numerically in python3: CheckSum("10000000") = 2 and CheckSum("90000000") = 7, matching the digit-based mod-11 rule exactly.
- ValidateCartaoCidadao has no format guard at all — only a length-12 check plus the implicit 'first 9 must be digits' from CalculateSum's `i < 9 && d > 9` test. Positions 9 and 10 are not required to be letters, so an all-digit 12-character string that happens to satisfy the mod-10 sum is accepted as a Cartao de Cidadao. Separately, the `chars` dictionary is uppercase-only, so a lowercase card number ("000000000zz4") is rejected while the identical uppercase one is accepted.
  - *Not fixed:* Outside the assigned NIF scope, and I have no authoritative statement on the exact CC character layout to encode. I did verify the arithmetic is right: the doubling/cutoff and index parity are equivalent to python-stdnum stdnum/pt/cc.py calc_check_digit (C# doubles forward even indices over all 12 chars, stdnum doubles reversed-even indices over the 11-char prefix — the same index set, and requiring sum % 10 == 0 is the same test as check == (10 - s) % 10). Both agree that 000000000ZZ4 is valid and 000000000ZZ3 is not, so the existing positive rows are safe to keep.
- ValidatePostalCode accepts "0000000". Portuguese postal codes run from 1000-000 upward (the leading digit is the district, 1 Lisboa through 9 Madeira/Acores); there is no 0 range.
  - *Not fixed:* I did not confirm the 1-9 leading-digit rule against a published CTT source in this pass, and tightening a format regex on an unverified rule risks rejecting real codes. No test asserts the 0 case in either direction.

## Russia

- ValidateIndividualTaxCode returns ErrorMessage "Invalid length" when a 10-digit INN fails its check digit: the if/else chain falls through to the `else if (id.Length != 12)` branch. IsValid is correctly false, only the message lies.
  - *Not fixed:* Message-only defect. Correcting it means restructuring the whole if/else chain that the 12-digit path falls through, which is a bigger diff than the payoff; my tests assert IsValid, so the behaviour is pinned either way.
- ValidateBIK never checks the leading country code, so "999999999" is accepted as a bank identifier. It only checks 9 digits plus the last three being 000-002 or 050-999.
  - *Not fixed:* Sources disagree on the current BIK layout: bank sites (assistentus.ru, sberbusiness.live, els24.com) describe the classic 04 + region + subdivision + 050..999 structure, while ru.wikipedia describes the post-2020 Regulation 507-P layout where the first digit is a participation type (0/1/2) and digits 2-9 are a free participant identifier. The rule is not settled enough to encode, and a wrong prefix guard would reject live BIKs.
- ValidateBIK and ValidateOGRN report ValidationResult.InvalidChecksum() for failures that are a range or structure violation, not a checksum failure.
  - *Not fixed:* Cosmetic; the repo has no InvalidRange-style result and inventing one touches the shared ValidationResult, which is off limits.

## Taiwan

- ValidateEntity throws NotImplementedException even though Taiwan has a published 8-digit Unified Business Number with a documented checksum (python-stdnum tw.ubn). ValidateVAT throws NotSupportedException for the same number.
  - *Not fixed:* Implementing UBN is a new feature, not a defect fix. Per the brief I did not test methods that throw.
- The resident branch only matches the old ARC format "^[A-Z][A-D][0-9]{8}$". Resident certificate numbers issued since the 2021 reform use "^[A-Z][89][0-9]{8}$" (the second character is 8 or 9, mapped as a plain digit in the checksum) and are rejected outright.
  - *Not fixed:* Adding a third branch with its own digit mapping is a rewrite of the dispatch, not a local fix, and I could not find an authoritative worked example to pin the new-format weighting. No test asserts the current behaviour for [89] numbers.
- ValidatePostalCode requires exactly 5 digits. Taiwan uses 3-digit district codes and 3+2 (5) and, since 2020, 3+3 (6) forms; "100" and "100091" are both real and both rejected.
  - *Not fixed:* Which of the three forms the library intends to accept is a product decision, not a derivable rule. Tested only the 5-digit accept path and unambiguous garbage.

## Turkey

- ValidatePostalCode accepts any five digits, including "00000" and "99999"; the first two digits are a province code in the range 01-81.
  - *Not fixed:* Same reasoning as the Faroese postal code - every ValidatePostalCode in this repo is a bare format regex, and the province-code range drifts as provinces are added. Not worth diverging in one file.

## Ukraine

- ValidateVAT accepts any 12 digits. The 12-digit ІПН of a VAT payer is only the legal-entity form; individuals and sole traders use their 10-digit РНОКПП, which this method rejects. No check digit is validated either.
  - *Not fixed:* Sources contradict each other on the accepted VAT forms (globalvatcompliance.com: 12 for entities, 10 for individuals; vatify.eu: 8 or 12) and no check digit algorithm for the 12-digit form is published, so widening it would be guesswork. Left at the status quo and tested as a format guard only.

## UnitedArabEmirates

- ValidateNationalIdentity checks only the 784 + 12 digit shape; the trailing check digit is never verified, so 784198012345670..9 are all accepted.
  - *Not fixed:* ICP/u.ae publish the 15-digit format but no check digit algorithm. The widely circulated Luhn recipe is explicitly untested by its own author and is reported to fail real cards whose check digit is 0, so encoding it would reject valid IDs. The valid samples in the test file were nevertheless chosen to carry the Luhn digit of their first 14 digits, so they survive if a checksum is added later.

## Uruguay

- Registration-number range mismatch. The validator accepts the first two digits 01-21; python-stdnum master accepts 01-22 (`if number[:2] < '01' or number[:2] > '22'`). One of the two is wrong, so a real RUT beginning with 22 would be rejected here.
  - *Not fixed:* Only one source (stdnum master, which itself changed this bound at some point) and no authoritative DGI document or real 22-prefixed RUT to corroborate. Widening the range on a single source risks accepting numbers Uruguay does not issue. No test asserts either behaviour for prefix 22.

## Uzbekistan

- ValidateIndividualTaxCode and, through it, ValidateNationalIdentity accept any 14 digits; the PINFL's 14th digit is a check digit.
  - *Not fixed:* The PINFL check digit algorithm is not publicly documented (searched Russian, Uzbek and English sources; only the field layout is published). Note the 14-digit length itself is correct, not a bug: since 2021 the PINFL replaced the individual TIN in Uzbekistan, which I confirmed before writing the tests.
