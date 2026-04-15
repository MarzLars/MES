using SteelOrdering.Api.Contracts.Request;
using SteelOrdering.Api.Contracts.Response;
using SteelOrdering.Api.Queries;
using SteelOrdering.Data;
using SteelOrdering.Domain.Entities;

namespace SteelOrdering.Api.Handlers;

public static class ProjectHandlers
{
    public static Task<IResult> GetAll(ManufacturingDbContext dbContext, CancellationToken cancellationToken) {
        return GetProjectsQueryHandler.Handle(new GetProjectsQuery(), dbContext, cancellationToken);
    }

    public static async Task<IResult> Create(
        CreateProjectRequest request,
        ManufacturingDbContext dbContext,
        CancellationToken cancellationToken) {
        try {
            var project = ProjectFactory.Create(request.ToProjectName());
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Created($"/projects/{project.Id.Value}", new IdResponse(project.Id.Value));
        }
        catch (ArgumentException exception) {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                [ToValidationKey(exception)] = [exception.Message]
            });
        }
    }

    static string ToValidationKey(ArgumentException exception) {
        if (string.IsNullOrWhiteSpace(exception.ParamName)) return "request";

        return char.ToLowerInvariant(exception.ParamName[0]) + exception.ParamName[1..];
    }
}