namespace Attest.Playground;

/// <summary>Maps a request onto Attest's identifier registry. Pure: no HTTP, no logging.</summary>
internal static class Engine
{
    public const int MaxValueLength = 256;
    private const int MaxDetectResults = 12;

    // Postal codes, bank accounts, IMEI and the like are not tax IDs: the playground answers
    // "is this a person or business identifier", the same set IdentifierKind.Any means.
    private const IdentifierKind Wanted = IdentifierKind.Any;

    private static readonly Lazy<CountriesResponse> CountryList = new(BuildCountries);

    public static CountriesResponse Countries => CountryList.Value;

    public static (ValidateResponse? Response, ErrorResponse? Error) Run(ValidateRequest request)
    {
        string? value = request.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return (null, new ErrorResponse("empty_value", "Send a value to validate."));
        }

        if (value.Length > MaxValueLength)
        {
            return (null, new ErrorResponse("value_too_long", "The value is longer than " + MaxValueLength + " characters."));
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            IdentifierType? type = Identifiers.Find(request.Type!.Trim().ToLowerInvariant());
            if (type == null)
            {
                return (null, new ErrorResponse("unknown_type", "No identifier type with that id. See /v1/countries."));
            }

            IdentifierItem item = Item(type, value);
            return (Respond("type", new[] { item }), null);
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            if (!TryCountry(request.Country!, out Country country))
            {
                return (null, new ErrorResponse("unknown_country", "Use an ISO 3166-1 alpha-2 code Attest supports. See /v1/countries."));
            }

            var items = new List<IdentifierItem>();
            foreach (IdentifierType type in Identifiers.For(country, Wanted))
            {
                items.Add(Item(type, value));
            }

            if (items.Count == 0)
            {
                return (null, new ErrorResponse("no_rules", "Attest has no person or business identifier for that country."));
            }

            return (Respond("country", Order(items)), null);
        }

        return (Detect(value), null);
    }

    private static ValidateResponse Detect(string value)
    {
        var valid = new List<IdentifierItem>();
        foreach (IdentifierType type in Identifiers.All)
        {
            if ((type.Kind & Wanted) == 0 || type.Coverage == Coverage.NotCovered)
            {
                continue;
            }

            IdentifierResult result = type.Validate(value);
            if (result.IsValid)
            {
                valid.Add(ToItem(type, result));
            }
        }

        if (valid.Count > 0)
        {
            if (valid.Count > MaxDetectResults)
            {
                valid.RemoveRange(MaxDetectResults, valid.Count - MaxDetectResults);
            }

            return Respond("detect", valid);
        }

        // Nothing matched as given. A VAT number carries its country, so say why that one failed.
        IdentifierResult vat = Identifiers.ValidateVat(value);
        if (vat.Type != null)
        {
            return Respond("detect", new[] { ToItem(vat.Type, vat) });
        }

        return new ValidateResponse(false, "detect",
            "No identifier of any country matches. Pick the country, or start a VAT number with its two-letter prefix.",
            Array.Empty<IdentifierItem>());
    }

    private static ValidateResponse Respond(string mode, IReadOnlyList<IdentifierItem> items)
    {
        bool anyValid = items.Any(i => i.Valid);
        string message;
        if (anyValid)
        {
            int n = items.Count(i => i.Valid);
            message = n == 1 ? "Valid: well formed, check digits pass." : n + " identifier types accept this value.";
        }
        else if (items.Count == 1 && items[0].Verdict == "not_covered")
        {
            message = "Attest has no rule for this type, so it has no opinion.";
        }
        else
        {
            message = "Not valid for the types checked.";
        }

        return new ValidateResponse(anyValid, mode, message, items);
    }

    private static List<IdentifierItem> Order(List<IdentifierItem> items)
    {
        // Valid first, then by type id, so the order does not change between runs.
        return items.OrderByDescending(i => i.Valid).ThenBy(i => i.Type, StringComparer.Ordinal).ToList();
    }

    private static IdentifierItem Item(IdentifierType type, string value) => ToItem(type, type.Validate(value));

    private static IdentifierItem ToItem(IdentifierType type, IdentifierResult result)
    {
        string verdict = result.Verdict switch
        {
            Verdict.Valid => "valid",
            Verdict.NotCovered => "not_covered",
            _ => "invalid",
        };

        return new IdentifierItem(
            type.Id,
            type.Country?.ToString(),
            KindName(type.Kind),
            type.Name,
            verdict,
            result.IsValid,
            ReasonCode(result.Reason),
            result.IsValid ? null : result.Detail,
            result.Normalized,
            result.IsValid ? type.Format(result.Normalized) : null);
    }

    internal static string ReasonCode(Reason reason) => reason switch
    {
        Reason.None => "none",
        Reason.Empty => "empty",
        Reason.TooLong => "too_long",
        Reason.InvalidCharacters => "invalid_characters",
        Reason.InvalidLength => "invalid_length",
        Reason.InvalidFormat => "invalid_format",
        Reason.InvalidChecksum => "invalid_checksum",
        Reason.InvalidDate => "invalid_date",
        Reason.InvalidComponent => "invalid_component",
        Reason.InvalidPrefix => "invalid_prefix",
        _ => "unknown",
    };

    private static string KindName(IdentifierKind kind) => kind.ToString();

    internal static bool TryCountry(string text, out Country country)
    {
        country = Country.XX;
        string code = text.Trim().ToUpperInvariant();
        if (code == "EL")
        {
            code = "GR";
        }

        // Enum.TryParse accepts "1" and "1,2": only two letters are a country code.
        if (code.Length != 2 || code[0] < 'A' || code[0] > 'Z' || code[1] < 'A' || code[1] > 'Z')
        {
            return false;
        }

        return Enum.TryParse(code, out country) && country != Country.XX && Identifiers.For(country).Count > 0;
    }

    private static CountriesResponse BuildCountries()
    {
        var list = new List<CountryInfo>();
        foreach (Country country in Enum.GetValues<Country>())
        {
            if (country == Country.XX)
            {
                continue;
            }

            var types = Identifiers.For(country, Wanted)
                .Where(t => t.Coverage != Coverage.NotCovered)
                .Select(t => new TypeInfo(t.Id, t.Name, KindName(t.Kind)))
                .ToList();
            if (types.Count > 0)
            {
                list.Add(new CountryInfo(country.ToString(), types));
            }
        }

        return new CountriesResponse(list.OrderBy(c => c.Code, StringComparer.Ordinal).ToList());
    }
}
