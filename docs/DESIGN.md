# Attest 2.0: design

Status: proposal. Nothing here is built until a PR says so. Decisions marked **Open** need the
maintainer; each has a default that the PRs follow until it is overruled.

## 1. What this is

Attest 2.0 is the most complete identifier library for .NET: the python-stdnum of .NET. It decides,
in-process and with no network call, whether a string is a well-formed identifier of a named type
(a Belgian enterprise number, a Brazilian CPF, an IBAN), and it can also normalize it, print it in
its official form and take it apart.

It is layer 1 of two. Layer 2, later and separate, is a hosted verification service (VIES and other
registry lookups, a timestamped proof, MCP and x402 access). Layer 1 stays a free Apache-2.0 library
with no network code, no telemetry and no nag. Layer 2 consumes layer 1's rule identifiers and rule
revisions (section 5), which is the only coupling.

Standing rule, unchanged from 1.x: the expensive failure is a false reject. A rule encodes what the
authority publishes and nothing more. A check digit nobody publishes is not invented; the gap is
recorded as data instead of prose (section 5).

## 2. Where 1.x falls short

Measured on `main` at 1.2.1.

| Fact | Number |
|---|---|
| Countries | 87 |
| Country/kind pairs (87 x 5 kinds) | 435 |
| Pairs with no rule | 25 |
| Tests | 4,212 |
| Open gaps in `KNOWN-ISSUES.md` | 19 |
| python-stdnum 2.2 modules (repo directory listing, helpers included) | about 216, of which about 27 international |
| Countries in python-stdnum that Attest lacks | 14: DZ EG GH GN KE LI MA MZ OM SG SN TN VN, plus the `eu` package |
| Countries in Attest that python-stdnum lacks | 11: AE AM BA BH BO GE HK KZ NG PH UZ |
| International identifiers (IBAN, ISBN, LEI, ISIN, BIC, EAN, IMEI ...) | 0 of 27 |

Structural limits, not just missing data:

1. **Five fixed kinds.** The model is country x {personal id, personal tax code, company, VAT,
   postal code}. A country with five different business registers, a bank account format, a health
   number or a vehicle number has nowhere to put them. python-stdnum's unit is the identifier type
   (`be.vat`, `br.cpf`), and that is the right unit.
2. **No normalize, format or parse.** Callers cannot store a canonical form, print the official
   form, or read the birth date out of a number the library already decoded to validate it.
3. **Rejections are strings.** `ValidationResult.ErrorMessage` is English prose. Callers cannot
   branch on "wrong length" versus "wrong check digit", and a localized UI cannot choose its own
   wording.
4. **"No rule" looks like "invalid".** `Supports(country, kind)` exists, but `Validate` reports the
   same `IsValid == false` for a malformed number and for a pair nobody wrote a rule for. Some of
   the 25 empty pairs are empty because the country issues no such identifier; others because the
   work is not done. Those are different answers.
5. **Provenance is a comment.** A source URL beside the code cannot be listed, tested or shown to a
   user, and "last verified" does not exist at all.
6. **A public class per country.** 87 public validator classes and one dispatch facade are the
   whole surface, so internals cannot change without a break.
7. **No bounds.** No input length limit and no regex timeouts (`docs/guide/status.md` lists this as
   not built).

## 3. API

Namespace `Attest`. Static first: rules are immutable and stateless, so a registry needs no
instance, no DI and no lifetime guidance. Targets stay `netstandard2.0` and `net8.0`, and the
package keeps zero dependencies.

### 3.1 Identifier types

```csharp
public sealed class IdentifierType
{
    public string Id { get; }                 // "be.vat", "br.cpf", "iban": lower case, stable, never reused
    public Country? Country { get; }          // null for international types
    public IdentifierKind Kind { get; }
    public string Name { get; }               // "Belgian enterprise number" (English)
    public string LocalName { get; }          // "ondernemingsnummer"
    public IReadOnlyList<string> Aliases { get; }   // "btw", "tva", "nwst"
    public Coverage Coverage { get; }         // section 5
    public RuleInfo Rule { get; }             // section 5: sources, last verified, revision

    public IdentifierResult Validate(string? value);
    public string? Normalize(string? value);  // compact canonical form, null if not recoverable
    public string? Format(string? value);     // official printed form, null if invalid
    public ParseResult Parse(string? value);  // validate and decompose
}
```

Lookup and discovery live on one static class:

```csharp
public static class Identifiers
{
    public static IReadOnlyList<IdentifierType> All { get; }
    public static IdentifierType? Find(string id);                       // by Id or alias
    public static IReadOnlyList<IdentifierType> For(Country country);
    public static IReadOnlyList<IdentifierType> For(Country country, IdentifierKind kinds);

    public static IdentifierResult Validate(string? value, string typeId);

    // Several types of one country at once. Replaces CountryValidator.Validate(value, country, kinds).
    public static MatchResult Match(string? value, Country country, IdentifierKind kinds = IdentifierKind.Any);

    // EU VAT with its prefix ("BE0428759497"): the country comes from the value.
    public static IdentifierResult ValidateVat(string? value);
}
```

`Id` is the stable public key. It is what layer 2 and every serialized record will store, so the
scheme is fixed now: `<iso3166-alpha2-lower>.<name>` for national types, bare `<name>` for
international ones, and the second part is the name the issuing authority or python-stdnum uses
(`br.cnpj`, `es.nif`). A type is never renamed; a replacement gets a new id and the old one stays
as an alias.

No generated per-type constants in 2.0. `Identifiers.Find("br.cpf")` plus a test that every id in
the docs resolves is enough. A source generator for typed constants is an option for later.

### 3.2 Results

`ValidationResult` collides with `System.ComponentModel.DataAnnotations.ValidationResult`, which the
DataAnnotations package already has to disambiguate. The new type is `IdentifierResult` (the name
1.x gave to the multi-kind result, which becomes `MatchResult`).

```csharp
public readonly struct IdentifierResult
{
    public Verdict Verdict { get; }      // Valid, Invalid, NotCovered
    public bool IsValid => Verdict == Verdict.Valid;
    public Reason Reason { get; }        // None when valid; why it failed otherwise
    public string? Detail { get; }       // English, for logs; never the thing to branch on
    public string? Normalized { get; }   // compact form when the value could be normalized
    public IdentifierType Type { get; }
}

public enum Verdict { Valid, Invalid, NotCovered }

public enum Reason
{
    None,
    Empty,              // null, "", whitespace
    TooLong,            // over the input bound, rejected before any parsing
    InvalidCharacters,  // a character that cannot occur in this type
    InvalidLength,
    InvalidFormat,      // right characters and length, wrong shape
    InvalidChecksum,
    InvalidDate,        // an embedded date does not exist
    InvalidComponent,   // a field outside its published range: region, month, century, type letter
    InvalidPrefix,      // wrong or missing country prefix
}
```

Rules for the enum, because callers will `switch` on it: values are only ever added; a rule
returns the first reason in the order above that applies (so a value with a bad character is
`InvalidCharacters` even if it is also too short); no member means "unknown", because
`Verdict.NotCovered` carries that.

**`NotCovered`** is the answer when Attest has no rule for the pair. It is not `Invalid`. A caller
who sees it knows the library has no opinion; the 1.x behaviour ("every value is invalid") is gone.
`IsValid` is false for it, so a caller who only reads `IsValid` is still fail-closed.

`ParseResult` extends the verdict with the decoded parts:

```csharp
public readonly struct ParseResult
{
    public IdentifierResult Result { get; }
    public DateTime? BirthDate { get; }        // personal ids that encode one
    public Sex? Sex { get; }                   // Female, Male, when encoded
    public Country? IssuingCountry { get; }    // for prefixed or multi-country types
    public IReadOnlyDictionary<string, string> Parts { get; }  // type-specific: "region", "serial", "check"
}
```

`Parts` keys are documented per type in the generated coverage table. `Parse` never decodes a value
that failed validation: an invalid value yields an empty `Parts`.

`MatchResult` is 1.x's `IdentifierResult`, renamed: `Requested`, `Matched`, `Details` keyed by type,
`IsValid`, `IsAmbiguous`. Ambiguity keeps its meaning (one value legitimately valid as both a person
and a business identifier: Armenia, Nigeria, sole traders in RU, PE, ID, UA).

### 3.3 Normalize and Format

- `Normalize` is the compact canonical form: separators and spaces removed, letters upper-cased with
  `ToUpperInvariant`, the country prefix kept or dropped per type (a VAT number keeps its prefix,
  as the VIES form). It is idempotent, and `Normalize` of a valid value is valid.
- `Normalize` never repairs. Per `AGENTS.md` rule 7, dropping a wrong character turns a wrong number
  into a right one. It removes only the separators the type documents (`.`, `-`, space, `/`).
- `Format` prints the form on the card, invoice or register, e.g. `85.07.15-017.09`. It returns
  `null` for an invalid value. `Format(Normalize(x)) == Format(x)`.
- Both are pure and culture-invariant; `CultureSweepTests` extends to them.

### 3.4 Kinds

`IdentifierKind` stays a flags enum, and its five 1.x members keep their values. It gains members
for what the five-kind model had no room for:

`BankAccount`, `Healthcare` (NHS, AMKA, AIC), `Security` (ISIN, CUSIP, FIGI, LEI), `TradeItem`
(EAN, GTIN, ISBN, ISSN), `Telecom` (IMEI, IMSI, MAC), `Vehicle`, `Other`. `Any` keeps meaning
"personal and business", so existing callers of `Match(..., Any)` do not suddenly receive IBAN
matches.

### 3.5 Bounds and safety

- Input longer than 128 characters is `TooLong` before any rule runs. (**Open**: 128 is a starting
  value; the longest published identifier is an IBAN at 34 plus separators, and a few European
  registry numbers with spaces.)
- Every regex is constructed with a match timeout and without backtracking-prone alternation.
- No `Validate` throws, for any input, in any culture: the existing four sweeps stay, and extend to
  every new method.

### 3.6 DataAnnotations

`Attest.DataAnnotations` gets one attribute, `[Identifier("be.vat")]`, backed by the new API. The
five 1.x attributes stay, marked obsolete, and call through to it.

## 4. Architecture

```
Attest/
  Identifiers.cs, IdentifierType.cs, IdentifierResult.cs, Reason.cs ...   public surface
  Rules/                  internal: one file per identifier type, grouped by country folder
  Algorithms/             internal: Luhn, Verhoeff, Damm, ISO 7064 MOD 11-2/11-10/37-2/97-10,
                          weighted-sum helpers
  Registry/               internal: id -> rule table built once; provenance table
  Legacy/                 v1 surface kept for one major (section 7)
```

A rule is a small internal class: it declares `Info` (section 5) and implements one method that
turns a raw string into a result. Normalize, Format and Parse are optional overrides with a shared
default (strip separators, upper-case, same string). A type that does not override Format is
printed in compact form; the coverage table shows which types do.

Rule bodies are not written from zero. The 87 validators embody 197 repaired defects and 4,212
tests, and discarding them to be clean would reintroduce the Hungary bug class. 2.0 starts with an
adapter that exposes each 1.x validator method as a rule, so all 410 covered pairs appear in the new
API on day one, and the 1.x suite runs against the new engine as a parity oracle. Rules are then
ported to native form (typed reasons, Normalize, Format, Parse) one country at a time, and each
port keeps its tests.

The algorithms are the shared part that python-stdnum is right about: a handful of check-digit
schemes cover most identifiers, and implementing them once, with their own test vectors, makes a new
identifier a few lines.

### 4.1 IBAN: integrate or own

IbanNet (github.com/skwasjer/IbanNet) is mature and Apache-2.0, with its own registry, parsing and
DI packages. Options considered:

| Option | For | Against |
|---|---|---|
| A. Depend on IbanNet | no IBAN code to maintain; broad registry | first runtime dependency; its result type and reasons differ from ours; version pinning for every consumer; provenance and rule revision would be IbanNet's, not ours |
| B. Own it | zero dependencies; same `Verdict`/`Reason`/`Format`/`Parse` as every other type; provenance of our own | we maintain a length/structure table |
| C. Satellite `Attest.Iban.IbanNet` adapter | no cost to core | an adapter nobody asked for |

**Default: B.** IBAN is the one identifier every consumer of an identifier library will check first;
it must behave like the rest. The core is small: mod 97-10, per-country length and BBAN structure
pattern, and, where the national authority publishes one, the national check digits. Registry
facts (lengths, patterns) are compiled from the national bank sources and the ECBS/SWIFT summary
that each country page cites. IbanNet is used as a **differential test oracle** in the test project
only (never shipped): a test sweeps every country's published example IBANs through both and fails
on disagreement. **Open:** the maintainer may prefer A.

### 4.2 python-stdnum: coverage map, not source

python-stdnum is LGPL-2.1-or-later. Translating its code into C# would make Attest a derivative
under that license, which conflicts with Apache-2.0. So:

- It is the **coverage target**: its module list is the checklist in section 6.
- Rules are implemented from the **authority's publication** (the same bar `AGENTS.md` already sets),
  and each cites that source, not stdnum's file. Where the authority publishes nothing, a stdnum
  module is evidence that a rule exists, not a licence to copy it: the gap goes to the provenance
  table as `FormatOnly` or `NotPublished`.
- Do not paste stdnum doctests or code. Test vectors come from the authority's examples or are
  computed independently (section 6.2 vocabulary).
- A dev-only script may run stdnum as an oracle against our corpus. It is not part of the build.

**Open:** 1.x's `docs/adding-a-country.md` allows copying stdnum doctest numbers. This design
withdraws that allowance as the conservative default. If the maintainer takes legal advice that
numbers alone are fine, the vocabulary adds an `origin=stdnum` value and nothing else changes.

## 5. Provenance

Each rule carries machine-readable metadata:

```csharp
public sealed class RuleInfo
{
    public IReadOnlyList<Source> Sources { get; }   // Authority, Url, Title, Accessed
    public DateTime? LastVerified { get; }          // a human or agent re-read the source and the rule matched
    public int Revision { get; }                    // bumped whenever any verdict for this type can change
    public IReadOnlyList<string> KnownGaps { get; } // ids into KNOWN-ISSUES.md
    public Coverage Coverage { get; }
}

public sealed class Source { string Authority; string Url; string Title; DateTime Accessed; SourceKind Kind; }
public enum SourceKind { Authority, Law, Oecd, ReferenceImplementation, Secondary }

public enum Coverage
{
    Full,           // everything the authority publishes is checked
    FormatOnly,     // shape and field ranges; the check digit exists but its algorithm is not published
    NotApplicable,  // the country issues no such identifier (US has no VAT, HK no postal codes)
    NotPublished,   // it exists, nothing authoritative describes it
    NotCovered,     // it exists and is documented; Attest has no rule yet
}
```

Why each field is there:

- `Revision` is what layer 2 stamps into a proof: "valid under `be.vat` revision 7". A proof is
  reproducible only if the rule that produced it can be named. **A verdict change bumps `Revision`**,
  and a test fails when a corpus vector's expected verdict changes without a revision bump (the
  corpus file records the revision it was written at).
- `LastVerified` is honest or null. Rules ported from 1.x start with null and `Coverage` as
  inherited; the first sweep that re-reads the source sets it. The coverage table reports how many
  are null instead of pretending. A rule with no source URL does not register (tests enforce:
  at least one `https` source per type, `Accessed` not in the future).
- `Coverage` replaces the prose half of `KNOWN-ISSUES.md`. That file keeps the reasoning;
  each entry gets an id, and `KnownGaps` links to it so the table and the file cannot drift.
- The 25 empty 1.x pairs become explicit: each is `NotApplicable`, `NotPublished` or implemented.
  `Verdict.NotCovered` is returned only for `NotPublished` and `NotCovered` types.

`docs/COVERAGE.md` is generated from the registry by a test (the build fails if it is stale), so the
table below is not maintained by hand:

| Column | Source |
|---|---|
| Id, name, kind, country | `IdentifierType` |
| Coverage | `RuleInfo.Coverage` |
| Source(s), last verified, revision | `RuleInfo` |
| Normalize / Format / Parse supported | which overrides exist |
| Known gaps | `KnownGaps` |

A scheduled (not gating) workflow checks every source URL and opens one issue listing the dead
ones. A dead link means "re-verify", never "delete the rule".

## 6. Coverage targets and tests

### 6.1 Coverage table (summary)

Target is python-stdnum 2.2 module parity plus what Attest already has beyond it. Exact per-type
status is generated into `docs/COVERAGE.md`; this is the plan by group. Order of work is by value to
callers, not alphabetical; there is no schedule.

| Group | Today | Target |
|---|---|---|
| Attest's 87 countries x 5 kinds | 410 pairs with a rule, 25 empty | 435 resolved (rule or `NotApplicable`/`NotPublished` with a source) |
| Countries in python-stdnum not in Attest | 14 + EU package | all added |
| Types stdnum has beyond Attest's five kinds, per existing country (bank account, registers, health, excise, EORI-like) | 0 | all with an authority source |
| International (27 in stdnum: IBAN, BIC, LEI, ISIN, CUSIP, FIGI, EAN/GTIN, ISBN, ISSN, ISMN, IMEI, IMSI, MAC, ISO 6346 ...) | 0 | all |
| Attest-only (AE AM BA BH BO GE HK KZ NG PH UZ) | covered | kept |
| Normalize and Format | none | every `Full` type; `FormatOnly` types where the authority prints a canonical form |
| Parse | none | every type that encodes a date, sex, region or entity type |

The new-country list from python-stdnum: DZ (NIF), EG (tax number), GH (TIN), GN (NIFP), KE (PIN),
LI (PEID), MA (ICE), MZ (NUIT), OM, SG (UEN), SN (NINEA), TN (MF), VN (MST), and `eu` (VAT, OSS,
EIC, excise, NACE, EC number).

### 6.2 Test-vector corpus

Vectors move out of C# `InlineData` into data files, so they can be reviewed as data, reused by
another implementation, and traced to an origin. One file per type:
`Attest.Tests/corpus/<id>.tsv`, UTF-8, LF, tab-separated, `#` starts a comment.

```
# be.vat  revision=1
# value<TAB>verdict<TAB>reason<TAB>normalized<TAB>formatted<TAB>origin<TAB>note
BE0428759497	valid	-	BE0428759497	BE 0428.759.497	published:https://...	KBO example
BE0428759498	invalid	InvalidChecksum	-	-	computed:mod97	last two digits changed
BE1428759497	invalid	InvalidFormat	-	-	synthetic	starts with 1 after 2005 rule
```

- `verdict`: `valid`, `invalid`, `notcovered`. `reason` is a `Reason` member or `-`.
- `origin`, required, one of: `published:<url>` (an example the authority prints),
  `computed:<method>` (check digit worked out independently of the rule under test),
  `synthetic` (malformed on purpose; only allowed with `invalid`).
  **A `valid` row must be `published` or `computed`.** This is `AGENTS.md` rule 2 made mechanical.
- One test loads every file and asserts verdict, reason, normalized and formatted. A second test
  asserts every registered type has a corpus file with at least one valid and one invalid row of each
  `Reason` the rule can produce, and that no file refers to an unknown id.
- Existing `InlineData` rows migrate with their comments as `note`. A row whose origin cannot be
  established is kept as `invalid`-only evidence or dropped, never promoted to `valid`.
- The sweeps (`UnicodeDigitSweepTests`, `ValidatorClassSweepTests`, `CultureSweepTests`,
  `PrefixStripTests`, `SourceConventionSweepTests`) stay and extend to the new surface. Nothing is
  weakened.

## 7. Migration from 1.x: 2.0.0

The API shape moves, so this is a major, per the repository's own release classes.

- **Version:** 2.0.0. The version comes from the git tag; nobody edits the `1.0.0-dev` placeholder,
  and releasing stays the maintainer's act (`RELEASING.md`).
- **1.x surface stays for the whole 2.x line**, in namespace `Attest` unchanged:
  `CountryValidator`, `ICountryValidator`, `ValidationResult`, `IdentifierKind`, the 87
  `Attest.Countries.*Validator` classes and `Country`. All are `[Obsolete]` with a message naming the
  replacement, and are implemented **on top of the new engine**, so there is one set of rules. They
  are removed in 3.0.
- The 1.x `IdentifierResult` (multi-kind) is renamed `MatchResult`. This is the one source break for
  callers of `CountryValidator.Validate(value, country, kinds)`; the obsolete facade returns a
  `LegacyIdentifierResult` carrying the old shape so existing code compiles.
- **Verdict parity is the rule.** The 4,212 existing tests run unchanged against the shim and must
  pass. The only verdicts allowed to change are the 25 empty pairs (and a verdict change in any other
  rule is a bug fix named in `MIGRATION.md`, never silent).
- `MIGRATION.md` gains a 1.x -> 2.0 section: method map (old call -> new call), the `NotCovered`
  verdict, the renamed result type, and the corpus-backed list of changed verdicts.
- `CHANGELOG.md` is not edited by contributors (rule 9); the maintainer writes the 2.0.0 entry.

## 8. Name

**Keep "Attest".** Reasons: the NuGet id and ~660 downloads are continuity for existing users; the
word fits both layers (a library that attests to well-formedness, a service that attests with a
timestamped proof); renaming costs a deprecated package and a search-ranking reset for a library
that has no brand yet. The cost: "attest" is a common English word (also a JS test tool name and a
sigstore verb), so search results are noisy. Mitigation is positioning in the first README line, not
the package id: "the python-stdnum of .NET". The NuGet tags already carry the local names
(`cpf`, `cnpj`, `pesel`, `nip` ...) that people actually search. **Open:** a name that is unique to
the product could still be worth it before layer 2 ships; it costs least now, while there are ~660
downloads, and most later.

## 9. Delivery: PRs

One task, one branch, one PR; each green before the next opens, none merged by the author.

1. **Design** (this document).
2. **Core API.** `Identifiers`, `IdentifierType`, results, `Reason`, `Verdict`, `Coverage`/`RuleInfo`
   types, algorithms folder, registry, the 1.x adapter, the obsolete shim, input bounds. The 1.x
   suite passes unchanged.
3. **Provenance table.** `RuleInfo` for all 435 pairs, sources extracted from the existing code
   comments, `docs/COVERAGE.md` generated and checked, the source-URL test.
4. **The 25 empty pairs.** Each resolved with a cited source: implemented, or `NotApplicable` /
   `NotPublished`. `Supports` and README table updated with the table test.
5. **Normalize and Format.** Native ports for the highest-traffic types first (EU VAT, BR, IN, US,
   GB, DE, FR, ES, IT, NL, PL, MX, CA, AU, plus IBAN core), then the rest.
6. **Corpus and harness.** The TSV format, the loader, the two consistency tests, migration of
   existing rows country by country.

Later waves (separate PRs): new countries, international identifiers, Parse for every type that
encodes something, `[Identifier]` attribute, the scheduled link check.

## 10. Open decisions

1. **IBAN: own or IbanNet.** Default own (4.1).
2. **Withdraw the stdnum-doctest allowance** in `adding-a-country.md` (4.2). Default yes.
3. **Name.** Default keep (8).
4. **`Reason` granularity and the 128-character bound** (3.2, 3.5). Defaults as written; both are
   cheap to change before 2.0.0 and expensive after.
5. **Static `Identifiers` entry point versus an injectable service.** Default static, because rules
   are stateless. Layer 2 will wrap it in its own service.
