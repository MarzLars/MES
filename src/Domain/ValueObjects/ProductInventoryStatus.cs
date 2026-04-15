namespace SteelOrdering.Domain.ValueObjects;

public sealed record ProductInventoryStatus
{
    ProductInventoryStatus(string value) {
        Value = value;
    }

    public static ProductInventoryStatus InStock { get; } = new(nameof(InStock));
    public static ProductInventoryStatus LowStock { get; } = new(nameof(LowStock));
    public static ProductInventoryStatus OutOfStock { get; } = new(nameof(OutOfStock));
    public static ProductInventoryStatus Discontinued { get; } = new(nameof(Discontinued));

    public string Value { get; }

    public static ProductInventoryStatus From(string value) {
        ArgumentException.ThrowIfNullOrEmpty(value);

        return value switch {
            nameof(InStock) => InStock,
            nameof(LowStock) => LowStock,
            nameof(OutOfStock) => OutOfStock,
            nameof(Discontinued) => Discontinued,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown inventory status.")
        };
    }

    public override string ToString() {
        return Value;
    }
}