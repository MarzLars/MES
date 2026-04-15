namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProductId(
    int Value)
{
    public static ProductId From(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new ProductId(value);
    }
}