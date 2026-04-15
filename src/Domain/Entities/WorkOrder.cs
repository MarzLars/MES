using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record WorkOrder
{
    readonly List<WorkOrderLine> _orderLines = [];
    WorkOrder() { } // For EF Core

    internal WorkOrder(
        WorkOrderId id,
        ProjectId projectId,
        Project project,
        IEnumerable<WorkOrderLine> orderLines,
        DateTimeOffset createdDateTimeUtc) {
        Id = id;
        ProjectId = projectId;
        Project = project;
        _orderLines = [.. orderLines];
        CreatedDateTimeUtc = createdDateTimeUtc;
    }

    public WorkOrderId Id { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }
    public IReadOnlyCollection<WorkOrderLine> OrderLines => _orderLines.AsReadOnly();
}

public static class WorkOrderFactory
{
    public static WorkOrder Create(Project project, IReadOnlyCollection<WorkOrderLineSpec> requestedLines, IReadOnlyCollection<Product> availableProducts) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(requestedLines);
        ArgumentNullException.ThrowIfNull(availableProducts);

        if (project.Id.Value <= 0)
            throw new ArgumentException("Project must be persisted before creating a work order.", nameof(project));

        if (requestedLines.Count == 0)
            throw new ArgumentException("A work order must contain at least one line.", nameof(requestedLines));

        if (requestedLines.Any(line => line.ProductId.Value <= 0))
            throw new ArgumentException("All requested product ids must be greater than zero.", nameof(requestedLines));

        if (availableProducts.Any(product => product is null || product.Id.Value <= 0))
            throw new ArgumentException("All available products must be persisted before creating a work order.", nameof(availableProducts));

        var productMap = availableProducts
            .GroupBy(product => product.Id)
            .ToDictionary(group => group.Key, group => group.First());

        var requestedLineArray = requestedLines.ToArray();

        int[] missingProductIds = requestedLineArray
            .Select(line => line.ProductId)
            .Distinct()
            .Where(productId => !productMap.ContainsKey(productId))
            .Select(productId => productId.Value)
            .ToArray();

        if (missingProductIds.Length > 0)
            throw new ArgumentException($"Unknown product ids: {string.Join(", ", missingProductIds)}.", nameof(requestedLines));

        var normalizedLines = requestedLineArray
            .GroupBy(line => line.ProductId)
            .Select(group => new {
                Product = productMap[group.Key],
                Quantity = QuantityFactory.Create(group.Sum(line => line.Quantity.Value))
            })
            .Select(item => WorkOrderLineFactory.Create(item.Product, item.Quantity))
            .ToList();

        return new WorkOrder(default, project.Id, project, normalizedLines, DateTimeOffset.UtcNow);
    }
}

public static class WorkOrderExtensions
{
    public static decimal GetTotalWeightInKilograms(this WorkOrder workOrder) {
        ArgumentNullException.ThrowIfNull(workOrder);
        return workOrder.OrderLines.Sum(workOrderLine => workOrderLine.GetTotalLineWeightInKilograms());
    }
}