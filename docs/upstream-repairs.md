# What is different from upstream

The short version of three repair waves, all of it verified by running the code rather than reading
it:

- **Hungary** accepted none of the 36,363,636 valid tax ids there are. Its checksum summed UTF-16
  code units instead of digit values, adding a constant 48 x (1+...+9) = 2160, which is 4 mod 11.
  The check digit it computed was therefore always the correct one plus 4, mod 11, and so could
  never equal it. The count is an enumeration of the whole space; the impossibility is arithmetic.
- **22 of 87 validators threw** instead of returning a result — on `null`, on short input, on a
  letter where a digit belonged. A sweep over every country and every method now proves none does.
- **Mexico and South Africa** could never validate anything: their date helpers always returned
  false. **Malaysia, Canada and San Marino** had inverted guards, so they accepted everything,
  including the empty string, and rejected the real formats.
- **Test data was fiction.** Thailand's tax-code tests used a Swedish personnummer; Cyprus's only
  national-id test was a Czech rodné číslo copied from the Czech file. Both passed for years.
- **Every bug report left open upstream is answered.** Ten of the twelve open issues report a
  defect — the other two ask whether the project is maintained and what the four `Validate` methods
  mean. The ten are the Netherlands' post-2020 btw-identificatienummer, Belgium's check number below
  ten (reported twice), Belgian enterprise numbers starting with 1, Finland's 2023 HETU separators,
  the printed Swiss TVA number, Mexico's unreachable company RFC, Chile, Paraguay and Uruguay, the
  Indian Aadhaar validator throwing on a letter and the GSTIN that replaced the VAT TIN in 2017, and
  the French VAT number capped at nine characters. Each is asserted with its reporter's own value in
  [UpstreamReportTests.cs](Attest.Tests/UpstreamReportTests.cs).

The test suite went from 586 cases to 4,212, and every validator now has one. What is still weak is
written down rather than hidden: [KNOWN-ISSUES.md](KNOWN-ISSUES.md) lists 33 entries by country: 19
still-open gaps, almost all of them check digits no authority publishes, and 14 the repair waves investigated and closed as decided rather than pending.
