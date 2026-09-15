# Known weaknesses

What is left after the repair waves, and why. None of these is a wrong verdict on a well-formed
number — they are validators that check less than the country publishes, or that are stricter than
the spec in a narrow case. Three labels:

- ***Not fixed*** — a real gap nobody has closed. Almost every one is a check digit the issuing
  authority does not publish. Inventing one rejects live numbers and nobody reports it to you, so
  the format and the field ranges are validated and the gap is written down instead.
- ***Won't fix*** — investigated and decided against: the rule was sourced and says not to make the
  change, or the sources contradict each other and any choice would reject what the others describe.
- ***Settled*** — not a defect. The behaviour is what the country publishes.

Anyone picking one of these up: confirm the rule from an official source or python-stdnum first, and
add the test before the fix.

## Armenia

- The 8th digit of the TIN is a check digit, but only the length is validated.
  - *Not fixed:* Still unsourced. The SRC's OECD sheet states the check digit exists and is calculated from the first seven digits but gives no algorithm, and nothing published by the SRC, python-stdnum (no stdnum/am module — confirmed against the repository's module listing) or any secondary guide supplies one. Inventing one would reject live numbers.
- The 10th digit of the public services number (the 'specifying digit') is not verified.
  - *Not fixed:* Law HO-288-N art. 4 part 2 and decision N 1783-N annex 1 both say the calculation order for that digit is established by the authorised body (the migration/population-register authority, formerly the Ministry of Social Security), and that procedure is not published. Format and field ranges are enforced; the check digit is not.
- The unshifted June month code (06, 26, 46, 66, 86) is accepted alongside the shifted one (14, 34, 54, 74, 94).
  - *Won't fix:* The acts are self-contradictory here: they state the range as 01-12 (which contains 06) and then say June is coded 14. I could not establish which reading governs issued numbers, so the validator accepts both rather than risking a false negative on live June-born numbers. No test asserts either way. The acts are self-contradictory and accepting both codings cannot false reject; picking one would.

## Azerbaijan

- VÖEN validation is length-only: the 9th digit is documented as an algorithmic check digit and the 10th as a taxpayer type marker (1 legal person, 2 natural person), and neither is enforced.
  - *Not fixed:* The check digit algorithm is not published, and the type-digit constraint rests on a single secondary source (lookuptax), so enforcing it risks rejecting live numbers.

## Bahrain

- ValidateIndividualTaxCode checks only '9 digits'. The CPR's ninth digit is a check digit and is not verified.
  - *Not fixed:* The iGA does not publish the check digit algorithm and no authoritative source for it could be found; guessing one would reject valid CPRs.
- The YYMM birth prefix of the CPR is not validated, so '999912345' (month 99) is accepted as valid.
  - *Not fixed:* The documented format is YYMMNNNNC, but the same source notes a minority of citizens and residents hold personal numbers that do not follow it, so a month range check would reject real numbers. No test asserts either way.

## Bolivia

- The 7-13 digit bound I encoded is a union of secondary sources, not a SIN specification.
  - *Won't fix:* Fixed in the sense that the unbounded regex is gone, but flagging the residual uncertainty: the SIN publishes the generation rule (CI + 3 digits) and no total length, and the three secondary sources disagree on the maximum (10 / 12 / 13). I took the widest published bound so the change cannot reject anything any source endorses, and the reasoning is in a code comment. If a SIAT field spec turns up later the maximum should be tightened to it. The widest published bound is the only one that cannot reject a number some source endorses.

## Bosnia

- The RS rulebook (clan 11-13) also prescribes an inner check digit K at position 9, 'kontrolni broj po modulu 11 za prethodni osmocifreni dio broja'. It is not implemented: neither the EDB-style weights nor ISO 7064 MOD 11,10 reproduce position 9 on the official FBiH data (142/29319 and 180/29319 respectively), and the FBiH rulebook does not mention an inner check digit at all, so it may only exist for RS-issued numbers.
  - *Not fixed:* The weights are unpublished and I could not derive them with confidence from the data I had. Encoding a guess would reject real JIBs; the 13th-digit rule alone is verified on 29318/29319 official numbers.

## Brazil

- No text-readable Receita Federal source states that repeated-digit CPFs are invalid; the rule rests on two independent Brazilian implementations plus the arithmetic demonstration.
  - *Won't fix:* Fix applied anyway (it was the assignment), but flagging the citation quality: gov.br's own CPF material is either a scanned PDF or behind a restricted-content gate for fetching. Note the behaviour now deliberately diverges from python-stdnum, which rejects only the all-zero CPF via `int(number) <= 0`. The code comment says so explicitly. The rule is implemented; only the citation quality is flagged.

## Cyprus

- ValidateNationalIdentity checks format only (10 digits), and the question of whether TICs in the 6xxxxxxx range use a revised check-character algorithm after the Tax For All migration is unresolved.
  - *Won't fix:* Unchanged from the earlier wave and still correct: no Cypriot government source publishes an identity-card check digit, and no specification of a second VAT check-character algorithm exists. python-stdnum applies one algorithm to all CY numbers. Nothing new found this pass; I did not touch either method. Re-checked this pass and unchanged: no Cypriot source publishes an identity card check digit.

## DominicanRepublic

- The _validCedula / _validRnc whitelists are consulted before the length check and contain entries shorter than the format (two 10-digit cedulas, one 8-digit RNC), so ValidateIndividualTaxCode("0094662667") is valid at 10 characters.
  - *Won't fix:* Unchanged from the earlier wave's finding and still correct: this is a faithful port - python-stdnum's own cedula.py/rnc.py whitelists carry those exact short tokens and stdnum also checks the whitelist before the length. "Fixing" it would diverge from the reference data on numbers the DGII apparently does issue. Diverging from the reference data would reject numbers the DGII apparently does issue.

## FaroeIslands

- ValidateIndividualTaxCode may reject the identification number TAKS issues to a non-resident who is taxable in the Faroes.
  - *Not fixed:* The OECD sheet says that number's structure is "similar, but not identical to the P number" and no source publishes it -- not OECD, norden.org, lookuptax, torshavn.fo, nor the TAKS temporary p-tal application form. If it offsets the day the way Danish erstatningsnumre (+60) or Norwegian D-numbers (+40) do, the calendar check now in place would reject it. Widening it is a one-line regex change once the format is known.

## Georgia

- The 11-digit personal number's trailing verification digits are still unchecked; ValidateIndividualTaxCode remains a 9-or-11-digit format guard.
  - *Not fixed:* No algorithm published, so the earlier wave's reason stands. python-stdnum has no ge module. Sources also contradict each other on what the trailing digits even are: lookuptax.com says 'the last two digits are used for verification purposes', another says the last three are randomly generated, and tin-check.com states validation is performed against the Revenue Service registry rather than by formula. Nothing to encode.

## Iceland

- ValidateVAT accepts any 5-6 digit VSK number with no structural or check-digit validation.
  - *Won't fix:* Not part of the assignment and not researched; reported only so it is not mistaken for something this pass verified. Checked this pass: the VSK is a separate 5-6 digit number from the kennitala and no source -- Skatturinn, vatify, lookuptax -- publishes a check digit or any structure inside it. The format is all there is to validate.

## Kazakhstan

- ValidateIndividualTaxCode only range-checks the birth date (month 1-12, day 1-31), so 900231... (31 February) passes.
  - *Won't fix:* Not unsourced — sourced against the fix. Постановление Правительства РК № 853 of 26.08.2013 removed the birth date from the Правила формирования идентификационного номера, and the Minister of Internal Affairs states that a mismatch between the IIN and the holder's date of birth is not an error and is no ground to refuse service; the migration police add that the IIN must be treated as one whole number, not decomposed. A real DateTime.TryParseExact would therefore reject legitimately issued numbers. Separately, this makes the EXISTING month 1-12 / day 1-31 gate (rows 901301300108 and 900700300108 assert it) doctrinally indefensible too - but removing it is a behaviour change on a secondary source and well outside a defect fix, so I left it. https://ru.wikipedia.org/wiki/Индивидуальный_идентификационный_номер
- ValidateEntity (BIN) checks length and check digit only; the published BIN structure constrains the 5th digit to 4/5/6 and the 6th to 0/1/2/3, so e.g. 12-digit numbers with a 5th digit of 0-3 or 7-9 are accepted as BINs.
  - *Not fixed:* Not in my assignment or in KNOWN-ISSUES, and I could only source it from secondary accounting portals (adilet.zan.kz renders its documents in JS and returns no text to a fetch, so I could not read Приказ МВД РК от 29.06.2023 itself). Tightening on that basis risks false negatives, and the same 2013 reasoning that kills the IIN date rule may or may not extend to the BIN - I could not establish which. Reported for visibility; the BIN structure is now at least documented in the test file's comment.

## Korea

- The lower date bound `datetime < new DateTime(1860, 1, 1)` is stricter than python-stdnum, which accepts any valid 18xx date for S digits 9/0.
  - *Won't fix:* Unsourced in either direction and unreachable for any living person (it only rejects births in 1800-1859). Removing it would widen accepted values on no authority; left alone deliberately. It is unreachable for anyone living, and widening it on no authority would be the guess.

## Malta

- ValidateVAT computes the check digits as `37 - sum % 37`, yielding 37 when the weighted sum is a multiple of 37, so a number whose check digits are '00' is rejected.
  - *Settled:* INVESTIGATED AND SETTLED — this is not a defect, so I made no behavioural change. The task asked me to decide from the spec whether such a number is invalid or the check digits are 00. The premise that 37 cannot be a check pair is wrong: the pair is a two-digit number in 00-99, and 37 fits. Three independent implementations compute it as `37 - sum % 37` and therefore accept '…37' and reject '…00' — vatdb, the Braemoor-derived algorithm, and vat-validator, whose code handles precisely this case on purpose: `return (r == 0 and c7_c8 == 37) or (c7_c8 == r)`. python-stdnum does not contradict them: it weights all eight digits 3,4,6,7,8,9,10,1 and asks for a multiple of 37, which is the same test loosened into a congruence, so it accepts the check value plus 37 and plus 74 indiscriminately — I verified in python3 that it accepts 11679149 and 11679186 as aliases of the correct 11679112, which Malta certainly never issues. stdnum accepting '00' is an artefact of that looseness, not a statement that the IRD emits '00'. Answer: the check digits are 37 and '00' is invalid; the current arithmetic is right. I recorded the reasoning and the vat-validator citation in the ValidateVAT doc comment and added one regression row, "10001737" true (3+0+0+0+8+63 = 74 = 2x37), which is exactly the row that would break if a future wave changed the formula to `(37 - sum % 37) % 37`. Not a defect: the arithmetic is right and three independent implementations agree.

## Mauritius

- Neither the BRN nor the VAT registration number is checksum-validated, and the identity card date still accepts 29 February in non-leap years.
  - *Not fixed:* No check-digit algorithm is published for either business number — the MRA register gives the shapes but nothing derivable. For 29 February the card carries no century digit, so rejecting it would need a guessed pivot year; accepting it can only be too loose, never a false reject.

## Moldova

- The leading registry digit of the IDNP is still unenforced, so an IDNO or IDNV with a correct checksum passes ValidateIndividualTaxCode.
  - *Won't fix:* The only source for the accepted prefixes is idnx-validator, which allows both 2xxxxxxxxxxxx and 09xxxxxxxxxxx (the second presumably for foreigners/refugees). Encoding a prefix rule on that evidence alone risks rejecting real IDNPs, and false rejections are worse than the current over-acceptance. The checksum was the sourced half; I stopped there. Re-checked this pass and the sources contradict each other outright: one guide says an IDNP opens with the birth year 19 or 20, idnx-validator allows 2 and 09, and the OECD sheet gives no prefix rule at all. Encoding any of them would reject the numbers the others describe.

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

## Pakistan

- No entity or VAT rule exists - the FBR's National Tax Number and sales tax registration number have no published format or check digit.
  - *Not fixed:* python-stdnum has only pk.cnic for Pakistan (confirmed against the 2.1 module index); the FBR publishes no NTN algorithm. Inventing one would reject live numbers.

## Philippines

- python-stdnum PR #349 also accepts 13-digit TINs; my regex accepts only 9, 12 and 14 digits, so the existing test row "1234567890000" (13 digits) stays false.
  - *Won't fix:* I found no BIR source for a 4-digit branch code. The BIR's own wording sources 9 (core), 12 (3-digit branch) and 14 (5-digit branch); PR #349 is an unmerged community proposal and I would not widen the accepted set on it alone. Confirmed this pass against the BIR's own structure and the 2026 branch-code expansion: 9 digits, 12 with a 3 digit branch and 14 with a 5 digit branch are what is issued. Nothing published describes a 13 digit TIN, and PR #349 is an unmerged community proposal.

## Portugal

- ValidateCartaoCidadao positions 9 and 10 accept digits as well as letters, so the 'two-letter document version' is not enforced.
  - *Won't fix:* Deliberately not tightened. python-stdnum's own pattern for those two positions is [A-Z0-9], not [A-Z], and stdnum is the authority named in my instructions. Requiring letters would diverge from it and risk rejecting real cards, and I found no official Portuguese statement on the version field's character set. The new guard matches stdnum exactly. The guard matches python-stdnum exactly, which is the authority named for this library.

## Russia

- ValidateBIK never checks the leading country code, and its 000-002 / 050-999 guard on the last three digits is itself not backed by the current regulation.
  - *Not fixed:* I re-checked the authoritative source rather than the bank blogs the earlier wave used. Appendix 6 of Положение Банка России от 06.07.2017 № 595-П defines the БИК as position 1 = participation type (0 direct, 1 indirect, 2 non-participant client) and positions 2-9 = a free participant identifier 00000001..99999999 — no fixed "04" prefix and no reserved range for the last three digits (https://www.consultant.ru/document/cons_doc_LAW_280683/24a8cab5291d2517c8fdaeee243a44901d717408/). That contradicts the classic layout the current code and its tests encode, and real post-2021 БИКs (e.g. 004525988, 017003983) happen to satisfy both readings, so neither adding a prefix guard nor removing the existing range guard can be justified without evidence about which values are actually assigned. Left alone; the earlier wave's "unsettled" call stands and is now better evidenced.

## Taiwan

- The UBN '7' exception makes two different check digits valid for the same first seven digits (12345670 and 12345675 both pass).
  - *Settled:* That is the published rule, not a defect: 7 x weight 4 = 28, whose digit sum may be counted as 10 or as 1. python-stdnum behaves identically. Encoded as-is and covered by a test row. Not a defect: that is the published rule, and python-stdnum behaves identically.

## Thailand

- Residual false-negative risk on ValidateNationalIdentity: the Department of Provincial Administration also issues 13-digit numbers beginning with 0 to persons of undetermined registration status (the 'pink card' holders), which the OECD/RD structure does not account for and which can collide with the 010-096 DBD window. Those are rejected as national identity numbers now.
  - *Won't fix:* Both sourced references say a personal number is 100-999 (OECD Section II) and that leading 0 is 'not found on cards of Thai nationals' (Wikipedia, Thai identity card); python-stdnum rejects 0 for PIN too. Accepting leading 0 as personal would re-create exactly the ambiguity this task asked me to remove, since every company number starts with 0. Flagging rather than guessing. Accepting a leading zero as personal would re-create the ambiguity with company numbers that the holder-type work removed, and all three sources say a personal number is 100-999.

## Ukraine

- ValidateVAT accepts the twelve digit IPN on its format alone; the twelfth digit is a check digit and is not verified.
  - *Not fixed:* Genuinely unpublished. uk.wikipedia quotes the rule as "12-й знак -- контрольний розряд, алгоритм формування якого встановлює центральний орган державної податкової служби України": the tax authority sets the algorithm and does not publish it. The composition either side of that digit is now recorded in the code comment, and the ten digit legacy form -- whose check digit *is* published -- is validated in full.

## UnitedArabEmirates

- ValidateNationalIdentity checks only the 784 + 12 digit shape; the trailing check digit is never verified, so 784198012345670..9 are all accepted.
  - *Not fixed:* ICP/u.ae publish the 15-digit format but no check digit algorithm. The widely circulated Luhn recipe is explicitly untested by its own author and is reported to fail real cards whose check digit is 0, so encoding it would reject valid IDs. The valid samples in the test file were nevertheless chosen to carry the Luhn digit of their first 14 digits, so they survive if a checksum is added later.

