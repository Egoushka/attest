# Security policy

Attest is an offline parsing library. It takes a string and a `Country`, runs regular expressions
and check-digit arithmetic over it, and returns a `ValidationResult`. It opens no sockets, reads no
files, logs nothing, holds no secrets, and keeps nothing after the call returns. The `Attest`
package has no dependencies; `Attest.DataAnnotations` has one, `System.ComponentModel.Annotations`
4.7.0.

So confidentiality is not the interesting question here. What matters is what a hostile string can
do to the process that calls into the library, because the values reaching a validator are usually
typed into a form by someone you do not control.

## In scope

**A validator that throws.** The contract is that every public `Validate*` method returns a
`ValidationResult` for any input at all — null, empty, whitespace, punctuation, another script, a
very long string. It never throws. A throw inside a caller's validation path turns a bad form field
into an unhandled exception, which is an availability bug in their service. This was not
theoretical: an audit of all 87 validators found that 22 of them threw (see CHANGELOG.md).
`BulgariaValidator.ValidateEntity(null)` threw `NullReferenceException` at `vat.Length`;
`CroatiaValidator.ValidateVAT` threw `ArgumentNullException` by explicit design; Estonia, the United
States, Poland and Bolivia parsed unsanitised input, so an Arabic-Indic digit reached `int.Parse`
and came back as `FormatException`. `Attest.Tests/UnicodeDigitSweepTests.cs` now sweeps all 87
countries across all six entry points against null, `""`, `"   "`, `"---"`, `"abc"` and five
non-ASCII digit strings, and fails the build on a regression. A throw that gets past that sweep is
in scope.

**Input that makes a validator slow.** There are 248 `Regex.IsMatch` call sites and not one sets a
match timeout; no entry point caps the length of the input; and
`IdExtensions.RemoveSpecialCharacthers` copies the whole string before any format check runs. The
patterns in the tree today are anchored at both ends and bounded by explicit `{n}` counts — the
heaviest is the 431-character postcode alternation at
`Attest/CountriesValidators/UnitedKingdomValidator.cs:147` — and no input is known that drives one
of them superlinear. But nothing in the library would contain it if one were introduced. If you
have a country, a method and a string where the call takes longer than milliseconds, that is worth
reporting.

## Out of scope

**A wrong verdict on a well-formed number.** That is a validation bug, not a vulnerability. Open a
normal issue and cite a source the fix can be made from: the country's tax authority, an OECD TIN
sheet, or python-stdnum. KNOWN-ISSUES.md already records the gaps that are known and deliberate —
Armenia's TIN check digit is unverified because the State Revenue Committee has never published the
algorithm, Azerbaijan's VÖEN is length-only for the same reason.

**Anything about the values you pass in being stored, logged or transmitted.** They are not. There
is nowhere in this library for them to go.

**One known throw that is documented rather than fixed.** `CountryValidator` catches
`NotSupportedException` and `NotImplementedException` from a dispatch and answers
`Invalid("Not supported")`. The per-country validator classes, which the README documents as a
direct entry point, do not. Eleven of them still throw for a method they have no rule for (Korea,
China, Pakistan, Hong Kong, Uzbekistan, Cuba, United States, United Arab Emirates, Mauritius,
Bahrain, Monaco), so `new MonacoValidator().ValidateIndividualTaxCode(x)` throws for every `x`.
It is in KNOWN-ISSUES.md. Report it as an issue if it bites you, not as a vulnerability.

## Supported versions

| Version | Supported |
| --- | --- |
| 1.0.x | Yes |
| CountryValidator (the upstream project this forked) | No |

Attest is at its first release under this name. Fixes ship forward as a new patch version on NuGet;
there are no maintenance branches and nothing is backported. CountryValidator is a separate project
with no release since 1.1.3 — do not report its bugs here, and do not report Attest's bugs there.

## How to report

Use GitHub's private vulnerability reporting on this repository:
<https://github.com/Egoushka/attest/security/advisories/new> (Security tab → Report a
vulnerability). It stays private between you and the maintainer until an advisory is published. Do
not open a public issue for something exploitable.

There is no security email address, no PGP key and no bounty.

Include the package version, the target framework (`netstandard2.0` or `net8.0`), the country and
method, and the exact input as a C# string literal or as escaped code points. Several of the bugs
this policy exists for are triggered by characters that do not survive being copied through a text
box — `٥` looks like `5` in most fonts.

## What to expect

One unpaid maintainer in one time zone, so this is best effort with no service level:

- an acknowledgement within about a week, and a verdict on whether it reproduces once I have run it;
- a fix released as a patch version on NuGet, with a GitHub advisory published alongside it;
- credit under whatever name or handle you ask for, or none if you prefer.

If a month passes with no reply, assume the report was missed rather than ignored. Open a public
issue saying only that you sent a private report — no detail — and it will get picked up.
