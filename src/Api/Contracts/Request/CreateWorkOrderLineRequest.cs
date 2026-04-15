using SteelOrdering.Domain.ValueObjects;

namespace SteelOrdering.Api.Contracts.Request;

public sealed record CreateWorkOrderLineRequest(int ProductId, int Quantity);

public static class CreateWorkOrderLineRequestExtensions
{
    public static WorkOrderLineSpec ToWorkOrderLineSpec(this CreateWorkOrderLineRequest request) {
        ArgumentNullException.ThrowIfNull(request);

        var productId = ProductIdFactory.Create(request.ProductId);
        var quantity = QuantityFactory.Create(request.Quantity);

        return WorkOrderLineSpecFactory.Create(productId, quantity);
    }
}
