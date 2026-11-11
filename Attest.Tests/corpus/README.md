# Test-vector corpus

One file per identifier type: `<id>.tsv`, UTF-8, LF, tab-separated. `#` starts a comment. The harness is
`CorpusTests`; the format is DESIGN.md section 6.2.

```
# be.vat  revision=1
# value<TAB>verdict<TAB>reason<TAB>normalized<TAB>formatted<TAB>origin<TAB>note
BE0428759497	valid	-	BE0428759497	BE0428759497	computed:mod97	04287594 mod 97 = 0, check number 97
BE0428759498	invalid	InvalidChecksum	-	-	computed:mod97	last digit changed
```

- `value`: as a caller would type it. `\t`, `\n`, `\s` and `\\` stand for tab, newline, a space and a backslash.
- `verdict`: `valid`, `invalid` or `notcovered`. `reason`: a `Reason` member, or `-` for valid and notcovered rows.
- `normalized`, `formatted`: required on valid rows. On invalid rows `normalized` is `-` (not asserted) or the
  expected `Normalize` result, and `formatted` is `-` because `Format` of a rejected value is null.
- `origin`, required:
  - `published:<https url>`: an example the authority prints, from that page.
  - `computed:<method>`: the check digit worked out by hand or by a script that is not the rule under test.
  - `synthetic`: malformed on purpose; only for `invalid` rows.
  A `valid` row must be `published` or `computed` (AGENTS.md rule 2). A number whose origin cannot be shown
  does not go in as `valid`. Do not copy numbers from python-stdnum (LGPL): see DESIGN.md 4.2.
- The `revision=` in the header must equal the rule's revision (`RuleInfo.Revision`). Bumping a revision fails
  the test until the rows are reviewed and the header is updated.
- A type with a rule needs at least one valid and one invalid row.
- Types without a file are listed in `_pending.txt`. The list can only shrink; regenerate it with
  `ATTEST_UPDATE_CORPUS_PENDING=1 dotnet test --filter CorpusTests`.
