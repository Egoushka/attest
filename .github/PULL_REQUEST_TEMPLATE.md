## What changes

<!-- The country, the method, and the verdict that changes. One or two sentences. -->

## Source

<!--
The tax authority page, the OECD TIN country sheet, or the python-stdnum module the rule comes
from, with a URL. Put the same citation in a comment next to the code. A rule from memory is not a
rule: Belarus's UNP validator demanded two leading letters that no UNP has, and rejected every
genuine Belarusian company number for years, because nobody checked.
-->

## Checklist

- [ ] The rule change cites a published source, here and in a comment beside the code.
- [ ] Every `valid` number in a test is one whose check digit I computed, or a published example
      cited in that row's comment. Copied numbers certify nothing — Thailand's tax-code tests used
      a Swedish personnummer, Cyprus's only national-ID test was a Czech rodné číslo taken from the
      Czech file, and both passed for years.
- [ ] The theory has negative rows as well as positive ones: a wrong check digit, the wrong holder
      type, the wrong length. Three rows that all expect `true` prove nothing, which is how
      Germany's `ValidateEntity` went years without verifying the Prüfziffer it captured.
- [ ] No test asserts behaviour I believe is wrong. Where I could not fix a validator safely I left
      the code, skipped the test, and added the finding to `KNOWN-ISSUES.md`. Russia's tests used
      to assert that Sberbank's and Gazprom's company numbers were valid *personal* tax codes.
- [ ] Every public `Validate*` method I touched returns a `ValidationResult` for null, empty,
      whitespace and garbage, and throws for nothing. `UnicodeDigitSweepTests` enforces this for
      all 87 countries and will fail this PR otherwise.
- [ ] Input goes through `IdExtensions.RemoveSpecialCharacthers`, nothing deletes a character to
      make a number parse, and digits are guarded with `[0-9]` rather than `\d` or `char.IsDigit`
      anywhere a parse follows.
- [ ] If this makes one number valid as both a personal and a business identifier, that is sourced
      and reported through `IdentifierResult.IsAmbiguous`, not a silent delegation from
      `ValidateEntity` to `ValidateIndividualTaxCode`. Peru, Andorra, Iceland and Thailand all
      looked ambiguous and were bugs — each encodes the holder type.
- [ ] `CHANGELOG.md` has an entry if a caller can see the difference.
- [ ] `dotnet test Attest.Tests/Attest.Tests.csproj` passes locally.
- [ ] No number in the diff or in this description belongs to a real person.
