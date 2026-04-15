namespace SteelOrdering.Domain.ValueObjects;

public abstract record ProductInventoryStatus;

public sealed record InStock : ProductInventoryStatus;
public sealed record LowStock : ProductInventoryStatus;
public sealed record OutOfStock : ProductInventoryStatus;
public sealed record Discontinued : ProductInventoryStatus;

public static class ProductInventoryStatusFactory
{
    public static ProductInventoryStatus Create(string value) => value switch {
        null or "" => throw new ArgumentException("Value cannot be null or empty.", nameof(value)),
        "InStock" => CreateInStock(),
        "LowStock" => CreateLowStock(),
        "OutOfStock" => CreateOutOfStock(),
        "Discontinued" => CreateDiscontinued(),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown inventory status.")
    };

    public static ProductInventoryStatus CreateInStock() => new InStock();
    public static ProductInventoryStatus CreateLowStock() => new LowStock();
    public static ProductInventoryStatus CreateOutOfStock() => new OutOfStock();
    public static ProductInventoryStatus CreateDiscontinued() => new Discontinued();
}
