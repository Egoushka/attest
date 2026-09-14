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

Publishing uses **trusted publishing**, so there is no API key anywhere in this repository. The
workflow asks GitHub for a short-lived OIDC token, nuget.org checks it against a policy that names
this repository and this workflow file, and returns an API key valid for one hour. Nothing to
rotate, nothing to leak.

1. Create a nuget.org account if you do not have one, and note your **profile name** — not the email
   address you sign in with. That is what the login step needs.
2. On nuget.org: your username → **Trusted Publishing** → add a policy.

   | Field | Value |
   |---|---|
   | Repository owner | `Egoushka` |
   | Repository | `attest` |
   | Workflow file | `release.yml` (file name only, no path) |
   | Environment | leave empty |

3. Set the policy **scope** to allow publishing new packages, with the glob `Attest*`. Neither
   `Attest` nor `Attest.DataAnnotations` exists on nuget.org yet, so a policy scoped only to new
   versions of existing packages would reject the first release.
4. Add your nuget.org profile name to the repository:

   ```
   gh secret set NUGET_USER -R Egoushka/attest
   ```

   It is a username rather than a credential, but it lives in a secret because the NuGet
   documentation recommends it and it keeps the workflow file free of personal details.

Two things to know about the policy:

- **It is bound to the workflow file name.** Renaming `.github/workflows/release.yml` breaks
  publishing until you update the policy to match.
- **A new policy can start out temporarily active for seven days**, mostly on private repositories.
  nuget.org needs the GitHub repository and owner IDs, which arrive with the first successful
  publish, to pin the policy against someone deleting the repo and recreating it under the same
  name. If nothing is published in that window the policy goes inactive, and you can restart the
  window whenever you like.

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
