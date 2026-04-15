namespace MES.Models;

public record WorkOrder(
    int ProjectId)
{
    List<WorkOrderLine> _orderLines = new();

    public int WorkOrderId { get; init; }
    public Project? Project { get; init; }
    public IReadOnlyCollection<WorkOrderLine> OrderLines => _orderLines.AsReadOnly();

    public void AddWorkOrderLine(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        _orderLines.Add(new WorkOrderLine(productId, quantity));
    }

    public decimal TotalWeightInKilograms => _orderLines.Sum(orderLine => orderLine.TotalLineWeightInKilograms);
}