namespace SteelOrdering.Api.Contracts.Response;

public sealed record WorkOrderResponse(
    int WorkOrderId,
    int ProjectId,
    string ProjectName,
    DateTimeOffset CreatedDateTimeUtc,
    IReadOnlyCollection<WorkOrderLineResponse> Lines,
    decimal TotalWeightInKilograms);