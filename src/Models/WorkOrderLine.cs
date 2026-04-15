namespace MES.Models;

public record WorkOrderLine
{
    public WorkOrderLine(int productId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        ProductId = productId;
        Quantity = quantity;
    }

    public int WorkOrderLineId { get; init; }
    public int WorkOrderId { get; init; }
    public WorkOrder? WorkOrder { get; init; }
    public int ProductId { get; init; }
    public Product? Product { get; init; }
    public int Quantity { get; init; }
    public DateTimeOffset CreatedDateTimeUtc { get; init; } = DateTimeOffset.UtcNow;

    public decimal TotalLineWeightInKilograms => Quantity * (Product?.WeightInKilogramsPerUnit ?? 0);
}