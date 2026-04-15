using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Api.Contracts.Request;

public sealed record CreateProjectRequest(
    string Name)
{
    public ProjectName ToProjectName() {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        return ProjectName.From(Name);
    }
}