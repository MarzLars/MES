using Microsoft.EntityFrameworkCore;
using SteelOrdering.Api.Contracts.Response;
using SteelOrdering.Data;

namespace SteelOrdering.Api.Queries;

/// <summary>
///     Query for retrieving all projects.
/// </summary>
public sealed record GetProjectsQuery;

public static class GetProjectsQueryHandler
{
    public static async Task<IResult> Handle(
        GetProjectsQuery query,
        ManufacturingDbContext dbContext,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await dbContext.Projects
            .AsNoTracking()
            .OrderBy(project => project.Id)
            .Select(project => new ProjectResponse(
                project.Id.Value,
                project.Name.Value,
                project.CreatedDateTimeUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(response);
    }
}