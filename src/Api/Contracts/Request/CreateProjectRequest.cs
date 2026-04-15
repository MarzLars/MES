using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Api.Contracts.Request;

public sealed record CreateProjectRequest(string Name);

public static class CreateProjectRequestExtensions
{
    public static ProjectName ToProjectName(this CreateProjectRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        return ProjectNameFactory.Create(request.Name);
    }
}
