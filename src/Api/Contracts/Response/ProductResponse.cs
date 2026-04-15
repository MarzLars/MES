namespace SteelOrdering.Api.Contracts.Response;

public sealed record ProductResponse(
    int Id,
    string Name,
    decimal UnitWeightKilograms,
    DateTimeOffset CreatedDateTimeUtc);