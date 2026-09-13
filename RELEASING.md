# Releasing

The git tag is the version. Nothing in the repository records it: `Directory.Build.props` carries
`1.0.0-dev` so local builds produce something, and the release workflow overrides it with
`-p:Version` read from the tag. The two packages therefore cannot drift apart, and a package on
NuGet always corresponds to exactly one commit.

## Versioning

Semantic versioning, with one rule that matters more here than in most libraries.

**A change of verdict is never a patch.** This library answers yes or no about people's
identification numbers. A caller who upgrades and finds numbers they were accepting now rejected has
a production incident, whether or not the new answer is the correct one. So:

| Change | Version |
|---|---|
| Public API changes shape: a method signature, an enum member removed, a type renamed | **major** |
| A verdict changes for any input — a rule corrected, a country's spec updated, a validator narrowed or widened | **minor** |
| Nothing any caller can observe: documentation, tests, internal refactoring, performance | **patch** |

The wave 3 Russia change is the worked example. Splitting the 10-digit company INN from the 12-digit
personal one was a bug fix by any reading — no natural person holds a 10-digit INN — but code using
`ValidateIndividualTaxCode` as a generic "is this any INN" check started failing. That is a minor
release with a loud changelog entry, not a patch.

Crash fixes are the one soft edge. A method that used to throw and now returns
`Invalid("Not supported")` technically changes behaviour, but no caller can have depended on an
exception that the facade was already swallowing. Patch is fine.

Pre-releases use a SemVer suffix: `v1.1.0-rc.1` publishes as a NuGet prerelease and is marked as a
prerelease on GitHub. Use one when a wave has changed many verdicts at once.

## Before the first release

The `NUGET_API_KEY` secret does not exist yet, so pushing a tag today builds, tests, packs, and then
fails at the push step having shipped nothing.

1. Sign in at nuget.org, then **API Keys → Create**. Scope it to **Push** and, for the first
   release, glob pattern `Attest*` so one key covers both packages.
2. Add it to the repository. Either paste it at
   `https://github.com/Egoushka/attest/settings/secrets/actions/new` as `NUGET_API_KEY`, or run:

   ```
   gh secret set NUGET_API_KEY -R Egoushka/attest
   ```

   which prompts for the value so it never appears in your shell history.
3. NuGet keys expire — a year at most. Put the expiry in your calendar; a release that fails at the
   push step with a 401 is almost always an expired key.

## Releasing

1. Check the working tree is on `main` and CI is green.
2. Update `CHANGELOG.md`: rename the `## 1.0.0 (unreleased)` heading to the version being released,
   dated. The release workflow **refuses to publish a version with no changelog section**, and that
   section becomes the GitHub release notes verbatim.
3. Decide the number using the table above. If any verdict changed, it is at least a minor.
4. Tag and push:

   ```
   git tag v1.0.0
   git push origin v1.0.0
   ```

5. Watch the run: `gh run watch -R Egoushka/attest`. It validates the tag is SemVer, checks the
   changelog, builds, tests, packs, pushes both packages plus their symbol packages to NuGet, and
   opens a GitHub release with the artifacts attached.
6. The package takes a few minutes to appear on nuget.org while it is indexed and scanned.

## When a bad version ships

nuget.org does not allow deletion. A published version is permanent, because other people's builds
may already depend on it.

What you can do is **unlist** it, which hides it from search and from `dotnet add package` while
leaving it restorable for anyone who already pinned it: nuget.org → the package → the version →
Listing → uncheck. Then fix the defect and release the next patch or minor. Never try to reuse the
version number — NuGet will reject the push, which is `--skip-duplicate` quietly doing its job.
