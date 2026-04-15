using SteelOrdering.Domain.ValueObjects;
using DomainProductId = SteelOrdering.Domain.ValueObjects.ProductId;
using DomainQuantity = SteelOrdering.Domain.ValueObjects.Quantity;

namespace SteelOrdering.Api.Contracts.Request;

public sealed record CreateWorkOrderLineRequest(
    int ProductId,
    int Quantity)
{
    public WorkOrderLineSpec ToWorkOrderLineSpec() {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ProductId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Quantity);

        return new WorkOrderLineSpec(
            DomainProductId.From(ProductId),
            DomainQuantity.From(Quantity));
    }
}