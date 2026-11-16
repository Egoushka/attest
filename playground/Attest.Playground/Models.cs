using System.Text.Json.Serialization;

namespace Attest.Playground;

internal sealed record ValidateRequest(string? Value, string? Country, string? Type);

internal sealed record IdentifierItem(
    string Type,
    string? Country,
    string Kind,
    string Name,
    string Verdict,
    bool Valid,
    string Reason,
    string? Detail,
    string? Normalized,
    string? Formatted);

internal sealed record ValidateResponse(bool Valid, string Mode, string Message, IReadOnlyList<IdentifierItem> Results);

internal sealed record ErrorResponse(string Error, string Message);

internal sealed record TypeInfo(string Id, string Name, string Kind);

internal sealed record CountryInfo(string Code, IReadOnlyList<TypeInfo> Types);

internal sealed record CountriesResponse(IReadOnlyList<CountryInfo> Countries);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ValidateRequest))]
[JsonSerializable(typeof(ValidateResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(CountriesResponse))]
internal sealed partial class Json : JsonSerializerContext;
