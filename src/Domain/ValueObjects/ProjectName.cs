namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProjectName
{
    public ProjectName(string value) {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public static ProjectName From(string value) {
        return new ProjectName(value);
    }

    public override string ToString() {
        return Value;
    }
}