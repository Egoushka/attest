# Changelog

## 1.2.0 (unreleased)

First release of this fork, based on CountryValidator 1.1.3.

### Fixed

- **Belgium** — national register numbers whose check number is below 10 were rejected. The check
  number was compared as text, so a computed `1` never matched the `01` printed in the number.
  Roughly 9% of all Belgian numbers were affected. Upstream issues
  [#10](https://github.com/anghelvalentin/CountryValidator/issues/10) and
  [#22](https://github.com/anghelvalentin/CountryValidator/issues/22).
- **Thailand** — `ValidateIndividualTaxCode` returned success for any input that was not exactly ten
  digits, and rejected the ten digit numbers it did recognise. Thai tax identification numbers are
  13 digits since 1 February 2012, so it now validates the 13 digit number and its check digit.
  Test data for the individual, entity and VAT cases was Swedish, and has been replaced.
- **Thailand** — `ValidateNationalIdentity` accepted 13 characters of any kind; it now requires digits.

### Changed

- Target frameworks are `netstandard2.0` and `net8.0`. `netstandard2.1` and `net48` were dropped.
- Tests run on `net9.0` with xunit 2.9.2.
- Package ids are `CountryValidator.Community` and `CountryValidator.DataAnnotations.Community`.
  Namespaces and assembly names are unchanged, so this is a drop-in replacement.
