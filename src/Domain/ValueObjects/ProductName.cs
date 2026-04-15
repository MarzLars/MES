namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProductName
{
    public ProductName(string value) {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public static ProductName From(string value) {
        return new ProductName(value);
    }

    public override string ToString() {
        return Value;
    }
}