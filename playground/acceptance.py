#!/usr/bin/env python3
"""Machine-run acceptance checks (SPEC.md A1-A11) against a running playground.

    python3 -I playground/acceptance.py http://127.0.0.1:8080

Exits non-zero on the first failure. A10 sends enough requests to trip the rate limit, so point it
at a local run or a deployment you own, from a client address of its own.
"""
import json
import sys
import urllib.error
import urllib.request

base = sys.argv[1].rstrip("/") if len(sys.argv) > 1 else sys.exit(__doc__)


def call(path, body=None):
    req = urllib.request.Request(base + path, data=body, headers={"Content-Type": "application/json"} if body else {})
    try:
        with urllib.request.urlopen(req, timeout=10) as r:
            return r.status, dict(r.headers), r.read()
    except urllib.error.HTTPError as e:
        return e.code, dict(e.headers), e.read()


def check(name, ok):
    print(("PASS " if ok else "FAIL ") + name)
    if not ok:
        sys.exit(1)


def j(raw):
    return json.loads(raw)


s, _, _ = call("/healthz")
check("A1 healthz 200", s == 200)

s, h, b = call("/")
check("A2 index 200 with CSP and no inline script",
      s == 200 and "script-src 'self'" in h.get("Content-Security-Policy", "") and "<script>" not in b.decode())

s, _, b = call("/v1/validate?value=BE0403019261&country=BE")
check("A3 valid BE VAT", s == 200 and j(b)["valid"] is True)

s, _, b = call("/v1/validate?value=BE0403019262&country=BE")
d = j(b)
check("A4 changed digit fails checksum", s == 200 and d["valid"] is False and any(r["reason"] == "invalid_checksum" for r in d["results"]))

s, _, b = call("/v1/validate?value=BE0403019261")
d = j(b)
check("A5 auto-detect finds be.vat", d["valid"] is True and any(r["type"] == "be.vat" for r in d["results"]))

s, _, b = call("/v1/validate", b'{"value":"BE0403019261","country":"BE"}')
check("A6 POST like GET", s == 200 and j(b)["valid"] is True)

s, _, b = call("/v1/validate?value=1&country=ZZ")
check("A7 unknown country is 400 with a code", s == 400 and j(b)["error"] == "unknown_country")

s, _, _ = call("/v1/validate?value=" + "1" * 257)
check("A8 value over 256 chars is 400", s == 400)

s, _, _ = call("/v1/validate", b'{"value":"' + b"1" * 4000 + b'"}')
check("A9 body over 2 KiB refused", s in (400, 413))

s, _, b = call("/v1/countries")
be = [c for c in j(b)["countries"] if c["code"] == "BE"]
check("A11 countries lists BE with be.vat", s == 200 and be and any(t["id"] == "be.vat" for t in be[0]["types"]))

limited = None
for _ in range(400):
    s, h, _ = call("/v1/validate?value=BE0403019261&country=BE")
    if s == 429:
        limited = h
        break
check("A10 429 with Retry-After under load", limited is not None and "Retry-After" in limited)
