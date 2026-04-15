using Dapper;
using MES.Data;
using MES.Models;

namespace MES.Queries;

/// <summary>
///     Retrieves a WorkOrder with all its OrderLines and the pre-computed TotalWeightKg.
///     Sorting is delegated to SQL ORDER BY, which is the pragmatic optimum
///     (see README — "Sorting in SQL" design note).
/// </summary>
public sealed class GetWorkOrderQuery(
    IDbConnectionFactory connectionFactory)
{
    const string Sql = """
                       SELECT
                           wo.Id           AS WorkOrderId,
                           wo.ProjectId    AS ProjectId,
                           p.Name          AS ProjectName,
                           ol.Id           AS OrderLineId,
                           ol.ProductId    AS ProductId,
                           pr.Name         AS ProductName,
                           pr.WeightKgPerUnit AS WeightKgPerUnit,
                           ol.Quantity     AS Quantity
                       FROM  WorkOrder wo
                       JOIN  Project   p  ON p.Id  = wo.ProjectId
                       JOIN  OrderLine ol ON ol.WorkOrderId = wo.Id
                       JOIN  Product   pr ON pr.Id = ol.ProductId
                       WHERE wo.Id = @WorkOrderId
                       ORDER BY ol.Id;
                       """;

    public WorkOrder? Execute(int workOrderId) {
        using var db = connectionFactory.Create();

        return db.Query<FlatRow>(Sql, new { WorkOrderId = workOrderId }).ToList() switch {
            [] => null,
            [var first, ..] and var rows => new WorkOrder(
                first.WorkOrderId,
                first.ProjectId,
                first.ProjectName,
                rows.Select(r => new OrderLine(r.OrderLineId, r.WorkOrderId, r.ProductId,
                    r.ProductName, r.WeightKgPerUnit, r.Quantity)).ToList())
        };
    }

    // Flat DTO matching the column types returned by SQLite/SQL Server.
    // WeightKgPerUnit is declared as decimal; Dapper converts the REAL value
    // from SQLite without precision loss for the weight ranges encountered.
    sealed class FlatRow
    {
        public int WorkOrderId { get; init; }
        public int ProjectId { get; init; }
        public string ProjectName { get; init; } = "";
        public int OrderLineId { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = "";
        public decimal WeightKgPerUnit { get; init; }
        public int Quantity { get; init; }
    }
}