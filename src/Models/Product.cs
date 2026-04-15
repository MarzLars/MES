namespace MES.Models;

public record Product
{
    public Product(string productName, decimal weightInKilogramsPerUnit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weightInKilogramsPerUnit);

        ProductName = productName;
        WeightInKilogramsPerUnit = weightInKilogramsPerUnit;
    }

    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal WeightInKilogramsPerUnit { get; init; }
    public DateTimeOffset CreatedDateTimeUtc { get; init; } = DateTimeOffset.UtcNow;
}