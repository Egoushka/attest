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

- **KI-AM-1** The 8th digit of the TIN is a check digit, but only the length is validated.
  - *Not fixed:* Still unsourced. The SRC's OECD sheet states the check digit exists and is calculated from the first seven digits but gives no algorithm, and nothing published by the SRC, python-stdnum (no stdnum/am module — confirmed against the repository's module listing) or any secondary guide supplies one. Inventing one would reject live numbers.
- **KI-AM-2** The 10th digit of the public services number (the 'specifying digit') is not verified.
  - *Not fixed:* Law HO-288-N art. 4 part 2 and decision N 1783-N annex 1 both say the calculation order for that digit is established by the authorised body (the migration/population-register authority, formerly the Ministry of Social Security), and that procedure is not published. Format and field ranges are enforced; the check digit is not.
- **KI-AM-3** The unshifted June month code (06, 26, 46, 66, 86) is accepted alongside the shifted one (14, 34, 54, 74, 94).
  - *Won't fix:* The acts are self-contradictory here: they state the range as 01-12 (which contains 06) and then say June is coded 14. I could not establish which reading governs issued numbers, so the validator accepts both rather than risking a false negative on live June-born numbers. No test asserts either way. The acts are self-contradictory and accepting both codings cannot false reject; picking one would.

## Azerbaijan

- **KI-AZ-1** VÖEN validation is length-only: the 9th digit is documented as an algorithmic check digit and the 10th as a taxpayer type marker (1 legal person, 2 natural person), and neither is enforced.
  - *Not fixed:* The check digit algorithm is not published, and the type-digit constraint rests on a single secondary source (lookuptax), so enforcing it risks rejecting live numbers.

## Bahrain

- **KI-BH-1** ValidateIndividualTaxCode checks only '9 digits'. The CPR's ninth digit is a check digit and is not verified.
  - *Not fixed:* The iGA does not publish the check digit algorithm and no authoritative source for it could be found; guessing one would reject valid CPRs.
- **KI-BH-2** The YYMM birth prefix of the CPR is not validated, so '999912345' (month 99) is accepted as valid.
  - *Not fixed:* The documented format is YYMMNNNNC, but the same source notes a minority of citizens and residents hold personal numbers that do not follow it, so a month range check would reject real numbers. No test asserts either way.

- **KI-BH-3** ValidateVAT checks only 15 digits; the VAT account number has no published structure or check digit.
  - *Not fixed:* The NBR's VAT FAQ (https://www.nbr.gov.bh/vat_faqs/2, Arabic) is quoted by a web search as describing a private number of 15 digits. The page returned 403 to a direct fetch, so the wording was not read in full; treat the source as unverified. Some commercial guides say the number starts with 2; the NBR does not.
- **KI-BH-4** No company number rule: bh.company is NotCovered.
  - *Not fixed:* The commercial registration (CR) number format is not published by the Ministry of Industry and Commerce in anything I could read.

## Bolivia

- **KI-BO-1** The 7-13 digit bound I encoded is a union of secondary sources, not a SIN specification.
  - *Won't fix:* Fixed in the sense that the unbounded regex is gone, but flagging the residual uncertainty: the SIN publishes the generation rule (CI + 3 digits) and no total length, and the three secondary sources disagree on the maximum (10 / 12 / 13). I took the widest published bound so the change cannot reject anything any source endorses, and the reasoning is in a code comment. If a SIAT field spec turns up later the maximum should be tightened to it. The widest published bound is the only one that cannot reject a number some source endorses.

## Bosnia

- **KI-BA-1** The RS rulebook (clan 11-13) also prescribes an inner check digit K at position 9, 'kontrolni broj po modulu 11 za prethodni osmocifreni dio broja'. It is not implemented: neither the EDB-style weights nor ISO 7064 MOD 11,10 reproduce position 9 on the official FBiH data (142/29319 and 180/29319 respectively), and the FBiH rulebook does not mention an inner check digit at all, so it may only exist for RS-issued numbers.
  - *Not fixed:* The weights are unpublished and I could not derive them with confidence from the data I had. Encoding a guess would reject real JIBs; the 13th-digit rule alone is verified on 29318/29319 official numbers.

## Brazil

- **KI-BR-1** No text-readable Receita Federal source states that repeated-digit CPFs are invalid; the rule rests on two independent Brazilian implementations plus the arithmetic demonstration.
  - *Won't fix:* Fix applied anyway (it was the assignment), but flagging the citation quality: gov.br's own CPF material is either a scanned PDF or behind a restricted-content gate for fetching. Note the behaviour now deliberately diverges from python-stdnum, which rejects only the all-zero CPF via `int(number) <= 0`. The code comment says so explicitly. The rule is implemented; only the citation quality is flagged.

## China

- **KI-CN-1** No VAT rule: cn.vat is NotCovered.
  - *Not fixed:* A Chinese invoice carries the buyer's taxpayer identification number, which for an enterprise is the 18-character unified social credit code, for a person the 18-digit citizen ID, and older numbers are 15 to 20 characters. No single rule is published, and delegating to cn.company would reject the others.

## Cuba

- **KI-CU-1** No company or VAT rule: cu.company and cu.vat are NotCovered.
  - *Not fixed:* No published format for a company number was found. Cuba levies a sales tax and a services tax rather than VAT, but the government has announced a move to a creditable VAT, so cu.vat is not marked NotApplicable.

## Cyprus

- **KI-CY-1** ValidateNationalIdentity checks format only (10 digits), and the question of whether TICs in the 6xxxxxxx range use a revised check-character algorithm after the Tax For All migration is unresolved.
  - *Won't fix:* Unchanged from the earlier wave and still correct: no Cypriot government source publishes an identity-card check digit, and no specification of a second VAT check-character algorithm exists. python-stdnum applies one algorithm to all CY numbers. Nothing new found this pass; I did not touch either method. Re-checked this pass and unchanged: no Cypriot source publishes an identity card check digit.

## DominicanRepublic

- **KI-DO-1** The _validCedula / _validRnc whitelists are consulted before the length check and contain entries shorter than the format (two 10-digit cedulas, one 8-digit RNC), so ValidateIndividualTaxCode("0094662667") is valid at 10 characters.
  - *Won't fix:* Unchanged from the earlier wave's finding and still correct: this is a faithful port - python-stdnum's own cedula.py/rnc.py whitelists carry those exact short tokens and stdnum also checks the whitelist before the length. "Fixing" it would diverge from the reference data on numbers the DGII apparently does issue. Diverging from the reference data would reject numbers the DGII apparently does issue.

## FaroeIslands

- **KI-FO-1** ValidateIndividualTaxCode may reject the identification number TAKS issues to a non-resident who is taxable in the Faroes.
  - *Not fixed:* The OECD sheet says that number's structure is "similar, but not identical to the P number" and no source publishes it -- not OECD, norden.org, lookuptax, torshavn.fo, nor the TAKS temporary p-tal application form. If it offsets the day the way Danish erstatningsnumre (+60) or Norwegian D-numbers (+40) do, the calendar check now in place would reject it. Widening it is a one-line regex change once the format is known.

## UnitedKingdom

- **KI-GB-1** ValidateIndividualTaxCode (NINO) does not strip spaces, so "AB 12 34 56 C", the printed form, is rejected there while ValidateNationalIdentity accepts it.
  - *Not fixed:* Making it strip would move a verdict, which is a minor-release change of its own. gb.taxcode therefore prints compact; gb.nationalid prints the spaced form.

## Georgia

- **KI-GE-1** The 11-digit personal number's trailing verification digits are still unchecked; ValidateIndividualTaxCode remains a 9-or-11-digit format guard.
  - *Not fixed:* No algorithm published, so the earlier wave's reason stands. python-stdnum has no ge module. Sources also contradict each other on what the trailing digits even are: lookuptax.com says 'the last two digits are used for verification purposes', another says the last three are randomly generated, and tin-check.com states validation is performed against the Revenue Service registry rather than by formula. Nothing to encode.

## Guatemala

- **KI-GT-1** ValidateNationalIdentity (CUI) checks only 13 digits.
  - *Not fixed:* The RENAP law (decreto 39-2010) fixes the layout as 8 assigned digits, 1 check digit, 2 department digits, 2 municipality digits. RENAP publishes no check algorithm and no department or municipality list that I could find, so only the length is enforced.
- **KI-GT-2** No personal tax code rule: gt.taxcode is NotCovered, and the NIT rule of gt.company/gt.vat may reject newer individual NITs.
  - *Not fixed:* The SAT says NITs generated from 2023-10-04 for individuals are the first 9 digits of the CUI. The CUI's check digit is not published, and whether it equals the NIT's modulo 11 digit is unknown, so no rule is applied to gt.taxcode and the NIT check of the other two types is unverified for those numbers.

## HongKong

- **KI-HK-1** No company number rule: hk.company is NotCovered.
  - *Not fixed:* Companies Registry numbers and business registration numbers have no published structure or check digit that I could find.

## Iceland

- **KI-IS-1** ValidateVAT accepts any 5-6 digit VSK number with no structural or check-digit validation.
  - *Won't fix:* Not part of the assignment and not researched; reported only so it is not mistaken for something this pass verified. Checked this pass: the VSK is a separate 5-6 digit number from the kennitala and no source -- Skatturinn, vatify, lookuptax -- publishes a check digit or any structure inside it. The format is all there is to validate.

## Kazakhstan

- **KI-KZ-1** ValidateIndividualTaxCode only range-checks the birth date (month 1-12, day 1-31), so 900231... (31 February) passes.
  - *Won't fix:* Not unsourced — sourced against the fix. Постановление Правительства РК № 853 of 26.08.2013 removed the birth date from the Правила формирования идентификационного номера, and the Minister of Internal Affairs states that a mismatch between the IIN and the holder's date of birth is not an error and is no ground to refuse service; the migration police add that the IIN must be treated as one whole number, not decomposed. A real DateTime.TryParseExact would therefore reject legitimately issued numbers. Separately, this makes the EXISTING month 1-12 / day 1-31 gate (rows 901301300108 and 900700300108 assert it) doctrinally indefensible too - but removing it is a behaviour change on a secondary source and well outside a defect fix, so I left it. https://ru.wikipedia.org/wiki/Индивидуальный_идентификационный_номер
- **KI-KZ-2** ValidateEntity (BIN) checks length and check digit only; the published BIN structure constrains the 5th digit to 4/5/6 and the 6th to 0/1/2/3, so e.g. 12-digit numbers with a 5th digit of 0-3 or 7-9 are accepted as BINs.
  - *Not fixed:* Not in my assignment or in KNOWN-ISSUES, and I could only source it from secondary accounting portals (adilet.zan.kz renders its documents in JS and returns no text to a fetch, so I could not read Приказ МВД РК от 29.06.2023 itself). Tightening on that basis risks false negatives, and the same 2013 reasoning that kills the IIN date rule may or may not extend to the BIN - I could not establish which. Reported for visibility; the BIN structure is now at least documented in the test file's comment.

## Korea

- **KI-KR-1** The lower date bound `datetime < new DateTime(1860, 1, 1)` is stricter than python-stdnum, which accepts any valid 18xx date for S digits 9/0.
  - *Won't fix:* Unsourced in either direction and unreachable for any living person (it only rejects births in 1800-1859). Removing it would widen accepted values on no authority; left alone deliberately. It is unreachable for anyone living, and widening it on no authority would be the guess.

- **KI-KR-2** No company or VAT rule: kr.company and kr.vat are NotCovered. Both would be the 10-digit business registration number (3 tax office, 2 type, 4 serial, 1 check).
  - *Not fixed:* The National Tax Service publishes no check-digit algorithm that I could find. The weights 1 3 7 1 3 7 1 3 5 appear only on vendor blogs, so under rule 1 of AGENTS.md they are not used. The structure is also only on secondary sites.

## Malta

- **KI-MT-1** ValidateVAT computes the check digits as `37 - sum % 37`, yielding 37 when the weighted sum is a multiple of 37, so a number whose check digits are '00' is rejected.
  - *Settled:* INVESTIGATED AND SETTLED — this is not a defect, so I made no behavioural change. The task asked me to decide from the spec whether such a number is invalid or the check digits are 00. The premise that 37 cannot be a check pair is wrong: the pair is a two-digit number in 00-99, and 37 fits. Three independent implementations compute it as `37 - sum % 37` and therefore accept '…37' and reject '…00' — vatdb, the Braemoor-derived algorithm, and vat-validator, whose code handles precisely this case on purpose: `return (r == 0 and c7_c8 == 37) or (c7_c8 == r)`. python-stdnum does not contradict them: it weights all eight digits 3,4,6,7,8,9,10,1 and asks for a multiple of 37, which is the same test loosened into a congruence, so it accepts the check value plus 37 and plus 74 indiscriminately — I verified in python3 that it accepts 11679149 and 11679186 as aliases of the correct 11679112, which Malta certainly never issues. stdnum accepting '00' is an artefact of that looseness, not a statement that the IRD emits '00'. Answer: the check digits are 37 and '00' is invalid; the current arithmetic is right. I recorded the reasoning and the vat-validator citation in the ValidateVAT doc comment and added one regression row, "10001737" true (3+0+0+0+8+63 = 74 = 2x37), which is exactly the row that would break if a future wave changed the formula to `(37 - sum % 37) % 37`. Not a defect: the arithmetic is right and three independent implementations agree.

## Mauritius

- **KI-MU-1** Neither the BRN nor the VAT registration number is checksum-validated, and the identity card date still accepts 29 February in non-leap years.
  - *Not fixed:* No check-digit algorithm is published for either business number — the MRA register gives the shapes but nothing derivable. For 29 February the card carries no century digit, so rejecting it would need a guessed pivot year; accepting it can only be too loose, never a false reject.

## Monaco

- **KI-MC-1** No company, national identity or tax code rule: mc.company, mc.nationalid and mc.taxcode are NotCovered.
  - *Not fixed:* The Monegasque business register (RCI) number has no published rule, and I found nothing authoritative on a Monegasque personal number or personal tax number. Monaco VAT is the French VAT, which mc.vat already uses.

## Moldova

- **KI-MD-1** The leading registry digit of the IDNP is still unenforced, so an IDNO or IDNV with a correct checksum passes ValidateIndividualTaxCode.
  - *Won't fix:* The only source for the accepted prefixes is idnx-validator, which allows both 2xxxxxxxxxxxx and 09xxxxxxxxxxx (the second presumably for foreigners/refugees). Encoding a prefix rule on that evidence alone risks rejecting real IDNPs, and false rejections are worse than the current over-acceptance. The checksum was the sourced half; I stopped there. Re-checked this pass and the sources contradict each other outright: one guide says an IDNP opens with the birth year 19 or 20, idnx-validator allows 2 and 09, and the OECD sheet gives no prefix rule at all. Encoding any of them would reject the numbers the others describe.

## Montenegro

- **KI-ME-1** Which number ValidateIndividualTaxCode should accept is still not settled by a primary source. I could not find a Montenegrin law or rulebook that states the natural person's PIB in as many words; the evidence I have is Zakon o poreskoj administraciji clan 27 (PIB assigned to legal and natural persons) plus the DRI audit report's statement that the tax authority's PIB for preduzetnici 'se zasniva na JMBG'. I chose the JMB, consistent with Serbia, Bosnia and Macedonia in this repo. If a Montenegrin sole trader's tax number is in fact the 8-digit PIB, this method will false-reject it; a future wave with a primary source may need to accept both.
  - *Not fixed:* Sourced only indirectly. I implemented the better-evidenced of the two readings rather than throwing, and the test rows only assert JMBs that are also valid under the JMBG rule the file already encodes.

## Nigeria

- **KI-NG-1** No check digit is validated for any identifier — NIN, any TIN form, or the postal code.
  - *Not fixed:* None is published. The OECD profile gives structure only; taxdo.com states outright that any checksum in the TIN is "not publicly disclosed"; NIMC publishes no NIN check digit; python-stdnum has no NG module. Inventing one was not an option.
- **KI-NG-2** The 13-digit Tax ID is accepted on length alone, and I cannot tell whether it embeds the 11-digit NIN or the CAC number.
  - *Not fixed:* Every source — including the FCT-IRS and LIRS notices — says only "13 digit" and "linked to"/"generated from" the NIN or CAC number. None states whether the source identifier is embedded, prefixed or merely mapped. A length check is what the published record supports.
- **KI-NG-3** The four digits after the hyphen in a FIRS TIN are accepted as any \d{4}, not just 0001.
  - *Not fixed:* The OECD profile literally writes the structure as "eight digits, a hyphen then the 0001" but never explains the suffix. Branch/office TINs with other suffixes are widely reported and taxid.pro gives the pattern as \d{8}-\d{4}. Constraining to 0001 on an unexplained example would reject live branch numbers, which is the costly direction.
- **KI-NG-4** ValidateEntity does not validate a CAC RC/BN company registration number.
  - *Not fixed:* Out of scope for this assignment and arguably for the method: the CAC number is a company-register identifier, not a tax identifier, and under the 2025 Act it is the input from which the corporate Tax ID is generated rather than the Tax ID itself. Adding it would change what ValidateEntity means for NG relative to every other country.

## Pakistan

- **KI-PK-1** No entity or VAT rule exists - the FBR's National Tax Number and sales tax registration number have no published format or check digit.
  - *Not fixed:* python-stdnum has only pk.cnic for Pakistan (confirmed against the 2.1 module index); the FBR publishes no NTN algorithm. Inventing one would reject live numbers.

## Philippines

- **KI-PH-1** python-stdnum PR #349 also accepts 13-digit TINs; my regex accepts only 9, 12 and 14 digits, so the existing test row "1234567890000" (13 digits) stays false.
  - *Won't fix:* I found no BIR source for a 4-digit branch code. The BIR's own wording sources 9 (core), 12 (3-digit branch) and 14 (5-digit branch); PR #349 is an unmerged community proposal and I would not widen the accepted set on it alone. Confirmed this pass against the BIR's own structure and the 2026 branch-code expansion: 9 digits, 12 with a 3 digit branch and 14 with a 5 digit branch are what is issued. Nothing published describes a 13 digit TIN, and PR #349 is an unmerged community proposal.

## Portugal

- **KI-PT-1** ValidateCartaoCidadao positions 9 and 10 accept digits as well as letters, so the 'two-letter document version' is not enforced.
  - *Won't fix:* Deliberately not tightened. python-stdnum's own pattern for those two positions is [A-Z0-9], not [A-Z], and stdnum is the authority named in my instructions. Requiring letters would diverge from it and risk rejecting real cards, and I found no official Portuguese statement on the version field's character set. The new guard matches stdnum exactly. The guard matches python-stdnum exactly, which is the authority named for this library.

## Russia

- **KI-RU-1** ValidateBIK never checks the leading country code, and its 000-002 / 050-999 guard on the last three digits is itself not backed by the current regulation.
  - *Not fixed:* I re-checked the authoritative source rather than the bank blogs the earlier wave used. Appendix 6 of Положение Банка России от 06.07.2017 № 595-П defines the БИК as position 1 = participation type (0 direct, 1 indirect, 2 non-participant client) and positions 2-9 = a free participant identifier 00000001..99999999 — no fixed "04" prefix and no reserved range for the last three digits (https://www.consultant.ru/document/cons_doc_LAW_280683/24a8cab5291d2517c8fdaeee243a44901d717408/). That contradicts the classic layout the current code and its tests encode, and real post-2021 БИКs (e.g. 004525988, 017003983) happen to satisfy both readings, so neither adding a prefix guard nor removing the existing range guard can be justified without evidence about which values are actually assigned. Left alone; the earlier wave's "unsettled" call stands and is now better evidenced.

## Taiwan

- **KI-TW-1** The UBN '7' exception makes two different check digits valid for the same first seven digits (12345670 and 12345675 both pass).
  - *Settled:* That is the published rule, not a defect: 7 x weight 4 = 28, whose digit sum may be counted as 10 or as 1. python-stdnum behaves identically. Encoded as-is and covered by a test row. Not a defect: that is the published rule, and python-stdnum behaves identically.

## Thailand

- **KI-TH-1** Residual false-negative risk on ValidateNationalIdentity: the Department of Provincial Administration also issues 13-digit numbers beginning with 0 to persons of undetermined registration status (the 'pink card' holders), which the OECD/RD structure does not account for and which can collide with the 010-096 DBD window. Those are rejected as national identity numbers now.
  - *Won't fix:* Both sourced references say a personal number is 100-999 (OECD Section II) and that leading 0 is 'not found on cards of Thai nationals' (Wikipedia, Thai identity card); python-stdnum rejects 0 for PIN too. Accepting leading 0 as personal would re-create exactly the ambiguity this task asked me to remove, since every company number starts with 0. Flagging rather than guessing. Accepting a leading zero as personal would re-create the ambiguity with company numbers that the holder-type work removed, and all three sources say a personal number is 100-999.

## Ukraine

- **KI-UA-1** ValidateVAT accepts the twelve digit IPN on its format alone; the twelfth digit is a check digit and is not verified.
  - *Not fixed:* Genuinely unpublished. uk.wikipedia quotes the rule as "12-й знак -- контрольний розряд, алгоритм формування якого встановлює центральний орган державної податкової служби України": the tax authority sets the algorithm and does not publish it. The composition either side of that digit is now recorded in the code comment, and the ten digit legacy form -- whose check digit *is* published -- is validated in full.

## Uzbekistan

- **KI-UZ-1** No company or VAT rule: uz.company and uz.vat are NotCovered.
  - *Not fixed:* The taxpayer number (STIR) and the VAT registration code have no published check digit or structure that I could read; the lex.uz act cited for uz.taxcode covers the personal number only.

## UnitedArabEmirates

- **KI-AE-1** ValidateNationalIdentity checks only the 784 + 12 digit shape; the trailing check digit is never verified, so 784198012345670..9 are all accepted.
  - *Not fixed:* ICP/u.ae publish the 15-digit format but no check digit algorithm. The widely circulated Luhn recipe is explicitly untested by its own author and is reported to fail real cards whose check digit is 0, so encoding it would reject valid IDs. The valid samples in the test file were nevertheless chosen to carry the Luhn digit of their first 14 digits, so they survive if a checksum is added later.


- **KI-AE-2** No company number rule: ae.company is NotCovered.
  - *Not fixed:* Trade licence numbers are issued by each emirate's licensing authority (and each free zone), and I found no federal format or check digit published. A rule would have to be one per issuing authority.
- **KI-AE-3** No VAT rule: ae.vat is NotCovered. The Tax Registration Number (TRN) is described as 15 digits.
  - *Not fixed:* The FTA's TRN lookup page asks for 15 characters, but I could not open an FTA or Ministry of Finance specification that states the format. The Ministry's e-invoicing consultation draft says the number starts with 1 and ends with 03, which other sources do not repeat as a rule, and the final specification PDF could not be read here. Not enforced until the published rule is read.
- **KI-AE-4** No personal tax code rule: ae.taxcode is NotCovered.
  - *Not fixed:* The UAE has no personal income tax and issues no personal tax number as far as I know, but I found no authority page that says so, so it is not marked NotApplicable. The OECD TIN sheet URL used for other countries returned 404.

## Algeria

- **KI-DZ-1** No rule for any of the five kinds: dz.* are NotCovered.
  - *Not fixed:* Added in 2.0 as registered types only. No authority page stating a format (NIF, national id, postal code) was read in this pass.

## Egypt

- **KI-EG-1** No rule for any of the five kinds: eg.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format (tax registration number, national id, postal code) was read.

## Ghana

- **KI-GH-1** No rule for any of the five kinds: gh.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format (TIN, Ghana Card, GhanaPostGPS) was read.

## Guinea

- **KI-GN-1** No rule for any of the five kinds: gn.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format (NIFP) was read.

## Kenya

- **KI-KE-1** ke.nationalid and ke.postalcode are NotCovered; the KRA PIN rules (ke.taxcode, ke.company, ke.vat) check only the shape.
  - *Not fixed:* KRA's eTIMS specification gives A or P, nine digits and a letter, and no algorithm for the last letter, so it is not checked. The first letter is accepted as A or P for all three kinds because the specification does not tie a letter to a use. No national id or postal code format was read.

## Liechtenstein

- **KI-LI-1** No rule for any of the five kinds: li.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format (PEID, UID, postal codes) was read.

## Morocco

- **KI-MA-1** Only ma.company (ICE, 15 digits) has a rule, and it checks the length only; the other four kinds are NotCovered.
  - *Not fixed:* DGI forms print the ICE in boxes of 9, 4 and 2 digits (decree 2-11-63). The meaning of the groups and any check were not found. The tax identifier (IF), national id and postal codes were not read.

## Mozambique

- **KI-MZ-1** No rule for any of the five kinds: mz.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format (NUIT) was read.

## Oman

- **KI-OM-1** No rule for any of the five kinds: om.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format was read.

## Senegal

- **KI-SN-1** No rule for any of the five kinds: sn.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format (NINEA) was read.

## Singapore

- **KI-SG-1** No rule for any of the five kinds: sg.* are NotCovered.
  - *Not fixed:* ACRA's Special UEN page shows sample UENs of several shapes, but no format specification or check-letter algorithm was found, and the NRIC/FIN check letters were not found on an authority page. Nothing is guessed.

## Tunisia

- **KI-TN-1** No rule for any of the five kinds: tn.* are NotCovered.
  - *Not fixed:* Registered types only; no authority page stating a format (matricule fiscal) was read.

## Vietnam

- **KI-VN-1** vn.taxcode, vn.company and vn.vat check only the shape (10 digits, or 10 and 3); vn.nationalid and vn.postalcode are NotCovered.
  - *Not fixed:* Circular 105/2020/TT-BTC defines N1N2 N3..N9 N10 - N11N12N13 with N10 a check digit; the check algorithm was not found. The citizen identification number and postal codes were not read.

## Norway

- **KI-NO-1** Parse decodes the sex (9th digit, odd for a man) but not the birth date.
  - *Not fixed:* Skatteetaten's pages state DDMMYY and say the first six digits show the issue date when the numbers for a date run out, but neither the English nor the Norwegian page states how the century is read from the individual number. Not guessed.

## Denmark

- **KI-DK-1** Parse decodes the sex (10th digit, even for a woman) but not the birth date.
  - *Not fixed:* cpr.dk says digits 5-7 select the century; the table was not on the page read. cpr.dk reports modulus 11 as holding only up to 2007-10-01; the existing rule does not check it, so no false rejects follow.

## Sweden

- **KI-SE-1** Parse decodes the birth date only from the twelve-digit form. The ten-digit form has no century (the separator "-" becomes "+" from the year a person turns 100), and Normalize drops the separator. The sex (9th digit of the ten-digit part, odd for a man) is decoded for both.
  - *Not fixed:* Skatteverket's page does not give the control digit algorithm and the legal guidance page 330245 refused the fetch.

## Bulgaria

- **KI-BG-1** bg.nationalid reports every rejection (a wrong check digit included) as Reason.InvalidFormat with the message "Invalid code"; the 1.x rule does not separate the causes.
  - *Not fixed:* The verdict is right; only the reason is coarse. No corpus row asserts the coarse reason.

## Estonia

- **KI-EE-1** No Parse. The ministry page says the first digit gives sex and the next six the date and cites standard EVS 585, but the digit-to-century table is not on any page that could be read (the Riigi Teataja regulation returned only its header).
  - *Not fixed:* Read the regulation or EVS 585. Secondary sources give 1/2 for the 1800s, 3/4 for the 1900s and 5/6 from 2000; not used.

## Lithuania

- **KI-LT-1** No Parse. Law I-2237 says the first digit gives sex and century, but the mapping is in the standard LST 1185-91, whose text is restricted.
  - *Not fixed:* Read LST 1185-91.

## International

- **KI-IBAN-1** The national check digits inside the BBAN are not verified; only the registry length, the BBAN character classes and the MOD 97-10 check digits are.
  - *Not fixed:* Several registry entries carry a national check digit (BE, ES, FR, IT, PL, PT, NO, FI and others). Each is defined by the national bank, not by the IBAN registry, and none has been read from its national source yet, so none is guessed (AGENTS.md rule 1). Until then an IBAN with a wrong national check digit but a consistent MOD 97-10 check is accepted.
- **KI-IBAN-2** The structure table was not read from the SWIFT registry itself: swift.com answered 403 to every fetch. It was written from memory of the registry's structures and checked against Wikipedia's table and IbanNet (tests only).
  - *Not fixed:* Re-read the table against the current registry release when it can be fetched, and record the release number in the provenance table.
- **KI-IBAN-3** RU is in the table (33 characters, 9 digits, 5 digits, 15 alphanumerics) on the strength of Wikipedia's table alone; IbanNet's registry does not have it.
  - *Not fixed:* Confirm Russia's registration and structure against the SWIFT registry release notes. Until then an RU IBAN is accepted on structure and MOD 97-10 only.
- **KI-ISSN-1** Only the ISSN source was read in full (issn.org states eight digits, modulus 11, X for 10). The check weights 8..2 follow from that rule but the page does not list them.
  - *Not fixed:* Re-read the ISSN Manual for the weights.
- **KI-ISBN-1** The ISBN Users Manual URL answered 404 and was not read; ISBN-10 (modulus 11, weights 10..1) and ISBN-13 (EAN check, prefix 978/979) are implemented from the standard as commonly published. No hyphenation: printing the groups needs the agency's range table.
  - *Not fixed:* Read the current manual; add range-based Format.
- **KI-GTIN-1** gs1.org answered 403, so the GS1 check digit rule (weights 3 and 1) was not read on that page. GTIN-8/12/13/14 are checked by length, digits and check digit; the GS1 prefix allocation is not.
  - *Not fixed:* Re-read the GS1 General Specifications.
- **KI-ISIN-1** The ISIN source URL (ISO 6166 page) was not fetched. The check (Luhn over letters read as 10-35) is the standard's published method. The country prefix is two letters, not tested against the ISO 3166 / ANNA allocation list.
  - *Not fixed:* Read ISO 6166 or the ANNA ISIN guidelines.
- **KI-LEI-1** gleif.org confirms 20 alphanumeric characters but its page does not describe the check digits; MOD 97-10 comes from ISO 17442 as published elsewhere. The LOU prefix is not checked against GLEIF's list.
  - *Not fixed:* Read ISO 17442; consider the LOU prefix list.
- **KI-BIC-1** swift.com answered 403; BIC (8 or 11 characters, letters then alphanumerics) was not read from the source and has no check digit. The country letters are not tested against ISO 3166.
  - *Not fixed:* Read ISO 9362.
- **KI-ISMN-1** Only the 13-digit form with prefix 979-0 is accepted. The pre-2008 form (M followed by nine digits) is not, and no hyphenation is printed: that needs the agency's range table.
  - *Not fixed:* Read the ISMN Users' Manual for the legacy form and the publisher ranges.
- **KI-IMEI-1** The Luhn check and the 15-digit length come from 3GPP TS 23.003, read through a mirror (itecspec.com) because the specification archive is a zip download. The 16-digit IMEISV has no check digit and is not accepted. The TAC is not checked against the GSMA allocation.
  - *Not fixed:* Read the TS 23.003 archive itself; the GSMA TAC database is not public.
- **KI-ISO6346-1** BIC's identification-number page confirms the layout (three owner letters, U, J or Z, six digits, a check digit) but does not state the check algorithm, and ISO 6346 itself is paywalled. The weights (powers of two), the letter values (A is 10, multiples of 11 skipped) and the mod 11 then mod 10 reduction are the published method, checked against the widely quoted example CSQU3054383 (a secondary source, not BIC) only. The owner code is not looked up in BIC's register.
  - *Not fixed:* Read ISO 6346 or the BIC calculator's stated method; consider the owner-code register.
