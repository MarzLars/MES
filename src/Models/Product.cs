namespace MES.Models;

public record Product
{
    public Product(string productName, decimal weightInKilogramsPerUnit)
    {
        if (string.IsNullOrWhiteSpace(productName)) 
        {
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));
        }

        if (weightInKilogramsPerUnit <= 0) 
        {
            throw new ArgumentException("Weight must be greater than zero.", nameof(weightInKilogramsPerUnit));
        }

        ProductName = productName;
        WeightInKilogramsPerUnit = weightInKilogramsPerUnit;
    }

    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal WeightInKilogramsPerUnit { get; init; }
    public DateTimeOffset CreatedDateTimeUtc { get; init; } = DateTimeOffset.UtcNow;
}