# Releasing

Maintainer runbook. Attest ships two packages, `Attest` and `Attest.DataAnnotations`, both at
1.0.0, neither published yet.

## The mechanism

`.github/workflows/release.yml` runs on any pushed tag matching `v*`. It builds `Attest.sln`, runs
`Attest.Tests`, packs, and pushes every `.nupkg` to nuget.org with `secrets.NUGET_API_KEY`.

**That secret does not exist yet.** Until it is set, pushing a tag builds, tests and packs
successfully and then fails at the `Push to NuGet` step with an empty API key. Nothing ships, and
nothing is left half-published. To set it: create a key on nuget.org with the *Push new packages and
package versions* scope and a glob pattern of `Attest*` — a package-scoped key cannot name packages
that do not exist yet — then add it under Settings → Secrets and variables → Actions as
`NUGET_API_KEY`. Keys expire after at most 365 days; a release that worked last year and now fails
at the push step is usually this.

The workflow does not create a GitHub release, and does not check that the tag matches the version
in the projects. Both are on you.

## Versioning

SemVer, with the boundary drawn where it matters for a validation library.

**MAJOR** — code that compiled against the previous version no longer compiles: a public type,
method or `Country` member removed or renamed, a changed signature, a namespace move. The
`CountryValidation` → `Attest` rename was one of these.

**MINOR** — new countries, new public members (`Supports`, `Validate(value, country, kinds)`), and
**any change to which numbers a validator accepts or rejects**, in either direction.

Widening is easy to justify: Belgium's check numbers below 10, Brazil's alphanumeric CNPJ,
Indonesia's 16-digit NPWP, Bosnia's JIB where `ValidateEntity` used to throw. Nobody is hurt by a
number starting to validate.

Narrowing is the one to be careful with. A validator that starts rejecting numbers it used to accept
is a bug fix in the repo and a breakage at the call site. The wave-3 Russia change is the worked
example: `ValidateIndividualTaxCode` used to accept both the 10-digit company ИНН and the 12-digit
personal one, so callers used it as a generic "is this any ИНН" check. It now accepts only the
personal form, and that code broke — silently, on live data, with no compiler error. Those callers
want `Validate(value, Country.RU, IdentifierKind.Any)`.

Narrowing still ships as MINOR rather than MAJOR. Almost every one of the 197 fixes tightened
something; treating each as MAJOR would put the library in double digits and tell users nothing.
What carries the weight is the CHANGELOG entry, under its own `### Behaviour changes` heading, which
must name what stops validating and what to call instead.

**PATCH** — no verdict changes anywhere: error message wording, docs, packaging, performance. A
method that stops throwing and returns a `ValidationResult` is a patch, not a behaviour change —
the contract always said it returns a result, the throw was never it.

## Where the version lives

Three properties in each of two files, all carrying the same number:

- `Attest/Attest.csproj` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>`
- `Attest.DataAnnotations/Attest.DataAnnotations.csproj` — the same three

`<Version>` is `1.0.0`; the assembly and file versions take the four-part `1.0.0.0`.

Bump both projects together even when only one changed. `Attest.DataAnnotations` has a
`ProjectReference` to `Attest`, so its package takes a dependency on whatever `Attest` version is
being packed. Bump `Attest` alone and you get an `Attest.DataAnnotations` 1.0.0 package depending on
`Attest` 1.0.1 — which then fails to publish, because 1.0.0 is already on nuget.org and
`--skip-duplicate` reports success while shipping nothing.

## Checklist

1. Everything is merged to `main` and CI is green on the merge commit. CI builds `netstandard2.0`
   and `net8.0`, runs `Attest.Tests` on `net9.0`, and packs — the release workflow does the same
   work again, so a red CI is a release that will fail after the tag is already public.
2. `KNOWN-ISSUES.md` reconciled: entries fixed in this release removed, new *not fixed* findings
   added. Entries that flag staleness in other files — the Russia entry pointing at README.md line
   57 and the `IsAmbiguous` XML doc in `Attest/IdentifierResult.cs` — are either resolved or
   carried forward deliberately.
3. `CHANGELOG.md`: change `## X.Y.Z (unreleased)` to `## X.Y.Z — YYYY-MM-DD`. Every rule change in
   it cites its source (tax authority, OECD TIN sheet, or python-stdnum) and every narrowing is
   under `### Behaviour changes`.
4. Bump the six properties above. Commit as `Release X.Y.Z`, push, wait for CI green again.
5. Tag the release commit — `git tag -a vX.Y.Z -m "X.Y.Z" && git push origin vX.Y.Z`. The tag must
   match `<Version>` exactly; nothing checks this, so a `v1.0.1` tag on unbumped projects will
   happily publish a package called 1.0.0.
6. Watch the Release workflow. On success, confirm both packages appear on nuget.org — indexing
   takes a few minutes — and that `Attest.DataAnnotations X.Y.Z` lists `Attest X.Y.Z` as its
   dependency.
7. Create the GitHub release from the tag by hand, with the CHANGELOG section as the body.

A tag pushed from a branch other than `main`, or from a commit that is not the version bump, will
publish whatever that commit contains. There is no guard.

## Changelog

`CHANGELOG.md` is written as the work happens, not at release time. Open a
`## X.Y.Z (unreleased)` section on the first change after a release and append to it; step 3 above
is meant to be a date substitution and nothing else.

Entries are grouped by country, name the method, and say what the defect produced — "Belarus
demanded two leading letters that no UNP has, so every genuine company number was rejected" — not
"fixed Belarus validation". That is what makes the file usable when someone hits the same number two
years from now.

## When a bad version reaches NuGet

nuget.org allows unlisting. It does not allow deletion. A published version stays downloadable
forever for anyone who pins it, and existing lockfiles keep restoring it. Re-uploading the same
version number is rejected, and the workflow's `--skip-duplicate` turns that rejection into a green
run that shipped nothing — so a fix is always a new version number.

1. Unlist the bad version on nuget.org. It disappears from search and from version resolution for
   new installs; that is the whole effect.
2. Ship a patch, through the full checklist above.
3. Add a CHANGELOG line under the bad version saying it was unlisted, why, and which version
   replaces it.

Worth an emergency patch: a validator that regressed to throwing instead of returning a
`ValidationResult` (`UnicodeDigitSweepTests` should have caught it, so also work out why it did
not), or one that rejects live numbers — the Belarus defect is the shape of this, and it sat in the
upstream project for years. Not worth one: a wrong error message, or a validator that is merely
less strict than the published rule. Those wait for the next release and go in `KNOWN-ISSUES.md`
meanwhile.

A tag matching `v*` with a prerelease suffix — `v1.1.0-rc.1`, with the projects set to `1.1.0-rc.1`
— publishes as a NuGet prerelease, which is the cheap way to put a wave that changes many verdicts
in front of someone before it lands as a stable version.
