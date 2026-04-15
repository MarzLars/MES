using SteelOrdering.Domain.ValueObjects;
using DomainProjectId = SteelOrdering.Domain.ValueObjects.ProjectId;

namespace SteelOrdering.Api.Contracts.Request;

public sealed record CreateWorkOrderRequest(int ProjectId, IReadOnlyCollection<CreateWorkOrderLineRequest> Lines);

public static class CreateWorkOrderRequestExtensions
{
    public static DomainProjectId ToProjectId(this CreateWorkOrderRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        return ProjectIdFactory.Create(request.ProjectId);
    }

    public static IReadOnlyCollection<WorkOrderLineSpec> ToWorkOrderLineSpecs(this CreateWorkOrderRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Lines);

        if (request.Lines.Count == 0)
            throw new ArgumentException("A work order must contain at least one line.", nameof(request.Lines));

        return request.Lines.Select(line => line.ToWorkOrderLineSpec()).ToArray();
    }
}
