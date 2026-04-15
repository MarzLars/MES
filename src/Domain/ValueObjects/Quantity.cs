namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct Quantity(int Value);

public static class QuantityFactory
{
    public static Quantity Create(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new(value);
    }
}