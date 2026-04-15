namespace SteelOrdering.Api.Contracts.Response;

public sealed record ProjectResponse(
    int Id,
    string Name,
    DateTimeOffset CreatedDateTimeUtc);