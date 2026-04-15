namespace MES.Models;

public record OrderLine(int Id, int WorkOrderId, int ProductId, string ProductName, decimal WeightKgPerUnit, int Quantity)
{
    public decimal TotalWeightKg => Quantity * WeightKgPerUnit;
}
