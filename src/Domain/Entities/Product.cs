using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record Product
{
    Product() { } // For EF Core

    internal Product(
        ProductId id,
        ProductName name,
        UnitWeightKilograms unitWeightKilograms,
        ProductInventoryStatus inventoryStatus,
        DateTimeOffset createdDateTimeUtc) {
        Id = id;
        Name = name;
        UnitWeightKilograms = unitWeightKilograms;
        InventoryStatus = inventoryStatus;
        CreatedDateTimeUtc = createdDateTimeUtc;
    }

    public ProductId Id { get; private set; }
    public ProductName Name { get; private set; }
    public UnitWeightKilograms UnitWeightKilograms { get; private set; }
    public ProductInventoryStatus InventoryStatus { get; private set; } = ProductInventoryStatusFactory.CreateOutOfStock();
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }
}

public static class ProductFactory
{
    public static Product Create(ProductName name, UnitWeightKilograms unitWeightKilograms) =>
        Create(name, unitWeightKilograms, ProductInventoryStatusFactory.CreateOutOfStock());

    public static Product Create(ProductName name, UnitWeightKilograms unitWeightKilograms, ProductInventoryStatus inventoryStatus) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name.Value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitWeightKilograms.Value);
        ArgumentNullException.ThrowIfNull(inventoryStatus);
        return new Product(default, name, unitWeightKilograms, inventoryStatus, DateTimeOffset.UtcNow);
    }
}