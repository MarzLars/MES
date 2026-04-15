using Microsoft.EntityFrameworkCore;
using SteelOrdering.Api.Contracts.Request;
using SteelOrdering.Api.Contracts.Response;
using SteelOrdering.Api.Queries;
using SteelOrdering.Data;
using SteelOrdering.Domain.Entities;

namespace SteelOrdering.Api.Handlers;

public static class WorkOrderHandlers
{
    public static Task<IResult> GetById(int workOrderId, ManufacturingDbContext dbContext,
        CancellationToken cancellationToken) {
        return GetWorkOrderByIdQueryHandler.Handle(new GetWorkOrderByIdQuery(workOrderId), dbContext,
            cancellationToken);
    }

    public static async Task<IResult> Create(
        CreateWorkOrderRequest request,
        ManufacturingDbContext dbContext,
        CancellationToken cancellationToken) {
        try {
            var projectId = request.ToProjectId();
            var lines = request.ToWorkOrderLineSpecs();

            var project = await dbContext.Projects
                .AsNoTracking()
                .SingleOrDefaultAsync(existingProject => existingProject.Id == projectId, cancellationToken);

            if (project is null)
                return Results.Problem(
                    $"Project {projectId.Value} does not exist.",
                    statusCode: StatusCodes.Status404NotFound);

            int[] requestedProductIds = lines
                .Select(line => line.ProductId.Value)
                .Distinct()
                .ToArray();

            var products = await dbContext.Products
                .AsNoTracking()
                .Where(product => requestedProductIds.AsEnumerable().Contains(product.Id.Value))
                .ToArrayAsync(cancellationToken);

            int[] missingProductIds = requestedProductIds
                .Except(products.Select(product => product.Id.Value))
                .ToArray();

            if (missingProductIds.Length > 0)
                return Results.ValidationProblem(new Dictionary<string, string[]> {
                    ["lines"] = [$"Unknown product ids: {string.Join(", ", missingProductIds)}."]
                });

            var workOrder = WorkOrderFactory.Create(project, lines, products);
            dbContext.WorkOrders.Add(workOrder);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/work-orders/{workOrder.Id.Value}", new IdResponse(workOrder.Id.Value));
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