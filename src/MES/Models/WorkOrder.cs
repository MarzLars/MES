namespace MES.Models;

public record WorkOrder
{
    List<WorkOrderLine> _orderLines = new();

    public WorkOrder(int projectId)
    {
        ProjectId = projectId;
    }

    // Required by EF Core
    WorkOrder() { }

    public int WorkOrderId { get; init; }
    public int ProjectId { get; init; }
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