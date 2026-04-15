using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record WorkOrderLine
{
    WorkOrderLine() { } // For EF Core

    internal WorkOrderLine(
        WorkOrderLineId id,
        WorkOrderId workOrderId,
        WorkOrder? workOrder,
        ProductId productId,
        Product product,
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
    public Product Product { get; private set; } = null!;
    public Quantity Quantity { get; private set; }
    public UnitWeightKilograms UnitWeightKilograms { get; private set; }
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }
}

public static class WorkOrderLineFactory
{
    public static WorkOrderLine Create(Product product, Quantity quantity) {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Id.Value <= 0)
            throw new ArgumentException("Product must be persisted before it can be used in a work order line.", nameof(product));

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity.Value);

        return new WorkOrderLine(default, default, null, product.Id, product, quantity, product.UnitWeightKilograms, DateTimeOffset.UtcNow);
    }
}

public static class WorkOrderLineExtensions
{
    public static decimal GetTotalLineWeightInKilograms(this WorkOrderLine workOrderLine) {
        ArgumentNullException.ThrowIfNull(workOrderLine);
        return workOrderLine.Quantity.Value * workOrderLine.UnitWeightKilograms.Value;
    }
}