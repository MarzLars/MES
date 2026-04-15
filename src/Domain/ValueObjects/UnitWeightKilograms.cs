namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct UnitWeightKilograms
{
    public UnitWeightKilograms(decimal value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }

    public decimal Value { get; }

    public static UnitWeightKilograms From(decimal value) {
        return new UnitWeightKilograms(value);
    }

    public override string ToString() {
        return Value.ToString("0.###");
    }
}