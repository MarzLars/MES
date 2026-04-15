namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProjectId(int Value);

public static class ProjectIdFactory
{
    public static ProjectId Create(int value) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return new(value);
    }
}