using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Domain.Entities;

public sealed record WorkOrder
{
    readonly List<WorkOrderLine> _orderLines = [];
    WorkOrder() { } // For EF Core


    WorkOrder(
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

    public decimal TotalWeightInKilograms => _orderLines.Sum(orderLine => orderLine.TotalLineWeightInKilograms);

    public static WorkOrder Create(
        Project project,
        IReadOnlyCollection<WorkOrderLineSpec> requestedLines,
        IReadOnlyCollection<Product> availableProducts) {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(requestedLines);
        ArgumentNullException.ThrowIfNull(availableProducts);

        if (requestedLines.Count == 0)
            throw new ArgumentException("A work order must contain at least one line.", nameof(requestedLines));

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
            throw new ArgumentException($"Unknown product ids: {string.Join(", ", missingProductIds)}.",
                nameof(requestedLines));

        if (project.Id.Value <= 0)
            throw new ArgumentException("Project must be persisted before creating a work order.", nameof(project));

        var normalizedLines = requestedLineArray
            .GroupBy(line => line.ProductId)
            .Select(group => new {
                Product = productMap[group.Key],
                Quantity = Quantity.From(group.Sum(line => line.Quantity.Value))
            })
            .Select(item => WorkOrderLine.FromProduct(item.Product, item.Quantity))
            .ToArray();

        return new WorkOrder(new WorkOrderId(0), project.Id, project, normalizedLines, DateTimeOffset.UtcNow);
    }

    internal static WorkOrder FromDatabase(
        int id,
        ProjectId projectId,
        Project project,
        IReadOnlyCollection<WorkOrderLine> orderLines,
        DateTimeOffset createdDateTimeUtc) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(orderLines);

        if (orderLines.Count == 0)
            throw new InvalidOperationException("A work order must contain at least one line.");

        var lines = orderLines.ToArray();

        return new WorkOrder(new WorkOrderId(id), projectId, project, lines, createdDateTimeUtc);
    }
}