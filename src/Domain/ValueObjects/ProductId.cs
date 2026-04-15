namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProductId(int Value);

public static class ProductIdFactory
{
    public static ProductId Create(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new(value);
    }
}