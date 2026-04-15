namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct Quantity
{
    public Quantity(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }

    public int Value { get; }

    public static Quantity From(int value) {
        return new Quantity(value);
    }

    public override string ToString() {
        return Value.ToString();
    }
}