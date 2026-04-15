namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct WorkOrderId(
    int Value)
{
    public static WorkOrderId From(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new WorkOrderId(value);
    }
}