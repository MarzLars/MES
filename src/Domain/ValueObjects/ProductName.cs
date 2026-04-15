namespace SteelOrdering.Domain.ValueObjects;

public readonly record struct ProductName(string Value);

public static class ProductNameFactory
{
    public static ProductName Create(string value) {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new(value.Trim());
    }
}