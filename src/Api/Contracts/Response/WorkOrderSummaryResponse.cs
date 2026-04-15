namespace SteelOrdering.Api.Contracts.Response;

public sealed record WorkOrderSummaryResponse(
    int WorkOrderId,
    int ProjectId,
    string ProjectName,
    DateTimeOffset CreatedDateTimeUtc,
    int LineCount,
    decimal TotalWeightInKilograms);