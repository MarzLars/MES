namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct WorkOrderId(int Value);

public static class WorkOrderIdFactory
{
    public static WorkOrderId Create(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new(value);
    }
}