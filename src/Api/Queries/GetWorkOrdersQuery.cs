using Microsoft.EntityFrameworkCore;
using SteelOrdering.Api.Contracts.Response;
using SteelOrdering.Data;
using SteelOrdering.Domain.Entities;

namespace SteelOrdering.Api.Queries;

/// <summary>
///     Query for retrieving recent work orders.
/// </summary>
public sealed record GetWorkOrdersQuery(
    int Limit);

public static class GetWorkOrdersQueryHandler
{
    public static async Task<IResult> Handle(
        GetWorkOrdersQuery query,
        ManufacturingDbContext dbContext,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(dbContext);

        if (query.Limit <= 0)
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                ["limit"] = ["Limit must be greater than zero."]
            });

        var workOrders = await dbContext.WorkOrders
            .AsNoTracking()
            .Include(workOrder => workOrder.Project)
            .Include(workOrder => workOrder.OrderLines)
            .ToArrayAsync(cancellationToken);

        var response = workOrders
            .OrderByDescending(workOrder => workOrder.Id.Value)
            .Take(query.Limit)
            .Select(workOrder => new WorkOrderSummaryResponse(
                workOrder.Id.Value,
                workOrder.ProjectId.Value,
                workOrder.Project.Name.Value,
                workOrder.CreatedDateTimeUtc,
                workOrder.OrderLines.Count,
                workOrder.GetTotalWeightInKilograms()))
            .ToArray();

        return Results.Ok(response);
    }
}