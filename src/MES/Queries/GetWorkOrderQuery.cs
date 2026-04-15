using Microsoft.EntityFrameworkCore;
using MES.Data;
using MES.Models;

namespace MES.Queries;

public sealed class GetWorkOrderQuery(ManufacturingDbContext manufacturingDbContext)
{
    public WorkOrder? Execute(int workOrderIdentifier)
    {
        return manufacturingDbContext.WorkOrders
            .Include(workOrder => workOrder.Project)
            .Include(workOrder => workOrder.OrderLines)
                .ThenInclude(workOrderLine => workOrderLine.Product)
            .FirstOrDefault(workOrder => workOrder.WorkOrderId == workOrderIdentifier);
    }
}