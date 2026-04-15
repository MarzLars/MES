namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct WorkOrderLineId(int Value);

public static class WorkOrderLineIdFactory
{
    public static WorkOrderLineId Create(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new(value);
    }
}