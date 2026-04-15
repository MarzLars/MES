namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct WorkOrderLineSpec(
    ProductId ProductId,
    Quantity Quantity);