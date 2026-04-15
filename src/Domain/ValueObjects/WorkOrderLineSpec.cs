namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct WorkOrderLineSpec(ProductId ProductId, Quantity Quantity);

public static class WorkOrderLineSpecFactory
{
    public static WorkOrderLineSpec Create(ProductId productId, Quantity quantity) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId.Value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity.Value);
        return new(productId, quantity);
    }
}