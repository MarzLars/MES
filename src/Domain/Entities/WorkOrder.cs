using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record WorkOrder
{
    WorkOrder() { } // For EF Core
    
    readonly List<WorkOrderLine> _orderLines = [];


    WorkOrder(
        WorkOrderId id,
        ProjectId projectId,
        Project? project,
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
    public Project? Project { get; private set; }
    public DateTimeOffset CreatedDateTimeUtc { get; private set; }
    public IReadOnlyCollection<WorkOrderLine> OrderLines => _orderLines.AsReadOnly();

    public decimal TotalWeightInKilograms => _orderLines.Sum(orderLine => orderLine.TotalLineWeightInKilograms);

    public static WorkOrder Create(
        Project project,
        IEnumerable<WorkOrderLineSpec> requestedLines,
        IEnumerable<Product> availableProducts) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(requestedLines);
        ArgumentNullException.ThrowIfNull(availableProducts);

        var requestedLineArray = requestedLines.ToArray();
        if (requestedLineArray.Length == 0)
            throw new ArgumentException("A work order must contain at least one line.", nameof(requestedLines));

        var productMap = availableProducts
            .GroupBy(product => product.Id)
            .ToDictionary(group => group.Key, group => group.First());

        int[] missingProductIds = requestedLineArray
            .Select(line => line.ProductId)
            .Distinct()
            .Where(productId => !productMap.ContainsKey(productId))
            .Select(productId => productId.Value)
            .ToArray();

        if (missingProductIds.Length > 0)
            throw new InvalidOperationException($"Unknown product ids: {string.Join(", ", missingProductIds)}.");

        if (project.Id.Value <= 0)
            throw new ArgumentException("Project must be persisted before creating a work order.", nameof(project));

        var normalizedLines = requestedLineArray
            .GroupBy(line => line.ProductId)
            .Select(group => new {
                Product = productMap[group.Key],
                Quantity = new Quantity(group.Sum(line => line.Quantity.Value))
            })
            .Select(item => WorkOrderLine.FromProduct(item.Product, item.Quantity))
            .ToArray();

        return new WorkOrder(new WorkOrderId(0), project.Id, project, normalizedLines, DateTimeOffset.UtcNow);
    }

    internal static WorkOrder FromDatabase(
        int id,
        ProjectId projectId,
        Project? project,
        IEnumerable<WorkOrderLine> orderLines,
        DateTimeOffset createdDateTimeUtc) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        var lines = orderLines.ToArray();
        if (lines.Length == 0)
            throw new ArgumentException("A work order must contain at least one line.", nameof(orderLines));

        return new WorkOrder(new WorkOrderId(id), projectId, project, lines, createdDateTimeUtc);
    }
}