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

    // Required by EF Core
    Product() { }

    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal WeightInKilogramsPerUnit { get; init; }
}