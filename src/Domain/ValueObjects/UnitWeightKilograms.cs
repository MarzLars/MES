namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct UnitWeightKilograms(decimal Value);

public static class UnitWeightKilogramsFactory
{
    public static UnitWeightKilograms Create(decimal value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new(value);
    }
}