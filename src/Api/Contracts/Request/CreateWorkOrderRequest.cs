using SteelOrdering.Domain.ValueObjects;
using DomainProjectId = SteelOrdering.Domain.ValueObjects.ProjectId;

namespace SteelOrdering.Api.Contracts.Request;

public sealed record CreateWorkOrderRequest(
    int ProjectId,
    IReadOnlyCollection<CreateWorkOrderLineRequest> Lines)
{
    public DomainProjectId ToProjectId() {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ProjectId);
        return DomainProjectId.From(ProjectId);
    }

    public IReadOnlyCollection<WorkOrderLineSpec> ToWorkOrderLineSpecs() {
        ArgumentNullException.ThrowIfNull(Lines);

        if (Lines.Count == 0)
            throw new ArgumentException("A work order must contain at least one line.", nameof(Lines));

        return Lines
            .Select(line => line.ToWorkOrderLineSpec())
            .ToArray();
    }
}