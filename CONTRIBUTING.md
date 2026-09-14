# Contributing

Attest decides whether a real person's identity number, a real company's tax number or a real
address's postal code is valid. The expensive failure is not a crash, it is a wrong verdict: a
false reject means someone is refused service by software that was confidently wrong about their
country. Everything below exists because that already happened here.

The library is a fork of [CountryValidator](https://github.com/anghelvalentin/CountryValidator),
unmaintained since 2023. Three repair waves fixed 197 defects across the 87 validators and took the
test suite from 586 cases to 3,593. The rules in this document were learned from those
defects. [CHANGELOG.md](CHANGELOG.md) lists them one by one if you want the evidence.

## Build and test

```
dotnet build Attest.sln
dotnet test Attest.Tests/Attest.Tests.csproj
```

`Attest` and `Attest.DataAnnotations` target `netstandard2.0` and `net8.0`; the test project targets
`net9.0`, so you need both the 8.0 and 9.0 SDKs installed. CI runs restore, build, test and pack in
`Release` on every push and pull request (`.github/workflows/ci.yml`).

One country while you work:

```
dotnet test Attest.Tests/Attest.Tests.csproj --filter FullyQualifiedName~ThailandValidatorTests
```

## The rules

### 1. Every rule change cites a published source

The country's tax authority, its legal act, an OECD TIN sheet, or python-stdnum's implementation.
Put the link in the XML doc comment or a comment beside the code; there are 114 such links in
`Attest/CountriesValidators` already. No rule goes in from memory, and "this is how it works" is not
a source.

`BelarusValidator` required two leading letters on a UNP (`^[AaBbCcEeHhKkMmOoPpTt]{2}`). No UNP has
them — a company's UNP is nine digits, and the letter form is what individuals get. Every genuine
Belarusian company number was rejected for years because nobody checked the rule against the
registry.

### 2. Test numbers are computed, never copied

Every number asserted valid must be one whose check digit you computed yourself, or a published
example from the source you cited, named in a comment on the row.

Cyprus's only national-identity test was `7103192745`, a Czech rodné číslo copied verbatim from
`CzechValidatorTests.cs`. Thailand's individual, entity and VAT tests were Swedish personnummer —
the `+` century separator gave them away. Both files were green for years and certified nothing.

Show your work in the row where it is not obvious. `MaltaValidatorTests` carries
`[InlineData("10001737", true)]` with the arithmetic in the comment, precisely because that row is
the one that breaks if someone changes the checksum formula.

### 3. Never write a test that asserts behaviour you believe is wrong

If a validator is wrong and you cannot fix it safely, leave the code alone, leave the test out, and
write the finding into [KNOWN-ISSUES.md](KNOWN-ISSUES.md). A green test locking in a bug is worse
than no test: it converts a defect into a specification and the next contributor has to disprove it.

`RussiaValidatorTests` asserted that Sberbank's and Gazprom's company ИНН were valid *personal* tax
codes. `IcelandValidatorTests` asserted that a person's kennitala is a valid company identifier.
`AndorraValidatorTests` asserted that a public body's NRT is a natural person's tax code. Four rows,
three countries, all passing, all wrong.

The same applies to deleting a row: if a test fails because your fix is right, delete the row and
say so in the commit body. If it fails because your fix is wrong, fix the fix.

### 4. The contract: a Validate\* method never throws

Every public `Validate*` method returns a `ValidationResult` for any input, including `null`, `""`,
`"   "`, `"---"`, `"abc"` and digits from any script. 22 of the 87 validators used to throw —
`NullReferenceException` from Belarus and Poland, `IndexOutOfRangeException` from Bulgaria,
`FormatException` from India, `ArgumentOutOfRangeException` from Monaco's `Substring(2, 3)` before
any length guard. `Attest.Tests/UnicodeDigitSweepTests.cs` now sweeps all 87 countries across six
entry points -- the five `Validate*` methods plus `Validate(value, country, kind)` -- and will fail
your PR if you reintroduce one. Do not weaken it to make a change
pass.

One exception, and do not "fix" it locally: eleven validators still throw `NotSupportedException`
for kinds their country has no rule for. `CountryValidator.Supports` (line 276) uses that throw as
its only signal for "this country has no rule for this kind", and the facade converts it to
`Invalid("Not supported")` before any caller sees it. Converting one validator to return a result
makes `Supports` claim a rule exists where none does. Changing this needs one PR that moves all
eleven and `Supports` together.

### 5. Do not bypass or "improve" input normalisation

`IdExtensions.RemoveSpecialCharacthers` (the misspelling is public API, leave it) keeps letters of
any script and ASCII digits, drops punctuation and separators, and replaces a decimal digit outside
0-9 with `U+FFFD`, which no format check in the library accepts. Letters of any script are kept on
purpose: a Belarusian UNP is written with Cyrillic characters that `BelarusValidator` maps to their
Latin look-alikes (А→A, В→B, Н→H, Р→P, С→C) — transliteration (В→V, Н→N) is the wrong answer there.

Never repair input by deleting a character. Dropping a bad digit validates the digits that remain
and turns a wrong number into a right one.

### 6. Guard with `[0-9]`, not `\d`, anywhere a parse follows

.NET's `\d` and `char.IsDigit` match every Unicode decimal digit; `int.Parse`, `long.Parse` and
`Convert` accept only ASCII. A validator that guards with one and parses with the other throws on
Arabic-Indic, Devanagari or fullwidth digits. Where `char.GetNumericValue` follows instead, there is
no crash and something worse: it silently validates a number no registry ever issued. Both patterns
still exist in validators nobody has swept yet.

### 7. Ambiguity is reported, not guessed

Some countries issue one number that is both a personal and a business identifier. Armenia's 8-digit
ՀՎՀՀ and Nigeria's TIN are the real cases, as is a Russian sole trader filing VAT under his personal
ИНН. `IdentifierResult.IsAmbiguous` says so rather than picking a side.

Before you add a country to that list, check whether the number actually encodes the holder. Four
that the library called ambiguous were defects: Peru's RUC carries a holder-type prefix (10 and 15
natural persons, 20 legal entities), Andorra's NRT a holder-type letter, Iceland's kennitala adds 40
to the day field for an organisation, and Thailand's TIN names the issuing agency in digits 1-3.
Each of those was `ValidateEntity` delegating to `ValidateIndividualTaxCode` with no comment saying
why.

## What a good change looks like

The smallest correct fix, in the surrounding file's style, with a test that fails before it and
passes after.

- One country per pull request, or one pattern across many. Not a country plus an unrelated
  refactor.
- Match the file you are in: its naming, its `ValidationResult.Invalid*` choice, its comment style.
  The result should be unremarkable next to the code around it. Repo-wide style opinions belong in
  an issue, not in a validator diff.
- Pick the right failure reason. A wrong length is `InvalidLength`, not `InvalidChecksum`; Colombia
  had that backwards.
- Write the test first, watch it fail, then fix. Most of the defects in the second wave were found
  by writing the first test a validator had ever had.
- Cover both directions. A theory of three rows that all expect `true` certifies nothing — Germany's
  `TestCorrectEntityCode` would have passed against a method that returned success for every input.
- Do not edit `CHANGELOG.md`; the maintainer writes it at release. Put the reasoning in your commit
  body and PR description. `README.md`'s country table changes only when coverage changes.

## When you cannot source the rule

Stop and write it down. An honest gap in [KNOWN-ISSUES.md](KNOWN-ISSUES.md) is worth more than a
plausible invention: an invented check digit rejects live numbers, and nobody reports the rejection
to you.

Entries go under a `##` country heading, one line for the gap, one indented line for why it was left:

```markdown
## Georgia

- The 11-digit personal number's trailing verification digits are still unchecked;
  ValidateIndividualTaxCode remains a 9-or-11-digit format guard.
  - *Not fixed:* No algorithm published. python-stdnum has no ge module, and sources contradict
    each other on what the trailing digits even are.
```

Name the file and method, say what input gets the wrong answer, and say what you checked and did not
find. "Unsourced" without the search behind it just makes the next person repeat it.

## Where to start

[KNOWN-ISSUES.md](KNOWN-ISSUES.md) is the contribution funnel. It has 65 entries across 36 headings,
each one a gap someone found, verified and deliberately left, with the reason it was left. Most are a
single validator plus its test file.

Read the `*Not fixed:*` line first, because it tells you which kind of entry you have:

- **"Outside my file list" / "belongs to another wave"** — tractable, and the best place to start.
  The two largest are cross-cutting sweeps: about twenty validators still strip a country prefix with
  an unanchored `.Replace("XX", "")`, so `76086CL4285` validated as the Chilean RUT `76086428-5`
  (see the Norway entry); and `\d` before a parse still exists in validators nobody has swept (see
  the OTHER COUNTRIES entry). Norway, Turkey, Malta and Chile show the fixed form.
- **"No algorithm is published"** — Armenia's TIN check digit, Nigeria's NIN, the UAE's 15-digit ID.
  These are closed until a source appears. Open them only if you have found that source.
- **"Behaviour change" / "product decision"** — Peru's `ValidateVAT`, Indonesia's `ValidateVAT`,
  Kazakhstan's birth-date gate. Open an issue and argue the reading before writing code.

Entries leave this file when they are fixed, not when they are argued about. The XML `<param>` tags
that used to head it are gone, and CS1572 and CS1573 came out of the `NoWarn` list in
`Directory.Build.props` in the same change, so they cannot come back.

## Commit messages

Imperative subject line, no scope prefix, no trailing period. A body that explains why, not what —
the diff already says what. Real subjects from this repository:

```
Fix 52 defects across 22 countries and the core types
Tell personal identifiers from business ones, and close 65 more defects
Close the non-ASCII digit crash class, and add CI
```

For a single fix, name the country and the effect: `Reject Belarusian UNPs written with Cyrillic
look-alikes`. In the body, give the source you used and the number that used to get the wrong
answer.

## The review bar

A pull request is read against this list:

1. CI is green. Build, all tests, pack.
2. Every rule change carries a link to the authority it came from.
3. Every number asserted valid is computed or published, and traceable to that source.
4. No test asserts behaviour you believe is wrong, and no test was deleted or loosened to get green
   without a sentence saying why.
5. `UnicodeDigitSweepTests` is untouched and passing.
6. The diff is the smallest one that fixes the defect, and reads like the file around it.
7. Anything found and not fixed is in `KNOWN-ISSUES.md` with its reason.

Expect the reviewer to recompute a check digit or two by hand, and to open the source you cited. A
PR that fails on 2 or 3 does not get a style review; it gets sent back for evidence.

## AI-assisted contributions

Use whatever tools you like — much of the repair work here was done that way, and
[CHANGELOG.md](CHANGELOG.md) is the record of it. Two things are true at once: generated code is
good at this problem, and it fails in exactly the places these rules guard.

A model will produce a confident check-digit algorithm for a country that publishes none, and a
plausible-looking valid test number whose check digit is wrong. It will write the test that asserts
whatever the validator currently does, which is how Sberbank's ИНН became a personal tax code. It
will "clean up" input by deleting the character that did not fit.

So rules 1, 2 and 3 are where review concentrates, and they apply to your tools' output exactly as
they apply to yours: open the source yourself, compute the check digits yourself, and read every
assertion asking whether it is true rather than whether it passes. If you cannot verify a number,
leave it out.
