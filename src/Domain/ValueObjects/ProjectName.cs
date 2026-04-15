namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProjectName(string Value);

public static class ProjectNameFactory
{
    public static ProjectName Create(string value) {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new(value.Trim());
    }
}