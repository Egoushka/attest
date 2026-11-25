# Promotion plan

Plan only. Nothing is posted or opened without Yehor. Read 2026-11-11.

## 1. README (done in this PR)
It starts with a one-line pitch, the problem (a bad VAT number found weeks late), and a five-line example
I ran against the repo's `Attest` project: `Identifiers.ValidateVat("BE0428759496")` returns `Invalid`,
reason `InvalidChecksum`; the real number `BE0428759497` returns `Valid`. Upstream history and the category
guide moved to `docs/` with no fact deleted. `ws readme check` needs python3, which this box lacks, so
it was not run; run it before merging.

## 2. One catalog listing: awesome-dotnet
Repo: [quozd/awesome-dotnet](https://github.com/quozd/awesome-dotnet), the list most .NET developers
check. Its contribution rules ask for projects that are maintained, stable, documented and tested, one
link per pull request, in the format `[Name](link) - Description` (source: that repo's CONTRIBUTING.md,
not re-read for this draft; open it before sending). Attest has CI, 4,212 tests and docs, so it qualifies.

Draft entry (a validation section, alphabetical):

```
[Attest](https://github.com/Egoushka/attest) - Validates national ID, tax ID, VAT and postal codes for 87 countries, in-process.
```

Check: the PR is merged and the NuGet downloads page shows a rise in the following 14 days (starting
value: any rise above the daily baseline Yehor reads from nuget.org first).
Stop result: the PR is closed without merge, or merged with no change in downloads after 30 days. Then
stop listing hunts and spend the time on the consumer below.

## 3. One real consumer
No public project that depends on Attest was found. A consumer proves the library in real use, so it is
the item that matters most. Candidates, in order:
1. `Attest.PackageConsumer` inside this repo: already there, but it is the library's own test, not an
   outside user. It counts only as a floor.
2. A public repo of Yehor's that validates identifiers (for example a small CLI or an MCP tool): the
   cheapest real consumer, built in one session.
3. An outside project: open one issue or PR on an open-source .NET project that hand-rolls VAT or ID
   checks, offering the swap. One ask only, no outreach lists.

Check: one public repo outside `Egoushka/attest` lists the `Attest` package in a project file
(`gh search code "Include=\"Attest\"" --language xml`).
Stop result: none by day 30 means the library has no pull from outside and stays parked.

## Business gate
| Check | Result |
|---|---|
| P1 to P4 | Pass: free Apache-2.0 library promotion, no paid tier, no nagging |
| E1 to E3 | Pass: each step has a check and a stop result; the baseline is a starting value |
| Marketing | Pass: proof is a command anyone can run; one listing, no vote asking |
| Sales | Pass: no outreach; any ask to a project is a single public issue Yehor writes |
| §5 reviewer | Not required: no price, partner offer or launch copy |
