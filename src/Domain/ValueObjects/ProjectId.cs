namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProjectId(
    int Value)
{
    public static ProjectId From(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new ProjectId(value);
    }
}