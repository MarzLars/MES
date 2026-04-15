using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record Product
{
    Product() { } // For EF Core

    Product(
        ProductId id,
        ProductName name,
        UnitWeightKilograms unitWeightKilograms,
        DateTimeOffset createdDateTimeUtc) {
        Id = id;
        Name = name;
        UnitWeightKilograms = unitWeightKilograms;
        CreatedDateTimeUtc = createdDateTimeUtc;
    }

    public ProductId Id { get; private set; }
    public ProductName Name { get; private set; }
    public UnitWeightKilograms UnitWeightKilograms { get; private set; }
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }

    public static Product Create(ProductName name, UnitWeightKilograms unitWeightKilograms) {
        return new Product(new ProductId(0), name, unitWeightKilograms, DateTimeOffset.UtcNow);
    }

    internal static Product FromDatabase(
        int id,
        ProductName name,
        UnitWeightKilograms unitWeightKilograms,
        DateTimeOffset createdDateTimeUtc) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return new Product(new ProductId(id), name, unitWeightKilograms, createdDateTimeUtc);
    }
}