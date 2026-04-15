namespace SteelOrdering.Api.Contracts.Response;

public sealed record WorkOrderLineResponse(
    int WorkOrderLineId,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitWeightKilograms,
    decimal TotalLineWeightInKilograms);