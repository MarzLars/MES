using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record WorkOrderLine
{
    WorkOrderLine() { } // For EF Core

    WorkOrderLine(
        WorkOrderLineId id,
        WorkOrderId workOrderId,
        WorkOrder? workOrder,
        ProductId productId,
        Product? product,
        Quantity quantity,
        UnitWeightKilograms unitWeightKilograms,
        DateTimeOffset createdDateTimeUtc) {
        Id = id;
        WorkOrderId = workOrderId;
        WorkOrder = workOrder;
        ProductId = productId;
        Product = product;
        Quantity = quantity;
        UnitWeightKilograms = unitWeightKilograms;
        CreatedDateTimeUtc = createdDateTimeUtc;
    }

    public WorkOrderLineId Id { get; private set; }
    public WorkOrderId WorkOrderId { get; private set; }
    public WorkOrder? WorkOrder { get; private set; }
    public ProductId ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Quantity Quantity { get; }
    public UnitWeightKilograms UnitWeightKilograms { get; }
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }

    public decimal TotalLineWeightInKilograms => Quantity.Value * UnitWeightKilograms.Value;

    public static WorkOrderLine FromProduct(Product product, Quantity quantity) {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Id.Value <= 0)
            throw new ArgumentException("Product must be persisted before it can be used in a work order line.",
                nameof(product));

        return new WorkOrderLine(new WorkOrderLineId(0), new WorkOrderId(0), null, product.Id, product, quantity,
            product.UnitWeightKilograms, DateTimeOffset.UtcNow);
    }

    internal static WorkOrderLine FromDatabase(
        int id,
        WorkOrderId workOrderId,
        WorkOrder? workOrder,
        ProductId productId,
        Product? product,
        Quantity quantity,
        UnitWeightKilograms unitWeightKilograms,
        DateTimeOffset createdDateTimeUtc) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        return new WorkOrderLine(new WorkOrderLineId(id), workOrderId, workOrder, productId, product, quantity,
            unitWeightKilograms, createdDateTimeUtc);
    }
}