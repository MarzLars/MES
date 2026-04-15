using Microsoft.EntityFrameworkCore;
using SteelOrdering.Api.Contracts.Response;
using SteelOrdering.Data;
using SteelOrdering.Domain.Entities;
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

        var workOrderId = WorkOrderIdFactory.Create(query.WorkOrderId);

        var workOrder = await dbContext.WorkOrders
            .AsNoTracking()
            .Include(workOrder => workOrder.Project)
            .Include(workOrder => workOrder.OrderLines)
                .ThenInclude(orderLine => orderLine.Product)
            .SingleOrDefaultAsync(workOrder => workOrder.Id == workOrderId, cancellationToken);

        if (workOrder is null)
            return Results.NotFound();

        var response = new WorkOrderResponse(
            workOrder.Id.Value,
            workOrder.ProjectId.Value,
            workOrder.Project.Name.Value,
            workOrder.CreatedDateTimeUtc,
            workOrder.OrderLines
                .OrderBy(orderLine => orderLine.Id.Value)
                .Select(orderLine => new WorkOrderLineResponse(
                    orderLine.Id.Value,
                    orderLine.ProductId.Value,
                    orderLine.Product.Name.Value,
                    orderLine.Quantity.Value,
                    orderLine.UnitWeightKilograms.Value,
                    orderLine.GetTotalLineWeightInKilograms()))
                .ToArray(),
            workOrder.GetTotalWeightInKilograms());
            
        return Results.Ok(response);
    }
}