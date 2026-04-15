using Dapper;
using MES.Data;

namespace MES.Commands;

/// <param name="ProductId">Existing product to include on the order line.</param>
/// <param name="Quantity">Number of units ordered (must be positive).</param>
public record OrderLineRequest(
    int ProductId,
    int Quantity);

/// <summary>
///     Creates a WorkOrder with one or more OrderLines inside a single transaction.
///     Returns the generated WorkOrder Id for caller bookkeeping (CQS corollary — see README).
/// </summary>
public sealed class CreateWorkOrderCommand(
    IDbConnectionFactory connectionFactory)
{
    public int Execute(int projectId, IEnumerable<OrderLineRequest> lines) {
        using var db = connectionFactory.Create();
        db.Open();
        using var tx = db.BeginTransaction();

        var workOrderId = db.ExecuteScalar<int>(
            "INSERT INTO WorkOrder (ProjectId) VALUES (@ProjectId); SELECT last_insert_rowid();",
            new { ProjectId = projectId },
            tx);

        db.Execute(
            "INSERT INTO OrderLine (WorkOrderId, ProductId, Quantity) VALUES (@WorkOrderId, @ProductId, @Quantity);",
            lines.Select(line => new { WorkOrderId = workOrderId, line.ProductId, line.Quantity }),
            tx);

        tx.Commit();
        return workOrderId;
    }
}