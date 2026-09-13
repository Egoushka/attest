# Known weaknesses

Found during the audit and the repair waves, and deliberately left alone: either the fix would have
been a rewrite, or the country's real rule could not be sourced with confidence. None of these is a
wrong verdict on a well-formed number — they are validators that check less than the country
publishes, or that are stricter than the spec in a narrow case.

Anyone picking one of these up: confirm the rule from an official source or python-stdnum first, and
add the test before the fix.

## ALL (documentation)

- 44 validators carry XML `<param>` tags inherited from upstream that name a parameter the method
  does not have, or omit the one it does — `NorwayValidator.ValidateIndividualTaxCode` documents
  `id` where the parameter is `number`, and so on. The compiler reports these as CS1572 and CS1573;
  both are suppressed in `Directory.Build.props` so the build stays readable.
  - *Not fixed:* Mechanical and safe, but it touches 44 files and would bury any real change it was
    committed alongside. A good first contribution: fix the tags, then delete CS1572 and CS1573
    from the `NoWarn` list so they cannot come back.

## Armenia

- The 8th digit of the TIN is a check digit, but only the length is validated.
  - *Not fixed:* Still unsourced. The SRC's OECD sheet states the check digit exists and is calculated from the first seven digits but gives no algorithm, and nothing published by the SRC, python-stdnum (no stdnum/am module — confirmed against the repository's module listing) or any secondary guide supplies one. Inventing one would reject live numbers.
- The 10th digit of the public services number (the 'specifying digit') is not verified.
  - *Not fixed:* Law HO-288-N art. 4 part 2 and decision N 1783-N annex 1 both say the calculation order for that digit is established by the authorised body (the migration/population-register authority, formerly the Ministry of Social Security), and that procedure is not published. Format and field ranges are enforced; the check digit is not.
- The unshifted June month code (06, 26, 46, 66, 86) is accepted alongside the shifted one (14, 34, 54, 74, 94).
  - *Not fixed:* The acts are self-contradictory here: they state the range as 01-12 (which contains 06) and then say June is coded 14. I could not establish which reading governs issued numbers, so the validator accepts both rather than risking a false negative on live June-born numbers. No test asserts either way.
- The day/month pair is not cross-checked against the year (e.g. day 31 in a 30-day month, 29 February in a non-leap year).
  - *Not fixed:* Out of scope for the person/business question and not required by the acts; the century is derivable from the month pair, so it could be added later, but it buys nothing for the ambiguity this task was about.
- KNOWN-ISSUES.md still records 'ValidateNationalIdentity is not overridden, so it falls back to the 8-digit TIN' as unfixed.
  - *Not fixed:* KNOWN-ISSUES.md is on the forbidden shared-file list, so I did not edit it. Whoever owns that file should drop the second half of the Armenia entry (the national-identity fallback is now fixed and sourced) and keep the first half (the TIN check digit remains unpublished), optionally adding the PSN check digit as a new known gap.

## Azerbaijan

- VÖEN validation is length-only: the 9th digit is documented as an algorithmic check digit and the 10th as a taxpayer type marker (1 legal person, 2 natural person), and neither is enforced.
  - *Not fixed:* The check digit algorithm is not published, and the type-digit constraint rests on a single secondary source (lookuptax), so enforcing it risks rejecting live numbers.

## Bahrain

- ValidateIndividualTaxCode checks only '9 digits'. The CPR's ninth digit is a check digit and is not verified.
  - *Not fixed:* The iGA does not publish the check digit algorithm and no authoritative source for it could be found; guessing one would reject valid CPRs.
- The YYMM birth prefix of the CPR is not validated, so '999912345' (month 99) is accepted as valid.
  - *Not fixed:* The documented format is YYMMNNNNC, but the same source notes a minority of citizens and residents hold personal numbers that do not follow it, so a month range check would reject real numbers. No test asserts either way.

## Belarus

- IdExtensions.Translit() is dead code (BelarusValidator was its only caller and now uses its own Cyrillic look-alike map instead).
  - *Not fixed:* Attest/IdExtensions.cs is on the forbidden shared-file list. Confirmed the situation is unchanged: grep for Translit() across Attest/CountriesValidators returns zero hits, and BelarusValidator.Normalize() does the mapping inline (А->A, В->B, Н->H, Р->P, С->C) precisely because transliteration is wrong for these numbers. Unchanged, reported only.
- Normalize() strips "УНП"/"UNP" anywhere in the input, where stdnum's compact() strips it only as a leading prefix - so "200988541UNP" validates here and is rejected by the reference implementation.
  - *Not fixed:* Over-acceptance of malformed input only (U and N are not in the letter alphabet ABCEHKMOPT, so the token cannot occur inside a well-formed UNP), not in my assignment, and no test asserts either way. One-line fix if wanted: replace the two .Replace() calls with a StartsWith/Substring prefix strip before RemoveSpecialCharacthers. https://raw.githubusercontent.com/arthurdejong/python-stdnum/master/stdnum/by/unp.py

## Bolivia

- The complemento is still capped at one character (`[A-Za-z0-9]?`), but SEGIP assigns two-character complements such as "1A" and "1B".
  - *Not fixed:* I pulled the SEGIP regulation itself. Articulo 40 defines the Numero Complemento Alfanumerico as "caracteres alfanumericos" (plural, no count) separated from the root number by a hyphen; the document states no length anywhere. Widening to {0,2} without a count is guesswork and it widens a real false-positive hole: because RemoveSpecialCharacthers is not applied here and the complemento may be numeric, "1234567890" would then validate as 8 digits plus a two-digit complemento. I fixed only the part that is wrong under any reading (the underscore) and left the count alone.
- The 7-13 digit bound I encoded is a union of secondary sources, not a SIN specification.
  - *Not fixed:* Fixed in the sense that the unbounded regex is gone, but flagging the residual uncertainty: the SIN publishes the generation rule (CI + 3 digits) and no total length, and the three secondary sources disagree on the maximum (10 / 12 / 13). I took the widest published bound so the change cannot reject anything any source endorses, and the reasoning is in a code comment. If a SIAT field spec turns up later the maximum should be tightened to it.

## Bosnia

- The RS rulebook (clan 11-13) also prescribes an inner check digit K at position 9, 'kontrolni broj po modulu 11 za prethodni osmocifreni dio broja'. It is not implemented: neither the EDB-style weights nor ISO 7064 MOD 11,10 reproduce position 9 on the official FBiH data (142/29319 and 180/29319 respectively), and the FBiH rulebook does not mention an inner check digit at all, so it may only exist for RS-issued numbers.
  - *Not fixed:* The weights are unpublished and I could not derive them with confidence from the data I had. Encoding a guess would reject real JIBs; the 13th-digit rule alone is verified on 29318/29319 official numbers.

## Brazil

- No text-readable Receita Federal source states that repeated-digit CPFs are invalid; the rule rests on two independent Brazilian implementations plus the arithmetic demonstration.
  - *Not fixed:* Fix applied anyway (it was the assignment), but flagging the citation quality: gov.br's own CPF material is either a scanned PDF or behind a restricted-content gate for fetching. Note the behaviour now deliberately diverges from python-stdnum, which rejects only the all-zero CPF via `int(number) <= 0`. The code comment says so explicitly.
- The two official Receita PDFs (manual-dv-cnpj.pdf, cnpj-alfanumerico.pdf) are image scans; I could not extract their text directly.
  - *Not fixed:* No OCR available in this environment (pdftoppm/poppler not installed, and running dotnet/installing tooling is out of scope). I verified the rule against the reference implementations Receita/Serpro published instead, and cross-checked every alphanumeric test vector by running their Python generator locally: `-dv "12.ABC.345/01DE"` -> 35, `-dv "A1.B2C.3D4/E5F6"` -> 68, and `-dv "16.727.230/0001"` -> 97 (existing numeric CNPJs keep their check digits).
- ValidateEntity's InvalidFormat hint is still the digits-only example "12345678901234", which no longer advertises that letters are accepted.
  - *Not fixed:* Cosmetic message-only change with no behavioural effect, and every existing assertion is on IsValid rather than ErrorMessage. Left alone to keep the diff minimal; one-line change if wanted.
- All-same-character CNPJs other than all-zero are not specially rejected.
  - *Not fixed:* Not needed: I verified in python3 that 00000000000000 is the only repdigit CNPJ that satisfies the check digits, and that case is already caught by the existing StartsWith("000000000000") guard (which matches both stdnum and the Receita reference's REGEX_VALOR_ZERADO). Adding a repdigit rule would be dead code.

## Chile, Colombia, Peru (and ~70 validators outside this assignment)

- Sibling `All(char.IsDigit)` guards elsewhere in the library accept non-ASCII Unicode digits (Arabic-Indic, Devanagari, fullwidth). Where such a guard is followed by int.Parse the validator throws instead of returning a ValidationResult; where it is followed by char.GetNumericValue it silently validates a number no registry issued.
  - *Not fixed:* Fixed inside my five files only. The root-cause fix is a shared IsAsciiDigits helper in Attest/IdExtensions.cs, which this assignment forbids touching, and the remaining call sites belong to other agents' countries.

## Cyprus

- ValidateNationalIdentity checks format only (10 digits), and the question of whether TICs in the 6xxxxxxx range use a revised check-character algorithm after the Tax For All migration is unresolved.
  - *Not fixed:* Unchanged from the earlier wave and still correct: no Cypriot government source publishes an identity-card check digit, and no specification of a second VAT check-character algorithm exists. python-stdnum applies one algorithm to all CY numbers. Nothing new found this pass; I did not touch either method.

## DominicanRepublic

- The _validCedula / _validRnc whitelists are consulted before the length check and contain entries shorter than the format (two 10-digit cedulas, one 8-digit RNC), so ValidateIndividualTaxCode("0094662667") is valid at 10 characters.
  - *Not fixed:* Unchanged from the earlier wave's finding and still correct: this is a faithful port - python-stdnum's own cedula.py/rnc.py whitelists carry those exact short tokens and stdnum also checks the whitelist before the length. "Fixing" it would diverge from the reference data on numbers the DGII apparently does issue.

## FaroeIslands

- The new P-number guard still accepts impossible calendar dates (31 February, 31 April, 30 February), and it may reject the non-resident identification number TAKS issues.
  - *Not fixed:* Two separate blockers, both unresolved after searching. (1) No century rule is published for the two-digit year, and 29 February is only a real date under one century interpretation (2000 yes, 1900 no), so a day-in-month check cannot be written without inventing the rule the earlier wave already refused to invent. A day 01-31 / month 01-12 guard is exactly what the OECD sheet states and nothing more. (2) The same OECD sheet says that for a person who is not a Faroese resident but is taxable there, "the Faroese Tax Administration issues an Identification number. The structure and format of this Identification number is similar, but not identical to the P number" — and no source I could find (OECD, norden.org, lookuptax, torshavn.fo, the TAKS temporary-p-tal application form) publishes that format. If it offsets the day the way Danish erstatningsnumre (+60) or Norwegian D-numbers (+40) do, my new guard would reject it. I flagged this risk in the source comment. Widening it later is a one-line regex change.
- Postal codes 971-999 and the large unassigned gaps inside 100-970 are still accepted.
  - *Not fixed:* The earlier wave's reason still stands for the assigned-set part: encoding the roughly 100 allocated codes would need a hard-coded list that exists nowhere else among the repo's ~85 ValidatePostalCode methods. I did not add a 970 ceiling either, because my only source for the upper bound is the Danish Wikipedia list, and a newly allocated code above 970 would then be falsely rejected. I tightened only the 000-099 block, which is impossible under every source.

## Georgia

- The 11-digit personal number's trailing verification digits are still unchecked; ValidateIndividualTaxCode remains a 9-or-11-digit format guard.
  - *Not fixed:* No algorithm published, so the earlier wave's reason stands. python-stdnum has no ge module. Sources also contradict each other on what the trailing digits even are: lookuptax.com says 'the last two digits are used for verification purposes', another says the last three are randomly generated, and tin-check.com states validation is performed against the Revenue Service registry rather than by formula. Nothing to encode.

## Guatemala

- The repo-wide NotSupportedException convention still stands in 13 other validators (Bosnia, China, Pakistan, Taiwan, HongKong, UAE, Uzbekistan, Mauritius, UnitedStates, Cuba, Bahrain, Monaco, Korea).
  - *Not fixed:* Outside my file list. Note that KoreaValidatorTests already asserts Assert.False(ValidateEntity(...).IsValid) against a KoreaValidator that still throws at lines 15 and 100, which either means another agent is mid-edit on Korea in this working tree or that pair is currently inconsistent. Worth a check before the central test run.

## Iceland

- The century digit (position 10) is decoded as `year = (century == 9) ? 1900 + year : (20 + century) * 100 + year`. Digit 8 — documented by Registers Iceland/Wikipedia as marking births in 1800-1899 — yields year 28yy instead of 18yy, and digits 1-7 are accepted at all, producing years 2100-2799. python-stdnum restricts the field to [09] and so rejects digit 8 entirely.
  - *Not fixed:* Out of scope for the assigned question, and the two candidate fixes disagree: python-stdnum's [09] would reject 1800s-born people that Þjóðskrá documents as existing, while accepting 8 needs a rule I could not source with confidence for the 1-7 values. The practical impact is near zero — the decoded year only flips a verdict for 29 February in an x800 year, since 1800 is not a leap year but 2800 is; every other date has identical leap status at +1000 years. Needs its own sourcing pass against skra.is.
- Attest/IdentifierResult.cs line 34 still names Iceland in the IsAmbiguous doc comment's list of "countries that issue one number for both". That is now false for IS.
  - *Not fixed:* IdentifierResult.cs is on the do-not-touch shared-file list. Iceland should be struck from that sentence.
- ValidateVAT accepts any 5-6 digit VSK number with no structural or check-digit validation.
  - *Not fixed:* Not part of the assignment and not researched; reported only so it is not mistaken for something this pass verified.

## Indonesia

- ValidateNik checks the 16-digit shape and the embedded birth date but not the registration-place code (first 6 digits), so a NIK with an impossible province/regency is accepted.
  - *Not fixed:* python-stdnum validates it against numdb.get('id/loc'), a bundled province/regency/district database; this library carries no such table and embedding one is a data-shipping decision, not a local fix. Province codes alone are not a safe substitute - the 2022 reorganisation added codes 93-96, so a hardcoded range would start rejecting valid numbers. Documented in the XML comment on ValidateNik.
- ValidateVAT still delegates to ValidateEntity, so it only accepts organisation taxpayer types (0-3). A VAT-registered sole proprietor (PKP orang pribadi), whose NPWP is an individual number or a NIK, fails VAT validation.
  - *Not fixed:* Pre-existing design that predates my change and applies equally to the 15-digit individual numbers. Widening ValidateVAT to accept individual types changes what the method means, which is a product decision, not a lookup. No test asserts either way.

## Kazakhstan

- ValidateIndividualTaxCode only range-checks the birth date (month 1-12, day 1-31), so 900231... (31 February) passes.
  - *Not fixed:* The earlier wave's reason not only still stands, it is now positively sourced against the fix. Постановление Правительства РК № 853 of 26.08.2013 removed the birth date from the Правила формирования идентификационного номера, and the Minister of Internal Affairs states that a mismatch between the IIN and the holder's date of birth is not an error and is no ground to refuse service; the migration police add that the IIN must be treated as one whole number, not decomposed. A real DateTime.TryParseExact would therefore reject legitimately issued numbers. Separately, this makes the EXISTING month 1-12 / day 1-31 gate (rows 901301300108 and 900700300108 assert it) doctrinally indefensible too - but removing it is a behaviour change on a secondary source and well outside a defect fix, so I left it. Whoever owns KNOWN-ISSUES should record that this item is closed as won't-fix rather than as unsourced. https://ru.wikipedia.org/wiki/Индивидуальный_идентификационный_номер
- ValidateEntity (BIN) checks length and check digit only; the published BIN structure constrains the 5th digit to 4/5/6 and the 6th to 0/1/2/3, so e.g. 12-digit numbers with a 5th digit of 0-3 or 7-9 are accepted as BINs.
  - *Not fixed:* Not in my assignment or in KNOWN-ISSUES, and I could only source it from secondary accounting portals (adilet.zan.kz renders its documents in JS and returns no text to a fetch, so I could not read Приказ МВД РК от 29.06.2023 itself). Tightening on that basis risks false negatives, and the same 2013 reasoning that kills the IIN date rule may or may not extend to the BIN - I could not establish which. Reported for visibility; the BIN structure is now at least documented in the test file's comment.

## Korea

- The lower date bound `datetime < new DateTime(1860, 1, 1)` is stricter than python-stdnum, which accepts any valid 18xx date for S digits 9/0.
  - *Not fixed:* Unsourced in either direction and unreachable for any living person (it only rejects births in 1800-1859). Removing it would widen accepted values on no authority; left alone deliberately.

## Korea, Pakistan

- ValidateEntity and ValidateVAT still throw NotSupportedException instead of returning ValidationResult.Invalid("Not supported"), which the hard rules require.
  - *Not fixed:* I made the change, then found the caller that breaks and reverted it. Attest/CountryValidator.cs:290 (Supports) uses the throw as its ONLY signal for "this country has no rule for this kind": it calls ValidateOnValidator inside a try and returns false only from `catch (Exception e) when (e is NotSupportedException || e is NotImplementedException)`. Converting two of the 11 throwing validators would make Supports(Country.KR, IdentifierKind.CompanyNumber) return true - a wrong answer from a public API documented as "Whether this country has a rule for the given kind" - and the fix lives in CountryValidator.cs, a forbidden shared file. Nothing escapes to facade callers today: ValidateEntity/ValidateVAT/Validate all catch the throw and return exactly Invalid("Not supported"). This needs a wave that can change all 11 validators and Supports together. I pinned the current behaviour with Assert.Throws tests instead, matching UnitedStatesValidatorTests.cs:45.

## Macedonia

- ValidateVAT still strips the MK/МК prefix with an unanchored String.Replace, so '1MK234567890123' has the letters removed from the middle.
  - *Not fixed:* KNOWN-ISSUES already records this as a codebase-wide pattern (Norway entry: ~20 validators) deliberately left for a wave that can touch all of them at once. Fixing it in one file would make the library inconsistent, and no test asserts the false positive.

## Malta

- ValidateVAT computes the check digits as `37 - sum % 37`, yielding 37 when the weighted sum is a multiple of 37, so a number whose check digits are '00' is rejected.
  - *Not fixed:* INVESTIGATED AND SETTLED — this is not a defect, so I made no behavioural change. The task asked me to decide from the spec whether such a number is invalid or the check digits are 00. The premise that 37 cannot be a check pair is wrong: the pair is a two-digit number in 00-99, and 37 fits. Three independent implementations compute it as `37 - sum % 37` and therefore accept '…37' and reject '…00' — vatdb, the Braemoor-derived algorithm, and vat-validator, whose code handles precisely this case on purpose: `return (r == 0 and c7_c8 == 37) or (c7_c8 == r)`. python-stdnum does not contradict them: it weights all eight digits 3,4,6,7,8,9,10,1 and asks for a multiple of 37, which is the same test loosened into a congruence, so it accepts the check value plus 37 and plus 74 indiscriminately — I verified in python3 that it accepts 11679149 and 11679186 as aliases of the correct 11679112, which Malta certainly never issues. stdnum accepting '00' is an artefact of that looseness, not a statement that the IRD emits '00'. Answer: the check digits are 37 and '00' is invalid; the current arithmetic is right. I recorded the reasoning and the vat-validator citation in the ValidateVAT doc comment and added one regression row, "10001737" true (3+0+0+0+8+63 = 74 = 2x37), which is exactly the row that would break if a future wave changed the formula to `(37 - sum % 37) % 37`.

## Mauritius

- ValidatePostalCode still throws NotSupportedException instead of returning ValidationResult.Invalid("Not supported").
  - *Not fixed:* I made the change, then reverted it. CountryValidator.Supports (Attest/CountryValidator.cs:292-301) catches NotSupportedException/NotImplementedException as its sentinel for "this country has no rule for this kind" — its own comment says so, and CountryValidatorKindsTests.SupportsDistinguishesNoRuleFromWrongValue asserts on it. Returning an Invalid result would make Supports(Country.MU, IdentifierKind.PostalCode) claim true for a method that can never succeed, which is a worse lie than the throw and is a shared-facade decision, not a Mauritius one. The facade already converts the throw to Invalid("Not supported") for callers. I left an XML comment on the method saying why it throws. Separately: Mauritius does have postal codes (five digits, or a letter and four digits for an island) but I found that only on Wikipedia, not on a Mauritius Post page, so I did not encode it.
- Neither the BRN nor the VAT registration number is checksum-validated, and the identity card date still accepts 29 February in non-leap years.
  - *Not fixed:* No check-digit algorithm is published for either business number — the MRA register gives the shapes but nothing derivable. For 29 February the card carries no century digit, so rejecting it would need a guessed pivot year; accepting it can only be too loose, never a false reject.

## Moldova

- The leading registry digit of the IDNP is still unenforced, so an IDNO or IDNV with a correct checksum passes ValidateIndividualTaxCode.
  - *Not fixed:* The only source for the accepted prefixes is idnx-validator, which allows both 2xxxxxxxxxxxx and 09xxxxxxxxxxx (the second presumably for foreigners/refugees). Encoding a prefix rule on that evidence alone risks rejecting real IDNPs, and false rejections are worse than the current over-acceptance. The checksum was the sourced half; I stopped there.

## Montenegro

- Which number ValidateIndividualTaxCode should accept is still not settled by a primary source. I could not find a Montenegrin law or rulebook that states the natural person's PIB in as many words; the evidence I have is Zakon o poreskoj administraciji clan 27 (PIB assigned to legal and natural persons) plus the DRI audit report's statement that the tax authority's PIB for preduzetnici 'se zasniva na JMBG'. I chose the JMB, consistent with Serbia, Bosnia and Macedonia in this repo. If a Montenegrin sole trader's tax number is in fact the 8-digit PIB, this method will false-reject it; a future wave with a primary source may need to accept both.
  - *Not fixed:* Sourced only indirectly. I implemented the better-evidenced of the two readings rather than throwing, and the test rows only assert JMBs that are also valid under the JMBG rule the file already encodes.

## Nigeria

- No check digit is validated for any identifier — NIN, any TIN form, or the postal code.
  - *Not fixed:* None is published. The OECD profile gives structure only; taxdo.com states outright that any checksum in the TIN is "not publicly disclosed"; NIMC publishes no NIN check digit; python-stdnum has no NG module. Inventing one was not an option.
- The 13-digit Tax ID is accepted on length alone, and I cannot tell whether it embeds the 11-digit NIN or the CAC number.
  - *Not fixed:* Every source — including the FCT-IRS and LIRS notices — says only "13 digit" and "linked to"/"generated from" the NIN or CAC number. None states whether the source identifier is embedded, prefixed or merely mapped. A length check is what the published record supports.
- The four digits after the hyphen in a FIRS TIN are accepted as any \d{4}, not just 0001.
  - *Not fixed:* The OECD profile literally writes the structure as "eight digits, a hyphen then the 0001" but never explains the suffix. Branch/office TINs with other suffixes are widely reported and taxid.pro gives the pattern as \d{8}-\d{4}. Constraining to 0001 on an unexplained example would reject live branch numbers, which is the costly direction.
- ValidateEntity does not validate a CAC RC/BN company registration number.
  - *Not fixed:* Out of scope for this assignment and arguably for the method: the CAC number is a company-register identifier, not a tax identifier, and under the 2025 Act it is the input from which the corporate Tax ID is generated rather than the Tax ID itself. Adding it would change what ValidateEntity means for NG relative to every other country.

## Norway

- The unanchored-prefix-strip pattern still exists in roughly twenty other validators (France, Latvia, Lithuania, Slovenia and others).
  - *Not fixed:* Those files belong to other agents in this shared working tree and the hard rules forbid touching them. I fixed both instances that fell inside my assignment — Norway and Turkey, which KNOWN-ISSUES had filed together under the Norway entry — so the two are now consistent with each other and with MaltaValidator, which already used the anchored StartsWith/Substring form. Suggest a sweep replacing `.Replace("XX", string.Empty)` with that form across the remaining validators.

## OTHER COUNTRIES (codebase-wide pattern, outside my files)

- The \d-matches-Unicode-Nd + int.Parse-throws combination is not Argentina-specific. Any validator that guards with "^\\d{N}$" (or \d inside a longer pattern) and then calls int.Parse/Convert on the characters will throw FormatException on Eastern-Arabic, Devanagari or fullwidth digits, because RemoveSpecialCharacthers preserves them. Grep showed the "^\\d{4}$" form in at least Belgium, Mexico and Paraguay postal codes (harmless there — no parse follows), but every checksum validator using \d before a parse is a candidate crash.
  - *Not fixed:* Those files belong to other agents in this shared working tree; the hard rules forbid touching them. Suggest a sweep for `\d` in validators that parse digits afterwards, replacing the class with [0-9].

## Pakistan

- No entity or VAT rule exists - the FBR's National Tax Number and sales tax registration number have no published format or check digit.
  - *Not fixed:* python-stdnum has only pk.cnic for Pakistan (confirmed against the 2.1 module index); the FBR publishes no NTN algorithm. Inventing one would reject live numbers.

## Peru

- Taxpayer type 17 is accepted as both a personal and a company RUC, so it is the one Peruvian input that still reports IsAmbiguous.
  - *Not fixed:* SUNAT's structure description lists only 10, 15 and 20. python-stdnum accepts 17 as a RUC type but says nothing about who holds it, and the secondary pages that do contradict each other ("no domiciliados" vs "carné de FFAA" vs "carné de extranjería"), none citing a SUNAT norm. Assigning 17 to one side on that basis would risk rejecting a live number, so it stays valid for both. If SUNAT ever publishes the type, the fix is one string moved between _personTypes and _entityTypes.
- Attest/IdentifierResult.cs line 34 still names Peru in the IsAmbiguous XML doc as a country that "issues one number for both". That is now stale for everything except type 17.
  - *Not fixed:* IdentifierResult.cs is on the forbidden shared-file list. Suggested edit for whoever owns it: drop "Peru" from that list, or qualify it — the RUC's leading two digits distinguish the holder, and only the unsourced type 17 remains ambiguous.
- ValidateVAT still delegates to ValidateEntity, so a natural person's RUC (type 10/15) is rejected as a VAT number even though Peru files IGV under that same RUC.
  - *Not fixed:* Deliberate, and now documented in the method's XML comment. Peru has no separate VAT number, so the only question is which reading IdentifierKind.Vat carries — and this library places Vat under IdentifierKind.Business. Making ValidateVAT accept every RUC would keep every personal RUC matching a Business kind and so leave IsAmbiguous true for all of them, defeating the fix. Flag it if the intended reading of Vat is "any IGV registration" rather than "a business VAT identifier"; it is a one-line change.
- The shared KNOWN-ISSUES entry about All(char.IsDigit) accepting non-ASCII Unicode digits.
  - *Not fixed:* Already resolved inside this file by an earlier wave — ValidateNationalIdentity, the RUC path and ValidatePostalCode all guard with Regex "^[0-9]...", and both checksum helpers run only after that guard. Nothing left to do here; the remaining call sites are other agents' files and the root-cause helper belongs in the forbidden Attest/IdExtensions.cs.

## Philippines

- python-stdnum PR #349 also accepts 13-digit TINs; my regex accepts only 9, 12 and 14 digits, so the existing test row "1234567890000" (13 digits) stays false.
  - *Not fixed:* I found no BIR source for a 4-digit branch code. The BIR's own wording sources 9 (core), 12 (3-digit branch) and 14 (5-digit branch); PR #349 is an unmerged community proposal and I would not widen the accepted set on it alone.

## Portugal

- `public int CheckSum(string value)` multiplies the character CODE by the weight instead of the digit, and throws NullReferenceException on null.
  - *Not fixed:* Unchanged, and the earlier wave's reasoning holds: it is public API but not a Validate* method, so the never-throw rule as written does not reach it, and the ASCII bias cancels mod 11 on its only internal call path (48 x 44 = 2112, and 2112 % 11 == 0) so ValidateBilhetedeIdentidade is correct. Changing a public signature is outside this assignment. Note for a future wave: it is also public surface that a caller could reach with any length and get garbage.
- ValidateCartaoCidadao positions 9 and 10 accept digits as well as letters, so the 'two-letter document version' is not enforced.
  - *Not fixed:* Deliberately not tightened. python-stdnum's own pattern for those two positions is [A-Z0-9], not [A-Z], and stdnum is the authority named in my instructions. Requiring letters would diverge from it and risk rejecting real cards, and I found no official Portuguese statement on the version field's character set. The new guard matches stdnum exactly.

## Russia

- ValidateBIK never checks the leading country code, and its 000-002 / 050-999 guard on the last three digits is itself not backed by the current regulation.
  - *Not fixed:* I re-checked the authoritative source rather than the bank blogs the earlier wave used. Appendix 6 of Положение Банка России от 06.07.2017 № 595-П defines the БИК as position 1 = participation type (0 direct, 1 indirect, 2 non-participant client) and positions 2-9 = a free participant identifier 00000001..99999999 — no fixed "04" prefix and no reserved range for the last three digits (https://www.consultant.ru/document/cons_doc_LAW_280683/24a8cab5291d2517c8fdaeee243a44901d717408/). That contradicts the classic layout the current code and its tests encode, and real post-2021 БИКs (e.g. 004525988, 017003983) happen to satisfy both readings, so neither adding a prefix guard nor removing the existing range guard can be justified without evidence about which values are actually assigned. Left alone; the earlier wave's "unsettled" call stands and is now better evidenced.
- ValidateBIK and ValidateOGRN report InvalidChecksum for range/structure violations.
  - *Not fixed:* Cosmetic, and the fix needs a new result kind on the shared Attest/ValidationResult.cs, which is off limits.
- README.md line 57 and the XML doc on IdentifierResult.IsAmbiguous (Attest/IdentifierResult.cs line 35) both still list Russia among the countries that issue one number for both roles.
  - *Not fixed:* Both files are on the hard no-touch list. They are now stale: Russia no longer reports IsAmbiguous for a company ИНН. The 12-digit personal form is still ambiguous (PersonalTaxCode + Vat), so Russia is not simply removable from the list — the wording needs to narrow to the entrepreneur case. Flagging for whoever owns those files.

## Taiwan

- ValidatePostalCode accepts any 3/5/6 digits, including 000 and 999, where real district codes run roughly 100-983.
  - *Not fixed:* Every ValidatePostalCode in this repo is a bare format regex and the district list drifts as districts are merged; same reasoning the earlier wave gave for Turkey and the Faroes. Not worth diverging in one file.
- The UBN '7' exception makes two different check digits valid for the same first seven digits (12345670 and 12345675 both pass).
  - *Not fixed:* That is the published rule, not a defect: 7 x weight 4 = 28, whose digit sum may be counted as 10 or as 1. python-stdnum behaves identically. Encoded as-is and covered by a test row.

## Thailand

- Attest.Tests/CountryValidatorKindsTests.cs:44-53 CountriesThatIssueOneNumberForBothReportAmbiguity WILL NOW FAIL. It uses 0107537001510 (a DBD company number) as the example of an ambiguous value and asserts IsAmbiguous is true and that Matched contains PersonalTaxCode. After this fix that number matches CompanyNumber|Vat only, so IsAmbiguous is false.
  - *Not fixed:* Not one of my assigned files. Smallest correct repair, if you want to keep Thailand as the example: swap the value to the personal number 3100600445635, which still matches PersonalId|PersonalTaxCode|Vat and so still reports IsAmbiguous true — because a Thai sole trader's VAT registration genuinely is the personal number — and assert Matched contains IdentifierKind.Vat instead of PersonalTaxCode. Alternatively re-point it at Russia, Iceland, Peru, Andorra, Armenia or Nigeria.
- Attest/IdentifierResult.cs:34 and README.md:57 both name Thailand in the list of countries that 'cannot tell the two apart'. That is now false for company numbers: a Thai company number no longer matches any personal kind. Thailand still reports ambiguity for a personal number, but only via VAT, which is a real fact about sole-trader VAT registration rather than a limitation.
  - *Not fixed:* Both are on the forbidden shared-file list.
- The OECD sheet's carve-out 'Numbers 100-999 ... (except 601)' is not encoded, so a 601-prefixed number with a valid check digit is still accepted as a personal number.
  - *Not fixed:* Deliberate. The sheet does not say who, if anyone, holds 601, and rejecting it risks a false negative on a live personal number while gaining nothing for person/company discrimination — 601 belongs to neither side. Encoding it is a one-line change if you want the stricter reading.
- Residual false-negative risk on ValidateNationalIdentity: the Department of Provincial Administration also issues 13-digit numbers beginning with 0 to persons of undetermined registration status (the 'pink card' holders), which the OECD/RD structure does not account for and which can collide with the 010-096 DBD window. Those are rejected as national identity numbers now.
  - *Not fixed:* Both sourced references say a personal number is 100-999 (OECD Section II) and that leading 0 is 'not found on cards of Thai nationals' (Wikipedia, Thai identity card); python-stdnum rejects 0 for PIN too. Accepting leading 0 as personal would re-create exactly the ambiguity this task asked me to remove, since every company number starts with 0. Flagging rather than guessing.

## Ukraine

- ValidateVAT still accepts any 12 digits with no checksum, and rejects the legacy 10-digit ІПН.
  - *Not fixed:* The 12-digit check digit is genuinely unpublished — uk.wikipedia quotes the rule as '12-й знак — контрольний розряд, алгоритм формування якого встановлює центральний орган державної податкової служби України', i.e. the tax authority sets it and does not publish it. I did resolve the 'sources contradict' half of the KNOWN-ISSUES entry, and recorded it in the code comment: the 12-digit ІПН is 7 ЄДРПОУ digits (without the ЄДРПОУ's own check digit) + 2 oblast + 2 district + 1 check digit for a legal entity, and RNOKPP + 2 control digits for an individual. Separately, per LIGA:ZAKON citing Minfin order no. 30 of 29.01.2020 (amending Regulation no. 1130 of 14.11.2014), sole traders registered for VAT before 09.03.2020 keep a 10-digit ІПН that stays valid until their VAT registration is cancelled — so the 12-digit-only rule does produce false negatives. I did not widen it: that is a format/scope change rather than a check-digit fix, it loosens validation, and it rests on a secondary source. Flagging it for a wave that owns the format question.

## UnitedArabEmirates

- ValidateNationalIdentity checks only the 784 + 12 digit shape; the trailing check digit is never verified, so 784198012345670..9 are all accepted.
  - *Not fixed:* ICP/u.ae publish the 15-digit format but no check digit algorithm. The widely circulated Luhn recipe is explicitly untested by its own author and is reported to fail real cards whose check digit is 0, so encoding it would reject valid IDs. The valid samples in the test file were nevertheless chosen to carry the Luhn digit of their first 14 digits, so they survive if a checksum is added later.

## Uzbekistan

- ValidateEntity and ValidateVAT still throw NotSupportedException instead of returning ValidationResult.Invalid("Not supported").
  - *Not fixed:* This is a deliberate repo-wide design, not a local defect: 11 validators in Attest/CountriesValidators throw, CountryValidator.cs catches NotSupportedException/NotImplementedException at seven call sites and maps them to Invalid("Not supported"), and Attest.Tests/CountriesValidators/CountryValidatorTests.cs tests exactly that contract with a comment saying a throwing validator 'must be answered exactly like an unregistered country (XX), not escape the facade'. Changing only Uzbekistan would break that consistency without fixing anything the facade does not already handle. Note also that Uzbek legal entities do have a 9-digit INN/STIR, but I found no published check digit for it, so implementing the methods would be unsourced new functionality.
