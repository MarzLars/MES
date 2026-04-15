using MES.Data;
using MES.Models;

namespace MES.Commands;

public record WorkOrderLineRequest(
    int ProductId,
    int Quantity);

public sealed class CreateWorkOrderCommand(ManufacturingDbContext manufacturingDbContext)
{
    public int Execute(int projectIdentifier, IEnumerable<WorkOrderLineRequest> workOrderLineRequests)
    {
        var newlyCreatedWorkOrder = new WorkOrder(projectIdentifier);

        foreach (var workOrderLineRequest in workOrderLineRequests)
        {
            newlyCreatedWorkOrder.AddWorkOrderLine(workOrderLineRequest.ProductId, workOrderLineRequest.Quantity);
        }

        manufacturingDbContext.WorkOrders.Add(newlyCreatedWorkOrder);
        manufacturingDbContext.SaveChanges();

        return newlyCreatedWorkOrder.WorkOrderId;
    }
}