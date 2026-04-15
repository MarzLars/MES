namespace MES.Models;

public record WorkOrder(
    int ProjectId)
{
    private readonly List<WorkOrderLine> _orderLines = [];

    public int WorkOrderId { get; init; }
    public Project? Project { get; init; }
    public DateTimeOffset CreatedDateTimeUtc { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyCollection<WorkOrderLine> OrderLines => _orderLines.AsReadOnly();

    public void AddWorkOrderLine(int productId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        _orderLines.Add(new WorkOrderLine(productId, quantity));
    }

    public decimal TotalWeightInKilograms => _orderLines.Sum(orderLine => orderLine.TotalLineWeightInKilograms);
}