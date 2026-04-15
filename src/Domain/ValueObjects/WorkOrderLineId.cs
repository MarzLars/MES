namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct WorkOrderLineId(
    int Value)
{
    public static WorkOrderLineId From(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new WorkOrderLineId(value);
    }
}