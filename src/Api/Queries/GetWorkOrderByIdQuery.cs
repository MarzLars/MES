using Microsoft.EntityFrameworkCore;
using SteelOrdering.Api.Contracts.Response;
using SteelOrdering.Data;
using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Api.Queries;

/// <summary>
///     Query for retrieving a work order by ID.
///     Part of CQRS pattern - read-only operation with optimized query model.
/// </summary>
public sealed record GetWorkOrderByIdQuery(
    int WorkOrderId);

public static class GetWorkOrderByIdQueryHandler
{
    public static async Task<IResult> Handle(
        GetWorkOrderByIdQuery query,
        ManufacturingDbContext dbContext,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(query);

        if (query.WorkOrderId <= 0)
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                ["workOrderId"] = ["Work order id must be greater than zero."]
            });

        var response = await dbContext.WorkOrders
            .AsNoTracking()
            .Where(workOrder => workOrder.Id == new WorkOrderId(query.WorkOrderId))
            .Select(workOrder => new WorkOrderResponse(
                workOrder.Id.Value,
                workOrder.ProjectId.Value,
                workOrder.Project.Name.Value,
                workOrder.CreatedDateTimeUtc,
                workOrder.OrderLines
                    .OrderBy(orderLine => orderLine.Id)
                    .Select(orderLine => new WorkOrderLineResponse(
                        orderLine.Id.Value,
                        orderLine.ProductId.Value,
                        orderLine.Product.Name.Value,
                        orderLine.Quantity.Value,
                        orderLine.UnitWeightKilograms.Value,
                        orderLine.Quantity.Value * orderLine.UnitWeightKilograms.Value))
                    .ToArray(),
                workOrder.OrderLines.Sum(orderLine => orderLine.Quantity.Value * orderLine.UnitWeightKilograms.Value)))
            .SingleOrDefaultAsync(cancellationToken);

        return response is null ?
            Results.NotFound() :
            Results.Ok(response);
    }
}