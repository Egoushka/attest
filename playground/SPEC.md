# Attest playground and public API: spec

Goal: the most-read page of hrabovskyi.online (the Attest post) gets something to do. A reader pastes
a tax ID or VAT number, optionally picks the country, and sees the verdict and the reason at once.
The same engine is a small free HTTP API. Value hypothesis: readers become GitHub stars and NuGet
installs. Check after 4 weeks: `validate` log events per day and the NuGet download trend.

Not in scope: payments, pricing, accounts, API keys, a batch endpoint (reserved for a later paid
line), network calls to VIES (Attest checks form only, so the page says so).

## Design choice

One ASP.NET Core minimal API compiled with Native AOT, serving the static page and `/v1/*` from
the same container at `attest.hrabovskyi.online`.

| Option | Verdict |
|---|---|
| JS port of the rules | Rejected. 87 countries of rules would live twice and drift; a false reject is the library's one expensive failure (AGENTS.md). |
| .NET WASM in the browser | Rejected for now. A trimmed runtime plus the library is several MB before the page paints, and the free API needs a server anyway. |
| Minimal API, Native AOT, page served by it (chosen) | One code path (the NuGet library itself, referenced as a project), one image, no framework in the page. |

Because the page calls the same API, "playground events" and "API usage" are one stream.

## Abuse limits (security)

Input is a short string; the engine is microseconds of CPU, so the risks are volume and logging.

1. Edge: Traefik `cloudflare-only`, a per-client rate limit keyed on `CF-Connecting-IP`, an in-flight cap.
2. App: sliding window per client IP (default 60 per minute) plus a global token bucket, 429 with `Retry-After`.
3. Kestrel: body 2 KiB, header timeout 5 s, 512 connections. Value over 128 chars is rejected by the library (`too_long`); over 256 here.
4. Submitted values are never logged. A log line holds the source, country, type, verdict and reason only.
5. CORS `*` on `/v1/*` without credentials: there is no cookie or secret to protect.
6. No outbound requests, no file or database access: nothing to SSRF or exfiltrate.

## Acceptance checks (machine-run)

`python3 -I playground/acceptance.py BASE_URL` runs them and exits non-zero on the first failure; the
tests project covers the same cases in process (`dotnet test playground/Attest.Playground.Tests`).

- A1 `GET /healthz` is 200.
- A2 `GET /` is 200 HTML with a CSP header and no inline script.
- A3 Valid Belgian VAT `BE0403019261` with `country=BE` gives `valid:true`.
- A4 The same number with one digit changed gives `valid:false`, reason `invalid_checksum`.
- A5 Auto-detect: `value=BE0403019261` with no country gives `valid:true` and a `be.vat` result.
- A6 `POST /v1/validate` with a JSON body behaves like the GET.
- A7 Unknown country `ZZ` gives 400 with an error code, never 500.
- A8 A value over 256 characters gives 400.
- A9 A body over 2 KiB gives 413 or 400.
- A10 More than the limit in a minute from one client gives 429 with `Retry-After`.
- A11 `GET /v1/countries` lists countries with their identifier types.
- A12 The image builds with Native AOT in CI and `/healthz` answers from the container.
- A13 Design gate: screenshots at 375 and 1280 px, dark and light, pass axe and the slop linter.

A12 runs in `.github/workflows/playground.yml` (AOT publish with `-warnaserror`, then A1-A11 against
the AOT binary, then the image smoke). A13 result, 2026-11-16: the slop linter passes at the floor
tier except rule 20 (the Hallmark stamp comment, which belongs to pages Hallmark generated); axe
(lighthouse accessibility) 100/100 and the Impeccable detector 0 findings at 375 and 1280 px; dark
and light screenshots were read for the empty, valid, invalid states.

## Deploy

Image `ghcr.io/egoushka/attest-playground`, built and smoked by the workflow, pushed on merge to main.
The homelab stack `attest-playground` pins its digest and sits on a private network with Traefik, which
routes `attest.hrabovskyi.online` (Cloudflare proxied, `cloudflare-only`, rate limit on
`CF-Connecting-IP`). The app reads that header via `Playground__ClientIpHeader`.
