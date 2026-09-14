# Notes for coding agents

Read [CONTRIBUTING.md](CONTRIBUTING.md) first. It explains why each rule below exists, naming the
defect that produced it. This file is only the short list of things not to do, because every one of
them has already happened here.

Attest decides whether a real person's identity number is valid. The expensive failure is not a
crash, it is a false reject: someone refused service by software that was confidently wrong about
their country.

## Do not

1. **Do not invent a check digit.** If the authority publishes no algorithm, implement the format
   and the field ranges, and write the gap into `KNOWN-ISSUES.md`. A guessed checksum rejects live
   numbers and nobody reports it to you. Armenia, Azerbaijan and Nigeria are unfinished on purpose.
2. **Do not write a test number you did not compute.** Every value asserted valid is one whose check
   digit you worked out, or a published example from the source cited on that row. Thailand's tests
   used a Swedish personnummer for years and were green the whole time.
3. **Do not change a validator to make a failing test pass.** If a test fails because your fix is
   right, delete the row and say so. If it fails because your fix is wrong, fix the fix.
4. **Do not write a test asserting behaviour you believe is wrong.** A green test locking in a bug
   turns a defect into a specification. Put it in `KNOWN-ISSUES.md` instead.
5. **Do not weaken the sweeps.** `UnicodeDigitSweepTests`, `ValidatorClassSweepTests`,
   `CultureSweepTests` and `PrefixStripTests` exist because each defect they catch shipped once.
   They are not obstacles to your change.
6. **Do not use `\d`, `char.IsDigit`, `ToUpper()`, or `StartsWith(string)`.** Use `[0-9]`,
   `IsAsciiDigits`, `ToUpperInvariant()` and `StripPrefix`. The first set depends on the caller's
   culture or accepts Unicode digits that `int.Parse` rejects.
7. **Do not repair input by deleting a character.** Dropping a bad digit validates the rest and
   turns a wrong number into a right one.
8. **Do not touch the release machinery.** Renaming `.github/workflows/release.yml` breaks
   publishing: nuget.org's trusted-publishing policy names that file. The version comes from the git
   tag; the `1.0.0-dev` in `Directory.Build.props` is a placeholder, not a thing to bump.
9. **Do not edit `CHANGELOG.md`.** The maintainer writes it at release.
10. **Do not reformat files you did not otherwise change.** The BOM, brace and namespace conventions
    in `.editorconfig` are measured and deliberate. Whitespace churn buries the one character that
    actually changed.

## Before you open a pull request

- Every rule you changed cites a published source, in a comment beside the code.
- `dotnet test Attest.Tests/Attest.Tests.csproj` passes, and the build has no warnings.
- Anything you found and did not fix is in `KNOWN-ISSUES.md` with what you checked.
- Say the release class: patch if nothing observable changed, minor if any verdict moved, major if
  the API shape moved. A verdict change is never a patch, even when the new answer is correct.

## Where things are

| You want to | Read |
|---|---|
| add or fix a country | [docs/adding-a-country.md](docs/adding-a-country.md) |
| understand a rule | [CONTRIBUTING.md](CONTRIBUTING.md) |
| find work | [KNOWN-ISSUES.md](KNOWN-ISSUES.md) |
| release | [RELEASING.md](RELEASING.md) |
