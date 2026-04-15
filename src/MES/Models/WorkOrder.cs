namespace MES.Models;

public record WorkOrder(
    int Id,
    int ProjectId,
    string ProjectName,
    IReadOnlyList<OrderLine> Lines)
{
    public decimal TotalWeightKg => Lines.Sum(l => l.TotalWeightKg);
}